/*
Descriere: Într-o aplicație de investiții (brokeraj), utilizatorul cumpără acțiuni AAPL la date și prețuri diferite.
Când vinde o parte din acțiuni, regula fiscală obligă aplicarea metodei FIFO (First In, First Out)
– se consideră că vinde primele acțiuni cumpărate. 
Trebuie să calculezi "Realized Profit" (profitul realizat) pe baza prețului de achiziție al pachetelor vândute.
*/

/// Input/Output:
/// decimal CalculateRealizedProfit(Queue<BuyOrder> inventory, int quantityToSell, decimal sellPrice)
/// Returnează profitul (sau pierderea) obținută din vânzare. Modifică coada inventory (sau returnează noua stare) pentru a consuma acțiunile.


/// Constrangeri:
/// Nu poți vinde mai multe acțiuni decât ai în inventar.
/// Un ordin de vânzare poate "sparge" un ordin de cumpărare (ex: ai cumpărat 10 acțiuni, dar vinzi doar 3; cele 7 rămân în FIFO).


/// Edge Cases:
/// Vânzarea unei cantități care se aliniază perfect cu granițele ordinelor de cumpărare (fără resturi).
/// Tratarea excepțiilor: aruncă o eroare clară dacă quantityToSell > total_inventory.


/// Exemplu:
// // Inventar: Cumpărat 10 @ $100; Cumpărat 10 @ $150
// CalculateRealizedProfit(inventory, 15, 200m);
// // Se vând primele 10 (cumpărate la 100): profit 10 * (200 - 100) = 1000
// // Se vând încă 5 (din cele cumpărate la 150): profit 5 * (200 - 150) = 250
// // Profit total returnat: 1250m. În inventar mai rămân 5 acțiuni @ $150.


// BuyOrder - nrActiuni, Data, Pretul
// data pentru ca bagam Queue, nu ne intereseaza, decat daca nu e sortata lista

public class Broker
{
    public static decimal CalculateRealizedProfit(Queue<BuyOrder> inventory, int quantityToSell, decimal sellPrice)
    {
        decimal result = 0m;
        int totalShares = 0;
        foreach (var order in inventory)
        {
            totalShares += order.NumberOfShares;
        }
        if (quantityToSell > totalShares) throw new ArgumentException("we don't have enough shares to sell");

        while (quantityToSell > 0)
        {
            var order = inventory.Peek();
            var sellableInThisOrder = Math.Min(order.NumberOfShares, quantityToSell);
            result += order.SellShares(sellableInThisOrder, sellPrice);
            quantityToSell -= sellableInThisOrder;
            if (order.NumberOfShares <= 0) inventory.Dequeue();
        }
        return result;
    }
}

public class BuyOrder
{
    public int NumberOfShares { get; private set;}
    public decimal PricePerShare { get; }

    public BuyOrder(int numberOfShares, decimal pricePerShare)
    {
        NumberOfShares = numberOfShares;
        PricePerShare = pricePerShare;
    }

    public decimal SellShares(int quantityToSell, decimal priceToSell)
    {
        // teoretic nu se va ajunge aici vreodata
        if (quantityToSell > NumberOfShares) throw new ArgumentException("the quantity cannot exceed the current number of shares");

        NumberOfShares -= quantityToSell;
        var realizedPrice = quantityToSell * (priceToSell - PricePerShare);
        return realizedPrice;
    }
}


class Program
{
    static void Main(string[] args)
    {
        Queue<BuyOrder> inventory = new Queue<BuyOrder>();
        inventory.Enqueue(new BuyOrder(10, 100m));
        inventory.Enqueue(new BuyOrder(10, 150m));
        
        try
        {
            var result = Broker.CalculateRealizedProfit(inventory, 15, 200m);
            Console.WriteLine(result);
        }
        catch (Exception exc)
        {
            Console.WriteLine(exc.Message);
        }

        foreach(var order in inventory)
        {
            Console.WriteLine($"{order.NumberOfShares} @ {order.PricePerShare}");
        }
    }
}