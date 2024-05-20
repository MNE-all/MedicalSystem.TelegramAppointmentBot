using System.ComponentModel.DataAnnotations;
using TelegramAppointmentBot.Context.Enums;

namespace TelegramAppointmentBot.Context.Models;

public class User
{
    [Key]
    public Guid SystemId { get; set; }
    public long Id { get; set; }
    public string FirstName { get; set; }

    public ProfileStatement Statement { get; set; } = ProfileStatement.None;
    public Guid? CurrentProfile { get; set; }
    public Guid? CurrentHunter { get; set; }
}