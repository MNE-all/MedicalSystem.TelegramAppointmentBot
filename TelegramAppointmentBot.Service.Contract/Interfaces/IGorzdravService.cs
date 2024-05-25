using TelegramAppointmentBot.Context.Models.Request;
using TelegramAppointmentBot.Context.Models.Response;

namespace TelegramAppointmentBot.Service.Contract.Interfaces;

public interface IGorzdravService
{
    Task<GetLPUsByOMS> GetLPUs(Guid profileId, CancellationToken cancellationToken);
    Task<GetSpecialties> GetSpecialties(int lpuId, CancellationToken cancellationToken);
    Task<GetDoctor> GetDoctors(int lpuId, string specialityId, CancellationToken cancellationToken);
    Task<GorzdravResponse> CreateAppointment(CreateAnAppointment model, CancellationToken cancellationToken);
    Task<GetTimetable> GetTimetable(int lpuId, int doctorId, CancellationToken cancellationToken);
    Task<GetAppointments> GetAppointments(int lpuId, int doctorId, CancellationToken cancellationToken);

    Task<GetPatient> GetPatient(Guid profileId, int lpuId, CancellationToken cancellationToken);

    Task<GetVisits> GetVisits(string patientId, int lpuId, CancellationToken cancellationToken);

    Task<Context.Models.Response.CancelTheAppointment> DeleteAppointment(Context.Models.Request.CancelTheAppointment model, CancellationToken cancellationToken);

}