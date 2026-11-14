// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Deepgram;
using Vercel.AI.Operations;
using Vercel.AI.Tests.Upstream.ProviderUtils;

namespace Vercel.AI.Tests.Upstream.Deepgram.DeepgramStreamingTranscription;

/// <summary>Port of <c>deepgram-streaming-transcription.test.ts</c> &gt; <c>Deepgram streaming transcription</c>.</summary>
[Collection("MockWebSocket")]
public sealed class DeepgramStreamingTranscriptionTests
{
    private const string Prefix = "packages/deepgram/src/deepgram-streaming-transcription.test.ts::Deepgram streaming transcription::";
    private static readonly AudioFormat Pcm16 = new AudioFormat("audio/pcm", 16000);

    [Fact]
    [UpstreamTest(Prefix + "uses the live endpoint, auth subprotocols, headers, and streaming options", Coverage = UpstreamCoverage.Covered, Commit = "5b89b2560ccb9eef8f0fc375367158bdfbbb5724")]
    public async Task Uses_the_live_endpoint_auth_subprotocols_headers_and_streaming_options()
    {
        MockWebSocket.Reset();
        var options = new DeepgramOptions { ApiKeyProvider = _ => Task.FromResult("resolved-key"), WebSocket = MockWebSocket.Factory };
        options.Headers["X-Provider"] = "provider";
        var result = await Stream(DeepgramProvider.Create(options).TranscriptionModel("nova-3"), Audio(), Pcm16, headers: new Dictionary<string, string> { ["X-Request"] = "request" }, providerOptions: @"{""deepgram"":{""language"":""fr"",""redact"":[""numbers"",""names""],""streaming"":{""interimResults"":true,""endpointing"":false,""channels"":2,""multichannel"":true,""vadEvents"":true,""utteranceEndMs"":1000}}}");

        var socket = MockWebSocket.Instances[0];
        Assert.Equal("wss://api.deepgram.com/v1/listen", socket.Url.GetLeftPart(UriPartial.Path));
        var query = Query(socket.Url);
        Assert.Equal("nova-3", query["model"][0]);
        Assert.Equal("linear16", query["encoding"][0]);
        Assert.Equal("16000", query["sample_rate"][0]);
        Assert.Equal("fr", query["language"][0]);
        Assert.Equal("true", query["interim_results"][0]);
        Assert.Equal("false", query["endpointing"][0]);
        Assert.Equal("2", query["channels"][0]);
        Assert.Equal("true", query["multichannel"][0]);
        Assert.Equal("true", query["vad_events"][0]);
        Assert.Equal("1000", query["utterance_end_ms"][0]);
        Assert.Equal(new[] { "numbers", "names" }, query["redact"]);
        Assert.Equal(new[] { "token", "resolved-key" }, socket.Protocols);
        Assert.Equal("Token resolved-key", socket.Headers!["authorization"]);
        Assert.Equal("provider", socket.Headers["x-provider"]);
        Assert.Equal("request", socket.Headers["x-request"]);
        Assert.DoesNotContain("resolved-key", result.Request!.Body);
        result.Stream.Cancel();
        Assert.NotEmpty(socket.Closes);
    }

    [Fact]
    [UpstreamTest(Prefix + "sends binary and base64 audio, skips empty frames, and flushes with CloseStream", Coverage = UpstreamCoverage.Covered, Commit = "5b89b2560ccb9eef8f0fc375367158bdfbbb5724")]
    public async Task Sends_binary_and_base64_audio_skips_empty_frames_and_flushes_with_CloseStream()
    {
        MockWebSocket.Reset();
        var result = await Stream(Model(), Audio(), Pcm16);
        var parts = Collect(result);
        var socket = MockWebSocket.Instances[0];
        socket.Open();
        await Wait.UntilAsync(() => socket.Sent.Count == 3);
        Assert.Equal(new byte[] { 1, 2 }, (byte[])socket.Sent[0]);
        Assert.Equal(new byte[] { 3, 4 }, (byte[])socket.Sent[1]);
        Assert.Equal(@"{""type"":""CloseStream""}", socket.Sent[2]);
        socket.Message(new JsonObject { ["type"] = "Metadata", ["duration"] = 2 });
        var finish = (await parts).Last();
        Assert.Equal("finish", finish.Type);
        Assert.Equal(string.Empty, finish.Text);
        Assert.Equal(2, finish.DurationInSeconds);
        Assert.Equal(2, finish.Usage!.Seconds);
        Assert.Contains(1000, socket.Closes);
    }

