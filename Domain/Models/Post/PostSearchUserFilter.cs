using Common;
using System.Collections.Generic;
using System.Configuration;

namespace Domain.Models
{
	public class PostSearchUserFilter
	{
		public PostSearchUserFilter()
		{
			UserCategories = new List<UserCategory>();
		}

        [StringValidator(InvalidCharacters = Constants.InvalidChars)]
        public string Text { get; set; }

        public List<UserCategory> UserCategories { get; set; }

        public bool? SchoolAttached { get; set; }
	}
}
