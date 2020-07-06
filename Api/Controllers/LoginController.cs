using Api.Auth;
using Common;
using Domain;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Route("[controller]")]
	[ApiController]
	public class LoginController : ControllerBase
	{

		private readonly SttUserManager _userManager;
		private readonly IJwtFactory _jwtFactory;
		private readonly JwtIssuerOptions _jwtOptions;
        private readonly ILogger<LoginController> _logger;

		public LoginController(SttUserManager userManager, ILogger<LoginController> logger, IJwtFactory jwtFactory, IOptions<JwtIssuerOptions> jwtOptions)
		{
			_userManager = userManager;
			_jwtFactory = jwtFactory;
			_jwtOptions = jwtOptions.Value;
            _logger = logger;
		}

        [HttpPost]
		[AllowAnonymous]
		[ProducesResponseType(typeof(LoginResult), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> PostCredentials(PostCredentialModel credentials)
		{
			try
			{
				var identity = await GetClaimsIdentity(credentials.UserName, credentials.Password);

				if (identity == null)
				{
					return BadRequest(ErrorConstants.InvalidUsernameOrPassword);
				}

				var loginResult = await Tokens.GetLoginResult(identity, _jwtFactory, credentials.UserName, _jwtOptions);

				return Ok(loginResult);
			}
			catch (Exception ex)
			{
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
			}
		}

		private async Task<ClaimsIdentity> GetClaimsIdentity(string userName, string password)
		{
			if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
				return await Task.FromResult<ClaimsIdentity>(null);

			// get user to verify
			var userToVerify = await _userManager.FindByNameAsync(userName);

			if (userToVerify == null)
			{
				return await Task.FromResult<ClaimsIdentity>(null);
			}

			// students are not allowed to sign in.
			var forbiddenCategoriesToSignIn = new List<UserCategory>() {
				UserCategory.Student,
			};

			if (forbiddenCategoriesToSignIn.Contains(userToVerify.Category))
			{
				return await Task.FromResult<ClaimsIdentity>(null);
			}

			// check the credentials
			if (await _userManager.CheckPasswordAsync(userToVerify, password))
			{
				return await Task.FromResult(_jwtFactory.GenerateClaimsIdentity(userToVerify));
			}

			// Credentials are invalid, or account doesn't exist
			return await Task.FromResult<ClaimsIdentity>(null);
		}
	}
}
