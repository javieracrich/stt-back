using Common;
using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace Domain.Models
{
	public class PostCredentialModel
	{
		[Required]
        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string UserName { get; set; }

		[Required]
		public string Password { get; set; }
	}
}
