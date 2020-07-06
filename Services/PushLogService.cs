using Domain;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{

    public class PushLogService : IPushLogService
    {
        private readonly IGenericService _service;
        private readonly IDateTimeService _dateTimeService;
        private readonly ILogger<PushLogService> _logger;
        private readonly GeneralOptions _generalOptions;

        public PushLogService(IGenericService service,
            IDateTimeService dateTimeService,
            ILogger<PushLogService> logger,
            GeneralOptions generalOptions)
        {
            this._service = service;
            this._dateTimeService = dateTimeService;
            this._logger = logger;
            this._generalOptions = generalOptions;
        }

        public async Task<int> UpsertPushLog(PushNotification push)
        {
            var pushLog = await _service.FirstOrDefaultAsync<PushLog>(
                x => x.CardCode == push.CardCode &&
                x.DeviceCode == push.Device.DeviceCode &&
                x.SchoolCode == push.SchoolCode &&
                x.PushType == push.PushType);

            if (pushLog != null)
            {
                pushLog.Date = _dateTimeService.UtcNow();
                return await _service.UpdateAsync(pushLog);
            }

            return await _service.CreateAsync(new PushLog
            {
                CardCode = push.CardCode,
                Date = _dateTimeService.UtcNow(),
                DeviceCode = push.Device.DeviceCode,
                SchoolCode = push.SchoolCode,
                PushType = push.PushType
            });
        }

        public async Task<bool> ShouldSendPushNotification(PushNotification push)
        {
            _logger.LogInformation("starting sspn for push type {0}, card {1} and device {2}", push.PushType, push.CardCode, push.Device.DeviceCode);

            var watch = new Stopwatch();
            watch.Start();

            var ago = _dateTimeService.UtcNow().AddMinutes(-_generalOptions.MinutesBetweenPushNotifications);

            var mostRecentPushLog = await _service.FirstOrDefaultAsync<PushLog>(x =>
              x.CardCode == push.CardCode &&
              x.SchoolCode == push.SchoolCode &&
              x.Date > ago,
              m => m.OrderByDescending(x => x.Date), true);

            if (mostRecentPushLog == null)
            {
                _logger.LogInformation("sspn returned true because most recent push log was not found");
                LogFinished(push, watch);
                return true;
            }

            if (mostRecentPushLog.PushType == GetTheOther(push.PushType))
            {
                _logger.LogInformation("sspn returned true because most recent push date is {0} and the type is {1}", mostRecentPushLog.Date, mostRecentPushLog.PushType);
                LogFinished(push, watch);
                return true;
            }

            _logger.LogInformation("sspn returned false");
            LogFinished(push, watch);
            return false;

            void LogFinished(PushNotification p, Stopwatch w)
            {
                w.Stop();
                _logger.LogInformation("finished sspn for push type {0}, card {1} and device {2}", p.PushType, p.CardCode, p.Device.DeviceCode);
                _logger.LogInformation("sspn took {0} ms", w.ElapsedMilliseconds);
            }

        }


        private static PushType GetTheOther(PushType pushType)
        {
            switch (pushType)
            {
                case PushType.ENTERING_SCHOOL:
                    return PushType.LEAVING_SCHOOL;
                case PushType.LEAVING_SCHOOL:
                    return PushType.ENTERING_SCHOOL;
                default:
                    throw new Exception("invalid push type");
            }
        }


    }

    public interface IPushLogService
    {
        Task<bool> ShouldSendPushNotification(PushNotification push);
        Task<int> UpsertPushLog(PushNotification push);
    }
}
