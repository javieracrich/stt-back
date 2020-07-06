using System;

namespace Domain
{
    public class NewsItem : BaseEntity, IBaseEntity
    {
        public DateTime DateTime { get; set; }
        public virtual School School { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public virtual User Author { get; set; }
        public Guid Code { get; set; }
    }
}
