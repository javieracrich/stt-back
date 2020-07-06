using Common;
using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace Domain.Models
{
    public class PostCardModel
	{
		[Required]
        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string Name { get; set; }

		[Required]
        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string CardCode { get; set; }
	}
}
