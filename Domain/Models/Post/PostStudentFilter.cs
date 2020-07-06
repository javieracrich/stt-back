using Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace Domain.Models
{
    public class PostStudentFilter
    {
        public Guid? RoomCode { get; set; }

        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string Text { get; set; }

        [Required]
        public PushType LastStatusId { get; set; }
    }
}
