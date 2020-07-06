using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Auth;
using AutoMapper;
using Domain;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AlertController : SttControllerBase
    {
        private readonly ILogger<AlertController> _logger;
        private readonly IMapper mapper;
        private readonly IDateTimeService dateTimeService;
        private readonly IPrincipalProvider principalProvider;

        public AlertController(ILogger<AlertController> logger,
            IMapper mapper,
            IGenericService service,
            IDateTimeService dateTimeService,
            IPrincipalProvider principalProvider) : base(service)
        {
            _logger = logger;
            this.mapper = mapper;
            this.dateTimeService = dateTimeService;
            this.principalProvider = principalProvider;
        }

        /// <summary>
        /// Saves a new alert in the db and sends the push notification to selected users
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = AuthPolicy.Contributor)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PostAlert(PostAlertRequest request)
        {
            try
            {
                SendAlert(request);

                int[] categoriesId = Array.ConvertAll(request.UserCategories, value => (int)value);

                var newAlert = new Alert
                {
                    Message = request.Message,
                    UserCategories = string.Join(',', categoriesId),
                    AlertType = request.AlertType,
                    DateTime = this.dateTimeService.UtcNow(),
                    School = await this.service.FirstOrDefaultAsync<School>(x => x.Code == SchoolCode),
                    Author = await this.service.FirstOrDefaultAsync<User>(x => x.Code == this.principalProvider.GetUserCode())
                };

                return Ok(await this.service.CreateAsync(newAlert));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        /// <summary>
        /// Retrieves the last 10 alerts for the current school
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Policy = AuthPolicy.Reader)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<AlertModel>>> GetLatestAlerts()
        {
            var alerts = await this.service.FindTakeAsync<Alert>(x => x.School.Code == SchoolCode,
                                                                 m => m.OrderByDescending(x => x.DateTime), true, 10);

            return this.mapper.Map<List<AlertModel>>(alerts);
        }



        /// <summary>
        /// Retrieves alert count from the last 30 days;
        /// </summary>
        /// <returns></returns>
        [HttpGet("count")]
        [Authorize(Policy = AuthPolicy.Reader)]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> CountAlerts()
        {
            try
            {
                var lastMonth = this.dateTimeService.UtcNow().AddDays(-30);

                return Ok(await this.service.CountAsync<Alert>(x => x.School.Code == SchoolCode && x.DateTime >= lastMonth));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        private void SendAlert(PostAlertRequest alertRequest)
        {
            //todo send push notifications with alerts to the corresponding groups
            //verify the alert is only sent to the groups from the current school !!
            // throw new NotImplementedException();
        }
    }
}