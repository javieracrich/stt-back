using Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace Domain.Models
{
    public class PostDeviceModel
    {
        [Required]
        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string Name { get; set; }

        [DeviceCode]
        public string DeviceCode { get; set; }

        public DeviceType Type { get; set; }

        public Guid? BusCode { get; set; }
    }
}
