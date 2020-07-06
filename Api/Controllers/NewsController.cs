using Domain;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class NewsController : SttControllerBase
    {
        private readonly IPrincipalProvider principalProvider;
        private readonly IDateTimeService dateTimeService;
        private readonly ILogger<NewsController> _logger;
        private readonly IMapper _mapper;

        public NewsController(IMapper mapper, IGenericService service, IPrincipalProvider principalProvider, IDateTimeService dateTimeService, ILogger<NewsController> logger) : base(service)
        {
            this.principalProvider = principalProvider;
            this.dateTimeService = dateTimeService;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Gets the 10 latest news items.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Policy = AuthPolicy.Reader)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<NewsItemModel>>> GetAllNews()
        {
            try
            {
                var list = await service.FindTakeAsync<NewsItem>(x => x.School.Code == SchoolCode,
                    m => m.OrderByDescending(x => x.DateTime),
                    noTrack: false,
                    take: 10,
                    x => x.School,
                    x => x.Author);

                return _mapper.Map<List<NewsItemModel>>(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        /// <summary>
        /// Creates a new NewsItem
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = AuthPolicy.Contributor)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<int>> PostNews(PostNewsItemModel model)
        {
            try
            {
                var newsItem = new NewsItem
                {
                    Body = model.Body,
                    Author = await service.FirstOrDefaultAsync<User>(x => x.Code == principalProvider.GetUserCode().Value),
                    School = await service.FirstOrDefaultAsync<School>(x => x.Code == SchoolCode),
                    Title = model.Title,
                    DateTime = dateTimeService.UtcNow(),
                    Code = Guid.NewGuid()
                };

                var rowsAffected = await service.CreateAsync(newsItem);

                return Created(string.Empty, model);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }
    }
}
