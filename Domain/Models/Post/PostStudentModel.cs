
using Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace Domain.Models
{
    public class PostStudentModel
    {
        [Required]
        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string FirstName { get; set; }

        [Required]
        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string LastName { get; set; }

        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string StudentId { get; set; }

        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string Phone { get; set; }

        [Required]
        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string CardCode { get; set; }

        [Required]
        public Guid GradeRoomCode { get; set; }

        [Required]
        public Guid SchoolCode { get; set; }
    }
}
