namespace TelegramAppointmentBot.Context.Models.Request
{
    public class AppointmentHunterRequest
    {
        public Guid PatientId { get; set; }

        public int LpuId { get; set; }
        public int DoctorId { get; set; }

        public DayOfWeek? DesiredDay { get; set; }
        public DateTime? DesiredTime { get; set; }
        public string SpecialityName { get; set;}
    }
}
