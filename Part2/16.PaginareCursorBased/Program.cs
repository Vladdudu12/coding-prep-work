/*
Design + implementare pentru un endpoint care listează facturile unui client, sortate după dată, cu paginare care rămâne corectă chiar dacă se adaugă facturi noi între request-uri.
*/

/// Input/Output:
/// Testează: de ce offset-based paginare (SKIP/TAKE) poate sări sau duplica rezultate cand datele se schimbă; design de cursor (ex: bazat pe (DataFactura, Id) ca tie-breaker)
/// Hint: cursor-ul encodează ultimul element văzut, nu o poziție numerică


/// metoda de paginare cu cursor
/// initial e null cursorul
/// dupa care subsequent requests (apeluri ale functiei) vor folosi ultimul element din raspunsul anterior
/// 
/// Fiecare factura are un id Guid
/// sortam in functie de data

public record Factura(Guid Id, Guid ClientId, DateTime Data);

class Program
{
    static void Main(string[] args)
    {
        List<Factura> facturi = new List<Factura>();

        for (int i = 1; i <= 60; i++)
        {
            facturi.Add(new Factura(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays((-1) * i)));
        }


        var result1 = GetPaged(facturi);
        foreach (var r in result1)
        {
            Console.WriteLine($"{r}");
        }
        Console.WriteLine($"URMATOAREA PAGINA");

        var result2 = GetPaged(facturi, result1.Last().Id);
        foreach (var r in result2)
        {
            Console.WriteLine($"{r}");
        }
        Console.WriteLine($"URMATOAREA PAGINA");
    }

    public static List<Factura> GetPaged(List<Factura> facturi, Guid? cursor = null, int? pageSize = 12)
    {
        var facturiOrdonate = facturi.OrderBy(x => x.Data).ThenBy(x => x.Id).ToList();
        if (cursor == null)
        {
            return facturiOrdonate.Take(pageSize.Value).ToList();
        }
        else
        {
            var cursorIdx = facturiOrdonate.FindIndex(x => x.Id == cursor.Value);
            return facturiOrdonate.Skip(cursorIdx == -1 ? 0 : cursorIdx + 1).Take(pageSize.Value).ToList();
        }

    }
}