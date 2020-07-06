using System;

namespace Domain.Models
{
    public class NewsItemModel
    {
        public DateTime DateTime { get; set; }

        public Guid SchoolCode { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }

        public string AuthorFullName { get; set; }

        public Guid AuthorCode { get; set; }

        public Guid Code { get; set; }
    }
}
