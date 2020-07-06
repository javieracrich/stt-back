using Common;
using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
	public class FirebaseRegisterModel
	{
		[Required]
		[LettersAndNumbersOnly]
		public string UserId { get; set; }

		[Required]
		[LettersAndNumbersOnly]
		public string DeviceToken { get; set; }
	}
}
