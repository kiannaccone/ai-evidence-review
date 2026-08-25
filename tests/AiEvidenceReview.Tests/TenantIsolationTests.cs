using Xunit;

namespace AiEvidenceReview.Tests;

public class TenantIsolationTests : AssistantFixture
{
    [Fact]
    public void NeverCitesADocumentBelongingToAnotherTenant()
    {
        var tenantBIds = InMemoryDocumentStore.Seed
            .Where(document => document.TenantId == "tenant-b")
            .Select(document => document.Id)
            .ToHashSet();

        var result = Ask("tenant-a", BroadQuestion);

        Assert.NotEmpty(result.Citations);
        Assert.All(result.Citations, citation =>
        {
            Assert.DoesNotContain(citation.DocumentId, tenantBIds);
            Assert.Contains("-a-", citation.DocumentId);
        });
    }

    [Fact]
    public void GivesEachTenantADifferentAnswerToTheSameQuestion()
    {
        var a = Ask("tenant-a", BroadQuestion);
        var b = Ask("tenant-b", BroadQuestion, userId: "analyst-2");

        Assert.NotEqual(a.Answer, b.Answer);

        // tenant-a has no retention clause; tenant-b does.
        Assert.Contains(a.MissingEvidence, line => line.Contains("data retention period"));
        Assert.Contains(b.Citations, citation => citation.Topic == "data-retention");
    }

    [Fact]
    public void ReturnsNothingForAnUnknownTenant()
    {
        var result = Ask("tenant-does-not-exist", BroadQuestion);

        Assert.Empty(result.Citations);
        Assert.Equal(3, result.MissingEvidence.Count);
    }

    [Theory]
    [InlineData("", "analyst-1")]
    [InlineData("   ", "analyst-1")]
    [InlineData("tenant-a", "")]
    public void RejectsARequestMissingCallerIdentity(string tenantId, string userId)
    {
        var request = new AskRequest(tenantId, userId, BroadQuestion);

        var error = Assert.Throws<ArgumentException>(() => Assistant.AnswerQuestion(request));
        Assert.Contains("tenantId and userId are required", error.Message);
    }
}