    [Theory]
    [InlineData("Bearer")]
    [InlineData("bearer")]
    [UpstreamTest(Prefix + "authenticates temporary tokens using the %s subprotocol", Coverage = UpstreamCoverage.Covered, Commit = "5b89b2560ccb9eef8f0fc375367158bdfbbb5724")]
    public async Task Authenticates_temporary_tokens_using_the_subprotocol(string scheme)
    {
        MockWebSocket.Reset();
        var result = await Stream(Model(), Audio(), Pcm16, headers: new Dictionary<string, string> { ["authorization"] = scheme + " temporary-jwt" });
        Assert.Equal(new[] { "bearer", "temporary-jwt" }, MockWebSocket.Instances[0].Protocols);
        Assert.DoesNotContain("temporary-jwt", result.Request!.Body);
        result.Stream.Cancel();
    }

    [Fact]
    [UpstreamTest(Prefix + "accepts an Authorization override with conventional header casing", Coverage = UpstreamCoverage.Covered, Commit = "5b89b2560ccb9eef8f0fc375367158bdfbbb5724")]
    public async Task Accepts_an_Authorization_override_with_conventional_header_casing()
    {
        MockWebSocket.Reset();
        var result = await Stream(Model(), Audio(), Pcm16, headers: new Dictionary<string, string> { ["Authorization"] = "Bearer temporary-jwt" });
        Assert.Equal(new[] { "bearer", "temporary-jwt" }, MockWebSocket.Instances[0].Protocols);
        result.Stream.Cancel();
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects unsupported authentication schemes without exposing credentials", Coverage = UpstreamCoverage.Covered, Commit = "5b89b2560ccb9eef8f0fc375367158bdfbbb5724")]
    public async Task Rejects_unsupported_authentication_schemes_without_exposing_credentials()
    {
        MockWebSocket.Reset();
        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() => Stream(Model(), Audio(), Pcm16, headers: new Dictionary<string, string> { ["authorization"] = "Basic secret" }));
        Assert.Contains("must use the Token or Bearer scheme", error.Message);
        Assert.DoesNotContain("secret", error.Message);
        Assert.Empty(MockWebSocket.Instances);
    }

    [Fact]
    [UpstreamTest(Prefix + "omits undefined headers before resolving the authentication scheme", Coverage = UpstreamCoverage.Covered, Commit = "5b89b2560ccb9eef8f0fc375367158bdfbbb5724")]
    public async Task Omits_undefined_headers_before_resolving_the_authentication_scheme()
    {
        MockWebSocket.Reset();
        var result = await Stream(Model(), Audio(), Pcm16, headers: new Dictionary<string, string> { ["authorization"] = null!, ["X-Optional"] = null! });
        var socket = MockWebSocket.Instances[0];
        Assert.Null(socket.Protocols);
        Assert.DoesNotContain("authorization", socket.Headers!.Keys);
        Assert.DoesNotContain("X-Optional", socket.Headers.Keys);
        result.Stream.Cancel();
    }

