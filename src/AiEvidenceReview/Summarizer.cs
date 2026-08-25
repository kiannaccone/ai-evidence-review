namespace AiEvidenceReview;

public record Summary(string Answer, IReadOnlyList<string> MissingEvidence);

/// <summary>
/// Deterministic mock summarizer.
///
/// It only counts and phrases what <see cref="EvidenceSearch"/> already returned,
/// so the answer can never assert something that has no citation behind it.
///
/// This is also the seam where a real LLM would go: the prompt would receive the
/// matched snippets as *data* (clearly delimited, never concatenated into the
/// instruction block), and the citation list would still be built from the
/// matches rather than from anything the model wrote.
/// </summary>
public static class Summarizer
{
    public static Summary Summarize(
        string tenantId,
        IReadOnlyList<Topic> topicsAsked,
        IReadOnlyList<EvidenceMatch> matches)
    {
        if (topicsAsked.Count == 0)
        {
            return new Summary(
                Answer: "I could not map this question to a supported topic (breach notification, data retention, or audit rights), so no documents were searched.",
                MissingEvidence: ["Question did not match a supported topic; rephrase it using breach notification, data retention, or audit rights."]);
        }

        var answerParts = new List<string>();
        var missingEvidence = new List<string>();

        foreach (var topic in topicsAsked)
        {
            var topicMatches = matches
                .Where(match => match.Topic.Id == topic.Id)
                .ToList();

            if (topicMatches.Count == 0)
            {
                missingEvidence.Add($"No {tenantId} document {topic.MissingPhrase}.");
                continue;
            }

            var ids = string.Join(", ", topicMatches.Select(match => match.Document.Id));
            var noun = topicMatches.Count == 1 ? "document" : "documents";
            var verb = topicMatches.Count == 1 ? "mentions" : "mention";

            answerParts.Add($"{topicMatches.Count} {tenantId} {noun} {verb} {topic.Label} ({ids}).");
        }

        var answer = answerParts.Count > 0
            ? string.Join(" ", answerParts)
            : $"No {tenantId} documents matched this question.";

        return new Summary(answer, missingEvidence);
    }
}
