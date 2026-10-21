// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Google;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests;

/// <summary><c>pollGoogleInteractionUntilTerminal</c>.</summary>
public sealed class GoogleInteractionsPollingUpstreamTests
{
    private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta";
    private const string InteractionId = "v1_test-poll-id";
    private const string GetUrl = BaseUrl + "/interactions/" + InteractionId;
    private const string CancelUrl = GetUrl + "/cancel";

    private static readonly IReadOnlyDictionary<string, string?> NoHeaders = new Dictionary<string, string?>();

    private static ProviderHttp Http(SequencedHandler handler)
    {
        return new ProviderHttp(new HttpClient(handler));
    }

    [Fact]
    [UpstreamTest("packages/google/src/interactions/poll-google-interactions.test.ts::pollGoogleInteractionUntilTerminal::polls until terminal status and returns the final response", Coverage = UpstreamCoverage.Covered)]
    public async Task Polls_until_terminal_status_and_returns_the_final_response()
    {
        var handler = new SequencedHandler()
            .Json("{\"id\":\"" + InteractionId + "\",\"status\":\"in_progress\"}")
            .Json("{\"id\":\"" + InteractionId + "\",\"status\":\"in_progress\"}")
            .Json("{\"id\":\"" + InteractionId + "\",\"status\":\"completed\",\"outputs\":[{\"type\":\"text\",\"text\":\"final answer\"}],\"usage\":{\"total_input_tokens\":5,\"total_output_tokens\":7}}");

        using var result = await GoogleInteractionsPolling.PollUntilTerminalAsync(
            Http(handler),
            BaseUrl,
            InteractionId,
            NoHeaders,
            TimeSpan.FromMilliseconds(1),
            TimeSpan.FromMilliseconds(1),
            TimeSpan.FromMilliseconds(5000),
            CancellationToken.None);

        Assert.Equal("completed", result.RootElement.GetProperty("status").GetString());
        var outputs = result.RootElement.GetProperty("outputs");
        Assert.Equal(1, outputs.GetArrayLength());
        Assert.Equal("text", outputs[0].GetProperty("type").GetString());
        Assert.Equal("final answer", outputs[0].GetProperty("text").GetString());
        Assert.Equal(3, handler.Methods.Count);
        Assert.All(handler.Methods, method => Assert.Equal("GET", method));
    }

    [Fact]
    [UpstreamTest("packages/google/src/interactions/poll-google-interactions.test.ts::pollGoogleInteractionUntilTerminal::returns immediately on the first call if status is already terminal", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_immediately_on_the_first_call_if_status_is_already_terminal()
    {
        var handler = new SequencedHandler().Json("{\"id\":\"" + InteractionId + "\",\"status\":\"completed\",\"outputs\":[]}");

        using var result = await GoogleInteractionsPolling.PollUntilTerminalAsync(
            Http(handler),
            BaseUrl,
            InteractionId,
            NoHeaders,
            TimeSpan.FromMilliseconds(1),
            TimeSpan.FromMilliseconds(1),
            TimeSpan.FromMilliseconds(5000),
            CancellationToken.None);

        Assert.Equal("completed", result.RootElement.GetProperty("status").GetString());
        Assert.Single(handler.Methods);
    }

    [Fact]
    [UpstreamTest("packages/google/src/interactions/poll-google-interactions.test.ts::pollGoogleInteractionUntilTerminal::aborts polling when abortSignal fires", Coverage = UpstreamCoverage.Covered)]
    public async Task Aborts_polling_when_abortSignal_fires()
    {
        var handler = new SequencedHandler().Json("{\"id\":\"" + InteractionId + "\",\"status\":\"in_progress\"}");
        using var abort = new CancellationTokenSource();

        var poll = GoogleInteractionsPolling.PollUntilTerminalAsync(
            Http(handler),
            BaseUrl,
            InteractionId,
            NoHeaders,
            TimeSpan.FromMilliseconds(50),
            TimeSpan.FromMilliseconds(50),
            TimeSpan.FromMilliseconds(5000),
            abort.Token);
        abort.CancelAfter(10);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => poll);

        var cancelIndexes = Enumerable.Range(0, handler.Uris.Count).Where(i => handler.Uris[i] == CancelUrl).ToList();
        var cancelIndex = Assert.Single(cancelIndexes);
        Assert.Equal("POST", handler.Methods[cancelIndex]);
    }

    [Fact]
    [UpstreamTest("packages/google/src/interactions/poll-google-interactions.test.ts::pollGoogleInteractionUntilTerminal::throws when no interaction id is provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Throws_when_no_interaction_id_is_provided()
    {
        var handler = new SequencedHandler().Json("{}");

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => GoogleInteractionsPolling.PollUntilTerminalAsync(
            Http(handler),
            BaseUrl,
            null,
            NoHeaders,
            null,
            null,
            null,
            CancellationToken.None));

        Assert.Matches("(?i)cannot poll", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/google/src/interactions/poll-google-interactions.test.ts::pollGoogleInteractionUntilTerminal::throws when timeout is exceeded", Coverage = UpstreamCoverage.Covered)]
    public async Task Throws_when_timeout_is_exceeded()
    {
        var handler = new SequencedHandler().Json("{\"id\":\"" + InteractionId + "\",\"status\":\"in_progress\"}");

        var error = await Assert.ThrowsAsync<TimeoutException>(() => GoogleInteractionsPolling.PollUntilTerminalAsync(
            Http(handler),
            BaseUrl,
            InteractionId,
            NoHeaders,
            TimeSpan.FromMilliseconds(5),
            TimeSpan.FromMilliseconds(5),
            TimeSpan.FromMilliseconds(50),
            CancellationToken.None));

        Assert.Matches("(?i)timed out", error.Message);
    }
}
