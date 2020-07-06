using Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Services.Configurations;
using System.Linq;

namespace Services
{
    public class SttContext : IdentityDbContext<User, UserRole, int>
    {
        public SttContext(DbContextOptions<SttContext> options) : base(options)
        {

        }

        public DbSet<PushLog> PushLogs { get; set; }
        public DbSet<NewsItem> NewsItems { get; set; }
        public DbSet<School> Schools { get; set; }
        public DbSet<Bus> Buses { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<StudentCardHistoryItem> StudentCardHistoryItems { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Alert> Alerts { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new UserConfiguration());
            builder.ApplyConfiguration(new BusConfiguration());
            builder.ApplyConfiguration(new SchoolConfiguration());
            builder.ApplyConfiguration(new CardConfiguration());
            builder.ApplyConfiguration(new SchoolGradeRoomConfiguration());
            builder.ApplyConfiguration(new DeviceConfiguration());
            builder.ApplyConfiguration(new PushLogConfiguration());

            SetDefaultMaxLength(builder, 500);
        }

        private static void SetDefaultMaxLength(ModelBuilder builder, int maxLength)
        {
            var properties = builder.Model
                .GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(string) && p.GetMaxLength() == null && !p.Name.Contains("Id") && !p.DeclaringType.Name.Contains("Microsoft"))
                .ToList();

            foreach (var p in properties)
            {
                p.SetMaxLength(maxLength);
                p.IsUnicode(false);
            }
        }
    }
}
