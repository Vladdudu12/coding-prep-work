/*
Doi utilizatori (contabil + PFA) deschid aceeași factură simultan și editează. Al doilea salvează suprascriind modificările primului. Cum previi asta?
*/


using System.Collections.Concurrent;


/// Testează: optimistic locking (versiune/timestamp pe rând, verificat la UPDATE) vs pessimistic locking (lock explicit) — discută trade-off
/// Hint: WHERE Id = @id AND Version = @versiuneCitita la UPDATE; dacă 0 rânduri afectate → conflict, arată UI de reconciliere
public record Factura(Guid Id, string ClientId, decimal Suma, int Versiune);

public class ConcurrencyException: Exception
{
    public ConcurrencyException(string message) : base(message) { }
}

public class FacturaStoreOptimistic
{
    private readonly Dictionary<Guid, Factura> _store = new();
    private readonly object _lock = new();

    public void Adauga(Factura factura)
    {
        lock (_lock) { _store[factura.Id] = factura; }
    }

    public Factura Get(Guid id)
    {
        lock (_lock) { return _store[id]; }
    }

    public void Salveaza(Guid id, Func<Factura, Factura> aplicaModificari, int versiuneCitita)
    {
        lock (_lock)
        {
            var curent = _store[id];
            if (curent.Versiune != versiuneCitita)
            {
                throw new ConcurrencyException(
                    $"Factura {id} a fost modificata de altcineva intre timp " +
                    $"(versiune curenta: {curent.Versiune}, versiune citita: {versiuneCitita})"
                );
            }

            var modificat = aplicaModificari(curent) with { Versiune = curent.Versiune + 1};
            _store[id] = modificat;
        }
    }
}

public class FacturaStorePessimistic
{
    private readonly Dictionary<Guid, Factura> _store = new();
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _lockuriPerFactura = new();

    public void Adauga(Factura factura) => _store[factura.Id] = factura;

    private SemaphoreSlim GetLock(Guid id) => _lockuriPerFactura.GetOrAdd(id, _ => new SemaphoreSlim(1, 1));

    public async Task<FacturaEditareSesiune> DeschideEditareAsync(Guid id, TimeSpan timeout)
    {
        var sem = GetLock(id);
        var obtinut = await sem.WaitAsync(timeout);
        if (!obtinut)
            throw new TimeoutException($"Factura {id} este deja editata de altcineva, incearca din nou mai tarziu");
        
        return new FacturaEditareSesiune(this, id, sem);
    }

    internal Factura GetIntern(Guid id) => _store[id];
    internal void SalveazaIntern(Guid id, Factura noua) => _store[id] = noua;
}

public class FacturaEditareSesiune : IDisposable
{
    private readonly FacturaStorePessimistic _store;
    private readonly Guid _id;
    private readonly SemaphoreSlim _sem;
    private bool _eliberat;

    internal FacturaEditareSesiune(FacturaStorePessimistic store, Guid id, SemaphoreSlim sem)
    {
        _store = store;
        _id = id;
        _sem = sem;
    }

    public Factura Factura => _store.GetIntern(_id);

    public void Salveaza(Func<Factura, Factura> aplicaModificari) => _store.SalveazaIntern(_id, aplicaModificari(_store.GetIntern(_id)));

    public void Dispose()
    {
        if (!_eliberat)
        {
            _sem.Release();
            _eliberat = true;
        }
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=====OPTIMISTIC APPROACH=====");
        // bun pentru conflicte rare pentru ca te blocheaza la salvare 

        var storeOptimistic = new FacturaStoreOptimistic();
        var facturaId = Guid.NewGuid();
        storeOptimistic.Adauga(new Factura(facturaId, "C1", 1000m, Versiune: 0));

        var facturaContabil = storeOptimistic.Get(facturaId);
        var facturaPfa = storeOptimistic.Get(facturaId);

        storeOptimistic.Salveaza(facturaId, f => f with { Suma = 1200m }, versiuneCitita: facturaContabil.Versiune);
        Console.WriteLine("Contabilul a salvat cu succes");

        try
        {
            storeOptimistic.Salveaza(facturaId, f => f with { Suma = 999m }, versiuneCitita: facturaPfa.Versiune);
        }
        catch (ConcurrencyException ex)
        {
            Console.WriteLine($"PFA-ul primeste conflict: {ex.Message}");
        }


        Console.WriteLine("=====PESSIMISTIC APPROACH=====");
        // bun pentru conflicte frecvente pentru ca te blocheaza la deschidere
        var storePessimistic = new FacturaStorePessimistic();

        storePessimistic.Adauga(new Factura(facturaId, "C1", 1000m, 0));

        using (var sesiuneContabil = await storePessimistic.DeschideEditareAsync(facturaId, TimeSpan.FromSeconds(2)))
        {
            Console.WriteLine("Contabilul a deschis factura pentru editare (lock obtinut)");

            try
            {
                using var sesiunePfa = await storePessimistic.DeschideEditareAsync(facturaId, TimeSpan.FromSeconds(2));
            }
            catch (TimeoutException ex)
            {
                Console.WriteLine($"PFA-ul este blocat: {ex.Message}");
            }

            sesiuneContabil.Salveaza(f => f with { Suma = 1200m });
            Console.WriteLine("Contabilul a salvat; lock-ul se elibereaza la Dispose.");
        }

        using var sesiunePfa2 = await storePessimistic.DeschideEditareAsync(facturaId, TimeSpan.FromSeconds(2));
        Console.WriteLine($"PFA-ul vede acum suma actualizata: {sesiunePfa2.Factura.Suma}");
    }
}