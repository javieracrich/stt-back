using Domain.Models;
using System;

namespace Domain
{
    public class PushLog : BaseEntity, IBaseEntity, IHaveRowVersion
    {
        public string DeviceCode { get; set; }

        public string CardCode { get; set; }

        public Guid SchoolCode { get; set; }

        public PushType PushType { get; set; }

        public DateTime Date { get; set; }
    }
}
