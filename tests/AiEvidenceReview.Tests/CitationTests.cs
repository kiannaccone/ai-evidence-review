using Xunit;

namespace AiEvidenceReview.Tests;

public class CitationTests : AssistantFixture
{
    [Fact]
    public void EveryReferencedDocumentHasAnIdASnippetAndAMentionInTheAnswer()
    {
        var result = Ask("tenant-a", "What are our breach notification obligations?");

        Assert.NotEmpty(result.Citations);
        Assert.All(result.Citations, citation =>
        {
            Assert.False(string.IsNullOrWhiteSpace(citation.DocumentId));
            Assert.False(string.IsNullOrWhiteSpace(citation.Snippet));
            Assert.Contains(citation.DocumentId, result.Answer);
        });
    }

    [Fact]
    public void SnippetsAreQuotedVerbatimFromTheSourceDocument()
    {
        // tenant-b carries no fake PII, so its snippets survive redaction unchanged.
        var result = Ask("tenant-b", "How long do we retain data?", userId: "analyst-2");

        Assert.All(result.Citations, citation =>
        {
            var source = InMemoryDocumentStore.Seed.Single(d => d.Id == citation.DocumentId);
            Assert.Contains(citation.Snippet, source.Text);
        });
    }

    [Fact]
    public void ReportsMissingEvidenceInsteadOfGuessing()
    {
        var result = Ask("tenant-a", "What is our data retention period?");

        Assert.Empty(result.Citations);
        Assert.Equal(
            "No tenant-a document states a data retention period.",
            Assert.Single(result.MissingEvidence));
        Assert.Equal("This is not legal advice.", result.Warning);
    }

    [Fact]
    public void SearchesNothingWhenTheQuestionMatchesNoSupportedTopic()
    {
        var result = Ask("tenant-a", "Who signed the office lease?");

        Assert.Empty(result.Citations);
        Assert.Contains("could not map this question", result.Answer);
    }
}
