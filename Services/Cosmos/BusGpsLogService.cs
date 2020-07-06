using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Spatial;
using Services.Config;

namespace Services.Cosmos
{

    public interface IBusGpsLogService
    {
        IEnumerable<BusGpsDocument> GetHistoricBusLocation(Guid schoolCode, string deviceCode, DateTime fromDate, DateTime untilDate);
        IEnumerable<BusGpsDocument> GetMostRecentBusLocations(Guid schoolCode, IEnumerable<string> deviceCodeList, DateTime starting);
        Task<ResourceResponse<Document>> Post(BusGpsDocument gps);
    }

    public class BusGpsLogService : CosmosService, IBusGpsLogService
    {
        private readonly Uri _busGpsUri;

        public BusGpsLogService(CosmosOptions config, ToggleOptions toggleOptions, IDateTimeService dateTimeService)
                : base(config, toggleOptions, dateTimeService)
        {
            _busGpsUri = GetCollectionUri(Config.BusGpsCollectionId);
        }

        public IEnumerable<BusGpsDocument> GetMostRecentBusLocations(Guid schoolCode, IEnumerable<string> deviceCodeList, DateTime starting)
        {
            //todo. delete this
            if (!ToggleOptions.CosmosEnabled)
            {
                return MostRecentDummyData();
            }

            //GROUPBY DOES NOT WORK
            //https://feedback.azure.com/forums/263030-azure-cosmos-db/suggestions/18561901-add-group-by-support-for-aggregate-functions
            //TODO USE GROUPBY DEVICE ID ONCE THE fuckers SHIP THE FEATURE SO YOU DONT HAVE TO DO FOREACH DEVICE
            var list = new List<BusGpsDocument>();

            foreach (var deviceCode in deviceCodeList)
            {
                var result = Client.CreateDocumentQuery<BusGpsDocument>(_busGpsUri, FeedOptions)
               .Where(doc => deviceCode == doc.DeviceCode && doc.SchoolCode == schoolCode && doc.Date >= starting)
               .OrderByDescending(x => x.Date)
               .Take(1)
               .AsEnumerable()
               .FirstOrDefault();

                if (result != null)
                {
                    list.Add(result);
                }
            }

            return list;
        }

        public IEnumerable<BusGpsDocument> GetHistoricBusLocation(Guid schoolCode, string deviceCode, DateTime fromDate, DateTime untilDate)
        {
            //todo. delete this
            if (!ToggleOptions.CosmosEnabled)
            {
                return HistoricDummyData();
            }

            return Client.CreateDocumentQuery<BusGpsDocument>(_busGpsUri, FeedOptions)
                .Where(c => c.DeviceCode == deviceCode && c.SchoolCode == schoolCode && fromDate <= c.Date && c.Date <= untilDate)
                .OrderByDescending(c => c.Date)
                .AsEnumerable();
        }

        public Task<ResourceResponse<Document>> Post(BusGpsDocument gps)
        {
            return Client.CreateDocumentAsync(_busGpsUri, gps);
        }

        private static IEnumerable<BusGpsDocument> HistoricDummyData()
        {
            var list = new List<BusGpsDocument>();
            var lat = 25.7723328M;
            var lng = -80.2381683M;

            for (var i = 0; i < 250; i++)
            {
                lat = lat + 0.0001M;
                var location = new BusGpsDocument
                {
                    Location = new Point((double)lng, (double)lat),
                };
                list.Add(location);
            }

            for (var i = 0; i < 300; i++)
            {
                lng = lng + 0.0001M;
                var location = new BusGpsDocument
                {
                    Location = new Point((double)lng, (double)lat),
                };
                list.Add(location);
            }

            for (var i = 0; i < 250; i++)
            {
                lat = lat + 0.0001M;
                var location = new BusGpsDocument
                {
                    Location = new Point((double)lng, (double)lat),
                };
                list.Add(location);
            }
            for (var i = 0; i < 300; i++)
            {
                lng -= 0.0002M;
                var location = new BusGpsDocument
                {
                    Location = new Point((double)lng, (double)lat),
                };
                list.Add(location);
            }

            return list;
        }

        private IEnumerable<BusGpsDocument> MostRecentDummyData()
        {
            var list = new List<BusGpsDocument>();
            var location = new BusGpsDocument
            {
                Location = new Point(-80.2249736, 25.7920835),
                Date = DateTimeService.UtcNow(),
                DeviceCode = "TEST#1"
            };

            list.Add(location);
            return list;
        }
    }
}


