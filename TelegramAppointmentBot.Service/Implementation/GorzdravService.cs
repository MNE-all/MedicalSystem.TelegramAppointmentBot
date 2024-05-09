using System.Net.Http.Json;
using TelegramAppointmentBot.Context.Models.Response;
using TelegramAppointmentBot.Service.Contract.Interfaces;

namespace TelegramAppointmentBot.Service.Implementation;

public class GorzdravService : IGorzdravService
{
    private readonly HttpClient httpClient = new HttpClient();

    public async Task<IEnumerable<LPUResult>> GetLPUs(CancellationToken cancellationToken)
    {
        using (var request = new HttpRequestMessage(new HttpMethod("GET"), "https://api.local/tables?tableName=myTable"))
        {
            request.Headers.TryAddWithoutValidation("X-Auth-Token", "LocalTocken");
            request.Headers.TryAddWithoutValidation("X-User-Id", "LocalID");    
            var response = httpClient.SendAsync(request, cancellationToken).Result;
            Console.WriteLine(response.Content.ReadFromJsonAsync<GetLPUsByOMS>(cancellationToken: cancellationToken));
        }

        return new List<LPUResult>();
    }
}