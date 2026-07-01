/*
Descriere: O aplicație bancară permite unui utilizator să fie autentificat (logat) 
de pe maxim 3 dispozitive activesimultan (ex: Telefon, Laptop, Tabletă). 
Dacă utilizatorul se autentifică de pe al 4-lea dispozitiv, 
cea mai veche sesiune a sa trebuie invalidată (ștearsă) automat, făcând loc celei noi.
*/

/// Input/Output:
/// void Login(string userId, string deviceId, DateTime timestamp)
/// bool IsSessionValid(string userId, string deviceId)


/// Constrangeri:
/// API-ul primește mii de logări pe secundă de la utilizatori diferiți.
/// Folosirea unui singur lock (globalObject) va gâtui (bottleneck) întregul sistem, deci performanța la nivel macro va suferi masiv.
/// Nu poți bloca UserB în timp ce actualizezi sesiunile lui UserA.
/// Un utilizator poate da click de multiple ori la login: atenție la logări simultane pe același userId (chiar și cu același deviceId).


/// Edge Cases:
/// Granular Locking: Cum eviți lock-ul global? Structura ideală este un ConcurrentDictionary<string, UserSessions>, 
/// unde clasa internă UserSessions are propriul său lock restrâns, permițând utilizatorilor diferiți să se logheze perfect în paralel.
/// Un apel de Login de pe un dispozitiv deja logat: ar trebui doar să îi facă "touch" 
/// (să îi actualizeze timestamp-ul la cel mai recent, modificând ordinea de expirare).


/// Exemplu:
// Login("user_1", "iphone", DateTime.Parse("10:00"));
// Login("user_1", "macbook", DateTime.Parse("10:05"));
// Login("user_1", "ipad", DateTime.Parse("10:10"));

// IsSessionValid("user_1", "iphone"); // true

// // A 4-a logare (trebuie să invalideze iphone-ul, fiind cel mai vechi)
// Login("user_1", "windows_pc", DateTime.Parse("10:15"));

// IsSessionValid("user_1", "iphone"); // false
// IsSessionValid("user_1", "windows_pc"); // true

using System.Collections.Concurrent;


class SessionStorage
{
    private readonly ConcurrentDictionary<string, UserSessions> _store = new();

    public void Login(string userId, string deviceId, DateTime timestamp)
    {
        var sessions = _store.GetOrAdd(userId, _ => new UserSessions());
        sessions.Login(deviceId, timestamp);
    }

    public bool IsSessionValid(string userId, string deviceId)
    {
        return _store.TryGetValue(userId, out var sessions) && sessions.IsValid(deviceId);
    }

    private sealed class UserSessions
    {
        private const int MaxDevices = 3;

        // _order: cel mai vechi la Front, cel mai recent la Last
        // _byDevice: lookup O(1) dupa deviceId -> nodul din lista
        private readonly LinkedList<Session> _order = new();
        private readonly Dictionary<string, LinkedListNode<Session>> _byDevice = new();
        private readonly object _lock = new();

        public void Login(string deviceId, DateTime timestamp)
        {
            lock (_lock)
            {
                if (_byDevice.TryGetValue(deviceId, out var existingNode))
                {
                    // Touch: actualizam timestamp si mutam nodul la finalul listei
                    // astfel incat sa nu mai fie candidat la evacuare ca fiind cel mai vechi
                    existingNode.Value.Timestamp = timestamp;
                    _order.Remove(existingNode);
                    _order.AddLast(existingNode);
                    return;
                }

                if (_order.Count >= MaxDevices)
                {
                    var oldest = _order.First!;
                    _order.RemoveFirst();
                    _byDevice.Remove(oldest.Value.DeviceId);
                }

                var newNode = new LinkedListNode<Session>(new Session(deviceId, timestamp));
                _order.AddLast(newNode);
                _byDevice[deviceId] = newNode;
            }
        }

        public bool IsValid(string deviceId)
        {
            lock (_lock)
            {
                return _byDevice.ContainsKey(deviceId);
            }
        }
    }

    private sealed class Session
    {
        public string DeviceId { get; }
        public DateTime Timestamp { get; set; }

        public Session(string deviceId, DateTime timestamp)
        {
            DeviceId = deviceId;
            Timestamp = timestamp;
        }
    };
}

class Program
{
    static void Main(string[] args)
    {
        var sessionStorage = new SessionStorage();

        sessionStorage.Login("user_1", "iphone", DateTime.Parse("10:00"));
        sessionStorage.Login("user_1", "macbook", DateTime.Parse("10:05"));
        sessionStorage.Login("user_1", "ipad", DateTime.Parse("10:10"));

        Console.WriteLine(sessionStorage.IsSessionValid("user_1", "iphone")); // true
        // A 4-a logare (trebuie să invalideze iphone-ul, fiind cel mai vechi)
        sessionStorage.Login("user_1", "windows_pc", DateTime.Parse("10:15"));

        Console.WriteLine(sessionStorage.IsSessionValid("user_1", "iphone")); // false
        Console.WriteLine(sessionStorage.IsSessionValid("user_1", "windows_pc")); // true
    }
}