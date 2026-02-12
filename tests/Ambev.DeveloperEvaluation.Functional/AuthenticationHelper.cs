using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.WebApi;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Auth.AuthenticateUserFeature;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Ambev.DeveloperEvaluation.Functional;

/// <summary>
/// Helper for authentication in functional tests.
/// </summary>
public static class AuthenticationHelper
{
    /// <summary>
    /// Creates a user with Manager role and authenticates, returning an HttpClient configured with the Bearer token.
    /// </summary>
    public static Task<HttpClient> CreateAuthenticatedClientAsync(WebApplicationFactory<Program> factory)
        => CreateAuthenticatedClientAsync(factory, UserRole.Manager);

    /// <summary>
    /// Creates a user with the specified role and authenticates, returning an HttpClient configured with the Bearer token.
    /// </summary>
    public static async Task<HttpClient> CreateAuthenticatedClientAsync(WebApplicationFactory<Program> factory, UserRole role)
    {
        var client = factory.CreateClient();

        var email = $"{role.ToString()}_{new Random(100).Next()}@gmail.com";
        var password = "Test@1234";

        var createUserRequest = new
        {
            username = "TestUser",
            password,
            phone = "11999999999",
            email,
            status = (int)UserStatus.Active,
            role = (int)role
        };

        var createResponse = await client.PostAsJsonAsync("/api/users", createUserRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var authRequest = new { email, password };
        var authResponse = await client.PostAsJsonAsync("/api/auth", authRequest);
        authResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var authContent = await authResponse.Content.ReadFromJsonAsync<ApiResponseWithData<AuthenticateUserResponse>>(jsonOptions);
        authContent.Should().NotBeNull();
        authContent!.Data.Should().NotBeNull();
        authContent.Data!.Token.Should().NotBeNullOrEmpty();

        var authenticatedClient = factory.CreateClient();
        authenticatedClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {authContent.Data.Token}");

        return authenticatedClient;
    }
}
