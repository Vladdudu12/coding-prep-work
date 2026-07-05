/*
Primești un fișier CSV cu tranzacții bancare pentru import. Unele rânduri au date corupte (sumă invalidă, dată lipsă). Scrie o funcție care importă rândurile valide și raportează clar care rânduri au eșuat și de ce.
*/


using System.Globalization;


/// Testează: error handling fără excepții pentru control flow, design de tip Result<T> sau listă de erori, product thinking (ce faci cu rândurile bune?)
/// Hint: nu arunca excepție per rând — colectează erorile într-o listă și continuă procesarea
public record CSVEntry(DateTime Data, decimal Suma, string Categorie);

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public IReadOnlyList<string> Errors { get; }

    private Result(bool isSuccess, T? value, IReadOnlyList<string> errors)
    {
        IsSuccess = isSuccess;
        Value = value;
        Errors = errors;
    }

    public static Result<T> Success(T value) => new(true, value, Array.Empty<string>());
    public static Result<T> Failure(params string[] errors) => new(false, default, errors);
    public static Result<T> Failure(IReadOnlyList<string> errors) => new(false, default, errors);
}
class Program
{
    public static Result<CSVEntry> ParseRow(string rand, int numarLinie)
    {
        var splitValues = rand.Split(',');

        if (splitValues.Length != 3)
        {
            return Result<CSVEntry>.Failure($"Linia {numarLinie}: format invalid, astept 3 campuri");
        }

        var erori = new List<string>();

        DateTime? data = null;

        if (string.IsNullOrWhiteSpace(splitValues[0]))
        {
            erori.Add($"Linia {numarLinie}: data lipsa");
        }
        else if (!DateTime.TryParseExact(splitValues[0].Trim(), "yyyy-MM-dd",
             CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
        {
            erori.Add($"Linia {numarLinie}: data invalida");
        }
        else
        {
            data = d;
        }

        decimal? suma = null;
        if (string.IsNullOrWhiteSpace(splitValues[1]))
        {
            erori.Add($"Linia {numarLinie}: suma lipsa");
        }
        else if (!decimal.TryParse(splitValues[1].Trim(), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                 CultureInfo.InvariantCulture, out var s))
        {
            erori.Add($"Linia {numarLinie}: suma invalida");
        }
        else
        {
            suma = s;
        }

        var categorie = splitValues[2].Trim();
        if (string.IsNullOrWhiteSpace(categorie))
        {
            erori.Add($"Linia {numarLinie}: categorie lipsă");
        }

        if (erori.Count > 0)
        {
            return Result<CSVEntry>.Failure(erori);
        }

        return Result<CSVEntry>.Success(new CSVEntry(data!.Value, suma!.Value, categorie));
    }

    public record ImportRezultat(List<CSVEntry> Valide, List<string> Erori);

    public static ImportRezultat ParseCSV(string[] linii, bool hasHeader)
    {
        if (linii.Length == 0)
        {
            return new ImportRezultat(new List<CSVEntry>(), new List<string> { "Fisierul CSV este gol" });
        }

        var rezultate = new List<Result<CSVEntry>>();
        for (int i = hasHeader ? 1 : 0; i < linii.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(linii[i])) continue;
            rezultate.Add(ParseRow(linii[i], i + 1));
        }

        var valide = rezultate.Where(r => r.IsSuccess).Select(r => r.Value!).ToList();
        var erori = rezultate.Where(r => !r.IsSuccess).SelectMany(r => r.Errors).ToList();

        return new ImportRezultat(valide, erori);
    }

    public static (List<CSVEntry> CSVEntries, List<string> Errors) ParseCSV_V1(string[] csv, bool hasHeader)
    {
        if (csv.Length == 0) throw new Exception("The CSV file is empty");
        var correctEntries = new List<CSVEntry>();
        List<string> errors = new List<string>();
        for (var i = hasHeader ? 1 : 0; i < csv.Length; i++)
        {
            var splitValues = csv[i].Split(',');
            var rowErrorCounter = 0;
            DateTime? data = null;
            decimal? suma = null;
            string categorie = string.Empty;

            if (splitValues.Length == 0)
            {
                errors.Add($"randul [{i}]: este complet gol");
                continue;
            }
            else if (splitValues.Length != 3)    // mai exista cazul in care nu avem toate virgulele necesare deci nu avem destule elemente pentru array de split values
            {
                errors.Add($"randul [{i}]: este invalid");
                continue;
            }


            if (string.IsNullOrWhiteSpace(splitValues[0].Trim()))
            {
                errors.Add($"randul [{i}]: Data lipsa");
                rowErrorCounter++;
            }
            else if (!DateTime.TryParseExact(splitValues[0].Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
            {
                errors.Add($"randul [{i}]: Data este invalida");
                rowErrorCounter++;
            }
            else
            {
                data = d;
            }

            if (string.IsNullOrWhiteSpace(splitValues[1].Trim()))
            {
                errors.Add($"randul [{i}]: Suma lipsa");
                rowErrorCounter++;
            }
            else if (!decimal.TryParse(splitValues[1].Trim(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var s))
            {
                errors.Add($"randul [{i}]: Suma este invalida");
                rowErrorCounter++;
            }
            else
            {
                suma = s;
            }

            if (string.IsNullOrWhiteSpace(splitValues[2].Trim()))
            {
                errors.Add($"randul [{i}]: Categorie lipsa");
                rowErrorCounter++;
            }
            else
            {
                categorie = splitValues[2].Trim();
            }

            if (rowErrorCounter == 0 && data.HasValue && suma.HasValue && !string.IsNullOrEmpty(categorie))
            {
                correctEntries.Add(new CSVEntry(data.Value, suma.Value, categorie));
            }
        }

        return (correctEntries, errors);
    }

    static void Main(string[] args)
    {
        string[] linii = new[]
        {
            "Data,Suma,Categorie",                    // header
            "2024-01-15,1250.50,Mancare",             // validă
            "2024-01-16,abc,Transport",               // sumă invalidă (nenumerică)
            "2024-01-17,300.00,",                     // categorie lipsă
            ",450.75,Utilitati",                       // dată lipsă
            "2024-01-20,-999999999999999999999,Mancare", // sumă care depășește range-ul decimal
            "2024-13-40,200.00,Transport",             // dată invalidă (lună 13, ziua 40)
            "2024-01-22,100.25,Mancare",               // validă
            "",                                        // rând complet gol
            "2024-01-23,75,Calatorii"                  // validă
        };

        (var CSVEntries, var errors) = ParseCSV(linii, true);

        if (errors.Count == 0) Console.WriteLine("No import errors");
        else
        {
            Console.WriteLine($"The import had {errors.Count} issues:");
            foreach (var err in errors)
            {
                Console.WriteLine(err);
            }
        }

        Console.WriteLine();
        Console.WriteLine("These are the imported entries:");
        foreach (var entry in CSVEntries)
        {
            Console.WriteLine($"{entry.Data} | {entry.Suma} | {entry.Categorie}");
        }
    }
}