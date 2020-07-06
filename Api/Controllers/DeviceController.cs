using Common;
using Domain;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DeviceController : SttControllerBase
    {
        private readonly ILogger<DeviceController> _logger;
        private readonly IMapper _mapper;

        public DeviceController(IMapper mapper, IGenericService service, ILogger<DeviceController> logger) : base(service)
        {
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy = AuthPolicy.Reader)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<DeviceModel>>> GetAllDevices()
        {
            try
            {
                var list = await service.FindAsync<Device>(x => x.School.Code == SchoolCode, null, false, x => x.School, x => x.Bus);

                //todo: do not use auto mapper in lists;
                return Ok(_mapper.Map<List<DeviceModel>>(list));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        [HttpGet("{deviceCode}")]
        [Authorize(Policy = AuthPolicy.Reader)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DeviceModel>> GetDevice(string deviceCode)
        {
            try
            {
                var device = await service.FirstOrDefaultAsync<Device>(
                    x => x.DeviceCode == deviceCode.ToUpperInvariant() && x.School.Code == SchoolCode,
                    null,
                    true,
                    x => x.School,
                    x => x.Bus,
                    x => x.RelatedDevice);

                if (device == null)
                {
                    return NotFound(ErrorConstants.DeviceNotFound);
                }
                return Ok(_mapper.Map<DeviceModel>(device));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        /// <summary>
        /// Creates a new Device
        /// </summary>
        /// <remarks>
        /// type 1: school pointing outside
        /// type 2: school pointing inside  
        /// type 3: school bus front door
        /// </remarks>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = AuthPolicy.Contributor)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PostDeviceModel>> PostDevice(PostDeviceModel model)
        {
            try
            {
                model.DeviceCode = model.DeviceCode.ToUpperInvariant();

                if (model.Type == DeviceType.BusFrontDoor && model.BusCode == null)
                {
                    return BadRequest($"you must provide a school bus code if the type is {nameof(DeviceType.BusFrontDoor)}");
                }

                var newDevice = new Device()
                {
                    DeviceCode = model.DeviceCode,
                    Name = model.Name,
                    School = await service.FirstOrDefaultAsync<School>(x => x.Code == SchoolCode)
                };

                if (model.BusCode.HasValue)
                {
                    var bus = await service.FirstOrDefaultAsync<Bus>(x => x.School.Code == SchoolCode && x.Code == model.BusCode);

                    if (bus == null)
                    {
                        return NotFound(ErrorConstants.BusNotFound);
                    }
                    newDevice.Bus = bus;
                }

                await service.CreateAsync(newDevice);

                var url = $"/device/{newDevice.DeviceCode}";
                return Created(url, model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        [HttpPut("{deviceCode}")]
        [Authorize(Policy = AuthPolicy.Contributor)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutDevice(string deviceCode, PutDeviceModel model)
        {
            try
            {
                //todo: only admins can call this method.

                deviceCode = deviceCode.ToUpperInvariant();

                var device = await service.FirstOrDefaultAsync<Device>(x => x.DeviceCode == deviceCode && x.School.Code == SchoolCode, null, false, d => d.School, d => d.Bus);

                if (device == null)
                {
                    return NotFound(ErrorConstants.DeviceNotFound);
                }

                device.Name = model.Name;
                device.Type = model.Type;

                if (model.BusCode.HasValue)
                {
                    var bus = await service.FirstOrDefaultAsync<Bus>(x => x.Code == model.BusCode);
                    if (bus == null)
                    {
                        return NotFound(ErrorConstants.BusNotFound);
                    }
                    device.Bus = bus;
                    device.School = null;
                }

                device.RowVersion = model.RowVersion;
                var rowsAffected = await service.UpdateAsync(device);

                return Ok(rowsAffected);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ErrorConstants.ConcurrencyMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        /// <summary>
        /// Relates an Inside Device to an Outside Device. 
        /// </summary>
        [HttpPatch("relate")]
        [Authorize(Policy = AuthPolicy.Contributor)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<int>> PatchRelateDevices(PatchRelateDevicesModel model)
        {
            try
            {
                var deviceCodeInside = model.InsideDeviceCode.ToUpperInvariant();

                var deviceCodeOutside = model.OutsideDeviceCode.ToUpperInvariant();

                var devices = await service.FindAsync<Device>(x => (x.DeviceCode == deviceCodeInside || x.DeviceCode == deviceCodeOutside) && x.School.Code == SchoolCode);

                var insideDevice = devices.FirstOrDefault(x => x.Type == DeviceType.SchoolPointingInside);

                var outsideDevice = devices.FirstOrDefault(x => x.Type == DeviceType.SchoolPointingOutside);

                if (insideDevice == null)
                {
                    return NotFound("could not find device " + deviceCodeInside);
                }
                if (outsideDevice == null)
                {
                    return NotFound("could not find device " + deviceCodeOutside);
                }
                if (insideDevice.Type != DeviceType.SchoolPointingInside)
                {
                    return BadRequest($"device {deviceCodeInside} is not of type {nameof(DeviceType.SchoolPointingInside)}");
                }
                if (outsideDevice.Type != DeviceType.SchoolPointingOutside)
                {
                    return BadRequest($"device {deviceCodeOutside} is not of type {nameof(DeviceType.SchoolPointingOutside)}");
                }

                insideDevice.RelatedDevice = outsideDevice;
                outsideDevice.RelatedDevice = insideDevice;

                var rowsAffected = await service.UpdateAsync(insideDevice);
                return Ok(rowsAffected);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        [HttpDelete("{deviceCode}")]
        [Authorize(Policy = AuthPolicy.Contributor)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<int>> DeleteDevice(string deviceCode)
        {
            try
            {
                var device = await service.FirstOrDefaultAsync<Device>(x => x.DeviceCode == deviceCode && x.School.Code == SchoolCode);

                if (device == null)
                {
                    return NotFound(ErrorConstants.DeviceNotFound);
                }

                var rowsAffected = await service.DeleteAsync(device);

                return Ok(rowsAffected);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }
    }
}
