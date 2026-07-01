/*
Descriere: Ai un API pentru verificarea IBAN-urilor, 
limitat la 100 de cereri pe minut pentru fiecare client. 
O fereastră fixă înseamnă că minutele sunt definite pe ceas (ex: 10:00:00 - 10:00:59, apoi 10:01:00 - 10:01:59).
*/

/// Input/Output:
/// bool AllowRequest(string clientId)
/// Returnează true și incrementează contorul dacă clientul nu a depășit limita în minutul curent, altfel false.
/// Constrangeri:
/// Acces concurent: dacă contorul este la 99 și vin 3 request-uri paralele, doar unul trebuie să primească true.
/// Edge Cases:
/// Tranziția la minutul următor: cum resetezi contoarele fără a bloca toate firele de execuție din API?
/// Fără stocare în baze de date — totul e în memorie, cu ConcurrentDictionary și clase interlocked.

/// Exemplu
// // Presupunem limită: 2 pe minut, ora curentă: 10:00:45
// AllowRequest("client-1"); // true
// AllowRequest("client-1"); // true
// AllowRequest("client-1"); // false (limită atinsă)

// // Ora devine 10:01:00 (reset)
// AllowRequest("client-1"); // true 



// limita 100 cereri pe minut
// timer de 1 minut dupa care se reseteaza

using System.Collections.Concurrent;

public class RateLimiter
{
    private int _maxRequests;
    private readonly ConcurrentDictionary<string, ClientCounter> _store = new();

    private readonly Func<DateTime> _clock;

    public RateLimiter(int maxRequests, Func<DateTime>? clock = null)
    {
        _maxRequests = maxRequests;
        _clock = clock ?? (() => DateTime.UtcNow);
    }

    public bool AllowRequest(string clientId)
    {
        var currentWindow = GetWindowKey(_clock());

        var counter = _store.AddOrUpdate(
            clientId,
            addValueFactory: _ => new ClientCounter(currentWindow, 1),
            updateValueFactory: (_, existing) => existing.Increment(currentWindow)
        );

        return counter.Count <= _maxRequests;
    }

    private static long GetWindowKey(DateTime timestamp) => timestamp.Ticks / TimeSpan.TicksPerMinute;

    private sealed class ClientCounter
    {
        public long WindowKey { get; }
        public int Count { get; }

        public ClientCounter(long windowKey, int count)
        {
            WindowKey = windowKey;
            Count = count;
        }

        public ClientCounter Increment(long currentWindow)
        {
            return WindowKey == currentWindow
                ? new ClientCounter(currentWindow, Count + 1)
                : new ClientCounter(currentWindow, 1);
        }
    }
}


class Program
{
    static void Main(string[] args)
    {
        var currentTime = new DateTime(2025, 1, 1, 10, 0, 45, DateTimeKind.Utc);
        var limiter = new RateLimiter(2, () => currentTime);

        Console.WriteLine(limiter.AllowRequest("client-1")); // true
        Console.WriteLine(limiter.AllowRequest("client-1")); // true
        Console.WriteLine(limiter.AllowRequest("client-1")); // false (limita atinsa)

        currentTime = new DateTime(2025, 1, 1, 10, 1, 0, DateTimeKind.Utc); // trece minutul
        Console.WriteLine(limiter.AllowRequest("client-1")); // true (reset)

        var fixedTime = new DateTime(2025, 1, 1, 10, 2, 0, DateTimeKind.Utc);
        var limiter2 = new RateLimiter(100, () => fixedTime);
        var results = new ConcurrentBag<bool>();

        Parallel.For(0, 103, _ =>
        {
            results.Add(limiter2.AllowRequest("client-1"));
        });

        Console.WriteLine(results.Count(r => r)); // trebuie sa fie exact 100
    }
}