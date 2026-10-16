// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Streaming;

/// <summary>Checks that a model streams: parts arrive while the response is open, and a consumer that stops closes it.</summary>
internal static class LiveStreamAssertions
{
    private static readonly TimeSpan Wait = TimeSpan.FromSeconds(10);

    /// <summary>Opens the stream, takes parts until one matches, and checks the response body is still open.</summary>
    public static async Task<LanguageModelStreamPart> NextAsync(IAsyncEnumerator<LanguageModelStreamPart> parts, Func<LanguageModelStreamPart, bool> match)
    {
        while (await parts.MoveNextAsync().AsTask().WaitAsync(Wait))
        {
            if (match(parts.Current))
            {
                return parts.Current;
            }
        }

        throw new Xunit.Sdk.XunitException("The stream ended before the expected part arrived.");
    }

    public static async Task<List<LanguageModelStreamPart>> DrainAsync(IAsyncEnumerator<LanguageModelStreamPart> parts)
    {
        var all = new List<LanguageModelStreamPart>();
        while (await parts.MoveNextAsync().AsTask().WaitAsync(Wait))
        {
            all.Add(parts.Current);
        }

        return all;
    }

    /// <summary>The first part arrives while the provider has sent only the first chunks. The rest follow.</summary>
    public static async Task FirstPartArrivesBeforeTheStreamEndsAsync(
        GatedStream body,
        Func<Task<LanguageModelStreamResult>> open,
        Action first,
        Action<string> check,
        Action rest)
    {
        first();
        var result = await open().WaitAsync(Wait);
        await using var parts = result.GetAsyncEnumerator();
        var part = await NextAsync(parts, candidate => candidate is TextDeltaStreamPart);
        check(((TextDeltaStreamPart)part).Delta);
        Assert.False(body.Disposed);
        rest();
        var remaining = await DrainAsync(parts);
        Assert.IsType<FinishStreamPart>(remaining[^1]);
    }

    /// <summary>A consumer that stops after the first part closes the response and leaves later chunks unread.</summary>
    public static async Task StoppingTheConsumerStopsTheReadAsync(
        GatedStream body,
        Func<Task<LanguageModelStreamResult>> open,
        Action first,
        Action later)
    {
        first();
        var result = await open().WaitAsync(Wait);
        var parts = result.GetAsyncEnumerator();
        await NextAsync(parts, candidate => candidate is TextDeltaStreamPart);
        Assert.False(body.Disposed);
        await parts.DisposeAsync();
        Assert.True(body.Disposed);
        later();
        Assert.True(body.UnreadChunks > 0);
    }

    /// <summary>Cancelling the token while the stream waits for data ends the read with a cancellation.</summary>
    public static async Task CancellingTheTokenStopsTheReadAsync(
        GatedStream body,
        Func<CancellationToken, Task<LanguageModelStreamResult>> open,
        Action first)
    {
        using var cancellation = new CancellationTokenSource();
        first();
        var result = await open(cancellation.Token).WaitAsync(Wait);
        await using var parts = result.GetAsyncEnumerator(cancellation.Token);
        await NextAsync(parts, candidate => candidate is TextDeltaStreamPart);
        var pending = parts.MoveNextAsync().AsTask();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending.WaitAsync(Wait));
        Assert.True(body.Disposed);
    }
}
