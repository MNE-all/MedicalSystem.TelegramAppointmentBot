using System.ComponentModel.DataAnnotations;

namespace TelegramAppointmentBot.Context.Models
{
    public class UserEncrypt
    {
        [Key]
        public Guid SystemId { get; set; }
        public long Id { get; set; }
        public byte[] Key { get; set; }
        public byte[] IV { get; set; }
    }
}
