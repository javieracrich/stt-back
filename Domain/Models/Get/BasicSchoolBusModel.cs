using System;

namespace Domain.Models
{
    public class BasicBusModel
    {
        public string Name { get; set; }

        public Guid Code { get; set; } //school bus code

        public string Patent { get; set; }

        public string DeviceCode { get; set; }
    }
}
