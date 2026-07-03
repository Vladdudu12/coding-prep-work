// /// <summary>
// /// Descriere: Pentru a proteja un API public de plăți împotriva atacurilor DDoS, 
// /// trebuie să limitezi numărul de cereri pe care le poate face un utilizator (API Key) 
// /// într-o fereastră de timp glisantă (sliding window). 
// /// De exemplu: maxim 5 cereri în ultimele 60 de secunde.
// /// </summary>
// /// 
// /// Input/Output
// /// bool IsAllowed(string apiKey, DateTime requestTime)
// /// Returnează true dacă cererea este permisă, sau false dacă limita a fost atinsă.
// /// 
// /// Constrangeri:
// /// Fereastra este "sliding", nu fixă. Adică la ora 10:01:15, analizezi intervalul strict 10:00:15 - 10:01:15.
// /// Curățarea memoriei: Nu trebuie să păstrezi în memorie timestamp-urile cererilor care au ieșit deja din fereastra de 60 de secunde.
// /// Thread-safety este necesar (mai multe request-uri pot veni concomitent pentru aceeași cheie).

// /// Exemplu:
// /// // Presupunem limită: 3 request-uri pe minut (60 sec)
// /// IsAllowed("key-1", "10:00:00"); // true
// /// IsAllowed("key-1", "10:00:10"); // true
// /// IsAllowed("key-1", "10:00:20"); // true
// /// IsAllowed("key-1", "10:00:50"); // false (a atins limita de 3)
// /// IsAllowed("key-1", "10:01:05"); // true (primul request de la 10:00:00 a expirat)

// /// Edge Cases:
// /// Burst-uri de request-uri sosite exact în aceeași milisecundă.
// /// Scurgeri de memorie (memory leaks) dacă utilizatorul face 1 milion de request-uri respinse. Păstrezi timestamp-urile celor respinse?


// class Program1
// {
//     const int AMOUNT_OF_REQUESTS = 3;
//     const int WINDOW_SECONDS = 60;
//     static IDictionary<string, SortedSet<DateTime>> requests = new Dictionary<string, SortedSet<DateTime>>();

//     static void Main(string[] args)
//     {
//         // am nevoie de o structura care sa tina api key (key) si lista de requesturi pe care o curatam constant. 
//         // mecanismul de curatare, in functie de primul request din lista (ordonat dupa timestamp)
//         requests.Add("key-1", new SortedSet<DateTime>());
//         // daca trece timpul necesar, dam wipe la tot ce e 
//         try
//         {
//             Console.WriteLine(IsAllowed("key-1", DateTime.Parse("10:00:00"))); // true
//             Console.WriteLine(IsAllowed("key-1", DateTime.Parse("10:00:10"))); // true
//             Console.WriteLine(IsAllowed("key-1", DateTime.Parse("10:00:20"))); // true
//             Console.WriteLine(IsAllowed("key-1", DateTime.Parse("10:00:50"))); // false (a atins limita de 3)
//             Console.WriteLine(IsAllowed("key-1", DateTime.Parse("10:01:05"))); // true (primul request de la 10:00:00 a expirat)
//         }
//         catch(Exception exc)
//         {
//             Console.WriteLine(exc.Message);
//         }
//     }

//     static bool IsAllowed(string apiKey, DateTime requestTime)
//     {
//         if (!requests.ContainsKey(apiKey)) throw new Exception("the api key is incorrect");
//         var timestamps = requests[apiKey];
//         //todo
//         //luam request time, formam fereastra cu request time - 1 min

//         var windowLowerBound = requestTime.AddSeconds(-WINDOW_SECONDS);

//         // trebuie sa verificam request count intre windowLowerBound si requestTime
//         var requestsCount = GetRequestsBetweenBounds(apiKey, windowLowerBound, requestTime);
//         if (requestsCount >= AMOUNT_OF_REQUESTS)
//             return false;

//         timestamps.Add(requestTime);
//         return true;
//     }

//     static int GetRequestsBetweenBounds(string apiKey, DateTime windowLowerBound, DateTime requestTime)
//     {
//         if (!requests.ContainsKey(apiKey)) throw new Exception("the api key is incorrect");

//         var timestamps = requests[apiKey];
//         var count = 0;
//         foreach (var timestamp in timestamps)
//         {
//             if (timestamp.CompareTo(windowLowerBound) >= 0 && timestamp.CompareTo(requestTime) < 0)
//             {
//                 count++;
//                 // Console.WriteLine($"{timestamp} pentru {windowLowerBound}-{requestTime}");
//             }
//         }

//         return count;
//     }
// }