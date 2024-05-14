using TelegramAppointmentBot.Context.Enums;
using TelegramAppointmentBot.Context.Models;

namespace TelegramAppointmentBot.Service.Contract.Interfaces;

public interface IUserService
{
    Task<User> AddUser(User user, CancellationToken cancellationToken);
    Task ChangeStatement(long userId, ProfileStatement statement, CancellationToken cancellationToken);
    Task<ProfileStatement> CheckStatement(long userId, CancellationToken cancellationToken);
    Task ChangeCurrentProfile(long userId, Guid profileId, CancellationToken cancellationToken);
    Task ClearCurrentProfile(long userId, CancellationToken cancellationToken);
    Task<Guid> GetCurrentProfile(long userId, CancellationToken cancellationToken);

}