namespace Services.Config
{
	public class CosmosOptions
	{
		public string EndpointUri { get; set; }
		public string PrimaryKey { get; set; }
		public string DatabaseId { get; set; }
		public string CardStatusCollectionId { get; set; }
		public string DeviceEventCollectionId { get; set; }
		public string BusGpsCollectionId { get; set; }
		public string LocationLogCollectionId { get; set; }
	}
}
