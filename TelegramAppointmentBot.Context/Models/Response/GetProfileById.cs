namespace TelegramAppointmentBot.Context.Models.Response;

public class GetProfileById
{
    public string Title { get; set; }
    public string OMS { get; set; }
    public string Surname { get; set; }
    public string Name { get; set; }
    public string Patronomyc { get; set; }
    public string Email { get; set; }
    public DateTime Birthdate { get; set; }
}