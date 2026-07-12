using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using DareToDance.Api.IntegrationTests.TestUtils;

namespace DareToDance.Api.IntegrationTests.Authentication;

public class AuthenticationFlowTests(DareToDanceApiFactory factory) : IClassFixture<DareToDanceApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static string UniqueEmail() => $"{Guid.NewGuid():N}@test.com";

    private string LastCodeFor(string email) =>
        factory.EmailSender.Sent.Last(sent => sent.Email == email).Code;

    private async Task<HttpResponseMessage> RegisterAsync(string email) =>
        await _client.PostAsJsonAsync("/auth/register", new { firstName = "Flow", lastName = "Test", email });

    private async Task<HttpResponseMessage> VerifyAsync(string email, string code) =>
        await _client.PostAsJsonAsync("/auth/verify-otp", new { email, code });

    [Fact]
    public async Task Register_Verify_Me_HappyPathIssuesAWorkingToken()
    {
        var email = UniqueEmail();

        var register = await RegisterAsync(email);
        Assert.Equal(HttpStatusCode.OK, register.StatusCode);

        var verify = await VerifyAsync(email, LastCodeFor(email));
        Assert.Equal(HttpStatusCode.OK, verify.StatusCode);

        using var auth = JsonDocument.Parse(await verify.Content.ReadAsStringAsync());
        var token = auth.RootElement.GetProperty("token").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));

        var me = new HttpRequestMessage(HttpMethod.Get, "/users/me");
        me.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var meResponse = await _client.SendAsync(me);

        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
        using var profile = JsonDocument.Parse(await meResponse.Content.ReadAsStringAsync());
        Assert.Equal(email, profile.RootElement.GetProperty("email").GetString());
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        var email = UniqueEmail();
        await RegisterAsync(email);

        var second = await RegisterAsync(email);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task VerifyOtp_WrongCode_Returns401()
    {
        var email = UniqueEmail();
        await RegisterAsync(email);
        var wrong = LastCodeFor(email) == "000000" ? "999999" : "000000";

        var verify = await VerifyAsync(email, wrong);

        Assert.Equal(HttpStatusCode.Unauthorized, verify.StatusCode);
    }

    [Fact]
    public async Task VerifyOtp_ReusedCode_Returns401()
    {
        var email = UniqueEmail();
        await RegisterAsync(email);
        var code = LastCodeFor(email);
        await VerifyAsync(email, code);

        var reuse = await VerifyAsync(email, code);

        Assert.Equal(HttpStatusCode.Unauthorized, reuse.StatusCode);
    }

    [Fact]
    public async Task VerifyOtp_NonNumericCode_Returns400()
    {
        var email = UniqueEmail();
        await RegisterAsync(email);

        var verify = await VerifyAsync(email, "abcdef");

        Assert.Equal(HttpStatusCode.BadRequest, verify.StatusCode);
    }

    [Fact]
    public async Task VerifyOtp_FiveWrongGuesses_LocksOutTheCorrectCodeWith403()
    {
        var email = UniqueEmail();
        await RegisterAsync(email);
        var code = LastCodeFor(email);
        var wrong = code == "000000" ? "999999" : "000000";

        for (var i = 0; i < 5; i++)
        {
            await VerifyAsync(email, wrong);
        }

        var locked = await VerifyAsync(email, code);

        Assert.Equal(HttpStatusCode.Forbidden, locked.StatusCode);
    }

    [Fact]
    public async Task Login_UnknownEmail_IsIndistinguishableFromKnownEmail()
    {
        var email = UniqueEmail();
        await RegisterAsync(email);

        var known = await _client.PostAsJsonAsync("/auth/login", new { email });
        var unknown = await _client.PostAsJsonAsync("/auth/login", new { email = UniqueEmail() });

        Assert.Equal(HttpStatusCode.OK, known.StatusCode);
        Assert.Equal(HttpStatusCode.OK, unknown.StatusCode);
        Assert.Equal(
            await known.Content.ReadAsStringAsync(),
            await unknown.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task ResendOtp_WithinCooldown_Returns409()
    {
        var email = UniqueEmail();
        await RegisterAsync(email);

        var resend = await _client.PostAsJsonAsync("/auth/resend-otp", new { email });

        Assert.Equal(HttpStatusCode.Conflict, resend.StatusCode);
    }

    [Fact]
    public async Task UsersMe_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
