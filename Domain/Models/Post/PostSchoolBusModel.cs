using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace Domain.Models
{
    public class PostBusModel
    {
        public PostBusModel()
        {
            Drivers = new List<Guid>();
            Supervisors = new List<Guid>();
        }

        [Required]
        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string Name { get; set; }

        [EnsureMinimumElements(1, ErrorMessage = "At least 1 driver is required")]
        public List<Guid> Drivers { get; set; }

        [EnsureMinimumElements(1, ErrorMessage = "At least 1 supervisor is required")]
        public List<Guid> Supervisors { get; set; }

        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string DeviceCode { get; set; }

        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string Patent { get; set; }
    }
}
