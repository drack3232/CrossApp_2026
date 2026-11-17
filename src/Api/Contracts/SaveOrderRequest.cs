namespace Api.Contracts;

public sealed record SaveOrderLineRequest(
    string ProductId,
    string Name,
    decimal Price,
    int Quantity);

public sealed record SaveOrderRequest(
    string Customer,
    string Address,
    string? Comment,
    List<SaveOrderLineRequest> Lines,
    bool IsConfirmed = false)
{
    public bool TryValidate(out Dictionary<string, string[]> errors)
    {
        var found = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(Customer) || Customer.Length > 200)
            found[nameof(Customer)] = ["Ім'я клієнта обов'язкове (до 200 символів)."];

        if (string.IsNullOrWhiteSpace(Address) || Address.Length > 300)
            found[nameof(Address)] = ["Адреса доставки обов'язкова (до 300 символів)."];

        if (Comment is { Length: > 500 })
            found[nameof(Comment)] = ["Коментар не може перевищувати 500 символів."];

        if (Lines is null || Lines.Count == 0)
        {
            found[nameof(Lines)] = ["Замовлення повинно містити хоча б одну позицію."];
        }
        else
        {
            for (int i = 0; i < Lines.Count; i++)
            {
                var line = Lines[i];
                if (string.IsNullOrWhiteSpace(line.ProductId) || line.ProductId.Length > 50)
                    found[$"Lines[{i}].ProductId"] = ["ID товару обов'язковий (до 50 символів)."];

                if (string.IsNullOrWhiteSpace(line.Name) || line.Name.Length > 200)
                    found[$"Lines[{i}].Name"] = ["Назва товару обов'язкова (до 200 символів)."];

                if (line.Price < 0)
                    found[$"Lines[{i}].Price"] = ["Ціна не може бути від'ємною."];

                if (line.Quantity <= 0)
                    found[$"Lines[{i}].Quantity"] = ["Кількість має бути більшою за нуль."];
            }
        }

        errors = found;
        return found.Count == 0;
    }
}

public sealed record OrderTotalResponse(
    string OrderId,
    string Customer,
    decimal Total,
    int LinesCount,
    bool IsConfirmed);