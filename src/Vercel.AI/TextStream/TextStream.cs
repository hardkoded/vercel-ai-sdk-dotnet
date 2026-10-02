// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text;
using Vercel.AI.Provider;

namespace Vercel.AI;

/// <summary>A response that receives a UTF-8 text stream. Maps to Node <c>ServerResponse</c> for <c>pipeTextStreamToResponse</c>.</summary>
public interface ITextStreamServerResponse
{
    /// <summary>HTTP status code.</summary>
    int StatusCode { get; set; }

    /// <summary>HTTP reason phrase. Null when the caller did not set one.</summary>
    string? StatusMessage { get; set; }

    /// <summary>Replaces headers with <paramref name="headers"/>. Names are compared case-insensitively. Repeated names are kept, which is how <c>Set-Cookie</c> stays a list.</summary>
    void SetHeaders(IReadOnlyList<KeyValuePair<string, string>> headers);

    /// <summary>Writes one UTF-8 chunk. Return false when the caller should wait for <see cref="WaitForDrainAsync"/>.</summary>
    bool Write(ReadOnlyMemory<byte> chunk);

    /// <summary>Completes after a <see cref="Write"/> call returned false.</summary>
    Task WaitForDrainAsync(CancellationToken cancellationToken);

    /// <summary>Finishes the response.</summary>
    void End();
}

/// <summary>A text/plain response built from a stream of strings. Maps to <c>createTextStreamResponse</c>.</summary>
public sealed class TextStreamResponse
{
    /// <summary>Creates a response.</summary>
    public TextStreamResponse(int status, string? statusText, IReadOnlyList<KeyValuePair<string, string>> headers, IAsyncEnumerable<byte[]> body)
    {
        Status = status;
        StatusText = statusText;
        Headers = headers ?? throw new ArgumentNullException(nameof(headers));
        Body = body ?? throw new ArgumentNullException(nameof(body));
    }

    /// <summary>HTTP status code.</summary>
    public int Status { get; }

    /// <summary>HTTP reason phrase.</summary>
    public string? StatusText { get; }

    /// <summary>Response headers. Names are lowercase. <c>set-cookie</c> may repeat.</summary>
    public IReadOnlyList<KeyValuePair<string, string>> Headers { get; }

    /// <summary>UTF-8 chunks, one per input string.</summary>
    public IAsyncEnumerable<byte[]> Body { get; }

    /// <summary>Header values for <paramref name="name"/>.</summary>
    public IReadOnlyList<string> GetHeaderValues(string name)
    {
        var values = new List<string>();
        for (var i = 0; i < Headers.Count; i++)
        {
            if (string.Equals(Headers[i].Key, name, StringComparison.OrdinalIgnoreCase))
            {
                values.Add(Headers[i].Value);
            }
        }

        return values;
    }
}

/// <summary>Text-stream helpers. Maps to <c>toTextStream</c>, <c>createTextStreamResponse</c>, and <c>pipeTextStreamToResponse</c>.</summary>
public static class TextStreams
{
    /// <summary>Yields the text of each <see cref="TextDeltaStreamPart"/> and drops every other part.</summary>
    public static async IAsyncEnumerable<string> ToTextStream(
        IAsyncEnumerable<LanguageModelStreamPart> stream,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        await foreach (var part in stream.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (part is TextDeltaStreamPart text)
            {
                yield return text.Delta;
            }
        }
    }

    /// <summary>Builds a <c>text/plain; charset=utf-8</c> response. Each string is one UTF-8 chunk.</summary>
    public static TextStreamResponse CreateTextStreamResponse(
        IAsyncEnumerable<string> stream,
        int status = 200,
        string? statusText = null,
        IEnumerable<KeyValuePair<string, string>>? headers = null)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        var prepared = PrepareHeaders(headers, "text/plain; charset=utf-8");
        return new TextStreamResponse(status, statusText, prepared, Encode(stream));
    }

    /// <summary>Writes a text stream to <paramref name="response"/> and completes when the stream ends or faults.</summary>
    public static Task PipeTextStreamToResponseAsync(
        ITextStreamServerResponse response,
        IAsyncEnumerable<string> stream,
        int? status = null,
        string? statusText = null,
        IEnumerable<KeyValuePair<string, string>>? headers = null,
        CancellationToken cancellationToken = default)
    {
        if (response is null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        return WriteToServerResponseAsync(response, status ?? 200, statusText, PrepareHeaders(headers, "text/plain; charset=utf-8"), Encode(stream), cancellationToken);
    }

    /// <summary>Adds <paramref name="contentType"/> when the caller did not set <c>content-type</c>.</summary>
    public static List<KeyValuePair<string, string>> PrepareHeaders(IEnumerable<KeyValuePair<string, string>>? headers, string contentType)
    {
        var prepared = new List<KeyValuePair<string, string>>();
        var hasContentType = false;
        if (headers != null)
        {
            foreach (var header in headers)
            {
                var name = header.Key.ToLowerInvariant();
                if (name == "content-type")
                {
                    hasContentType = true;
                }

                prepared.Add(new KeyValuePair<string, string>(name, header.Value));
            }
        }

        if (!hasContentType)
        {
            prepared.Add(new KeyValuePair<string, string>("content-type", contentType));
        }

        return prepared;
    }

    private static async IAsyncEnumerable<byte[]> Encode(
        IAsyncEnumerable<string> stream,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var chunk in stream.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            yield return Encoding.UTF8.GetBytes(chunk ?? string.Empty);
        }
    }

    private static async Task WriteToServerResponseAsync(
        ITextStreamServerResponse response,
        int status,
        string? statusText,
        IReadOnlyList<KeyValuePair<string, string>> headers,
        IAsyncEnumerable<byte[]> stream,
        CancellationToken cancellationToken)
    {
        response.SetHeaders(headers);
        response.StatusCode = status;
        if (statusText != null)
        {
            response.StatusMessage = statusText;
        }

        try
        {
            await foreach (var chunk in stream.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                if (!response.Write(chunk))
                {
                    await response.WaitForDrainAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }
        finally
        {
            response.End();
        }
    }
}
