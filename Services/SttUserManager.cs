using Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services
{
    public class SttUserManager : UserManager<User>
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly IPrincipalProvider _principalProvider;

        public SttUserManager(IUserStore<User> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<User> passwordHasher,
            IEnumerable<IUserValidator<User>> userValidators,
            IEnumerable<IPasswordValidator<User>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<SttUserManager> logger,
            IDateTimeService dateTimeService,
            IPrincipalProvider principalProvider) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            _dateTimeService = dateTimeService;
            _principalProvider = principalProvider;
        }

        public override Task<IdentityResult> CreateAsync(User user, string password)
        {
            user.Created = _dateTimeService.UtcNow();
            user.Code = Guid.NewGuid();
            user.CreatedBy = _principalProvider.GetUserCode();

            return base.CreateAsync(user, password);
        }

        public override Task<IdentityResult> UpdateAsync(User user)
        {
            user.Updated = _dateTimeService.UtcNow();
            user.UpdatedBy = _principalProvider.GetUserCode();
            return base.UpdateAsync(user);
        }
    }
}
