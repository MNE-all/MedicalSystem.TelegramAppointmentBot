using System.Net.Http.Json;
using TelegramAppointmentBot.Context.Models;
using TelegramAppointmentBot.Context.Models.Request;
using TelegramAppointmentBot.Context.Models.Response;
using TelegramAppointmentBot.Service.Contract.Interfaces;

namespace TelegramAppointmentBot.Service.Implementation;

public class GorzdravService : IGorzdravService
{
    private readonly IProfileService profileService = new ProfileService();
    private readonly HttpClient httpClient = new HttpClient();

    public Task CreateAppointment(CreateAnAppointment model, CancellationToken cancellationToken)
    {
        // TODO Создание записи
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<DoctorResult>> GetDoctors(int lpuId, int specialtyId, CancellationToken cancellationToken)
    {
        var result = new List<DoctorResult>();
        using (var request = new HttpRequestMessage(new HttpMethod("GET"), $"https://gorzdrav.spb.ru/_api/api/v2/schedule/lpu/{lpuId}/speciality/{specialtyId}/doctors"))
        {
            var response = httpClient.SendAsync(request, cancellationToken).Result;
            var responseBody = await response.Content.ReadFromJsonAsync<GetDoctor>(cancellationToken: cancellationToken);
            result.AddRange(responseBody!.result);
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
            var x = await response.Content.ReadAsStringAsync(cancellationToken);
            Console.WriteLine(x);
            var responseBody = await response.Content.ReadFromJsonAsync<GetTimetable>(cancellationToken: cancellationToken);
           
            return responseBody!;
        }
    }
}