    [Fact]
    [UpstreamTest(Prefix + "maps revisable and final transcripts, word timings, channels, language, raw events and usage", Coverage = UpstreamCoverage.Covered, Commit = "5b89b2560ccb9eef8f0fc375367158bdfbbb5724")]
    public async Task Maps_revisable_and_final_transcripts_word_timings_channels_language_raw_events_and_usage()
    {
        MockWebSocket.Reset();
        var result = await Stream(Model(), Audio(), Pcm16, includeRawChunks: true);
        var parts = Collect(result);
        var socket = MockWebSocket.Instances[0];
        socket.Open();
        await Wait.UntilAsync(() => socket.Sent.Count == 3);
        socket.Message(Results("Hel", false));
        socket.Message(Results("Hello!", true, 1));
        socket.Message(new JsonObject { ["type"] = "Metadata", ["duration"] = 2.5, ["request_id"] = "request-id" });
        var values = await parts;
        Assert.Contains(values, part => part.Type == "raw" && part.Raw!.Value.GetRawText() == Results("Hello!", true, 1).ToJsonString());
        var partial = Assert.Single(values, part => part.Type == "transcript-partial");
        Assert.Equal("Hel", partial.Text);
        Assert.Equal(0, partial.StartSecond);
        Assert.Equal(1.5, partial.DurationInSeconds);
        Assert.Equal(0, partial.ChannelIndex);
        Assert.Equal(@"{""deepgram"":{""speechFinal"":false}}", partial.ProviderMetadata!.Value.GetRawText());
        var final = Assert.Single(values, part => part.Type == "transcript-final");
        Assert.Equal("Hello!", final.Text);
        Assert.Equal(0, final.StartSecond);
        Assert.Equal(1.5, final.EndSecond);
        Assert.Equal(1, final.ChannelIndex);
        Assert.Equal(@"{""deepgram"":{""speechFinal"":true}}", final.ProviderMetadata!.Value.GetRawText());
        var finish = values.Last();
        Assert.Equal("finish", finish.Type);
        Assert.Equal("Hello!", finish.Text);
        var segment = Assert.Single(finish.Segments!);
        Assert.Equal(("Hello!", 0.2, 0.8), (segment.Text, segment.StartSecond, segment.EndSecond));
        Assert.Equal("en", finish.Language);
        Assert.Equal(2.5, finish.DurationInSeconds);
        Assert.Equal(2.5, finish.Usage!.Seconds);
    }

    [Fact]
    [UpstreamTest(Prefix + "keeps only finalized text and joins separate finalized segments", Coverage = UpstreamCoverage.Covered, Commit = "5b89b2560ccb9eef8f0fc375367158bdfbbb5724")]
    public async Task Keeps_only_finalized_text_and_joins_separate_finalized_segments()
    {
        MockWebSocket.Reset();
        var result = await Stream(Model(), Audio(), Pcm16);
        var parts = Collect(result);
        var socket = MockWebSocket.Instances[0];
        socket.Open();
        await Wait.UntilAsync(() => socket.Sent.Count == 3);
        socket.Message(Results("Hello"));
        socket.Message(Results("world"));
        socket.Message(Results("unfinished", false));
        socket.ServerClose();
        var finish = (await parts).Last();
        Assert.Equal("finish", finish.Type);
        Assert.Equal("Hello world", finish.Text);
    }

    [Fact]
    [UpstreamTest(Prefix + "omits native usage when only transcript timing is available", Coverage = UpstreamCoverage.Covered, Commit = "5b89b2560ccb9eef8f0fc375367158bdfbbb5724")]
    public async Task Omits_native_usage_when_only_transcript_timing_is_available()
    {
        MockWebSocket.Reset();
        var result = await Stream(Model(), Audio(), Pcm16);
        var parts = Collect(result);
        var socket = MockWebSocket.Instances[0];
        socket.Open();
        await Wait.UntilAsync(() => socket.Sent.Count == 3);
        socket.Message(Results("Hello!"));
        socket.ServerClose();
        var finish = (await parts).Last();
        Assert.Equal("finish", finish.Type);
        Assert.Equal(1.5, finish.DurationInSeconds);
        Assert.Null(finish.Usage);
    }

