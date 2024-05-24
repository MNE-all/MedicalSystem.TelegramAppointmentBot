using System.ComponentModel.DataAnnotations;

namespace TelegramAppointmentBot.Context.Models
{
    public class Speciality
    {
        [Key]
        public Guid SystemId { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public int lpuId { get; set; }
    }
}
