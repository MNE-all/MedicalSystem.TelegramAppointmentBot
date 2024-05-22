namespace TelegramAppointmentBot.Context.Models.Response
{
    public class GetVisits : GorzdravResponse
    {
        public List<VisitResult> result { get; set; }
    }
}
