using Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace Domain.Models
{
    public class PutSchoolModel : IHaveRowVersion
    {
        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string Name { get; set; }

        [Url(ErrorMessage = ErrorConstants.InvalidUrl)]
        public string LogoUrl { get; set; }

        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string Phone { get; set; }

        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string Address { get; set; }

        [Range(-90, 90, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public double Lat { get; set; }

        [Range(-180, 180, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public double Lng { get; set; }

        [EmailAddress(ErrorMessage = ErrorConstants.InvalidEmail)]
        public string Email { get; set; }

        public Guid? DirectorCode { get; set; }

        public byte[] RowVersion { get; set; }
    }
}
