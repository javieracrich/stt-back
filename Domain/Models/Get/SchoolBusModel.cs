using System;
using System.Collections.Generic;

namespace Domain.Models
{
    public class BusModel : IHaveRowVersion
    {
        public BusModel()
        {
            Drivers = new List<BasicUserModel>();
            Supervisors = new List<BasicUserModel>();
        }

        public Guid Code { get; set; }

        public string Name { get; set; }

        public string Patent { get; set; }

        public BasicSchoolModel School { get; set; }

        public IEnumerable<BasicUserModel> Drivers { get; set; }

        public IEnumerable<BasicUserModel> Supervisors { get; set; }

        public BasicDeviceModel Device { get; set; }

        public byte[] RowVersion { get; set; }
    }
}
