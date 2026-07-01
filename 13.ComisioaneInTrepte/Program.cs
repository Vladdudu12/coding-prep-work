/*
Descriere: Procesoarele de plată (precum Stripe) folosesc comisioane în trepte pe baza volumului de procesare.
De exemplu: primii 10.000 RON sunt taxați cu 2.9%, 
următorii 40.000 RON cu 2.5%, 
iar orice sumă care depășește 50.000 RON cu 2.0%. 
Trebuie să calculezi comisionul total pentru un volum dat.
*/

/// Input/Output:
/// decimal CalculateFee(decimal totalVolume, List<Tier> tiers)
/// O clasă Tier conține decimal? UpTo (limita superioară a treptei, null pentru ultima treaptă) și decimal Rate.
/// Returnează comisionul total aplicat.


/// Constrangeri:
/// Treptele sunt mereu ordonate crescător și acoperă intervalul până la infinit.
/// Volumul poate fi fracționar.


/// Edge Cases:
/// Volumul se oprește exact pe limita superioară a unei trepte (ex: exact 10.000).
/// Volum zero (comision 0).
/// Procesarea corectă a ultimei trepte unde UpTo este null sau decimal.MaxValue.


/// Exemplu:
// // Trepte: [0 - 10,000] -> 2%; [10,000 - Infinit] -> 1%
// CalculateFee(15000m, tiers);
// // Calcul: (10000 * 0.02) + (5000 * 0.01) = 200 + 50
// // Așteptat: 250m

public record Tier(decimal? Threshold, decimal TaxRate);

public class PaymentProcessor
{
    public static decimal CalculateFee(decimal totalVolume, List<Tier> tiers)
    {
        //parcurgem tiers, 
        // luam minimul dintre tier si totalVolume, 
        // aplicam rate pe suma rezultata
        // daca tier e null, atunci luam suma ramasa si aplicam rate
        if (totalVolume <= 0) return 0m;
        decimal result = 0m;
        decimal lowerBound = 0m;

        foreach (var tier in tiers)
        {
            if (totalVolume <= lowerBound) break;

            decimal upperBound = tier.Threshold ?? decimal.MaxValue;
            var taxableInThisTier = Math.Min(upperBound, totalVolume) - lowerBound;
            if(taxableInThisTier > 0)
                result += taxableInThisTier * tier.TaxRate;
            
            lowerBound = upperBound;
            
        }

        return result;
    }
}

class Program
{
    static void Main(string[] args)
    {
        List<Tier> tiers = new List<Tier> { new Tier(10000, 0.029m), new Tier(50000, 0.025m), new Tier(null, 0.02m) };
        var result = PaymentProcessor.CalculateFee(100000, tiers);
        Console.WriteLine(result);
    }
}