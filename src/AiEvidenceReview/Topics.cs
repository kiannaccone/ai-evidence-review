namespace AiEvidenceReview;

/// <param name="Id">Stable identifier that appears in JSON output and audit events.</param>
/// <param name="QuestionKeywords">Words that, in a user's question, signal interest in this topic.</param>
/// <param name="DocumentKeywords">Words that, in a document sentence, count as evidence for this topic.</param>
/// <param name="MissingPhrase">Used to phrase a missingEvidence line when no document matches.</param>
public record Topic(
    string Id,
    string Label,
    IReadOnlyList<string> QuestionKeywords,
    IReadOnlyList<string> DocumentKeywords,
    string MissingPhrase);

public static class TopicCatalog
{
    public static readonly IReadOnlyList<Topic> All =
    [
        new Topic(
            Id: "breach-notification",
            Label: "breach notification",
            QuestionKeywords: ["breach notification", "breach", "notify", "notification", "incident"],
            DocumentKeywords: ["breach notification", "breach", "notify", "security incident"],
            MissingPhrase: "states a breach notification requirement"),

        new Topic(
            Id: "data-retention",
            Label: "data retention",
            QuestionKeywords: ["data retention", "retention", "retain", "how long", "delete"],
            DocumentKeywords: ["data retention", "retention", "retain", "retained"],
            MissingPhrase: "states a data retention period"),

        new Topic(
            Id: "audit-rights",
            Label: "audit rights",
            QuestionKeywords: ["audit rights", "audit", "inspect", "inspection"],
            DocumentKeywords: ["audit", "inspect"],
            MissingPhrase: "grants audit rights"),
    ];

    /// <summary>
    /// Keyword matching, not an LLM. It is deterministic, easy to test, and it
    /// cannot be talked out of its behaviour by text inside a document.
    /// </summary>
    public static IReadOnlyList<Topic> Detect(string? question)
    {
        var haystack = (question ?? string.Empty).ToLowerInvariant();

        return All
            .Where(topic => topic.QuestionKeywords.Any(haystack.Contains))
            .ToList();
    }
}
