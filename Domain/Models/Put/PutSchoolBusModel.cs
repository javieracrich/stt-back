using Common;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace Domain.Models
{
    public class PutBusModel : IHaveRowVersion
    {
        public PutBusModel()
        {
            DriverCodes = new List<Guid>();
            SupervisorCodes = new List<Guid>();
        }

        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string Name { get; set; }
        public List<Guid> DriverCodes { get; set; }
        public List<Guid> SupervisorCodes { get; set; }
        public string DeviceCode { get; set; }
        public string Patent { get; set; }
        public byte[] RowVersion { get; set; }
    }
}
