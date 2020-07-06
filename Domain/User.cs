using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class User : IdentityUser<int>, IBaseEntity
    {
        public User()
        {
            Students = new List<User>();
            Cards = new List<Card>();
        }

        public Guid Code { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public UserCategory Category { get; set; }
        public string PhotoUrl { get; set; }

        public virtual School School { get; set; }

        //only for students
        [ForeignKey("Room")]
        public int? RoomId { get; set; }
        public virtual Room Room { get; set; }
        public virtual List<Card> Cards { get; set; }
        public string StudentId { get; set; }
        public virtual User Parent { get; set; }

        //only for parents
        public virtual List<User> Students { get; set; }

        //only for drivers and supervisors
        [ForeignKey("Bus")]
        public int? BusId { get; set; }
        public virtual Bus Bus { get; set; }

        //audit
        public DateTime? Created { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? Updated { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool? IsDisabled { get; set; }
    }
}
