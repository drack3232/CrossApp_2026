using System.Security.Cryptography;
using Api.Contracts;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrders(this WebApplication app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders");

        // 1. GET /api/orders — список усіх замовлень
        group.MapGet("/", (IOrderStore store) =>
            TypedResults.Ok(store.List().Select(o => o.ToDto()).ToList()));

        // 2. GET /api/orders/{id} — деталі замовлення
        group.MapGet("/{id}", Results<Ok<OrderDto>, NotFound>
            (string id, IOrderStore store) =>
            store.GetById(id) is { } found
                ? TypedResults.Ok(found.ToDto())
                : TypedResults.NotFound());

        // Додатковий маршрут варіанта: GET /api/orders/{id}/total
        group.MapGet("/{id}/total", Results<Ok<OrderTotalResponse>, NotFound>
            (string id, IOrderStore store) =>
            store.GetById(id) is { } found
                ? TypedResults.Ok(new OrderTotalResponse(
                    found.Id,
                    found.Customer,
                    found.Total,
                    found.Lines.Count,
                    found.IsConfirmed))
                : TypedResults.NotFound());

        // 3. POST /api/orders — створення замовлення
        group.MapPost("/", Results<Created<OrderDto>, ValidationProblem>
            (SaveOrderRequest request, IOrderStore store) =>
        {
            if (!request.TryValidate(out var errors))
                return TypedResults.ValidationProblem(errors);

            var id = Convert.ToHexString(RandomNumberGenerator.GetBytes(4)).ToLowerInvariant();
            var created = BuildOrder(id, request);

            store.Add(created);
            return TypedResults.Created($"/api/orders/{created.Id}", created.ToDto());
        });

        // 4. PUT /api/orders/{id} — повна заміна замовлення
        group.MapPut("/{id}", Results<Ok<OrderDto>, NotFound, ValidationProblem>
            (string id, SaveOrderRequest request, IOrderStore store) =>
        {
            if (!request.TryValidate(out var errors))
                return TypedResults.ValidationProblem(errors);

            if (store.GetById(id) is null)
                return TypedResults.NotFound();

            var replacement = BuildOrder(id, request);
            store.Update(replacement);
            return TypedResults.Ok(replacement.ToDto());
        });

        // 5. DELETE /api/orders/{id} — видалення замовлення
        group.MapDelete("/{id}", Results<NoContent, NotFound>
            (string id, IOrderStore store) =>
            store.Remove(id)
                ? TypedResults.NoContent()
                : TypedResults.NotFound());
    }

    private static Order BuildOrder(string id, SaveOrderRequest request)
    {
        var order = Order.Create(id, request.Customer, request.Address, request.Comment);

        foreach (var line in request.Lines)
        {
            order.AddLine(line.ProductId, line.Name, line.Price, line.Quantity);
        }

        if (request.IsConfirmed)
        {
            order.Confirm();
        }

        return order;
    }
}