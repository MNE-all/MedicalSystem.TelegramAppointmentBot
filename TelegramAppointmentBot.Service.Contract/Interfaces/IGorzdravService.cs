using TelegramAppointmentBot.Context.Models.Request;
using TelegramAppointmentBot.Context.Models.Response;

namespace TelegramAppointmentBot.Service.Contract.Interfaces;

public interface IGorzdravService
{
    Task<IEnumerable<LPUResult>> GetLPUs(Guid profileId, CancellationToken cancellationToken);
    Task<IEnumerable<SpecialtiesResult>> GetSpecialties(int lpuId, CancellationToken cancellationToken);
    Task<IEnumerable<DoctorResult>> GetDoctors(int lpuId, int specialityId, CancellationToken cancellationToken);
    Task CreateAppointment(CreateAnAppointment model, CancellationToken cancellationToken);
    Task<GetTimetable> GetTimetable(int lpuId, int doctorId, CancellationToken cancellationToken);
    Task<GetAppointments> GetAppointments(int lpuId, int doctorId, CancellationToken cancellationToken);

    Task<GetPatient> GetPatient(Guid profileId, int lpuId, CancellationToken cancellationToken);

    Task<GetVisits> GetVisits(string patientId, int lpuId, CancellationToken cancellationToken);

    Task DeleteAppointment(CancelTheAppointment model, CancellationToken cancellationToken);

}