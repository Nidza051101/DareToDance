using DareToDance.Api.IntgrationTests.Common;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace DareToDance.Api.IntgrationTests.Features.RegisterUserTest;

public class RegisterUserTest : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RegisterUserTest(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RegisterUser_Should_ReturnCreated()
    {
        var request = new
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            Phone = "123456789"
        };

        var response = await _client.PostAsJsonAsync("/users/register",request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
