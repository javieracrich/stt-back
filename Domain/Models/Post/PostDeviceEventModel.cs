using Common;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;

namespace Domain.Models
{
    public class PostDeviceEventModel
    {
        public PostDeviceEventModel()
        {
            CardCodes = new List<string>();
        }

        [JsonProperty("deviceCode")]
        [DeviceCode]
        public string DeviceCode { get; set; }

        [JsonProperty("cardCodes")]
        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public List<string> CardCodes { get; set; }

    }

}