    [Fact]
    [UpstreamTest(Prefix + "orders aggregate multichannel transcripts and word timings chronologically", Coverage = UpstreamCoverage.Covered, Commit = "5b89b2560ccb9eef8f0fc375367158bdfbbb5724")]
    public async Task Orders_aggregate_multichannel_transcripts_and_word_timings_chronologically()
    {
        MockWebSocket.Reset();
        var result = await Stream(Model(), Audio(), Pcm16, providerOptions: @"{""deepgram"":{""streaming"":{""channels"":2,""multichannel"":true}}}");
        var parts = Collect(result);
        var socket = MockWebSocket.Instances[0];
        socket.Open();
        await Wait.UntilAsync(() => socket.Sent.Count == 3);
        var later = Results("Second", true, 1);
        later["start"] = 5;
        later["channel"]!["alternatives"]![0]!["words"] = new JsonArray(new JsonObject { ["word"] = "second", ["punctuated_word"] = "Second", ["start"] = 5.2, ["end"] = 5.8 });
        var earlier = Results("First", true, 0);
        earlier["channel"]!["alternatives"]![0]!["words"] = new JsonArray(new JsonObject { ["word"] = "first", ["punctuated_word"] = "First", ["start"] = 0.2, ["end"] = 0.8 });
        socket.Message(later);
        socket.Message(earlier);
        socket.Message(new JsonObject { ["type"] = "Metadata", ["duration"] = 7 });
        var values = await parts;
        var finals = values.Where(part => part.Type == "transcript-final").ToList();
        Assert.Equal(new[] { ("Second", 1), ("First", 0) }, finals.Select(part => (part.Text!, part.ChannelIndex!.Value)));
        var finish = values.Last();
        Assert.Equal("First Second", finish.Text);
        Assert.Equal(new[] { ("First", 0.2, 0.8), ("Second", 5.2, 5.8) }, finish.Segments!.Select(segment => (segment.Text, segment.StartSecond, segment.EndSecond)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [UpstreamTest(Prefix + "accepts speech and utterance events with raw chunks enabled: %s", Coverage = UpstreamCoverage.Covered, Commit = "5b89b2560ccb9eef8f0fc375367158bdfbbb5724")]
    public async Task Accepts_speech_and_utterance_events_with_raw_chunks_enabled(bool includeRawChunks)
    {
        MockWebSocket.Reset();
        var result = await Stream(Model(), Audio(), Pcm16, includeRawChunks: includeRawChunks);
        var parts = Collect(result);
        var socket = MockWebSocket.Instances[0];
        socket.Open();
        await Wait.UntilAsync(() => socket.Sent.Count == 3);
        var events = new JsonNode[]
        {
            new JsonObject { ["type"] = "SpeechStarted", ["channel"] = new JsonArray(0), ["timestamp"] = 0.1 },
            new JsonObject { ["type"] = "UtteranceEnd", ["channel"] = new JsonArray(0, 1), ["last_word_end"] = 1.5 },
            new JsonObject { ["type"] = "FutureEvent", ["channel"] = "provider-specific" },
        };
        foreach (var @event in events)
        {
            socket.Message(@event);
        }

        socket.Message(Results("Hello!"));
        socket.Message(new JsonObject { ["type"] = "Metadata", ["duration"] = 2 });
        var values = await parts;
        var finish = values.Last();
        Assert.Equal("finish", finish.Type);
        Assert.Equal("Hello!", finish.Text);
        var expected = includeRawChunks
            ? events.Select(node => node.ToJsonString()).Concat(new[] { Results("Hello!").ToJsonString(), @"{""type"":""Metadata"",""duration"":2}" }).ToList()
            : new List<string>();
        Assert.Equal(expected, values.Where(part => part.Type == "raw").Select(part => part.Raw!.Value.GetRawText()).ToList());
    }

    [Theory]
    [InlineData("audio/pcmu", "mulaw")]
    [InlineData("audio/pcma", "alaw")]
    [UpstreamTest(Prefix + "supports %s", Coverage = UpstreamCoverage.Covered, Commit = "5b89b2560ccb9eef8f0fc375367158bdfbbb5724")]
    public async Task Supports_audio_format(string type, string encoding)
    {
        MockWebSocket.Reset();
        var result = await Stream(Model(), Audio(), new AudioFormat(type, 8000));
        Assert.Equal(encoding, Query(MockWebSocket.Instances[0].Url)["encoding"][0]);
        result.Stream.Cancel();
    }

    [Theory]
    [InlineData("audio/mp3", 16000.0)]
    [InlineData("audio/pcm", null)]
    [InlineData("audio/pcm", 0.0)]
    [InlineData("audio/pcm", 1.5)]
    [UpstreamTest(Prefix + "rejects unsupported raw formats or sample rates: %j", Coverage = UpstreamCoverage.Covered, Commit = "5b89b2560ccb9eef8f0fc375367158bdfbbb5724")]
    public async Task Rejects_unsupported_raw_formats_or_sample_rates(string type, double? rate)
    {
        MockWebSocket.Reset();
        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() => Stream(Model(), Audio(), new AudioFormat(type, rate)));
        Assert.Contains("Deepgram streaming requires", error.Message);
        Assert.Empty(MockWebSocket.Instances);
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects Flux models which use a distinct protocol", Coverage = UpstreamCoverage.Covered, Commit = "5b89b2560ccb9eef8f0fc375367158bdfbbb5724")]
    public async Task Rejects_Flux_models_which_use_a_distinct_protocol()
    {
        var provider = DeepgramProvider.Create(new DeepgramOptions { ApiKey = "test-key", WebSocket = MockWebSocket.Factory });
        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => Stream(provider.TranscriptionModel("flux-general-en"), Audio(), Pcm16));
        Assert.Contains("Flux", error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "warns about batch-only options", Coverage = UpstreamCoverage.Covered, Commit = "5b89b2560ccb9eef8f0fc375367158bdfbbb5724")]
    public async Task Warns_about_batch_only_options()
    {
        MockWebSocket.Reset();
        var result = await Stream(Model(), Audio(), Pcm16, providerOptions: @"{""deepgram"":{""summarize"":""v2"",""detectLanguage"":true,""streaming"":{""interimResults"":true}}}");
        var parts = Collect(result);
        var socket = MockWebSocket.Instances[0];
        socket.Open();
        await Wait.UntilAsync(() => socket.Sent.Count == 3);
        socket.ServerClose();
        var first = (await parts)[0];
        Assert.Equal("stream-start", first.Type);
        Assert.Equal(new[] { "providerOptions.deepgram.detectLanguage", "providerOptions.deepgram.summarize" }, first.Warnings!.Select(warning => warning.Feature));
        Assert.False(Query(socket.Url).ContainsKey("summarize"));
    }

    [Fact]
    [UpstreamTest(Prefix + "does not warn about explicitly disabled batch-only options", Coverage = UpstreamCoverage.Covered, Commit = "5b89b2560ccb9eef8f0fc375367158bdfbbb5724")]
    public async Task Does_not_warn_about_explicitly_disabled_batch_only_options()
    {
        MockWebSocket.Reset();
        var result = await Stream(Model(), Audio(), Pcm16, providerOptions: @"{""deepgram"":{""detectLanguage"":false,""summarize"":false,""utterances"":false}}");
        var parts = Collect(result);
        var socket = MockWebSocket.Instances[0];
        socket.Open();
        await Wait.UntilAsync(() => socket.Sent.Count == 3);
        socket.ServerClose();
        var first = (await parts)[0];
        Assert.Equal("stream-start", first.Type);
        Assert.Empty(first.Warnings!);
        Assert.False(Query(socket.Url).ContainsKey("detect_language"));
    }

    [Fact]
    [UpstreamTest(Prefix + "cancels the audio source when the output is cancelled before opening", Coverage = UpstreamCoverage.Covered, Commit = "5b89b2560ccb9eef8f0fc375367158bdfbbb5724")]
    public async Task Cancels_the_audio_source_when_the_output_is_cancelled_before_opening()
    {
        MockWebSocket.Reset();
        var cancels = 0;
        var result = await Stream(Model(), new AudioChunkStream(_ => cancels++), Pcm16);
        result.Stream.Cancel();
        Assert.Equal(1, cancels);
        Assert.NotEmpty(MockWebSocket.Instances[0].Closes);
    }

    [Fact]
    [UpstreamTest(Prefix + "aborts an active connection and cancels a pending audio read", Coverage = UpstreamCoverage.Partial, Note = "A cancelled token carries no reason, so the test checks for cancellation instead of the 'stop' message.", Commit = "5b89b2560ccb9eef8f0fc375367158bdfbbb5724")]
    public async Task Aborts_an_active_connection_and_cancels_a_pending_audio_read()
    {
        MockWebSocket.Reset();
        var cancels = 0;
        using var controller = new CancellationTokenSource();
        var result = await Stream(Model(), new AudioChunkStream(_ => cancels++), Pcm16, cancellationToken: controller.Token);
        var parts = Collect(result);
        MockWebSocket.Instances[0].Open();
        controller.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => parts);
        Assert.Equal(1, cancels);
        Assert.NotEmpty(MockWebSocket.Instances[0].Closes);
    }

    [Fact]
    [UpstreamTest(Prefix + "handles an already aborted request without opening a socket", Coverage = UpstreamCoverage.Partial, Note = "A cancelled token carries no reason, so the test checks for cancellation instead of the 'already stopped' message.", Commit = "5b89b2560ccb9eef8f0fc375367158bdfbbb5724")]
    public async Task Handles_an_already_aborted_request_without_opening_a_socket()
    {
        MockWebSocket.Reset();
        var cancels = 0;
        using var controller = new CancellationTokenSource();
        controller.Cancel();
        var result = await Stream(Model(), new AudioChunkStream(_ => cancels++), Pcm16, cancellationToken: controller.Token);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Collect(result));
        Assert.Equal(1, cancels);
        Assert.Empty(MockWebSocket.Instances);
    }

