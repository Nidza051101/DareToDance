namespace NotificationService.Grpc.Features.EmailChannel.Shared;

public static class EmailTemplates
{
    public static (string Subject, string HtmlBody) Render(
        string template,
        IReadOnlyDictionary<string, string> variables)
    {
        return template switch
        {
            "OtpCode" => (
                "Your DareToDance sign-in code",
                Fill(
                    """
                    <div style="font-family: Arial, sans-serif; max-width: 500px; margin: 0 auto; padding: 30px;">
                        <h2 style="margin-bottom: 10px;">DareToDance</h2>

                        <p>Your sign-in code is:</p>

                        <p style="font-size: 32px; font-weight: bold; letter-spacing: 6px;">
                            {{code}}
                        </p>

                        <p style="color: #666;">
                            This code expires at {{expiresAtUtc}}.
                        </p>

                        <p style="color: #999; font-size: 13px;">
                            If you didn't request this code, you can safely ignore this email.
                        </p>
                    </div>
                    """,
                    variables)),

            _ => throw new ArgumentOutOfRangeException(
                nameof(template),
                template,
                "Unknown email template.")
        };
    }

    private static string Fill(
        string template,
        IReadOnlyDictionary<string, string> variables)
    {
        foreach (var (key, value) in variables)
        {
            template = template.Replace($"{{{{{key}}}}}", value);
        }

        return template;
    }
}