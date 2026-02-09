using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ambev.DeveloperEvaluation.WebApi;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional;

/// <summary>
/// Functional tests for Sales API.
/// </summary>
public class SalesApiTests : IClassFixture<SalesWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SalesApiTests(SalesWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact(DisplayName = "Create sale should return 201")]
    public async Task CreateSale_ValidRequest_Returns201()
    {
        var request = new
        {
            saleDate = DateTime.UtcNow,
            customerId = Guid.NewGuid(),
            customerName = "Test Customer",
            branchId = Guid.NewGuid(),
            branchName = "Test Branch",
            items = new[]
            {
                new { productId = Guid.NewGuid(), productName = "Product 1", quantity = 2, unitPrice = 10.00m }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/sales", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        content.GetProperty("data").GetProperty("saleNumber").GetString().Should().StartWith("SALE-");
    }

    [Fact(DisplayName = "List sales should return 200")]
    public async Task ListSales_Returns200()
    {
        var response = await _client.GetAsync("/api/sales?page=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
