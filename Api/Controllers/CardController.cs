
using Api.Auth;
using AutoMapper;
using Common;
using Domain;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CardController : SttControllerBase
    {
        private readonly ICardService _cardService;
        private readonly ILogger<CardController> _logger;
        private readonly IMapper _mapper;

        public CardController(IGenericService service,
            ILogger<CardController> logger,
            ICardService cardService, IMapper mapper) : base(service)
        {
            _logger = logger;
            _mapper = mapper;
            _cardService = cardService;
        }

        /// <summary>
        /// Retrieves all the cards
        /// </summary>
        /// <remarks>
        /// STATUS:
        /// 1-active
        /// 2-disabled
        /// </remarks>
        /// <returns></returns>
        [HttpGet("")]
        [Authorize(Policy = AuthPolicy.Reader)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<CardModel>>> GetAllCards()
        {
            try
            {
                var list = await service.FindAsync<Card>(x => x.School.Code == SchoolCode, null, true);

                //todo: do not use mapper in lists
                return Ok(_mapper.Map<List<CardModel>>(list));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        /// <summary>
        /// Retrieves card total count
        /// </summary>
        /// <returns></returns>
        [HttpGet("count")]
        [Authorize(Policy = AuthPolicy.Reader)]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> CountCards()
        {
            try
            {
                return Ok(await service.CountAsync<Card>(x => x.School.Code == SchoolCode));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }


        /// <summary>
        /// Retrieves the amount of cards attached to students
        /// </summary>
        /// <returns></returns>
        [HttpGet("count-attached")]
        [Authorize(Policy = AuthPolicy.Reader)]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> CountAttachedCards()
        {
            try
            {
                var count = await service.CountAsync<Card>(x => x.User != null && x.School.Code == SchoolCode);

                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        /// <summary>
        /// bulk inserts cards. Post a .txt file with a card code per line.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("bulk-insert")]
        [Authorize(Policy = AuthPolicy.Contributor)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PostBulkInsertCard(IFormFile file)
        {
            try
            {
                const double oneMb = 1048576;

                var validExtensions = new List<string> { "txt", };

                var validContentTypes = new List<string> { "text/plain" };

                var fileUploadResult = new FileUploadResult();

                //validate content type
                if (validContentTypes.All(e => file.ContentType != e))
                {
                    fileUploadResult.Error = "File is not a text file.";
                    fileUploadResult.Success = false;
                    return BadRequest(fileUploadResult);
                }
                // validate extensions
                if (!validExtensions.Any(e => file.FileName.EndsWith(e)))
                {
                    fileUploadResult.Error = "File extension is invalid.";
                    fileUploadResult.Success = false;
                    return BadRequest(fileUploadResult);
                }
                // validate length;
                if (file.Length > oneMb)
                {
                    fileUploadResult.Error = "File should have 1MB max";
                    fileUploadResult.Success = false;
                    return BadRequest(fileUploadResult);
                }

                string result;

                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    result = reader.ReadToEnd();
                }

                result = result.Trim();

                var list = result.Split(Environment.NewLine).ToList();

                list = list
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .Distinct()
                    .ToList();

                var results = new List<PostCardResult>();


                foreach (var item in list)
                {
                    var card = new PostCardModel
                    {
                        CardCode = item,
                        Name = string.Empty
                    };
                    results.Add(await _cardService.PostCardAsync(card));
                }

                //todo associate cards with school
                var school = await service.FindAsync<School>(x => x.Code == SchoolCode);


                var failed = results.Where(x => x.Succeed == false);

                return Ok(failed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }


        [HttpGet("{cardCode}")]
        [Authorize(Policy = AuthPolicy.Reader)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CardModel>> GetCard(string cardCode)
        {
            try
            {
                //todo:
                //admin can get any card id.
                //parents can only get his children cards.

                var card = await service.FirstOrDefaultAsync<Card>(x => x.CardCode == cardCode && x.School.Code == SchoolCode, null, true);

                if (card == null)
                {
                    return NotFound(ErrorConstants.CardNotFound);
                }

                return Ok(_mapper.Map<CardModel>(card));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        /// <summary>
        /// Searches cards by card code.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost("search")]
        [Authorize(Policy = AuthPolicy.Reader)]
        [ProducesResponseType(typeof(IEnumerable<CardModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<CardModel>>> Search(PostSearchCardFilter filter)
        {

            //todo: only admins can call this method.

            try
            {
                if (string.IsNullOrWhiteSpace(filter.Code))
                    return new List<CardModel>();

                var result = await service.FindAsync<Card>(c => c.CardCode.Contains(filter.Code)
                                                                && c.User != null == filter.UserAttached
                                                                && c.School.Code == SchoolCode, null, true);

                //todo: do not use auto mapper in lists;
                return Ok(_mapper.Map<List<CardModel>>(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        /// <summary>
        /// Creates a new card
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = AuthPolicy.Contributor)]
        [ProducesResponseType(typeof(CardModel), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> PostCard(PostCardModel model)
        {
            try
            {
                var newCard = new Card()
                {
                    CardCode = model.CardCode,
                    Name = model.Name,
                    School = await service.FirstOrDefaultAsync<School>(x => x.Code == SchoolCode)
                };

                var rowsAffected = await service.CreateAsync(newCard);

                if (rowsAffected != 1)
                {
                    _logger.LogError("post card operation returned {0} rows affected", rowsAffected);
                }

                return Created($"/card/{newCard.CardCode}", newCard);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        /// <summary>
        /// Updates card name and status
        /// </summary>
        /// <remarks>
        /// status 1:active
        /// status 2:disabled
        /// </remarks>
        /// <param name="cardCode"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("{cardCode}")]
        [Authorize(Policy = AuthPolicy.Contributor)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<int>> PutCard(string cardCode, PutCardModel model)
        {
            try
            {
                //todo: a parent can only update his children cards.

                var card = await service.FirstOrDefaultAsync<Card>(x => x.CardCode == cardCode && x.School.Code == SchoolCode);

                if (card == null)
                {
                    return NotFound(ErrorConstants.CardNotFound);
                }

                //user can only update card name, not code
                card.Name = model.Name;
                card.RowVersion = model.RowVersion;

                var rowsAffected = await service.UpdateAsync(card);
                return Ok(rowsAffected);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ErrorConstants.ConcurrencyMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        /// <summary>
        /// Deletes a card
        /// </summary>
        /// <param name="cardCode"></param>
        /// <returns></returns>
        [HttpDelete("{cardCode}")]
        [Authorize(Policy = AuthPolicy.Contributor)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<int>> DeleteCard(string cardCode)
        {
            try
            {
                //todo: only admins can delete cards.

                var card = await service.FirstOrDefaultAsync<Card>(x => x.CardCode == cardCode && x.School.Code == SchoolCode);

                if (card == null)
                {
                    return NotFound(ErrorConstants.CardNotFound);
                }

                var rowsAffected = await service.DeleteAsync(card);

                return Ok(rowsAffected);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }
    }
}
