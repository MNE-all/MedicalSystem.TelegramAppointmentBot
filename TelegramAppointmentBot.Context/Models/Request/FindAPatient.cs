namespace TelegramAppointmentBot.Context.Models.Request;

/// <summary>
/// Сущность для поиска patientId в gorzdrav api
/// </summary>
public class FindAPatient
{
    public int lpuId { get; set; }
    public string lastName { get; set; }
    public string firstName { get; set; }
    public string middleName { get; set; }
    public DateTime birthdate { get; set; }
}