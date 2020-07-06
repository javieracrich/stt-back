using Common;
using System;
using System.Configuration;

namespace Domain.Models
{
    public class PutDeviceModel : IHaveRowVersion
    {
        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string Name { get; set; }
        public Guid? BusCode { get; set; }
        public DeviceType Type { get; set; }
        public byte[] RowVersion { get; set; }
    }
}
