// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gladia;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.GladiaTranscriptionModel;

public sealed class PollingTests
{
    private const string Prefix = "packages/gladia/src/gladia-transcription-model.test.ts::polling::";
    private const string Commit = "3ab5e75d790c3657574cdbff56a8e3f14ba62bd7";
    private const string ResultUrl = "https://api.gladia.io/v2/pre-recorded/7b0137fd-5fd6-4d08-b9bf-4cdf9876797d";
    private const string Transcript = "Galileo was an American robotic space program.";
    private const string DoneBody = "{\"status\":\"done\",\"result\":{\"metadata\":{\"audio_duration\":36.74},\"transcription\":{\"full_transcript\":\"" + Transcript + "\",\"languages\":[\"en\"],\"utterances\":[]}}}";
    private const string ProcessingBody = "{\"status\":\"processing\"}";

    private static AudioInput Audio()
    {
        return new AudioInput(new byte[] { 1, 2, 3, 4 }, "audio/mp3", null);
    }

    private static ParityHandler Handler(Func<int, string> poll, Action? onPoll = null)
    {
        var pollCount = 0;
        return new ParityHandler(call =>
        {
            if (call.Uri.AbsolutePath.EndsWith("/upload", StringComparison.Ordinal))
            {
                return ParityHandler.Json("{\"audio_url\":\"https://api.gladia.io/file/7403f025-be30-4335-ae29-20131e0adbd6\"}");
            }

            if (call.Method == HttpMethod.Post)
            {
                return ParityHandler.Json("{\"id\":\"7b0137fd-5fd6-4d08-b9bf-4cdf9876797d\",\"result_url\":\"" + ResultUrl + "\"}");
            }

            pollCount++;
            onPoll?.Invoke();
            return ParityHandler.Json(poll(pollCount));
        });
    }

    private static int Polls(ParityHandler handler)
    {
        return handler.Calls.Count(call => call.Uri.AbsoluteUri == ResultUrl);
    }

