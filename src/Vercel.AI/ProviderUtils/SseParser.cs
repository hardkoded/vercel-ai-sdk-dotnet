// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.ProviderUtils;

/// <summary>Reads Server-Sent Events <c>data:</c> lines.</summary>
public static class SseParser
{
    /// <summary>
    /// Yields event data. A <c>data: [DONE]</c> line ends the stream.
    /// Multi-line data fields are joined with newlines.
    /// </summary>
    public static async IAsyncEnumerable<string> ReadDataAsync(
        Stream stream,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        using var reader = new StreamReader(stream, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);
        var data = new System.Text.StringBuilder();
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (line is null)
            {
                if (data.Length > 0)
                {
                    yield return data.ToString();
                }

                yield break;
            }

            if (line.Length == 0)
            {
                if (data.Length > 0)
                {
                    var payload = data.ToString();
                    data.Clear();
                    if (payload == "[DONE]")
                    {
                        yield break;
                    }

                    yield return payload;
                }

                continue;
            }

            if (StartsWithOrdinal(line, "data:"))
            {
                var value = line.Substring(5).TrimStart();
                if (data.Length > 0)
                {
                    data.Append('\n');
                }

                data.Append(value);
            }
        }
    }

    private static bool StartsWithOrdinal(string value, string prefix)
    {
        return value.Length >= prefix.Length
            && string.CompareOrdinal(value, 0, prefix, 0, prefix.Length) == 0;
    }
}
