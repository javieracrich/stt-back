using Domain.NoSql;
using Newtonsoft.Json;

namespace Domain
{
	//NO SQL
	public class DeviceEventDocument : NoSqlDocument
	{

		[JsonProperty("deviceCode")]
		public string DeviceCode { get; set; }

		[JsonProperty("type")]
		public DeviceType Type { get; set; }

		[JsonProperty("cardCode")]
		public string CardCode { get; set; }

		[JsonProperty("relatedDeviceCode")]
		public string RelatedDeviceCode { get; set; }
	}

}
