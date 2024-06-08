using Microsoft.EntityFrameworkCore;
using TelegramAppointmentBot.Context.Models;

namespace TelegramAppointmentBot.Context;

public class AppointmentContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Profile> Profiles { get; set; }
    public DbSet<AppointmentHunter> Hunters { get; set; }
    public DbSet<Visit> Visits { get; set; }
    public DbSet<Speciality> Specialities { get; set; }



    public AppointmentContext()
    {
        Database.EnsureCreated();
        
    }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        optionsBuilder.UseNpgsql(Configuration.connectionStringPostgre);
        // optionsBuilder.UseSqlServer(Configuration.connectionString);
        base.OnConfiguring(optionsBuilder);
    }
}