    [Theory]
    [InlineData("provider")]
    [InlineData("malformed")]
    [InlineData("socket")]
    [InlineData("early-close")]
    [InlineData("abnormal-close")]
    [UpstreamTest(Prefix + "propagates %s failures and closes the source", Coverage = UpstreamCoverage.Covered, Commit = "5b89b2560ccb9eef8f0fc375367158bdfbbb5724")]
    public async Task Propagates_failures_and_closes_the_source(string failure)
    {
        MockWebSocket.Reset();
        var cancels = 0;
        var result = await Stream(Model(), new AudioChunkStream(_ => cancels++), Pcm16);
        var parts = Collect(result);
        var socket = MockWebSocket.Instances[0];
        socket.Open();
        if (failure == "provider")
        {
            socket.Message(new JsonObject { ["type"] = "Error", ["description"] = "invalid audio" });
        }

        if (failure == "malformed")
        {
            socket.OnMessage!("invalid json");
        }

        if (failure == "socket")
        {
            socket.OnError!(null);
        }

        if (failure == "early-close")
        {
            socket.ServerClose();
        }

        if (failure == "abnormal-close")
        {
            socket.ServerClose(1011);
        }

        await Assert.ThrowsAnyAsync<Exception>(() => parts);
        Assert.Equal(1, cancels);
        Assert.NotEmpty(socket.Closes);
    }

