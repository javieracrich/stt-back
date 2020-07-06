using System;

namespace Domain.Models
{
    public class RoomModel : IHaveRowVersion
    {
        public string Name { get; set; }

        public Guid Code { get; set; }

        //public Guid SchoolCode { get; set; }

        public SchoolGrade Grade { get; set; }

        public byte[] RowVersion { get; set; }
    }
}
