// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Tests.Upstream.ProviderUtils;
using static Vercel.AI.Tests.Upstream.OpenAI.OpenAITranscriptionModel.OpenAITranscriptionTestSupport;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAITranscriptionModel;

/// <summary>Port of <c>openai-transcription-model.test.ts</c> &gt; <c>doStream</c>.</summary>
[Collection("MockWebSocket")]
public sealed class DoStreamTests
{
    private const string Prefix = "packages/openai/src/transcription/openai-transcription-model.test.ts::doStream::";

    public static TheoryData<string, bool> LanguagesCases => new()
    {
        { "gpt-live-transcribe", true },
        { "gpt-live-transcribe-2026-01-01", true },
        { "gpt-transcribe", false },
        { "gpt-transcribe-2026-01-01", false },
    };

    [Fact]
    [UpstreamTest(Prefix + "should stream gpt-realtime-whisper using OpenAI realtime transcription", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_gpt_realtime_whisper_using_OpenAI_realtime_transcription()
    {
        MockWebSocket.Reset();
        var model = Model("gpt-realtime-whisper", clock: () => DateTimeOffset.UnixEpoch);

        var (start, parts, ws) = await StartAsync(
            model,
            providerOptions: "{\"openai\":{\"language\":\"en\",\"streaming\":{\"delay\":\"low\",\"include\":[\"item.input_audio_transcription.logprobs\"]}}}");

        Assert.Equal("wss://api.openai.com/v1/realtime?intent=transcription", ws.Url.AbsoluteUri);
        Assert.Equal(new[] { "realtime", "openai-insecure-api-key.test-api-key" }, ws.Protocols);

        // OpenAI rejects handshakes that send both auth channels:
        Assert.DoesNotContain(ws.Headers!.Keys, key => key.Equals("authorization", StringComparison.OrdinalIgnoreCase));

        ws.Open();
        await Wait.UntilAsync(() => ws.Sent.Count >= 3);

        const string sessionUpdate = "{\"type\":\"session.update\",\"session\":{\"type\":\"transcription\",\"audio\":{\"input\":{\"format\":{\"type\":\"audio/pcm\",\"rate\":24000},"
            + "\"transcription\":{\"model\":\"gpt-realtime-whisper\",\"language\":\"en\",\"delay\":\"low\"},\"turn_detection\":null}},\"include\":[\"item.input_audio_transcription.logprobs\"]}}";
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(sessionUpdate), Sent(ws, 0)));
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"type\":\"input_audio_buffer.append\",\"audio\":\"AQID\"}"), Sent(ws, 1)));
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"type\":\"input_audio_buffer.commit\"}"), Sent(ws, 2)));

        ws.Message(new JsonObject { ["type"] = Transcription + ".delta", ["item_id"] = "item-1", ["delta"] = "Hel" });
        ws.Message(Completed("Hello"));

        var result = await parts;
        Assert.Equal(new[] { "stream-start", "transcript-delta", "transcript-final", "finish" }, result.Select(part => part.Type).ToArray());
        Assert.Empty(result[0].Warnings!);
        Assert.Equal(("item-1", "Hel"), (result[1].Id, result[1].Delta));
        Assert.Equal(("item-1", "Hello"), (result[2].Id, result[2].Text));
        Assert.Equal("Hello", result[3].Text);
        Assert.Empty(result[3].Segments!);
        Assert.Equal("en", result[3].Language);
        Assert.Equal(DateTime.UnixEpoch, start.Response!.Timestamp);
        Assert.Equal("gpt-realtime-whisper", start.Response.ModelId);
    }

    [Theory]
    [MemberData(nameof(LanguagesCases))]
    [UpstreamTest(Prefix + "should stream %s and send the language hint as a languages list", Coverage = UpstreamCoverage.Covered, Commit = "8ba3940d3dd1f3cd3c4365bbaec505c6ef0eb1e3")]
    public async Task Should_stream_model_and_send_the_language_hint_as_a_languages_list(string modelId, bool supportsDelay)
    {
        MockWebSocket.Reset();
        var model = Model(modelId);

        var (_, parts, ws) = await StartAsync(model, providerOptions: "{\"openai\":{\"language\":\"en\",\"streaming\":{\"delay\":\"low\"}}}");

        Assert.Equal("wss://api.openai.com/v1/realtime?intent=transcription", ws.Url.AbsoluteUri);
        ws.Open();
        await Wait.UntilAsync(() => ws.Sent.Count >= 1);

        var transcription = "\"model\":\"" + modelId + "\",\"languages\":[\"en\"]" + (supportsDelay ? ",\"delay\":\"low\"" : string.Empty);
        var sessionUpdate = "{\"type\":\"session.update\",\"session\":{\"type\":\"transcription\",\"audio\":{\"input\":{\"format\":{\"type\":\"audio/pcm\",\"rate\":24000},"
            + "\"transcription\":{" + transcription + "},\"turn_detection\":null}}}}";
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(sessionUpdate), Sent(ws, 0)));

        ws.Message(Completed("Hello"));
        var result = await parts;
        var final = Assert.Single(result, part => part.Type == "transcript-final");
        Assert.Equal(("item-1", "Hello"), (final.Id, final.Text));
        Assert.Equal("stream-start", result[0].Type);
        if (supportsDelay)
        {
            Assert.Empty(result[0].Warnings!);
        }
        else
        {
            var warning = Assert.Single(result[0].Warnings!);
            Assert.Equal("unsupported", warning.Type);
            Assert.Equal("providerOptions.openai.streaming.delay", warning.Feature);
            Assert.Equal("OpenAI streaming transcription with " + modelId + " does not support delay.", warning.Details);
        }
    }

    [Theory]
    [InlineData("gpt-4o-transcribe")]
    [InlineData("whisper-1")]
    [UpstreamTest(Prefix + "should reject %s for streaming transcription", Coverage = UpstreamCoverage.Covered, Commit = "8ba3940d3dd1f3cd3c4365bbaec505c6ef0eb1e3")]
    public async Task Should_reject_model_for_streaming_transcription(string modelId)
    {
        MockWebSocket.Reset();
        var model = Model(modelId);

        await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => StartAsync(model));
        Assert.Empty(MockWebSocket.Instances);
    }

    [Fact]
    [UpstreamTest(Prefix + "should accept dated gpt-realtime-whisper snapshot model IDs", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_accept_dated_gpt_realtime_whisper_snapshot_model_IDs()
    {
        MockWebSocket.Reset();
        var model = Model("gpt-realtime-whisper-2026-01-01");

        var (_, parts, ws) = await StartAsync(model);
        ws.Open();
        await Wait.UntilAsync(() => ws.Sent.Count >= 1);

        Assert.Equal("gpt-realtime-whisper-2026-01-01", Sent(ws, 0)!["session"]!["audio"]!["input"]!["transcription"]!["model"]!.GetValue<string>());

        ws.Message(Completed("Hello"));
        Assert.NotEmpty(await parts);
    }

    [Fact]
    [UpstreamTest(Prefix + "should strip only the Authorization header and pass other headers to the WebSocket constructor", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_strip_only_the_Authorization_header_and_pass_other_headers_to_the_WebSocket_constructor()
    {
        MockWebSocket.Reset();
        var model = Model(
            "gpt-realtime-whisper",
            () => new Dictionary<string, string?>
            {
                ["Authorization"] = "Bearer test-api-key",
                ["OpenAI-Organization"] = "test-organization",
                ["Custom-Header"] = "custom-value",
            });

        var (_, parts, ws) = await StartAsync(model);

        Assert.Equal(new[] { "realtime", "openai-insecure-api-key.test-api-key" }, ws.Protocols);
        Assert.Equal("test-organization", ws.Headers!["OpenAI-Organization"]);
        Assert.Equal("custom-value", ws.Headers["Custom-Header"]);
        Assert.DoesNotContain(ws.Headers.Keys, key => key.Equals("authorization", StringComparison.OrdinalIgnoreCase));

        ws.Open();
        await Wait.UntilAsync(() => ws.Sent.Count >= 3);
        ws.Message(Completed("Hello"));
        Assert.NotNull(await parts);
    }

    [Fact]
    [UpstreamTest(Prefix + "should use only the realtime protocol and pass headers unchanged when there is no Authorization header", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_only_the_realtime_protocol_and_pass_headers_unchanged_when_there_is_no_Authorization_header()
    {
        MockWebSocket.Reset();
        var model = Model("gpt-realtime-whisper", () => new Dictionary<string, string?> { ["Custom-Header"] = "custom-value" });

        var (_, parts, ws) = await StartAsync(model);

        Assert.Equal(new[] { "realtime" }, ws.Protocols);
        Assert.Equal("custom-value", ws.Headers!["Custom-Header"]);

        ws.Open();
        await Wait.UntilAsync(() => ws.Sent.Count >= 3);
        ws.Message(Completed("Hello"));
        Assert.NotNull(await parts);
    }

    [Fact]
    [UpstreamTest(Prefix + "should strip undefined header values before the WebSocket constructor", Coverage = UpstreamCoverage.Partial, Note = "TranscriptionStreamCall.Headers is not nullable, so the unset value comes from the configuration headers instead of the call headers.")]
    public async Task Partial_Should_strip_undefined_header_values_before_the_WebSocket_constructor()
    {
        MockWebSocket.Reset();
        var model = Model(
            "gpt-realtime-whisper",
            () => new Dictionary<string, string?> { ["Authorization"] = "Bearer test-api-key", ["X-Unset"] = null });

        var (start, _, ws) = await StartAsync(model, headers: new Dictionary<string, string> { ["Custom-Header"] = "custom-value" });

        start.Stream.Cancel();
        Assert.DoesNotContain("X-Unset", ws.Headers!.Keys);
        Assert.DoesNotContain(ws.Headers.Values, value => value == null);
        Assert.Equal("custom-value", ws.Headers["Custom-Header"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should cancel the audio stream when the WebSocket constructor throws", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_cancel_the_audio_stream_when_the_WebSocket_constructor_throws()
    {
        var audioCancelled = false;
        var audio = new AudioChunkStream(_ => audioCancelled = true);
        var model = Model("gpt-realtime-whisper", webSocket: (_, _, _) => throw new InvalidOperationException("constructor failed"));

        var (_, parts, _) = await StartThrowingAsync(model, audio);

        var error = await Assert.ThrowsAnyAsync<Exception>(() => parts);
        Assert.Equal("constructor failed", error.Message);
        Assert.True(audioCancelled);
    }

    [Fact]
    [UpstreamTest(Prefix + "should cancel the audio stream when an audio send throws mid-stream", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_cancel_the_audio_stream_when_an_audio_send_throws_mid_stream()
    {
        MockWebSocket.Reset();
        var audioCancelled = false;
        var audio = new AudioChunkStream(_ => audioCancelled = true);
        audio.Write(new byte[] { 1, 2, 3 });
        var model = Model("gpt-realtime-whisper");

        var (_, parts, ws) = await StartAsync(model, audio);
        ws.SendHook = data =>
        {
            if (data.Contains("input_audio_buffer.append", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("send failed");
            }
        };

        ws.Open();
        var error = await Assert.ThrowsAnyAsync<Exception>(() => parts);
        Assert.Equal("send failed", error.Message);
        Assert.True(audioCancelled);
    }

    [Fact]
    [UpstreamTest(Prefix + "should carry mixed-case Authorization headers into the subprotocol and strip them", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_carry_mixed_case_Authorization_headers_into_the_subprotocol_and_strip_them()
    {
        MockWebSocket.Reset();
        var model = Model("gpt-realtime-whisper", () => new Dictionary<string, string?> { ["AUTHORIZATION"] = "Bearer test-api-key" });

        var (start, _, ws) = await StartAsync(model);

        start.Stream.Cancel();
        Assert.Equal(new[] { "realtime", "openai-insecure-api-key.test-api-key" }, ws.Protocols);
        Assert.DoesNotContain(ws.Headers!.Keys.Select(key => key.ToLowerInvariant()), key => key == "authorization");
    }

    [Fact]
    [UpstreamTest(Prefix + "should let per-call authorization headers override configuration headers across casings", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_let_per_call_authorization_headers_override_configuration_headers_across_casings()
    {
        MockWebSocket.Reset();
        var model = Model("gpt-realtime-whisper", () => new Dictionary<string, string?> { ["Authorization"] = "Bearer config-key" });

        // lowercase scheme + lowercase header key, both case-insensitive
        var (start, _, ws) = await StartAsync(model, headers: new Dictionary<string, string> { ["authorization"] = "bearer per-call-key" });

        start.Stream.Cancel();
        Assert.Equal(new[] { "realtime", "openai-insecure-api-key.per-call-key" }, ws.Protocols);
        Assert.DoesNotContain(ws.Headers!.Keys.Select(key => key.ToLowerInvariant()), key => key == "authorization");
    }

    [Fact]
    [UpstreamTest(Prefix + "should warn about unsupported non-streaming provider options", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_warn_about_unsupported_non_streaming_provider_options()
    {
        MockWebSocket.Reset();
        var model = Model("gpt-realtime-whisper");

        var (_, parts, ws) = await StartAsync(
            model,
            providerOptions: "{\"openai\":{\"languages\":[\"en\",\"fr\"],\"keywords\":[\"AC-42\"],\"prompt\":\"context prompt\",\"temperature\":0.5}}");
        ws.Open();
        await Wait.UntilAsync(() => ws.Sent.Count >= 3);
        ws.Message(Completed("Hello"));

        var warnings = (await parts)[0].Warnings!;
        Assert.Equal(
            new[]
            {
                ("providerOptions.openai.languages", "OpenAI streaming transcription does not support languages."),
                ("providerOptions.openai.keywords", "OpenAI streaming transcription does not support keywords."),
                ("providerOptions.openai.prompt", "OpenAI streaming transcription does not support prompt."),
                ("providerOptions.openai.temperature", "OpenAI streaming transcription does not support temperature."),
            },
            warnings.Select(warning => (warning.Feature!, warning.Details!)).ToArray());
        Assert.All(warnings, warning => Assert.Equal("unsupported", warning.Type));
    }

    [Fact]
    [UpstreamTest(Prefix + "should error the stream with the server message on error events", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_error_the_stream_with_the_server_message_on_error_events()
    {
        MockWebSocket.Reset();
        var model = Model("gpt-realtime-whisper");

        var (_, parts, ws) = await StartAsync(model);
        ws.Open();
        await Wait.UntilAsync(() => ws.Sent.Count >= 3);

        ws.Message(new JsonObject { ["type"] = "error", ["error"] = new JsonObject { ["message"] = "invalid session configuration" } });
        var error = await Assert.ThrowsAnyAsync<Exception>(() => parts);

        Assert.Contains("invalid session configuration", error.Message, StringComparison.Ordinal);
        Assert.NotEmpty(ws.Closes);
    }

    [Fact]
    [UpstreamTest(Prefix + "should close the WebSocket and stop reading audio when the stream is cancelled", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_close_the_WebSocket_and_stop_reading_audio_when_the_stream_is_cancelled()
    {
        MockWebSocket.Reset();
        var audioCancelled = false;
        var audio = new AudioChunkStream(_ => audioCancelled = true);
        var model = Model("gpt-realtime-whisper");

        var (start, _, ws) = await StartAsync(model, audio);
        ws.Open();
        await Wait.UntilAsync(() => ws.Sent.Count >= 1);

        start.Stream.Cancel();
        await Wait.FlushAsync();

        Assert.NotEmpty(ws.Closes);
        Assert.True(audioCancelled);
    }

    // The failing factory leaves no instance, so the stream is read without a socket.
    private static async Task<(TranscriptionStreamStart Start, Task<List<ModelStreamPart>> Parts, MockWebSocket? Socket)> StartThrowingAsync(
        Vercel.AI.OpenAI.OpenAITranscriptionModel model,
        AudioChunkStream audio)
    {
        var call = new TranscriptionStreamCall(
            audio,
            new AudioFormat("audio/pcm", 24000),
            System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>("{}"),
            new Dictionary<string, string>(),
            null,
            CancellationToken.None);
        var start = await model.DoStreamAsync(call, CancellationToken.None);
        return (start!, Collect(start!), null);
    }
}
