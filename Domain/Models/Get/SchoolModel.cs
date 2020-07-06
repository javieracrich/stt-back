using System;
using System.Collections.Generic;

namespace Domain.Models
{
    public class SchoolModel : IHaveRowVersion
    {
        public string Name { get; set; }

        public string LogoUrl { get; set; }

        public Guid Code { get; set; }

        public string Phone { get; set; }

        public string Address { get; set; }

        public double Lat { get; set; }

        public double Lng { get; set; }

        public string Email { get; set; }

        public IEnumerable<BasicUserModel> Directors { get; set; }

        public byte[] RowVersion { get; set; }
    }
}
