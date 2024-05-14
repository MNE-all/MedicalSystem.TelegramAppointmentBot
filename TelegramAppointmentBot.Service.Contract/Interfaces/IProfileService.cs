using System.Threading.Tasks;
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
    Task<Profile> AddProfile(long ownerId, string title, CancellationToken cancellationToken);
    Task ChangeTitle(Guid profileId, string Title, CancellationToken cancellationToken);
    Task ChangeOMS(Guid profileId, string OMS, CancellationToken cancellationToken);
    Task ChangeSurname(Guid profileId, string Surname, CancellationToken cancellationToken);
    Task ChangeName(Guid profileId, string Name, CancellationToken cancellationToken);
    Task ChangePatronomyc(Guid profileId, string Patronomyc, CancellationToken cancellationToken);
    Task ChangeEmail(Guid profileId, string Email, CancellationToken cancellationToken);
    Task ChangeBirthdate(Guid profileId, DateTime Birthdate, CancellationToken cancellationToken);
    Task<bool> ValidateProfile(Guid profileId, CancellationToken cancellationToken);





}