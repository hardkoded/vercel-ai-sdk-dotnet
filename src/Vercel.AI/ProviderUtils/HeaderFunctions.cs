// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Collections;
using System.Net.Http.Headers;

namespace Vercel.AI.ProviderUtils;

/// <summary>Header normalization and merging. Maps to <c>normalizeHeaders</c>, <c>combineHeaders</c>, and <c>withUserAgentSuffix</c>.</summary>
public static class HeaderFunctions
{
    /// <summary>Returns an empty map.</summary>
    public static Dictionary<string, string> NormalizeHeaders(object? headers)
    {
        if (headers is null || headers is JsUndefined)
        {
            return new Dictionary<string, string>();
        }

        if (headers is HttpHeaders httpHeaders)
        {
            return NormalizeHttpHeaders(httpHeaders);
        }

        if (headers is IDictionary dictionary)
        {
            var entries = new List<KeyValuePair<string, string?>>();
            foreach (DictionaryEntry entry in dictionary)
            {
                var key = entry.Key as string ?? string.Empty;
                string? value = null;
                if (entry.Value is string text)
                {
                    value = text;
                }
                else if (entry.Value != null && entry.Value is not JsUndefined)
                {
                    value = entry.Value.ToString();
                }

                entries.Add(new KeyValuePair<string, string?>(key, value));
            }

            return NormalizePairs(entries);
        }

        if (headers is IEnumerable<KeyValuePair<string, string?>> pairs)
        {
            return NormalizePairs(pairs);
        }

        throw new ArgumentException("Headers must be a dictionary, a list of pairs, or HTTP headers.", nameof(headers));
    }

    /// <summary>Merges header maps. Later entries overwrite earlier ones, including null values.</summary>
    public static Dictionary<string, string?> CombineHeaders(params IReadOnlyDictionary<string, string?>?[] headers)
    {
        var combined = new Dictionary<string, string?>();
        if (headers is null)
        {
            return combined;
        }

        foreach (var current in headers)
        {
            if (current is null)
            {
                continue;
            }

            foreach (var pair in current)
            {
                combined[pair.Key] = pair.Value;
            }
        }

        return combined;
    }

    /// <summary>Copies response headers into a case-insensitive map. Maps to <c>extractResponseHeaders</c>.</summary>
    public static Dictionary<string, string> ExtractResponseHeaders(HttpResponseMessage response)
    {
        if (response is null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        var headers = new Dictionary<string, string>(StringComparer.Ordinal);
        Add(headers, response.Headers);
        if (response.Content != null)
        {
            Add(headers, response.Content.Headers);
        }

        return headers;
    }

    /// <summary>Copies <see cref="FetchResponse"/> headers with lowercase keys.</summary>
    public static Dictionary<string, string> ExtractResponseHeaders(FetchResponse response)
    {
        if (response is null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        var headers = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in response.Headers)
        {
            headers[pair.Key.ToLowerInvariant()] = pair.Value;
        }

        return headers;
    }

    /// <summary>Appends suffix parts to the user-agent header. Maps to <c>withUserAgentSuffix</c>.</summary>
    public static Dictionary<string, string> WithUserAgentSuffix(object? headers, params string[] userAgentSuffixParts)
    {
        var normalized = NormalizeHeaders(headers);
        string? current;
        normalized.TryGetValue("user-agent", out current);
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(current))
        {
            parts.Add(current!);
        }

        if (userAgentSuffixParts != null)
        {
            foreach (var part in userAgentSuffixParts)
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

    private static Dictionary<string, string> NormalizeHttpHeaders(HttpHeaders headers)
    {
        var normalized = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var header in headers)
        {
            normalized[header.Key.ToLowerInvariant()] = string.Join(", ", header.Value);
        }

        return normalized;
    }

    private static Dictionary<string, string> NormalizePairs(IEnumerable<KeyValuePair<string, string?>> headers)
    {
        var normalized = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in headers)
        {
            if (pair.Value != null)
            {
                normalized[pair.Key.ToLowerInvariant()] = pair.Value;
            }
        }

        return normalized;
    }

    private static void Add(Dictionary<string, string> target, HttpHeaders headers)
    {
        foreach (var header in headers)
        {
            target[header.Key.ToLowerInvariant()] = string.Join(", ", header.Value);
        }
    }
}

/// <summary>A fetch-style response used by provider response handlers.</summary>
public sealed class FetchResponse
{
    /// <summary>Creates an empty response.</summary>
    public FetchResponse()
    {
        Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>HTTP status code.</summary>
    public int Status { get; set; } = 200;

    /// <summary>HTTP status text.</summary>
    public string StatusText { get; set; } = string.Empty;

    /// <summary>Response headers.</summary>
    public Dictionary<string, string> Headers { get; }

    /// <summary>Response body. Null when the body is absent.</summary>
    public Stream? Body { get; set; }

    /// <summary>Cancels the underlying body. Exceptions propagate to the caller.</summary>
    public Func<Task>? BodyCancel { get; set; }

    /// <summary>True while a reader holds the body.</summary>
    public bool Locked { get; set; }

    /// <summary>True after <see cref="CancelAsync"/> runs.</summary>
    public bool Cancelled { get; private set; }

    /// <summary>Optional cancel callback. Exceptions propagate to the caller of <see cref="CancelAsync"/>.</summary>
    public Action? OnCancel { get; set; }

    /// <summary>Cancels the body.</summary>
    public Task CancelAsync()
    {
        return CancelBodyAsync();
    }

    /// <summary>Runs <see cref="BodyCancel"/> and records that the body was cancelled.</summary>
    public async Task CancelBodyAsync()
    {
        if (BodyCancel != null)
        {
            await BodyCancel().ConfigureAwait(false);
        }

        OnCancel?.Invoke();
        Cancelled = true;
    }
}
