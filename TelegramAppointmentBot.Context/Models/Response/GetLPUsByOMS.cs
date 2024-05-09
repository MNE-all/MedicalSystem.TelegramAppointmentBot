namespace TelegramAppointmentBot.Context.Models.Response;

public class GetLPUsByOMS : GorzdravResponse
{
    public List<LPUResult> result { get; set; }
}