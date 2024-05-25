using TelegramAppointmentBot.Context.Enums;
using TelegramAppointmentBot.Context.Models;
using TelegramAppointmentBot.Context.Models.Request;

namespace TelegramAppointmentBot.Service.Contract.Interfaces
{
    public interface IAppointmentHunterService
    {
        //TODO Методы для создания записи к врачу
        Task<Guid> Create(Guid patientId, int lpuId, string specialityName, int doctorId, CancellationToken cancellationToken);

        Task ChangeDayOfWeek(Guid appointmentId, System.DayOfWeek? dayOfWeek, CancellationToken cancellationToken);

        Task<AppointmentHunter> GetHunterById(Guid appointmentId, CancellationToken cancellationToken);

        Task ChangeTime(Guid appointmentId, DateTime time, CancellationToken cancellationToken);
        Task ChangeTime(Guid appointmentId, DateTime timeFrom, DateTime timeTo, CancellationToken cancellationToken);
        Task ChangeStatement(Guid appointmentId, HunterStatement statement, CancellationToken cancellationToken);
        Task<HunterStatement> GetStatement(Guid appointmentId, CancellationToken cancellationToken);
        Task Delete(Guid appointmentId, CancellationToken cancellationToken);

        Task<List<AppointmentHunter>> GetHuntersInProgress(CancellationToken cancellationToken);

        Task<List<AppointmentHunter>> GetHuntersInProgressByProfileId(Guid profileId, CancellationToken cancellationToken);
    }
}
