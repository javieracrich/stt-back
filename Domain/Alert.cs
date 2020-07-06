using Domain.Models;
using System;

namespace Domain
{
    public class Alert : BaseEntity, IBaseEntity, IHaveRowVersion
    {
        public string UserCategories { get; set; }
        public AlertType AlertType { get; set; }
        public string Message { get; set; }
        public DateTime DateTime { get; set; }
        public virtual User Author { get; set; }
        public virtual School School { get; set; }
    }
}