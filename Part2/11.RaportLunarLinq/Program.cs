/*
Dintr-o listă de tranzacții (dată, sumă, categorie), generează un raport grupat pe lună și categorie, cu total și medie, folosind LINQ.

*/

/// Testează: GroupBy cu chei compuse, Select cu proiecții, diferența IEnumerable (lazy) vs ToList() (eager) — probabil vor întreba de ce ai pus (sau nu) ToList() la un anumit pas
/// Hint: grupează pe new { an, luna, categorie }, atenție la execuția amânată dacă query-ul e reosolosit de mai multe ori (recalculare!)

public record Tranzactie(DateTime Data, decimal Suma, string Categorie);
public record TranzactieRaport(string Categorie, int An, int Luna, decimal Suma, decimal Medie);

class Program
{

    // am tranzactii (data, suma, categorie)
    // metoda care primeste lista de tranzactii
    // grupam lista dupa an luna categorie, apoi select total(sum) si medie(mean)
    static void Main(string[] args)
    {
        List<Tranzactie> tranzactii = new List<Tranzactie>();
        tranzactii.Add(new Tranzactie(DateTime.UtcNow, 1000m, "Mancare"));
        tranzactii.Add(new Tranzactie(DateTime.UtcNow, 250m, "Mancare"));
        tranzactii.Add(new Tranzactie(DateTime.UtcNow, 3000m, "Mancare"));
        tranzactii.Add(new Tranzactie(DateTime.UtcNow.AddMonths(1), 230m, "Mancare"));
        tranzactii.Add(new Tranzactie(DateTime.UtcNow.AddMonths(1), 400m, "Mancare"));
        tranzactii.Add(new Tranzactie(DateTime.UtcNow.AddMonths(1), 6000m, "Mancare"));

        tranzactii.Add(new Tranzactie(DateTime.UtcNow.AddMonths(1), 30m, "Transport"));
        tranzactii.Add(new Tranzactie(DateTime.UtcNow.AddMonths(1), 900m, "Transport"));
        tranzactii.Add(new Tranzactie(DateTime.UtcNow.AddMonths(1), 1200m, "Transport"));

        tranzactii.Add(new Tranzactie(DateTime.UtcNow.AddYears(1), 300m, "Mancare"));
        tranzactii.Add(new Tranzactie(DateTime.UtcNow.AddYears(1), 5000m, "Transport"));
        tranzactii.Add(new Tranzactie(DateTime.UtcNow.AddYears(1), 8000m, "Mancare"));
        tranzactii.Add(new Tranzactie(DateTime.UtcNow.AddYears(1).AddMonths(2), 399m, "Calatorii"));
        tranzactii.Add(new Tranzactie(DateTime.UtcNow.AddYears(1).AddMonths(2), 599m, "Calatorii"));
        tranzactii.Add(new Tranzactie(DateTime.UtcNow.AddYears(1).AddMonths(2), 600m, "Calatorii"));


        GenereazaRaport(tranzactii);
    }

    public static List<TranzactieRaport> GenereazaRaport(List<Tranzactie> tranzactii)
    {
        if (tranzactii.Count == 0) throw new Exception("Nu exista nicio tranzactie");

        var tranzactiiGrupate = tranzactii.GroupBy(x => new { x.Data.Year, x.Data.Month, x.Categorie }).Select(group => new TranzactieRaport
        (
            Categorie : group.Key.Categorie,
            An : group.Key.Year,
            Luna : group.Key.Month,
            Suma : group.Sum(x => x.Suma),
            Medie : Math.Round(group.Average(x => x.Suma), 2, MidpointRounding.AwayFromZero)
        )).ToList();
        // var suma = tranzactiiGrupate.Select(x => new Tranzactie(
        //     Data: x.First().Data,
        //     Suma:  x.Sum(item => item.Suma),
        //     Categorie: x.First().Categorie)).ToList();

        // var medie = tranzactiiGrupate.Select(x => new Tranzactie(
        //     Data: x.First().Data,
        //     Suma: x.Average(item => item.Suma),
        //     Categorie: x.First().Categorie)).ToList();

        // Console.WriteLine("======SUMA=======");
        // foreach (var s in suma)
        // {
        //     Console.WriteLine(s);
        // }

        // Console.WriteLine("======MEDIE=======");
        // foreach (var m in medie)
        // {
        //     Console.WriteLine(m);
        // }

        foreach(var tranzactie in tranzactiiGrupate)
        {
            Console.WriteLine(tranzactie);
        }

        return tranzactiiGrupate;
    }
}