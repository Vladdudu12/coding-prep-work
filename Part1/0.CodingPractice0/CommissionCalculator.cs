// See https://aka.ms/new-console-template for more information

using System;
using System.Collections.Generic;
using System.Linq;



using System.Runtime.Serialization;
namespace Solo.Fintech;


public record CommissionTier(decimal? UpperBound, decimal Rate);
public class CommissionCalculator
{
    private readonly IReadOnlyList<CommissionTier> _tiers;

    public CommissionCalculator(IEnumerable<CommissionTier> tiers)
    {
        _tiers = tiers
            .OrderBy(t => t.UpperBound ?? decimal.MaxValue)
            .ToList();

        if (_tiers.Count == 0)
        {
            throw new ArgumentException("Este nevoie de cel putin un tier");
        }
    }

    public decimal Calculate(IEnumerable<decimal> transactions)
    {
        decimal totalVolume = transactions.Sum();
        decimal commission = 0m;
        decimal lowerBound = 0m;

        foreach (var tier in _tiers)
        {
            if (totalVolume <= lowerBound) break;

            decimal upperBound = tier.UpperBound ?? totalVolume;
            decimal amountInTier = Math.Min(totalVolume, upperBound) - lowerBound;

            commission += amountInTier * tier.Rate;
            lowerBound = upperBound;
        }

        return Math.Round(commission, 2, MidpointRounding.AwayFromZero);
    }
}

/// <summary>
/// Un startup fintech aplică un comision procentual pe tranzacțiile valutare, 
/// dar structura de comision variază în funcție de volumul lunar al clientului. 
/// Dacă totalul tranzacțiilor dintr-o lună depășește anumite praguri, 
/// rata de comision scade pentru tranșele superioare (sistem de tier-uri progresive, similar cu un impozit pe venit). 
/// Trebuie să implementezi o clasă care primește configurația de tier-uri și calculează comisionul total pentru o
///  listă de tranzacții ale unui client într-o lună dată. Soluția trebuie să fie ușor de extins cu noi tier-uri fără a modifica logica de calcul.
/// INPUT: O listă de tier-uri, fiecare cu un prag superior (sau infinit pentru ultimul) și o rată procentuală; și o listă de sume de tranzacții (în EUR, valori pozitive). Tier-urile sunt ordonate crescător după prag.
/// OUTPUT: Un număr zecimal reprezentând comisionul total calculat, rotunjit la 2 zecimale.
/// CONSTRÂNGERI: 1–100 tranzacții per lună; sumele sunt între 0.01 și 50000.00 EUR; 1–5 tier-uri; pragurile tier-urilor sunt strict crescătoare; ultimul tier nu are prag superior (acoperă orice volum rămas); ratele sunt între 0% și 5%.
/// </summary>



class Program1
{
    static void Main(string[] args)
    {
        var tiers = new List<CommissionTier>
        {
            new (1000m, 0.015m),
            new (5000m, 0.010m),
            new (null, 0.005m),
        };

        var calculator = new CommissionCalculator(tiers);

        var transactions = new List<decimal> { 200m, 800m, 1500m, 3000m };
        Console.WriteLine($"Volum total: {transactions.Sum():0.00} EUR");
        Console.WriteLine($"Comision: {calculator.Calculate(transactions):0.00} EUR");
        Console.WriteLine();

        
        Console.WriteLine($"Exact pe prag (1000): {calculator.Calculate(new List<decimal> {1000m})}");
        Console.WriteLine($"O tranzactie mare (5500): {calculator.Calculate(new List<decimal> {5500m})}");
        Console.WriteLine($"Tot sub primul prag (500): {calculator.Calculate(new List<decimal> {200m, 300m})}");
        Console.WriteLine($"Tranzactie minima (0.01): {calculator.Calculate(new List<decimal> {0.01m})}");
        
    }
}