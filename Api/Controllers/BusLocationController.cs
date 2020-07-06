using Common;
using Domain;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.Spatial;
using Microsoft.Extensions.Logging;
using Services;
using System;
using System.Threading.Tasks;
using Services.Cosmos;
using System.Net;
using Api.Auth;
using System.Collections.Generic;
using System.Linq;

namespace Api.Controllers
{
    /// <summary>
    /// MOVE THIS TO A MICRO SERVICE
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class BusLocationController : SttControllerBase
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly ILogger<BusLocationController> _logger;
        private readonly IBusGpsLogService _busGpsLogService;

        public BusLocationController(IGenericService service,
            ILogger<BusLocationController> logger,
            IBusGpsLogService busGpsLogService,
            IDateTimeService dateTimeService) : base(service)
        {
            _dateTimeService = dateTimeService;
            _logger = logger;
            _busGpsLogService = busGpsLogService;
        }

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PostLocation(PostBusGpsModel model)
        {
            try
            {
                model.DeviceCode = model.DeviceCode.ToUpperInvariant();

                //todo: cache this call
                var device = await service.FirstOrDefaultAsync<Device>(x =>
                x.DeviceCode == model.DeviceCode,
                null,
                true,
                x => x.Bus.School);

                // validate
                if (device == null)
                {
                    _logger.LogError("device {0} was not found", model.DeviceCode);
                    return NotFound(ErrorConstants.DeviceNotFound);
                }

                //DO NOT USE AUTOMAPPER HERE. SLOW.
                var gps = new BusGpsDocument
                {
                    Id = Guid.NewGuid().ToString(),
                    // NOTE: In GeoJSON STANDARD, longitude comes before latitude.
                    // http://geojson.org/
                    Location = new Point(model.Lng, model.Lat),
                    SchoolCode = device.Bus.School.Code,
                    DeviceCode = device.DeviceCode,
                    Date = _dateTimeService.UtcNow(),
                };

                var result = await _busGpsLogService.Post(gps);
                if (result.StatusCode != HttpStatusCode.Created)
                {
                    _logger.LogError("bus gps coordinates were not saved");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }


        /// <summary>
        /// Returns the most recent location of all the buses for a school starting from a week ago
        /// </summary>
        /// <returns></returns>
        [HttpGet("mostrecent")]
        [Authorize(Policy = AuthPolicy.Reader)]
        [ProducesResponseType(typeof(List<BusGpsDocument>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<LocationModel>>> GetMostRecentLocations()
        {
            try
            {
                var buses = await service.FindAsync<Bus>(x => x.School.Code == SchoolCode && x.Device != null,
                    null,
                    true,
                    x => x.School,
                    x => x.Device);

                if (buses == null || buses.Empty())
                {
                    return NotFound($"school {SchoolCode} does not have any bus associated");
                }

                var deviceCodeList = buses.Select(x => x.Device.DeviceCode);

                var aWeekAgo = this._dateTimeService.UtcNow().AddDays(-7);

                var list = this._busGpsLogService
                    .GetMostRecentBusLocations(SchoolCode, deviceCodeList, aWeekAgo)
                    .ToList();

                var result = new List<LocationModel>();

                foreach (var l in list)
                {
                    var bus = buses.FirstOrDefault(x => x.Device.DeviceCode == l.DeviceCode);
                    if (bus == null)
                    {
                        _logger.LogWarning("no bus with device code {0} was found", l.DeviceCode);
                        continue;
                    }
                    var loc = new LocationModel
                    {
                        Lat = l.Location.Position.Latitude,
                        Lng = l.Location.Position.Longitude,
                        DateTime = l.Date,
                        BusName = bus.Name,
                        BusCode = bus.Code
                    };

                    result.Add(loc);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }


        /// <summary>
        /// Returns the most recent location of a specific school bus
        /// </summary>
        /// <param name="busCode"></param>
        /// <returns></returns>
        [HttpGet("mostrecent/{busCode}")]
        [Authorize(Policy = AuthPolicy.Reader)]
        [ProducesResponseType(typeof(List<BusGpsDocument>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<LocationModel>>> GetMostRecentLocationForBus(Guid busCode)
        {
            try
            {
                var bus = await service.FirstOrDefaultAsync<Bus>(x => x.School.Code == SchoolCode &&
                    x.Code == busCode &&
                    x.Device != null,
                    null,
                    true,
                    x => x.School,
                    x => x.Device);

                if (bus == null)
                {
                    return NotFound($"bus not found");
                }

                var deviceCodeList = new List<string>() { bus.Device.DeviceCode };

                var list = this._busGpsLogService
                    .GetMostRecentBusLocations(SchoolCode, deviceCodeList, DateTime.MinValue)
                    .ToList();

                var result = new List<LocationModel>();

                foreach (var location in list)
                {
                    var loc = new LocationModel
                    {
                        Lat = location.Location.Position.Latitude,
                        Lng = location.Location.Position.Longitude,
                        DateTime = location.Date,
                        BusName = bus.Name,
                        BusCode = bus.Code
                    };

                    result.Add(loc);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        /// <summary>
        /// Returns historic gps locations in a date range 
        /// </summary>
        /// <remarks>
        /// Dates in ISO 8601 format.
        /// https://en.wikipedia.org/wiki/ISO_8601
        /// </remarks>
        /// <param name="deviceCode">Device Code (XXXX#Y) </param>
        /// <param name="from">From (YYYY-MM-DD) </param>
        /// <param name="until">Until (YYYY-MM-DD) </param>
        /// <returns></returns>
        [HttpGet("historic/{deviceCode}/{from}/{until}")]
        [Authorize(Policy = AuthPolicy.Reader)]
        [ProducesResponseType(typeof(List<BusGpsDocument>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<LocationModel>> GetHistoricLocations(string deviceCode, DateTime from, DateTime until)
        {
            var list = this._busGpsLogService
                .GetHistoricBusLocation(SchoolCode, deviceCode.ToUpperInvariant(), from, until)
                .ToList();

            var result = new List<LocationModel>();

            foreach (var l in list)
            {
                var loc = new LocationModel
                {
                    Lat = l.Location.Position.Latitude,
                    Lng = l.Location.Position.Longitude,
                    DateTime = l.Date,
                    //	BusName = string.Empty 
                };

                result.Add(loc);
            }
            return result;
        }
    }
}
