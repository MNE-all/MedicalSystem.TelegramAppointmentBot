using TelegramAppointmentBot.Context.Enums;
using TelegramAppointmentBot.Context.Models;
using TelegramAppointmentBot.Context.Models.Request;

namespace TelegramAppointmentBot.Service.Contract.Interfaces
{
    public interface IAppointmentHunterService
    {
        //TODO Методы для создания записи к врачу
        Task<AppointmentHunter> Create(AppointmentHunterRequest requestModel, CancellationToken cancellationToken);

        Task ChangeTime(Guid appointmentId, DateTime time, CancellationToken cancellationToken);
        Task ChangeStatement(Guid appointmentId, HunterStatement statement, CancellationToken cancellationToken);
        Task<HunterStatement> GetStatement(Guid appointmentId, CancellationToken cancellationToken);
        Task Delete(Guid appointmentId, CancellationToken cancellationToken);

        Task<List<AppointmentHunter>> GetHuntersInProgress(CancellationToken cancellationToken);

        Task<List<AppointmentHunter>> GetHuntersInProgressByProfileId(Guid profileId, CancellationToken cancellationToken);
    }
}
