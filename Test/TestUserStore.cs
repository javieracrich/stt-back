using Domain;
using Microsoft.AspNetCore.Identity;
using Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
	public class TestUserStore : IUserStore<User>, IUserPasswordStore<User>, IUserSecurityStampStore<User>
	{

		IUserService _service;
		public TestUserStore(IUserService service)
		{
			_service = service;
		}

		public Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
		{
			var result = IdentityResult.Success;
			_service.CreateAsync(user);
			return Task.FromResult(result);
		}

		public Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			//nothing
		}

		public Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken)
		{
			return Task.FromResult(_service.GetAll().FirstOrDefault(x => x.Id == userId));
		}

		public Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
		{
			return Task.FromResult(_service.GetAll().FirstOrDefault(x => x.UserName == normalizedUserName));
		}

		public Task<string> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
		{
			return Task.FromResult(user.NormalizedUserName);
		}

		public Task<string> GetPasswordHashAsync(User user, CancellationToken cancellationToken)
		{
			return Task.FromResult(user.PasswordHash);
		}

		public Task<string> GetSecurityStampAsync(User user, CancellationToken cancellationToken)
		{
			return Task.FromResult(user.SecurityStamp);
		}

		public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
		{
			return Task.FromResult(user.Id);
		}

		public Task<string> GetUserNameAsync(User user, CancellationToken cancellationToken)
		{
			return Task.FromResult(user.UserName);
		}

		public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken)
		{
			return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
		}

		public Task SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken)
		{
			user.NormalizedUserName = normalizedName;
			return Task.FromResult(0);
		}

		public Task SetPasswordHashAsync(User user, string passwordHash, CancellationToken cancellationToken)
		{
			user.PasswordHash = passwordHash;
			return Task.FromResult(0);
		}

		public Task SetSecurityStampAsync(User user, string stamp, CancellationToken cancellationToken)
		{
			user.SecurityStamp = stamp;
			return Task.FromResult(0);
		}

		public Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken)
		{
			user.UserName = userName;
			return Task.FromResult(0);
		}

		public Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
		{
			var result = IdentityResult.Success;
			_service.UpdateAsync(user);
			return Task.FromResult(result);
		}
	}
}
