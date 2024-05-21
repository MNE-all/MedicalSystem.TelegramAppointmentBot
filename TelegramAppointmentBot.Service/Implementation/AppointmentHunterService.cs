using TelegramAppointmentBot.Context;
using TelegramAppointmentBot.Context.Enums;
using TelegramAppointmentBot.Context.Models;
using TelegramAppointmentBot.Context.Models.Request;
using TelegramAppointmentBot.Service.Contract.Interfaces;

namespace TelegramAppointmentBot.Service.Implementation
{
    public class AppointmentHunterService : IAppointmentHunterService
    {
        public Task ChangeStatement(Guid appointmentId, HunterStatement statement, CancellationToken cancellationToken)
        {
            using (var db = new AppointmentContext())
            {
                db.Hunters.First(x => x.Id == appointmentId).Statement = statement;
                db.SaveChanges();
                return Task.CompletedTask;
            }
        }

        public Task ChangeTime(Guid appointmentId, DateTime time, CancellationToken cancellationToken)
        {
            using (var db = new AppointmentContext())
            {
                db.Hunters.First(x => x.Id == appointmentId).DesiredTime = time;
                db.SaveChanges();
                return Task.CompletedTask;
            }
        }

        public Task<AppointmentHunter> Create(AppointmentHunterRequest requestModel, CancellationToken cancellationToken)
        {
            using (var db = new AppointmentContext())
            {
                var result = db.Hunters.Add(new AppointmentHunter
                {
                    LpuId = requestModel.LpuId,
                    PatientId = requestModel.PatientId,
                    DoctorId = requestModel.DoctorId,
                    DesiredDay = requestModel.DesiredDay,
                    DesiredTime = requestModel.DesiredTime,
                });
                db.SaveChanges();
                return Task.FromResult(result.Entity);
            }
        }

        public Task Delete(Guid appointmentId, CancellationToken cancellationToken)
        {
            using (var db = new AppointmentContext())
            {
                db.Hunters.Remove(db.Hunters.First(x => x.Id.Equals(appointmentId)));
                db.SaveChanges();
                return Task.CompletedTask;
            }
        }

        public Task<List<AppointmentHunter>> GetHuntersInProgress(CancellationToken cancellationToken)
        {
            using (var db = new AppointmentContext())
            {
                return Task.FromResult(db.Hunters.Where(x => x.Statement == HunterStatement.InProgress).ToList());
            }
        }

        public Task<HunterStatement> GetStatement(Guid appointmentId, CancellationToken cancellationToken)
        {
            using (var db = new AppointmentContext())
            {
                return Task.FromResult(db.Hunters.First(x => x.Id == appointmentId).Statement);
            }
        }
    }
}
