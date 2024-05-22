namespace TelegramAppointmentBot.Context.Models.Request
{
    public class CancelTheAppointment
    {
        public string appointmentId { get; set; }
        public int lpuId { get; set; }
        public string patientId { get; set; }
        public long esiaId { get; set; } = 000000000;
    }
}
