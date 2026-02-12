using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional;

/// <summary>
/// Functional tests for Health Check API.
/// </summary>
public class HealthCheckApiTests : IClassFixture<SalesWebApplicationFactory>
{
    private readonly SalesWebApplicationFactory _factory;

    public HealthCheckApiTests(SalesWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact(DisplayName = "GET /health/live returns 200 with Healthy status")]
    public async Task HealthLive_Returns200WithHealthyStatus()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        json.RootElement.GetProperty("status").GetString().Should().Be("Healthy");
    }

    [Fact(DisplayName = "GET /health/ready returns 200 with Healthy status")]
    public async Task HealthReady_Returns200WithHealthyStatus()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/ready");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        json.RootElement.GetProperty("status").GetString().Should().Be("Healthy");
    }

    [Fact(DisplayName = "GET /health returns 200")]
    public async Task Health_Returns200()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
