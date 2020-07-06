using System.Linq;
using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore.Authorization;
using Services;

namespace Api.Auth
{
    public class ApiAccessHandler : AuthorizationHandler<ApiAccessRequirement>
    {
        private readonly ToggleOptions _options;

        public ApiAccessHandler(ToggleOptions options)
        {
            _options = options;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ApiAccessRequirement requirement)
        {

            if (!_options.AuthEnabled)
            {
                //AUTH IS DISABLED 
                //MAKE SURE AUTHORIZATION IS ENABLED IN PRODUCTION ENVIRONMENT!!!
                //THIS TOGGLE IS ONLY FOR SPECIFIC TESTING PURPOSES.
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            var apiAccessClaim = context
                .User
                .Claims
                .FirstOrDefault(c => c.Type == Constants.JwtClaimIdentifiers.ApiAccess);

            if (apiAccessClaim == null || string.IsNullOrWhiteSpace(apiAccessClaim.Value))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var permissions = apiAccessClaim.Value.Split("|");

            if (!permissions.Contains(requirement.Permission))
            {
                // To guarantee failure, even if other requirement handlers succeed, call context.Fail
                context.Fail();
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}