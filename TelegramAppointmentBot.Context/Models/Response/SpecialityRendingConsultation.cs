namespace TelegramAppointmentBot.Context.Models.Response
{
    public class SpecialityRendingConsultation
    {
        public int id { get; set; }
        public int ferId { get; set; }
        public string name { get; set; }
        public int countFreeParticipant { get; set; }
        public int countFreeTicket { get; set;}
        public DateTime? lastDate { get; set; }
        public DateTime? nearestDate { get; set;}
    }
}
