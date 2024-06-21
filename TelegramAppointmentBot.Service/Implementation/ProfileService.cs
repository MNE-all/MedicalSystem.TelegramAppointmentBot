using System.Linq;
using System.Net.WebSockets;
using System.Reflection.Emit;
using System.Xml.Linq;
using TelegramAppointmentBot.Context;
using TelegramAppointmentBot.Context.Models;
using TelegramAppointmentBot.Context.Models.Request;
using TelegramAppointmentBot.Context.Models.Response;
using TelegramAppointmentBot.Service.Contract.Interfaces;

namespace TelegramAppointmentBot.Service.Implementation;

public class ProfileService : IProfileService
{
    private static IEncryptService encryptService = new EncryptService();
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

    public Task<GetProfileById> GetProfileByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        GetProfileById result = new();
        using (var db = new AppointmentContext())
        {
            var profile = db.Profiles.FirstOrDefault(p => p.Id == id);
            var userId = db.Users.First(u => u.SystemId == profile!.OwnerId).Id;
            using (var encryptDb = new EncryptionContext())
            {
                var userEncrypt = encryptDb.UserEncrypts.First(u => u.Id == userId);

                result.OMS = encryptService.Decrypt(profile.OMS, userEncrypt.Key, userEncrypt.IV).Result;
                result.Surname = encryptService.Decrypt(profile.Surname, userEncrypt.Key, userEncrypt.IV).Result;
                result.Name = encryptService.Decrypt(profile.Name, userEncrypt.Key, userEncrypt.IV).Result;
                result.Patronomyc = encryptService.Decrypt(profile.Patronomyc, userEncrypt.Key, userEncrypt.IV).Result;
                result.Birthdate = profile.Birthdate;
                result.Id = profile.Id;
                result.OwnerId = profile.OwnerId;
            }
            return Task<Profile>.FromResult(result);
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
            using(var encryptDb = new EncryptionContext())
            {
                var profile = db.Profiles.First(p => p.Id == profileId);
                var userId = db.Users.First(u => u.SystemId == profile.OwnerId).Id;
                var userEncrypt = encryptDb.UserEncrypts.First(u => u.Id == userId);

                db.Profiles.First(p => p.Id == profileId).OMS = encryptService.Encrypt(OMS, userEncrypt.Key, userEncrypt.IV).Result;
                db.SaveChanges();
            }
            
            return Task.CompletedTask;
        }
    }

    public Task ChangeSurname(Guid profileId, string Surname, CancellationToken cancellationToken)
    {
        using (var db = new AppointmentContext())
        {
            using (var encryptDb = new EncryptionContext())
            {
                var profile = db.Profiles.First(p => p.Id == profileId);
                var userId = db.Users.First(u => u.SystemId == profile.OwnerId).Id;
                var userEncrypt = encryptDb.UserEncrypts.First(u => u.Id == userId);

                db.Profiles.First(p => p.Id == profileId).Surname = encryptService.Encrypt(Surname, userEncrypt.Key, userEncrypt.IV).Result;
                db.SaveChanges();
            }
            return Task.CompletedTask;
        }
    }

    public Task ChangeName(Guid profileId, string Name, CancellationToken cancellationToken)
    {
        using (var db = new AppointmentContext())
        {
            using (var encryptDb = new EncryptionContext())
            {
                var profile = db.Profiles.First(p => p.Id == profileId);
                var userId = db.Users.First(u => u.SystemId == profile.OwnerId).Id;
                var userEncrypt = encryptDb.UserEncrypts.First(u => u.Id == userId);

                db.Profiles.First(p => p.Id == profileId).Name = encryptService.Encrypt(Name, userEncrypt.Key, userEncrypt.IV).Result;
                db.SaveChanges();
            }
            return Task.CompletedTask;
        }
    }

    public Task ChangePatronomyc(Guid profileId, string Patronomyc, CancellationToken cancellationToken)
    {
        using (var db = new AppointmentContext())
        {
            using (var encryptDb = new EncryptionContext())
            {
                var profile = db.Profiles.First(p => p.Id == profileId);
                var userId = db.Users.First(u => u.SystemId == profile.OwnerId).Id;
                var userEncrypt = encryptDb.UserEncrypts.First(u => u.Id == userId);

                db.Profiles.First(p => p.Id == profileId).Patronomyc = encryptService.Encrypt(Patronomyc, userEncrypt.Key, userEncrypt.IV).Result;
                db.SaveChanges();
            }
            return Task.CompletedTask;
        }
    }

    public Task ChangeEmail(Guid profileId, string Email, CancellationToken cancellationToken)
    {
        using (var db = new AppointmentContext())
        {
            using (var encryptDb = new EncryptionContext())
            {
                var profile = db.Profiles.First(p => p.Id == profileId);
                var userId = db.Users.First(u => u.SystemId == profile.OwnerId).Id;
                var userEncrypt = encryptDb.UserEncrypts.First(u => u.Id == userId);

                db.Profiles.First(p => p.Id == profileId).Email = encryptService.Encrypt(Email, userEncrypt.Key, userEncrypt.IV).Result;
                db.SaveChanges();
            }
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
            var profile = db.Profiles.First(x => x.Id == profileId);

            db.Hunters.RemoveRange(db.Hunters.Where(h => h.PatientId == profile.Id).ToList());

            db.Profiles.Remove(profile);
            db.SaveChanges();
            return Task.CompletedTask;
        }
    }
}