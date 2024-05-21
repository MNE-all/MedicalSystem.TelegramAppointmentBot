namespace TelegramAppointmentBot.Context.Models.Request;

/// <summary>
/// Сущность для создания записи через gorzdrav api
/// </summary>
public class CreateAnAppointment
{
    public long esiaId { get; set; } = 000000000;
    public int lpuId { get; set; }
    public int patientId { get; set; }
    public string appointmentId { get; set; }
    public string? referralId { get; set; }
    public Guid ipmpiCardId { get; set; } = Guid.Parse("1670097a-1d3b-4946-a7a1-a84d20f34550");
    public string recipientEmail { get; set; }
    public string patientLastName { get; set; }
    public string patientFirstName { get; set; }
    public string patientMiddleName { get; set; }
    public DateTime patientBirthdate { get; set; }
    public string room { get; set; }
    public int? num { get; set; }
    public string address { get; set; }
}