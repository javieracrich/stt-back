using Common;
using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace Domain.Models
{
    public class PostUserModel
    {
        [Required]
        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string FirstName { get; set; }

        [Required]
        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = ErrorConstants.InvalidEmail)]
        public string Email { get; set; }

        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string Phone { get; set; }
    }
}
