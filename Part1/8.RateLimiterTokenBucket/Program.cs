/*
Descriere: Algoritmul Fixed Window are o problemă: permite de 2x limita la granița dintre minute 
(ex: 100 req la 10:00:59 și 100 la 10:01:01). 
Token Bucket rezolvă asta. 
Fiecare client are o "găleată" cu o capacitate maximă de Cjetoane (tokens).
Găleata se umple treptat la o rată de R jetoane pe secundă. 
Fiecare request consumă 1 jeton. 
Dacă găleata e goală, request-ul e respins.
*/



using System.Collections.Concurrent;


/// Input/Output:
/// Constructor: TokenBucket(int capacity, int refillRatePerSecond)
/// bool TryConsume(string clientId)

/// Constrangeri:
/// Regulă de aur: NU folosi Timer sau thread-uri de background pentru a adăuga jetoane. 
/// Adăugarea (refill) trebuie calculată "on the fly", matematic, 
/// în momentul în care se apelează TryConsume, comparând DateTime.UtcNow cu ultimul acces.

/// Edge Cases:
/// Clientul face request după o oră de pauză (găleata nu trebuie să depășească capacitatea maximă C).
/// Jetoane fracționare (ce faci dacă a trecut 0.5 secunde? Stochezi timpul exact sau valoarea zecimală a token-urilor?).

/// Exemplu:
// var bucket = new TokenBucket(3, 1); // Max 3 tokens, 1 jeton refăcut pe secundă
// bucket.TryConsume("user1"); // true (2 tokens rămași)
// bucket.TryConsume("user1"); // true (1 token rămas)
// bucket.TryConsume("user1"); // true (0 tokens rămași)
// bucket.TryConsume("user1"); // false

// Thread.Sleep(1000); 
// bucket.TryConsume("user1"); // true (s-a generat 1 token nou

/// MODIFICARI REALIZATE: schimbat din int in double pentru tokens pentru a avea fractionar

class TokenBucket
{
    private int _capacity;
    private int _refillRatePerSecond;
    private readonly ConcurrentDictionary<string, (double Tokens, DateTime LastRefill)> _store = new();

    public TokenBucket(int capacity, int refillRatePerSecond)
    {
        _capacity = capacity;
        _refillRatePerSecond = refillRatePerSecond;
    }

    public bool TryConsume(string clientId)
    {
        var now = DateTime.UtcNow;
        bool allowed = false;
        _store.AddOrUpdate(
            clientId,
            addValueFactory: _ =>
            {
                allowed = _capacity >= 1;
                return (allowed ? _capacity - 1 : _capacity, now);
            },
            updateValueFactory: (_, existing) =>
            {
                var elapsedSeconds = (now - existing.LastRefill).TotalSeconds;
                var refilled = Math.Min(_capacity, existing.Tokens + elapsedSeconds * _refillRatePerSecond);

                allowed = refilled >= 1;
                var newTokens = allowed ? refilled - 1 : refilled;

                return (newTokens, now);
            }
            // (key, existing) =>
            //     {
            //         var utcNow = DateTime.UtcNow;
            //         var lastAccess = existing.LastRefill;
            //         var differenceInSeconds = Convert.ToInt32((utcNow - lastAccess).TotalSeconds);
            //         // Console.WriteLine($"differenceInSeconds:{differenceInSeconds}");
            //         var refilledTokens = differenceInSeconds * _refillRatePerSecond;
            //         // Console.WriteLine($"refilledTokens:{refilledTokens}");

            //         var lastCapacity = Convert.ToInt32(Math.Max(0, existing.Tokens));
            //         var refilledCapacity = lastCapacity + refilledTokens;
            //         // Console.WriteLine($"lastCapacity:{lastCapacity}");
            //         // Console.WriteLine($"refilled:{refilledCapacity}");

            //         var currentCapacity = Convert.ToInt32(Math.Min(_capacity, refilledCapacity));
            //         // Console.WriteLine($"currentCapacity:{currentCapacity}");

            //         existing.LastRefill = utcNow;
            //         existing.Tokens = currentCapacity - 1;

            //         return existing;
            //     }
            );

        return allowed;
    }
}

class Program
{
    static void Main(string[] args)
    {
        var bucket = new TokenBucket(3, 1); // Max 3 tokens, 1 jeton refăcut pe secundă
        var r1 = bucket.TryConsume("user1"); // true (2 tokens rămași)
        var r2 = bucket.TryConsume("user1"); // true (1 token rămas)
        var r3 = bucket.TryConsume("user1"); // true (0 tokens rămași)
        var r4 = bucket.TryConsume("user1"); // false

        Thread.Sleep(1000);
        var r5 = bucket.TryConsume("user1"); // true (s-a generat 1 token nou)

        Console.WriteLine($"{r1}-{r2}-{r3}-{r4}-{r5}");
    }
}