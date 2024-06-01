using System;
using System.Net.Http.Json;
using TelegramAppointmentBot.Context;
using TelegramAppointmentBot.Context.Models.Request;
using TelegramAppointmentBot.Context.Models.Response;
using TelegramAppointmentBot.Service.Contract.Interfaces;

namespace TelegramAppointmentBot.Service.Implementation;

public class GorzdravService : IGorzdravService
{
    private readonly IProfileService profileService = new ProfileService();
    private readonly HttpClient httpClient = new HttpClient();

    public async Task<GorzdravResponse?> CreateAppointment(CreateAnAppointment model, CancellationToken cancellationToken)
    {
        // TODO Создание записи
        JsonContent content = JsonContent.Create(model);
        try
        {
            using (var response = httpClient.PostAsync($"https://gorzdrav.spb.ru/_api/api/v2/appointment/create", content))
            {
                var responseBody = await response.Result.Content.ReadFromJsonAsync<GorzdravResponse>(cancellationToken: cancellationToken);

                Console.WriteLine(responseBody!.message);
                return responseBody;
            }
        }
        catch (Exception)
        {
            return null;
        }
    }
    

    public async Task<Context.Models.Response.CancelTheAppointment?> DeleteAppointment(Context.Models.Request.CancelTheAppointment model, CancellationToken cancellationToken)
    {
        // TODO Создание записи
        JsonContent content = JsonContent.Create(model);

        try
        {
            using (var response = httpClient.PostAsync("https://gorzdrav.spb.ru/_api/api/v2/appointment/cancel", content))
            {
                var responseBody = await response.Result.Content.ReadFromJsonAsync<Context.Models.Response.CancelTheAppointment>(cancellationToken: cancellationToken);
                Console.WriteLine("DeleteAppointment: " + responseBody!.message);
                return responseBody;
            }
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<GetAppointments?> GetAppointments(int lpuId, int doctorId, CancellationToken cancellationToken)
    {
        try
        {
            using (var request = new HttpRequestMessage(new HttpMethod("GET"), $"https://gorzdrav.spb.ru/_api/api/v2/schedule/lpu/{lpuId}/doctor/{doctorId}/appointments"))
            {
                var response = httpClient.SendAsync(request, cancellationToken).Result;
                var responseBody = await response.Content.ReadFromJsonAsync<GetAppointments>(cancellationToken: cancellationToken);

                return responseBody!;
            }
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<GetDoctor?> GetDoctors(int lpuId, string specialtyId, CancellationToken cancellationToken)
    {
        try
        {
            using (var request = new HttpRequestMessage(new HttpMethod("GET"), $"https://gorzdrav.spb.ru/_api/api/v2/schedule/lpu/{lpuId}/speciality/{specialtyId}/doctors"))
            {
                var response = httpClient.SendAsync(request, cancellationToken).Result;
                Console.WriteLine(await response.Content.ReadAsStringAsync());
                var responseBody = await response.Content.ReadFromJsonAsync<GetDoctor>(cancellationToken: cancellationToken);

                return responseBody!;
            }
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<GetLPUsByOMS?> GetLPUs(Guid profileId, CancellationToken cancellationToken)
    {
        try
        {
            var profile = await profileService.GetProfileByIdAsync(profileId, cancellationToken);
            using (var request = new HttpRequestMessage(new HttpMethod("GET"), $"https://gorzdrav.spb.ru/_api/api/v2/oms/attachment/lpus?polisN={profile.OMS}"))
            {
                var response = httpClient.SendAsync(request, cancellationToken).Result;
                var responseBody = await response.Content.ReadFromJsonAsync<GetLPUsByOMS>(cancellationToken: cancellationToken);
                return responseBody!;
            }
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<GetPatient?> GetPatient(Guid profileId, int lpuId, CancellationToken cancellationToken)
    {
        try
        {
            using (var db = new AppointmentContext())
            {
                var profile = db.Profiles.First(x => x.Id == profileId);
                var content = new FindAPatient
                {
                    lpuId = lpuId,
                    firstName = profile.Name!,
                    lastName = profile.Surname!,
                    middleName = profile.Patronomyc!,
                    birthdate = profile.Birthdate!.Value.ToShortDateString(),

                };

                var link = $"https://gorzdrav.spb.ru/_api/api/v2/patient/search?lpuId={lpuId}&lastName={profile.Surname}&firstName={profile.Name}&middleName={profile.Patronomyc}&birthdate={profile.Birthdate.Value.ToString("s")}";

                using (var request = new HttpRequestMessage(HttpMethod.Get, link))
                {
                    var response = httpClient.SendAsync(request, cancellationToken).Result;

                    Console.WriteLine(await response.Content.ReadAsStringAsync());

                    var responseBody = await response.Content.ReadFromJsonAsync<GetPatient>(cancellationToken: cancellationToken);

                    return responseBody!;
                }
            }
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<GetSpecialties?> GetSpecialties(int lpuId, CancellationToken cancellationToken)
    {
        try
        {
            using (var request = new HttpRequestMessage(new HttpMethod("GET"), $"https://gorzdrav.spb.ru/_api/api/v2/schedule/lpu/{lpuId}/specialties"))
            {
                var response = httpClient.SendAsync(request, cancellationToken).Result;
                var responseBody = await response.Content.ReadFromJsonAsync<GetSpecialties>(cancellationToken: cancellationToken);
                return responseBody!;
            }
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<GetTimetable?> GetTimetable(int lpuId, int doctorId, CancellationToken cancellationToken)
    {
        try
        {
            using (var request = new HttpRequestMessage(new HttpMethod("GET"), $"https://gorzdrav.spb.ru/_api/api/v2/schedule/lpu/{lpuId}/doctor/{doctorId}/timetable"))
            {
                var response = httpClient.SendAsync(request, cancellationToken).Result;
                var responseBody = await response.Content.ReadFromJsonAsync<GetTimetable>(cancellationToken: cancellationToken);

                return responseBody!;
            }
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<GetVisits?> GetVisits(string patientId, int lpuId, CancellationToken cancellationToken)
    {
        int errorCode = 1;
        GetVisits? responseBody = null;

        try
        {
            while (errorCode == 1)
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), $"https://gorzdrav.spb.ru/_api/api/v2/appointments?lpuId={lpuId}&patientId={patientId}"))
                {
                    {
                        var response = httpClient.SendAsync(request, cancellationToken).Result;
                        Console.WriteLine(await response.Content.ReadAsStringAsync());
                        responseBody = await response.Content.ReadFromJsonAsync<GetVisits>(cancellationToken: cancellationToken);

                        errorCode = responseBody!.errorCode;
                    }
                }
            }
            return responseBody!;
        }
        catch (Exception)
        {
            return null;
        }

    }
}