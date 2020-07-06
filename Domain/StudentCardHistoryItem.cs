using Domain.Models;
using System;

namespace Domain
{
    public class StudentCardHistoryItem : BaseEntity, IBaseEntity, IHaveRowVersion
    {
        public int UserId { get; set; }
        public int CardId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? UntilDate { get; set; }
    }
}
