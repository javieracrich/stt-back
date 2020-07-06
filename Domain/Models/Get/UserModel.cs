using System;
using System.Collections.Generic;

namespace Domain.Models
{
    public class UserModel 
    {
        public UserModel()
        {
            Cards = new List<CardModel>();
        }

        public Guid Code { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public UserCategory Category { get; set; }

        public SchoolGrade? Grade { get; set; }

        public string GradeRoomName { get; set; }

        public string StudentId { get; set; }

        public List<CardModel> Cards { get; set; }

        public string Email { get; set; }

        public string PhotoUrl { get; set; }

        public string Phone { get; set; }

        public BasicSchoolModel School { get; set; }

        public int LastStatusId { get; set; }

        public DateTime? LastStatusDateTime { get; set; }
    }
}
