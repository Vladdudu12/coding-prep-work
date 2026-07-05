/*
Implementează un cache cu capacitate fixă care ține ultimele N interogări (ex: sold curent per client), eliminând cea mai puțin recent folosită intrare când se depășește capacitatea.
*/

/// Testează: combinație Dictionary + Linked List (sau LinkedList<T> din .NET), complexitate O(1) pentru get/put
/// Hint: dicționarul ține referințe către nodurile din listă pentru acces O(1) + reordonare O(1)
public record CacheEntry(string ClientId, decimal Sold);


class Program
{
    static Dictionary<string, LinkedListNode<CacheEntry>> harta = new ();
    static LinkedList<CacheEntry> ordineRecenta = new (); // capul = cel mai recent folosit, coada = cel mai vechi
    const int capacitateMaxima = 5;

    public static decimal? GetSoldClient(string clientId)
    {
        if (Program.harta.TryGetValue(clientId, out var node))
        {
            Program.ordineRecenta.Remove(node);
            Program.ordineRecenta.AddFirst(node);
            return node.Value.Sold;
        }
        else
        {
            return null;
        }
    }

    public static void PutSoldClient(string clientId, decimal sold)
    {
        if(Program.harta.TryGetValue(clientId, out var node))
        {
            Program.ordineRecenta.Remove(node);
            var newEntry = new CacheEntry(clientId, sold);
            var newNode = Program.ordineRecenta.AddFirst(newEntry);
            Program.harta[clientId] = newNode;
        }
        else
        {
            if(Program.harta.Count == capacitateMaxima)
            {
                var celMaiVechi = Program.ordineRecenta.Last;
                Program.ordineRecenta.RemoveLast();
                harta.Remove(celMaiVechi.Value.ClientId);
            }

            var nodNou = Program.ordineRecenta.AddFirst(new CacheEntry(clientId, sold));
            Program.harta[clientId] = nodNou;
        }
    }
    static void Main(string[] args)
    {
        PutSoldClient("C1", 200m);
        PutSoldClient("C1", 500m);
        PutSoldClient("C2", 100m);
        PutSoldClient("C3", 400m);
        PutSoldClient("C4", 500m);
        PutSoldClient("C5", 7600m);
        Console.WriteLine(GetSoldClient("C1"));
        PutSoldClient("C6", 600m);
        Console.WriteLine(GetSoldClient("C2"));
        PutSoldClient("C7", 760m);
        PutSoldClient("C8", 700m);

        foreach(var node in harta)
        {
            Console.WriteLine($"{node.Key} - {node.Value.Value.Sold}");
        }
    }

    // sold curent per client => rezulta intr-o LinkedList<Result>
    // dupa ce am facut interogarea
    // stocam LinkedList<Result> in Dictionar cu Timestamp
    // daca count + 1 ar depasi N
    // ordonam dupa Timestamp si eliminam primul Timestamp
    // Dupa adaugam noul raspuns

}