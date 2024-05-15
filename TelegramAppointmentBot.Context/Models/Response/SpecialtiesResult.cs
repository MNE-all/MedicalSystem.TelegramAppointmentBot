namespace TelegramAppointmentBot.Context.Models.Response
{
    public class SpecialtiesResult
    {
        public string id { get; set; }
        public string ferId { get; set; }
        public string name { get; set; }
        public int countFreeParticipant { get; set; }
        public int countFreeTicket { get; set; }
        public DateTime? lastDate { get; set; }
        public DateTime? nearestDate { get; set; }
    }
}
