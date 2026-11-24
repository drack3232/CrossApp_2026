using Core;
using Core.Abstractions;
using Core.Domain;
namespace Infrastructure.Persistence;

public static class DbSeeder
{
    public static void Seed(AppDbContext db, IOrderStore store)
    {
        if (db.Orders.Any())
            return;

        var first = Order.Create("seed0001", "Геральт з Рівії", "Каер Морхен", "Стартове замовлення");
        first.AddLine("P-01", "Срібний меч", 1500m, 1);

        var second = Order.Create("seed0002", "Йеннефер з Венгерберга", "Венгерберг", null);
        second.AddLine("P-02", "Зілля Ластівка", 250m, 2);
        second.Confirm();

        store.Add(first);
        store.Add(second);
    }
}