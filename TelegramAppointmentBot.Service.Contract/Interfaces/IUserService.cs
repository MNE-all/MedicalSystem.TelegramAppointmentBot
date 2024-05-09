using TelegramAppointmentBot.Context.Models;

namespace TelegramAppointmentBot.Service.Contract.Interfaces;

public interface IUserService
{
    Task<User> AddUser(User user, CancellationToken cancellationToken);
}