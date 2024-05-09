using Microsoft.EntityFrameworkCore;
using TelegramAppointmentBot.Context.Models;

namespace TelegramAppointmentBot.Context;

public class AppointmentContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Profile> Profiles { get; set; }
    public DbSet<AppointmentHunter> Hunters { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("");
        base.OnConfiguring(optionsBuilder);
    }
}