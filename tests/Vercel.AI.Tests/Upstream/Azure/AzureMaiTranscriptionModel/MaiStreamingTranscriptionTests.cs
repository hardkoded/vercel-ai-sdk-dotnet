// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Tests.Upstream.ProviderUtils;
using static Vercel.AI.Tests.Upstream.Azure.AzureMaiTestSupport;

namespace Vercel.AI.Tests.Upstream.Azure.AzureMaiTranscriptionModel;

/// <summary>Port of <c>azure-mai-transcription-model.test.ts</c> &gt; <c>MAI streaming transcription</c>.</summary>
[Collection("MockWebSocket")]
public sealed class MaiStreamingTranscriptionTests
{
    private const string TestPrefix = "packages/azure/src/azure-mai-transcription-model.test.ts::MAI streaming transcription::";

    [Fact]
    [UpstreamTest(TestPrefix + "configures the session, streams audio, and maps transcript events", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Configures_the_session_streams_audio_and_maps_transcript_events()
    {
        var stream = await StartStream(providerOptions: "{\"azure\":{\"language\":\"en\"}}");
        var ws = stream.Socket;

        Assert.Equal("wss://test-resource.services.ai.azure.com/mai/v1/realtime?intent=transcription", ws.Url.AbsoluteUri);
        Assert.Equal("test-key", ws.Headers!["api-key"]);

        ws.Open();
        ws.Message(new JsonObject { ["type"] = "session.created", ["session"] = new JsonObject { ["id"] = "sess_1" } });
        await Wait.UntilAsync(() => ws.Sent.Count >= 1);
        await Wait.FlushAsync();
        Assert.Single(ws.Sent);
        var sessionUpdate = "{\"type\":\"session.update\",\"session\":{\"type\":\"transcription\",\"audio\":{\"input\":{\"format\":{\"type\":\"audio/pcm\",\"rate\":24000},"
            + "\"transcription\":{\"model\":\"mai-transcribe-2-streaming\",\"language\":\"en\"},\"turn_detection\":null,\"noise_reduction\":null}}}}";
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(sessionUpdate), JsonNode.Parse((string)ws.Sent[0])));
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(sessionUpdate), JsonNode.Parse(stream.Start.Request!.Body!)));

        ws.Message(new JsonObject { ["type"] = "session.updated", ["session"] = new JsonObject { ["id"] = "sess_1" } });
        await Wait.UntilAsync(() => ws.Sent.Count >= 3);
        Assert.Equal(new[] { "session.update", "input_audio_buffer.append", "input_audio_buffer.commit" }, stream.Types.ToArray());
        Assert.Equal(Convert.ToBase64String(Pcm), JsonNode.Parse((string)ws.Sent[1])!["audio"]!.GetValue<string>());

        ws.Message(new JsonObject { ["type"] = "input_audio_buffer.committed", ["item_id"] = "item_1" });
        ws.Message(Event(Transcription + ".delta", "item_1", delta: "Hello"));
        ws.Message(Event(Transcription + ".intermediate", "item_1", intermediate: " wor"));
        ws.Message(Event(Transcription + ".delta", "item_1", delta: " world."));
        ws.Message(Event(Transcription + ".completed", "item_1", transcript: "Hello world."));

        var parts = await stream.Parts;
        Assert.Equal(new[] { "stream-start", "transcript-delta", "transcript-partial", "transcript-delta", "transcript-final", "finish" }, parts.Select(part => part.Type).ToArray());
        Assert.Empty(parts[0].Warnings!);
        Assert.Equal(("item_1", "Hello"), (parts[1].Id, parts[1].Delta));
        Assert.Equal(("item_1", " wor"), (parts[2].Id, parts[2].Text));
        Assert.Equal(("item_1", " world."), (parts[3].Id, parts[3].Delta));
        Assert.Equal(("item_1", "Hello world."), (parts[4].Id, parts[4].Text));
        Assert.Equal("Hello world.", parts[5].Text);
        Assert.Empty(parts[5].Segments!);
        Assert.Equal("en", parts[5].Language);
        Assert.Equal(1, parts[5].DurationInSeconds);
        Assert.Contains(1000, ws.Closes);
    }

    [Fact]
    [UpstreamTest(TestPrefix + "does not send audio before the session update is acknowledged", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Does_not_send_audio_before_the_session_update_is_acknowledged()
    {
        var stream = await StartStream();
        var ws = stream.Socket;

        ws.Open();
        ws.Message(new JsonObject { ["type"] = "session.created" });
        await Wait.UntilAsync(() => ws.Sent.Count >= 1);
        await Wait.FlushAsync();
        await Wait.FlushAsync();

        Assert.Equal(new[] { "session.update" }, stream.Types.ToArray());
    }

    [Fact]
    [UpstreamTest(TestPrefix + "counts base64 audio chunks toward the duration at 16 kHz", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Counts_base64_audio_chunks_toward_the_duration_at_16_kHz()
    {
        var chunk = Convert.ToBase64String(new byte[16_000]);
        var stream = await StartStream(audio: new object[] { chunk, chunk }, rate: 16000);
        var ws = stream.Socket;

        await ConfigureSession(ws);

        var appends = ws.Sent.Select(sent => JsonNode.Parse((string)sent)!).Where(sent => sent["type"]!.GetValue<string>() == "input_audio_buffer.append").ToList();
        Assert.Equal(new[] { chunk, chunk }, appends.Select(append => append["audio"]!.GetValue<string>()).ToArray());
        ws.Message(Event(Transcription + ".completed", "i", transcript: "Hi"));
        var finish = (await stream.Parts).Last();
        Assert.Equal("finish", finish.Type);
        Assert.Equal("Hi", finish.Text);
        Assert.Equal(1, finish.DurationInSeconds);
    }

    [Fact]
    [UpstreamTest(TestPrefix + "falls back to accumulated deltas when the completed event has no transcript", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Falls_back_to_accumulated_deltas_when_the_completed_event_has_no_transcript()
    {
        var stream = await StartStream();
        var ws = stream.Socket;

        await ConfigureSession(ws);
        ws.Message(Event(Transcription + ".delta", "i", delta: "Hello"));
        ws.Message(Event(Transcription + ".completed", "i"));

        var finish = (await stream.Parts).Last();
        Assert.Equal("finish", finish.Type);
        Assert.Equal("Hello", finish.Text);
    }

    [Fact]
    [UpstreamTest(TestPrefix + "finishes without committing when there is no audio", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Finishes_without_committing_when_there_is_no_audio()
    {
        var stream = await StartStream(audio: Array.Empty<object>());
        var ws = stream.Socket;

        await ConfigureSession(ws);

        Assert.Equal(new[] { "session.update" }, stream.Types.ToArray());
        var finish = (await stream.Parts).Last();
        Assert.Equal("finish", finish.Type);
        Assert.Equal(string.Empty, finish.Text);
        Assert.Empty(finish.Segments!);
        Assert.Null(finish.Language);
        Assert.Null(finish.DurationInSeconds);
    }

    [Fact]
    [UpstreamTest(TestPrefix + "emits raw events when requested", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Emits_raw_events_when_requested()
    {
        var stream = await StartStream(includeRawChunks: true);
        var ws = stream.Socket;

        await ConfigureSession(ws);
        ws.Message(Event(Transcription + ".completed", "i", transcript: "Hi"));

        Assert.Equal(3, (await stream.Parts).Count(part => part.Type == "raw"));
    }

    [Fact]
    [UpstreamTest(TestPrefix + "fails the stream on transcription failures", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Fails_the_stream_on_transcription_failures()
    {
        var stream = await StartStream();
        var ws = stream.Socket;

        await ConfigureSession(ws);
        ws.Message(new JsonObject
        {
            ["type"] = Transcription + ".failed",
            ["item_id"] = "i",
            ["error"] = new JsonObject { ["type"] = "server_error", ["code"] = "DeploymentNotFound", ["message"] = "Input transcription failed." },
        });

        var error = await Assert.ThrowsAnyAsync<Exception>(() => stream.Parts);
        Assert.Equal("Azure MAI transcription error (DeploymentNotFound): Input transcription failed.", error.Message);
    }

    [Fact]
    [UpstreamTest(TestPrefix + "fails the stream on error events", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Fails_the_stream_on_error_events()
    {
        var stream = await StartStream();
        var ws = stream.Socket;

        ws.Open();
        ws.Message(new JsonObject
        {
            ["type"] = "error",
            ["error"] = new JsonObject
            {
                ["type"] = "invalid_request_error",
                ["code"] = "invalid_value",
                ["message"] = "PCM input rate must be 24000, or 16000 for MAI transcription.",
            },
        });

        var error = await Assert.ThrowsAnyAsync<Exception>(() => stream.Parts);
        Assert.Equal("Azure MAI transcription error (invalid_value): PCM input rate must be 24000, or 16000 for MAI transcription.", error.Message);
    }

    [Fact]
    [UpstreamTest(TestPrefix + "fails the stream when the connection closes before the transcript completes", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Fails_the_stream_when_the_connection_closes_before_the_transcript_completes()
    {
        var stream = await StartStream();
        var ws = stream.Socket;

        await ConfigureSession(ws);
        ws.ServerClose(1011, "server error");

        var error = await Assert.ThrowsAnyAsync<Exception>(() => stream.Parts);
        Assert.Equal("Azure MAI transcription connection closed before the transcript completed (code 1011: server error)", error.Message);
    }

    [Fact]
    [UpstreamTest(TestPrefix + "closes the socket when aborted", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Closes_the_socket_when_aborted()
    {
        using var cancellation = new CancellationTokenSource();
        var stream = await StartStream(cancellationToken: cancellation.Token);
        var ws = stream.Socket;

        await ConfigureSession(ws);
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => stream.Parts);
        Assert.NotEmpty(ws.Closes);
    }

    [Fact]
    [UpstreamTest(TestPrefix + "warns about file-transcription options", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Warns_about_file_transcription_options()
    {
        var stream = await StartStream(providerOptions: "{\"azure\":{\"timestamps\":\"word\"}}");
        var ws = stream.Socket;

        await ConfigureSession(ws);
        ws.Message(Event(Transcription + ".completed", "i", transcript: "Hi"));

        var start = (await stream.Parts)[0];
        Assert.Equal("stream-start", start.Type);
        var warning = Assert.Single(start.Warnings!);
        Assert.Equal(("unsupported", "providerOptions.azure.timestamps", "This option requires the Azure Speech API."), (warning.Type, warning.Feature, warning.Details));
    }

    [Theory]
    [InlineData("audio/pcmu", null, "got audio/pcmu")]
    [InlineData("audio/pcm", 8000, "got audio/pcm at 8000 Hz")]
    [UpstreamTest(TestPrefix + "rejects unsupported input audio %j", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Rejects_unsupported_input_audio(string type, int? rate, string detail)
    {
        MockWebSocket.Reset();
        var model = (ITranscriptionCaller)Setup().Transcription("mai-transcribe-2-streaming");
        var chunks = new AudioChunkStream();
        chunks.Write(Pcm);
        chunks.Complete();
        var call = new TranscriptionStreamCall(chunks, new AudioFormat(type, rate), JsonSerializer.Deserialize<JsonElement>("{}"), new Dictionary<string, string>(), null, CancellationToken.None);

        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() => model.DoStreamAsync(call, CancellationToken.None));

        Assert.Equal("inputAudioFormat", error.Parameter);
        Assert.Contains(detail, error.Message);
        Assert.Empty(MockWebSocket.Instances);
    }
}
