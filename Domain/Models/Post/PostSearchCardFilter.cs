
using Common;
using System.Configuration;

namespace Domain.Models
{
	public class PostSearchCardFilter
	{
        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string Code { get; set; }

        public bool UserAttached { get; set; }
	}
}
