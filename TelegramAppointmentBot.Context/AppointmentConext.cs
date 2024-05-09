using Microsoft.EntityFrameworkCore;
using TelegramAppointmentBot.Context.Models;

namespace TelegramAppointmentBot.Context;

public class AppointmentContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Profile> Profiles { get; set; }
    public DbSet<AppointmentHunter> Hunters { get; set; }

    public AppointmentContext()
    {
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql(
            Configuration.connectionString, 
            new MySqlServerVersion(new Version(8, 0, 36))
        );
        base.OnConfiguring(optionsBuilder);
    }
}