﻿using System;
using TelegramAppointmentBot.Context;
using TelegramAppointmentBot.Context.Enums;
using TelegramAppointmentBot.Context.Models;
using TelegramAppointmentBot.Service.Contract.Interfaces;

namespace TelegramAppointmentBot.Service.Implementation
{
    public class AppointmentHunterService : IAppointmentHunterService
    {
        public Task ChangeDate(Guid appointmentId, DateTime date, CancellationToken cancellationToken)
        {
            using (var db = new AppointmentContext())
            {
                db.Hunters.First(x => x.Id == appointmentId).DesiredCurrentDay = date;
                db.SaveChanges();
                return Task.CompletedTask;
            }
        }

        public Task ChangeDayOfWeek(Guid appointmentId, System.DayOfWeek? dayOfWeek, CancellationToken cancellationToken)
        {
            using (var db = new AppointmentContext())
            {
                db.Hunters.First(x => x.Id == appointmentId).DesiredDay = dayOfWeek;
                db.SaveChanges();
                return Task.CompletedTask;
            }
        }


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

        public Task ChangeTime(Guid appointmentId, DateTime timeFrom, DateTime timeTo, CancellationToken cancellationToken)
        {
            using (var db = new AppointmentContext())
            {
                var hunter = db.Hunters.First(x => x.Id == appointmentId);
                hunter.DesiredTimeFrom = timeFrom;
                hunter.DesiredTimeTo = timeTo;
                db.SaveChanges();
                return Task.CompletedTask;
            }
        }

        public Task<Guid> Create(Guid patientId, int lpuId, string specialityName, int doctorId, CancellationToken cancellationToken)
        {
            using (var db = new AppointmentContext())
            {
                var result = db.Hunters.Add(new AppointmentHunter
                {
                    LpuId = lpuId,
                    PatientId = patientId,
                    SpecialityName = specialityName,
                    DoctorId = doctorId,
                });
                db.SaveChanges();
                return Task.FromResult(result.Entity.Id);
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

        public Task<AppointmentHunter> GetHunterById(Guid appointmentId, CancellationToken cancellationToken)
        {
            using (var db = new AppointmentContext())
            {
                return Task.FromResult(db.Hunters.First(x => x.Id == appointmentId));
            }
        }

        public Task<List<AppointmentHunter>> GetHuntersInProgress(CancellationToken cancellationToken)
        {
            using (var db = new AppointmentContext())
            {
                return Task.FromResult(db.Hunters.Where(x => x.Statement == HunterStatement.InProgress).OrderBy(h => h.CreatedAt).ToList());
            }
        }

        public Task<List<AppointmentHunter>> GetHuntersInProgressByProfileId(Guid profileId, CancellationToken cancellationToken)
        {
            using (var db = new AppointmentContext())
            {
                return Task.FromResult(db.Hunters.Where(x => x.PatientId == profileId && x.Statement == HunterStatement.InProgress).ToList());
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
