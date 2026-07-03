// /*
// Descriere: Sistemul tău depinde de un API terț care uneori cade. Pentru a nu aștepta timeout-uri de fiecare dată când API-ul este jos, implementezi un Circuit Breaker.
// Closed: Request-urile trec. Dacă eșuează de 3 ori consecutiv, trece în Open.
// Open: Toate request-urile sunt respinse instant (fără a apela API-ul terț) timp de 10 secunde.
// Half-Open: După cele 10s, se permite o singură cerere de test. Dacă reușește, starea devine Closed. Dacă eșuează, redevine Open pentru încă 10s.

// */

// /// Input/Output:
// /// bool CanExecute() – returnează true dacă apelul extern ar trebui efectuat.
// /// void RecordSuccess() / void RecordFailure() – apelate după execuția API-ului terț pentru a ajusta starea internă.


// /// Constrangeri:
// /// Starea Half-Open trebuie să permită strict un singur request de test să treacă. 
// /// Restul request-urilor sosite concurent trebuie să primească false (să fie tratate ca și cum ar fi Open) până la confirmarea finală.


// /// Edge Cases:
// /// Dacă starea este Half-Open și primul request a primit permisiunea să treacă, 
// /// dar rulează foarte greu, ce returnează CanExecute() pentru request-urile secunde care sosesc în acest interval? (Răspuns corect: false).
// /// Thread-safety la modificarea stărilor și la contorizarea eșecurilor succesive.


// /// Exemplu:


// class CircuitBreaker
// {
//     private Status _status;
//     private readonly int _maxFails;
//     private readonly TimeSpan _timeoutWindow;
//     private int _counter = 0;
//     private DateTime _timeoutTime;

//     public CircuitBreaker(int maxFails, TimeSpan timeoutWindow)
//     {
//         _status = Status.Closed;
//         _maxFails = maxFails;
//         _timeoutWindow = timeoutWindow;
//     }
//     public bool CanExecute() //– returnează true dacă apelul extern ar trebui efectuat.
//     {
//         switch (_status)
//         {
//             case Status.Closed:
//                 return true;
//                 break;
//             case Status.HalfOpen:
//                 var utcNow = DateTime.UtcNow;
//                 var timeDiff = utcNow - _timeoutTime;
//                 return timeDiff > _timeoutWindow;
//                 break;
//             case Status.Open:
//                 return false;
//                 break;
//             default:
//                 return false;
//                 break;
//         }
//     }

//     public void RecordSuccess() // apelate după execuția API-ului terț pentru a ajusta starea internă.
//     {
//         if (_status == Status.Open)
//         {
//             var utcNow = DateTime.UtcNow;
//             var timeDiff = utcNow - _timeoutTime;
//             if (timeDiff > _timeoutWindow) _status = Status.HalfOpen;
//         }
//         if (_status == Status.HalfOpen)
//         {
//             _status = Status.Closed;
//             Console.WriteLine("Status changed to Closed");
//         }
//     }

//     public void RecordFailure() // apelate după execuția API-ului terț pentru a ajusta starea internă.
//     {
//         if (_status == Status.Open)
//         {
//             var utcNow = DateTime.UtcNow;
//             var timeDiff = utcNow - _timeoutTime;
//             if (timeDiff > _timeoutWindow) _status = Status.HalfOpen;
//         }
//         if (_status == Status.Closed)
//         {
//             _counter++;
//             Console.WriteLine($"Failed counter up to {_counter}.");

//             if (_counter >= _maxFails)
//             {
//                 _status = Status.Open;
//                 _timeoutTime = DateTime.UtcNow;
//                 _counter = 0;
//                 Console.WriteLine($"Failed {_maxFails} times. Status changed to Open");
//             }
//         }
//         if (_status == Status.HalfOpen)
//         {
//             _status = Status.Open;
//             _timeoutTime = DateTime.UtcNow;
//             Console.WriteLine($"Failed the halfOpen check. Status changed to Open");
//         }
//     }

//     private enum Status
//     {
//         Closed,
//         Open,
//         HalfOpen
//     }
// }

// class Program
// {
//     static void Main(string[] args)
//     {
//         var circuitBreaker = new CircuitBreaker(3, TimeSpan.FromSeconds(10));
//         circuitBreaker.RecordFailure();
//         circuitBreaker.RecordFailure();
//         Console.WriteLine(circuitBreaker.CanExecute());
//         circuitBreaker.RecordFailure();
//         Console.WriteLine(circuitBreaker.CanExecute());
//         Thread.Sleep(10000);
//         circuitBreaker.RecordFailure();
//         Console.WriteLine(circuitBreaker.CanExecute());
//         Thread.Sleep(10000);
//         circuitBreaker.RecordSuccess();
//         Console.WriteLine(circuitBreaker.CanExecute());
//     }
// }