// /*
// Descriere: Într-un sistem fintech, clienții (aplicațiile mobile) pot face "Retry" 
// dacă o cerere de plată dă timeout pe rețea, trimițând exact același Idempotency-Key. 
// Implementează un serviciu in-memory care previne taxarea dublă a clientului. 
// Dacă o cerere cu o cheie nouă sosește, plata este "executată". 
// Dacă sosește o cerere cu o cheie deja văzută,sistemul trebuie să returneze 
// direct rezultatul primei procesări, fără a executa plata a doua oară.
// */

// /// Input/Output:
// /// Metoda: PaymentResult ProcessPayment(string idempotencyKey, decimal amount)
// /// Se presupune că execuția efectivă a plății (simulată) durează ~100ms.
// /// Returnează un PaymentResult (ex: TransactionId, Status).


// /// Constrangeri:
// /// Metoda va fi apelată puternic concurent.
// /// Sistemul trebuie să reziste la un "Race Condition" sever: 
// /// ce se întâmplă dacă Thread A și Thread B sosesc cu exact același Idempotency-Key fix 
// /// în aceeași milisecundă? O singură execuție trebuie să aibă loc.


// /// Edge Cases:
// /// Request-ul simultan (Overlapping): Thread 2 sosește când Thread 1 abia a început să proceseze cererea "key-123", 
// /// dar nu a terminat (rezultatul nu e gata). 
// /// Thread 2 nu are ce rezultat să returneze încă, dar nici nu are voie să execute plata. 
// /// Cum blochezi Thread 2 să aștepte terminarea lui Thread 1 pentru acea cheie specifică?
// /// Structura de date: folosirea ConcurrentDictionary cu metoda GetOrAdd și Lazy<T> este soluția clasică aici în C#.


// /// Exemplu:
// // // Thread 1:
// // ProcessPayment("key-123", 50m); // Durează 100ms, returnează TX_001

// // // Thread 2 (sosit după 2 secunde):
// // ProcessPayment("key-123", 50m); // Returnează instant TX_001, fără a mai executa simularea.
// using System.Collections.Concurrent;
// public record PaymentResult(string TransactionId, string Status);
// public class PaymentProcessor
// {  
//     private readonly ConcurrentDictionary<string, (Lazy<PaymentResult> PaymentResult, decimal Amount)> _transactions = new();

//     public PaymentProcessor()
//     {
//     }
//     public PaymentResult ProcessPayment(string idempotencyKey, decimal amount)
//     {
//         var result = _transactions.AddOrUpdate(
//             idempotencyKey,
//             addValueFactory: _ => (
//                 new Lazy<PaymentResult>(new PaymentResult(GenerateTransactionId(), "Processed")),
//                 amount
//             ),
//             updateValueFactory: (_, existing) => existing
//         );
//         Console.WriteLine($"key: {idempotencyKey} | transactionID: {result.PaymentResult.Value.TransactionId} | status: {result.PaymentResult.Value.Status} ");
//         return result.PaymentResult.Value;
//     }


//     private string GenerateTransactionId()
//     {
//         return Guid.NewGuid().ToString();
//     }
// }

// class Program
// {
//     static void Main(string[] args)
//     {
//         var paymentProcessor = new PaymentProcessor();

//         Thread t1 = new Thread(() => paymentProcessor.ProcessPayment("key-123", 50m));
//         Thread t2 = new Thread(() => paymentProcessor.ProcessPayment("key-123", 50m));
        
//         t1.Start();
//         t2.Start();

//         t1.Join();
//         t2.Join();
//     }
// }