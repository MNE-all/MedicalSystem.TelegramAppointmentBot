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

    public async Task<GorzdravResponse> CreateAppointment(CreateAnAppointment model, CancellationToken cancellationToken)
    {
        // TODO Создание записи
        JsonContent content = JsonContent.Create(model);

        using(var response = httpClient.PostAsync($"https://gorzdrav.spb.ru/_api/api/v2/appointment/create", content))
        {
            var responseBody = await response.Result.Content.ReadFromJsonAsync<GorzdravResponse>(cancellationToken: cancellationToken);

            Console.WriteLine(responseBody.message);
            return responseBody;
        }        
    }
    

    public async Task DeleteAppointment(CancelTheAppointment model, CancellationToken cancellationToken)
    {
        // TODO Создание записи
        JsonContent content = JsonContent.Create(model);

        bool success = false;
        while (!success)
        {
            using (var response = httpClient.PostAsync("https://gorzdrav.spb.ru/_api/api/v2/appointment/cancel", content))
            {
                var responseBody = await response.Result.Content.ReadFromJsonAsync<GorzdravResponse>(cancellationToken: cancellationToken);
                Console.WriteLine("DeleteAppointment: " + responseBody.message);
                success = responseBody.success;
                return;
            }
        }
    }

    public async Task<GetAppointments> GetAppointments(int lpuId, int doctorId, CancellationToken cancellationToken)
    {
        using (var request = new HttpRequestMessage(new HttpMethod("GET"), $"https://gorzdrav.spb.ru/_api/api/v2/schedule/lpu/{lpuId}/doctor/{doctorId}/appointments"))
        {
            var response = httpClient.SendAsync(request, cancellationToken).Result;
            var responseBody = await response.Content.ReadFromJsonAsync<GetAppointments>(cancellationToken: cancellationToken);

            return responseBody!;
        }
    }

    public async Task<IEnumerable<DoctorResult>> GetDoctors(int lpuId, int specialtyId, CancellationToken cancellationToken)
    {
        var result = new List<DoctorResult>();
        bool success = false;
        while (!success)
        {
            using (var request = new HttpRequestMessage(new HttpMethod("GET"), $"https://gorzdrav.spb.ru/_api/api/v2/schedule/lpu/{lpuId}/speciality/{specialtyId}/doctors"))
            {
                var response = httpClient.SendAsync(request, cancellationToken).Result;
                Console.WriteLine(await response.Content.ReadAsStringAsync());
                var responseBody = await response.Content.ReadFromJsonAsync<GetDoctor>(cancellationToken: cancellationToken);

                success = responseBody.success;
                if(responseBody.result != null)
                {
                    result.AddRange(responseBody!.result);
                }
            }
        }

        return result;
    }

    public async Task<IEnumerable<LPUResult>> GetLPUs(Guid profileId, CancellationToken cancellationToken)
    {
        var result = new List<LPUResult>();
        var profile = await profileService.GetProfileByIdAsync(profileId, cancellationToken);
        using (var request = new HttpRequestMessage(new HttpMethod("GET"), $"https://gorzdrav.spb.ru/_api/api/v2/oms/attachment/lpus?polisN={profile.OMS}"))
        {
            var response = httpClient.SendAsync(request, cancellationToken).Result;
            var responseBody = await response.Content.ReadFromJsonAsync<GetLPUsByOMS>(cancellationToken: cancellationToken);
            result.AddRange(responseBody!.result);
        }

        return result;
    }

    public async Task<GetPatient> GetPatient(Guid profileId, int lpuId, CancellationToken cancellationToken)
    {
        using(var db = new AppointmentContext())
        {
            var profile = db.Profiles.First(x => x.Id == profileId);
            var content = new FindAPatient
            {
                lpuId = lpuId,
                firstName = profile.Name,
                lastName = profile.Surname,
                middleName = profile.Patronomyc,
                birthdate = profile.Birthdate.Value.ToShortDateString(),

            };

            bool success = false;
            GetPatient responseBody = new();
            var link = $"https://gorzdrav.spb.ru/_api/api/v2/patient/search?lpuId={lpuId}&lastName={profile.Surname}&firstName={profile.Name}&middleName={profile.Patronomyc}&birthdate={profile.Birthdate.Value.ToString("s")}";
            while (!success)
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, link))
                {
                    var response = httpClient.SendAsync(request, cancellationToken).Result;

                    Console.WriteLine(await response.Content.ReadAsStringAsync());

                    responseBody = await response.Content.ReadFromJsonAsync<GetPatient>(cancellationToken: cancellationToken);

                    success = responseBody.success;
                }
            }
            return responseBody;

        }
    }

    public async Task<IEnumerable<SpecialtiesResult>> GetSpecialties(int lpuId, CancellationToken cancellationToken)
    {
        var result = new List<SpecialtiesResult>();
        using (var request = new HttpRequestMessage(new HttpMethod("GET"), $"https://gorzdrav.spb.ru/_api/api/v2/schedule/lpu/{lpuId}/specialties"))
        {
            var response = httpClient.SendAsync(request, cancellationToken).Result;
            var responseBody = await response.Content.ReadFromJsonAsync<GetSpecialties>(cancellationToken: cancellationToken);
            result.AddRange(responseBody!.result);
        }

        return result;
    }

    public async Task<GetTimetable> GetTimetable(int lpuId, int doctorId, CancellationToken cancellationToken)
    {
        using (var request = new HttpRequestMessage(new HttpMethod("GET"), $"https://gorzdrav.spb.ru/_api/api/v2/schedule/lpu/{lpuId}/doctor/{doctorId}/timetable"))
        {
            var response = httpClient.SendAsync(request, cancellationToken).Result;
            var responseBody = await response.Content.ReadFromJsonAsync<GetTimetable>(cancellationToken: cancellationToken);
           
            return responseBody!;
        }
    }

    public async Task<GetVisits> GetVisits(string patientId, int lpuId, CancellationToken cancellationToken)
    {
        bool success = false;
        GetVisits responseBody = new();

        while (!success)
        {
            using (var request = new HttpRequestMessage(new HttpMethod("GET"), $"https://gorzdrav.spb.ru/_api/api/v2/appointments?lpuId={lpuId}&patientId={patientId}"))
            {
                {
                    var response = httpClient.SendAsync(request, cancellationToken).Result;
                    Console.WriteLine(await response.Content.ReadAsStringAsync());
                    responseBody = await response.Content.ReadFromJsonAsync<GetVisits>(cancellationToken: cancellationToken);

                    success = responseBody.success;
                }
            }
        }
        return responseBody;

    }
}