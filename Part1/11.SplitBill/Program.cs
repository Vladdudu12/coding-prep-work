/*
Descriere: Într-o aplicație de tip Revolut, un utilizator vrea să împartă o notă de plată de la restaurant
(în valoare de XRON) în mod egal cu N prieteni (inclusiv el). 
Din cauza diviziunilor care dau rest (ex: 100 RON la 3 persoane înseamnă 33.333... RON), 
împărțirea matematică directă pierde bani (33.33 * 3 = 99.99 RON, lipsește 1 ban). 
Implementează o funcție care împarte suma corect, fără a pierde sau crea bani din aer, adăugând restul de 1 ban la oricare dintre participanți.
*/

/// Input/Output
/// List<decimal> SplitBill(decimal totalAmount, int numberOfPeople)
/// Returnează o listă cu valorile de plată pentru fiecare persoană. Suma exactă a elementelor din listă trebuie să fie egală cu totalAmount.

/// Constrangeri 
/// Banii suportă doar două zecimale.
/// numberOfPeople este întotdeauna > 0.
/// Diferența dintre cea mai mare și cea mai mică sumă de plată atribuită persoanelor trebuie să fie de maximum 0.01.

/// Edge Cases
/// Nota este mai mică decât numărul de persoane (ex: total 0.02 RON la 3 persoane -> a treia persoană plătește 0.00).
/// Sume negative (dacă împart o datorie sau un discount).
/// Rotunjiri implicite eronate în C# (ex: evitarea diviziei floating point, se folosește exclusiv decimal).

/// Exemplu
/// SplitBill(100.00m, 3); // Așteptat: [33.34m, 33.33m, 33.33m] (sau orice ordine a acestor valori)
/// SplitBill(100.00m, 1); // Așteptat: [100.00m]


class BillSplittingService
{
    public List<decimal> SplitBill(decimal totalAmount, int numberOfPeople)
    {
        if (numberOfPeople <= 0) 
            throw new ArgumentException("numberOfPeople must be > 0");
        
        long totalCents = (long)(totalAmount * 100m);

        long baseCents = totalCents / numberOfPeople;
        long remainder = totalCents - baseCents * numberOfPeople;

        var result = new List<decimal>(numberOfPeople);
        for (int i = 0; i < numberOfPeople; i++)
        {
            result.Add(baseCents / 100m);
        }

        int sign = remainder >= 0 ? 1 : -1;
        for(int i = 0; i < Math.Abs(remainder); i++)
        {
            result[i] += sign * 0.01m;
        }

        return result;
    }
}

class Program
{
    static void Main(string[] args)
    {
        var billSplitter = new BillSplittingService();
        var result1 = billSplitter.SplitBill(100.00m, 3); // Așteptat: [33.34m, 33.33m, 33.33m] (sau orice ordine a acestor valori)
        foreach (var res in result1)
        {
            Console.Write($"{res},");
        }
        Console.WriteLine();
        var result2 = billSplitter.SplitBill(100.00m, 1); // Așteptat: [100.00m]
        foreach (var res in result2)
        {
            Console.Write($"{res},");
        }
    }
}