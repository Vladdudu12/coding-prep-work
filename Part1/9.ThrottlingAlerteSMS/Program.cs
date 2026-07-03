/*
Descriere: Sistemul trimite SMS-uri cu OTP (One Time Password). 
Pentru a preveni costurile mari și spam-ul, 
un număr de telefon poate primi maximum 3 SMS-uri în oricare fereastră de o oră. 
Spre deosebire de rate limiter-ul de la problema #2 
(care suporta mii de request-uri), aici volumul per user este mic, dar fereastra este mare (1 oră).
*/






// CanSendSms("0700111222"); // true (10:00)
// CanSendSms("0700111222"); // true (10:15)
// CanSendSms("0700111222"); // true (10:30)
// CanSendSms("0700111222"); // false (10:45) - a atins limita

// // La ora 11:05
// CanSendSms("0700111222"); // true (Primul SMS de la 10:00 a ieșit din fereastra de 1h)

using System.Collections.Concurrent;


/// Input/Output:
/// bool CanSendSms(string phoneNumber)
/// Constrangeri:
/// Trebuie să folosești o fereastră glisantă exactă (sliding window log). 
/// Deoarece limitezi la max 3 mesaje, stocarea timestamp-urilor (în loc de simple contoare) este foarte ieftină și preferabilă.
/// Edge Cases:
/// Evacuarea memoriei: la apelul funcției, trebuie să ștergi din colecția numărului respectiv timestamp-urile mai vechi de 1 oră.
/// Alegerea structurii de date: o coadă (Queue<DateTime>) per utilizator este ideală aici.
/// Exemplu:
class SMSService
{
    private readonly int _maxRequests;
    private readonly TimeSpan _window;

    private readonly ConcurrentDictionary<string, (Queue<DateTime> Timestamps, object Lock)> _store = new();

    public SMSService(int maxRequests, TimeSpan window)
    {
        _maxRequests = maxRequests;
        _window = window;
    }

    public bool CanSendSms(string phoneNumber)
    {
        var utcNow = DateTime.UtcNow;
        var entry = _store.GetOrAdd(phoneNumber, _ => (new Queue<DateTime>(), new object()));
        lock (entry.Lock)
        {
            var lowerBound = utcNow - _window;

            while(entry.Timestamps.Count > 0 && entry.Timestamps.Peek() < lowerBound) 
                entry.Timestamps.Dequeue();
            
            if (entry.Timestamps.Count >= _maxRequests) return false;

            entry.Timestamps.Enqueue(utcNow);
            return true;
        }
    }

    

}

class Program
{
    static void Main(string[] args)
    {
        var smsService = new SMSService(3, TimeSpan.FromSeconds(5));
        var r1 = smsService.CanSendSms("0700111222"); // true (10:00)
        var r2 = smsService.CanSendSms("0700111222"); // true (10:15)
        var r3 = smsService.CanSendSms("0700111222"); // true (10:30)
        var r4 = smsService.CanSendSms("0700111222"); // false (10:45) - a atins limita
        Thread.Sleep(5000);
        // La ora 11:05
        var r5 = smsService.CanSendSms("0700111222"); // true (Primul SMS de la 10:00 a ieșit din fereastra de 1h)
        Console.WriteLine($"{r1}-{r2}-{r3}-{r4}-{r5}");
    }
}