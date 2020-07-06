
using Common;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Api.Auth
{
    public class Tokens
    {
        public static async Task<LoginResult> GetLoginResult(ClaimsIdentity identity, IJwtFactory jwtFactory, string userName, JwtIssuerOptions jwtOptions)
        {
            return new LoginResult
            {
                //todo. remove user name and code. values are already encoded in the token.
                UserName = identity.Claims.Single(c => c.Type == Constants.JwtClaimIdentifiers.UserName).Value,
                UserCode = identity.Claims.Single(c => c.Type == Constants.JwtClaimIdentifiers.Code).Value,

                Token = await jwtFactory.GetToken(userName, identity),
            };
        }
    }
}
