// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.ProviderUtils;
using Vercel.AI.Util;
using static Vercel.AI.Tests.ElevenLabsSpeechStream.SpeechStreamHarness;

namespace Vercel.AI.Tests.ElevenLabsSpeechStream;

public sealed class ElevenLabsSpeechModelDoStreamDialogueTests
{
    private const string Prefix =
        "packages/elevenlabs/src/elevenlabs-speech-stream.test.ts::ElevenLabsSpeechModel.doStream (dialogue)::";

    private const string Commit = "9874c89b12bd7d546b5ecc521bdf74128e8ab0a9";

    private readonly SpeechStreamHarness _harness = new SpeechStreamHarness();

    private static SpeechTextPart Part(string text, string? voice = null, bool newTurn = false) =>
        new SpeechTextPart(
            text,
            voice,
            newTurn
                ? JsonDocument.Parse("""{"elevenlabs":{"newTurn":true}}""").RootElement.Clone()
                : null
        );

    [Fact]
    [UpstreamTest(
        Prefix + "streams speaker turns from text part voices over Text-to-Dialogue (v4)",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Streams_speaker_turns_from_text_part_voices_over_Text_to_Dialogue_v4()
    {
        var result = await DoStream(
            _harness.CreateModel("eleven_v4"),
            new TrackedText([
                Part("Hi there. ", "a"),
                Part("Hello 👋 ", "b"),
                Part("friend.", "b"),
            ]),
            outputFormat: new SpeechAudioFormat("audio/pcm", 24000),
            providerOptions: """
            {"elevenlabs":{"dialogue":{"voices":["a","b"],"voiceSettings":{"stability":0.5}},
            "streaming":{"syncAlignment":true,"inactivityTimeout":60}}}
            """
        );
        var socket = _harness.LastSocket;
        Assert.Equal(
            "wss://api.elevenlabs.io/v1/text-to-dialogue/stream-input?model_id=eleven_v4&output_format=pcm_24000&sync_alignment=true",
            socket.Url.ToString()
        );
        socket.Open();
        await Wait.UntilAsync(() => socket.Sent.Count >= 5);
        AssertJson(
            """
            [
              {"voice_settings":{"stability":0.5},"voices":["a","b"],"xi_api_key":"test-api-key"},
              {"inputs":[{"text":"Hi there. ","voice_id":"a"}]},
              {"inputs":[{"text":"Hello 👋 ","voice_id":"b","new_turn":true}]},
              {"inputs":[{"text":"friend.","voice_id":"b"}]},
              {"close_socket":true}
            ]
            """,
            SentMessages(socket)
        );

        socket.Message(
            JsonNode.Parse(
                """{"audio":"AQI=","alignment":{"chars":["H"],"char_start_times_ms":[0],"char_durations_ms":[5]}}"""
            )!
        );
        socket.Message(JsonNode.Parse("""{"is_final_audio_for_turn":true}""")!);
        socket.Message(JsonNode.Parse("""{"is_final":true}""")!);

        var parts = await ReadAllAsync(result.Stream);
        // 'Hi there. ' (10) + 'Hello 👋 ' (8) + 'friend.' (7)
        Assert.Equal(4, parts.Count);
        Assert.Equal(
            "providerOptions.elevenlabs.streaming.inactivityTimeout",
            parts[0].Warnings!.Single().Feature
        );
        AssertJson(
            """
            [
              {"type":"audio","audio":"AQI=","audioFormat":{"type":"audio/pcm","rate":24000},
               "providerMetadata":{"elevenlabs":{"alignment":{"chars":["H"],"charStartTimesMs":[0],"charDurationsMs":[5]}}}},
              {"type":"audio","audio":"","audioFormat":{"type":"audio/pcm","rate":24000},
               "providerMetadata":{"elevenlabs":{"isFinalAudioForTurn":true}}},
              {"type":"finish","usage":{"inputCharacters":25}}
            ]
            """,
            parts.Skip(1).Select(Describe)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "flushes held text when the speaker changes, and honors newTurn",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Flushes_held_text_when_the_speaker_changes_and_honors_newTurn()
    {
        await DoStream(
            _harness.CreateModel("eleven_v4"),
            new TrackedText([
                Part("Hi the", "a"),
                Part("Hello "),
                Part("Again. ", newTurn: true),
                Part("More."),
            ]),
            voice: "b",
            providerOptions: """{"elevenlabs":{"dialogue":{"voices":["a","b"]}}}"""
        );
        var socket = _harness.LastSocket;
        socket.Open();
        await Wait.UntilAsync(() => socket.Sent.Count >= 7);
        AssertJson(
            """
            [
              {"inputs":[{"text":"Hi ","voice_id":"a"}]},
              {"inputs":[{"text":"the","voice_id":"a"}]},
              {"inputs":[{"text":"Hello ","voice_id":"b","new_turn":true}]},
              {"inputs":[{"text":"Again. ","voice_id":"b","new_turn":true}]},
              {"inputs":[{"text":"More.","voice_id":"b"}]},
              {"close_socket":true}
            ]
            """,
            SentMessages(socket).Skip(1)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "does not split held text when a part names the default voice explicitly",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Does_not_split_held_text_when_a_part_names_the_default_voice_explicitly()
    {
        await DoStream(
            _harness.CreateModel("eleven_v4"),
            new TrackedText([Part("Hel"), Part("lo there.", "a")]),
            providerOptions: """{"elevenlabs":{"dialogue":{"voices":["a","b"]}}}"""
        );
        var socket = _harness.LastSocket;
        socket.Open();
        await Wait.UntilAsync(() => socket.Sent.Count >= 4);
        AssertJson(
            """
            [
              {"inputs":[{"text":"Hello ","voice_id":"a"}]},
              {"inputs":[{"text":"there.","voice_id":"a"}]},
              {"close_socket":true}
            ]
            """,
            SentMessages(socket).Skip(1)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "keeps a newTurn whose text is entirely held for the next text",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Keeps_a_newTurn_whose_text_is_entirely_held_for_the_next_text()
    {
        await DoStream(
            _harness.CreateModel("eleven_v4"),
            new TrackedText([Part("Hi. "), Part("Ag", newTurn: true), Part("ain. ")]),
            providerOptions: """{"elevenlabs":{"dialogue":{"voices":["a"]}}}"""
        );
        var socket = _harness.LastSocket;
        socket.Open();
        await Wait.UntilAsync(() => socket.Sent.Count >= 4);
        AssertJson(
            """
            [
              {"inputs":[{"text":"Hi. ","voice_id":"a"}]},
              {"inputs":[{"text":"Again. ","voice_id":"a","new_turn":true}]},
              {"close_socket":true}
            ]
            """,
            SentMessages(socket).Skip(1)
        );
    }

    [Theory]
    [InlineData("a held part", "Hel")]
    [InlineData("an empty part", "")]
    [UpstreamTest(
        Prefix + "errors on an unlisted voice in %s without waiting for more text",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Errors_on_an_unlisted_voice_in_name_without_waiting_for_more_text(
        string name,
        string text
    )
    {
        Assert.NotNull(name);
        // never closes: the error must not wait for a flush
        var result = await DoStream(
            _harness.CreateModel("eleven_v4"),
            new TrackedText([new SpeechTextPart(text, "c")], holdOpen: true),
            providerOptions: """{"elevenlabs":{"dialogue":{"voices":["a"]}}}"""
        );
        _harness.LastSocket.Open();
        var error = await Assert.ThrowsAsync<InvalidArgumentError>(() =>
            ReadAllAsync(result.Stream)
        );
        Assert.Contains("is not listed in", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "warns about options Text-to-Dialogue ignores",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Warns_about_options_Text_to_Dialogue_ignores()
    {
        var result = await DoStream(
            _harness.CreateModel("eleven_v4"),
            TextParts("Hi."),
            speed: 1.2,
            providerOptions: """
            {"elevenlabs":{"dialogue":{"voices":["a"]},"voiceSettings":{"stability":0.3},
            "streaming":{"autoMode":true,"chunkLengthSchedule":[50]}}}
            """
        );
        var socket = _harness.LastSocket;
        socket.Open();
        await Wait.UntilAsync(() => socket.Sent.Count >= 1);
        socket.Message(JsonNode.Parse("""{"is_final":true}""")!);
        var start = (await ReadAllAsync(result.Stream))[0];
        Assert.Equal("stream-start", start.Type);
        Assert.Equal(
            "speed,providerOptions.elevenlabs.voiceSettings,providerOptions.elevenlabs.streaming.autoMode,providerOptions.elevenlabs.streaming.chunkLengthSchedule",
            string.Join(",", start.Warnings!.Select(warning => warning.Feature))
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "errors when a text part voice is not a dialogue voice",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Errors_when_a_text_part_voice_is_not_a_dialogue_voice()
    {
        var result = await DoStream(
            _harness.CreateModel("eleven_v4"),
            new TrackedText([Part("Hi ", "c")]),
            providerOptions: """{"elevenlabs":{"dialogue":{"voices":["a","b"]}}}"""
        );
        _harness.LastSocket.Open();
        var error = await Assert.ThrowsAsync<InvalidArgumentError>(() =>
            ReadAllAsync(result.Stream)
        );
        Assert.Contains(
            "Dialogue text part voice \"c\" is not listed in providerOptions.elevenlabs.dialogue.voices.",
            error.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "rejects a call voice that is not a dialogue voice before connecting",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Rejects_a_call_voice_that_is_not_a_dialogue_voice_before_connecting()
    {
        var socketsBefore = _harness.SocketCount;
        await Assert.ThrowsAsync<InvalidArgumentError>(() =>
            DoStream(
                _harness.CreateModel("eleven_v4"),
                TextParts("Hi"),
                voice: "c",
                providerOptions: """{"elevenlabs":{"dialogue":{"voices":["a"]}}}"""
            )
        );
        Assert.Equal(socketsBefore, _harness.SocketCount);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "keeps alive even when inactivityTimeout is set",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit,
        Note = "The keepalive interval is shortened with ElevenLabsOptions.KeepaliveIntervalMs instead of fake timers."
    )]
    public async Task Keeps_alive_even_when_inactivityTimeout_is_set()
    {
        await DoStream(
            _harness.CreateModel("eleven_v4", keepaliveIntervalMs: 100),
            new TrackedText([], holdOpen: true),
            providerOptions: """
            {"elevenlabs":{"dialogue":{"voices":["a"]},"streaming":{"inactivityTimeout":60}}}
            """
        );
        var socket = _harness.LastSocket;
        socket.Open();
        await Wait.UntilAsync(() => socket.Sent.Count >= 2);
        AssertJson("""[{"keep_alive":true}]""", SentMessages(socket).Skip(1).Take(1));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "sends the whole script in one message for the v3 family",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Sends_the_whole_script_in_one_message_for_the_v3_family()
    {
        await DoStream(
            _harness.CreateModel("eleven_v3_conversational"),
            TextParts("Hello ", "world."),
            providerOptions: """{"elevenlabs":{"dialogue":{"voices":["a"]}}}"""
        );
        var socket = _harness.LastSocket;
        socket.Open();
        await Wait.UntilAsync(() => socket.Sent.Count >= 2);
        await Wait.FlushAsync();
        AssertJson(
            """
            [
              {"voices":["a"],"xi_api_key":"test-api-key"},
              {"inputs":[{"text":"Hello ","voice_id":"a"},{"text":"world.","voice_id":"a"}],"close_socket":true}
            ]
            """,
            SentMessages(socket)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "rejects more than one voice for eleven_v3_conversational before connecting",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Rejects_more_than_one_voice_for_eleven_v3_conversational_before_connecting()
    {
        var text = new TrackedText([], holdOpen: true);
        var socketsBefore = _harness.SocketCount;
        await Assert.ThrowsAsync<InvalidArgumentError>(() =>
            DoStream(
                _harness.CreateModel("eleven_v3_conversational"),
                text,
                providerOptions: """{"elevenlabs":{"dialogue":{"voices":["a","b"]}}}"""
            )
        );
        Assert.Equal(socketsBefore, _harness.SocketCount);
        Assert.True(text.Disposed);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "errors on server error frames",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Errors_on_server_error_frames()
    {
        var result = await DoStream(
            _harness.CreateModel("eleven_v4"),
            TextParts("Hi"),
            providerOptions: """{"elevenlabs":{"dialogue":{"voices":["a","b"]}}}"""
        );
        var socket = _harness.LastSocket;
        socket.Open();
        await Wait.UntilAsync(() => socket.Sent.Count >= 1);
        socket.Message(
            JsonNode.Parse(
                """
                {"message":"eleven_v3_conversational only supports a single voice.",
                 "error":"invalid_argument","code":1008}
                """
            )!
        );
        var error = await Assert.ThrowsAsync<AiSdkException>(() => ReadAllAsync(result.Stream));
        Assert.Contains(
            "ElevenLabs speech stream error: eleven_v3_conversational only supports a single voice.",
            error.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "rejects dialogue on non-dialogue models",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Rejects_dialogue_on_non_dialogue_models()
    {
        await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() =>
            DoStream(
                _harness.CreateModel("eleven_flash_v2_5"),
                TextParts("Hi"),
                providerOptions: """{"elevenlabs":{"dialogue":{"voices":["a"]}}}"""
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "sends keep_alive while a buffered v3 script waits for more text",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit,
        Note = "The keepalive interval is shortened with ElevenLabsOptions.KeepaliveIntervalMs instead of fake timers."
    )]
    public async Task Sends_keep_alive_while_a_buffered_v3_script_waits_for_more_text()
    {
        var text = new TrackedText([new SpeechTextPart("Hello there. ")], holdOpen: true);
        await DoStream(
            _harness.CreateModel("eleven_v3", keepaliveIntervalMs: 100),
            text,
            providerOptions: """{"elevenlabs":{"dialogue":{"voices":["a"]}}}"""
        );
        var socket = _harness.LastSocket;
        socket.Open();
        await Wait.UntilAsync(() => socket.Sent.Count >= 2);
        AssertJson("""[{"keep_alive":true}]""", SentMessages(socket).Skip(1).Take(1));

        text.Release();
        await Wait.UntilAsync(() =>
            socket.Sent.Count > 0
            && SentMessages(socket).Last()?["close_socket"]?.GetValue<bool>() == true
        );
        AssertJson(
            """{"inputs":[{"text":"Hello there. ","voice_id":"a"}],"close_socket":true}""",
            SentMessages(socket).Last()
        );
    }
}
