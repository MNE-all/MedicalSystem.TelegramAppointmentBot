namespace TelegramAppointmentBot.Context.Models.Response;

public class LPUResult
{
    public int id { get; set; }
    public string description { get; set; }
    public int district { get; set; }
    public int districtId { get; set; }
    public string districtName { get; set; }
    public bool isActive { get; set; }
    public string lpuFullName { get; set; }
    public string lpuShortName { get; set; }
    public string lpuType { get; set; }
    public string? oid { get; set; }
    public int partOf { get; set; }
    public Guid headOrganization { get; set; }
    public Guid organization { get; set; }
    public string address { get; set; }
    public string phone { get; set; }
    public string email { get; set; }
    public string longitude { get; set; }
    public string latitude { get; set; }
    public bool covidVaccination { get; set; }
    public bool inDepthExamination { get; set; }
}