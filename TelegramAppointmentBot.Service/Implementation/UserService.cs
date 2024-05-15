using TelegramAppointmentBot.Context;
using TelegramAppointmentBot.Context.Enums;
using TelegramAppointmentBot.Context.Models;
using TelegramAppointmentBot.Service.Contract.Interfaces;

namespace TelegramAppointmentBot.Service.Implementation;

public class UserService : IUserService
{
    Task IUserService.ChangeCurrentProfile(long userId, Guid profileId, CancellationToken cancellationToken)
    {
        using (var db = new AppointmentContext())
        {
            db.Users.First(x => x.Id == userId).CurrentProfile = profileId;
            db.SaveChanges();
            return Task.CompletedTask;
        }
    }

    Task IUserService.ClearCurrentProfile(long userId, CancellationToken cancellationToken)
    {
        using (var db = new AppointmentContext())
        {
            db.Users.First(x => x.Id == userId).CurrentProfile = null;
            db.SaveChanges();
            return Task.CompletedTask;
        }
    }

    Task<Guid> IUserService.GetCurrentProfile(long userId, CancellationToken cancellationToken)
    {
        using (var db = new AppointmentContext())
        {
            return Task.FromResult(db.Users.First(x => x.Id == userId).CurrentProfile.GetValueOrDefault());
        }
    }

    Task<User> IUserService.AddUser(User user, CancellationToken cancellationToken)
    {
        using (var db = new AppointmentContext())
        {
            var _user = db.Users.FirstOrDefault(u => u.Id == user.Id);

            if (_user == null)
            {
                db.Users.Add(user);
                db.SaveChanges();
            }
            else
            {
                if (_user.FirstName != user.FirstName)
                {
                    _user.FirstName = user.FirstName;
                    _user.Statement = ProfileStatement.None;
                    _user.CurrentProfile = user.CurrentProfile;
                    db.SaveChanges();
                }
            }
            
            return Task<User>.FromResult(user);
        }
    }
    
    Task IUserService.ChangeStatement(long userId, ProfileStatement statement, CancellationToken cancellationToken)
    {
        using (var db = new AppointmentContext())
        {
            var _user = db.Users.FirstOrDefault(s => s.Id == userId);
            if (_user != null)
            {
                _user.Statement = statement;
                db.SaveChanges();
                return Task<User>.FromResult(_user);
            }
            else
                return Task.FromCanceled(cancellationToken);
        }

    }

    Task<ProfileStatement> IUserService.CheckStatement(long userId, CancellationToken cancellationToken)
    {
        using (var db = new AppointmentContext())
        {
            var _user = db.Users.First(u => u.Id == userId);
            return Task.FromResult(_user.Statement);
        }
    }
}