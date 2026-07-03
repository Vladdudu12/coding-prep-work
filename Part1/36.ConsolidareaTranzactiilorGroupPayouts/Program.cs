/*
Descriere: O platformă de food-delivery procesează mii de comenzi, 
dar nu face un transfer bancar către restaurant pentru fiecare comandă în parte. 
La finalul zilei, sumele sunt consolidate într-un singur transfer (Payout) pe zi/restaurant.
Din acest transfer consolidat se scade o taxă fixă de transfer bancar de 5 RON.
*/

/// Input/Output:
/// List<Payout> CalculatePayouts(List<Order> orders, decimal flatTransferFee)
/// Order conține RestaurantId, Date, Amount.
/// Payout conține RestaurantId, Date, FinalAmountTransfered.


/// Constrangeri:
/// 

/// Edge Cases:
/// Totalul comenzilor unui restaurant într-o zi este mai mic decât taxa de transfer 
/// (ex: are comenzi de 3 RON, taxa e 5 RON). Ce se întâmplă? 
/// Payout-ul nu ar trebui să fie negativ; se raportează 0 sau nu se efectuează transferul deloc.
/// Gruparea eficientă folosind LINQ (GroupBy(x => new { x.RestaurantId, x.Date.Date })).


/// Exemplu:
// Comenzi pentru RestA azi: 50 RON, 30 RON, 20 RON. Taxa fixă: 5 RON.
// Payout calculat: (50 + 30 + 20) - 5 = 95 RON.

public record Order(string RestaurantId, DateTime Date, decimal Amount);
public record Payout(string RestaurantId, DateTime Date, decimal FinalAmountTransfered);

public class FoodApp
{

    public static List<Payout> CalculatePayouts(List<Order> orders, decimal flatTransferFee)
    {
        return orders
            .GroupBy(o => new { o.RestaurantId, Date = o.Date.Date})
            .Select(g =>
            {
                var total = g.Sum(o => o.Amount);
                var final = Math.Max(0m, total - flatTransferFee);
                return new Payout(g.Key.RestaurantId, g.Key.Date, Math.Round(final, 2));
            })
            .ToList();
    }

    public static List<Payout> CalculatePayoutsV1(List<Order> orders, decimal flatTransferFee)
    {
        var result = new List<Payout>();
        var groupedByRestaurant = orders.GroupBy(x => new { x.RestaurantId, x.Date.Date }).ToList();
        foreach (var orderList in groupedByRestaurant)
        {
            var restaurantId = orderList.FirstOrDefault()?.RestaurantId;
            var date = orderList.FirstOrDefault()?.Date;
            var sumToPay = orderList.Sum(o => o.Amount) - flatTransferFee;
            result.Add(new Payout(restaurantId, date.Value, sumToPay));
        }

        return result;
    }
}

class Program
{
    static void Main(string[] args)
    {
        List<Order> orders = new List<Order>();
        orders.Add(new Order("res-1", DateTime.UtcNow, 50m));
        orders.Add(new Order("res-1", DateTime.UtcNow.AddSeconds(5), 30m));
        orders.Add(new Order("res-1", DateTime.UtcNow.AddSeconds(10), 20m));
        orders.Add(new Order("res-1", DateTime.UtcNow.AddDays(1), 100m));
        orders.Add(new Order("res-1", DateTime.UtcNow.AddDays(1).AddSeconds(5), 40m));
        orders.Add(new Order("res-1", DateTime.UtcNow.AddDays(1).AddSeconds(10), 50m));

        orders.Add(new Order("res-2", DateTime.UtcNow, 50m));
        orders.Add(new Order("res-2", DateTime.UtcNow.AddSeconds(5), 30m));
        orders.Add(new Order("res-2", DateTime.UtcNow.AddSeconds(10), 20m));
        orders.Add(new Order("res-2", DateTime.UtcNow.AddDays(1), 100m));
        orders.Add(new Order("res-2", DateTime.UtcNow.AddDays(1).AddSeconds(5), 40m));
        orders.Add(new Order("res-2", DateTime.UtcNow.AddDays(1).AddSeconds(10), 50m));
        var result = FoodApp.CalculatePayouts(orders, 5m);

        foreach (var res in result)
        {
            Console.WriteLine($"{res.RestaurantId} - {res.Date.Date} | {res.FinalAmountTransfered}");
        }
    }
}