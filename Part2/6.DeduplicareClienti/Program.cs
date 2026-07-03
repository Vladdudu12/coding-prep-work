/*
Ai o listă de clienți importați dintr-un CSV, unii introduși de mai multe ori cu mici diferențe (spații, majuscule, diacritice). Scrie o funcție care grupează înregistrările "probabil identice".
*/

/// Testează: normalizare de string, gândire despre ce înseamnă "similar" (exact match pe CUI e sigur; pe nume, ambiguu)
/// Hint: discută explicit cu intervievatorul ce nivel de fuzzy-matching se așteaptă — e o problemă deschisă intenționat

class Program
{
    public record Client(string CUI, string Nume);
    static void Main(string[] args)
    {
        var clienti = new List<Client>();
        clienti.Add(new Client("2", "BBB"));
        clienti.Add(new Client("1", "A A"));
        clienti.Add(new Client("2", " B BB"));
        clienti.Add(new Client("1 ", "A a"));
        clienti.Add(new Client(" 1", " A a"));
        clienti.Add(new Client("3", "CC"));
        clienti.Add(new Client("1", "a A"));
        clienti.Add(new Client("3", "CCC _c "));
        clienti.Add(new Client("2", "BBB _B"));

        var result = FindDuplicates(clienti);
        if (result != null)
        {
            foreach (var (key, value) in result)
            {
                foreach (var client in value)
                {
                    Console.WriteLine($"{key}: {client}");
                }
                Console.WriteLine();
            }
        }
    }

    static Dictionary<string, List<Client>> FindDuplicates(List<Client> clients)
    {
        if (clients.Count <= 0) return null;

        var result = new Dictionary<string, List<Client>>();
        for (int i = 0; i < clients.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(clients[i].CUI)) continue;
            var CUI = clients[i].CUI.Trim().Normalize(System.Text.NormalizationForm.FormD);
            var nume = clients[i].Nume.Trim().Normalize(System.Text.NormalizationForm.FormD);

            if (result.TryGetValue(CUI, out var clientList)) clientList.Add(new Client(CUI, nume));
            else result.Add(CUI, new List<Client> { new Client(CUI, nume) });
        }

        return result;
    }
}