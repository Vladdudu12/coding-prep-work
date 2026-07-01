/*
Descriere: La un cont de economii, dobânda nu se calculează pe baza soldului de la finalul lunii,
ci se calculează zilnic pe baza soldului activ (End-of-Day Balance) 
și se plătește cumulat o dată pe lună. Dată fiind o listă de tranzacții din timpul lunii, 
calculează dobânda totală câștigată în acea lună. Formula zilnică: (Sold * Rata Anuală) / Zile in An.
*/

/// Input/Output:
/// decimal CalculateInterest(List<Transaction> transactions, decimal startBalance, decimal annualRate, int year, int month)
/// Returnează totalul dobânzii acumulate pe acea lună, rotunjit la 2 zecimale.

/// Constrangeri:
/// O tranzacție e o clasă record Transaction(DateTime Date, decimal Amount).
/// Pot exista mai multe tranzacții în aceeași zi.
/// Zilele dintr-un an trebuie luate corect (365 sau 366). Anii bisecți contează.

/// Edge Cases:
/// Tranzacții efectuate în ultima zi a lunii versus prima zi a lunii.
/// Zile în care nu există nicio tranzacție (soldul rămâne același de ziua precedentă).
/// Rotunjiri: dobânda per zi se ține cu precizie maximă și se rotunjește doar suma totală la final? (Da, standardul financiar).

/// Exemplu:
// Sold inițial: 10,000. Rata: 0.05 (5%). Luna: Aprilie (30 zile). An non-bisect.
// Fără tranzacții în lună.
// CalculateInterest(new List<Transaction>(), 10000m, 0.05m, 2026, 4); 
// Formula zilnică: (10,000 * 0.05) / 365 = 1.3698... pe zi.
// Total pe 30 zile: ~41.10

using System.Runtime.CompilerServices;
public record Transaction(DateTime Date, decimal Amount);

class AccrualCalculator
{
    private readonly List<int> _months = new List<int>
    {
        31,// Ian - 31
        28,// Feb - 28/29
        31,// Mar - 31
        30,// Apr - 30
        31,// Mai - 31
        30,// Iun - 30
        31,// Iul - 31
        31,// Aug - 31
        30,// Sept - 30
        31,// Oct - 31
        30,// Nov - 30
        31,// Dec - 31
    };

    public decimal CalculateInterest(List<Transaction> transactions, decimal startBalance, decimal annualRate, int year, int month)
    {
        if (month <= 0 || month > 12) throw new Exception("the month is wrong");

        //Sortam tranzactiile in functie de DateTime
        var sortedTransactions = transactions.OrderBy(x => x.Date).ToList();

        // Aflam cate zile are luna ceruta
        // Daca luna e februarie (index 1) atunci verificam daca e an bisect
        var numberOfDays = month - 1 == 1 ? (DateTime.IsLeapYear(year) ? _months[month - 1] + 1 : _months[month - 1]) : _months[month - 1];

        decimal totalAccrual = 0;
        for (int i = 0; i < numberOfDays; i++)
        {
            var todayTransactions = sortedTransactions.Where(x => x.Date.Year == year && x.Date.Month == month && x.Date.Day == (i + 1)).ToList();
            foreach (var transaction in todayTransactions)
            {
                startBalance += transaction.Amount;
            }
            decimal dailyAccrual = (startBalance * annualRate) / (DateTime.IsLeapYear(year) ? 366 : 365);
            totalAccrual += dailyAccrual;
            
            Console.WriteLine($"ziua {i+1} - startingBalance: {startBalance} | total:{totalAccrual}");

        }

        // calculam pe ziua 1
        // calculam pe ziua 2
        // etc
        // la final suma
        // daca nu exista tranzactie in ziua n, atunci in n+1 avem acelasi balance.
        return Math.Round(totalAccrual, 2, MidpointRounding.AwayFromZero);
    }
}


class Program
{
    static void Main(string[] args)
    {
        var accrualCalculator = new AccrualCalculator();

        Console.WriteLine(accrualCalculator.CalculateInterest(new List<Transaction>(), 10000m, 0.05m, 2026, 4));
    }
}