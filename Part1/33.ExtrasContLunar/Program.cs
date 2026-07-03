/*
Descriere: Banca trebuie să genereze un extras de cont lunar pentru un utilizator. 
Ai la dispoziție soldul inițial al contului la momentul deschiderii lui și o listă completă cu toate tranzacțiile istorice. 
Pentru luna cerută, raportul trebuie să conțină: 
Soldul de Deschidere (Opening Balance la 1 ale lunii), Total Debit, Total Credit și Soldul de Închidere (Closing Balance la finalul lunii).
*/


/// Input/Output:
/// Statement GenerateStatement(int year, int month, decimal initialAccountBalance, List<Transaction> allTransactions)
/// Statement este un record care conține OpeningBalance, TotalDebit, TotalCredit, ClosingBalance.

/// Constrangeri:
/// 

/// Edge Cases:
/// 

/// Exemplu:
// // Sold inițial: 0. Tranzacții: [+100 (Ianuarie), -20 (Ianuarie), +50 (Februarie)]
// GenerateStatement(2026, 2, 0m, tranzactii);
// // Așteptat: OpeningBalance = 80m, TotalCredit = 50m, TotalDebit = 0m, ClosingBalance = 130m

public record Statement(decimal OpeningBalance, decimal TotalDebit, decimal TotalCredit, decimal ClosingBalance);

public class Bank
{
    public static Statement GenerateStatement(int year, int month, decimal initialAccountBalance, List<Transaction> allTransactions)
    {
        decimal balance = 0m;
        decimal totalDebit = 0m;
        decimal totalCredit = 0m;
        decimal openingBalance = 0m;
        decimal closingBalance = 0m;
        
        var validTransactions = allTransactions
            .Where(x => x.Year < year || (x.Year <= year && x.Month <= month)).ToList();
        openingBalance = initialAccountBalance + validTransactions.Where(x => x.Year < year || (x.Month < month && x.Year <= year)).Sum(x => x.Amount);
        closingBalance = initialAccountBalance + validTransactions.Sum(x => x.Amount);
        totalCredit = validTransactions.Where(x => x.Month == month && x.Year == year && x.Amount > 0).Sum(x => x.Amount);
        totalDebit = validTransactions.Where(x => x.Month == month && x.Year == year && x.Amount < 0).Sum(x => x.Amount);

        return new Statement(openingBalance, totalDebit, totalCredit, closingBalance);
    }

    public static Statement GenerateStatementV1(int year, int month, decimal initialAccountBalance, List<Transaction> allTransactions)
    {
        // pornim de la initialAccountBalance
        // in ordine cronologica, pana la luna din anul respectiv
        // cand ajungem la prima tranzactie din luna respectiva, salvam opening balance, si apoi calculam total credit, totaldebit si la ultima tranzactie, inchidem closing balance

        var orderedTransactions = allTransactions.OrderBy(x => x.Year).ThenBy(x => x.Month);
        decimal balance = 0m;
        decimal totalDebit = 0m;
        decimal totalCredit = 0m;
        decimal openingBalance = 0m;
        decimal closingBalance = 0m;
        bool firstTime = true;
        bool enteredCorrectMonth = false;
        foreach (var transaction in orderedTransactions)
        {
            if (transaction.Year == year && transaction.Month == month)
            {
                enteredCorrectMonth = true;
                if (firstTime)
                {
                    openingBalance = balance;
                    firstTime = !firstTime;
                }
                var amount = transaction.Amount;
                if (amount < 0)
                {
                    totalDebit += amount;
                }
                else
                {
                    totalCredit += amount;
                }

                balance += amount;

            }
            else
            {
                closingBalance = balance;
                balance += transaction.Amount;
                enteredCorrectMonth = false;
            }

            if (firstTime == false && enteredCorrectMonth == false)
            {
                return new Statement(openingBalance, totalDebit, totalCredit, closingBalance);
            }
        }

        return new Statement(openingBalance, totalDebit, totalCredit, closingBalance);
    }


}
public class Transaction
{
    public decimal Amount { get; }
    public int Month { get; }
    public int Year { get; }

    public Transaction(decimal amount, int month, int year)
    {
        Amount = amount;
        Month = month;
        Year = year;
    }
}

class Program
{
    static void Main(string[] args)
    {
        var transactions = new List<Transaction>();
        transactions.Add(new Transaction(100m, 1, 2026));
        transactions.Add(new Transaction(-20m, 1, 2026));
        transactions.Add(new Transaction(50m, 2, 2026));
        var statement = Bank.GenerateStatement(2026, 2, 0m, transactions);

        Console.WriteLine($"{statement.OpeningBalance} | {statement.TotalCredit} | {statement.TotalDebit} | {statement.ClosingBalance}");
    }
}