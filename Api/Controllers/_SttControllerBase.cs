using Common;
using Domain;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Services;
using System;
using System.Threading.Tasks;

namespace Api.Controllers
{
    public abstract class SttControllerBase : ControllerBase
    {
        protected readonly IGenericService service;

        public SttControllerBase(IGenericService service)
        {
            this.service = service;
        }


        protected async Task<User> GetNewUserAsync(UserCategory category, PostUserModel model)
        {
            return new User
            {
                UserName = model.Email,
                Category = category,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.Phone,
                School = await this.service.FirstOrDefaultAsync<School>(x => x.Code == SchoolCode)
            };
        }

        protected async Task<User> GetNewUserAsync(UserCategory category, PostUserWithPasswordModel model)
        {
            return new User
            {
                UserName = model.Email,
                Category = category,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.Phone,
                School = await this.service.FirstOrDefaultAsync<School>(x => x.Code == SchoolCode)
            };
        }

        public Guid SchoolCode
        {
            get
            {
                var schoolCode = HttpContext.Request.Headers[Constants.SchoolCodeHeader];
                if (string.IsNullOrWhiteSpace(schoolCode))
                {
                    throw new Exception("schoolCode header is not present in request");
                }
                return Guid.Parse(schoolCode);
            }
        }
    }
}
