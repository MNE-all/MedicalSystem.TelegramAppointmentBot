namespace TelegramAppointmentBot.Context.Models.Response;

public class GorzdravResponse
{
    public bool success { get; set; }
    public int errorCode { get; set; }
    public string? message { get; set; }
    public string? stackTrace { get; set; }
}