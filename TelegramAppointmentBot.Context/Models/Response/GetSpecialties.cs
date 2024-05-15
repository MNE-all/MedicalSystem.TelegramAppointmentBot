namespace TelegramAppointmentBot.Context.Models.Response
{
    public class GetSpecialties : GorzdravResponse
    {
        public List<SpecialtiesResult> result { get; set; }
    }
}
