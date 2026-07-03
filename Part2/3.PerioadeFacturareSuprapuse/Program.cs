/*
Un client are mai multe abonamente recurente, fiecare cu un interval [data_start, data_stop]. Găsește dacă există suprapuneri (ceea ce ar însemna dublă facturare) și unifică intervalele suprapuse.
*/

/// Testează: sortare + algoritm de tip "merge intervals"
/// Hint: sortează după data_start, apoi parcurge liniar comparând cu ultimul interval "deschis"

class Program
{
    static void Main(string[] args)
    {
        List<(int data_start, int data_final)> abonamente = new List<(int data_start, int data_final)> { (1, 3), (2, 5), (4, 6), (6, 8), (9, 12) };

        var abonamenteOrdonate = abonamente.OrderBy(x => x.data_start).ToList();
        List<(int data_start, int data_final)> abonamenteMerged = new List<(int data_start, int data_final)>();
        var data_start_draft = 0;
        var data_final_draft = 0;
        foreach (var abonament in abonamenteOrdonate)
        {
            // verificam daca data de inceput e diferita ( daca e mai mare decat data de start draft si mai mica decat data_final_draft)
            // daca e adevarat, atunci verificam data_final si o comparam cu data_final_draft. Daca e mai mare decat draft, atunci draft devine data_final. 

            //inchidem intervalul cand data de inceput e >= data_final_draft

            if (data_start_draft == 0 && data_final_draft == 0)
            {
                data_start_draft = abonament.data_start;
                data_final_draft = abonament.data_final;
            }

            if (abonament.data_start >= data_final_draft)
            {
                abonamenteMerged.Add((data_start_draft, data_final_draft));
                data_start_draft = abonament.data_start;
                data_final_draft = abonament.data_final;
            }
            else if (abonament.data_final > data_final_draft)
            {
                data_final_draft = abonament.data_final;
            }

        }
        if (data_start_draft != 0 && data_final_draft != 0) //Fragil daca nu e cu int de la 0
            abonamenteMerged.Add((data_start_draft, data_final_draft));

        // Console.WriteLine($"{data_start_draft} - {data_final_draft}");

        foreach (var x in abonamenteMerged)
        {
            Console.WriteLine($"{x.data_start} - {x.data_final}");
        }
    }
}