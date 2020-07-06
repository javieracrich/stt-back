using Common;
using Domain.Models;
using System.Configuration;

namespace Domain
{
    public class PutGradeRoomModel : IHaveRowVersion
    {
        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string Name { get; set; }
        public byte[] RowVersion { get; set; }
    }
}
