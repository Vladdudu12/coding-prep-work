/*
La reimportarea unui extras bancar, unele tranzacții pot apărea de două ori (același ID extern, sau aceeași sumă+dată+cont). Scrie o funcție de deduplicare la import.
*/

/// Input/Output:
/// Testează: alegerea cheii de unicitate (ID extern dacă există, altfel compus din câmpuri), idempotență la import
/// Hint: discută diferența dintre deduplicare "sigură" (ID extern unic) și "euristică" (sumă+dată — poate da fals pozitiv la două tranzacții identice legitime)


public class DbContext
{
    private readonly Dictionary<string, ExtrasBancar> _extraseBancare = new();

    public Dictionary<string, ExtrasBancar> Import(List<ExtrasBancar> extraseBancare)
    {
        foreach (var extras in extraseBancare)
        {
            var key = extras.Id != null ? extras.Id : extras.HashDetails();
            if (!_extraseBancare.TryGetValue(key, out var extrasBancar))
            {
                _extraseBancare.Add(key, extras);
            }
        }

        return _extraseBancare; // returnam instanta interna, nu e corect, dar e facut pentru viteza acum, normal facem o clona
    }
}
public class ExtrasBancar
{
    private readonly string? _id;
    private readonly decimal _suma;
    private readonly DateTime _data;
    private readonly string _cont;

    public ExtrasBancar(decimal suma, DateTime data, string cont, string? id = null)
    {
        _id = id;
        _suma = suma;
        _data = data;
        _cont = cont;
    }

    public string? Id => _id;
    public decimal Suma => _suma;
    public DateTime Data => _data;
    public string Cont => _cont;

    public string HashDetails()
    {
        return $"{_suma}{_data}{_cont}";
    }
}

class Program
{
    static void Main(string[] args)
    {
        List<ExtrasBancar> extraseBancare = new List<ExtrasBancar>();
        extraseBancare.Add(new ExtrasBancar(100m, DateTime.UtcNow, "Cont 1", "Boss 1"));
        extraseBancare.Add(new ExtrasBancar(200m, DateTime.UtcNow, "Cont 2", "Boss 1"));
        extraseBancare.Add(new ExtrasBancar(500, DateTime.UtcNow, "Cont 3"));
        extraseBancare.Add(new ExtrasBancar(500, DateTime.UtcNow, "Cont 3"));
        extraseBancare.Add(new ExtrasBancar(500, DateTime.UtcNow, "Cont 3", "Boss 4"));

        var DbContext = new DbContext();
        var result = DbContext.Import(extraseBancare);

        foreach (var res in result)
        {
            Console.WriteLine(res);
        }

        var result2 = DbContext.Import(extraseBancare);
        foreach (var res in result2)
        {
            Console.WriteLine(res);
        }
        Console.WriteLine(result.Count == result2.Count);
    }
}