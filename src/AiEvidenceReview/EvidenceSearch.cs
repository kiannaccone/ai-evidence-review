using System.Text.RegularExpressions;

namespace AiEvidenceReview;

public sealed class EvidenceSearch
{
    private static readonly Regex SentenceBoundary = new(@"(?<=[.!?])\s+", RegexOptions.Compiled);

    private readonly IDocumentStore _documents;

    public EvidenceSearch(IDocumentStore documents)
    {
        _documents = documents;
    }

    /// <summary>
    /// Returns at most one match per (topic, document) pair, so the citation list
    /// stays short and every citation points at a specific sentence.
    /// </summary>
    public IReadOnlyList<EvidenceMatch> Find(string tenantId, IReadOnlyList<Topic> topics)
    {
        // Everything downstream sees only this tenant's documents.
        var scope = _documents.ForTenant(tenantId);
        var matches = new List<EvidenceMatch>();

        foreach (var topic in topics)
        {
            foreach (var document in scope)
            {
                var snippet = FindSnippet(document, topic);

                if (snippet is not null)
                {
                    matches.Add(new EvidenceMatch(topic, document, Redactor.Redact(snippet)));
                }
            }
        }

        return matches;
    }

    /// <summary>First sentence in the document containing one of the topic's keywords.</summary>
    private static string? FindSnippet(ComplianceDocument document, Topic topic) =>
        SentenceBoundary
            .Split(document.Text)
            .Select(sentence => sentence.Trim())
            .Where(sentence => sentence.Length > 0)
            .FirstOrDefault(sentence =>
            {
                var lower = sentence.ToLowerInvariant();
                return topic.DocumentKeywords.Any(lower.Contains);
            });
}
