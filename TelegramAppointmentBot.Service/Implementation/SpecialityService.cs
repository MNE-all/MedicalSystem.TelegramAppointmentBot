using TelegramAppointmentBot.Context;
using TelegramAppointmentBot.Context.Models;
using TelegramAppointmentBot.Service.Contract.Interfaces;

namespace TelegramAppointmentBot.Service.Implementation
{
    public class SpecialityService : ISpecialityService
    {
        public Task<Guid> AddOrFind(string id, string name, int lpuId, CancellationToken cancellationToken)
        {
            using(var db = new AppointmentContext())
            {
                var item = db.Specialities.FirstOrDefault(x => x.name == name && x.id == id && x.lpuId == lpuId);

                if (item == null)
                {
                    item = db.Specialities.Add(new Speciality
                    {
                        id = id,
                        name = name,
                        lpuId = lpuId,
                    }).Entity;
                    db.Specialities.Add(item);
                    db.SaveChanges();
                }

                return Task.FromResult(item.SystemId);
            }
        }

        public Task<Speciality> GetBySystemId(Guid systemId, CancellationToken cancellationToken)
        {
            using (var db = new AppointmentContext())
            {
                return Task.FromResult(db.Specialities.First(x => x.SystemId == systemId));
            }
        }
    }
}
