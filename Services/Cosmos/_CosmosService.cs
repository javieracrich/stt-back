using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Services.Config;

namespace Services.Cosmos
{
    // https://github.com/Azure/azure-cosmosdb-dotnet/tree/master/samples
    // https://docs.microsoft.com/en-us/azure/cosmos-db/partition-data
    // https://www.youtube.com/watch?v=5YNJpGwj_Zs

    public abstract class CosmosService
    {
        protected readonly CosmosOptions Config;
        protected readonly ToggleOptions ToggleOptions;
        protected readonly FeedOptions FeedOptions;
        protected readonly IDateTimeService DateTimeService;
        protected readonly DocumentClient Client;

        protected CosmosService(CosmosOptions config, ToggleOptions toggleOptions, IDateTimeService dateTimeService)
        {
            Config = config;
            ToggleOptions = toggleOptions;
            DateTimeService = dateTimeService;
            FeedOptions = new FeedOptions
            {
                MaxItemCount = -1,
                EnableCrossPartitionQuery = true,
                MaxDegreeOfParallelism = -1,
                MaxBufferedItemCount = -1,
                ConsistencyLevel = ConsistencyLevel.Eventual
            };
            // https://medium.com/@thomasweiss_io/how-i-learned-to-stop-worrying-and-love-cosmos-dbs-request-units-92c68c62c938

            Client = new DocumentClient(new Uri(Config.EndpointUri), Config.PrimaryKey);
        }

        //protected DocumentClient GetClient()
        //{
        //    return new DocumentClient(new Uri(Config.EndpointUri), Config.PrimaryKey);
        //}

        protected Uri GetCollectionUri(string collectionId)
        {
            return UriFactory.CreateDocumentCollectionUri(Config.DatabaseId, collectionId);
        }
    }
}


