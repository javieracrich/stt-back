using Common;
using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace Domain.Models
{
    public class PostUserWithPasswordModel
    {
        [Required]
        [EmailAddress(ErrorMessage = ErrorConstants.InvalidEmail)]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string FirstName { get; set; }

        [Required]
        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string LastName { get; set; }

        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string Phone { get; set; }
    }

}
