using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;
using System;
using System.Collections.Generic;
using Api.Auth;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Domain;
using Microsoft.Extensions.Logging;

namespace Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DashboardController : SttControllerBase
    {
        private readonly IDateTimeService dateTimeService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IGenericService service,
            IDateTimeService dateTimeService,
            ILogger<DashboardController> logger):base(service)
        {
            this.dateTimeService = dateTimeService;
            _logger = logger;
        }

        /// <summary>
        /// this endpoint is only used in the admin panel web app
        /// </summary>
        /// <returns></returns>
        [HttpGet("notifications")]
        [Authorize(Policy = AuthPolicy.Reader)]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<string>>> GetAllDashboardNotifications()
        {
            try
            {
                var newUsersFromToday = await service.CountAsync<User>(u => u.Created.Value.Date == this.dateTimeService.UtcNow().Date && u.School.Code == SchoolCode);
                var newStudentsFromToday = await service.CountAsync<User>(u => u.Created.Value.Date == this.dateTimeService.UtcNow().Date
                                                                                    && u.Category == UserCategory.Student
                                                                                    && u.School.Code == SchoolCode);

                return new List<string>
                {
                    $"{newUsersFromToday} new users today",
                    $"{newStudentsFromToday} new students today"
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }
    }
}