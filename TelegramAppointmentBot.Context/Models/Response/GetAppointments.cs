namespace TelegramAppointmentBot.Context.Models.Response
{
    public class GetAppointments : GorzdravResponse
    {
        public List<TimetableResult> result { get; set; }

    }
}
