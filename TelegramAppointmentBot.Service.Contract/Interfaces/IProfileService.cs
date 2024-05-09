using TelegramAppointmentBot.Context.Models;
using TelegramAppointmentBot.Context.Models.Request;
using TelegramAppointmentBot.Context.Models.Response;

namespace TelegramAppointmentBot.Service.Contract.Interfaces;

public interface IProfileService
{
    /// <summary>
    /// Получить все профили пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<GetUserProfiles>> GetUserProfilesAsync (long userId, CancellationToken cancellationToken);

    /// <summary>   
    /// Получить профиль по идентификатору 
    /// </summary>
    /// <param name="id">Идентификатор профиля</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Profile> GetProfileByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Добавить профиль
    /// </summary>
    /// <param name="newProfile">модель для добавления</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Profile> AddProfile(long ownerId, AddProfile newProfile, CancellationToken cancellationToken);

}