// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Collections;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Vercel.AI.ProviderUtils;

/// <summary>Small collection, header, text, and media helpers shared by providers.</summary>
public static class ProviderValues
{
    /// <summary>Returns an empty list for null, the same list when <paramref name="value"/> is already a list, or a one-item list.</summary>
    public static IReadOnlyList<T> AsArray<T>(object? value)
    {
        if (value is null)
        {
            return Array.Empty<T>();
        }

        if (value is IReadOnlyList<T> list)
        {
            return list;
        }

        if (value is T item)
        {
            return new[] { item };
        }

        throw new ArgumentException("Value is not " + typeof(T).Name + " or a list of that type.", nameof(value));
    }

    /// <summary>Drops null entries and keeps other values, including false, zero, and empty strings.</summary>
    public static IReadOnlyList<object> FilterNullable(params object?[] values)
    {
        var result = new List<object>();
        if (values == null)
        {
            return result;
        }

        foreach (var value in values)
        {
            if (value != null)
            {
                result.Add(value);
            }
        }

        return result;
    }

    /// <summary>Copies entries whose values are not null.</summary>
    public static Dictionary<string, object> RemoveUndefinedEntries(IReadOnlyDictionary<string, object?> record)
    {
        var result = new Dictionary<string, object>(StringComparer.Ordinal);
        if (record == null)
        {
            return result;
        }

        foreach (var pair in record)
        {
            if (pair.Value != null)
            {
                result[pair.Key] = pair.Value;
            }
        }

        return result;
    }

    /// <summary>True for a non-null object that is not a string, primitive, or list.</summary>
    public static bool IsRecord(object? value)
    {
        if (value is null || value is string || value is ValueType || value is IList)
        {
            return false;
        }

        return true;
    }

    /// <summary>True when a value can be written as JSON: null, primitives, arrays, and string-keyed dictionaries.</summary>
    public static bool IsJsonSerializable(object? value)
    {
        if (value is null || value is string || value is bool)
        {
            return true;
        }

        if (value is byte || value is sbyte || value is short || value is ushort || value is int || value is uint
            || value is long || value is ulong || value is decimal)
        {
            return true;
        }

        if (value is double number)
        {
            return !double.IsNaN(number) && !double.IsInfinity(number);
        }

        if (value is float single)
        {
            return !float.IsNaN(single) && !float.IsInfinity(single);
        }

        if (value is Delegate || value is DateTime || value is DateTimeOffset || value is Regex)
        {
            return false;
        }

        if (value is JsonValue || value is JsonArray || value is JsonObject)
        {
            return true;
        }

        if (value is IList list)
        {
            foreach (var item in list)
            {
                if (!IsJsonSerializable(item))
                {
                    return false;
                }
            }

            return true;
        }

        if (value is IDictionary<string, object?> dictionary)
        {
            foreach (var pair in dictionary)
            {
                if (!IsJsonSerializable(pair.Value))
                {
                    return false;
                }
            }

            return true;
        }

        return false;
    }

    /// <summary>Header values that may be omitted.</summary>
    public readonly struct OptionalHeader
    {
        /// <summary>Creates a header value. Null means the header is omitted.</summary>
        public OptionalHeader(string? value)
        {
            Value = value;
            HasValue = true;
        }

        /// <summary>An omitted header.</summary>
        public static OptionalHeader Omitted
        {
            get { return default; }
        }

        /// <summary>True when the caller supplied a value, including null.</summary>
        public bool HasValue { get; }

        /// <summary>Header value.</summary>
        public string? Value { get; }
    }

    /// <summary>Lower-cases header names and drops null values.</summary>
    public static Dictionary<string, string> NormalizeHeaders(IEnumerable<KeyValuePair<string, string?>>? headers)
    {
        var normalized = new Dictionary<string, string>(StringComparer.Ordinal);
        if (headers == null)
        {
            return normalized;
        }

        foreach (var header in headers)
        {
            if (header.Value != null)
            {
                normalized[header.Key.ToLowerInvariant()] = header.Value;
            }
        }

        return normalized;
    }

    /// <summary>Appends <paramref name="suffixParts"/> to the user-agent header, creating it when needed.</summary>
    public static Dictionary<string, string> WithUserAgentSuffix(IEnumerable<KeyValuePair<string, string?>>? headers, params string[] suffixParts)
    {
        var normalized = NormalizeHeaders(headers);
        string? current;
        normalized.TryGetValue("user-agent", out current);
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(current))
        {
            parts.Add(current!);
        }

        if (suffixParts != null)
        {
            foreach (var part in suffixParts)
            {
                if (!string.IsNullOrEmpty(part))
                {
                    parts.Add(part);
                }
            }
        }

