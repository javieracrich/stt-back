using Common;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Auth;
using Services.Cosmos;
using Domain.Models;

namespace Api.Controllers
{
    /// <summary>
    /// MOVE THIS TO A MICRO SERVICE
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class RFIDController : SttControllerBase
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly IDeviceEventService _deviceEventService;
        private readonly IRFIDService rfidService;
        private readonly ILogger<RFIDController> _logger;

        public RFIDController(
            IDateTimeService dateTimeService,
            IGenericService service,
            IDeviceEventService deviceEventService,
            IRFIDService rfidService,
            ILogger<RFIDController> logger
            ) : base(service)
        {

            _dateTimeService = dateTimeService;
            _deviceEventService = deviceEventService;
            this.rfidService = rfidService;
            _logger = logger;
        }

        /// <summary>
        /// Ingestion endpoint for RFID marks.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<long>> PostRfid(PostDeviceEventModel model)
        {
            model.DeviceCode = model.DeviceCode.ToUpperInvariant();

            _logger.LogInformation("starting to process rfid for device {0}", model.DeviceCode);

            try
            {
                return Ok(await this.rfidService.Process(model));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }


        /// <summary>
        /// Retrieves all the RFID marks from the last 24 hours.
        /// </summary>
        /// <param name="deviceCode"></param>
        /// <param name="cardCode"></param>
        /// <returns></returns>
        [HttpGet("{deviceCode}/{cardCode}")]
        [Authorize(Policy = AuthPolicy.Reader)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<DeviceEventDocument>>> GetLatestRfidEvents(string deviceCode, string cardCode)
        {
            //todo: only admins can call this method

            var device = await service.FirstOrDefaultAsync<Device>(x => x.DeviceCode == deviceCode, null, true, x => x.School);

            if (device == null)
            {
                return BadRequest(ErrorConstants.DeviceNotFound);
            }

            var schoolCode = device.School.Code;

            var yesterday = _dateTimeService.UtcNow().AddDays(-1);

            var list = _deviceEventService.GetDeviceEvents(schoolCode, deviceCode.ToUpperInvariant(), cardCode, yesterday).ToList();

            return Ok(list);
        }
    }
}
