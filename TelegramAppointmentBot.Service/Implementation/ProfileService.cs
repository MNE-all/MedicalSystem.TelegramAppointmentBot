using TelegramAppointmentBot.Context;
using TelegramAppointmentBot.Context.Models;
using TelegramAppointmentBot.Context.Models.Request;
using TelegramAppointmentBot.Context.Models.Response;
using TelegramAppointmentBot.Service.Contract.Interfaces;

namespace TelegramAppointmentBot.Service.Implementation;

public class ProfileService : IProfileService
{
    public Task<IEnumerable<GetUserProfiles>> GetUserProfilesAsync(long userId, CancellationToken cancellationToken)
    {
        var result = new List<GetUserProfiles>();
        using (var db = new AppointmentContext())
        {
            var list = db.Profiles.Where(p => p.OwnerId == userId).ToList();
            result.AddRange(list.Select(profile => new GetUserProfiles { Id = profile.Id, Title = profile.Title }));
        }

        return Task.FromResult<IEnumerable<GetUserProfiles>>(result);
    }

    public Task<Profile> GetProfileByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        using (var db = new AppointmentContext())
        {
            var result = db.Profiles.FirstOrDefault(p => p.Id == id);
            return result != null ? Task<Profile>.FromResult(result) : null;
        }
    }

    public Task<Profile> AddProfile(long ownerId, AddProfile newProfile, CancellationToken cancellationToken)
    {
        using (var db = new AppointmentContext())
        {
            var fullProfile = new Profile
            {
                Surname = newProfile.Surname,
                Name = newProfile.Name,
                Patronomyc = newProfile.Patronomyc,
                Birthdate = newProfile.Birthdate,
                Email = newProfile.Email,
                Title = newProfile.Title,
                OMS = newProfile.OMS,
                OwnerId = ownerId
            };
            var result = db.Profiles.Add(fullProfile).Entity;
            db.SaveChanges();

            return Task<Profile>.FromResult(result);
        }
    }
}