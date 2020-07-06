using Common;
using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace Domain.Models
{
    public class PostAlertRequest
    {
        public UserCategory[] UserCategories { get; set; }
        public AlertType AlertType { get; set; }

        [Required]
        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string Message { get; set; }
    }
}