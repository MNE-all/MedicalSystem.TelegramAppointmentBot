using TelegramAppointmentBot.Context.Models;

namespace TelegramAppointmentBot.Service.Contract.Interfaces
{
    public interface ISpecialityService
    {
        public Task<Guid> AddOrFind(string id, string name, int lpuId, CancellationToken cancellationToken);
        public Task<Speciality> GetBySystemId(Guid systemId, CancellationToken cancellationToken);
    }
}
