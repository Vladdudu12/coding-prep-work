// /*
// Descriere: Ai un API pentru verificarea IBAN-urilor, 
// limitat la 100 de cereri pe minut pentru fiecare client. 
// O fereastră fixă înseamnă că minutele sunt definite pe ceas (ex: 10:00:00 - 10:00:59, apoi 10:01:00 - 10:01:59).
// */

// /// Input/Output:
// /// bool AllowRequest(string clientId)
// /// Returnează true și incrementează contorul dacă clientul nu a depășit limita în minutul curent, altfel false.
// /// Constrangeri:
// /// Acces concurent: dacă contorul este la 99 și vin 3 request-uri paralele, doar unul trebuie să primească true.
// /// Edge Cases:
// /// Tranziția la minutul următor: cum resetezi contoarele fără a bloca toate firele de execuție din API?
// /// Fără stocare în baze de date — totul e în memorie, cu ConcurrentDictionary și clase interlocked.

// /// Exemplu
// // // Presupunem limită: 2 pe minut, ora curentă: 10:00:45
// // AllowRequest("client-1"); // true
// // AllowRequest("client-1"); // true
// // AllowRequest("client-1"); // false (limită atinsă)

// // // Ora devine 10:01:00 (reset)
// // AllowRequest("client-1"); // true 



// // limita 100 cereri pe minut
// // timer de 1 minut dupa care se reseteaza

// using System.Collections.Concurrent;

// public class RateLimiter
// {
//     private Timer _timer;
//     private TimeSpan _window;
//     private int _maxRequests;
//     private readonly ConcurrentDictionary<string, int> _store = new();

//     public RateLimiter(int maxRequests, TimeSpan? window = null)
//     {
//         _maxRequests = maxRequests;
//         _window = window ?? TimeSpan.FromSeconds(60);

//         _timer = new Timer(_ => CleanupCounters(), 0, _window, _window);
//     }
//     public bool AllowRequest(string clientId)
//     {
//         var newValue = _store.AddOrUpdate(
//             clientId,
//             addValueFactory: 1,
//             updateValueFactory: (key, existingValue) => existingValue + 1
//         );

//         return newValue <= _maxRequests;
//     }

//     private void CleanupCounters()
//     {
//         foreach(var kvp in _store)
//         {
//             _store.AddOrUpdate(kvp.Key, 0, (key, value) => value = 0);
//         }
//     }
// }


// class Program
// {
//     static void Main(string[] args)
//     {
//         var rateLimiter = new RateLimiter(2);

//         // Presupunem limită: 2 pe minut, ora curentă: 10:00:45
//        var r1 = rateLimiter.AllowRequest("client-1"); // true
//        var r2 = rateLimiter.AllowRequest("client-1"); // true
//        var r3 = rateLimiter.AllowRequest("client-1"); // false (limită atinsă)

//         // Ora devine 10:01:00 (reset)
//         var r4 = rateLimiter.AllowRequest("client-1"); // true 

//         Console.WriteLine($"{r1}-{r2}-{r3}-{r4}");
//     }
// }