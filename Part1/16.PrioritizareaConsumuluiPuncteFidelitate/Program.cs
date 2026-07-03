/*
Descriere: Utilizatorii câștigă puncte de fidelitate care expiră la date diferite 
(ex: punctele de la ziua de naștere expiră în 30 de zile, cele standard în 365 de zile). 
Când un utilizator alege să consume un număr de puncte la checkout, 
algoritmul trebuie să le consume pe cele care expiră primele pentru a-l favoriza pe client.
*/

/// Input/Output:
/// void EarnPoints(int amount, DateTime expiryDate)
/// bool SpendPoints(int amountToSpend, DateTime currentDate) – Returnează true dacă punctele au fost deduse cu succes, altfel false (dacă nu are destule puncte valabile).


/// Constrangeri:
/// Punctele expirate strict înaintea lui currentDate nu pot fi folosite și trebuie curățate.

/// Edge Cases:
/// Utilizatorul are suficiente puncte total, dar o parte sunt deja expirate la currentDate, așa că soldul valabil este insuficient.
/// Două loturi de puncte au exact aceeași dată de expirare (ordinea devine arbitrară, dar trebuie gestionată fără erori).


/// Exemplu:
// EarnPoints(100, "2026-12-01"); // Expiră în iarnă
// EarnPoints(50, "2026-07-01");  // Expiră în vară

// SpendPoints(60, "2026-06-01"); // True
// // Consumă cele 50 de la vară și 10 din cele de la iarnă.

// LinkedList si gestionez eu cand dau earn indexul unde trebuie pusa noua inregistrare

// PriorityQueue si trebuie comparator de data.

public class PointSystem
{
    private readonly PriorityQueue<int, DateTime> _points = new();
    public void EarnPoints(int amount, DateTime expiryDate)
    {
        // adaugam punctele in priorityQueue
        _points.Enqueue(amount, expiryDate);

        // foreach(var kvp in _points.UnorderedItems)
        // {
        //     Console.WriteLine($"{kvp.Element} expira la {kvp.Priority}");
        // }
    }

    public bool SpendPointsV1(int amountToSpend, DateTime currentDate)
    {
        var temporaryQueue = new PriorityQueue<int, DateTime>();
        temporaryQueue.EnqueueRange(_points.UnorderedItems);
        while (temporaryQueue.Count > 0 && amountToSpend > 0)
        {
            // Console.WriteLine(_points.Count);
            // Console.WriteLine(amountToSpend);
            if (temporaryQueue.TryPeek(out int element, out DateTime priority))
            {
                if (priority.CompareTo(currentDate) < 0)
                {
                    temporaryQueue.Dequeue();
                    continue;
                }

                var points = Math.Min(element, amountToSpend);
                if (element > points)
                {
                    element -= points;
                    temporaryQueue.DequeueEnqueue(element, priority);
                    amountToSpend -= points;
                    continue;
                }
                else
                {
                    amountToSpend -= points;
                    temporaryQueue.Dequeue();
                    continue;
                }
            }
        }
        if (amountToSpend <= 0)
        {
            _points.Clear();
            _points.EnqueueRange(temporaryQueue.UnorderedItems);
            return true;
        }

        return false;
    }

    public bool SpendPoints(int amountToSpend, DateTime currentDate)
    {
        var valid = new List<(int Amount, DateTime Expiry)>();
        int available = 0;
        while (_points.TryDequeue(out int element, out DateTime priority))
        {
            if (priority < currentDate) continue;
            valid.Add((element, priority));
            available += element;
        }

        foreach (var (amount,expiry) in valid)
        {
            _points.Enqueue(amount, expiry);
        }

        if (available < amountToSpend)
        {
            return false;
        }

        int remaining = amountToSpend;
        var updated = new List<(int Amount, DateTime Expiry)>();
        while (_points.TryDequeue(out int element, out DateTime priority) && remaining > 0)
        {
            int consumed = Math.Min(element, remaining);
            remaining -= consumed;
            int leftover = element - consumed;
            if (leftover > 0) updated.Add((leftover, priority));
        }

        updated.AddRange(_points.UnorderedItems.Select(x => (x.Element, x.Priority)));

        _points.Clear();
        foreach (var (amount, expiry) in updated)
        {
            _points.Enqueue(amount, expiry);
        }

        return true;
    }
}


class Program
{
    static void Main(string[] args)
    {
        var pointSystem = new PointSystem();

        // pointSystem.EarnPoints(100, DateTime.Parse("2026-12-01"));
        // pointSystem.EarnPoints(50, DateTime.Parse("2026-07-01"));

        // var result = pointSystem.SpendPoints(60, DateTime.Parse("2026-06-01"));

        pointSystem.EarnPoints(10, DateTime.Parse("2026-01-01")); // deja expirat
        pointSystem.EarnPoints(20, DateTime.Parse("2026-12-01")); // valabil

        // cerem mai mult decât avem valabil -> false
        var result = pointSystem.SpendPoints(100, DateTime.Parse("2026-06-01")); // false, corect

        // dar lotul expirat din ianuarie e tot acolo în _points, "necurățat"
        // cerința spune explicit: "Punctele expirate ... trebuie curățate"

        Console.WriteLine(result);
    }
}