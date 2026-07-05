/*
Dată o factură cu scadență și data curentă, calculează penalitatea de întârziere (ex: 0.1%/zi din suma restantă, plafonat la un anumit procent din total).
*/

/// Testează: aritmetică pe date (DateTime/DateOnly), edge cases (weekend-uri? zile lucrătoare?), rotunjire corectă
/// Hint: clarifică explicit cu interviewer-ul dacă se numără zile calendaristice sau lucrătoare — e o presupunere pe care n-ar trebui s-o faci fără să întrebi


/// Plafon x procent din suma din factura
/// pe zi luam y procent => dobandaTotala = suma * procent y * diferenta zile (data curenta - data scadenta) (de vazut daca zile lucratoare sau zile calendaristice)
/// luam Math.Min(plafon x procent, dobandaTotala)

// decat sa calculam dobanda, calculam procentPlafonMaxim/procentDobandaZilnica si aflam numarul de zile in care se ajunge la plafon. 
// daca diferenta de zile dintre data scadenta si ziua curenta este mai mare decat numarul de zile in care se ajunge la plafon
// atunci luam direct valoarea de la plafon • suma
// altfel calculam suma * procentDobandaZilnica * diferenta zile


public record Factura(DateOnly DataScadenta, decimal Suma);

public class CalculatorDobanda
{
    private readonly decimal _procentDobandaZilnica;
    private readonly decimal _procentPlafonMaxim;

    public CalculatorDobanda(decimal procentDobandaZilnica, decimal procentPlafonMaxim)
    {
        if (procentDobandaZilnica <= 0)
        throw new ArgumentException("Procentul zilnic trebuie să fie pozitiv.", nameof(procentDobandaZilnica));

        _procentDobandaZilnica = procentDobandaZilnica;
        _procentPlafonMaxim = procentPlafonMaxim;
    }

    public decimal CalculeazaDobanda(Factura factura)
    {
        DateOnly utcNow = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        if(factura.DataScadenta.CompareTo(utcNow) >= 0) return 0m;

        var diferentaZile = utcNow.DayNumber - factura.DataScadenta.DayNumber;
        
        var maximZile = _procentPlafonMaxim/_procentDobandaZilnica;
        
        var rezultat = diferentaZile > maximZile ? factura.Suma * _procentPlafonMaxim : factura.Suma * _procentDobandaZilnica * diferentaZile; 
        return Math.Round(rezultat, 2, MidpointRounding.AwayFromZero);
    }

}
class Program
{
    static void Main(string[] args)
    {
        CalculatorDobanda calculatorDobanda = new CalculatorDobanda(0.01m, 0.2m);
        Factura factura = new Factura(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-20)), 1000m);

        var result = calculatorDobanda.CalculeazaDobanda(factura);
        Console.WriteLine(result);
    }
}