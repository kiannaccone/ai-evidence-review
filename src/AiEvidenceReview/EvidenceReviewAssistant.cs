namespace AiEvidenceReview;

/// <summary>
/// The one entry point for the assistant.
///
/// Order of operations matters:
///   1. validate the caller
///   2. work out what is being asked
///   3. search *only* that tenant's documents
///   4. summarize from the matches, never from the raw corpus
///   5. write an audit event for every request, including failures
/// </summary>
public sealed class EvidenceReviewAssistant
{
    public const string LegalWarning = "This is not legal advice.";

    private readonly EvidenceSearch _search;
    private readonly IAuditLog _auditLog;

    public EvidenceReviewAssistant(IDocumentStore documents, IAuditLog auditLog)
    {
        _search = new EvidenceSearch(documents);
        _auditLog = auditLog;
    }

    public AnswerResult AnswerQuestion(AskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TenantId) || string.IsNullOrWhiteSpace(request.UserId))
        {
            _auditLog.Record(
                tenantId: request.TenantId ?? "(missing)",
                userId: request.UserId ?? "(missing)",
                question: request.Question ?? string.Empty,
                topicsDetected: [],
                documentIdsReturned: [],
                outcome: "rejected");

            throw new ArgumentException("tenantId and userId are required.", nameof(request));
        }

        var topics = TopicCatalog.Detect(request.Question);
        var matches = _search.Find(request.TenantId, topics);
        var summary = Summarizer.Summarize(request.TenantId, topics, matches);

        var citations = matches
            .Select(match => new Citation(
                DocumentId: match.Document.Id,
                DocumentTitle: match.Document.Title,
                Topic: match.Topic.Id,
                Snippet: match.Snippet))
            .ToList();

        var outcome = topics.Count == 0
            ? "no-topic-match"
            : citations.Count == 0
                ? "no-evidence"
                : "answered";

        _auditLog.Record(
            tenantId: request.TenantId,
            userId: request.UserId,
            question: request.Question ?? string.Empty,
            topicsDetected: topics.Select(topic => topic.Id).ToList(),
            documentIdsReturned: citations.Select(citation => citation.DocumentId).ToList(),
            outcome: outcome);

        return new AnswerResult(
            Answer: summary.Answer,
            Citations: citations,
            MissingEvidence: summary.MissingEvidence,
            Warning: LegalWarning);
    }
}
