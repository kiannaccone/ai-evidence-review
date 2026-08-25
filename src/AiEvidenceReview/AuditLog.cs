namespace AiEvidenceReview;

public interface IAuditLog
{
    AuditEvent Record(
        string tenantId,
        string userId,
        string question,
        IReadOnlyList<string> topicsDetected,
        IReadOnlyList<string> documentIdsReturned,
        string outcome);

    IReadOnlyList<AuditEvent> Events { get; }

    IReadOnlyList<AuditEvent> ForTenant(string tenantId);
}

/// <summary>
/// In-memory audit log. In a real system this would be an append-only store
/// (or a write to a log pipeline) that the application cannot edit or delete.
///
/// It is injected rather than static, so every test gets a clean log without a
/// reset helper, and a future implementation can be swapped in without touching
/// the assistant.
/// </summary>
public sealed class InMemoryAuditLog : IAuditLog
{
    private readonly List<AuditEvent> _events = [];

    public AuditEvent Record(
        string tenantId,
        string userId,
        string question,
        IReadOnlyList<string> topicsDetected,
        IReadOnlyList<string> documentIdsReturned,
        string outcome)
    {
        var stored = new AuditEvent(
            EventId: Guid.NewGuid().ToString(),
            Timestamp: DateTimeOffset.UtcNow,
            TenantId: tenantId,
            UserId: userId,
            Question: Redactor.Redact(question),
            TopicsDetected: topicsDetected,
            DocumentIdsReturned: documentIdsReturned,
            CitationCount: documentIdsReturned.Count,
            Outcome: outcome);

        _events.Add(stored);
        return stored;
    }

    public IReadOnlyList<AuditEvent> Events => _events;

    public IReadOnlyList<AuditEvent> ForTenant(string tenantId) =>
        _events
            .Where(auditEvent => auditEvent.TenantId == tenantId)
            .ToList();
}
