using Common;
using Domain;
using Microsoft.Extensions.Options;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Api.Auth
{
    public interface IJwtFactory
    {
        Task<string> GetToken(string userName, ClaimsIdentity identity);
        ClaimsIdentity GenerateClaimsIdentity(User user);
    }

    public class JwtFactory : IJwtFactory
    {
        private readonly JwtIssuerOptions _jwtOptions;

        public JwtFactory(IOptions<JwtIssuerOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;
            ThrowIfInvalidOptions(_jwtOptions);
        }

        private string GetApiAccessClaim(UserCategory userCategory)
        {
            switch (userCategory)
            {
                case UserCategory.SchoolDirector:
                case UserCategory.BusDriver:
                case UserCategory.Supervisor:
                case UserCategory.Student:
                case UserCategory.Parent:
                case UserCategory.Teacher:
                case UserCategory.GovState:
                    return ApiAccess.Reader;
                case UserCategory.Admin:
                    return $"{ApiAccess.Reader}|{ApiAccess.Contributor}";
                default:
                    throw new ArgumentOutOfRangeException(nameof(userCategory), userCategory, null);
            }
        }

        public async Task<string> GetToken(string userName, ClaimsIdentity identity)
        {
            var claims = new[]
             {
                 new Claim(JwtRegisteredClaimNames.Sub, userName),
                 new Claim(JwtRegisteredClaimNames.Jti, await _jwtOptions.JtiGenerator()),
                 new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(_jwtOptions.IssuedAt).ToString(), ClaimValueTypes.Integer64),
                 identity.FindFirst(Constants.JwtClaimIdentifiers.UserName),
                 identity.FindFirst(Constants.JwtClaimIdentifiers.Code),
                 identity.FindFirst(Constants.JwtClaimIdentifiers.SchoolCode),
                 identity.FindFirst(Constants.JwtClaimIdentifiers.SchoolName),
                 identity.FindFirst(Constants.JwtClaimIdentifiers.Category),
                 identity.FindFirst(Constants.JwtClaimIdentifiers.ApiAccess)
             };

            // Create the JWT security token and encode it.
            var jwt = new JwtSecurityToken(
                _jwtOptions.Issuer,
                _jwtOptions.Audience,
                claims,
                _jwtOptions.NotBefore,
                _jwtOptions.Expiration,
                _jwtOptions.SigningCredentials);

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        public ClaimsIdentity GenerateClaimsIdentity(User user)
        {
            var claimIdentity = new ClaimsIdentity(new GenericIdentity(user.UserName, "Token"), new[]
            {
                new Claim(Constants.JwtClaimIdentifiers.Code, user.Code.ToString()),
                new Claim(Constants.JwtClaimIdentifiers.ApiAccess, GetApiAccessClaim(user.Category)),
                new Claim(Constants.JwtClaimIdentifiers.Category, ((int)user.Category).ToString()),
                new Claim(Constants.JwtClaimIdentifiers.UserName, user.UserName)
            });

            if (user.School != null)
            {
                var schoolCode = new Claim(Constants.JwtClaimIdentifiers.SchoolCode, user.School.Code.ToString());
                var schoolName = new Claim(Constants.JwtClaimIdentifiers.SchoolName, user.School.Name.ToString());
                claimIdentity.AddClaim(schoolCode);
                claimIdentity.AddClaim(schoolName);
            }
            return claimIdentity;
        }

        /// <returns>Date converted to seconds since Unix epoch (Jan 1, 1970, midnight UTC).</returns>
        private static long ToUnixEpochDate(DateTime date)
          => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);

        private static void ThrowIfInvalidOptions(JwtIssuerOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            if (options.ValidFor <= TimeSpan.Zero)
            {
                throw new ArgumentException("ValidFor must be a non-zero TimeSpan.", nameof(JwtIssuerOptions.ValidFor));
            }

            if (options.SigningCredentials == null)
            {
                throw new ArgumentNullException(nameof(JwtIssuerOptions.SigningCredentials));
            }

            if (options.JtiGenerator == null)
            {
                throw new ArgumentNullException(nameof(JwtIssuerOptions.JtiGenerator));
            }
        }
    }
}
