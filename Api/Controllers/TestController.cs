using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Auth;
using Bogus;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace _1.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {

        private readonly IGenericService _service;
        private readonly SttUserManager _userManager;
        readonly Random rand = new Random();
        public TestController(IGenericService service, SttUserManager userManager)
        {
            _service = service;
            _userManager = userManager;
        }

        private List<Room> GetDefaultGradeRooms(School school)
        {
            var list = new List<Room>();
            var grades = Enum.GetValues(typeof(SchoolGrade));

            foreach (var grade in grades)
            {
                var room = new Room()
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


        [Obsolete]
        [HttpPost("generate-test-data")]
        [Authorize(Policy = AuthPolicy.Test)]
        public async Task<ActionResult> PostTest()
        {
            var cards = new Faker<Card>()
                 .RuleFor(u => u.CardCode, (f, u) => TestGenerator.CardName())
                 .RuleFor(u => u.Name, (f, u) => TestGenerator.CardName())
                 .RuleFor(u => u.Created, f => DateTime.UtcNow)
                 .RuleFor(u => u.CreatedBy, f => Guid.Empty)
                 .Generate(100);

            var devices = new Faker<Device>()
                 .RuleFor(u => u.DeviceCode, (f, u) => TestGenerator.DeviceCode())
                 .RuleFor(u => u.Name, (f, u) => TestGenerator.DeviceName())
                 .RuleFor(u => u.Created, f => DateTime.UtcNow)
                 .RuleFor(u => u.CreatedBy, f => Guid.Empty)
                 .Generate(100);

            var buses = new Faker<Bus>()
                 .RuleFor(u => u.Name, (f, u) => TestGenerator.BusName())
                 .RuleFor(u => u.Code, (f, u) => Guid.NewGuid())
                 .RuleFor(u => u.Created, f => DateTime.UtcNow)
                 .RuleFor(u => u.CreatedBy, f => Guid.Empty)
                 .Generate(12);

            var schools = new Faker<School>()
                 .RuleFor(u => u.Name, (f, u) => TestGenerator.SchoolName())
                 .RuleFor(u => u.Code, (f, u) => Guid.NewGuid())
                 .RuleFor(u => u.Phone, (f, u) => f.Phone.PhoneNumber())
                 .RuleFor(u => u.Address, (f, u) => f.Address.FullAddress())
                 .RuleFor(u => u.Created, f => DateTime.UtcNow)
                 .RuleFor(u => u.CreatedBy, f => Guid.Empty)
                 .Generate(3);

            var users = new Faker<User>()
                .RuleFor(u => u.FirstName, (f, u) => f.Name.FirstName())
                .RuleFor(u => u.LastName, (f, u) => f.Name.LastName())
                .RuleFor(u => u.PhoneNumber, (f, u) => f.Phone.PhoneNumber())
                .RuleFor(u => u.PhotoUrl, f => f.Image.People())
                .RuleFor(u => u.UserName, (f, u) => f.Internet.UserName(u.FirstName, u.LastName).ToLowerInvariant())
                .RuleFor(u => u.Email, (f, u) => f.Internet.ExampleEmail(u.FirstName, u.LastName).ToLowerInvariant())
                .RuleFor(u => u.Code, f => Guid.NewGuid())
                .RuleFor(u => u.Created, f => DateTime.UtcNow)
                .RuleFor(u => u.CreatedBy, f => Guid.Empty)
                .RuleFor(u => u.Category, f => f.PickRandom<UserCategory>())
                .Generate(150);

            var admin = new User();
            admin.Category = UserCategory.Admin;
            admin.Email = "stt@stt.com";
            admin.UserName = "stt@stt.com";
            admin.FirstName = "Admin";
            admin.LastName = "Admin";
            admin.Code = Guid.NewGuid();
            admin.Created = DateTime.UtcNow;
            admin.CreatedBy = Guid.Empty;
            admin.PhoneNumber = "654654654";

            users.Add(admin);

            var govState = new User();
            admin.Category = UserCategory.GovState;
            admin.Email = "state@stt.com";
            admin.UserName = "state@stt.com";
            admin.FirstName = "State";
            admin.LastName = "Admin";
            admin.Code = Guid.NewGuid();
            admin.Created = DateTime.UtcNow;
            admin.CreatedBy = Guid.Empty;
            admin.PhoneNumber = "654654654";

            users.Add(govState);

            var c = _service.CreateRange(cards);
            var d = _service.CreateRange(devices);
            var b = _service.CreateRange(buses);
            var s = _service.CreateRange(schools);

            foreach (var school in schools)
            {
                var rooms = GetDefaultGradeRooms(school);
                _service.CreateRange(rooms);
            }

            var password = "123456";

            foreach (var u in users)
            {
                await _userManager.CreateAsync(u, password);
            }


            //relate bus to school and devices;
            foreach (var bus in buses)
            {
                bus.School = GetRandomSchool();
                bus.Device = GetRandomDevice();
                await _service.UpdateAsync(bus);
            }

            //relate user to school and cards
            foreach (var user in users)
            {
                user.School = GetRandomSchool();
                if (user.Category == UserCategory.Student)
                {
                    var card = GetRandomCard();
                    if (card != null)
                    {
                        user.Cards.Add(card);
                    }

                    user.Room = user.School.Rooms[0];
                }

                await _userManager.UpdateAsync(user);

            }

            School GetRandomSchool()
            {
                var index = rand.Next(schools.Count);
                return schools[index];
            }

            Card GetRandomCard()
            {
                var returnNull = rand.Next(3);
                if (returnNull != 1)
                {
                    var index = rand.Next(schools.Count);
                    return cards[index];
                }
                return null;
            }

            Device GetRandomDevice()
            {
                var returnNull = rand.Next(3);
                if (returnNull != 1)
                {
                    var index = rand.Next(devices.Count);
                    return devices[index];
                }
                return null;
            }

            return Ok();

        }
    }

    internal static class TestGenerator
    {
        private static Random random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public static string DeviceCode()
        {
            var first = new string(Enumerable.Repeat(chars, 4).Select(s => s[random.Next(s.Length)]).ToArray());
            return $"{first}#0";
        }
        public static string DeviceName()
        {
            var first = new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
            return $"TESTDEVICE-{first}";
        }
        public static string CardName()
        {
            var first = new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
            return $"TESTCARD-{first}";
        }

        public static string BusName()
        {
            var first = new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
            return $"TESTBUS-{first}";
        }
        public static string SchoolName()
        {
            var first = new string(Enumerable.Repeat(chars, 3).Select(s => s[random.Next(s.Length)]).ToArray());
            return $"TESTSCHOOL-{first}";
        }
    }
}