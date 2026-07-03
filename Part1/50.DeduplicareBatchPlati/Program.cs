/*
Descriere: Sistemele bancare vechi (legacy) ne trimit fișiere batch care conțin erori:
uneori, aceeași plată (același ExternalReferenceId) apare de mai multe ori pe rânduri diferite, 
cu stări sau timestamp-uri de actualizare diferite. 
Înainte de a le procesa, trebuie să "cureți" lista, reținând doar cea mai recentă versiune pentru fiecare ExternalReferenceId. 
Ordinea finală a elementelor unice trebuie să respecte ordinea apariției inițiale în fișier.
*/

/// Input/Output:
/// List<PaymentRecord> Deduplicate(List<PaymentRecord> batch)

/// Constrangeri:
/// 

/// Edge Cases:
/// Menținerea ordinii de inserție se rezolvă ușor combinând un dicționar pentru urmărirea ultimelor elemente și lista sursă.
/// Două elemente cu același ID au exact același timestamp de actualizare (folosește ultimul citit în fișier pentru a suprascrie).


/// Exemplu:

using System.Collections.Concurrent;

public record PaymentRecord(string ExternalReferenceId, string Status, DateTime Timestamp);
public class BankSystem
{

    public static List<PaymentRecord> Deduplicate(List<PaymentRecord> batch)
    {   
        var result = new List<PaymentRecord>();
        var indexByRef = new Dictionary<string, int>();

        foreach (var record in batch)
        {
            if (indexByRef.TryGetValue(record.ExternalReferenceId, out var idx))
            {
                if (record.Timestamp >= result[idx].Timestamp)
                {
                    result[idx] = record;
                }
            }
            else
            {
                indexByRef[record.ExternalReferenceId] = result.Count;
                result.Add(record);
            }
        }

        return result;
    }

    public static List<PaymentRecord> DeduplicateV1(List<PaymentRecord> batch)
    {   
        ConcurrentDictionary<string, PaymentRecord> latest = new ConcurrentDictionary<string, PaymentRecord>();

        foreach(var record in batch)
        {
            latest.AddOrUpdate(
                record.ExternalReferenceId,
                addValue: record,
                updateValueFactory: (key, existing) =>
                {
                    if (existing.Timestamp.CompareTo(record.Timestamp) <= 0)
                    {
                        return record;
                    }

                    return existing;
                }
            );
        }

        return latest.Values.ToList();
    }
}

class Program
{
    static void Main(string[] args)
    {
        List<PaymentRecord> batch = new List<PaymentRecord>();
        batch.Add(new PaymentRecord("ref-1", "good", DateTime.UtcNow));
        batch.Add(new PaymentRecord("ref-1", "bad", DateTime.UtcNow.AddHours(1)));
        batch.Add(new PaymentRecord("ref-1", "better", DateTime.UtcNow.AddDays(1)));
        batch.Add(new PaymentRecord("ref-1", "great", DateTime.UtcNow.AddDays(1)));
        batch.Add(new PaymentRecord("ref-2", "good", DateTime.UtcNow));
        batch.Add(new PaymentRecord("ref-2", "bad", DateTime.UtcNow.AddHours(1)));
        batch.Add(new PaymentRecord("ref-2", "better", DateTime.UtcNow.AddDays(1)));
        var result = BankSystem.Deduplicate(batch);

        foreach(var r in result)
        {
            Console.WriteLine($"{r.ExternalReferenceId} : {r.Status} | {r.Timestamp}");
        }
    }
}