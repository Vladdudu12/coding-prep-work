/*
Dată o listă mare de tranzacții deja sortate după dată, găsește eficient toate tranzacțiile dintr-un interval [dataStart, dataStop].
*/

/// Testează: binary search pentru a găsi limitele intervalului în O(log n) în loc de scanare liniară O(n)
/// Hint: două căutări binare — una pentru prima poziție ≥ dataStart, una pentru ultima poziție ≤ dataStop

using System;
using System.Collections.Generic;
using System.Linq;

public record Tranzactie(DateOnly Data, decimal Suma);

class Program
{
    static int PrimulIndexCuDataCelPutinEgala(List<Tranzactie> tranzactii, DateOnly target)
    {
        int low = 0, high = tranzactii.Count;
        while (low < high)
        {
            int mid = low + (high - low) / 2;
            if (tranzactii[mid].Data < target)
                low = mid + 1;
            else
                high = mid;
        }
        return low;
    }

    static int PrimulIndexCuDataMaiMare(List<Tranzactie> tranzactii, DateOnly target)
    {
        int low = 0, high = tranzactii.Count;
        while (low < high)
        {
            int mid = low + (high - low) / 2;
            if (tranzactii[mid].Data <= target)
                low = mid + 1;
            else
                high = mid;
        }
        return low;
    }

    static List<Tranzactie> GasesteTranzactii(List<Tranzactie> tranzactii, DateOnly dataStart, DateOnly dataStop)
    {
        if (tranzactii.Count == 0) return new List<Tranzactie>();
        if (dataStart > dataStop)
            throw new ArgumentException("dataStart nu poate fi după dataStop");

        int startIdx = PrimulIndexCuDataCelPutinEgala(tranzactii, dataStart);
        int stopIdxExclusiv = PrimulIndexCuDataMaiMare(tranzactii, dataStop);

        if (startIdx >= stopIdxExclusiv)
            return new List<Tranzactie>();

        return tranzactii.GetRange(startIdx, stopIdxExclusiv - startIdx);
    }

    static void Main(string[] args)
    {
        // listă deja sortată după dată, cu goluri intenționate (nu toate zilele au tranzacții)
        var baza = new DateOnly(2024, 1, 1);
        var tranzactii = new List<Tranzactie>
        {
            new(baza.AddDays(1), 100m),   // 2024-01-02
            new(baza.AddDays(1), 50m),    // 2024-01-02 (a doua tranzacție în aceeași zi)
            new(baza.AddDays(3), 200m),   // 2024-01-04
            new(baza.AddDays(5), 300m),   // 2024-01-06
            new(baza.AddDays(5), 75m),    // 2024-01-06
            new(baza.AddDays(8), 400m),   // 2024-01-09
            new(baza.AddDays(12), 500m),  // 2024-01-13
        };

        Console.WriteLine("--- Test 1: interval cu potriviri exacte la ambele capete ---");
        Afiseaza(GasesteTranzactii(tranzactii, baza.AddDays(1), baza.AddDays(5)));

        Console.WriteLine("\n--- Test 2: interval FĂRĂ nicio tranzacție exact la capete (2024-01-03 -> 2024-01-07) ---");
        Afiseaza(GasesteTranzactii(tranzactii, baza.AddDays(2), baza.AddDays(6)));

        Console.WriteLine("\n--- Test 3: interval fără nicio tranzacție (gol, dar valid) ---");
        Afiseaza(GasesteTranzactii(tranzactii, baza.AddDays(20), baza.AddDays(25)));

        Console.WriteLine("\n--- Test 4: interval care acoperă toată lista ---");
        Afiseaza(GasesteTranzactii(tranzactii, baza, baza.AddDays(30)));

        Console.WriteLine("\n--- Test 5: dataStart > dataStop (ar trebui să arunce excepție) ---");
        try
        {
            GasesteTranzactii(tranzactii, baza.AddDays(10), baza.AddDays(2));
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Excepție așteptată: {ex.Message}");
        }
    }

    static void Afiseaza(List<Tranzactie> rezultat)
    {
        if (rezultat.Count == 0)
        {
            Console.WriteLine("(nicio tranzacție găsită)");
            return;
        }
        foreach (var t in rezultat)
            Console.WriteLine($"{t.Data:yyyy-MM-dd} | {t.Suma}");
    }
}