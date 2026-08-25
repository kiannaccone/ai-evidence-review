using Xunit;

namespace AiEvidenceReview.Tests;

public class AuditTests : AssistantFixture
{
    [Fact]
    public void WritesExactlyOneEventPerRequest()
    {
        Ask("tenant-a", "audit rights?");
        Ask("tenant-b", "audit rights?", userId: "analyst-2");

        Assert.Equal(2, AuditLog.Events.Count);
        Assert.Single(AuditLog.ForTenant("tenant-a"));
    }

    [Fact]
    public void RecordsWhoAskedWhatWasDetectedAndWhatWasReturned()
    {
        var result = Ask("tenant-a", "What are our breach notification obligations?");

        var auditEvent = Assert.Single(AuditLog.Events);
        Assert.Equal("tenant-a", auditEvent.TenantId);
        Assert.Equal("analyst-1", auditEvent.UserId);
        Assert.Equal("answered", auditEvent.Outcome);
        Assert.Equal("breach-notification", Assert.Single(auditEvent.TopicsDetected));
        Assert.Equal(
            string.Join(",", result.Citations.Select(citation => citation.DocumentId)),
            string.Join(",", auditEvent.DocumentIdsReturned));
        Assert.True(auditEvent.Timestamp > DateTimeOffset.MinValue);
        Assert.False(string.IsNullOrWhiteSpace(auditEvent.EventId));
    }

    [Fact]
    public void LogsRequestsThatFoundNothing()
    {
        Ask("tenant-a", "What is our retention period?");

        var auditEvent = Assert.Single(AuditLog.Events);
        Assert.Equal("no-evidence", auditEvent.Outcome);
        Assert.Empty(auditEvent.DocumentIdsReturned);
    }

    [Fact]
    public void LogsARejectedRequestBeforeThrowing()
    {
        Assert.Throws<ArgumentException>(() =>
            Assistant.AnswerQuestion(new AskRequest("", "analyst-1", "audit rights?")));

        var auditEvent = Assert.Single(AuditLog.Events);
        Assert.Equal("rejected", auditEvent.Outcome);
    }

    [Fact]
    public void RedactsPiiOutOfTheLoggedQuestion()
    {
        Ask("tenant-a", "Does the breach policy cover SSN 123-45-6789?");

        var auditEvent = Assert.Single(AuditLog.Events);
        Assert.DoesNotContain("123-45-6789", auditEvent.Question);
        Assert.Contains("[REDACTED_SSN]", auditEvent.Question);
    }
}
