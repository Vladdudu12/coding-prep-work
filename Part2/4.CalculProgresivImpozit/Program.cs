/*
PFA-urile plătesc impozit pe tranșe (de ex: 10% până la un prag, 16% peste). Scrie o funcție care calculează impozitul total dat un venit și o listă de tranșe (prag, procent).
*/

/// Testează: logică de business pe bucle, atenție la off-by-one la praguri, decimal vs double (foarte important în fintech — niciodată double pentru bani!)
/// Hint: iterează tranșele în ordine, calculează doar partea din venit ce cade în tranșa curentă

class Program
{
    public record Transa(decimal? Prag, decimal Procent);
    static void Main(string[] args)
    {
        List<Transa> transe = new List<Transa>();
        transe.Add(new Transa(1000m, 0.05m)); // 50
        transe.Add(new Transa(3000m, 0.1m)); // 300
        transe.Add(new Transa(null, 0.16m)); // 160
        
        var result = CalculeazaImpozit(5000m, transe); // 510

        Console.WriteLine(result);
    }

    public static decimal CalculeazaImpozit(decimal venit, List<Transa> transe)
    {
        // trebuie ordonata lista de transe
        
        if (venit < 0 || transe.Count <= 0) return 0m;
        transe = transe.OrderBy(x => x.Prag.Value);
        // de verificat daca nu exista nicio transa null
        // daca exista mai multe transe cu null
        
        var impozitTotal = 0m;
        foreach(var transa in transe)
        {
            var venitImpozitabilTransa = transa.Prag != null ? Math.Min(venit, transa.Prag.Value) : venit;
            impozitTotal += venitImpozitabilTransa * transa.Procent;
            venit -= venitImpozitabilTransa; 
        }

        return impozitTotal;
    }
}