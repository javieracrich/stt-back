using Common;
using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace Domain.Models
{
    public class PostSchoolModel
    {
        [Required]
        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string Name { get; set; }

        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string Phone { get; set; }

        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string Address { get; set; }

        [Url(ErrorMessage = ErrorConstants.InvalidUrl)]
        public string LogoUrl { get; set; }

        [EmailAddress(ErrorMessage = ErrorConstants.InvalidEmail)]
        public string Email { get; set; }

        [Range(-90, 90, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public double? Lat { get; set; }

        [Range(-180, 180, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public double? Lng { get; set; }
    }
}
