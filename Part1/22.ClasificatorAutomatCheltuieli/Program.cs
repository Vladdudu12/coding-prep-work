/*
Descriere: Băncile moderne categorisesc automat tranzacțiile 
(ex: "Mâncare", "Transport"). 
Ai o listă de reguli de tipul (string Keyword, string Category, int Priority). 
Când primești textul extrasului de cont al unei tranzacții, trebuie să-i asignezi o categorie.
*/






// // Reguli: ("uber", "Transport", 1), ("uber eats", "Mâncare", 2)
// Categorize("Plata card Uber Eats Amsterdam"); // Returnează "Mâncare"
// Categorize("Uber BV trip"); // Returnează "Transport"

using System.Text.RegularExpressions;


/// Input/Output:
/// string Categorize(string description, List<CategoryRule> rules)
/// Dacă descrierea conține cuvântul cheie din regulă, tranzacția ia acea categorie. 
/// Dacă se potrivesc mai multe reguli, o alegi pe cea cu prioritatea mai mare. Dacă niciuna nu se potrivește, returnezi "Altele".
/// Constrangeri:
/// Case-insensitive (textul poate fi "UBer EatS", regula "uber").
/// Dacă prioritățile sunt egale la două reguli potrivite, alege-o pe cea cu cel mai lung cuvânt cheie (mai specific).
/// Edge Cases:
/// Text scurt care conține un keyword parțial (ex: "sub" din "Subway" se potrivește oare cu regula "sub" pentru abonamente?).
///  Hint: caută whole word match sau folosește expresii regulate simple.
/// Exemplu:
public record CategoryRule(string Keyword, string Category, int Priority);

public class Bank
{
    public static string CategorizeV1(string description, List<CategoryRule> rules)
    {
        List<CategoryRule> matches = new List<CategoryRule>();

        var orderedRules = rules.OrderByDescending(x => x.Priority).ToList();

        for (int i = 0; i < orderedRules.Count; i++)
        {
            if (description.Contains(orderedRules[i].Keyword, StringComparison.InvariantCultureIgnoreCase))
            {
                matches.Add(orderedRules[i]);
            }
        }

        if (matches.Count <= 0)
        {
            return "Altele";
        }
        // foreach(var match in matches)
        // {
        //     Console.WriteLine($"{match.Category} - {match.Keyword.Length} - {match.Priority}");
        // }
        return matches.OrderByDescending(x => x.Priority).ThenByDescending(x => x.Keyword.Length).First().Category;
    }

    public static string Categorize(string description, List<CategoryRule> rules)
    {
        if (string.IsNullOrWhiteSpace(description) || rules == null || rules.Count == 0)
        {
            return "Altele";
        }

        var matches = rules.Where(r => IsWholeWordMatch(description, r.Keyword));

        var best = matches
            .OrderByDescending(r => r.Priority)
            .ThenByDescending(r => r.Keyword.Length)
            .FirstOrDefault();
        
        return best?.Category ?? "Altele";
    }

    private static bool IsWholeWordMatch(string text, string keyword)
    {
        var pattern = $@"\b{Regex.Escape(keyword)}\b";
        return Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase);
    }
}
class Program
{
    static void Main(string[] args)
    {
        List<CategoryRule> rules = new List<CategoryRule>();
        rules.Add(new CategoryRule("uber", "Transport", 1));
        rules.Add(new CategoryRule("uber eats", "Mancare", 1));
        Console.WriteLine(Bank.Categorize("Plata card Uber Eats Amsterdam", rules));
        Console.WriteLine(Bank.Categorize("Uber BV trip", rules));
    }
}