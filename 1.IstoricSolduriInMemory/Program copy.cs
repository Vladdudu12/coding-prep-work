







// using System.Diagnostics;


// /// <summary>
// /// Descriere: Implementează un serviciu in-memory care calculează și stochează soldul conturilor pe baza unui flux de tranzacții. 
// /// Sistemul poate primi tranzacții out-of-order (cu timestamp-uri din trecut din cauza întârzierilor de rețea). 
// /// Trebuie să oferi o metodă care să returneze soldul exact al unui cont la o anumită dată calendaristică (la finalul zilei).
// /// </summary>
// /// Input/Output
// /// void Process(string accountId, decimal amount, DateTime timestamp)
// /// decimal GetBalanceAtDate(string accountId, DateTime date) – returnează suma tranzacțiilor <= 23:59:59 a datei respective.
// /// Constrangeri:
// /// Sistemul trebuie să ruleze într-un singur proces.
// /// Fiecare cont poate avea până la 10.000 de tranzacții.
// /// Să fie optimizat: nu itera prin toate cele 10.000 de tranzacții la fiecare apel GetBalanceAtDate. 
// /// Edge cases:
// /// Interogarea soldului pentru o dată anterioară primei tranzacții a contului.
// /// Tranzacții inserate în aceeași zi, dar la ore diferite, neordonate.
// /// Tranzacții cu suma 0.
// /// Exemplu:
// /// Process("RO01", 100m, "2026-06-01 10:00");
// /// Process("RO01", -30m, "2026-06-05 14:00");
// /// Process("RO01", 50m, "2026-06-03 09:00");
// /// GetBalanceAtDate("RO01", "2026-06-04"); // Așteptat: 150m (calculat din 1 și 3 iunie, ignoră 5 iunie)

// class Program
// {

//     // avem o lista de accounts si la fiecare account avem o lista de tranzactii
//     // o tranzactie are un amount si data cand s-a intamplat tranzactia

//     // Procesezi toate tranzactiile
//     // le adaugi intr-o coada ordonata in functie de timestamp
//     // cand apelezi getbalanceatdate, doar reconstruiesti procesarea tranzactiilor pana la data respectiva. ora 23:59:59 a datei.

//     static IDictionary<string, SortedList<DateTime, decimal>> accounts = new Dictionary<string, SortedList<DateTime, decimal>>();

//     static void Main(string[] args)
//     {
//         accounts.Add("RO01", new SortedList<DateTime, decimal>());
//         accounts.Add("acc2", new SortedList<DateTime, decimal>());
//         accounts.Add("acc3", new SortedList<DateTime, decimal>());

//         try
//         {
//             const string accountId = "RO01";
//             Process(accountId, 0m, DateTime.Parse("2026-06-01 10:00"));
//             Process(accountId, 100m, DateTime.Parse("2026-06-01 10:00"));
//             Process(accountId, 100m, DateTime.Parse("2026-06-01 09:00"));
//             Process(accountId, -30m, DateTime.Parse("2026-06-05 14:00"));
//             Process(accountId, 50m, DateTime.Parse("2026-06-03 09:00"));

//             var balance = GetBalanceAtDate(accountId, DateTime.Parse("2026-06-04"));
//             Console.WriteLine($"account: {accountId} has the balance: {balance} for date: 2026-06-04");
//         }
//         catch (Exception exception)
//         {
//             Console.WriteLine(exception.Message);
//         }
//     }

//     static void Process(string accountId, decimal amount, DateTime timestamp)
//     {
//         //process the transaction
//         if (amount == 0) throw new Exception("The amount cannot be 0");
//         if (!accounts.ContainsKey(accountId)) throw new Exception("The account you requested doesn't exist");
//         if (accounts[accountId].Count > 10000) throw new Exception("You cannot process any more transactions at this time");
        
//         accounts[accountId].Add(timestamp, amount);
//     }

//     static decimal GetBalanceAtDate(string accountId, DateTime date)
//     {
//         if (!accounts.ContainsKey(accountId)) throw new Exception("The account you requested doesn't exist");
//         var accountTransactions = accounts[accountId];
//         if (accountTransactions.Count < 0) throw new Exception("There are no transactions");
//         decimal balance = 0;

//         foreach (var transaction in accountTransactions)
//         {
//             if (transaction.Key.CompareTo(date.Date) >= 0) return balance;
//             balance += transaction.Value;
//         }

//         return balance;
//     }
// }