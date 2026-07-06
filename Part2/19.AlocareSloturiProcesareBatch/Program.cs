/*
Ai N clienți care trebuie să primească un raport lunar generat, dar generarea e costisitoare (CPU) și ai doar M workeri disponibili într-o fereastră de timp limitată. Cum programezi procesarea?
*/


using System.Collections.Concurrent;


/// Testează: gândire de scheduling/greedy, prioritizare (ex: clienți cu deadline mai apropiat primii)
/// Hint: coadă de priorități (PriorityQueue<T> din .NET) sau simplă coadă FIFO cu N workeri paraleli — discută ce se întâmplă dacă un job eșuează (retry, dead-letter)
public record RaportJob(string ClientId, DateTime Deadline, TimeSpan CostEstimat);

public class ProgramatorRapoarte
{
    private readonly int _numarWorkeri;
    private readonly SemaphoreSlim _sloturi;

    public ProgramatorRapoarte(int numarWorkeri)
    {
        _numarWorkeri = numarWorkeri;
        _sloturi = new SemaphoreSlim(numarWorkeri, numarWorkeri);
    }

    public async Task<RezultatProcesare> ProcesazaAsync(
        List<RaportJob> joburi,
        Func<RaportJob, CancellationToken, Task> genereazaRaport,
        int maxIncercari = 2,
        CancellationToken cancellationToken = default)
    {
        var joburiOrdonate = joburi.OrderBy(j => j.Deadline).ToList();

        var reusite = new ConcurrentBag<string>();
        var esuate = new ConcurrentBag<(string ClientId, string Motiv)>();

        var taskuri = joburiOrdonate.Select(async job =>
        {
            await _sloturi.WaitAsync(cancellationToken);
            try
            {
                await ProceseazaCuRetryAsync(job, genereazaRaport, maxIncercari, cancellationToken);
                reusite.Add(job.ClientId);
            }
            catch (Exception ex)
            {
                esuate.Add((job.ClientId, ex.Message));
            }
            finally
            {
                _sloturi.Release();
            }
        });

        await Task.WhenAll(taskuri);

        return new RezultatProcesare(reusite.ToList(), esuate.ToList());
    }

    private async Task ProceseazaCuRetryAsync(
        RaportJob job,
        Func<RaportJob, CancellationToken, Task> genereazaRaport,
        int maxIncercari,
        CancellationToken cancellationToken)
    {
        for (int incercare = 1; incercare <= maxIncercari; incercare++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (DateTime.UtcNow > job.Deadline)
            {
                throw new TimeoutException($"Deadline depasit pentru {job.ClientId}");
            }

            try
            {
                await genereazaRaport(job, cancellationToken);
                return;
            }
            catch when (incercare < maxIncercari)
            {
                await Task.Delay(TimeSpan.FromSeconds(2 * incercare), cancellationToken);
            }
        }
    }
}

public record RezultatProcesare(List<string> ClientiProcesati, List<(string ClientId, string Motiv)> ClientiEsuati);

class Program
{
    static async Task Main(string[] args)
    {
        var joburi = new List<RaportJob>
        {
            new("PFA-001", DateTime.UtcNow.AddSeconds(5), TimeSpan.FromSeconds(1)),
            new("PFA-002", DateTime.UtcNow.AddSeconds(2), TimeSpan.FromSeconds(1)), // deadline mai urgent
            new("PFA-003", DateTime.UtcNow.AddSeconds(8), TimeSpan.FromSeconds(1)),
            new("PFA-004", DateTime.UtcNow.AddSeconds(1), TimeSpan.FromSeconds(3)), // foarte urgent, dar mai lent
        };

        var programator = new ProgramatorRapoarte(numarWorkeri: 2);

        var rezultat = await programator.ProcesazaAsync(joburi, async (job, token) =>
        {
            Console.WriteLine($"Procesez {job.ClientId} (deadline: {job.Deadline:HH:mm:ss})");
            await Task.Delay(job.CostEstimat, token);
            Console.WriteLine($"Gata {job.ClientId}");
        });

        Console.WriteLine($"Reușite: {string.Join(", ", rezultat.ClientiProcesati)}");
        Console.WriteLine($"Eșuate: {string.Join(", ", rezultat.ClientiEsuati.Select(e => e.ClientId))}");
    }
}