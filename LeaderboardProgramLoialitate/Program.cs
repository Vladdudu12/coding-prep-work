/*
Descriere: Aplicația ta oferă puncte de cashback utilizatorilor. 
Pe ecranul principal, vrei să afișezi în timp real un "Leaderboard" cu primii N utilizatori cu cele mai multe puncte. 
Volumul de actualizări ale punctelor este uriaș, iar interogările pentru "Top N" se fac la fiecare încărcare de pagină.
*/

/// Input/Output
/// void AddPoints(string userId, int points) – Adaugă (cumulează) puncte la un utilizator. Poate fi apelată de mii de ori pe secundă.
/// List<string> GetTopUsers(int n) – Returnează ID-urile celor mai buni n utilizatori, ordonați descrescător după punctaj.

/// Constrangeri:
/// Sistemul poate avea milioane de utilizatori unici.
/// O sortare completă a milioanelor de utilizatori la fiecare apel GetTopUsers va distruge performanța (O(U log U) nu este acceptabil).
/// Caută o complexitate cât mai apropiată de O(1) sau O(log U) pentru actualizare și interogare.

/// Edge Cases:
/// Egalitate de puncte (cum faci departajarea? ex: alfabetic, sau primul ajuns).
/// Interogarea unui n mai mare decât numărul total de utilizatori din sistem.
/// Puncte negative (dacă utilizatorul cheltuie punctele pentru o achiziție).

/// Exemplu:
/// AddPoints("UserA", 100);
/// AddPoints("UserB", 150);
/// AddPoints("UserA", 200); // UserA are acum 300
/// AddPoints("UserC", 50);
/// GetTopUsers(2); // Așteptat: ["UserA", "UserB"] (300, respectiv 150 puncte)


class LeaderboardService
{
    private class Entry : IComparable<Entry>
    {
        public string UserId;
        public int Points;
        public int CompareTo(Entry? other)
        {
            int cmp = other.Points.CompareTo(Points);
            if (cmp != 0) return cmp;
            return string.Compare(UserId, other.UserId, StringComparison.Ordinal);
        }
    }
    private readonly Dictionary<string, Entry> _userIndex = new();
    private readonly SortedSet<Entry> _sortedEntries = new();
    
    public void AddPoints(string userId, int points)
    {
        if (_userIndex.TryGetValue(userId, out var entry))
        {
            _sortedEntries.Remove(entry); // O(log U)
            entry.Points += points;
            _sortedEntries.Add(entry); //O(log U)
        }
        else
        {
            entry = new Entry { UserId = userId, Points = points};
            _userIndex[userId] = entry;
            _sortedEntries.Add(entry); //O(log U)
        }
    }

    public List<string> GetTopUsers(int n)
    {
        n = Math.Min(n, _sortedEntries.Count);
        return _sortedEntries.Take(n).Select(e => e.UserId).ToList();
    }
}

class Program
{
    static void Main(string[] args)
    {
        var leaderboardService = new LeaderboardService();
        leaderboardService.AddPoints("UserA", 100);
        leaderboardService.AddPoints("UserB", 150);
        leaderboardService.AddPoints("UserB", 150);
        leaderboardService.AddPoints("UserA", 200); // UserA are acum 300
        leaderboardService.AddPoints("UserC", 50);
        var leaderboard = leaderboardService.GetTopUsers(2); // Așteptat: ["UserA", "UserB"] (300, respectiv 150 puncte)
        for (int i = 0; i < leaderboard.Count; i++)
        {
            Console.WriteLine($"{i+1}.{leaderboard[i]}");
        }
    }
}