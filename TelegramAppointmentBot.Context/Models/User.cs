using System.ComponentModel.DataAnnotations;

namespace TelegramAppointmentBot.Context.Models;

public class User
{
    [Key]
    public int SystemId { get; set; }
    public long Id { get; set; }
    public string FirstName { get; set; }
}