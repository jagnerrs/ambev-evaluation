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

    [Fact(DisplayName = "Get sale by Id with valid token should return 200")]
    public async Task GetSale_WithAuth_Returns200()
    {
        var client = await AuthenticationHelper.CreateAuthenticatedClientAsync(_factory);
        var saleId = await CreateSaleAndGetId(client);

        var response = await client.GetAsync($"/api/sales/{saleId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        content.GetProperty("data").GetProperty("data").GetProperty("id").GetGuid().Should().Be(saleId);
    }

    [Fact(DisplayName = "Update sale with valid token should return 200")]
    public async Task UpdateSale_WithAuth_Returns200()
    {
        var client = await AuthenticationHelper.CreateAuthenticatedClientAsync(_factory);
        var (saleId, itemId) = await CreateSaleAndGetIds(client);

        var request = new
        {
            saleDate = DateTime.UtcNow.AddDays(1),
            customerId = Guid.NewGuid(),
            customerName = "Updated Customer",
            branchId = Guid.NewGuid(),
            branchName = "Updated Branch",
            items = new[]
            {
                new { id = (Guid?)itemId, productId = Guid.NewGuid(), productName = "Updated Product", quantity = 3, unitPrice = 15.00m }
            }
        };

        var response = await client.PutAsJsonAsync($"/api/sales/{saleId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "Delete sale with valid token should return 204")]
    public async Task DeleteSale_WithAuth_Returns204()
    {
        var client = await AuthenticationHelper.CreateAuthenticatedClientAsync(_factory);
        var saleId = await CreateSaleAndGetId(client);

        var response = await client.DeleteAsync($"/api/sales/{saleId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "Cancel sale with valid token should return 200")]
    public async Task CancelSale_WithAuth_Returns200()
    {
        var client = await AuthenticationHelper.CreateAuthenticatedClientAsync(_factory);
        var saleId = await CreateSaleAndGetId(client);

        var response = await client.PostAsync($"/api/sales/{saleId}/cancel", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "Cancel sale item with valid token should return 200")]
    public async Task CancelSaleItem_WithAuth_Returns200()
    {
        var client = await AuthenticationHelper.CreateAuthenticatedClientAsync(_factory);
        var (saleId, itemId) = await CreateSaleAndGetIds(client);

        var response = await client.PostAsJsonAsync($"/api/sales/{saleId}/items/{itemId}/cancel", new { });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static async Task<Guid> CreateSaleAndGetId(HttpClient client) => await CreateSaleAndGetIdInternal(client);

    private static async Task<(Guid SaleId, Guid ItemId)> CreateSaleAndGetIds(HttpClient client)
    {
        var saleId = await CreateSaleAndGetIdInternal(client);
        var getResponse = await client.GetAsync($"/api/sales/{saleId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var getContent = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
        var items = getContent.GetProperty("data").GetProperty("data").GetProperty("items");
        var itemId = items[0].GetProperty("id").GetGuid();
        return (saleId, itemId);
    }

    private static async Task<Guid> CreateSaleAndGetIdInternal(HttpClient client)
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

        var response = await client.PostAsJsonAsync("/api/sales", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        return content.GetProperty("data").GetProperty("id").GetGuid();
    }
}
