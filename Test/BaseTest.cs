using Api.Controllers;
using AutoMapper;
using Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Services;
using Services.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.AspNetCore.Http;
using System.Security.Principal;

namespace Test
{

    public class BaseTest
    {
        protected readonly SchoolController schoolController;
        protected readonly UserController userController;
        protected readonly CardController cardController;
        protected readonly DeviceController deviceController;
        protected readonly RFIDController rfidController;
        protected readonly IUserService userService;
        protected readonly IGenericService genericService;
        protected readonly IDateTimeService dateTimeService;

        protected BaseTest()
        {
            //todo: use object api instead. problems when testing
            try
            {
                Mapper.Reset();
                //static api
                Mapper.Initialize(cfg =>
                {
                    cfg.AddProfile(new AutomapperProfile());
                    cfg.AllowNullCollections = true;
                });

                Mapper.AssertConfigurationIsValid();
            }
            catch (Exception)
            {
                //do nothing

                //f** automapper throws ex if mapper is already initialized.
            }

            userService = GetUserService();
            dateTimeService = GetDateTimeService();
            genericService = GetGenericService(dateTimeService);

            schoolController = GetSchoolController();
            userController = GetUserController();
            cardController = GetCardController();
            deviceController = GetDeviceController();

            SetTestingPrincipal();

            void SetTestingPrincipal()
            {
                var principal = new ClaimsPrincipal();
                var identity = new ClaimsIdentity();
                var name = new Claim("name", "javier");
                identity.AddClaim(name);
                principal.AddIdentity(identity);

                Thread.CurrentPrincipal = principal;
            }
        }



        private static UnitOfWork _instance;
        private UnitOfWork UowSingletonInstance => _instance ?? (_instance = GetUow());

        private static SttContext GetSttContext(SqliteConnection connection)
        {
            var builder = new DbContextOptionsBuilder<SttContext>();

            var options = builder
               .UseSqlite(connection)
               .Options;

            var context = new SttContext(options);

            context.Database.OpenConnection();
            context.Database.EnsureCreated();
            return context;

        }

        protected Guid student1Id = Guid.NewGuid();
        protected Guid parentId = Guid.NewGuid();

        protected void SetCurrentPrincipal()
        {
            var claims = new List<Claim>
            {
                new Claim(Constants.JwtClaimIdentifiers.Id, parentId.ToString()),
                new Claim(Constants.JwtClaimIdentifiers.Category, UserCategory.Parent.ToString()),
                new Claim(Constants.JwtClaimIdentifiers.UserName, "javieracrich"),
            };

            var identity = new ClaimsIdentity(claims);

            Thread.CurrentPrincipal = new ClaimsPrincipal(identity);

        }

        private UnitOfWork GetUow()
        {
            var connection = new SqliteConnection("DataSource=:memory:");

            var context = GetSttContext(connection);

            var student1 = new User
            {
                FirstName = "Dante",
                LastName = "Acrich",
                Id = student1Id.ToString(),
                Category = UserCategory.Student
            };

            var parent1 = new User
            {
                FirstName = "Javier",
                LastName = "Acrich",
                Id = parentId.ToString(),
                Category = UserCategory.Parent
            };

            student1.Parent = parent1;

            var card1 = new Card
            {
                Name = "card1",
                CardCode = "code1",
            };

            var card2 = new Card
            {
                Name = "card2",
                CardCode = "code2",
            };

            var card3 = new Card
            {
                Name = "card3",
                CardCode = "code3",
            };

            var school1 = new School
            {
                Name = "school1",
                Address = "street 123",
                Phone = "123456789",
                Email = "school1@stt.com",
                SchoolCode = Guid.NewGuid(),
                Id = 1
            };


            var device1 = new Device
            {
                Name = "device1",
                DeviceCode = "AAAA#1",
                School = school1,
                Type = DeviceType.Outside,
                Id = 1,
            };

            var device2 = new Device
            {
                Name = "device2",
                DeviceCode = "AAAA#2",
                School = school1,
                Type = DeviceType.Inside,
                Id = 2
            };

            var insideDevice = new Device
            {
                Name = "device3",
                DeviceCode = "BBBB#3",
                School = school1,
                Type = DeviceType.Inside,
                Id = 3
            };

            var outsideDevice = new Device
            {
                Name = "device4",
                DeviceCode = "BBBB#4",
                School = school1,
                Type = DeviceType.Outside,
                Id = 4
            };

            insideDevice.RelatedDevice = outsideDevice;
            outsideDevice.RelatedDevice = insideDevice;

            var bus1 = new SchoolBus
            {
                Name = "bus1",
                School = school1,
                Device = device1,
                Id = 1
            };

            //cards
            Create(card1, connection);
            Create(card2, connection);
            Create(card3, connection);

            //users
            Create(parent1, connection);
            Create(student1, connection);

            //	student1.Cards.Add(card3);

            card3.User = student1;
            Update(card3, connection);

            //schools
            Create(school1, connection);

            //devices
            Create(device1, connection);
            Create(device2, connection);
            //related devices
            Create(insideDevice, connection);
            Create(outsideDevice, connection);

            Relate(insideDevice, outsideDevice, connection);
            Relate(outsideDevice, insideDevice, connection);

            //buses
            Create(bus1, connection);


            return new UnitOfWork(context);
        }



