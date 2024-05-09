using TelegramAppointmentBot.Context.Models.Response;

namespace TelegramAppointmentBot.Service.Contract.Interfaces;

public interface IGorzdravService
{
    Task<IEnumerable<LPUResult>> GetLPUs(CancellationToken cancellationToken);
}