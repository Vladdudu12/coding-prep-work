/*
Ai o listă de linii de factură, fiecare cu preț unitar, cantitate și cotă TVA. 
Calculează TVA total, dar cu regula: rotunjirea se face o singură dată la total, nu per linie (sau invers — discută diferența).
*/

/// Testează: înțelegerea capcanelor de rotunjire cu bani (suma rotunjirilor per linie ≠ rotunjirea sumei), MidpointRounding
/// Hint: Math.Round(value, 2, MidpointRounding.AwayFromZero) e des folosit în facturare fiscală din România; explică de ce alegi o strategie sau alta

public record LinieFactura(decimal PretUnitar, int Cantitate, decimal cotaTVA);

class Program
{
    static void Main(string[] args)
    {
        List<LinieFactura> liniiFactura = new List<LinieFactura>();
        liniiFactura.Add(new LinieFactura(300m, 5, 0.24m));
        liniiFactura.Add(new LinieFactura(200m, 2, 0.2m));
        liniiFactura.Add(new LinieFactura(500m, 5, 0.15m));
        liniiFactura.Add(new LinieFactura(100m, 10, 0.1m));

        Console.WriteLine(CalculeazaTVA(liniiFactura));
        Console.WriteLine(CalculeazaTVA(liniiFactura, 0.2m));
    }

    public static decimal CalculeazaTVA(List<LinieFactura> liniiFactura, decimal? tva = null)
        => tva.HasValue ?
        Math.Round(liniiFactura.Sum(x => x.PretUnitar * x.Cantitate) * tva.Value, 2, MidpointRounding.AwayFromZero)
        : Math.Round(liniiFactura.Sum(x => x.PretUnitar * x.Cantitate * x.cotaTVA), 2, MidpointRounding.AwayFromZero);

    // mai exista o abordare in care grupam in functie de Cota de TVA pe fiecare linie si dupa calculam normal in fiecare grup si dupa rotunjim suma
    public static decimal CalculeazaTVAPeCoteGrupate(List<LinieFactura> liniiFactura)
    {
        return liniiFactura
            .GroupBy(x => x.CotaTVA)
            .Sum(grup => Math.Round(
                grup.Sum(x => x.PretUnitar * x.Cantitate * x.CotaTVA),
                2,
                MidpointRounding.AwayFromZero));
    }
}