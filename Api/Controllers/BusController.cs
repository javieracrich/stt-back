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
using Services.Cosmos;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BusController : SttControllerBase
    {
        private readonly SttUserManager _userManager;
        private readonly ILogger<BusController> _logger;

        private readonly IMapper _mapper;

        public BusController(
            IGenericService service,
            SttUserManager userManager,
            IMapper mapper,
            ILogger<BusController> logger) : base(service)
        {
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(Policy = AuthPolicy.Reader)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<BusModel>>> GetAllBusesBySchool([FromQuery] bool withDevice)
        {
            try
            {
                //todo: only admins can call this method
                var list = await service.FindAsync<Bus>(x => x.School.Code == SchoolCode && (withDevice && x.Device != null || !withDevice), null, true, x => x.School, x => x.Device);

                var result = _mapper.Map<List<BusModel>>(list);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        [HttpGet("{busCode}")]
        [Authorize(Policy = AuthPolicy.Reader)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BusModel>> GetBus(Guid busCode)
        {
            try
            {
                var bus = await service.FirstOrDefaultAsync<Bus>(x => x.Code == busCode && x.School.Code == SchoolCode, null, true);

                if (bus == null)
                {
                    return NotFound(ErrorConstants.BusNotFound);
                }
                return _mapper.Map<BusModel>(bus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        [HttpPost]
        [Authorize(Policy = AuthPolicy.Contributor)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> PostBus(PostBusModel model)
        {
            try
            {
                //todo: only admins can call this method

                var existingCount = await service.CountAsync<Bus>(x => x.Name == model.Name && x.School.Code == SchoolCode);

                if (existingCount > 0)
                {
                    return BadRequest($"there is already a bus with name '{model.Name}' for school '{SchoolCode}'. Try with another name");
                }

                var newBus = new Bus
                {
                    Name = model.Name,
                    Patent = model.Patent,
                    Code = Guid.NewGuid(),
                    School = await service.FirstOrDefaultAsync<School>(x => x.Code == SchoolCode)
                };

                //device is optional
                if (!string.IsNullOrWhiteSpace(model.DeviceCode))
                {
                    var device = await service.FirstOrDefaultAsync<Device>(x => x.DeviceCode == model.DeviceCode);
                    if (device == null)
                    {
                        return BadRequest(ErrorConstants.DeviceNotFound);
                    }
                    var busWithDevice = await service.FirstOrDefaultAsync<Bus>(x => x.DeviceId == device.Id);
                    if (busWithDevice != null)
                    {
                        return BadRequest($"bus {busWithDevice.Name} in school {busWithDevice.School.Code} is already associated with device {device.Name}. Try another device name for this bus.");
                    }
                    newBus.Device = device;
                }

                var driversAndSupervisors = new List<User>();

                foreach (var driverCode in model.Drivers)
                {
                    var d = await service.FirstOrDefaultAsync<User>(x => x.Code == driverCode && x.Category == UserCategory.BusDriver);
                    if (d == null)
                    {
                        return BadRequest($"driver with code {driverCode} not found");
                    }
                    driversAndSupervisors.Add(d);
                }

                foreach (var supervisorCode in model.Supervisors)
                {
                    var s = await service.FirstOrDefaultAsync<User>(x => x.Code == supervisorCode && x.Category == UserCategory.Supervisor);
                    if (s == null)
                    {
                        return BadRequest($"supervisor with code {supervisorCode} not found");
                    }
                    driversAndSupervisors.Add(s);
                }

                newBus.DriversAndSupervisors.AddRange(driversAndSupervisors);

                await service.CreateAsync(newBus);

                var url = $"/bus/{newBus.Code}";
                return Created(url, newBus.Code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }


        /// <summary>
        /// Creates a new supervisor for a specific school bus
        /// </summary>
        /// <param name="busCode"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("{busCode}/supervisor")]
        [Authorize(Policy = AuthPolicy.Contributor)]
        [ProducesResponseType(typeof(UserModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<UserModel>> PostSupervisor(Guid busCode, PostUserModel model)
        {

            try
            {
                //todo: only admins can call this method

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var bus = await service.FirstOrDefaultAsync<Bus>(x => x.Code == busCode & x.School.Code == SchoolCode);

                if (bus == null)
                {
                    return NotFound(ErrorConstants.BusNotFound);
                }

                var newSupervisor = await GetNewUserAsync(UserCategory.Supervisor, model);

                // supervisors are not allowed to sign in, but a password is required to create the user entitity in db.
                var identityResult = await _userManager.CreateAsync(newSupervisor, Guid.NewGuid().ToString());

                if (!identityResult.Succeeded)
                    return BadRequest(identityResult.Errors);

                bus.DriversAndSupervisors.Add(newSupervisor);

                //update school bus
                await service.UpdateAsync(bus);

                var url = $"/user/{newSupervisor.Id}";

                return Created(url, _mapper.Map<UserModel>(newSupervisor));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }

        }


        /// <summary>
        /// Retrieves Supervisors for a specific school bus
        /// </summary>
        /// <param name="busCode"></param>
        /// <returns></returns>
        [HttpGet("{busCode}/supervisor")]
        [Authorize(Policy = AuthPolicy.Reader)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<UserModel>>> GetSupervisor(Guid busCode)
        {
            try
            {
                var bus = await service.FirstOrDefaultAsync<Bus>(x => x.Code == busCode && x.School.Code == SchoolCode);

                if (bus == null)
                {
                    return NotFound(ErrorConstants.BusNotFound);
                }

                var result = bus
                    .DriversAndSupervisors
                    .Where(x => x.Category == UserCategory.Supervisor);

                return _mapper.Map<List<UserModel>>(result);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        [HttpPut("{busCode}")]
        [Authorize(Policy = AuthPolicy.Contributor)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutBus(Guid busCode, PutBusModel model)
        {
            try
            {
                //todo: only admins can call this method

                var bus = await service.FirstOrDefaultAsync<Bus>(x => x.Code == busCode && x.School.Code == SchoolCode);

                if (bus == null)
                {
                    return NotFound(ErrorConstants.BusNotFound);
                }

                if (!string.IsNullOrWhiteSpace(model.Name))
                {
                    bus.Name = model.Name;
                }

                if (!string.IsNullOrWhiteSpace(model.Patent))
                {
                    bus.Patent = model.Patent;
                }

                if (!string.IsNullOrWhiteSpace(model.DeviceCode))
                {
                    var device = await service.FirstOrDefaultAsync<Device>(x => x.DeviceCode == model.DeviceCode);
                    if (device == null)
                    {
                        return NotFound(ErrorConstants.DeviceNotFound);
                    }
                    bus.Device = device;
                }

                if (model.DriverCodes.Any())
                {
                    bus.DriversAndSupervisors
                        .RemoveAll(x => x.Category == UserCategory.BusDriver);

                    foreach (var driverCode in model.DriverCodes)
                    {
                        var driver = await service.FirstOrDefaultAsync<User>(x => x.Code == driverCode);
                        if (driver == null)
                        {
                            return NotFound(ErrorConstants.DriverNotFound);
                        }
                        if (driver.Category != UserCategory.BusDriver)
                        {
                            return BadRequest(ErrorConstants.UserIsNotADriver);
                        }
                        bus.DriversAndSupervisors.Add(driver);
                    }
                }
                if (model.SupervisorCodes.Any())
                {
                    bus.DriversAndSupervisors
                        .RemoveAll(x => x.Category == UserCategory.Supervisor);

                    foreach (var supervisorCode in model.SupervisorCodes)
                    {
                        var supervisor = await service.FirstOrDefaultAsync<User>(x => x.Code == supervisorCode);
                        if (supervisor == null)
                        {
                            return NotFound(ErrorConstants.SupervisorNotFound);
                        }
                        if (supervisor.Category != UserCategory.Supervisor)
                        {
                            return BadRequest(ErrorConstants.UserIsNotASupervisor);
                        }
                        bus.DriversAndSupervisors.Add(supervisor);
                    }
                }
                bus.RowVersion = model.RowVersion;
                var rowsAffected = await service.UpdateAsync(bus);

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

        [HttpDelete("{busCode}")]
        [Authorize(Policy = AuthPolicy.Contributor)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBus(Guid busCode)
        {
            try
            {
                //todo: only admins can call this method

                var bus = await service.FirstOrDefaultAsync<Bus>(x => x.Code == busCode && x.School.Code == SchoolCode);

                if (bus == null)
                {
                    return NotFound(ErrorConstants.BusNotFound);
                }
                var rowsAffected = await service.DeleteAsync(bus);

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