    [Fact]
    [UpstreamTest(Prefix + "polls queued and processing jobs until the job is done", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Polls_queued_and_processing_jobs_until_the_job_is_done()
    {
        var handler = Handler(count => count == 1 ? "{\"status\":\"queued\"}" : count == 2 ? ProcessingBody : DoneBody);
        var provider = GladiaProvider.Create(new GladiaOptions { ApiKey = "test-api-key", PollIntervalMs = 10 }, handler);

        var result = await provider.Transcription().TranscribeAsync(Audio(), null, CancellationToken.None);

        Assert.Equal(Transcript, result.Text);
        Assert.Equal(3, Polls(handler));
    }

    [Fact]
    [UpstreamTest(Prefix + "does not send polling settings to the Gladia API", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Does_not_send_polling_settings_to_the_Gladia_API()
    {
        var handler = Handler(_ => DoneBody);
        var provider = GladiaProvider.Create(new GladiaOptions { ApiKey = "test-api-key" }, handler);

        await provider.Transcription().TranscribeAsync(Audio(), new GladiaTranscriptionRequest { PollIntervalMs = 1, PollTimeoutMs = 1000 }, CancellationToken.None);

        using var body = JsonDocument.Parse(handler.Calls[1].Text);
        Assert.False(body.RootElement.TryGetProperty("pollIntervalMs", out _));
        Assert.False(body.RootElement.TryGetProperty("pollTimeoutMs", out _));
        Assert.False(body.RootElement.TryGetProperty("PollIntervalMs", out _));
        Assert.False(body.RootElement.TryGetProperty("PollTimeoutMs", out _));
    }

    [Fact]
    [UpstreamTest(Prefix + "throws TranscriptionJobPollingTimedOut after the configured pollTimeoutMs", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Throws_TranscriptionJobPollingTimedOut_after_the_configured_pollTimeoutMs()
    {
        // Advance a mocked clock instead of waiting for real time so the test does not depend on machine speed.
        var now = 0;
        var handler = Handler(_ => ProcessingBody, () => now += 15);
        var provider = GladiaProvider.Create(
            new GladiaOptions
            {
                ApiKey = "test-api-key",
                PollTimeoutMs = 20,
                PollIntervalMs = 1,
                Clock = () => DateTimeOffset.UnixEpoch.AddMilliseconds(now),
            },
            handler);

        var error = Assert.IsType<AiSdkError>(await Record.ExceptionAsync(() =>
            provider.Transcription().TranscribeAsync(Audio(), null, CancellationToken.None)));

        Assert.Equal("TranscriptionJobPollingTimedOut", error.ErrorName);
        Assert.Equal("Transcription job polling timed out after 20 ms", error.Message);
        Assert.Equal(2, Polls(handler));
    }

    [Fact]
    [UpstreamTest(Prefix + "resolves polling settings from providerOptions.gladia before provider settings", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Resolves_polling_settings_from_providerOptions_gladia_before_provider_settings()
    {
        var now = 0;
        var handler = Handler(_ => ProcessingBody, () => now += 12);
        var provider = GladiaProvider.Create(
            new GladiaOptions
            {
                ApiKey = "test-api-key",
                PollIntervalMs = 600_000,
                PollTimeoutMs = 600_000,
                Clock = () => DateTimeOffset.UnixEpoch.AddMilliseconds(now),
            },
            handler);

        var error = Assert.IsType<AiSdkError>(await Record.ExceptionAsync(() =>
            provider.Transcription().TranscribeAsync(Audio(), new GladiaTranscriptionRequest { PollIntervalMs = 1, PollTimeoutMs = 20 }, CancellationToken.None)));

        Assert.Equal("TranscriptionJobPollingTimedOut", error.ErrorName);
        Assert.Equal("Transcription job polling timed out after 20 ms", error.Message);
        Assert.Equal(2, Polls(handler));
    }

    [Fact]
    [UpstreamTest(Prefix + "throws InvalidArgumentError for invalid per-call polling settings", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Throws_InvalidArgumentError_for_invalid_per_call_polling_settings()
    {
        var handler = Handler(_ => DoneBody);
        var provider = GladiaProvider.Create(new GladiaOptions { ApiKey = "test-api-key" }, handler);

        await Assert.ThrowsAsync<InvalidArgumentError>(() =>
            provider.Transcription().TranscribeAsync(Audio(), new GladiaTranscriptionRequest { PollTimeoutMs = 0 }, CancellationToken.None));
    }

    [Fact]
    [UpstreamTest(Prefix + "stops before the first poll when the signal is already aborted", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Stops_before_the_first_poll_when_the_signal_is_already_aborted()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        var handler = Handler(_ => DoneBody);
        var provider = GladiaProvider.Create(new GladiaOptions { ApiKey = "test-api-key" }, handler);

        var error = await Record.ExceptionAsync(() =>
            provider.Transcription().TranscribeAsync(Audio(), null, source.Token));

        var cancelled = Assert.IsAssignableFrom<OperationCanceledException>(error);
        Assert.Equal(source.Token, cancelled.CancellationToken);
        Assert.Equal(0, Polls(handler));
    }

    [Fact]
    [UpstreamTest(Prefix + "cancels the wait between polls when the signal is aborted", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Cancels_the_wait_between_polls_when_the_signal_is_aborted()
    {
        using var source = new CancellationTokenSource();
        var handler = Handler(_ => ProcessingBody, () => source.Cancel());
        var provider = GladiaProvider.Create(new GladiaOptions { ApiKey = "test-api-key" }, handler);

        var error = await Record.ExceptionAsync(() =>
            provider.Transcription().TranscribeAsync(Audio(), new GladiaTranscriptionRequest { PollIntervalMs = 600_000 }, source.Token));

        Assert.IsAssignableFrom<OperationCanceledException>(error);
        Assert.Equal(1, Polls(handler));
    }
}
