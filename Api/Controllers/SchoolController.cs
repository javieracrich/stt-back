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
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SchoolController : SttControllerBase
    {
        private readonly IImageService _imageService;
        private readonly IDateTimeService _dateTimeService;
        private readonly IPrincipalProvider _principalProvider;
        private readonly SttUserManager _userManager;
        private readonly ILogger<SchoolController> _logger;
        private readonly IMapper _mapper;

        public SchoolController(IGenericService service,
            IImageService imageService,
            IDateTimeService dateTimeService,
            IPrincipalProvider principalProvider,
            IMapper mapper,
            ILogger<SchoolController> logger,
            SttUserManager userManager) : base(service)
        {
            _imageService = imageService;
            this._dateTimeService = dateTimeService;
            this._principalProvider = principalProvider;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
        }

        /// <summary>
        /// Retrieves all schools
        /// </summary>
        /// <returns></returns>
        [HttpGet("all")]
        [Authorize(Policy = AuthPolicy.Reader)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<SchoolModel>>> GetAllSchools()
        {
            try
            {
                //todo: only admins can call this method

                var list = await service.GetAllAsync<School>();

                var result = _mapper.Map<List<SchoolModel>>(list);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        /// <summary>
        /// Retrieves a school
        /// </summary>
        /// <returns></returns>
        [HttpGet("{schoolCode}")]
        [Authorize(Policy = AuthPolicy.Reader)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SchoolModel>> GetSchool(Guid schoolCode)
        {
            try
            {
                //	_logger.LogInformation(LoggingEvents.GetItem, "Getting item {ID}", id);
                var school = await service.FirstOrDefaultAsync<School>(x => x.Code == schoolCode, null, true);
                if (school == null)
                {
                    return NotFound(ErrorConstants.SchoolNotFound);
                }
                var model = _mapper.Map<SchoolModel>(school);
                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        /// <summary>
        /// Retrieves today's assistance count
        /// </summary>
        /// <returns></returns>
        [HttpGet("assistance")]
        [Authorize(Policy = AuthPolicy.Reader)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SchoolModel>> GetAssistance()
        {
            try
            {
                var count = await service.CountAsync<PushLog>(x =>
                 x.Date == _dateTimeService.UtcNow().Date &&
                 x.PushType == PushType.ENTERING_SCHOOL &&
                 x.SchoolCode == SchoolCode);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        /// <summary>
        /// Search students using a filter
        /// </summary>
        /// <remarks>
        /// LastStatusId:
        /// Leaving School = 1
        /// Entering School = 2
        /// -----------------------------
        /// Grades:
        /// PreKinder = 1
        /// Kinder = 2
        /// Elementary = 3
        /// Middle = 4
        /// High = 5,
        /// </remarks>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost("student")]
        [Authorize(Policy = AuthPolicy.Reader)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<UserModel>>> GetStudents(PostStudentFilter filter)
        {
            CheckPermissions(SchoolCode);

            var predicate = GetPredicate(SchoolCode, filter);

            var students = await service.FindAsync(predicate,
                              x => x.OrderBy(y => y.LastName),
                              true,
                              x => x.School,
                              x => x.Cards,
                              x => x.Room);

            var cards = new List<Card>();
            students.ForEach(x => cards.AddRange(x.Cards));
            var cardCodes = cards.Select(x => x.CardCode).ToList();
            //todo: merge both data sources into one to increase performance 
            var pushLogs = await service.FindAsync<PushLog>(x => cardCodes.Contains(x.CardCode) && x.SchoolCode == SchoolCode);
            var usersWithLastStatus = ToStudentModel(students, pushLogs);

            return Ok(usersWithLastStatus.Where(x => x.LastStatusId == (int)filter.LastStatusId));

        }

        /// <summary>
        /// Retrieves all students in a room
        /// </summary>
        /// <remarks>
        /// LastStatusId:
        /// Leaving School = 1
        /// Entering School = 2
        /// </remarks>
        /// <param name="roomCode"></param>
        /// <returns></returns>
        [HttpGet("student/{roomCode}")]
        [Authorize(Policy = AuthPolicy.Reader)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<UserModel>>> GetAllStudentsInRoom(Guid roomCode)
        {
            CheckPermissions(SchoolCode);

            var students = await service.FindAsync<User>(x => x.School.Code == SchoolCode &&
                                                               x.Room.Code == roomCode &&
                                                               x.Category == UserCategory.Student,
                                                               x => x.OrderBy(y => y.LastName),
                                                               true,
                                                               x => x.School,
                                                               x => x.Cards,
                                                               x => x.Room);

            var cards = new List<Card>();
            students.ForEach(x => cards.AddRange(x.Cards));
            var cardCodes = cards.Select(x => x.CardCode).ToList();

            var pushLogs = await service.FindAsync<PushLog>(x => cardCodes.Contains(x.CardCode) && x.SchoolCode == SchoolCode);
            var usersWithLastStatus = ToStudentModel(students, pushLogs);

            return Ok(usersWithLastStatus);

        }

        /// <summary>
        /// Search users (not students) using a filter
        /// </summary>
        /// <remarks>
        /// USER CATEGORIES:
        /// Director = 1, Driver = 2, Supervisor = 3, Student = 4, Parent = 5, Teacher = 6
        /// </remarks>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost("user")]
        [Authorize(Policy = AuthPolicy.Reader)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<UserModel>>> GetUsers(PostUserFilter filter)
        {
            try
            {
                CheckPermissions(SchoolCode);

                List<User> usrs;

                if (string.IsNullOrWhiteSpace(filter.Text))
                {
                    usrs = await service.FindAsync<User>(x =>
                                                  x.School.Code == SchoolCode &&
                                                  x.Category == filter.Category,
                                                  null,
                                                  true,
                                                  x => x.School
                                                  );
                }
                else
                {
                    usrs = await service.FindAsync<User>(x =>
                                 x.School.Code == SchoolCode &&
                                 (x.FirstName.Contains(filter.Text) || x.LastName.Contains(filter.Text)) &&
                                 x.Category == filter.Category,
                                 null,
                                 true,
                                 x => x.School);
                }

                return Ok(ToUserModel(usrs));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        /// <summary>
        /// Retrieves all available rooms
        /// </summary>
        /// <remarks>
        /// PreKinder = 1
        /// Kinder = 2
        /// Elementary = 3
        /// Middle = 4
        /// High = 5
        /// </remarks>
        /// <returns></returns>
        [HttpGet("room")]
        [Authorize(Policy = AuthPolicy.Reader)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<RoomModel>>> GetAllRooms()
        {
            try
            {
                var list = await service.FindAsync<Room>(x =>
                x.School.Code == SchoolCode, null, true);

                //todo: do not use mapper in lists
                return Ok(_mapper.Map<List<RoomModel>>(list));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        /// <summary>
        /// Retrieves a room by room code
        /// </summary>
        /// <param name="roomCode"></param>
        /// <returns></returns>
        [HttpGet("room/{roomCode}")]
        [Authorize(Policy = AuthPolicy.Reader)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<RoomModel>>> GetRoom(Guid roomCode)
        {
            try
            {
                var room = await service.FirstOrDefaultAsync<Room>(x =>
                x.School.Code == SchoolCode && x.Code == roomCode, null, true);

                //todo: do not use mapper in lists
                return Ok(_mapper.Map<RoomModel>(room));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        /// <summary>
        /// Creates a grade room for a specific school and grade
        /// </summary>
        /// <remarks>
        /// PreKinder = 1
        /// Kinder = 2
        /// Elementary = 3
        /// Middle = 4
        /// High = 5
        /// </remarks>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("room")]
        [Authorize(Policy = AuthPolicy.Contributor)]
        [ProducesResponseType(typeof(Room), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> PostSchoolRoom(PostGradeRoomModel model)
        {
            var room = new Room()
            {
                Name = model.Name,
                Grade = model.Grade,
                Code = Guid.NewGuid(),
                School = await service.FirstOrDefaultAsync<School>(x => x.Code == SchoolCode)
            };

            await service.CreateAsync(room);

            return Created($"school/room/{room.Code}", room);
        }

        /// <summary>
        /// Updates a room
        /// </summary>
        [HttpPut("room/{roomCode}")]
        [Authorize(Policy = AuthPolicy.Contributor)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<int>> PutSchoolRoom(Guid roomCode, PutGradeRoomModel model)
        {
            try
            {
                var room = await service.FirstOrDefaultAsync<Room>(x => x.Code == roomCode && x.School.Code == SchoolCode);

                if (room == null)
                {
                    return NotFound(ErrorConstants.GradeRoomNotFound);
                }

                room.Name = model.Name;
                room.RowVersion = model.RowVersion;

                var rowsAffected = await service.UpdateAsync(room);

                return Ok(rowsAffected);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        /// <summary>
        /// Creates a new school
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = AuthPolicy.Contributor)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SchoolModel>> PostSchool(PostSchoolModel model)
        {
            try
            {
                var newSchool = new School()
                {
                    Name = model.Name,
                    LogoUrl = model.LogoUrl,
                    Phone = model.Phone,
                    Address = model.Address,
                    Lat = model.Lat,
                    Lng = model.Lng,
                    Email = model.Email
                };

                newSchool.Code = Guid.NewGuid();
                await service.CreateAsync(newSchool);

                var rooms = GetDefaultRooms(newSchool);

                await service.CreateRangeAsync(rooms);

                var url = $"/school/{newSchool.Code}";
                return Created(url, _mapper.Map<SchoolModel>(newSchool));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        /// <summary>
        /// Updates a school
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize(Policy = AuthPolicy.Contributor)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<int>> PutSchool(PutSchoolModel model)
        {
            try
            {
                //todo: only admins can call this method

                var school = await service.FirstOrDefaultAsync<School>(x => x.Code == SchoolCode);

                if (school == null)
                {
                    return NotFound(ErrorConstants.SchoolNotFound);
                }

                school.Name = model.Name;
                school.LogoUrl = model.LogoUrl;
                school.Phone = model.Phone;
                school.Address = model.Address;
                school.Lat = model.Lat;
                school.Lng = model.Lng;
                school.Email = model.Email;
                school.RowVersion = model.RowVersion;

                if (model.DirectorCode.HasValue)
                {
                    var director = await service.FirstOrDefaultAsync<User>(x => x.Code == model.DirectorCode);

                    if (director == null)
                    {
                        return BadRequest(ErrorConstants.DirectorNotFound);
                    }

                    if (director.Category != UserCategory.SchoolDirector)
                    {
                        return BadRequest(ErrorConstants.UserIsNotADirector);
                    }
                    //only a single director is allowed
                    school.Users.RemoveAll(u => u.Category == UserCategory.SchoolDirector);

                    school.Users.Add(director);
                };

                var rowsAffected = await service.UpdateAsync(school);

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
        /// Adds a director to a school
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("director")]
        [Authorize(Policy = AuthPolicy.Contributor)]
        [ProducesResponseType(typeof(UserModel), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserModel>> PostDirector(PostUserWithPasswordModel model)
        {
            try
            {
                var newDirector = await GetNewUserAsync(UserCategory.SchoolDirector, model);

                return await CreateSchoolUser(newDirector, model.Password);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        /// <summary>
        /// Adds a driver to a school
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("driver")]
        [Authorize(Policy = AuthPolicy.Contributor)]
        [ProducesResponseType(typeof(UserModel), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserModel>> PostDriver(PostUserWithPasswordModel model)
        {
            try
            {
                var newDriver = await GetNewUserAsync(UserCategory.BusDriver, model);

                return await CreateSchoolUser(newDriver, model.Password);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        /// <summary>
        /// Adds a teacher to a school
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("teacher")]
        [Authorize(Policy = AuthPolicy.Contributor)]
        [ProducesResponseType(typeof(UserModel), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserModel>> PostTeacher(PostUserWithPasswordModel model)
        {
            try
            {
                var newTeacher = await GetNewUserAsync(UserCategory.Teacher, model);

                return await CreateSchoolUser(newTeacher, model.Password);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(ex.InnermostMsg());
            }
        }

        /// <summary>
        /// Adds a logo image to the school (5MB MAX)
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("image")]
        [Authorize(Policy = AuthPolicy.Contributor)]
        [ProducesResponseType(typeof(FileUploadResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(FileUploadResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FileUploadResult>> PostSchoolLogo(IFormFile file)
        {
            try
            {
                var result = await _imageService.UploadAsync(SchoolCode.ToString(), file);

                if (!result.Success)
                    return BadRequest(result);

                var school = await service.FirstOrDefaultAsync<School>(x => x.Code == SchoolCode);
                school.LogoUrl = result.Location;
                await service.UpdateAsync(school);
                return Ok(result);

            }
            catch (Exception ex)
            {
                var result = new FileUploadResult
                {
                    Error = ex.InnermostMsg(),
                    Success = false
                };
                _logger.LogError(ex, ex.InnermostMsg());
                return BadRequest(result);
            }
        }






        private async Task<ActionResult<UserModel>> CreateSchoolUser(User user, string password)
        {
            var identityResult = await _userManager.CreateAsync(user, password);

            if (!identityResult.Succeeded)
                return BadRequest(identityResult.Errors);

            var url = $"/user/{user.Code}";
            return Created(url, _mapper.Map<UserModel>(user));
        }

        private List<UserModel> ToStudentModel(IEnumerable<User> users, IEnumerable<PushLog> pushLogs)
        {
            var studentModelList = new List<UserModel>();
            foreach (var user in users)
            {
                var model = ToInnerModel(user);
                var cardCodes = model.Cards.Select(x => x.CardCode);
                var latestLog = pushLogs.Where(x => cardCodes.Contains(x.CardCode)).OrderByDescending(x => x.Date).FirstOrDefault();
                if (latestLog != null)
                {
                    model.LastStatusId = (int)latestLog.PushType;
                    model.LastStatusDateTime = latestLog.Date;
                }
                studentModelList.Add(model);
            }

            return studentModelList;
        }

        private static UserModel ToInnerModel(User user)
        {
            var model = new UserModel
            {
                Category = user.Category,
                Code = user.Code,
                Email = user.Email,
                FirstName = user.FirstName,
                Grade = user.Room?.Grade,
                LastName = user.LastName,
                Phone = user.PhoneNumber,
                PhotoUrl = user.PhotoUrl,
                StudentId = user.StudentId
            };

            foreach (var c in user.Cards)
            {
                model.Cards.Add(new CardModel { CardCode = c.CardCode, Name = c.Name });
            }

            if (user.School != null)
            {
                model.School = new BasicSchoolModel { Code = user.School.Code, Name = user.School.Name };
            }

            return model;
        }

        private List<UserModel> ToUserModel(IEnumerable<User> users)
        {
            var modelList = new List<UserModel>();

            users.ToList().ForEach(x => modelList.Add(ToInnerModel(x)));

            return modelList;
        }

        private IEnumerable<Room> GetDefaultRooms(School school)
        {
            var list = new List<Room>();
            var grades = Enum.GetValues(typeof(SchoolGrade));

            foreach (var grade in grades)
            {
                var room = new Room
                {
                    Name = "Room 1",
                    School = school,
                    Grade = (SchoolGrade)Enum.Parse(typeof(SchoolGrade), grade.ToString()),
                    Code = Guid.NewGuid(),
                };
                list.Add(room);
            }
            return list;
        }

        private void CheckPermissions(Guid schoolCode)
        {
            var currentUserCategory = _principalProvider.GetUserCategory();

            if (currentUserCategory.HasValue && currentUserCategory != UserCategory.GovState)
            {
                var currentSchoolCode = _principalProvider.GetSchoolCode();

                if (currentSchoolCode != schoolCode)
                {
                    throw new Exception("You don't have permission to query this school.");
                }
            }
        }

        private static Expression<Func<User, bool>> GetPredicate(Guid schoolCode, PostStudentFilter filter)
        {
            return x => x.School.Code == schoolCode
                     && x.Category == UserCategory.Student
                     && (filter.RoomCode.HasValue && x.Room.Code == filter.RoomCode
                            || !filter.RoomCode.HasValue)
                     && (!string.IsNullOrWhiteSpace(filter.Text) && (x.FirstName.Contains(filter.Text) || x.LastName.Contains(filter.Text))
                            || string.IsNullOrWhiteSpace(filter.Text));
        }
    }
}