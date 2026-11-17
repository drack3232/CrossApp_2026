using System.Net;
using System.Net.Http.Json;
using Api.Contracts;
using Core.Dto;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Api.Tests;

public sealed class OrderEndpointsTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Post_CreatesOrder_And_GetByLocation_Returns200()
    {
        var request = new SaveOrderRequest(
            Customer: "Геральт з Рівії",
            Address: "Каер Морхен",
            Comment: "Тестове замовлення",
            Lines:
            [
                new SaveOrderLineRequest("P-01", "Срібний меч", 1500m, 1)
            ],
            IsConfirmed: false);

        var postResponse = await _client.PostAsJsonAsync("/api/orders", request);

        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        Assert.NotNull(postResponse.Headers.Location);

        var getResponse = await _client.GetAsync(postResponse.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var dto = await getResponse.Content.ReadFromJsonAsync<OrderDto>();
        Assert.NotNull(dto);
        Assert.Equal("Геральт з Рівії", dto.Customer);
        Assert.Equal(1500m, dto.Price);
    }

    [Fact]
    public async Task Post_WithInvalidPayload_Returns400ValidationProblem()
    {
        var invalidRequest = new SaveOrderRequest(
            Customer: "",
            Address: "",
            Comment: null,
            Lines: []);

        var response = await _client.PostAsJsonAsync("/api/orders", invalidRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetOrders_WithPaginationAndSearch_ReturnsPagedResult()
    {
        var request = new SaveOrderRequest(
            Customer: "Йеннефер з Венгерберга",
            Address: "Венгерберг",
            Comment: null,
            Lines:
            [
                new SaveOrderLineRequest("P-02", "Еліксир", 300m, 2)
            ]);

        await _client.PostAsJsonAsync("/api/orders", request);

        var paged = await _client.GetFromJsonAsync<PagedResponse<OrderDto>>(
            "/api/orders?page=1&pageSize=10&search=Йеннефер");

        Assert.NotNull(paged);
        Assert.True(paged.Total >= 1);
        Assert.Contains(paged.Items, o => o.Customer == "Йеннефер з Венгерберга");
    }
}