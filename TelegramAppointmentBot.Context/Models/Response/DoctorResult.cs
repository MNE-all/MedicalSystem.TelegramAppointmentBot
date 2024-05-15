namespace TelegramAppointmentBot.Context.Models.Response
{
    public class DoctorResult
    {
        public string ariaNumber { get; set; }
        public string? ariaType { get; set; }
        public string comment { get; set; }
        public int freeParticipantCount { get; set; }
        public int freeTicketCount { get; set; }
        public string id { get; set; }
        public DateTime lastDate { get; set; }
        public string name { get; set; }
        public DateTime? nearestDate { get; set; }

    }
}
