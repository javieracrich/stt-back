using Common;
using Domain;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace Services
{
    public class PrincipalProvider : IPrincipalProvider
    {
        private readonly ILogger<PrincipalProvider> _logger;
        private readonly ClaimsPrincipal _principal;
        public PrincipalProvider(IPrincipal principal, ILogger<PrincipalProvider> logger)
        {
            _logger = logger;
            _principal = principal as ClaimsPrincipal;
        }

        public Guid? GetUserCode()
        {
            var code = _principal?
                .Claims?
                .FirstOrDefault(x => x.Type == Constants.JwtClaimIdentifiers.Code)?
                .Value;

            if (code != null)
                return Guid.Parse(code);

            _logger.LogWarning("current user is null");
            return null;
        }

        public UserCategory? GetUserCategory()
        {
            var category = _principal?
                .Claims?
                .FirstOrDefault(x => x.Type == Constants.JwtClaimIdentifiers.Category)?
                .Value;

            if (category != null)
                return (UserCategory)Enum.Parse(typeof(UserCategory), category);

            return null;
        }

        public Guid? GetSchoolCode()
        {
            var code = _principal?
                .Claims?
                .FirstOrDefault(x => x.Type == Constants.JwtClaimIdentifiers.SchoolCode)?
                .Value;

            if (code != null)
                return Guid.Parse(code);

            _logger.LogWarning("current school code is null");
            return null;

        }
    }

    public interface IPrincipalProvider
    {
        Guid? GetUserCode();
        UserCategory? GetUserCategory();
        Guid? GetSchoolCode();
    }
}
