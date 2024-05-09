namespace TelegramAppointmentBot.Context.Models;

public class AppointmentHunter
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Profile? Patient { get; set; }
    public Guid PatientId { get; set; }

    public int LpuId { get; set; }
    public int DoctorId { get; set; }
    
    public DateTime DesiredTime { get; set; }
}