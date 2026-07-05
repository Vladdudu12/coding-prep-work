/*
Scrie o funcție async care apelează un API extern (ex: ANAF) și reîncearcă până la 3 ori cu backoff exponențial, respectând un CancellationToken.
*/



// metoda async care apeleaza un api
// await raspuns de la api
// daca am primit cancellation token 
// verificam daca este cancellation request 
// asa ca dam retry cu apelul de 3 ori. 
// daca tot nu am primit un raspuns pozitiv
// timeout cateva secunde initial
// reincercam iar de 3 ori
// and so on pana cand functioneaza. 

using System.Net;


/// Testează: async/await corect, CancellationToken propagat prin toate nivelurile, Task.Delay
/// Hint: verifică token.ThrowIfCancellationRequested() la fiecare iterație, nu doar la început
public class ApelExternCuRetry
{
    private readonly HttpClient _httpClient;
    private readonly Random _random = new();

    public ApelExternCuRetry(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<HttpResponseMessage> ApelCuRetryAsync(string url, int maxIncercari = 3, CancellationToken cancellationToken = default)
    {
        var delay = TimeSpan.FromSeconds(1);

        for (int incercare = 1; incercare <= maxIncercari; incercare++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var raspuns = await _httpClient.GetAsync(url, cancellationToken);

                if (EsteEroareTranzienta(raspuns.StatusCode))
                {
                    if (incercare == maxIncercari)
                    {
                        raspuns.EnsureSuccessStatusCode();
                    }

                    await AsteaptaCuJitter(delay, cancellationToken);
                    delay *= 2;
                    continue;
                }

                raspuns.EnsureSuccessStatusCode();
                return raspuns;
            }
            catch (HttpRequestException) when (incercare < maxIncercari)
            {
                await AsteaptaCuJitter(delay, cancellationToken);
                delay *= 2;
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                if (incercare == maxIncercari) throw;
                await AsteaptaCuJitter(delay, cancellationToken);
                delay *= 2;
            }
        }

        throw new InvalidOperationException("Toate incercarile au esuat");
    }

    private static bool EsteEroareTranzienta(HttpStatusCode statusCode)
    {
        return statusCode == HttpStatusCode.ServiceUnavailable // 503
        || statusCode == HttpStatusCode.BadGateway // 502
        || statusCode == HttpStatusCode.GatewayTimeout // 504
        || statusCode == HttpStatusCode.TooManyRequests; // 429
    }

    private async Task AsteaptaCuJitter(TimeSpan delay, CancellationToken cancellationToken)
    {
        var jitterMs = _random.Next(0, (int)(delay.TotalMilliseconds * 0.1) + 1);
        var delayCuJitter = delay + TimeSpan.FromMilliseconds(jitterMs);

        await Task.Delay(delayCuJitter, cancellationToken);
    }
}

class Program
{
    
    static async Task Main(string[] args)
    {
        using var httpClient = new HttpClient();
        var apelator = new ApelExternCuRetry(httpClient);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        try
        {
            var raspuns = await apelator.ApelCuRetryAsync(
                "https://exemplu-api-extern.ro/status",
                maxIncercari: 3,
                cancellationToken: cts.Token
            );

            Console.WriteLine($"Succes: {raspuns.StatusCode}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Apelul a fost anulat (timeout global sau cancellation token)");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Toate incercarile au esuat: {ex.Message}");
        }
    }
}