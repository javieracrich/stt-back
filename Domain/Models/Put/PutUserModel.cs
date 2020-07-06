using Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace Domain.Models
{
    public class PutUserModel : IHaveRowVersion
    {
        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string FirstName { get; set; }

        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string LastName { get; set; }

        public Guid? GradeRoomCode { get; set; }

        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string CardCode { get; set; }

        [EmailAddress(ErrorMessage = ErrorConstants.InvalidEmail)]
        public string Email { get; set; }

        [Url(ErrorMessage = ErrorConstants.InvalidUrl)]
        public string PhotoUrl { get; set; }

        public Guid? SchoolCode { get; set; }

        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string Phone { get; set; }

        public byte[] RowVersion { get; set; }
    }
}
