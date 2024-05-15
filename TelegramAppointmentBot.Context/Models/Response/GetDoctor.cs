namespace TelegramAppointmentBot.Context.Models.Response
{
    public class GetDoctor : GorzdravResponse
    {
        public List<DoctorResult> result { get; set; }
    }
}
