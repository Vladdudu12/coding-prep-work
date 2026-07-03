/*
Descriere: Sistemul tău depinde de un API terț care uneori cade. Pentru a nu aștepta timeout-uri de fiecare dată când API-ul este jos, implementezi un Circuit Breaker.
Closed: Request-urile trec. Dacă eșuează de 3 ori consecutiv, trece în Open.
Open: Toate request-urile sunt respinse instant (fără a apela API-ul terț) timp de 10 secunde.
Half-Open: După cele 10s, se permite o singură cerere de test. Dacă reușește, starea devine Closed. Dacă eșuează, redevine Open pentru încă 10s.

*/

/// Input/Output:
/// bool CanExecute() – returnează true dacă apelul extern ar trebui efectuat.
/// void RecordSuccess() / void RecordFailure() – apelate după execuția API-ului terț pentru a ajusta starea internă.


/// Constrangeri:
/// Starea Half-Open trebuie să permită strict un singur request de test să treacă. 
/// Restul request-urilor sosite concurent trebuie să primească false (să fie tratate ca și cum ar fi Open) până la confirmarea finală.


/// Edge Cases:
/// Dacă starea este Half-Open și primul request a primit permisiunea să treacă, 
/// dar rulează foarte greu, ce returnează CanExecute() pentru request-urile secunde care sosesc în acest interval? (Răspuns corect: false).
/// Thread-safety la modificarea stărilor și la contorizarea eșecurilor succesive.


/// Exemplu:


class CircuitBreaker
{
    private enum Status
    {
        Closed,
        Open,
        HalfOpen
    }

    private readonly object _lock = new object();
    private readonly int _maxFails;
    private readonly TimeSpan _timeoutWindow;

    private Status _status = Status.Closed;
    private int _consecutiveFails = 0;
    private DateTime _openedAt;
    private bool _halfOpenTestInFlight = false;

    public CircuitBreaker(int maxFails, TimeSpan timeoutWindow)
    {
        _maxFails = maxFails;
        _timeoutWindow = timeoutWindow;
    }
    public bool CanExecute() //– returnează true dacă apelul extern ar trebui efectuat.
    {
        lock (_lock)
        {
            switch (_status)
            {
                case Status.Closed:
                    return true;

                case Status.Open:
                    if (DateTime.UtcNow - _openedAt >= _timeoutWindow)
                    {
                        _status = Status.HalfOpen;
                        _halfOpenTestInFlight = true;
                        return true;
                    }
                    return false;

                case Status.HalfOpen:
                    if (!_halfOpenTestInFlight)
                    {
                        _halfOpenTestInFlight = true;
                        return true;
                    }
                    return false;

                default:
                    return false;
            }
        }

    }

    public void RecordSuccess() // apelate după execuția API-ului terț pentru a ajusta starea internă.
    {
        lock (_lock)
        {
            _status = Status.Closed;
            _consecutiveFails = 0;
            _halfOpenTestInFlight = false;
        }
    }

    public void RecordFailure() // apelate după execuția API-ului terț pentru a ajusta starea internă.
    {
        lock (_lock)
        {
            if (_status == Status.HalfOpen)
            {
                OpenCircuit();
                return;
            }

            _consecutiveFails++;
            if (_consecutiveFails >= _maxFails)
            {
                OpenCircuit();
            }
        }
    }

    private void OpenCircuit()
    {
        _status = Status.Open;
        _openedAt = DateTime.UtcNow;
        _consecutiveFails = 0;
        _halfOpenTestInFlight = false;
    }


}

class Program
{
    static void Main(string[] args)
    {
        var circuitBreaker = new CircuitBreaker(3, TimeSpan.FromSeconds(10));
        circuitBreaker.RecordFailure();
        circuitBreaker.RecordFailure();
        Console.WriteLine(circuitBreaker.CanExecute());
        circuitBreaker.RecordFailure();
        Console.WriteLine(circuitBreaker.CanExecute());
        Thread.Sleep(10000);
        Console.WriteLine(circuitBreaker.CanExecute());
        circuitBreaker.RecordFailure();
        Console.WriteLine(circuitBreaker.CanExecute());
        Thread.Sleep(10000);
        Console.WriteLine(circuitBreaker.CanExecute());
        circuitBreaker.RecordSuccess();
        Console.WriteLine(circuitBreaker.CanExecute());

    }
}