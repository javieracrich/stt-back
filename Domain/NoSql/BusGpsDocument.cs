using Domain.NoSql;
using Microsoft.Azure.Documents.Spatial;
using Newtonsoft.Json;

namespace Domain
{
    public class BusGpsDocument : NoSqlDocument
	{
		[JsonProperty("location")]
		public Point Location { get; set; }

		[JsonProperty("deviceCode")]
		public string DeviceCode { get; set; }
    }
}
