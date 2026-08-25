using System.Text.RegularExpressions;

namespace AiEvidenceReview;

/// <summary>
/// Very small redaction pass applied to every snippet before it leaves the
/// system and to every question before it is written to the audit log.
///
/// Regex redaction is a demo-grade control, not a real DLP layer.
/// See the limitations section of the README.
/// </summary>
public static class Redactor
{
    // Order matters: an SSN (3-2-4) is matched before the generic digit rules.
    private static readonly (Regex Pattern, string Replacement)[] Rules =
    [
        (new Regex(@"\b\d{3}-\d{2}-\d{4}\b", RegexOptions.Compiled), "[REDACTED_SSN]"),
        (new Regex(@"\b[\w.+-]+@[\w-]+\.[\w.-]{2,}\b", RegexOptions.Compiled), "[REDACTED_EMAIL]"),
        (new Regex(@"\b\d{4}-\d{4}-\d{4}(?:-\d{4})?\b", RegexOptions.Compiled), "[REDACTED_ACCOUNT]"),
        (new Regex(@"\b\d{10,19}\b", RegexOptions.Compiled), "[REDACTED_ACCOUNT]"),
        (new Regex(@"\b[A-Z]{2}\d{6}\b", RegexOptions.Compiled), "[REDACTED_PASSPORT]"),
    ];

    public static string Redact(string? text)
    {
        var result = text ?? string.Empty;

        foreach (var (pattern, replacement) in Rules)
        {
            result = pattern.Replace(result, replacement);
        }

        return result;
    }
}
