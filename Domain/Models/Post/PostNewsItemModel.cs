using Common;
using System.Configuration;

namespace Domain.Models
{
    public class PostNewsItemModel
    {
        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string Title { get; set; }

        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string Body { get; set; }
    }
}
