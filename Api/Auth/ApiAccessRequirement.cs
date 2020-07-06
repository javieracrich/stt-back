using Microsoft.AspNetCore.Authorization;

namespace Api.Auth
{
    public class ApiAccessRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public ApiAccessRequirement(string permission)
        {
            Permission = permission;
        }
    }
}