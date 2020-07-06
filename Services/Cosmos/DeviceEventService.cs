using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Services.Config;

namespace Services.Cosmos
{
    public interface IDeviceEventService
    {
        IEnumerable<DeviceEventDocument> GetDeviceEvents(Guid schoolCode, string deviceCode, string cardCode, DateTime starting);
        DeviceEventDocument GetMostRecentDeviceEvent(Guid schoolCode, string deviceCode, string cardCode, DateTime starting);
        Task<ResourceResponse<Document>> PostDeviceEvent(DeviceEventDocument deviceEvent);
    }

    public class DeviceEventService : CosmosService, IDeviceEventService
    {
        private readonly Uri _deviceEventUri;

        public DeviceEventService(CosmosOptions config, ToggleOptions toggleOptions, IDateTimeService dateTimeService)
            : base(config, toggleOptions, dateTimeService)
        {
            _deviceEventUri = GetCollectionUri(Config.DeviceEventCollectionId);
        }

        public Task<ResourceResponse<Document>> PostDeviceEvent(DeviceEventDocument deviceEvent)
        {
            return Client.CreateDocumentAsync(_deviceEventUri, deviceEvent);
        }

        public IEnumerable<DeviceEventDocument> GetDeviceEvents(Guid schoolCode, string deviceCode, string cardCode, DateTime starting)
        {
            return Client.CreateDocumentQuery<DeviceEventDocument>(_deviceEventUri, FeedOptions)
                        .Where(c => c.DeviceCode == deviceCode && c.SchoolCode == schoolCode && c.CardCode == cardCode && c.Date >= starting)
                        .OrderByDescending(x => x.Date)
                        .AsEnumerable();
        }

        public DeviceEventDocument GetMostRecentDeviceEvent(Guid schoolCode, string deviceCode, string cardCode, DateTime starting)
        {
            return Client.CreateDocumentQuery<DeviceEventDocument>(_deviceEventUri, FeedOptions)
                        .Where(c => c.DeviceCode == deviceCode && c.SchoolCode == schoolCode && c.CardCode == cardCode && c.Date >= starting)
                        .OrderByDescending(x => x.Date)
                        .Take(1)
                        .AsEnumerable()
                        .SingleOrDefault();
        }
    }
}


