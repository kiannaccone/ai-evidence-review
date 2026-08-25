namespace AiEvidenceReview;

/// <summary>A single fake contract or policy belonging to exactly one tenant.</summary>
public record ComplianceDocument(
    string Id,
    string TenantId,
    string Title,
    string Category,
    string Text);

/// <summary>The input to the one entry point.</summary>
public record AskRequest(string TenantId, string UserId, string Question);

/// <summary>A pointer back to the exact sentence an answer is based on.</summary>
public record Citation(
    string DocumentId,
    string DocumentTitle,
    string Topic,
    string Snippet);

/// <summary>The structured response returned to the caller.</summary>
public record AnswerResult(
    string Answer,
    IReadOnlyList<Citation> Citations,
    IReadOnlyList<string> MissingEvidence,
    string Warning);

/// <summary>One row in the audit log. Written for every request, including failures.</summary>
public record AuditEvent(
    string EventId,
    DateTimeOffset Timestamp,
    string TenantId,
    string UserId,
    // Redacted before storage so the audit log never becomes a second copy of PII.
    string Question,
    IReadOnlyList<string> TopicsDetected,
    IReadOnlyList<string> DocumentIdsReturned,
    int CitationCount,
    string Outcome);

/// <summary>An internal hit: one topic found in one document, with the sentence that proves it.</summary>
public record EvidenceMatch(Topic Topic, ComplianceDocument Document, string Snippet);
