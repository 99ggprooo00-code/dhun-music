using System.Text.RegularExpressions;

namespace Dhun.Core.Utils;

/// <summary>
///     Privacy redaction applied to crash reports before they are displayed, copied or exported.
/// </summary>
/// <remarks>
///     The redacted output is what the user sees; DHUN never uploads crash data anywhere automatically.
///     The live local log file is intentionally not rewritten — the user owns it. The moment a report
///     leaves the app (clipboard, saved file, issue body), secrets, tokens and personal profile paths
///     must already be replaced.
///     <para>
///     The key list is conservative: it targets config/credential-shaped text (named secrets, bearer
///     tokens, well-known API-key formats, Windows user-profile directories) and never removes stack
///     frames, message text or exception types that maintainers need. Replacement markers are chosen so
///     they can never be re-matched by a later rule (no `key`/`token` words in the marker text).
///     </para>
/// </remarks>
public static partial class CrashReportRedactor
{
    /// <summary>Default cap for a shareable crash report (keep the most recent tail).</summary>
    public const int DefaultMaxReportCharacters = 64 * 1024;

    /// <summary>Marker inserted for redacted secrets.</summary>
    public const string RedactedMarker = "[REDACTED]";

    /// <summary>Marker that replaces the user-name segment of a Windows profile path.</summary>
    public const string UserProfileMarker = "[USER]";

    /// <summary>
    ///     Named-secret patterns found in JSON (`"Key": "value"`), INI/config (`Key = value`),
    ///     URL query strings (`?key=value`) and bare assignments. Only the value is replaced.
    ///     Words that merely contain a key name (monkey, turnkey) must not match, so the assignment
    ///     form is exact-name and word-bounded; the JSON form is exact-name inside quotes.
    /// </summary>
    private static readonly string[] SecretKeyNames =
    [
        "apikey", "api_key", "clientid", "client_id", "clientsecret", "client_secret",
        "subscriptionkey", "subscription_key", "accesstoken", "access_token",
        "refreshtoken", "refresh_token", "sessionkey", "session_key", "lastfmkey",
        "password", "passwd", "secret", "token", "authorization", "cookie", "sk", "key"
    ];

    private static readonly Regex NamedSecretJsonRegex = CreateJsonRegex();
    private static readonly Regex NamedSecretEqualsRegex = CreateAssignmentRegex();

    [GeneratedRegex(@"([A-Za-z]:[\\/]+Users[\\/]+)([^\s""'<>|/\\]+)",
        RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex UserProfilePathRegex();

    [GeneratedRegex(@"\bBearer\s+[A-Za-z0-9\-._~+/]{8,}=*",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex BearerTokenRegex();

    [GeneratedRegex(@"\bAIza[0-9A-Za-z_\-]{20,}",
        RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex GoogleApiKeyRegex();

    [GeneratedRegex(@"\bgh[pousr]_[A-Za-z0-9]{20,}",
        RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex GitHubTokenRegex();

    [GeneratedRegex(@"\bxox[baprs]-[A-Za-z0-9\-]{10,}",
        RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex SlackTokenRegex();

    [GeneratedRegex(@"\bya29\.[A-Za-z0-9_\-]{10,}",
        RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex GoogleOAuthTokenRegex();

    /// <summary>
    ///     Redacts secrets and personal paths, then caps the report to the most recent
    ///     <paramref name="maxCharacters" /> characters (truncating the beginning, because crash dialogs
    ///     and exporters usually surface the tail where the exception lives).
    /// </summary>
    public static string Redact(string? content, int maxCharacters = DefaultMaxReportCharacters)
    {
        if (string.IsNullOrEmpty(content))
            return string.Empty;

        var redacted = RedactSecrets(content);

        if (maxCharacters > 0 && redacted.Length > maxCharacters)
            redacted = "[... earlier log truncated ...]\n" + redacted[^maxCharacters..];

        return redacted;
    }

    /// <summary>Redacts without applying a length cap.</summary>
    public static string RedactSecrets(string content)
    {
        ArgumentNullException.ThrowIfNull(content);
        if (content.Length == 0)
            return content;

        var result = content;

        // Provider token shapes first, so a generic key=value rule can never claim them partially.
        result = GoogleApiKeyRegex().Replace(result, RedactedMarker);
        result = GitHubTokenRegex().Replace(result, RedactedMarker);
        result = SlackTokenRegex().Replace(result, RedactedMarker);
        result = GoogleOAuthTokenRegex().Replace(result, RedactedMarker);
        result = BearerTokenRegex().Replace(result, "Bearer " + RedactedMarker);

        result = NamedSecretJsonRegex.Replace(result, MatchJsonSecretValue);
        result = NamedSecretEqualsRegex.Replace(result, MatchAssignmentSecretValue);
        result = UserProfilePathRegex().Replace(result, MatchUserProfilePath);

        return result;
    }

    /// <summary>
    ///     Replaces the value of `"someKey": "value"` JSON members. Group 1 holds the value text
    ///     without its quotes, so the JSON structure survives redaction.
    /// </summary>
    private static string MatchJsonSecretValue(Match match)
    {
        var value = match.Groups[1];
        return string.Concat(
            match.Value.Substring(0, value.Index - match.Index),
            RedactedMarker,
            match.Value.Substring(value.Index + value.Length - match.Index));
    }

    /// <summary>
    ///     Replaces the value of `key=value` and `key = "value"` assignments. Group 1 is the value,
    ///     optionally including its quotes; quotes are preserved around the marker.
    /// </summary>
    private static string MatchAssignmentSecretValue(Match match)
    {
        var value = match.Groups[1];
        var raw = value.Value;
        var quoted = raw.Length >= 2 && raw[0] == '"' && raw[^1] == '"';
        var marker = quoted ? "\"" + RedactedMarker + "\"" : RedactedMarker;
        return string.Concat(
            match.Value.Substring(0, value.Index - match.Index),
            marker,
            match.Value.Substring(value.Index + value.Length - match.Index));
    }

    /// <summary>
    ///     Replaces only the user-name segment of `X:\Users\<name>` (or `X:/Users/<name>`) with the
    ///     marker and keeps the remainder of the path, so developers can still tell whether a file lived
    ///     under AppData, Music, etc.
    /// </summary>
    private static string MatchUserProfilePath(Match match)
    {
        // Group 1: "X:\Users\" prefix (no personal data). Group 2: the user name (redacted).
        return match.Groups[1].Value + UserProfileMarker;
    }

    /// <summary>
    ///     Matches JSON string members whose quoted key is a secret name; group 1 captures the value
    ///     without quotes.
    /// </summary>
    private static Regex CreateJsonRegex()
    {
        var keys = string.Join("|", SecretKeyNames.Select(Regex.Escape));
        var pattern = $"\"(?:{keys})\"\\s*:\\s*\"([^\"\\\\]*(?:\\\\.[^\"\\\\]*)*)\"";

        return new Regex(pattern,
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));
    }

    /// <summary>
    ///     Matches `key=value` / `key = "value"` assignments and URL query parameters whose key is
    ///     exactly a secret name (word-bounded). Group 1 captures the value, including any quotes.
    /// </summary>
    private static Regex CreateAssignmentRegex()
    {
        var keys = string.Join("|", SecretKeyNames.Select(Regex.Escape));
        var pattern = $"(?<![\\w-])(?:{keys})(?![\\w-])\\s*=\\s*(\"[^\"\\r\\n]*\"|[^\\s\"&\\r\\n]+)";

        return new Regex(pattern,
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));
    }
}
