namespace NotificationService.Grpc.Features.EmailChannel.Shared;

// Namerno najprostiji mogući templating — dictionary lookup + string.Replace.
// Dovoljno za "OtpCode" danas; ako templejtova bude više ili treba pravi
// HTML layout, ovo je mesto da se zameni pravim engine-om (npr. Scriban),
// bez menjanja SendEmailViaGmail.Handler-a koji samo zove Render(...).
public static class EmailTemplates
{
    public static (string Subject, string HtmlBody) Render(
        string template, IReadOnlyDictionary<string, string> variables)
    {
        return template switch
        {
            "OtpCode" => (
                "Your DareToDance sign-in code",
                Fill(
                    "<p>Your sign-in code is <b>{{code}}</b>.</p>" +
                    "<p>It expires at {{expiresAtUtc}}.</p>",
                    variables)),

            _ => throw new ArgumentOutOfRangeException(
                nameof(template), template, "Unknown email template."),
        };
    }

    private static string Fill(string body, IReadOnlyDictionary<string, string> variables)
    {
        foreach (var (key, value) in variables)
        {
            body = body.Replace($"{{{{{key}}}}}", value);
        }

        return body;
    }
}
