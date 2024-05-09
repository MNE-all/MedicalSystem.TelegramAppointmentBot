using TelegramAppointmentBot.Context;
using TelegramAppointmentBot.Context.Models;
using TelegramAppointmentBot.Service.Contract.Interfaces;

namespace TelegramAppointmentBot.Service.Implementation;

public class UserService : IUserService
{
    public Task<User> AddUser(User user, CancellationToken cancellationToken)
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
                    db.SaveChanges();
                }
            }
            
            return Task<User>.FromResult(user);
        }
    }
}