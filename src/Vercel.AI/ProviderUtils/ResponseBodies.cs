// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http;
using Vercel.AI.Util;

namespace Vercel.AI.ProviderUtils;

/// <summary>Reads and releases HTTP response bodies.</summary>
public static class ResponseBodies
{
    /// <summary>
    /// Releases the response body so the connection can be reused. Errors are swallowed so they do not hide the original failure.
    /// Maps to <c>cancelResponseBody</c>.
    /// </summary>
    public static void CancelResponseBody(HttpResponseMessage response)
    {
        try
        {
            response.Content?.Dispose();
        }
        catch (Exception)
        {
            // The caller is already failing; keep its error.
        }
    }

    /// <summary>
    /// Reads the body, rejecting a declared Content-Length or a streamed size above <paramref name="maxBytes"/>.
    /// The default limit is <see cref="Download.DefaultMaxBytes"/>. With <paramref name="onChunk"/> each chunk goes to the callback
    /// (the buffer and the number of bytes in it) instead of being collected, so the body is not held in memory, and the returned array is empty.
    /// The buffer is reused, copy what you keep. Maps to <c>readResponseWithSizeLimit</c>.
    /// </summary>
    /// <exception cref="DownloadError">The body is larger than the limit.</exception>
    public static async Task<byte[]> ReadResponseWithSizeLimitAsync(
        HttpResponseMessage response,
        string url,
        long? maxBytes = null,
        CancellationToken cancellationToken = default,
        Action<byte[], int>? onChunk = null
    )
    {
        if (response is null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        var limit = maxBytes ?? Download.DefaultMaxBytes;
        if (response.Content is null)
        {
            return Array.Empty<byte>();
        }

        var contentLength = response.Content.Headers.ContentLength;
        if (contentLength > limit)
        {
            CancelResponseBody(response);
            throw new DownloadError(
                url,
                "Download of "
                    + url
                    + " exceeded maximum size of "
                    + limit
                    + " bytes (Content-Length: "
                    + contentLength
                    + ")."
            );
        }

        using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
        using (var body = new MemoryStream())
        {
            long total = 0;
            var chunk = new byte[81920];
            int read;
            while (
                (
                    read = await stream
                        .ReadAsync(chunk, 0, chunk.Length, cancellationToken)
                        .ConfigureAwait(false)
                ) > 0
            )
            {
                total += read;
                if (total > limit)
                {
                    CancelResponseBody(response);
                    throw new DownloadError(
                        url,
                        "Download of " + url + " exceeded maximum size of " + limit + " bytes."
                    );
                }

                if (onChunk != null)
                {
                    try
                    {
                        onChunk(chunk, read);
                    }
                    catch
                    {
                        CancelResponseBody(response);
                        throw;
                    }

                    continue;
                }

                body.Write(chunk, 0, read);
            }

            return body.ToArray();
        }
    }
}
