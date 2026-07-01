// using System.Collections.Concurrent;


// /*
// Descriere: Băncile impun obținerea unui "token de autentificare" extern înainte de procesarea plăților. 
// Acest token este valid o perioadă scurtă (ex: 5 minute).
// Pentru a nu face un request extern la fiecare plată, trebuie să salvezi token-ul in-memory, 
// dar să te asiguri că nu este returnat niciodată după ce a expirat.
// */

// /// Input/Output:
// /// void Set(string key, string value, TimeSpan ttl)
// /// string Get(string key) – returnează valoarea dacă există și NU a expirat, altfel returnează null.

// /// Constrangeri:
// /// O(1) pentru Get și Set.
// /// Curățare (Eviction): Memoria nu trebuie să crească la infinit dacă se inserează milioane de chei care nu mai sunt accesate niciodată.
// /// Thread-safety este obligatoriu.

// /// Edge Cases:
// /// Get apelat exact în milisecunda în care expiră TTL-ul.
// /// Actualizarea unei chei existente cu o nouă valoare și un nou TTL (resetează timer-ul).
// /// Cum implementezi curățarea cheilor "orfeine": un background task (Timer) versus o strategie pasivă (lazy eviction + limită superioară de memorie)?

// /// Exemplu:
// // Set("bank_token", "abc-123", TimeSpan.FromSeconds(2));
// // Get("bank_token"); // "abc-123"
// // Thread.Sleep(3000);
// // Get("bank_token"); // null (a expirat)

// class TokenAuthenthicationService
// {
//     private record Entry (string Value, TimeSpan TTL);
//     private readonly ConcurrentDictionary<string, Entry> tokenMemory = new();
//     private TimeSpan _expirationTimeSpan;

//     public TokenAuthenthicationService(TimeSpan expirationTimeSpan)
//     {
//         _expirationTimeSpan = expirationTimeSpan;
//     }
//     public void Set(string key, string value, TimeSpan ttl)
//     {
//         tokenMemory.AddOrUpdate(key, new Entry(value, ttl), (key, existingEntry) => existingEntry with { TTL = TimeSpan.Zero });
//     }

//     public string Get(string key)
//     {
//         if (!tokenMemory.ContainsKey(key)) return null;
//         var token = tokenMemory[key];
//         if (token.TTL > _expirationTimeSpan)
//         {
//             tokenMemory.Remove(key, out var value);
//             return null;
//         } 

//         return token.Value;
//         // returnează valoarea dacă există și NU a expirat, altfel returnează null.
//     }
// }

// class Program
// {
    
//     static void Main(string[] args)
//     {
//         const int EXPIRATION_TIME = 2;
//         var tokenAuthService = new TokenAuthenthicationService(TimeSpan.FromSeconds(EXPIRATION_TIME));

//         tokenAuthService.Set("bank_token", "abc-123", TimeSpan.FromSeconds(2));
//         Console.WriteLine(tokenAuthService.Get("bank_token")); // "abc-123"
//         Thread.Sleep(3000);
//         Console.WriteLine(tokenAuthService.Get("bank_token")); // null (a expirat)
//     }
// }