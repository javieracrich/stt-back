using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
	public class FirebaseToken : BaseEntity
	{
		/// <summary>
		/// varchar(max) is not supported in SQLite.
		/// https://github.com/aspnet/EntityFrameworkCore/issues/7030
		/// </summary>
		[Column(TypeName = "varchar(8000)")]
		public string Token { get; set; }
	}
}
