using System;

namespace Domain.Models
{
    public class BasicUserModel
    {
        public Guid Code { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public UserCategory Category { get; set; }

    }
}
