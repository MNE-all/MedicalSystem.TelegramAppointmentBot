using System.ComponentModel.DataAnnotations;

namespace TelegramAppointmentBot.Context.Models
{
    public class Visit
    {
        [Key]
        public Guid Id { get; set; }
        public string appointmentId { get; set; }
        public int lpuId { get; set; }
        public string patientId { get; set; }

    }
}