        normalized["user-agent"] = string.Join(" ", parts);
        return normalized;
    }

    /// <summary>Removes one trailing slash.</summary>
    public static string? WithoutTrailingSlash(string? url)
    {
        if (url is null)
        {
            return null;
        }

        if (url.Length > 0 && url[url.Length - 1] == '/')
        {
            return url.Substring(0, url.Length - 1);
        }

        return url;
    }

    /// <summary>Returns <paramref name="baseUrl"/>, or throws when it is empty or whitespace.</summary>
    public static string? ValidateBaseUrl(string? baseUrl)
    {
        if (baseUrl != null && baseUrl.Trim().Length == 0)
        {
            throw new ArgumentException("baseURL must be a non-empty string.", "baseURL");
        }

        return baseUrl;
    }

    /// <summary>True when both absolute URLs have the same scheme, host, and port.</summary>
    public static bool IsSameOrigin(string url, string baseUrl)
    {
        Uri left;
        Uri right;
        if (!Uri.TryCreate(url, UriKind.Absolute, out left) || !Uri.TryCreate(baseUrl, UriKind.Absolute, out right))
        {
            return false;
        }

        return string.Equals(Origin(left), Origin(right), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Returns the substring before the first dot. A trailing dot is removed with the empty extension.</summary>
    public static string StripFileExtension(string filename)
    {
        if (filename is null)
        {
            throw new ArgumentNullException(nameof(filename));
        }

        var dot = filename.IndexOf('.');
        return dot < 0 ? filename : filename.Substring(0, dot);
    }

    /// <summary>Maps an audio media type to a file extension. Unknown types use the subtype, or empty text when there is no slash.</summary>
    public static string MediaTypeToExtension(string mediaType)
    {
        if (mediaType is null)
        {
            return string.Empty;
        }

        var slash = mediaType.IndexOf('/');
        var subtype = slash < 0 ? string.Empty : mediaType.Substring(slash + 1).ToLowerInvariant();
        switch (subtype)
        {
            case "mpeg":
                return "mp3";
            case "x-wav":
                return "wav";
            case "opus":
                return "ogg";
            case "mp4":
            case "x-m4a":
                return "m4a";
            default:
                return subtype;
        }
    }

    /// <summary>
    /// Describes the runtime the way provider user-agent strings do.
    /// Window is checked first, then navigator, then Node, then the Edge runtime.
    /// </summary>
    public static string GetRuntimeEnvironmentUserAgent(RuntimeProbe? probe = null)
    {
        if (probe is null)
        {
            return "runtime/unknown";
        }

        if (probe.Window)
        {
            return "runtime/browser";
        }

        if (!string.IsNullOrEmpty(probe.NavigatorUserAgent))
        {
            return "runtime/" + probe.NavigatorUserAgent!.ToLowerInvariant();
        }

        if (!string.IsNullOrEmpty(probe.NodeVersion))
        {
            return "runtime/node.js/" + (probe.ProcessVersion ?? string.Empty);
        }

        if (probe.EdgeRuntime)
        {
            return "runtime/vercel-edge";
        }

        return "runtime/unknown";
    }

    /// <summary>Inputs for <see cref="GetRuntimeEnvironmentUserAgent"/>.</summary>
    public sealed class RuntimeProbe
    {
        /// <summary>True when a browser <c>window</c> is present.</summary>
        public bool Window { get; set; }

        /// <summary>Navigator user agent, when the host exposes one.</summary>
        public string? NavigatorUserAgent { get; set; }

        /// <summary>Node version marker. Any non-empty value selects the Node branch.</summary>
        public string? NodeVersion { get; set; }

        /// <summary><c>process.version</c> text appended after <c>runtime/node.js/</c>.</summary>
        public string? ProcessVersion { get; set; }

        /// <summary>True for the Vercel Edge runtime.</summary>
        public bool EdgeRuntime { get; set; }
    }

    /// <summary>Complete batch request counts.</summary>
    public sealed class BatchRequestCounts
    {
        /// <summary>Creates counts.</summary>
        public BatchRequestCounts(long total, long pending, long completed, long failed)
        {
            Total = total;
            Pending = pending;
            Completed = completed;
            Failed = failed;
        }

        /// <summary>Total requests.</summary>
        public long Total { get; }

        /// <summary>Requests still pending.</summary>
        public long Pending { get; }

        /// <summary>Completed requests.</summary>
        public long Completed { get; }

        /// <summary>Failed requests.</summary>
        public long Failed { get; }
    }

    /// <summary>
    /// Returns counts when every value is a non-negative safe integer and the parts add up to the total.
    /// </summary>
    public static BatchRequestCounts? NormalizeBatchRequestCounts(double? total, double? pending, double? completed, double? failed)
    {
        if (!IsNonNegativeSafeInteger(total)
            || !IsNonNegativeSafeInteger(pending)
            || !IsNonNegativeSafeInteger(completed)
            || !IsNonNegativeSafeInteger(failed))
        {
            return null;
        }

        var totalValue = (long)total!.Value;
        var pendingValue = (long)pending!.Value;
        var completedValue = (long)completed!.Value;
        var failedValue = (long)failed!.Value;
        if (pendingValue + completedValue + failedValue != totalValue)
        {
            return null;
        }

        return new BatchRequestCounts(totalValue, pendingValue, completedValue, failedValue);
    }

    /// <summary>
    /// Inclusive 1-based line slice. Line endings are detected as <c>\r\n</c>, then <c>\n</c>, then <c>\r</c>.
    /// When both bounds are null the text is returned unchanged.
    /// </summary>
    public static string ExtractLines(string text, int? startLine = null, int? endLine = null)
    {
        if (text is null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        if (startLine is null && endLine is null)
        {
            return text;
        }

        var lineEnding = text.IndexOf("\r\n", StringComparison.Ordinal) >= 0
            ? "\r\n"
            : text.IndexOf('\n') >= 0
                ? "\n"
                : text.IndexOf('\r') >= 0
                    ? "\r"
                    : "\n";
        var lines = Split(text, lineEnding);
        var start = Math.Max(1, startLine ?? 1) - 1;
        var end = Math.Min(lines.Count, endLine ?? lines.Count);
        if (end < start)
        {
            return string.Empty;
        }

        return string.Join(lineEnding, Slice(lines, start, end));
    }

    private static bool IsNonNegativeSafeInteger(double? value)
    {
        const double maxSafe = 9007199254740991d;
        return value is double number
            && !double.IsNaN(number)
            && !double.IsInfinity(number)
            && number >= 0
            && number <= maxSafe
            && number == Math.Floor(number);
    }

    private static string Origin(Uri uri)
    {
        var port = uri.IsDefaultPort ? string.Empty : ":" + uri.Port.ToString(System.Globalization.CultureInfo.InvariantCulture);
        return uri.Scheme + "://" + uri.Host + port;
    }

    private static List<string> Split(string text, string separator)
    {
        var lines = new List<string>();
        var start = 0;
        while (start <= text.Length)
        {
            var index = text.IndexOf(separator, start, StringComparison.Ordinal);
            if (index < 0)
            {
                lines.Add(text.Substring(start));
                break;
            }

            lines.Add(text.Substring(start, index - start));
            start = index + separator.Length;
        }

        return lines;
    }

    private static List<string> Slice(List<string> lines, int start, int end)
    {
        var slice = new List<string>();
        for (var index = start; index < end; index++)
        {
            slice.Add(lines[index]);
        }

        return slice;
    }
}

/// <summary>Base64 conversions for inline file bytes.</summary>
public static class ByteEncoding
{
    /// <summary>Encodes bytes as standard base64.</summary>
    public static string ToBase64(byte[] bytes)
    {
        if (bytes is null)
        {
            throw new ArgumentNullException(nameof(bytes));
        }

        return Convert.ToBase64String(bytes);
    }

    /// <summary>Decodes standard or base64url text. An empty string decodes to an empty array.</summary>
    public static byte[] FromBase64(string text)
    {
        if (text is null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        if (text.Length == 0)
        {
            return Array.Empty<byte>();
        }

        var normalized = text.Replace('-', '+').Replace('_', '/');
        var padding = normalized.Length % 4;
        if (padding != 0)
        {
            normalized = normalized + new string('=', 4 - padding);
        }

        return Convert.FromBase64String(normalized);
    }

    /// <summary>Returns base64 text unchanged, or encodes bytes.</summary>
    public static string ConvertToBase64(object value)
    {
        if (value is string text)
        {
            return text;
        }

        if (value is byte[] bytes)
        {
            return ToBase64(bytes);
        }

        throw new ArgumentException("Value must be a string or a byte array.", nameof(value));
    }
}

/// <summary>Runs a tool delegate and reports preliminary and final outputs.</summary>
public static class ToolExecution
{
    /// <summary>One tool output.</summary>
    public sealed class ToolStep<T>
    {
        /// <summary>Creates a step.</summary>
        public ToolStep(string type, T output)
        {
            Type = type;
            Output = output;
        }

        /// <summary><c>preliminary</c> or <c>final</c>.</summary>
        public string Type { get; }

        /// <summary>Output value.</summary>
        public T Output { get; }
    }

    /// <summary>True when <paramref name="execute"/> is present.</summary>
    public static bool IsExecutable(Delegate? execute)
    {
        return execute != null;
    }

    /// <summary>Yields one final step from a single result.</summary>
    public static async IAsyncEnumerable<ToolStep<T>> ExecuteAsync<T>(Func<Task<T>> execute)
    {
        if (execute is null)
        {
            throw new ArgumentNullException(nameof(execute));
        }

        var output = await execute().ConfigureAwait(false);
        yield return new ToolStep<T>("final", output);
    }

    /// <summary>Yields each streamed value as preliminary, then repeats the last value as final.</summary>
    public static async IAsyncEnumerable<ToolStep<T>> ExecuteStreamAsync<T>(IAsyncEnumerable<T> outputs)
    {
        if (outputs is null)
        {
            throw new ArgumentNullException(nameof(outputs));
        }

        var hasLast = false;
        T last = default!;
        await foreach (var output in outputs.ConfigureAwait(false))
        {
            hasLast = true;
            last = output;
            yield return new ToolStep<T>("preliminary", output);
        }

        if (hasLast)
        {
            yield return new ToolStep<T>("final", last);
        }
    }
}
