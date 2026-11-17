using System.Security.Cryptography;
using Api.Contracts;
using Api.Filters;
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

        group.MapGet("/", (
            IOrderStore store,
            int page = 1,
            int pageSize = 20,
            string? search = null) =>
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            IEnumerable<Order> query = store.List();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(o =>
                    o.Id.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    o.Customer.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    o.Address.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    o.Lines.Any(l => l.Name.Contains(term, StringComparison.OrdinalIgnoreCase)));
            }

            var filtered = query.ToList();
            var total = filtered.Count;
            var items = filtered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => o.ToDto())
                .ToList();

            return TypedResults.Ok(new PagedResponse<OrderDto>(items, total));
        });

        group.MapGet("/{id}", Results<Ok<OrderDto>, NotFound>
            (string id, IOrderStore store) =>
            store.GetById(id) is { } found
                ? TypedResults.Ok(found.ToDto())
                : TypedResults.NotFound());

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

        group.MapPost("/", Results<Created<OrderDto>, ValidationProblem>
            (SaveOrderRequest request, IOrderStore store) =>
        {
            var id = Convert.ToHexString(RandomNumberGenerator.GetBytes(4)).ToLowerInvariant();
            var created = BuildOrder(id, request);

            store.Add(created);
            return TypedResults.Created($"/api/orders/{created.Id}", created.ToDto());
        })
        .AddEndpointFilter<ValidationFilter<SaveOrderRequest>>();

group.MapPost("/{id}/lines", Results<Ok<OrderDto>, NotFound>
    (string id, SaveOrderLineRequest line, IOrderStore store) =>
{
    if (store.GetById(id) is not { } order)
        return TypedResults.NotFound();

    order.AddLine(line.ProductId, line.Name, line.Price, line.Quantity);
    store.Update(order);
    return TypedResults.Ok(order.ToDto());
});

        group.MapPut("/{id}", Results<Ok<OrderDto>, NotFound, ValidationProblem>
            (string id, SaveOrderRequest request, IOrderStore store) =>
        {
            if (store.GetById(id) is null)
                return TypedResults.NotFound();

            var replacement = BuildOrder(id, request);
            store.Update(replacement);
            return TypedResults.Ok(replacement.ToDto());
        })
        .AddEndpointFilter<ValidationFilter<SaveOrderRequest>>();

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