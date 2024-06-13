namespace TelegramAppointmentBot.Context.Models;

public class Profile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = null!;
    public byte[]? OMS { get; set; }
    public byte[]? Surname { get; set; }
    public byte[]? Name { get; set; }
    public byte[]? Patronomyc { get; set; }
    public byte[]? Email { get; set; }
    public DateTime? Birthdate { get; set; }

    public User? Owner { get; set; }
    public Guid OwnerId { get; set; }

    public bool IsFilled { get; set; } = false;
}