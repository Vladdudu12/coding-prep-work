/*
Ai o listă de tranzacții bancare (sume) și o listă de facturi emise (sume). Găsește toate perechile (tranzacție, factură) a căror sumă coincide, pentru reconciliere automată
*/

/// Testează: hashing/dictionare pentru O(n) în loc de O(n²), gestionarea duplicatelor (mai multe facturi cu aceeași sumă)
/// Hint: Dictionary<decimal, List<int>> pentru a grupa index-urile după sumă

class Program
{
    static void Main(string[] args)
    {
        List<decimal> tranzactiiBancare = new List<decimal> { 100m, 200m, 500m, 3300m, 300m };
        List<decimal> facturiBancare = new List<decimal> { 500m, 3300m, 200m, 150m, 500m };

        // METODA 1 - Two Pointers

        var tranzactiiPeSuma = new Dictionary<decimal, List<int>>();

        for (int i = 0; i < tranzactiiBancare.Count; i++)
        {
            if (!tranzactiiPeSuma.TryGetValue(tranzactiiBancare[i], out var list))
                tranzactiiPeSuma[tranzactiiBancare[i]] = list = new List<int>();

            list.Add(i);
        }

        var perechiGasite = new List<(int trazactieIdx, int facturaIdx)>();

        for (int j = 0; j < facturiBancare.Count; j++)
        {
            if (tranzactiiPeSuma.TryGetValue(facturiBancare[j], out var candidati) && candidati.Count > 0)
            {
                int trazactieIdx = candidati[0];
                candidati.RemoveAt(0);
                perechiGasite.Add((trazactieIdx, j));
            }
        }

        // METODA 2 - ABORDARE QUEUE
        var t = tranzactiiBancare.Select((v, i) => (Suma: v, Index: i)).OrderBy(x => x.Suma).ToList();
        var f = facturiBancare.Select((v, i) => (Suma: v, Index: i)).OrderBy(x => x.Suma).ToList();

        int n = 0;
        int m = 0;
        var perechi = new List<(int TranzactieIdx, int FacturaIdx)>();
        while (n < t.Count && m < f.Count)
        {
            if (t[n].Suma == f[m].Suma)
            {
                perechi.Add((t[n].Index, f[m].Index));
                n++; m++;
            }
            else if (t[n].Suma < f[m].Suma)
            {
                n++;
            }
            else
            {
                m++;
            }
        }

        // traversam prin lista
        // adaugam in dictionar
        // cand ajungem la facturi, traversam facturile
        // verificam daca exista deja un entry in dictionar pentru cheia respectiva (valoarea facturii)
        // adaugam indexul 



        foreach (var (tr, fa) in perechiGasite)
            Console.WriteLine($"Tranzacție[{tr}] ({tranzactiiBancare[tr]}) ↔ Factură[{fa}] ({facturiBancare[fa]})");

        Console.WriteLine();
        foreach (var (tr, fa) in perechi)
            Console.WriteLine($"Tranzacție[{tr}] ({tranzactiiBancare[tr]}) ↔ Factură[{fa}] ({facturiBancare[fa]})");

    }
}