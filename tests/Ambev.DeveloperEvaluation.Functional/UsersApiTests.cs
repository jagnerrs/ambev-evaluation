using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.CreateUser;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional;

/// <summary>
/// Functional tests for Users API.
/// </summary>
public class UsersApiTests : IClassFixture<SalesWebApplicationFactory>
{
    private readonly SalesWebApplicationFactory _factory;

    public UsersApiTests(SalesWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact(DisplayName = "Admin user can delete user and returns 200")]
    public async Task DeleteUser_AdminRole_Returns200()
    {
        var client = _factory.CreateClient();
        var userToDeleteId = await CreateUserAsync(client);

        var adminClient = await AuthenticationHelper.CreateAuthenticatedClientAsync(_factory, UserRole.Admin);

        var response = await adminClient.DeleteAsync($"/api/users/{userToDeleteId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "Manager user cannot delete user and returns 403")]
    public async Task DeleteUser_ManagerRole_Returns403()
    {
        var client = _factory.CreateClient();
        var userToDeleteId = await CreateUserAsync(client);

        var managerClient = await AuthenticationHelper.CreateAuthenticatedClientAsync(_factory, UserRole.Manager);

        var response = await managerClient.DeleteAsync($"/api/users/{userToDeleteId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "Customer user cannot delete user and returns 403")]
    public async Task DeleteUser_CustomerRole_Returns403()
    {
        var client = _factory.CreateClient();
        var userToDeleteId = await CreateUserAsync(client);

        var customerClient = await AuthenticationHelper.CreateAuthenticatedClientAsync(_factory, UserRole.Customer);

        var response = await customerClient.DeleteAsync($"/api/users/{userToDeleteId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "DELETE users without token returns 401")]
    public async Task DeleteUser_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();
        var userToDeleteId = await CreateUserAsync(client);

        var response = await client.DeleteAsync($"/api/users/{userToDeleteId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static async Task<Guid> CreateUserAsync(HttpClient client)
    {
        var email = $"user_{Guid.NewGuid():N}@example.com";
        var createRequest = new
        {
            username = "UserToDelete",
            password = "Test@1234",
            phone = "11999999999",
            email,
            status = (int)UserStatus.Active,
            role = (int)UserRole.Customer
        };

        var createResponse = await client.PostAsJsonAsync("/api/users", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var createContent = await createResponse.Content.ReadFromJsonAsync<ApiResponseWithData<CreateUserResponse>>(jsonOptions);
        createContent.Should().NotBeNull();
        createContent!.Data.Should().NotBeNull();

        return createContent.Data!.Id;
    }
}
