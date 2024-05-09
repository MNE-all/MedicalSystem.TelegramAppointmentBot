using System.ComponentModel.DataAnnotations;

namespace TelegramAppointmentBot.Context.Models;

public class Profile
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string OMS { get; set; }
    public string Surname { get; set; }
    public string Name { get; set; }
    public string Patronomyc { get; set; }
    public string Email { get; set; }
    public DateTime Birthdate { get; set; }

    public User? Owner { get; set; }
    public long OwnerId { get; set; }
}