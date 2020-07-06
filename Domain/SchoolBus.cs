using Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class Bus : BaseEntity, IBaseEntity, IHaveRowVersion
    {
        public Bus()
        {
            DriversAndSupervisors = new List<User>();
        }

        public Guid Code { get; set; }
        public string Name { get; set; }
        public string Patent { get; set; }
        public virtual School School { get; set; }
        public virtual List<User> DriversAndSupervisors { get; set; }

        [ForeignKey("Device")]
        public int? DeviceId { get; set; }
        public virtual Device Device { get; set; }
    }
}
