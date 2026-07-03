







using System.Diagnostics;


/// <summary>
/// Soluție optimizată: O(log n) la GetBalanceAtDate prin prefix sums + binary search.
/// Suportă tranzacții out-of-order și timestamp-uri duplicate.
/// 
/// Design decisions:
/// - Menținem tranzacțiile sortate după timestamp (List<Transaction> + inserare cu binary search)
/// - Menținem un array de prefix sums paralel, recalculat de la punctul de inserare în O(n)
/// - Query-ul GetBalanceAtDate face binary search în O(log n) și citește direct prefix sum-ul
/// - Trade-off: Process e O(n) în cel mai rău caz (inserare în mijloc) dar Write < Read în fintech
/// - Pentru 10.000 tranzacții per cont, O(n) la inserare e complet acceptabil
/// </summary>

class BalanceService
{
    private record Transaction(DateTime Timestamp, decimal Amount);

    private readonly Dictionary<string, List<Transaction>> _transactions = new();
    private readonly Dictionary<string, List<decimal>> _prefixSums = new();

    public void Process(string accountId, decimal amount, DateTime timestamp)
    {
        if (!_transactions.ContainsKey(accountId))
        {
            _transactions[accountId] = new List<Transaction>();
            _prefixSums[accountId] = new List<decimal>();
        }

        // amount == 0 tratam ca tranzactie valida

        var txList = _transactions[accountId];
        var psList = _prefixSums[accountId];

        // binary search dupa timestamp O(log n)
        int insertPos = FindInsertPosition(txList, timestamp);

        // Inseram tranzactia la pozitia corecta (O(n) shift, acceptabil pentru 10k)
        txList.Insert(insertPos, new Transaction(timestamp, amount));

        decimal prevSum = insertPos > 0 ? psList[insertPos - 1] : 0m;
        psList.Insert(insertPos, prevSum + amount);

        for (int i = insertPos + 1; i < psList.Count; i++)
        {
            psList[i] = psList[i - 1] + txList[i].Amount;
        }
    }

    public decimal GetBalanceAtDate(string accountId, DateTime date)
    {
        if (!_transactions.ContainsKey(accountId))
        {
            return 0m;
        }

        var txList = _transactions[accountId];

        if (txList.Count == 0)
        {
            return 0m;
        }

        DateTime endOfDay = date.Date.AddDays(1);

        int lo = 0, hi = txList.Count - 1, result = -1;

        while (lo <= hi)
        {
            int mid = (lo + hi) / 2;

            if (txList[mid].Timestamp < endOfDay)
            {
                result = mid;
                lo = mid + 1;
            }
            else
            {
                hi = mid - 1;
            }
        }

        if (result == -1)
        {
            return 0m;
        }

        return _prefixSums[accountId][result];
    }

    // Binary search pentru poziția de inserare: primul index cu Timestamp > timestamp dat
    // Tranzacțiile cu același timestamp sunt permise (inserare după cele existente)
    private static int FindInsertPosition(List<Transaction> list, DateTime timestamp)
    {
        int lo = 0, hi = list.Count;

        while (lo < hi)
        {
            int mid = (lo + hi) / 2;
            if (list[mid].Timestamp <= timestamp)
            {
                lo = mid + 1;
            }
            else
            {
                hi = mid;
            }
        }

        return lo;
    }
}

class Program
{
    static void Main(string[] args)
    {
        var service = new BalanceService();
        const string accountId = "RO01";

        service.Process(accountId, 100m, DateTime.Parse("2026-06-01 10:00"));
        service.Process(accountId, -30m, DateTime.Parse("2026-06-05 14:00"));
        service.Process(accountId, 50m, DateTime.Parse("2026-06-03 09:00"));

        var balance = service.GetBalanceAtDate(accountId, DateTime.Parse("2026-06-04"));
        Console.WriteLine($"Sold la 04 iunie: {balance}m — Așteptat: 150m — {(balance == 150m ? "✓" : "✗")}");

        // Edge case: dată anterioară primei tranzacții
        var beforeFirst = service.GetBalanceAtDate(accountId, DateTime.Parse("2026-05-31"));
        Console.WriteLine($"Sold la 31 mai (înainte de prima tx): {beforeFirst}m — Așteptat: 0m — {(beforeFirst == 0m ? "✓" : "✗")}");

        // Edge case: tranzacții în aceeași zi, ore diferite, neordonate
        var service2 = new BalanceService();
        service2.Process("acc2", 200m, DateTime.Parse("2026-06-10 17:00"));
        service2.Process("acc2", 100m, DateTime.Parse("2026-06-10 08:00")); // inserare în trecut
        service2.Process("acc2", 50m, DateTime.Parse("2026-06-10 12:00")); // inserare în mijloc

        var sameDay = service2.GetBalanceAtDate("acc2", DateTime.Parse("2026-06-10"));
        Console.WriteLine($"Sold aceeași zi (3 tx out-of-order): {sameDay}m — Așteptat: 350m — {(sameDay == 350m ? "✓" : "✗")}");

        // Edge case: tranzacție cu amount 0 (validă, nu aruncă excepție)
        var service3 = new BalanceService();
        service3.Process("acc3", 500m, DateTime.Parse("2026-06-01 10:00"));
        service3.Process("acc3", 0m, DateTime.Parse("2026-06-02 10:00"));
        service3.Process("acc3", 100m, DateTime.Parse("2026-06-03 10:00"));

        var withZero = service3.GetBalanceAtDate("acc3", DateTime.Parse("2026-06-03"));
        Console.WriteLine($"Sold cu tx de 0: {withZero}m — Așteptat: 600m — {(withZero == 600m ? "✓" : "✗")}");

        // Edge case: timestamp-uri identice (două tranzacții exact în același moment)
        var service4 = new BalanceService();
        service4.Process("acc4", 300m, DateTime.Parse("2026-06-01 10:00"));
        service4.Process("acc4", 200m, DateTime.Parse("2026-06-01 10:00")); // același timestamp

        var sameTimestamp = service4.GetBalanceAtDate("acc4", DateTime.Parse("2026-06-01"));
        Console.WriteLine($"Sold cu timestamp identic: {sameTimestamp}m — Așteptat: 500m — {(sameTimestamp == 500m ? "✓" : "✗")}");

        // Edge case: cont inexistent
        var unknown = service.GetBalanceAtDate("RO99", DateTime.Parse("2026-06-01"));
        Console.WriteLine($"Cont inexistent: {unknown}m — Așteptat: 0m — {(unknown == 0m ? "✓" : "✗")}");

    }
}