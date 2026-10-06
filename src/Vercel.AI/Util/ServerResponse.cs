// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;

namespace Vercel.AI.Util;

/// <summary>
/// Byte sink with status, headers, and optional write backpressure.
/// Mirrors the subset of Node <c>ServerResponse</c> used by the text-stream helpers.
/// </summary>
public sealed class ServerResponse
{
    private readonly object _gate = new object();
    private TaskCompletionSource<bool>? _drain;
    private TaskCompletionSource<bool> _ended = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    private bool _hold;

    /// <summary>Creates an empty response.</summary>
    public ServerResponse()
    {
        Headers = new Dictionary<string, object>(StringComparer.Ordinal);
        WrittenChunks = new List<byte[]>();
    }

    /// <summary>Status code passed to <see cref="WriteHead"/>.</summary>
    public int StatusCode { get; private set; }

    /// <summary>Status text passed to <see cref="WriteHead"/>.</summary>
    public string StatusMessage { get; private set; } = string.Empty;

    /// <summary>Lowercased headers. Repeated names become a string array.</summary>
    public Dictionary<string, object> Headers { get; }

    /// <summary>Chunks accepted by <see cref="Write"/>.</summary>
    public List<byte[]> WrittenChunks { get; }

    /// <summary>How many times <see cref="Write"/> was called.</summary>
    public int WriteCallCount { get; private set; }

    /// <summary>Whether <see cref="End"/> has run.</summary>
    public bool Ended { get; private set; }

    /// <summary>How many times <see cref="SimulateDrain"/> ran.</summary>
    public int DrainCount { get; private set; }

    /// <summary>When true, the first write succeeds and later writes return false until <see cref="SimulateDrain"/>.</summary>
    public bool EnableBackpressure { get; set; }

    /// <summary>Called after each successful <see cref="Write"/> with this response.</summary>
    public Action<ServerResponse>? Flush { get; set; }

    /// <summary>Stores headers, lowercasing names and grouping duplicates.</summary>
    public void SetHeaders(HeaderCollection? headers)
    {
        Headers.Clear();
        if (headers == null)
        {
            return;
        }

        var entries = headers.Entries();
        for (var i = 0; i < entries.Count; i++)
        {
            var key = entries[i].Key.ToLowerInvariant();
            object? existing;
            if (!Headers.TryGetValue(key, out existing) || existing == null)
            {
                Headers[key] = entries[i].Value;
                continue;
            }

            var list = existing as List<string>;
            if (list == null)
            {
                list = new List<string> { (string)existing };
                Headers[key] = list;
            }

            list.Add(entries[i].Value);
        }
    }

    /// <summary>Sets the status code and, when provided, the status text.</summary>
    public void WriteHead(int statusCode, string? statusText = null)
    {
        StatusCode = statusCode;
        if (statusText != null)
        {
            StatusMessage = statusText;
        }
    }

    /// <summary>Appends <paramref name="chunk"/>. Returns false when the caller should wait for drain.</summary>
    public bool Write(byte[] chunk)
    {
        WrittenChunks.Add(chunk);
        WriteCallCount++;
        if (!EnableBackpressure)
        {
            return true;
        }

        lock (_gate)
        {
            if (WriteCallCount == 1)
            {
                _hold = true;
                return true;
            }

            if (_hold)
            {
                return false;
            }

            _hold = true;
            return true;
        }
    }

    /// <summary>Releases a write that returned false.</summary>
    public void SimulateDrain()
    {
        TaskCompletionSource<bool>? drain;
        lock (_gate)
        {
            _hold = false;
            DrainCount++;
            drain = _drain;
            _drain = null;
        }

        if (drain != null)
        {
            drain.TrySetResult(true);
        }
    }

    /// <summary>Completes when <see cref="SimulateDrain"/> runs, or at once when it already ran after the last held write.</summary>
    public Task WaitForDrainAsync()
    {
        lock (_gate)
        {
            if (!_hold)
            {
                return Task.CompletedTask;
            }

            if (_drain == null)
            {
                _drain = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            }

            return _drain.Task;
        }
    }

    /// <summary>Marks the response finished.</summary>
    public void End()
    {
        Ended = true;
        _ended.TrySetResult(true);
    }

    /// <summary>Completes when <see cref="End"/> runs.</summary>
    public Task WaitForEndAsync()
    {
        return _ended.Task;
    }

    /// <summary>Decodes each written chunk as UTF-8.</summary>
    public List<string> GetDecodedChunks()
    {
        var decoded = new List<string>(WrittenChunks.Count);
        for (var i = 0; i < WrittenChunks.Count; i++)
        {
            decoded.Add(Encoding.UTF8.GetString(WrittenChunks[i]));
        }

        return decoded;
    }
}

/// <summary>Writes a byte stream to a <see cref="ServerResponse"/>.</summary>
public static class ServerResponseWriter
{
    /// <summary>
    /// Sets status and headers, then writes <paramref name="stream"/>.
    /// A false write waits for <see cref="ServerResponse.SimulateDrain"/>.
    /// The response is ended even when reading the stream fails.
    /// </summary>
    public static async Task WriteAsync(ServerResponse? response, ReadableStream<byte[]> stream, int? status = null, string? statusText = null, HeaderCollection? headers = null)
    {
        if (response == null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        if (headers != null)
        {
            response.SetHeaders(headers);
        }

        response.WriteHead(status ?? 200, statusText);
        var reader = stream.GetReader();
        try
        {
            while (true)
            {
                var read = await reader.ReadAsync().ConfigureAwait(false);
                if (read.Rejected)
                {
                    var error = read.Reason as Exception;
                    if (error != null)
                    {
                        throw error;
                    }

                    throw new UndefinedStreamError();
                }

                if (read.Done)
                {
                    break;
                }

                var canContinue = response.Write(read.Value);
                var flush = response.Flush;
                if (flush != null)
                {
                    flush(response);
                }

                if (!canContinue)
                {
                    await response.WaitForDrainAsync().ConfigureAwait(false);
                }
            }
        }
        finally
        {
            reader.ReleaseLock();
            response.End();
        }
    }
}
