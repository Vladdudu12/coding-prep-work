// /*
// Descriere: O aplicație bancară permite unui utilizator să fie autentificat (logat) 
// de pe maxim 3 dispozitive activesimultan (ex: Telefon, Laptop, Tabletă). 
// Dacă utilizatorul se autentifică de pe al 4-lea dispozitiv, 
// cea mai veche sesiune a sa trebuie invalidată (ștearsă) automat, făcând loc celei noi.
// */

// /// Input/Output:
// /// void Login(string userId, string deviceId, DateTime timestamp)
// /// bool IsSessionValid(string userId, string deviceId)


// /// Constrangeri:
// /// API-ul primește mii de logări pe secundă de la utilizatori diferiți.
// /// Folosirea unui singur lock (globalObject) va gâtui (bottleneck) întregul sistem, deci performanța la nivel macro va suferi masiv.
// /// Nu poți bloca UserB în timp ce actualizezi sesiunile lui UserA.
// /// Un utilizator poate da click de multiple ori la login: atenție la logări simultane pe același userId (chiar și cu același deviceId).


// /// Edge Cases:
// /// Granular Locking: Cum eviți lock-ul global? Structura ideală este un ConcurrentDictionary<string, UserSessions>, 
// /// unde clasa internă UserSessions are propriul său lock restrâns, permițând utilizatorilor diferiți să se logheze perfect în paralel.
// /// Un apel de Login de pe un dispozitiv deja logat: ar trebui doar să îi facă "touch" 
// /// (să îi actualizeze timestamp-ul la cel mai recent, modificând ordinea de expirare).


// /// Exemplu:
// // Login("user_1", "iphone", DateTime.Parse("10:00"));
// // Login("user_1", "macbook", DateTime.Parse("10:05"));
// // Login("user_1", "ipad", DateTime.Parse("10:10"));

// // IsSessionValid("user_1", "iphone"); // true

// // // A 4-a logare (trebuie să invalideze iphone-ul, fiind cel mai vechi)
// // Login("user_1", "windows_pc", DateTime.Parse("10:15"));

// // IsSessionValid("user_1", "iphone"); // false
// // IsSessionValid("user_1", "windows_pc"); // true

// using System.Collections.Concurrent;


// class SessionStorage
// {
//     private readonly ConcurrentDictionary<string, Queue<Session>> _store = new();

//     public void Login(string userId, string deviceId, DateTime timestamp)
//     {
//         var queue = new Queue<Session>();
//         queue.Enqueue(new Session(deviceId, timestamp));
//         var sessions = _store.GetOrAdd(userId, queue);

//         Session? existingSession = sessions.FirstOrDefault(x => x.DeviceId == deviceId);
//         if (existingSession != null)
//         {
//             existingSession.AttemptLogin(deviceId, timestamp);
//         }
//         else
//         {
//             if(sessions.Count() >= 3)
//             {
//                 sessions.Dequeue();
//             }
//             sessions.Enqueue(new Session(deviceId, timestamp));
//         }


//     }

//     public bool IsSessionValid(string userId, string deviceId)
//     {
//         // in dictionar avem key - userId
//         // la fiecare userId avem un queue prin care trebuie sa cautam sa vedem daca exista un Session cu deviceId cerut
//         foreach (var kvp in _store)
//         {
//             Console.WriteLine($"key:{kvp.Key}");
//             foreach (var session in kvp.Value)
//             {
//                 Console.WriteLine($"{session}");
//             }
//         }
//         if (!_store.TryGetValue(userId, out Queue<Session> sessions)) return false;
//         if (sessions.Count <= 0) return false;

//         var result = sessions.Count(x => x.DeviceId == deviceId);
//         return result > 0;
//     }

//     private sealed class Session
//     {
//         public string DeviceId { get; }
//         private DateTime Timestamp { get; set; }
//         private object _lock;

//         public Session(string deviceId, DateTime timestamp)
//         {
//             DeviceId = deviceId;
//             Timestamp = timestamp;
//             _lock = new object();
//         }

//         public void AttemptLogin(string deviceId, DateTime timestamp)
//         {
//             lock (_lock)
//             {
//                 if (deviceId == DeviceId)
//                 {
//                     Timestamp = timestamp;
//                 }
//             }
//         }

//         public override string ToString()
//         {
//             return $"DeviceId: {DeviceId} | Timestamp: {Timestamp}";
//         }

//     };
// }

// class Program
// {
//     static void Main(string[] args)
//     {
//         var sessionStorage = new SessionStorage();

//         sessionStorage.Login("user_1", "iphone", DateTime.Parse("10:00"));
//         sessionStorage.Login("user_1", "macbook", DateTime.Parse("10:05"));
//         sessionStorage.Login("user_1", "ipad", DateTime.Parse("10:10"));

//         Console.WriteLine(sessionStorage.IsSessionValid("user_1", "iphone")); // true
//         // A 4-a logare (trebuie să invalideze iphone-ul, fiind cel mai vechi)
//         sessionStorage.Login("user_1", "windows_pc", DateTime.Parse("10:15"));

//         Console.WriteLine(sessionStorage.IsSessionValid("user_1", "iphone")); // false
//         Console.WriteLine(sessionStorage.IsSessionValid("user_1", "windows_pc")); // true
//     }
// }