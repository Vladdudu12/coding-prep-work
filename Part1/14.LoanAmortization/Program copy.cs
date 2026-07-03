// /*
// Descriere: Când un utilizator ia un credit cu rată fixă, 
// banca generează un scadențar (Amortization Schedule). 
// Deși suma lunară de plată rămâne constantă, 
// proporția dintre principal (banii împrumutați) 
// și dobândă se schimbă în fiecare lună: la început plătești multă dobândă, iar la final plătești mai mult principal.
// */

// /// Input/Output:
// /// List<MonthlyInstallment> GenerateSchedule(decimal principal, decimal annualInterestRate, int months)
// /// MonthlyInstallment conține: MonthNumber, PrincipalPaid, InterestPaid, RemainingBalance. Toate rotunjite la 2 zecimale.


// /// Constrangeri:
// /// Formula ratei lunare (Annuity): PMT = P * (r * (1 + r)^n) / ((1 + r)^n - 1), unde r este rata lunară (annualRate / 12).
// /// Soldul rămas la finalul ultimei luni trebuie să fie exact 0.00.


// /// Edge Cases:
// /// Din cauza rotunjirilor de 2 zecimale în fiecare lună, ultima lună va avea un sold rămas de câțiva bani pe plus sau pe minus. 
// /// Scadențarul corect ajustează ultimul PrincipalPaid pentru a forța soldul la 0.
// /// Rata anuală a dobânzii este 0% (se împarte pur și simplu principalul la numărul de luni, evitând împărțirea la zero din formulă).


// /// Exemplu:
// // GenerateSchedule(10000m, 0.12m, 12);
// // // Rata lunară estimată va fi ~888.49m
// // // Luna 1: Interest = 10000 * (0.12/12) = 100m, Principal = 788.49m, Remaining = 9211.51m

// public class InstallmentScheduler
// {
//     public static List<MonthlyInstallment> GenerateSchedule(decimal principal, decimal annualInterestRate, int months)
//     {
//         var r = annualInterestRate / 12;
//         //InterestToPay

//         var remaining = principal;
//         List<MonthlyInstallment> monthlyInstallments = new List<MonthlyInstallment>();
//         for (int i = 0; i < months; i++)
//         {
//             Console.WriteLine(i);
//             var PMT = principal * (r * (decimal)Math.Pow((double)(1 + r), months)) / ((decimal)Math.Pow((double)(1 + r), months) - 1);
//             var amountToPayMonthly = Math.Round(PMT, 2, MidpointRounding.AwayFromZero);
//             var interestToPay = principal * r;
//             var principalToPay = Math.Round(amountToPayMonthly - interestToPay, 2, MidpointRounding.AwayFromZero);
//             remaining = Math.Max(0, remaining -= amountToPayMonthly);
//             monthlyInstallments.Add(new MonthlyInstallment(i + 1, principalToPay, interestToPay, remaining));
//         }

//         return monthlyInstallments;
//     }
// }

// public class MonthlyInstallment
// {
//     public decimal MonthNumber { get; }
//     public decimal PrincipalPaid { get; set; }
//     public decimal InterestPaid { get; set; }
//     public decimal RemainingBalance { get; set; }

//     public MonthlyInstallment(decimal monthNumber, decimal principalPaid, decimal interestPaid, decimal remainingBalance)
//     {
//         MonthNumber = monthNumber;
//         PrincipalPaid = principalPaid;
//         InterestPaid = interestPaid;
//         RemainingBalance = remainingBalance;
//     }

//     public override string ToString()
//     {
//         return $"Month number: {MonthNumber} | Principal Paid: {PrincipalPaid} | Interest Paid: {InterestPaid} | Remaining Balance: {RemainingBalance}";
//     }
// }

// class Program
// {
//     static void Main(string[] args)
//     {
//         var monthlyInstallments = InstallmentScheduler.GenerateSchedule(10000m, 0.12m, 12);
//         decimal interestSum = 0m;
//         decimal principalSum = 0m;
//         foreach (var installment in monthlyInstallments)
//         {
//             interestSum += installment.InterestPaid;
//             principalSum += installment.PrincipalPaid;
//             Console.WriteLine(installment);
//         }

//         Console.WriteLine($"interest sum: {interestSum} | principal sum: {principalSum}");
//     }
// }