using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional;

/// <summary>
/// Functional tests for Sales API.
/// </summary>
public class SalesApiTests : IClassFixture<SalesWebApplicationFactory>
{
    private readonly SalesWebApplicationFactory _factory;

    public SalesApiTests(SalesWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact(DisplayName = "Create sale with valid token should return 201")]
    public async Task CreateSale_ValidRequestWithAuth_Returns201()
    {
        var client = await AuthenticationHelper.CreateAuthenticatedClientAsync(_factory);

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

        var response = await client.PostAsJsonAsync("/api/sales", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        content.GetProperty("data").GetProperty("saleNumber").GetString().Should().StartWith("SALE-");
    }

    [Fact(DisplayName = "List sales with valid token should return 200")]
    public async Task ListSales_WithAuth_Returns200()
    {
        var client = await AuthenticationHelper.CreateAuthenticatedClientAsync(_factory);

        var response = await client.GetAsync("/api/sales?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "Sales endpoints without token should return 401")]
    public async Task Sales_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/sales?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
