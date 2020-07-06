using Domain.Models;
using System;

namespace Domain
{
    public class Room : BaseEntity, IBaseEntity , IHaveRowVersion
    {
        public string Name { get; set; }
        public Guid Code { get; set; }
        public virtual School School { get; set; }
        public SchoolGrade Grade { get; set; }
    }
}
