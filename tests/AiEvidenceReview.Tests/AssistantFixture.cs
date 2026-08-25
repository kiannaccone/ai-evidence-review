namespace AiEvidenceReview.Tests;

/// <summary>
/// xUnit creates a new instance of the test class for every test method, so the
/// constructor is the per-test setup (the equivalent of beforeEach). Each test
/// therefore gets its own audit log and no state leaks between tests.
/// </summary>
public abstract class AssistantFixture
{
    protected const string BroadQuestion =
        "Which policies or contracts mention breach notification, data retention, or audit rights?";

    protected readonly InMemoryAuditLog AuditLog;
    protected readonly EvidenceReviewAssistant Assistant;

    protected AssistantFixture()
    {
        AuditLog = new InMemoryAuditLog();
        Assistant = new EvidenceReviewAssistant(new InMemoryDocumentStore(), AuditLog);
    }

    protected AnswerResult Ask(string tenantId, string question, string userId = "analyst-1") =>
        Assistant.AnswerQuestion(new AskRequest(tenantId, userId, question));
}
