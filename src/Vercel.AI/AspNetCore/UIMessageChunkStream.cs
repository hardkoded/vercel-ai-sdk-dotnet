// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;

namespace Vercel.AI.AspNetCore;

/// <summary>SSE response for UI message chunks that are already built.</summary>
public sealed class UIMessageChunkStreamResult : IResult
{
    private readonly IReadOnlyList<JsonObject> _chunks;
    private readonly int _statusCode;
    private readonly IReadOnlyList<KeyValuePair<string, string>>? _headers;

    /// <summary>Creates a response that frames <paramref name="chunks"/> and then writes <c>data: [DONE]</c>.</summary>
    /// <param name="chunks">Chunk objects, in order.</param>
    /// <param name="statusCode">HTTP status code. Defaults to 200.</param>
    /// <param name="headers">Extra headers. Repeated names, including <c>Set-Cookie</c>, are all written.</param>
    public UIMessageChunkStreamResult(
        IReadOnlyList<JsonObject> chunks,
        int statusCode = 200,
        IReadOnlyList<KeyValuePair<string, string>>? headers = null)
    {
        _chunks = chunks ?? throw new ArgumentNullException(nameof(chunks));
        if (statusCode < 100 || statusCode > 599)
        {
            throw new ArgumentOutOfRangeException(nameof(statusCode));
        }

        _statusCode = statusCode;
        _headers = headers;
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        if (httpContext is null)
        {
            throw new ArgumentNullException(nameof(httpContext));
        }

        httpContext.Response.StatusCode = _statusCode;
        UIMessageStreamResult.ApplyProtocolHeaders(httpContext.Response);
        if (_headers != null)
        {
            foreach (var header in _headers)
            {
                httpContext.Response.Headers.Append(header.Key, header.Value);
            }
        }

        await UIMessageStreamResult.WriteChunksAsync(_chunks, httpContext.Response.Body, httpContext.RequestAborted).ConfigureAwait(false);
    }
}
