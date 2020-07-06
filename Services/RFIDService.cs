using Common;
using Domain;
using Domain.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;
using Services.Cosmos;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public interface IRFIDService
    {
        Task<long> Process(PostDeviceEventModel model);
    }

    public class RFIDService : IRFIDService
    {
        private readonly ILogger<RFIDService> logger;
        private readonly IGenericService service;
        private readonly IDeviceEventService deviceEventService;
        private readonly IDateTimeService dateTimeService;
        private readonly ILocationService locationService;
        private readonly IPushNotificationService pushNotificationService;

        public RFIDService(ILogger<RFIDService> logger,
            IGenericService service,
            IDeviceEventService deviceEventService,
            IDateTimeService dateTimeService,
            ILocationService locationService,
            IPushNotificationService pushNotificationService
            )
        {
            this.logger = logger;
            this.service = service;
            this.deviceEventService = deviceEventService;
            this.dateTimeService = dateTimeService;
            this.locationService = locationService;
            this.pushNotificationService = pushNotificationService;
        }




        public async Task<long> Process(PostDeviceEventModel model)
        {

            // TODO: CACHE THIS CALL 
            var device = await service.FirstOrDefaultAsync<Device>(x => x.DeviceCode == model.DeviceCode, null, true,
                x => x.RelatedDevice,
                x => x.School,
                x => x.Bus.DriversAndSupervisors);

            // validate
            if (device == null)
            {
                logger.LogError("device with code {0} was not found", model.DeviceCode);
                return 0;
            }

            var distinct = model.CardCodes.Distinct().ToList();

            var watch = new Stopwatch();

            watch.Start();

            var tasks = new List<Task>();



            foreach (var cardCode in distinct)
            {
                //TODO: MOVE THIS TO A MICRO SERVICE.
                //PROCESS ASYNC
                await ProcessCard(cardCode, device);
            }

            await Task.WhenAll(tasks.ToArray());

            watch.Stop();

            logger.LogInformation("processing {0} card codes took {1} ms", distinct.Count, watch.ElapsedMilliseconds);

            return watch.ElapsedMilliseconds;
        }


        private async Task ProcessCard(string cardCode, Device device)
        {
            logger.LogInformation("starting to process card {0}", cardCode);
            //TODO: CACHE THIS CALL
            var card = await service.FirstOrDefaultAsync<Card>(
                x => x.CardCode == cardCode,
                null,
                true,
                x => x.User.Parent,
                x => x.User.School);

            if (card == null)
            {
                logger.LogWarning("card with code {0} was not found", cardCode);
                return;
            }

            var result = await deviceEventService.PostDeviceEvent(new DeviceEventDocument
            {
                Id = Guid.NewGuid().ToString(),
                CardCode = cardCode,
                DeviceCode = device.DeviceCode,
                SchoolCode = device.School.Code,
                RelatedDeviceCode = device.RelatedDevice.DeviceCode,
                Type = device.Type,
                Date = dateTimeService.UtcNow(),
            });

            LogResult(cardCode, device, result);

            if (card.User == null)
            {
                logger.LogError("card {0} does not have any user attached", card.User.Code);
                return;
            }

            var push = await locationService.GetPushNotificationAsync(card.User, device);

            if (push == null)
            {
                logger.LogError("push is null");
                return;
            }

            await pushNotificationService.SendPush(push);

        }


        private void LogResult(string cardCode, Device device, ResourceResponse<Document> result)
        {
            if (result.StatusCode == System.Net.HttpStatusCode.Created)
            {
                logger.LogInformation("rfid event for card {0} and device {1} was successfully saved", cardCode, device.DeviceCode);
            }
            else
            {
                logger.LogWarning("rfid event for card {0} and device {1} was not saved", cardCode, device.DeviceCode);
            }
        }
    }

}
