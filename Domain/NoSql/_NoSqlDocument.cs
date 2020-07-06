
using Newtonsoft.Json;
using System;

namespace Domain.NoSql
{
	public abstract class NoSqlDocument
	{
		[JsonProperty(PropertyName = "id")] //lowercase id property is required for cosmos document
		public string Id { get; set; }

        // partition key
        [JsonProperty("schoolCode")]
        public Guid SchoolCode { get; set; }

        //TODO. REMOVE DATE. USE  _TS INTERNAL PROPERTY FROM COSMOS
        [JsonProperty(PropertyName = "date")]
		public DateTime Date { get; set; }

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