    [Fact]
    [UpstreamTest(Prefix + "propagates errors from the input producer", Coverage = UpstreamCoverage.Covered, Commit = "5b89b2560ccb9eef8f0fc375367158bdfbbb5724")]
    public async Task Propagates_errors_from_the_input_producer()
    {
        MockWebSocket.Reset();
        var audio = new AudioChunkStream();
        audio.Fail(new InvalidOperationException("microphone disconnected"));
        var result = await Stream(Model(), audio, Pcm16);
        var parts = Collect(result);
        MockWebSocket.Instances[0].Open();
        var error = await Assert.ThrowsAnyAsync<Exception>(() => parts);
        Assert.Equal("microphone disconnected", error.Message);
        Assert.NotEmpty(MockWebSocket.Instances[0].Closes);
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves server close codes and reasons in the error", Coverage = UpstreamCoverage.Covered, Commit = "5b89b2560ccb9eef8f0fc375367158bdfbbb5724")]
    public async Task Preserves_server_close_codes_and_reasons_in_the_error()
    {
        MockWebSocket.Reset();
        var result = await Stream(Model(), new AudioChunkStream(), Pcm16);
        var parts = Collect(result);
        var socket = MockWebSocket.Instances[0];
        socket.Open();
        socket.ServerClose(1008, "DATA-0000");
        var error = await Assert.ThrowsAnyAsync<Exception>(() => parts);
        Assert.Contains("closed unexpectedly (1008): DATA-0000", error.Message);
        Assert.NotEmpty(socket.Closes);
    }

    [Fact]
    [UpstreamTest(Prefix + "pauses input reads until the WebSocket send buffer drains", Coverage = UpstreamCoverage.Covered, Commit = "5b89b2560ccb9eef8f0fc375367158bdfbbb5724")]
    public async Task Pauses_input_reads_until_the_WebSocket_send_buffer_drains()
    {
        MockWebSocket.Reset();
        var result = await Stream(Model(), Audio(), Pcm16);
        var socket = MockWebSocket.Instances[0];
        socket.BufferedAmount = 2 * 1024 * 1024;
        socket.Open();
        await Wait.UntilAsync(() => socket.Sent.Count == 1);
        await Wait.FlushAsync();
        Assert.Single(socket.Sent);
        socket.BufferedAmount = 0;
        await Wait.UntilAsync(() => socket.Sent.Contains(@"{""type"":""CloseStream""}"));
        result.Stream.Cancel();
    }

    [Fact]
    [UpstreamTest(Prefix + "sends keepalive messages while waiting for live audio and stops on cancellation", Coverage = UpstreamCoverage.Covered, Commit = "5b89b2560ccb9eef8f0fc375367158bdfbbb5724")]
    public async Task Sends_keepalive_messages_while_waiting_for_live_audio_and_stops_on_cancellation()
    {
        MockWebSocket.Reset();
        var result = await Stream(Model(), new AudioChunkStream(), Pcm16);
        var socket = MockWebSocket.Instances[0];
        socket.Open();
        await Wait.UntilAsync(() => socket.Sent.Contains(@"{""type"":""KeepAlive""}"), 8000);
        result.Stream.Cancel();
        var sends = socket.Sent.Count;
        await Task.Delay(4500);
        Assert.Equal(sends, socket.Sent.Count);
    }

    private static ITranscriptionCaller Model()
    {
        var provider = DeepgramProvider.Create(new DeepgramOptions { ApiKey = "test-key", WebSocket = MockWebSocket.Factory });
        return (ITranscriptionCaller)provider.TranscriptionModel("nova-3");
    }

    private static ITranscriptionCaller Model(Vercel.AI.Provider.ITranscriptionModel model)
    {
        return (ITranscriptionCaller)model;
    }

    private static Task<TranscriptionStreamStart> Stream(ITranscriptionCaller model, AudioChunkStream audio, AudioFormat format, bool? includeRawChunks = null, string? providerOptions = null, IReadOnlyDictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return StartAsync(model, audio, format, includeRawChunks, providerOptions, headers, cancellationToken);
    }

    private static Task<TranscriptionStreamStart> Stream(Vercel.AI.Provider.ITranscriptionModel model, AudioChunkStream audio, AudioFormat format, bool? includeRawChunks = null, string? providerOptions = null, IReadOnlyDictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return StartAsync(Model(model), audio, format, includeRawChunks, providerOptions, headers, cancellationToken);
    }

    private static async Task<TranscriptionStreamStart> StartAsync(ITranscriptionCaller model, AudioChunkStream audio, AudioFormat format, bool? includeRawChunks, string? providerOptions, IReadOnlyDictionary<string, string>? headers, CancellationToken cancellationToken)
    {
        var call = new TranscriptionStreamCall(audio, format, OperationJson.Parse(providerOptions ?? "{}"), headers ?? new Dictionary<string, string>(), includeRawChunks, cancellationToken);
        var start = await model.DoStreamAsync(call, cancellationToken);
        return start!;
    }

    private static async Task<List<ModelStreamPart>> Collect(TranscriptionStreamStart start)
    {
        var parts = new List<ModelStreamPart>();
        await foreach (var part in start.Stream.ReadAsync())
        {
            parts.Add(part);
        }

        return parts;
    }

    private static Dictionary<string, List<string>> Query(Uri url)
    {
        var query = new Dictionary<string, List<string>>();
        foreach (var pair in url.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var index = pair.IndexOf('=');
            var key = Uri.UnescapeDataString(pair.Substring(0, index));
            if (!query.TryGetValue(key, out var list))
            {
                query[key] = list = new List<string>();
            }

            list.Add(Uri.UnescapeDataString(pair.Substring(index + 1)));
        }

        return query;
    }

    private static AudioChunkStream Audio()
    {
        var audio = new AudioChunkStream();
        audio.Write(new byte[] { 1, 2 });
        audio.Write("AwQ=");
        audio.Write(Array.Empty<byte>());
        audio.Complete();
        return audio;
    }

    private static JsonObject Results(string text, bool isFinal = true, int channel = 0)
    {
        return new JsonObject
        {
            ["type"] = "Results",
            ["is_final"] = isFinal,
            ["speech_final"] = isFinal,
            ["start"] = 0,
            ["duration"] = 1.5,
            ["channel_index"] = new JsonArray(channel, 2),
            ["channel"] = new JsonObject
            {
                ["alternatives"] = new JsonArray(new JsonObject
                {
                    ["transcript"] = text,
                    ["languages"] = new JsonArray("en"),
                    ["words"] = new JsonArray(new JsonObject { ["word"] = "hello", ["punctuated_word"] = "Hello!", ["start"] = 0.2, ["end"] = 0.8 }),
                }),
            },
        };
    }
}
