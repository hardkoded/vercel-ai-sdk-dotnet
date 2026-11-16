// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Mapper = global::Vercel.AI.Google.GoogleRealtimeEventMapper;

namespace Vercel.AI.Tests.Upstream.Google.GoogleRealtimeEventMapper;

/// <summary>Port of <c>google-realtime-event-mapper.test.ts</c> &gt; <c>GoogleRealtimeEventMapper &gt; serializeClientEvent</c>.</summary>
public sealed class SerializeClientEventTests
{
    private const string Id =
        "packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > serializeClientEvent::";

    private readonly Mapper _mapper = new();

    public static TheoryData<string, string, string> ToolResults
    {
        get
        {
            return new TheoryData<string, string, string>
            {
                { "a string", "\"SEARCH_UNAVAILABLE\"", """{"output":"SEARCH_UNAVAILABLE"}""" },
                { "a number", "42", """{"output":42}""" },
                { "an array", """["a","b"]""", """{"output":["a","b"]}""" },
                { "null", "null", """{"output":null}""" },
                { "a boolean", "false", """{"output":false}""" },
            };
        }
    }

    [Fact]
    [UpstreamTest(
        Id + "serializes session-update as setup message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Serializes_session_update_as_setup_message()
    {
        var result = _mapper.SerializeClientEvent(
            new RealtimeModelV4ClientEvent("session-update")
            {
                Config = new RealtimeModelV4SessionConfig(),
            },
            "gemini-2.0-flash-live-001"
        );

        GoogleUpstream.JsonEqual(
            result,
            """{"setup":{"model":"models/gemini-2.0-flash-live-001","generationConfig":{"responseModalities":["AUDIO"]}}}"""
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "serializes session-update with normalized session config",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Serializes_session_update_with_normalized_session_config()
    {
        var result = _mapper.SerializeClientEvent(
            new RealtimeModelV4ClientEvent("session-update")
            {
                Config = new RealtimeModelV4SessionConfig
                {
                    Instructions = "Be helpful",
                    Voice = "Puck",
                    OutputModalities = ["audio", "text"],
                    InputAudioFormatType = "audio/pcm",
                    InputAudioFormatRate = 24000,
                    Tools =
                    [
                        new RealtimeModelV4ToolDefinition(
                            "getWeather",
                            "Get weather",
                            GoogleUpstream.Element(
                                """{"type":"object","properties":{"city":{"type":"string"}}}"""
                            )
                        ),
                    ],
                },
            },
            "gemini-2.0-flash-live-001"
        );

        GoogleUpstream.JsonEqual(
            result,
            """
            {"setup":{"model":"models/gemini-2.0-flash-live-001","systemInstruction":{"parts":[{"text":"Be helpful"}]},
            "generationConfig":{"responseModalities":["AUDIO","TEXT"],"speechConfig":{"voiceConfig":{"prebuiltVoiceConfig":{"voiceName":"Puck"}}}},
            "tools":[{"functionDeclarations":[{"name":"getWeather","description":"Get weather","parametersJsonSchema":{"type":"object","properties":{"city":{"type":"string"}}}}]}]}}
            """
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "serializes live translation config into generationConfig",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Serializes_live_translation_config_into_generationConfig()
    {
        var result = _mapper.SerializeClientEvent(
            new RealtimeModelV4ClientEvent("session-update")
            {
                Config = new RealtimeModelV4SessionConfig
                {
                    InputAudioTranscription = new RealtimeModelV4AudioTranscription(),
                    OutputAudioTranscription = new RealtimeModelV4AudioTranscription(),
                    ProviderOptions = System
                        .Text.Json.Nodes.JsonNode.Parse(
                            """{"google":{"translationConfig":{"targetLanguageCode":"pl","echoTargetLanguage":true}}}"""
                        )!
                        .AsObject(),
                },
            },
            "gemini-3.5-live-translate-preview"
        );

        GoogleUpstream.JsonEqual(
            result,
            """
            {"setup":{"model":"models/gemini-3.5-live-translate-preview",
            "generationConfig":{"responseModalities":["AUDIO"],"translationConfig":{"targetLanguageCode":"pl","echoTargetLanguage":true}},
            "inputAudioTranscription":{},"outputAudioTranscription":{}}}
            """
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "serializes input-audio-append as realtimeInput",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Serializes_input_audio_append_as_realtimeInput()
    {
        var result = _mapper.SerializeClientEvent(
            new RealtimeModelV4ClientEvent("input-audio-append") { Audio = "base64data" },
            "model"
        );

        GoogleUpstream.JsonEqual(
            result,
            """{"realtimeInput":{"audio":{"data":"base64data","mimeType":"audio/pcm;rate=16000"}}}"""
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "serializes input-audio-commit as audioStreamEnd",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Serializes_input_audio_commit_as_audioStreamEnd()
    {
        GoogleUpstream.JsonEqual(
            _mapper.SerializeClientEvent(
                new RealtimeModelV4ClientEvent("input-audio-commit"),
                "model"
            ),
            """{"realtimeInput":{"audioStreamEnd":true}}"""
        );
    }

    [Fact]
    [UpstreamTest(Id + "returns null for input-audio-clear", Coverage = UpstreamCoverage.Covered)]
    public void Returns_null_for_input_audio_clear()
    {
        Assert.Null(
            _mapper.SerializeClientEvent(
                new RealtimeModelV4ClientEvent("input-audio-clear"),
                "model"
            )
        );
    }

    [Fact]
    [UpstreamTest(Id + "returns null for response-create", Coverage = UpstreamCoverage.Covered)]
    public void Returns_null_for_response_create()
    {
        Assert.Null(
            _mapper.SerializeClientEvent(new RealtimeModelV4ClientEvent("response-create"), "model")
        );
    }

    [Fact]
    [UpstreamTest(Id + "returns null for response-cancel", Coverage = UpstreamCoverage.Covered)]
    public void Returns_null_for_response_cancel()
    {
        Assert.Null(
            _mapper.SerializeClientEvent(new RealtimeModelV4ClientEvent("response-cancel"), "model")
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "serializes text message as realtimeInput",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Serializes_text_message_as_realtimeInput()
    {
        var result = _mapper.SerializeClientEvent(
            new RealtimeModelV4ClientEvent("conversation-item-create")
            {
                Item = new RealtimeModelV4ConversationItem("text-message")
                {
                    Role = "user",
                    Text = "hello",
                },
            },
            "model"
        );

        GoogleUpstream.JsonEqual(result, """{"realtimeInput":{"text":"hello"}}""");
    }

    [Fact]
    [UpstreamTest(
        Id + "serializes function-call-output as toolResponse",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Serializes_function_call_output_as_toolResponse()
    {
        var result = _mapper.SerializeClientEvent(FunctionCallOutput("{\"temp\":72}"), "model");

        GoogleUpstream.JsonEqual(
            result,
            """{"toolResponse":{"functionResponses":[{"id":"call_1","name":"getWeather","response":{"temp":72}}]}}"""
        );
    }

    [Theory]
    [MemberData(nameof(ToolResults))]
    [UpstreamTest(
        Id + "wraps %s tool result so the response stays an object",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Wraps_tool_result_so_the_response_stays_an_object(
        string label,
        string output,
        string expected
    )
    {
        Assert.NotEmpty(label);
        var result = _mapper.SerializeClientEvent(FunctionCallOutput(output), "model");

        GoogleUpstream.JsonEqual(
            result,
            "{\"toolResponse\":{\"functionResponses\":[{\"id\":\"call_1\",\"name\":\"getWeather\",\"response\":"
                + expected
                + "}]}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "keeps unparseable function-call-output as text instead of an empty object",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Keeps_unparseable_function_call_output_as_text_instead_of_an_empty_object()
    {
        var result = _mapper.SerializeClientEvent(FunctionCallOutput("{"), "model");

        GoogleUpstream.JsonEqual(
            result,
            """{"toolResponse":{"functionResponses":[{"id":"call_1","name":"getWeather","response":{"output":"{"}}]}}"""
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "returns null for conversation-item-truncate",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Returns_null_for_conversation_item_truncate()
    {
        Assert.Null(
            _mapper.SerializeClientEvent(
                new RealtimeModelV4ClientEvent("conversation-item-truncate")
                {
                    ItemId = "item_1",
                    ContentIndex = 0,
                    AudioEndMs = 1000,
                },
                "model"
            )
        );
    }

    private static RealtimeModelV4ClientEvent FunctionCallOutput(string output)
    {
        return new RealtimeModelV4ClientEvent("conversation-item-create")
        {
            Item = new RealtimeModelV4ConversationItem("function-call-output")
            {
                CallId = "call_1",
                Name = "getWeather",
                Output = output,
            },
        };
    }
}
