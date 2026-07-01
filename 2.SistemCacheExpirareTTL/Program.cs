using System.Collections.Concurrent;


/*
Descriere: Băncile impun obținerea unui "token de autentificare" extern înainte de procesarea plăților. 
Acest token este valid o perioadă scurtă (ex: 5 minute).
Pentru a nu face un request extern la fiecare plată, trebuie să salvezi token-ul in-memory, 
dar să te asiguri că nu este returnat niciodată după ce a expirat.
*/

/// Input/Output:
/// void Set(string key, string value, TimeSpan ttl)
/// string Get(string key) – returnează valoarea dacă există și NU a expirat, altfel returnează null.

/// Constrangeri:
/// O(1) pentru Get și Set.
/// Curățare (Eviction): Memoria nu trebuie să crească la infinit dacă se inserează milioane de chei care nu mai sunt accesate niciodată.
/// Thread-safety este obligatoriu.

/// Edge Cases:
/// Get apelat exact în milisecunda în care expiră TTL-ul.
/// Actualizarea unei chei existente cu o nouă valoare și un nou TTL (resetează timer-ul).
/// Cum implementezi curățarea cheilor "orfeine": un background task (Timer) versus o strategie pasivă (lazy eviction + limită superioară de memorie)?

/// Exemplu:
// Set("bank_token", "abc-123", TimeSpan.FromSeconds(2));
// Get("bank_token"); // "abc-123"
// Thread.Sleep(3000);
// Get("bank_token"); // null (a expirat)


class TtlCache
{
    private record Entry(string Value, DateTime ExpiresAtUtc);
    
    private readonly ConcurrentDictionary<string, Entry> _store = new();
    private readonly Timer _cleanupTimer;

    public TtlCache(TimeSpan? cleanupInterval = null)
    {
        var interval = cleanupInterval ?? TimeSpan.FromSeconds(30);
        _cleanupTimer = new Timer(_ => CleanupExpired(), null, interval, interval);
    }

    public void Set(string key, string value, TimeSpan ttl)
    {
        var entry = new Entry(value, DateTime.UtcNow + ttl);
        _store[key] = entry;
    }

    public string Get(string key)
    {
        if (!_store.TryGetValue(key, out var entry))
        {
            return null;
        }

        if (DateTime.UtcNow >= entry.ExpiresAtUtc)
        {
            _store.TryRemove(key, out _);
            return null;
        }

        return entry.Value;
    }

    private void CleanupExpired()
    {
        var now = DateTime.UtcNow;
        foreach (var kvp in _store)
        {
            if (now >= kvp.Value.ExpiresAtUtc)
            {
                _store.TryRemove(kvp.Key, out _);
            }
        }
    }
    
}

class Program
{
    
    static void Main(string[] args)
    {
        const int EXPIRATION_TIME = 2;
        var ttlCache = new TtlCache(TimeSpan.FromSeconds(EXPIRATION_TIME));

        ttlCache.Set("bank_token", "abc-123", TimeSpan.FromSeconds(2));
        Console.WriteLine(ttlCache.Get("bank_token")); // "abc-123"
        Thread.Sleep(3000);
        Console.WriteLine(ttlCache.Get("bank_token")); // null (a expirat)
    }
}