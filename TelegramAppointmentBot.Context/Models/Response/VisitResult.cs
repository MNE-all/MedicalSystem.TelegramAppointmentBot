namespace TelegramAppointmentBot.Context.Models.Response
{
    public class VisitResult
    {
        public string appointmentId { get; set; }
        public DateTime dateCreatedAppointment { get; set; }
        public string? doctorBringReferal { get; set; }
        public DoctorRendingConsultation doctorRendingConsultation { get; set; }
        public bool isAppointmentByReferral { get; set; }
        public string lpuAddress { get; set; }
        public string lpuFullName { get; set;}
        public int lpuId { get; set; }
        public string lpuPhone { get; set; }
        public string lpuShortName { get; set; }
        public string patientId { get; set; }
        public int? referralId { get; set; }
        public string? specialityBringReferal { get; set; }
        public SpecialityRendingConsultation specialityRendingConsultation { get; set; }
        public DateTime visitStart { get; set; }
        public string? status { get; set; }
        public string? type { get; set; }
        public string? positionBringReferal { get; set; }
        public PositionRendingConsultation positionRendingConsultation { get; set; }
        public string? infections { get; set; }
    }
}
