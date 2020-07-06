using Common;
using System.Configuration;

namespace Domain.Models
{
    public class PostUserFilter
    {
        public UserCategory Category { get; set; }

        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string  Text { get; set; }
    }
}
