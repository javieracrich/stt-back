using Common;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class PostBusGpsModel
    {
        [Required]
        [JsonProperty("lat")]
        [Range(-90, 90, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public double Lat { get; set; }

        [Required]
        [JsonProperty("lng")]
        [Range(-180, 180, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public double Lng { get; set; }

        [Required]
        [JsonProperty("deviceCode")]
        [DeviceCode]
        public string DeviceCode { get; set; }
    }
}