        private static void Update(Card card, SqliteConnection connection)
        {
            var updateSql = new SqliteCommand($"Update Cards SET" +

                $" UserId = \"{card.User.Id}\" " +
                " WHERE Id = 3", connection);

            try
            {
                updateSql.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        private static void Create(Card card, SqliteConnection connection)
        {

            SqliteCommand insertSQL;

            if (card.User != null)
            {
                insertSQL = new SqliteCommand($"INSERT INTO Cards (Name,CardCode,UserId) VALUES (\"{card.Name}\",\"{card.CardCode}\",\"{card.User.Id}\")", connection);
            }
            else
            {
                insertSQL = new SqliteCommand($"INSERT INTO Cards (Name,CardCode) VALUES (\"{card.Name}\",\"{card.CardCode}\")", connection);
            }


            try
            {
                insertSQL.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        private static void Create(School school, SqliteConnection connection)
        {
            var insertSQL = new SqliteCommand($"INSERT INTO Schools (Name,Phone,Address,Email,SchoolCode) VALUES (\"{school.Name}\",\"{school.Phone}\",\"{school.Address}\",\"{school.Email}\",\"{school.SchoolCode}\")", connection);

            try
            {
                insertSQL.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private static void Relate(Device device1, Device device2, SqliteConnection connection)
        {
            var insertSQL = new SqliteCommand($"UPDATE Devices SET RelatedDeviceId = {device2.RelatedDevice.Id} WHERE Id = {device1.Id}", connection);

            try
            {
                insertSQL.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private static void Create(Device device, SqliteConnection connection)
        {
            var insertSQL = new SqliteCommand($"INSERT INTO Devices (Name,DeviceCode,Type,SchoolId) VALUES (\"{device.Name}\",\"{device.DeviceCode}\",\"{(int)device.Type}\",\"{device.School.Id}\")", connection);

            try
            {
                insertSQL.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        private static void Create(SchoolBus bus, SqliteConnection connection)
        {
            var insertSQL = new SqliteCommand($"INSERT INTO SchoolBuses (Name,SchoolId,DeviceId) VALUES (\"{bus.Name}\",\"{bus.School.Id}\",\"{bus.Device.Id}\")", connection);

            try
            {
                insertSQL.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private static void Create(User user, SqliteConnection connection)
        {

            string sql;
            if (user.Parent != null)
            {
                sql = "INSERT INTO AspNetUsers (AccessFailedCount,Id,EmailConfirmed,LockoutEnabled,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,FirstName,LastName,Category,ParentId)" +
               $" VALUES (\"{user.AccessFailedCount}\",\"{user.Id}\",\"{user.EmailConfirmed}\",\"{user.LockoutEnabled}\",\"{user.PhoneNumber}\",\"{user.PhoneNumberConfirmed}\",\"{user.TwoFactorEnabled}\",\"{user.FirstName}\",\"{user.LastName}\",\"{(int)user.Category}\",\"{user.Parent.Id}\")";
            }
            else
            {
                sql = "INSERT INTO AspNetUsers (AccessFailedCount,Id,EmailConfirmed,LockoutEnabled,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,FirstName,LastName,Category)" +
                   $" VALUES (\"{user.AccessFailedCount}\",\"{user.Id}\",\"{user.EmailConfirmed}\",\"{user.LockoutEnabled}\",\"{user.PhoneNumber}\",\"{user.PhoneNumberConfirmed}\",\"{user.TwoFactorEnabled}\",\"{user.FirstName}\",\"{user.LastName}\",\"{(int)user.Category}\")";
            }

            var insertSQL = new SqliteCommand(sql, connection);

            try
            {
                insertSQL.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        protected IPrincipalProvider GetCurrentUserProvider()
        {
            var principal = new Mock<IPrincipal>();
            var logger = new Mock<ILogger<PrincipalProvider>>();
            return new PrincipalProvider(principal.Object, logger.Object);
        }

        protected IGenericService GetGenericService(IDateTimeService dateTimeService)
        {
            return new GenericService(UowSingletonInstance, dateTimeService, GetCurrentUserProvider());
        }
        protected IUserService GetUserService()
        {
            return new UserService(UowSingletonInstance);
        }
        protected IDateTimeService GetDateTimeService()
        {
            var dateTimeService = new Mock<IDateTimeService>();

            dateTimeService.Setup(x => x.UtcNow()).Returns(MockedNow);

            return dateTimeService.Object;
        }

        private CosmosOptions GetCosmosOptions()
        {
            return new CosmosOptions
            {
                BusGpsCollectionId = "bus-gps",
                CardStatusCollectionId = "card-status",
                DatabaseId = "school-time-tracker-cosmos-db",
                DeviceEventCollectionId = "device-event",
                EndpointUri = "https://localhost:8081",
                PrimaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="
            };
        }

        private static ToggleOptions GetToggleOptions()
        {
            return new ToggleOptions
            {
                CosmosEnabled = true,
                AuthEnabled = false
            };
        }

        protected IBusGpsLogService GetBusGpsService()
        {
            return new BusGpsLogService(GetCosmosOptions(), GetToggleOptions(), GetDateTimeService());
        }

        protected IDeviceEventService GetDeviceEventService()
        {
            return new DeviceEventService(GetCosmosOptions(), GetToggleOptions(), GetDateTimeService());
        }

        protected ILocationLogService GetLocationLogService()
        {
            return new LocationLogService(GetCosmosOptions(), GetToggleOptions(), GetDateTimeService());
        }

        protected UserManager<TUser> GetTestingUserManager<TUser>(IUserStore<TUser> store = null) where TUser : class
        {
            store = store ?? new Mock<IUserStore<TUser>>().Object;

            var options = new Mock<IOptions<IdentityOptions>>();

            var idOptions = new IdentityOptions();

            idOptions.Lockout.AllowedForNewUsers = false;

            options.Setup(o => o.Value).Returns(idOptions);

            var userValidators = new List<IUserValidator<TUser>>();

            var validator = new Mock<IUserValidator<TUser>>();

            userValidators.Add(validator.Object);

            var pwdValidators = new List<PasswordValidator<TUser>>
            {
                new PasswordValidator<TUser>()
            };

            var logger = new Mock<ILogger<UserManager<TUser>>>().Object;


            var userManager = new UserManager<TUser>(
                store,
                options.Object,
                new PasswordHasher<TUser>(),
                userValidators,
                pwdValidators,
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                null,
                logger);


            validator.Setup(v => v.ValidateAsync(userManager, It.IsAny<TUser>()))
                .Returns(Task.FromResult(IdentityResult.Success)).Verifiable();

            return userManager;
        }

        protected DeviceController GetDeviceController()
        {
            return new DeviceController(genericService);
        }

        protected CardController GetCardController()
        {

            var cardService = new CardService(UowSingletonInstance, dateTimeService, genericService, GetCurrentUserProvider());

            return new CardController(genericService, cardService);
        }

        protected RFIDController GetRfidController(IDateTimeService dateTimeService, INotificationSender notificationSender)
        {
            var locationService = new Mock<ILocationService>();
            var logger = new Mock<ILogger<RFIDController>>();
            var options = new GeneralOptions
            {
                MinutesBetweenPushNotifications = 5,
                SchoolLocationLogicMinutesDifference = 5
            };

            return new RFIDController(
                dateTimeService,
                genericService,
                notificationSender,
                locationService.Object,
                GetDeviceEventService(),
                logger.Object,
                options);
        }

        protected SchoolController GetSchoolController()
        {
            var imageService = new Mock<IImageService>();

            var userStore = new TestUserStore(userService);

            var userManager = GetTestingUserManager(userStore);

            return new SchoolController(genericService,
                                        imageService.Object,
                                        userService,
                                        dateTimeService,
                                        userManager,
                                        GetCurrentUserProvider());
        }


        protected UserController GetUserController()
        {

            var userStore = new TestUserStore(userService);
            var userManager = GetTestingUserManager(userStore);

            var connection = new SqliteConnection("DataSource=:memory:");

            var imageService = new Mock<IImageService>();
            var locationService = new Mock<ILocationService>();

            return new UserController(userManager,
                genericService,
                imageService.Object,
                userService,
                dateTimeService,
                locationService.Object,
                GetToggleOptions(),
                GetCurrentUserProvider());

        }

        protected static DateTime MockedNow => new DateTime(2018, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc);
    }

}
