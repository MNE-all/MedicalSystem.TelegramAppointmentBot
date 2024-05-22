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
            var owner = db.Users.First(p => p.Id == userId);

            var list = db.Profiles.Where(p => p.OwnerId == owner.SystemId).ToList();
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

    public Task<Profile> AddProfile(long ownerId, string title, CancellationToken cancellationToken)
    {
        using (var db = new AppointmentContext())
        {
            var owner = db.Users.First(p => p.Id == ownerId);
            
            var profile = db.Profiles.Add(new Profile
            {
                OwnerId = owner.SystemId,
                Title = title,
            }).Entity;
            db.SaveChanges();
            return Task<Profile>.FromResult(profile);
        }
    }

    public Task ChangeTitle(Guid profileId, string Title, CancellationToken cancellationToken)
    {
        using (var db = new AppointmentContext())
        {
            db.Profiles.First(p => p.Id == profileId).Title = Title;
            db.SaveChanges();
            return Task.CompletedTask;
        }
    }

    public Task ChangeOMS(Guid profileId, string OMS, CancellationToken cancellationToken)
    {
        using (var db = new AppointmentContext())
        {
            db.Profiles.First(p => p.Id == profileId).OMS = OMS;
            db.SaveChanges();
            return Task.CompletedTask;
        }
    }

    public Task ChangeSurname(Guid profileId, string Surname, CancellationToken cancellationToken)
    {
        using (var db = new AppointmentContext())
        {
            db.Profiles.First(p => p.Id == profileId).Surname = Surname;
            db.SaveChanges();
            return Task.CompletedTask;
        }
    }

    public Task ChangeName(Guid profileId, string Name, CancellationToken cancellationToken)
    {
        using (var db = new AppointmentContext())
        {
            db.Profiles.First(p => p.Id == profileId).Name = Name;
            db.SaveChanges();
            return Task.CompletedTask;
        }
    }

    public Task ChangePatronomyc(Guid profileId, string Patronomyc, CancellationToken cancellationToken)
    {
        using (var db = new AppointmentContext())
        {
            db.Profiles.First(p => p.Id == profileId).Patronomyc = Patronomyc;
            db.SaveChanges();
            return Task.CompletedTask;
        }
    }

    public Task ChangeEmail(Guid profileId, string Email, CancellationToken cancellationToken)
    {
        using (var db = new AppointmentContext())
        {
            db.Profiles.First(p => p.Id == profileId).Email = Email;
            db.SaveChanges();
            return Task.CompletedTask;
        }
    }

    public Task ChangeBirthdate(Guid profileId, DateTime Birthdate, CancellationToken cancellationToken)
    {
        using (var db = new AppointmentContext())
        {
            db.Profiles.First(p => p.Id == profileId).Birthdate = Birthdate;
            db.SaveChanges();
            return Task.CompletedTask;
        }
    }

    public Task<bool> ValidateProfile(Guid profileId, CancellationToken cancellationToken)
    {
        using (var db = new AppointmentContext())
        {
            var profile = db.Profiles.First(p => p.Id == profileId);
            if (profile.OMS != null &&
                profile.Surname != null &&
                profile.Name != null &&
                profile.Birthdate != null &&
                profile.Email != null)
            {
                profile.IsFilled = true;
                db.SaveChanges();
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }

    public Task Delete(Guid profileId, CancellationToken cancellationToken)
    {
        using(var db = new AppointmentContext())
        {
            db.Profiles.Remove(db.Profiles.First(x => x.Id == profileId));
            db.SaveChanges();
            return Task.CompletedTask;
        }
    }
}