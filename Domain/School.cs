using Domain.Models;
using System;
using System.Collections.Generic;

namespace Domain
{
    public class School : BaseEntity, IBaseEntity, IHaveRowVersion
    {
        public School()
        {
            Users = new List<User>();
        }

        public Guid Code { get; set; }
        public string Name { get; set; }
        public string LogoUrl { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }

        public double? Lat { get; set; }
        public double? Lng { get; set; }

        public string Email { get; set; }
        public virtual List<User> Users { get; set; }
        public virtual List<Device> Devices { get; set; }
        public virtual List<Room> Rooms { get; set; }
    }
}
