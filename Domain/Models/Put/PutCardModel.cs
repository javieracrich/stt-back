using Common;
using System.Configuration;

namespace Domain.Models
{
    public class PutCardModel : IHaveRowVersion
    {
        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string Name { get; set; }
        public byte[] RowVersion { get; set; }
    }
}
