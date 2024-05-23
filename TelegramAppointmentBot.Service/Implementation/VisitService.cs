using TelegramAppointmentBot.Context;
using TelegramAppointmentBot.Context.Models;
using TelegramAppointmentBot.Service.Contract.Interfaces;

namespace TelegramAppointmentBot.Service.Implementation
{
    public class VisitService : IVisitService
    {
        public Task<Guid> AddVisit(Visit visit, CancellationToken cancellationToken)
        {
            using (var db = new AppointmentContext())
            {
                var dbVisit = db.Visits.FirstOrDefault(x =>
                x.lpuId == visit.lpuId &&
                x.patientId == visit.patientId &&
                x.appointmentId == visit.appointmentId);
                if (dbVisit == null)
                {
                    dbVisit = db.Visits.Add(visit).Entity;
                    db.SaveChanges();
                }
                
                return Task.FromResult(dbVisit.Id);
            }
        }

        public Task<Visit> GetVisit(Guid id, CancellationToken cancellationToken)
        {
            using (var db = new AppointmentContext())
            {
                return Task.FromResult(db.Visits.First(x => x.Id == id));
            }
        }
    }
}
