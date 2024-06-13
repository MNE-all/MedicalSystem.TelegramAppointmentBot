using Microsoft.EntityFrameworkCore;
using TelegramAppointmentBot.Context.Models;

namespace TelegramAppointmentBot.Context
{
    public class EncryptionContext : DbContext
    {
        public DbSet<UserEncrypt> UserEncrypts { get; set; }

        public EncryptionContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Configuration.connectionStringMSSQL);
            base.OnConfiguring(optionsBuilder);
        }
    }
}
