using Xunit;

namespace AiEvidenceReview.Tests;

public class RedactionTests : AssistantFixture
{
    [Theory]
    [InlineData("SSN 123-45-6789 on file", "SSN [REDACTED_SSN] on file")]
    [InlineData("mail ops@example.com now", "mail [REDACTED_EMAIL] now")]
    [InlineData("account 4111111111111111", "account [REDACTED_ACCOUNT]")]
    [InlineData("card 4111-1111-1111-1111", "card [REDACTED_ACCOUNT]")]
    [InlineData("passport AB123456 attached", "passport [REDACTED_PASSPORT] attached")]
    public void MasksSsnsEmailsAndAccountNumbers(string input, string expected)
    {
        Assert.Equal(expected, Redactor.Redact(input));
    }

    [Fact]
    public void LeavesOrdinaryContractLanguageAlone()
    {
        const string text = "Vendor must notify Customer within 72 hours.";

        Assert.Equal(text, Redactor.Redact(text));
    }

    [Fact]
    public void RedactsPiiInsideAReturnedSnippet()
    {
        var result = Ask("tenant-a", "breach notification requirements");

        var citation = result.Citations.Single(c => c.DocumentId == "policy-a-002");
        Assert.DoesNotContain("@northwind-example.com", citation.Snippet);
        Assert.Contains("[REDACTED_EMAIL]", citation.Snippet);
    }
}
