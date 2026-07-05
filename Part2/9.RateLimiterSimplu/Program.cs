/*
Design + implementare pentru a limita un client la N request-uri/minut (ex: token bucket sau sliding window).
*/



using System.Collections.Concurrent;


/// Testează: thread-safety, alegerea structurii de date potrivite
/// Hint: ConcurrentDictionary<string, Queue<DateTime>> per client pentru sliding window; discută token bucket ca alternativă mai eficientă

public interface IRateLimiter
{
    public void CallAPI(string clientId);
}

public class TokenBucketRateLimiter : IRateLimiter
{
    private readonly int _maxTokens;
    private readonly double _tokensPerMilisecond;

    private readonly ConcurrentDictionary<string, BucketState> _store = new();

    public TokenBucketRateLimiter(int maxRequestsPerMinute)
    {
        _maxTokens = maxRequestsPerMinute;
        _tokensPerMilisecond = (double)_maxTokens / TimeSpan.FromMinutes(1).TotalMilliseconds;
    }

    public void CallAPI(string clientId)
    {
        var bucket = _store.GetOrAdd(clientId, _ => new BucketState(_maxTokens));

        lock (bucket)
        {
            Refill(bucket);

            if (bucket.Tokens >= 1.0)
            {
                bucket.Tokens -= 1.0;
                Console.WriteLine($"[Aprobat] Client: {clientId}. Tokens rămași: {Math.Round(bucket.Tokens, 2)}");
            }
            else
            {
                Console.WriteLine($"[Respins] Client: {clientId}. Limita atinsă!");
            }
        }
    }

    private void Refill(BucketState bucket)
    {
        var now = DateTime.UtcNow;
        var timePassed = now - bucket.LastRefill;

        double tokensToAdd = timePassed.TotalMilliseconds * _tokensPerMilisecond;

        if (tokensToAdd > 0)
        {
            bucket.Tokens = Math.Min(_maxTokens, bucket.Tokens + tokensToAdd);
            bucket.LastRefill = now;
        }
    }

    private class BucketState
    {
        public double Tokens { get; set; }
        public DateTime LastRefill { get; set; }

        public BucketState(int initialTokens)
        {
            Tokens = initialTokens;
            LastRefill = DateTime.UtcNow;
        }
    }
}


public class RateLimiter : IRateLimiter
{
    private readonly int _maxRequestsPerMinute;
    private readonly ConcurrentDictionary<string, Queue<DateTime>> _store = new();

    public RateLimiter(int maxRequestsPerMinute)
    {
        _maxRequestsPerMinute = maxRequestsPerMinute;
    }

    public void CallAPI(string clientId)
    {
        var utcNow = DateTime.UtcNow;

        var clientQueue = _store.GetOrAdd(clientId, _ => new Queue<DateTime>());

        lock (clientQueue)
        {
            while (clientQueue.TryPeek(out DateTime result) && result.AddMinutes(1) < utcNow)
            {
                clientQueue.Dequeue();
                Console.WriteLine("A fost eliminat un apel pentru ca a trecut destul timp");
            }

            if (clientQueue.Count < _maxRequestsPerMinute)
            {
                clientQueue.Enqueue(utcNow);
                Console.WriteLine("A fost aprobat un nou apel");
            }
            else
            {
                Console.WriteLine("Nu a fost permis apelul, s-a atins limita");
            }
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        IRateLimiter rateLimiterBucket = new TokenBucketRateLimiter(5);

        Console.WriteLine("--- Rafală inițială (consumăm tot) ---");
        for (int i = 0; i < 7; i++)
        {
            rateLimiterBucket.CallAPI("C1");
        }

        Console.WriteLine("\n--- Așteptăm 12 secunde pentru a regenera 1 jeton ---");
        Thread.Sleep(12000); 
        rateLimiterBucket.CallAPI("C1"); // Ar trebui să treacă (1 jeton regenerat)
        rateLimiterBucket.CallAPI("C1"); // Ar trebui să pice (s-a consumat jetonul)

        IRateLimiter rateLimiter = new RateLimiter(5);

        rateLimiter.CallAPI("C1");
        rateLimiter.CallAPI("C1");
        rateLimiter.CallAPI("C1");
        rateLimiter.CallAPI("C1");
        rateLimiter.CallAPI("C1");
        rateLimiter.CallAPI("C1");
        rateLimiter.CallAPI("C1");
        Thread.Sleep(61000);
        rateLimiter.CallAPI("C1");
    }
}