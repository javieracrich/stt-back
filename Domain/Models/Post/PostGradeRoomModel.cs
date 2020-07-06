using Common;
using System.Configuration;

namespace Domain.Models
{
    public class PostGradeRoomModel
    {
        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string Name { get; set; }

        public SchoolGrade Grade { get; set; }
    }
}
