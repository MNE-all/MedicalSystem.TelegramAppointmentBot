namespace TelegramAppointmentBot.Context.Models.Response
{
    public class TimetableResult
    {
        public string id { get; set; }
        public DateTime visitStart { get; set; }
        public DateTime visitEnd { get; set; }
        public string? address { get; set; }
        public int? number { get; set; }
        public int room { get; set; }

    }
}
