using Api.Auth;
using AutoMapper;
using Common;
using Domain;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : SttControllerBase
    {
        private readonly SttUserManager _userManager;
        private readonly IImageService _imageService;
        private readonly IDateTimeService _dateTimeService;
        private readonly ILocationService _locationService;
        private readonly IPrincipalProvider _principalProvider;
        private readonly ILogger<UserController> _logger;
        private readonly IMapper _mapper;

        public UserController(SttUserManager userManager,
            IGenericService service,
            IMapper mapper,
            IImageService imageService,
            ILogger<UserController> logger,
            IDateTimeService dateTimeService,
            ILocationService locationService,
            IPrincipalProvider principalProvider) : base(service)
        {
            _userManager = userManager;
            _logger = logger;
            _mapper = mapper;
            _imageService = imageService;
            _dateTimeService = dateTimeService;
            _locationService = locationService;
            _principalProvider = principalProvider;
        }

        /// <summary>
        /// Creates a new parent
        /// </summary>
        /// <remarks>
        /// USER CATEGORIES:
        ///  Director = 1, Driver = 2, Supervisor = 3, Student = 4, Parent = 5, Teacher = 6
        ///  -------------------
        /// SCHOOL GRADES:
        ///  PreKinder = 1, Kinder = 2, Elementary = 3, Middle = 4, High = 5
        /// </remarks>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("parent")]
        [Authorize(Policy = AuthPolicy.Contributor)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<UserModel>> PostParent(PostUserWithPasswordModel model)
        {
            try
            {
                //todo: check for special characters in the model. only allow letters

                var newParent = await GetNewUserAsync(UserCategory.Parent, model);

                var identityResult = await _userManager.CreateAsync(newParent, model.Password);

                if (!identityResult.Succeeded)
                    return BadRequest(identityResult.Errors);

                var url = $"/user/{newParent.Code}";
                return Created(url, _mapper.Map<UserModel>(newParent));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }

        }

        /// <summary>
        /// Creates a new child (Student) for a parent
        /// </summary>
        /// <param name="parentUserCode"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("{parentUserCode}/student")]
        [Authorize(Policy = AuthPolicy.Contributor)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserModel>> PostStudent(Guid parentUserCode, PostStudentModel model)
        {
            try
            {
                //todo:
                //validate the current user is  OWNER  or 
                //validate the current user is the parent sent by param.
                //a parent should not post another parent's student.
                //todo: check for special characters in the model. only allow letters

                var parent = await Get(parentUserCode);

                if (parent.Category != UserCategory.Parent)
                {
                    return BadRequest(ErrorConstants.UserIsNotAParent);
                }

                var card = await service.FirstOrDefaultAsync<Card>(c => c.CardCode == model.CardCode);

                if (card == null)
                {
                    return NotFound(ErrorConstants.CardNotFound);
                }

                if (card.User != null)
                {
                    return BadRequest("This card is already assigned to other student");
                }

                var school = await service.FirstOrDefaultAsync<School>(x => x.Code == model.SchoolCode);

                if (school == null)
                {
                    return NotFound(ErrorConstants.SchoolNotFound);
                }

                var gradeRoom = await service.FirstOrDefaultAsync<Room>(x => x.Code == model.GradeRoomCode);

                if (gradeRoom == null)
                {
                    return NotFound(ErrorConstants.GradeRoomNotFound);
                }

                var newStudent = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Category = UserCategory.Student,
                    StudentId = model.StudentId,
                    Room = gradeRoom,
                };

                newStudent.Cards.Add(card);

                newStudent.School = school;

                // students are not allowed to sign in, but a password is required to create the user entitity in db.
                var identityResult = await _userManager.CreateAsync(newStudent, Guid.NewGuid().ToString());

                if (!identityResult.Succeeded)
                    return BadRequest(identityResult.Errors);

                parent.Students.Add(newStudent);

                //update parent
                await _userManager.UpdateAsync(parent);

                //create student card history item
                var studentCardHistoryItem = new StudentCardHistoryItem
                {
                    CardId = card.Id,
                    FromDate = _dateTimeService.UtcNow(),
                    UserId = newStudent.Id
                };

                await service.CreateAsync(studentCardHistoryItem);

                var url = $"/user/{newStudent.Code}";

                return Created(url, _mapper.Map<UserModel>(newStudent));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        /// <summary>
        /// Creates a new Admin user
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("admin")]
        [Authorize(Policy = AuthPolicy.Contributor)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserModel>> PostAdminUser(PostUserWithPasswordModel model)
        {
            try
            {
                var newAdmin = await GetNewUserAsync(UserCategory.Admin, model);

                var identityResult = await _userManager.CreateAsync(newAdmin, model.Password);

                if (!identityResult.Succeeded)
                    return BadRequest(identityResult.Errors);

                var url = $"/user/{newAdmin.Code}";
                return Created(url, _mapper.Map<UserModel>(newAdmin));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        /// <summary>
        /// Creates a new Gov State user.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("gov-state")]
        [Authorize(Policy = AuthPolicy.Contributor)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserModel>> PostGovStateUser(PostUserWithPasswordModel model)
        {
            try
            {
                var newGovStateUser = await GetNewUserAsync(UserCategory.GovState, model);

                var identityResult = await _userManager.CreateAsync(newGovStateUser, model.Password);

                if (!identityResult.Succeeded)
                    return BadRequest(identityResult.Errors);

                var url = $"/user/{newGovStateUser.Code}";
                return Created(url, _mapper.Map<UserModel>(newGovStateUser));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        /// <summary>
        /// Detaches a student from the parent
        /// </summary>
        /// <param name="parentUserCode"></param>
        /// <param name="studentUserCode"></param>
        /// <returns>True if it was detached, false otherwise</returns>
        [HttpDelete("{parentUserCode}/student/{studentUserCode}")]
        [Authorize(Policy = AuthPolicy.Contributor)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<bool>> DetachStudent(Guid parentUserCode, Guid studentUserCode)
        {
            try
            {
                //todo: only admins can call this method

                var parent = await Get(parentUserCode);

                var student = await Get(studentUserCode);

                if (parent.Students.Empty())
                {
                    return BadRequest("Parent does not have any student attached.");
                }

                var wasRemoved = parent.Students.Remove(student);
                if (!wasRemoved)
                    return false;

                await _userManager.UpdateAsync(parent);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        /// <summary>
        /// Retrieves all users by page
        /// </summary>
        /// <returns></returns>
        [HttpGet("page/{pageNumber}")]
        [Authorize(Policy = AuthPolicy.Reader)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PagedResult<UserModel>>> GetAllUsers(int pageNumber = 1)
        {
            try
            {
                if (pageNumber < 1)
                {
                    return BadRequest("page number must be greater than 0");
                }
                var page = await service.GetAllPagedAsync<User, UserModel>(pageNumber: pageNumber);
                return Ok(page);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        /// <summary>
        /// Retrieves total user count
        /// </summary>
        /// <returns></returns>
        [HttpGet("count")]
        [Authorize(Policy = AuthPolicy.Reader)]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> CountUsers()
        {
            try
            {
                //todo: only admins can call this method
                return Ok(await service.CountAsync<User>(x => x.School.Code == SchoolCode));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        /// <summary>
        /// Retrieves a user
        /// </summary>
        /// <param name="userCode"></param>
        /// <returns></returns>
        [HttpGet("{userCode}")]
        [Authorize(Policy = AuthPolicy.Reader)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserModel>> GetUser(Guid userCode)
        {
            try
            {
                var user = await Get(userCode);

                return _mapper.Map<UserModel>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        /// <summary>
        /// Retrieves children (Students) for a parent
        /// </summary>
        /// <param name="parentUserCode"></param>
        /// <returns></returns>
        [HttpGet("{parentUserCode}/student")]
        [Authorize(Policy = AuthPolicy.Reader)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<UserModel>>> GetStudent(Guid parentUserCode)
        {
            try
            {
                //todo: only admins can call this method
                //todo: parents can only get his children

                var parent = await Get(parentUserCode);

                return _mapper.Map<List<UserModel>>(parent.Students);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        /// <summary>
        /// Retrieves card history for a student
        /// </summary>
        /// <param name="studentUserCode"></param>
        /// <returns></returns>
        [HttpGet("{studentUserCode}/card-history")]
        [Authorize(Policy = AuthPolicy.Reader)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<StudentCardHistoryItem>>> GetStudentCards(Guid studentUserCode)
        {
            try
            {
                var student = await Get(studentUserCode);

                if (student.Category != UserCategory.Student)
                {
                    return BadRequest(ErrorConstants.UserIsNotAStudent);
                }

                var list = await service.FindAsync<StudentCardHistoryItem>(x => x.UserId == student.Id);

                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        /// <summary>
        /// Attaches an existing card to an existing student
        /// </summary>
        /// <param name="studentUserCode"></param>
        /// <param name="cardCode"></param>
        /// <returns></returns>
        [HttpPost("{studentUserCode}/card/{cardCode}")]
        [Authorize(Policy = AuthPolicy.Contributor)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> AttachCardToStudent(Guid studentUserCode, string cardCode)
        {
            try
            {
                var student = await Get(studentUserCode);

                if (student.Category != UserCategory.Student)
                {
                    return BadRequest(ErrorConstants.UserIsNotAStudent);
                }

                var cards = await service.FindAsync<Card>(c => c.CardCode == cardCode);

                var card = cards.FirstOrDefault();

                if (card == null)
                {
                    return NotFound(ErrorConstants.CardNotFound);
                }

                var historyItems = await service.FindAsync<StudentCardHistoryItem>(x => x.UserId == student.Id && x.UntilDate == null);

                var latestHistoryItem = historyItems.FirstOrDefault();

                if (latestHistoryItem == null)
                {
                    throw new Exception("There is no history item");
                }

                latestHistoryItem.UntilDate = _dateTimeService.UtcNow();

                await service.UpdateAsync(latestHistoryItem);

                var newHistoryItem = new StudentCardHistoryItem
                {
                    UserId = student.Id,
                    CardId = card.Id,
                    FromDate = _dateTimeService.UtcNow()
                };

                await service.CreateAsync(newHistoryItem);

                student.Cards.Add(card);

                await _userManager.UpdateAsync(student);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }


        }

        /// <summary>
        /// Retrieves Student active card
        /// </summary>
        /// <param name="studentUserCode"></param>
        /// <returns></returns>
        [HttpGet("{studentUserCode}/card")]
        [Authorize(Policy = AuthPolicy.Reader)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<CardModel>>> GetActiveStudentCard(Guid studentUserCode)
        {
            try
            {
                var student = await Get(studentUserCode);

                if (student.Category != UserCategory.Student)
                {
                    return BadRequest(ErrorConstants.UserIsNotAStudent);
                }

                if (student.Cards.Empty())
                {
                    return NotFound(ErrorConstants.StudentDoesNotHaveACard);
                }

                //todo: do not use auto mapper for lists;
                return _mapper.Map<List<CardModel>>(student.Cards);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }

        }

        /// <summary>
        /// Deletes a user
        /// </summary>
        /// <param name="userCode"></param>
        /// <returns></returns>
        [HttpDelete("{userCode}")]
        [Authorize(Policy = AuthPolicy.Contributor)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(Guid userCode)
        {
            try
            {
                //todo: only admins can call this method

                var user = await Get(userCode);

                if (user.Category == UserCategory.Parent)
                {
                    if (user.Students.Any())
                    {
                        return BadRequest("This User has Students attached. In order to delete this Parent, you have to detach the students from the parent first.");
                    }
                }

                if (user.Cards.Any())
                {
                    user.Cards = null;
                    await _userManager.UpdateAsync(user);
                }

                var result = await _userManager.DeleteAsync(user);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        /// <summary>
        /// RETRIEVES THE STUDENT LOCATION
        /// </summary>
        /// <param name="studentUserCode"></param>
        /// <returns></returns>
        [HttpGet("{studentUserCode}/location")]
        [Authorize(Policy = AuthPolicy.Reader)]
        [ProducesResponseType(typeof(LocationModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<LocationModel>> GetLocation(Guid studentUserCode)
        {
            // parents can only get the location of his own children. not others.
            var userCode = _principalProvider.GetUserCode();

            if (!userCode.HasValue)
            {
                return Unauthorized("Authorization is required");
            }

            var currentUser = await service.FirstOrDefaultAsync<User>(x => x.Code == userCode, null, true, x => x.Students);

            switch (currentUser.Category)
            {

                case UserCategory.Parent:
                    {
                        var students = currentUser.Students.Where(s => s.Code == studentUserCode).ToList();
                        if (students.Empty())
                        {
                            return BadRequest("Parents can only track their own children");
                        }

                        break;
                    }

                case UserCategory.Admin:
                    {
                        //do not validate anything
                        break;
                    }
                default:
                    return BadRequest("Only Admins and Parents cant get locations");
            }


            var user = await Get(studentUserCode);

            // validate
            if (user.Category != UserCategory.Student)
            {
                return BadRequest(ErrorConstants.UserIsNotAStudent);
            }

            // validate
            if (user.Cards.Empty())
            {
                return BadRequest(ErrorConstants.StudentDoesNotHaveACard);
            }

            //todo: try the second, third card if the first one fails.
            var cardCode = user.Cards.First().CardCode;

            try
            {
                var location = _locationService.GetMostRecentLocationFromHistoricLog(cardCode);

                location.Status = string.Format(location.Status, user.FirstName);

                return location;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }

        }

        /// <summary>
        /// Updates a user
        /// </summary>
        /// <param name="userCode"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("{userCode}")]
        [Authorize(Policy = AuthPolicy.Contributor)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IdentityResult>> PutUser(Guid userCode, PutUserModel model)
        {
            try
            //todo: check for special characters in the model. only allow letters
            {
                var user = await Get(userCode);

                //todo: use fluent validator
                if (!string.IsNullOrWhiteSpace(model.FirstName))
                {
                    user.FirstName = model.FirstName;
                }
                if (!string.IsNullOrWhiteSpace(model.LastName))
                {
                    user.LastName = model.LastName;
                }
                if (!string.IsNullOrWhiteSpace(model.Email))
                {
                    user.Email = model.Email;
                }
                if (!string.IsNullOrWhiteSpace(model.PhotoUrl))
                {
                    user.PhotoUrl = model.PhotoUrl;
                }
                if (!string.IsNullOrWhiteSpace(model.Phone))
                {
                    user.PhoneNumber = model.Phone;
                }

                if (model.GradeRoomCode.HasValue)
                {
                    var room = await service.FirstOrDefaultAsync<Room>(x => x.Code == model.GradeRoomCode, null, false, x => x.School);
                    if (room == null)
                    {
                        return NotFound(ErrorConstants.GradeRoomNotFound);
                    }
                    if (room.School.Code != user.School.Code)
                    {
                        return BadRequest("invalid grade room");
                    }
                    user.Room = room;
                }

                if (model.SchoolCode.HasValue)
                {
                    var school = await service.FirstOrDefaultAsync<School>(x => x.Code == model.SchoolCode);

                    if (school == null)
                    {
                        return NotFound(ErrorConstants.SchoolNotFound);
                    }
                    user.School = school;
                }

                if (!string.IsNullOrWhiteSpace(model.CardCode))
                {

                    var card = await service.FirstOrDefaultAsync<Card>(x => x.CardCode == model.CardCode);

                    if (card == null)
                    {
                        return NotFound(ErrorConstants.CardNotFound);
                    }

                    var newHistoryItem = new StudentCardHistoryItem
                    {
                        UserId = user.Id,
                        CardId = card.Id,
                        FromDate = _dateTimeService.UtcNow()
                    };

                    await service.CreateAsync(newHistoryItem);

                    user.Cards.Add(card);

                }

                var result = await _userManager.UpdateAsync(user);

                return Ok(result);
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
        /// Adds a profile image to a user. (5MB MAX)
        /// </summary>
        /// <param name="userCode"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("{userCode}/image")]
        [Authorize(Policy = AuthPolicy.Contributor)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PostUserImage(Guid userCode, IFormFile file)
        {
            try
            {
                //todo: only admins can call this method
                //if not admin, you can only upload an image for yourself, or your children.

                var user = await Get(userCode);

                var result = await _imageService.UploadAsync(user.Code.ToString(), file);

                if (!result.Success)
                    return BadRequest(result.Error);

                user.PhotoUrl = result.Location;

                var identityResult = await _userManager.UpdateAsync(user);

                if (identityResult.Succeeded)
                {
                    return Ok(result);
                }

                return BadRequest(identityResult.Errors.Select(x => x.Description));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        private async Task<User> Get(Guid userCode)
        {
            var user = await service.FirstOrDefaultAsync<User>(x => x.Code == userCode);
            if (user == null)
            {
                throw new Exception($"User with code {userCode} was not found");
            }
            return user;
        }

    }
}