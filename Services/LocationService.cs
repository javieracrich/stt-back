using Common;
using Domain;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Services.Cosmos;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Services
{
    public class LocationService : ILocationService
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly ILocationLogService _locationLogService;
        private readonly IDeviceEventService _deviceEventService;
        private readonly IBusGpsLogService _busGpsLogService;
        private readonly GeneralOptions _generalOptions;
        private readonly ILogger<LocationService> _logger;

        public LocationService(IBusGpsLogService busGpsLogService,
            IDeviceEventService deviceEventService,
            ILocationLogService locationLogService,
            IDateTimeService dateTimeService,
            ILogger<LocationService> logger,
            GeneralOptions options)
        {

            _dateTimeService = dateTimeService;
            _logger = logger;
            _locationLogService = locationLogService;
            _deviceEventService = deviceEventService;
            _generalOptions = options;
            _busGpsLogService = busGpsLogService;
        }

        /// <summary>
        /// Retrieves the most recent location found in the historic log
        /// </summary>
        /// <param name="cardCode"></param>
        /// <returns></returns>
        public LocationModel GetMostRecentLocationFromHistoricLog(string cardCode)
        {
            var locationLog = _locationLogService.GetLatest(cardCode);

            if (locationLog == null)
            {
                _logger.LogWarning("latest location for card {0} was not found", cardCode);
                return NotFound();
            }

            var locationModel = new LocationModel
            {
                BusCode = locationLog.BusCode,
                BusName = locationLog.BusName,
                DateTime = locationLog.Date,
                Lat = locationLog.Lat,
                Lng = locationLog.Lng,
                NotFound = false,
                Status = locationLog.Status ?? string.Empty,
                SupervisorName = locationLog.SupervisorName
            };

            return locationModel;

        }

        public async Task<PushNotification> GetPushNotificationAsync(User student, Device device)
        {
            var cardCode = student.Cards.First().CardCode;

            var location = await GetCurrentLocationAsync(cardCode, device);

            if (location.NotFound)
            {
                return null;
            }

            var push = new PushNotification
            {
                PushType = location.PushType,
                CardCode = student.Cards.First().CardCode,
                Device = device,
                Student = student,
                ReceiverCode = student.Parent.Code,
                SchoolCode = student.School.Code,
                Message = string.Format(location.Status, student.FirstName)
            };

            return push;
        }

        /// <summary>
        /// Retrieves the current location within the last 5 minutes.
        /// </summary>
        /// <param name="cardCode"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        private async Task<LocationModel> GetCurrentLocationAsync(string cardCode, Device device)
        {
            LocationModel location;

            switch (device.Type)
            {
                case DeviceType.SchoolPointingOutside:
                case DeviceType.SchoolPointingInside:
                    location = SchoolLocationLogic(device, cardCode);
                    break;
                case DeviceType.BusFrontDoor:
                    location = BusLocationLogic(device);
                    break;
                default:
                    throw new Exception("invalid device type");
            }

            if (location.NotFound)
                return location;

            var locationLog = BuildLocationLogModel(cardCode, device, location);

            await _locationLogService.Post(locationLog);

            return location;

        }

        private LocationLogDocument BuildLocationLogModel(string cardCode, Device device, LocationModel location)
        {
            return new LocationLogDocument
            {
                Id = Guid.NewGuid().ToString(),
                Lat = location.Lat,
                Lng = location.Lng,
                NotFound = false,
                Status = location.Status,
                SupervisorName = location.SupervisorName,
                SchoolCode = device.School.Code,
                BusCode = location.BusCode,
                BusName = location.BusName,
                CardCode = cardCode,
                Date = _dateTimeService.UtcNow(),
                DeviceCode = device.DeviceCode
            };
        }

        private static string GetStatusText(Device device)
        {
            switch (device.Type)
            {
                case DeviceType.SchoolPointingOutside:
                    return "{0} is leaving school";
                case DeviceType.SchoolPointingInside:
                    return "{0} is entering school";
                default:
                    return "";
            }
        }

        private static PushType GetPushType(Device device)
        {
            switch (device.Type)
            {
                case DeviceType.SchoolPointingOutside:
                    return PushType.LEAVING_SCHOOL;
                case DeviceType.SchoolPointingInside:
                    return PushType.ENTERING_SCHOOL;
                default:
                    throw new Exception("Invalid device type");
            }
        }

        private LocationModel BusLocationLogic(Device device)
        {
            var deviceCodeList = new List<string> { device.DeviceCode };

            var schoolCode = device.School.Code;

            var mostRecentLocations = _busGpsLogService.GetMostRecentBusLocations(schoolCode, deviceCodeList, DateTime.MinValue);

            if (mostRecentLocations == null || mostRecentLocations.Empty())
            {
                return NotFound();
            }

            var document = mostRecentLocations.First();

            var position = document.Location.Position;

            var supervisors = device
                .Bus
                .DriversAndSupervisors
                .Where(x => x.Category == UserCategory.Supervisor)
                .ToList();

            var supervisor = supervisors.Any() ? supervisors.First() : new User();

            return new LocationModel
            {
                BusCode = device.Bus.Code,
                BusName = device.Bus.Name,
                DateTime = document.Date,
                Lat = position.Latitude,
                Lng = position.Longitude,
                Status = "{0} Is in transit",
                SupervisorName = $"{supervisor.FirstName} {supervisor.LastName}"
            };
        }

        private LocationModel SchoolLocationLogic(Device device, string cardCode)
        {
            var minutesAgo = _dateTimeService.UtcNow().AddMinutes(-_generalOptions.SchoolLocationLogicMinutesDifference);

            var schoolCode = device.School.Code;

            var latest = _deviceEventService.GetMostRecentDeviceEvent(schoolCode, device.RelatedDevice.DeviceCode, cardCode, minutesAgo);

            if (latest == null)
            {
                _logger.LogWarning("most recent location for card {0} was not found", cardCode);
                return NotFound();
            }

            return new LocationModel
            {
                Lat = device.School.Lat ?? 0,
                Lng = device.School.Lng ?? 0,
                Status = GetStatusText(device),
                PushType = GetPushType(device)
            };
        }

        private LocationModel NotFound()
        {
            var notFoundLocationModel = new LocationModel
            {
                Lat = 0,
                Lng = 0,
                Status = ErrorConstants.NotFound,
                SupervisorName = ErrorConstants.NotFound,
                BusName = ErrorConstants.NotFound,
                NotFound = true
            };
            return notFoundLocationModel;
        }
    }


    public interface ILocationService
    {
        /// <summary>
        /// Retrieves the last location found.
        /// </summary>
        /// <param name="cardCode"></param>
        /// <returns></returns>
        LocationModel GetMostRecentLocationFromHistoricLog(string cardCode);

        Task<PushNotification> GetPushNotificationAsync(User student, Device device);
    }
}
