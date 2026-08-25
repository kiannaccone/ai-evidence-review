using System.Text.Json;
using AiEvidenceReview;

// Tiny hand-rolled argument parser. Everything that is not a flag becomes the question.
var tenantId = "tenant-a";
var userId = "analyst-1";
var showAudit = false;
var questionWords = new List<string>();

for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--tenant" when i + 1 < args.Length:
            tenantId = args[++i];
            break;
        case "--user" when i + 1 < args.Length:
            userId = args[++i];
            break;
        case "--audit":
            showAudit = true;
            break;
        default:
            questionWords.Add(args[i]);
            break;
    }
}

var question = questionWords.Count > 0
    ? string.Join(" ", questionWords)
    : "Which policies or contracts mention breach notification, data retention, or audit rights?";

var auditLog = new InMemoryAuditLog();
var assistant = new EvidenceReviewAssistant(new InMemoryDocumentStore(), auditLog);

var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true,
    // Default encoder escapes apostrophes as \u0027 for HTML safety. This output
    // goes to a console, not a web page, so keep snippets readable.
    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
};

var result = assistant.AnswerQuestion(new AskRequest(tenantId, userId, question));

Console.WriteLine(JsonSerializer.Serialize(result, jsonOptions));

if (showAudit)
{
    Console.WriteLine();
    Console.WriteLine("--- audit log ---");
    Console.WriteLine(JsonSerializer.Serialize(auditLog.Events, jsonOptions));
}
