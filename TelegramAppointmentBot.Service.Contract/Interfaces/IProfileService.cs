using TelegramAppointmentBot.Context.Models;

namespace TelegramAppointmentBot.Service.Contract.Interfaces;

public interface IProfileService
{
    /// <summary>
    /// Получить все профили пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<Profile>> GetUserProfilesAsync (Guid userId, CancellationToken cancellationToken);

    /// <summary>   
    /// Получить профиль по идентификатору 
    /// </summary>
    /// <param name="id">Идентификатор профиля</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Profile> GetProfileByIdAsync(Guid id, CancellationToken cancellationToken);

}