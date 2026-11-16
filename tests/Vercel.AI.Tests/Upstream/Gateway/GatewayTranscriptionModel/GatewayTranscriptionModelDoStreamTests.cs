// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Operations;
using Vercel.AI.Tests.Upstream.ProviderUtils;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayTranscriptionModel.GatewayTranscriptionModelSupport;
using TimeoutError = Vercel.AI.Util.TimeoutError;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayTranscriptionModel;

/// <summary>Port of <c>gateway-transcription-model.test.ts</c> &gt; <c>GatewayTranscriptionModel &gt; doStream</c>.</summary>
[Collection("MockWebSocket")]
public sealed class GatewayTranscriptionModelDoStreamTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-transcription-model.test.ts::GatewayTranscriptionModel > doStream::";

    private const string FinishFrame = """{"type":"finish","text":"","segments":[]}""";

    public GatewayTranscriptionModelDoStreamTests()
    {
        MockWebSocket.Reset();
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should connect to the ws transcription-model URL with the model id in the query",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_connect_to_the_ws_transcription_model_URL_with_the_model_id_in_the_query()
    {
        var model = CreateStreamingTestModel();

        var result = await DoStream(model);

        var partsTask = Collect(result!);
        var ws = MockWebSocket.Instances[0];
        Assert.Equal(
            "wss://api.test.com/transcription-model?ai-model-id=openai%2Fgpt-realtime-whisper",
            ws.Url.AbsoluteUri
        );

        ws.Open();
        await Wait.FlushAsync();
        ws.Message(Node(FinishFrame));
        await partsTask;
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should carry auth in the subprotocols",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_carry_auth_in_the_subprotocols()
    {
        var model = CreateStreamingTestModel();

        var result = await DoStream(model);

        result!.Stream.Cancel();
        Assert.Equal(
            new[] { "ai-gateway-transcription.v1", "ai-gateway-auth.test-token" },
            MockWebSocket.Instances[0].Protocols
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should carry the team scope in the subprotocols when configured",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_carry_the_team_scope_in_the_subprotocols_when_configured()
    {
        var model = CreateStreamingTestModel(
            new Dictionary<string, string?>
            {
                ["Authorization"] = "Bearer test-token",
                ["ai-gateway-auth-method"] = "api-key",
                ["x-vercel-ai-gateway-team"] = "my-team",
            }
        );

        var result = await DoStream(model);

        result!.Stream.Cancel();
        Assert.Equal(
            new[]
            {
                "ai-gateway-transcription.v1",
                "ai-gateway-auth.test-token",
                "ai-gateway-team." + Base64Url("my-team"),
            },
            MockWebSocket.Instances[0].Protocols
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should derive the subprotocols from headers case-insensitively",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_derive_the_subprotocols_from_headers_case_insensitively()
    {
        var model = CreateStreamingTestModel(
            new Dictionary<string, string?>
            {
                ["AUTHORIZATION"] = "Bearer test-token",
                ["ai-gateway-auth-method"] = "api-key",
                ["X-Vercel-AI-Gateway-Team"] = "team1",
            }
        );

        var result = await DoStream(model);

        result!.Stream.Cancel();
        Assert.Equal(
            new[]
            {
                "ai-gateway-transcription.v1",
                "ai-gateway-auth.test-token",
                "ai-gateway-team." + Base64Url("team1"),
            },
            MockWebSocket.Instances[0].Protocols
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass the resolved headers to header-capable WebSocket implementations",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_the_resolved_headers_to_header_capable_WebSocket_implementations()
    {
        var model = CreateStreamingTestModel();

        var result = await DoStream(
            model,
            headers: new Dictionary<string, string?> { ["Custom-Header"] = "test-value" }
        );

        result!.Stream.Cancel();
        var headers = MockWebSocket.Instances[0].Headers!;
        Assert.Equal("Bearer test-token", headers["Authorization"]);
        Assert.Equal("test-value", headers["Custom-Header"]);
        Assert.Equal("4", headers["ai-transcription-model-specification-version"]);
        Assert.Equal("openai/gpt-realtime-whisper", headers["ai-model-id"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should strip undefined header values before passing headers to the WebSocket constructor",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_strip_undefined_header_values_before_passing_headers_to_the_WebSocket_constructor()
    {
        var model = CreateStreamingTestModel();

        // A null value is the documented way to unset a header; header-capable WebSocket implementations throw on it.
        var result = await DoStream(
            model,
            headers: new Dictionary<string, string?>
            {
                ["Custom-Header"] = "test-value",
                ["X-Unset-Header"] = null,
            }
        );

        result!.Stream.Cancel();
        var headers = MockWebSocket.Instances[0].Headers!;
        Assert.DoesNotContain("X-Unset-Header", headers.Keys);
        Assert.DoesNotContain(null, headers.Values);
        Assert.Equal("test-value", headers["Custom-Header"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send the session start frame on open",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_the_session_start_frame_on_open()
    {
        var model = CreateStreamingTestModel();

        var result = await DoStream(
            model,
            providerOptions: """{"openai":{"language":"en"}}""",
            includeRawChunks: true
        );

        var partsTask = Collect(result!);
        var ws = MockWebSocket.Instances[0];
        Assert.Empty(ws.Sent);

        ws.Open();
        await Wait.FlushAsync();

        var expected = Node(
            """{"type":"transcription-stream.start","inputAudioFormat":{"type":"audio/pcm","rate":16000},"providerOptions":{"openai":{"language":"en"}},"includeRawChunks":true}"""
        );
        Assert.True(JsonNode.DeepEquals(expected, Parse(ws.Sent[0])));
        Assert.True(JsonNode.DeepEquals(expected, JsonNode.Parse(result!.Request!.Body!)));

        ws.Message(Node(FinishFrame));
        await partsTask;
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit optional session start frame keys when undefined",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_optional_session_start_frame_keys_when_undefined()
    {
        var model = CreateStreamingTestModel();

        var result = await DoStream(model, inputAudioFormat: new AudioFormat("audio/pcm"));

        var partsTask = Collect(result!);
        var ws = MockWebSocket.Instances[0];
        ws.Open();
        await Wait.FlushAsync();

        Assert.True(
            JsonNode.DeepEquals(
                Node(
                    """{"type":"transcription-stream.start","inputAudioFormat":{"type":"audio/pcm"}}"""
                ),
                Parse(ws.Sent[0])
            )
        );

        ws.Message(Node(FinishFrame));
        await partsTask;
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should split audio chunks larger than the maximum frame size",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_split_audio_chunks_larger_than_the_maximum_frame_size()
    {
        var model = CreateStreamingTestModel();
        // 150 KiB chunk => 64 KiB + 64 KiB + 22 KiB frames
        var bigChunk = Enumerable.Repeat((byte)7, 150 * 1024).ToArray();

        var result = await DoStream(model, audio: AudioOf(bigChunk));

        var partsTask = Collect(result!);
        var ws = MockWebSocket.Instances[0];
        ws.Open();
        await Wait.UntilAsync(() =>
            ws.Sent.OfType<string>().Count(frame => frame.Contains("audio-done")) == 1
        );

        var binaryFrames = ws.Sent.OfType<byte[]>().ToList();
        Assert.Equal(
            new[] { 64 * 1024, 64 * 1024, 22 * 1024 },
            binaryFrames.Select(frame => frame.Length)
        );
        Assert.Equal(bigChunk.Length, binaryFrames.Sum(frame => frame.Length));

        ws.Message(Node(FinishFrame));
        await partsTask;
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send audio as binary frames followed by audio-done",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_audio_as_binary_frames_followed_by_audio_done()
    {
        var model = CreateStreamingTestModel();
        var audio = new AudioChunkStream();
        audio.Write(new byte[] { 1, 2, 3 });
        audio.Write("BAUG"); // base64 for [4, 5, 6]
        audio.Complete();

        var result = await DoStream(model, audio: audio);

        var partsTask = Collect(result!);
        var ws = MockWebSocket.Instances[0];
        ws.Open();
        await Wait.UntilAsync(() => ws.Sent.Count >= 4);

        Assert.Equal(new byte[] { 1, 2, 3 }, (byte[])ws.Sent[1]);
        Assert.Equal(new byte[] { 4, 5, 6 }, (byte[])ws.Sent[2]);
        Assert.True(
            JsonNode.DeepEquals(
                Node("""{"type":"transcription-stream.audio-done"}"""),
                Parse(ws.Sent[3])
            )
        );

        ws.Message(Node(FinishFrame));
        await partsTask;
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should relay stream parts and close the stream on finish",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_relay_stream_parts_and_close_the_stream_on_finish()
    {
        var testDate = new DateTimeOffset(DateTime.UnixEpoch, TimeSpan.Zero);
        var model = CreateStreamingTestModel(currentDate: () => testDate);

        var result = await DoStream(model);

        var partsTask = Collect(result!);
        var ws = MockWebSocket.Instances[0];
        ws.Open();
        await Wait.FlushAsync();

        ws.Message(
            Node(
                """{"type":"stream-start","warnings":[{"type":"other","message":"test warning"}]}"""
            )
        );
        ws.Message(Node("""{"type":"transcript-delta","id":"seg-1","delta":"Hel"}"""));
        ws.Message(Node("""{"type":"transcript-partial","id":"seg-1","text":"Hel"}"""));
        ws.Message(Node("""{"type":"transcript-final","id":"seg-1","text":"Hello"}"""));
        ws.Message(Node("""{"type":"raw","rawValue":{"some":"chunk"}}"""));
        ws.Message(
            Node(
                """{"type":"finish","text":"Hello","segments":[{"text":"Hello","startSecond":0,"endSecond":1}],"language":"en"}"""
            )
        );

        var parts = await partsTask;
        Assert.Equal(
            new[]
            {
                "stream-start",
                "transcript-delta",
                "transcript-partial",
                "transcript-final",
                "raw",
                "finish",
            },
            parts.Select(part => part.Type)
        );
        var warning = Assert.Single(parts[0].Warnings!);
        Assert.Equal("other", warning.Type);
        Assert.Equal("test warning", warning.Message);
        Assert.Equal("seg-1", parts[1].Id);
        Assert.Equal("Hel", parts[1].Delta);
        Assert.Equal("seg-1", parts[2].Id);
        Assert.Equal("Hel", parts[2].Text);
        Assert.Equal("seg-1", parts[3].Id);
        Assert.Equal("Hello", parts[3].Text);
        Assert.Equal("""{"some":"chunk"}""", parts[4].Raw!.Value.GetRawText());
        Assert.Equal("Hello", parts[5].Text);
        var segment = Assert.Single(parts[5].Segments!);
        Assert.Equal("Hello", segment.Text);
        Assert.Equal(0, segment.StartSecond);
        Assert.Equal(1, segment.EndSecond);
        Assert.Equal("en", parts[5].Language);
        Assert.Contains(1000, ws.Closes);
        Assert.Equal(testDate.UtcDateTime, result!.Response!.Timestamp);
        Assert.Equal("openai/gpt-realtime-whisper", result.Response.ModelId);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should revive response-metadata timestamps to Date",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_revive_response_metadata_timestamps_to_Date()
    {
        var model = CreateStreamingTestModel();

        var result = await DoStream(model);

        var partsTask = Collect(result!);
        var ws = MockWebSocket.Instances[0];
        ws.Open();
        await Wait.FlushAsync();

        ws.Message(
            Node(
                """{"type":"response-metadata","timestamp":"2026-01-01T00:00:00.000Z","modelId":"openai/gpt-realtime-whisper"}"""
            )
        );
        ws.Message(Node(FinishFrame));

        var parts = await partsTask;
        Assert.Equal("response-metadata", parts[0].Type);
        Assert.Equal(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), parts[0].Timestamp);
        Assert.Equal("openai/gpt-realtime-whisper", parts[0].ModelId);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should ignore unknown server part types (forward compat)",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_ignore_unknown_server_part_types_forward_compat()
    {
        var model = CreateStreamingTestModel();

        var result = await DoStream(model);

        var partsTask = Collect(result!);
        var ws = MockWebSocket.Instances[0];
        ws.Open();
        await Wait.FlushAsync();

        ws.Message(Node("""{"type":"some-future-part","payload":42}"""));
        ws.Message(Node("""{"type":"transcript-delta","id":"seg-1","delta":"Hel"}"""));
        ws.Message(Node("""{"type":"finish","text":"Hel","segments":[]}"""));

        var parts = await partsTask;
        Assert.Equal(new[] { "transcript-delta", "finish" }, parts.Select(part => part.Type));
        Assert.Equal("Hel", parts[0].Delta);
        Assert.Equal("Hel", parts[1].Text);
        Assert.Empty(parts[1].Segments!);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should relay error parts and surface the server message when the socket closes without finish",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_relay_error_parts_and_surface_the_server_message_when_the_socket_closes_without_finish()
    {
        var model = CreateStreamingTestModel();

        var result = await DoStream(model);

        await using var reader = result!.Stream.ReadAsync().GetAsyncEnumerator();
        var ws = MockWebSocket.Instances[0];
        ws.Open();
        await Wait.FlushAsync();

        ws.Message(Node("""{"type":"error","error":{"message":"model overloaded"}}"""));
        Assert.True(await reader.MoveNextAsync());
        Assert.Equal("error", reader.Current.Type);
        Assert.Equal(
            """{"message":"model overloaded"}""",
            reader.Current.ErrorValue!.Value.GetRawText()
        );

        // the server terminates the socket after an error part
        ws.ServerClose(1011);
        var error = await Assert.ThrowsAnyAsync<GatewayError>(async () =>
            await reader.MoveNextAsync()
        );
        Assert.Contains("model overloaded", error.Message);
        Assert.NotEmpty(ws.Closes);
    }

    [Theory]
    [InlineData("authentication_error", typeof(GatewayAuthenticationError))]
    [InlineData("invalid_request_error", typeof(GatewayInvalidRequestError))]
    [InlineData("rate_limit_exceeded", typeof(GatewayRateLimitError))]
    [UpstreamTest(
        Prefix + "should map %s error parts to the public gateway error class",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_error_parts_to_the_public_gateway_error_class(
        string errorType,
        Type errorClass
    )
    {
        var model = CreateStreamingTestModel();

        var result = await DoStream(model);

        var partsTask = Collect(result!);
        var ws = MockWebSocket.Instances[0];
        ws.Open();
        await Wait.FlushAsync();

        ws.Message(
            Node(
                """{"type":"error","error":{"message":"request rejected","type":"__TYPE__"}}""".Replace(
                    "__TYPE__",
                    errorType,
                    StringComparison.Ordinal
                )
            )
        );
        await Wait.FlushAsync();
        ws.ServerClose(1011);

        var error = await Assert.ThrowsAnyAsync<GatewayError>(() => partsTask);
        Assert.IsType(errorClass, error);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should stop sending audio after a server error part while keeping the stream open until close",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stop_sending_audio_after_a_server_error_part_while_keeping_the_stream_open_until_close()
    {
        var model = CreateStreamingTestModel();
        var audioCancelled = false;
        var audio = new AudioChunkStream(_ => audioCancelled = true);
        audio.Write(new byte[] { 1, 2, 3 });

        var result = await DoStream(model, audio: audio);

        await using var reader = result!.Stream.ReadAsync().GetAsyncEnumerator();
        var ws = MockWebSocket.Instances[0];
        ws.Open();
        await Wait.FlushAsync();

        ws.Message(Node("""{"type":"error","error":{"message":"model overloaded"}}"""));
        Assert.True(await reader.MoveNextAsync());
        Assert.Equal("error", reader.Current.Type);
        Assert.Equal(
            """{"message":"model overloaded"}""",
            reader.Current.ErrorValue!.Value.GetRawText()
        );

        // outbound audio stops on the terminal error part
        await Wait.UntilAsync(() => audioCancelled);
        Assert.DoesNotContain(ws.Sent.OfType<string>(), frame =>
            frame.Contains("transcription-stream.audio-done")
        );

        ws.ServerClose(1011);
        var error = await Assert.ThrowsAnyAsync<GatewayError>(async () =>
            await reader.MoveNextAsync()
        );
        Assert.Contains("model overloaded", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should stringify non-object error part payloads in the terminal error",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stringify_non_object_error_part_payloads_in_the_terminal_error()
    {
        var model = CreateStreamingTestModel();

        var result = await DoStream(model);

        var partsTask = Collect(result!);
        var ws = MockWebSocket.Instances[0];
        ws.Open();
        await Wait.FlushAsync();

        ws.Message(Node("""{"type":"error","error":"model overloaded"}"""));
        await Wait.FlushAsync();
        ws.ServerClose(1011);

        var error = await Assert.ThrowsAnyAsync<GatewayError>(() => partsTask);
        Assert.Contains("model overloaded", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should JSON-stringify object error part payloads without a message in the terminal error",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_JSON_stringify_object_error_part_payloads_without_a_message_in_the_terminal_error()
    {
        var model = CreateStreamingTestModel();

        var result = await DoStream(model);

        var partsTask = Collect(result!);
        var ws = MockWebSocket.Instances[0];
        ws.Open();
        await Wait.FlushAsync();

        ws.Message(Node("""{"type":"error","error":{"code":"overloaded"}}"""));
        await Wait.FlushAsync();
        ws.ServerClose(1011);

        var error = await Assert.ThrowsAnyAsync<GatewayError>(() => partsTask);
        Assert.Contains("""{"code":"overloaded"}""", error.Message);
        Assert.DoesNotContain("[object Object]", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should error the stream with the generic message when the socket closes without finish or error part",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_error_the_stream_with_the_generic_message_when_the_socket_closes_without_finish_or_error_part()
    {
        var model = CreateStreamingTestModel();

        var result = await DoStream(model);

        var partsTask = Collect(result!);
        var ws = MockWebSocket.Instances[0];
        ws.Open();
        await Wait.FlushAsync();

        ws.ServerClose(1006);

        var error = await Assert.ThrowsAnyAsync<GatewayError>(() => partsTask);
        Assert.Contains(
            "AI Gateway transcription stream closed before a finish part was received",
            error.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should error the stream with a GatewayError on connection errors",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_error_the_stream_with_a_GatewayError_on_connection_errors()
    {
        var model = CreateStreamingTestModel();

        var result = await DoStream(model);

        var partsTask = Collect(result!);
        var ws = MockWebSocket.Instances[0];
        ws.OnError!(null);

        var error = await Assert.ThrowsAnyAsync<GatewayError>(() => partsTask);
        Assert.Contains("Connection error on AI Gateway transcription stream", error.Message);
        Assert.NotEmpty(ws.Closes);
    }

    [Theory]
    [InlineData("AbortError")]
    [InlineData("TimeoutError")]
    [UpstreamTest(
        Prefix + "should propagate an upstream audio %s unchanged",
        Coverage = UpstreamCoverage.Covered,
        Commit = "640569da18351d8a75fd20bcd8ff3d214ff0a617"
    )]
    public async Task Should_propagate_an_upstream_audio_error_unchanged(string name)
    {
        // The port has no DOMException: an abort is an OperationCanceledException and a timeout is a TimeoutError.
        Exception error =
            name == "AbortError"
                ? new OperationCanceledException("Audio stream cancelled")
                : new TimeoutError("Audio stream cancelled");
        var audio = new AudioChunkStream();
        var result = await CreateStreamingTestModel()
            .DoStreamAsync(
                new TranscriptionStreamCall(
                    audio,
                    new AudioFormat("audio/pcm", 16000),
                    default,
                    new Dictionary<string, string?>(),
                    null,
                    CancellationToken.None
                ),
                CancellationToken.None
            );
        var ws = MockWebSocket.Instances[0];
        ws.Open();
        await Wait.FlushAsync();

        var reading = Collect(result!);
        audio.Fail(error);

        Assert.Same(error, await Assert.ThrowsAnyAsync<Exception>(() => reading));
        Assert.Same(error, await Assert.ThrowsAnyAsync<Exception>(() => Collect(result!)));
        await Wait.FlushAsync();
        Assert.NotEmpty(ws.Closes);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should cancel the audio stream when the connection fails before open",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_cancel_the_audio_stream_when_the_connection_fails_before_open()
    {
        var model = CreateStreamingTestModel();
        var audioCancelled = false;
        var audio = new AudioChunkStream(_ => audioCancelled = true);

        var result = await DoStream(model, audio: audio);

        var partsTask = Collect(result!);
        var ws = MockWebSocket.Instances[0];

        // handshake failure: onerror fires without the socket ever opening
        ws.OnError!(null);
        await Wait.FlushAsync();

        await Assert.ThrowsAnyAsync<GatewayError>(() => partsTask);
        Assert.True(audioCancelled);
        Assert.NotEmpty(ws.Closes);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should close the WebSocket and stop reading audio on abort",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_close_the_WebSocket_and_stop_reading_audio_on_abort()
    {
        var model = CreateStreamingTestModel();
        using var abortController = new CancellationTokenSource();
        var audioCancelled = false;
        var audio = new AudioChunkStream(_ => audioCancelled = true);
        var abortReason = new InvalidOperationException("user aborted");

        var result = await DoStream(
            model,
            audio: audio,
            abortSignal: abortController.Token,
            abortReason: abortReason
        );

        var partsTask = Collect(result!);
        var ws = MockWebSocket.Instances[0];
        ws.Open();
        await Wait.FlushAsync();

        abortController.Cancel();
        await Wait.FlushAsync();

        Assert.Same(abortReason, await Assert.ThrowsAnyAsync<Exception>(() => partsTask));
        Assert.NotEmpty(ws.Closes);
        Assert.True(audioCancelled);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should close the WebSocket and stop reading audio when the stream is cancelled",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_close_the_WebSocket_and_stop_reading_audio_when_the_stream_is_cancelled()
    {
        var model = CreateStreamingTestModel();
        var audioCancelled = false;
        var audio = new AudioChunkStream(_ => audioCancelled = true);

        var result = await DoStream(model, audio: audio);

        var ws = MockWebSocket.Instances[0];
        ws.Open();
        await Wait.FlushAsync();

        result!.Stream.Cancel();
        await Wait.FlushAsync();

        Assert.NotEmpty(ws.Closes);
        Assert.True(audioCancelled);
    }

    private static string Base64Url(string value)
    {
        return Convert
            .ToBase64String(Encoding.UTF8.GetBytes(value))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
