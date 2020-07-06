using System;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Services.Config;

namespace Services.Cosmos
{

	public interface ILocationLogService
	{
		LocationLogDocument GetLatest(string cardCode);
		Task<ResourceResponse<Document>> Post(LocationLogDocument locationLogDocument);
	}

	public class LocationLogService : CosmosService, ILocationLogService
	{
		private readonly Uri _locationLogUri;
		public LocationLogService(CosmosOptions config, ToggleOptions toggleOptions, IDateTimeService dateTimeService)
			: base(config, toggleOptions, dateTimeService)
		{
			_locationLogUri = GetCollectionUri(Config.LocationLogCollectionId);
		}

		public Task<ResourceResponse<Document>> Post(LocationLogDocument locationLogDocument)
		{
			return Client.CreateDocumentAsync(_locationLogUri, locationLogDocument);
		}

		public LocationLogDocument GetLatest(string cardCode)
		{
			return Client.CreateDocumentQuery<LocationLogDocument>(_locationLogUri, FeedOptions)
				.Where(c => c.CardCode == cardCode)
				.OrderByDescending(x => x.Date)
				.Take(1)
				.AsEnumerable()
				.SingleOrDefault();
		}

        //public LocationLogDocument DeleteAll()
        //{
        //    return Client.DeleteDocumentCollectionAsync()CreateDocumentQuery<LocationLogDocument>(_locationLogUri, FeedOptions)
        //        .Where(c => c.CardCode == cardCode)
        //        .OrderByDescending(x => x.Date)
        //        .Take(1)
        //        .AsEnumerable()
        //        .SingleOrDefault();
        //}
    }
}


