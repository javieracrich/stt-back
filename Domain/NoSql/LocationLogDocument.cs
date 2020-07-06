using Domain.NoSql;
using Newtonsoft.Json;
using System;

namespace Domain
{
	public class LocationLogDocument : NoSqlDocument
	{
		[JsonProperty("lat")]
		public double Lat { get; set; }

		[JsonProperty("lng")]
		public double Lng { get; set; }

		[JsonProperty("busName")]
		public string BusName { get; set; }

		[JsonProperty("busCode")]
		public Guid? BusCode { get; set; }

		[JsonProperty("supervisorName")]
		public string SupervisorName { get; set; }

		[JsonProperty("status")]
		public string Status { get; set; }

		[JsonProperty("notFound")]
		public bool NotFound { get; set; }

		[JsonProperty("deviceCode")]
		public string DeviceCode { get; set; }

		[JsonProperty("cardCode")]
		public string CardCode { get; set; }
	}
}
