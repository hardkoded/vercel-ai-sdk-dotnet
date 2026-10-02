// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Interactions finish reasons and Gemini Live event mapping.</summary>
public sealed class GoogleInteractionsRealtimeUpstreamTests
{
    [Fact]
    [UpstreamTest("packages/google/src/interactions/map-google-interactions-finish-reason.test.ts::mapGoogleInteractionsFinishReason::maps \"completed\" without function_call to \"stop\"", Coverage = UpstreamCoverage.Covered)]
    public void Maps_completed_without_a_function_call_to_stop()
    {
        Assert.Equal(FinishReason.Stop, GoogleInteractionsFinish.Map("completed", false));
    }

    [Fact]
    [UpstreamTest("packages/google/src/interactions/map-google-interactions-finish-reason.test.ts::mapGoogleInteractionsFinishReason::maps \"completed\" with function_call to \"tool-calls\"", Coverage = UpstreamCoverage.Covered)]
    public void Maps_completed_with_a_function_call_to_tool_calls()
    {
        Assert.Equal(FinishReason.ToolCalls, GoogleInteractionsFinish.Map("completed", true));
    }

    [Fact]
    [UpstreamTest("packages/google/src/interactions/map-google-interactions-finish-reason.test.ts::mapGoogleInteractionsFinishReason::maps \"requires_action\" to \"tool-calls\"", Coverage = UpstreamCoverage.Covered)]
    public void Maps_requires_action_to_tool_calls()
    {
        Assert.Equal(FinishReason.ToolCalls, GoogleInteractionsFinish.Map("requires_action", false));
    }

    [Fact]
    [UpstreamTest("packages/google/src/interactions/map-google-interactions-finish-reason.test.ts::mapGoogleInteractionsFinishReason::maps \"failed\" to \"error\"", Coverage = UpstreamCoverage.Covered)]
    public void Maps_failed_to_error()
    {
        Assert.Equal(FinishReason.Error, GoogleInteractionsFinish.Map("failed", false));
    }

    [Fact]
    [UpstreamTest("packages/google/src/interactions/map-google-interactions-finish-reason.test.ts::mapGoogleInteractionsFinishReason::maps \"incomplete\" to \"length\"", Coverage = UpstreamCoverage.Covered)]
    public void Maps_incomplete_to_length()
    {
        Assert.Equal(FinishReason.Length, GoogleInteractionsFinish.Map("incomplete", false));
    }

    [Fact]
    [UpstreamTest("packages/google/src/interactions/map-google-interactions-finish-reason.test.ts::mapGoogleInteractionsFinishReason::maps \"cancelled\" to \"other\"", Coverage = UpstreamCoverage.Covered)]
    public void Maps_cancelled_to_other()
    {
        Assert.Equal(FinishReason.Other, GoogleInteractionsFinish.Map("cancelled", false));
    }

    [Fact]
    [UpstreamTest("packages/google/src/interactions/map-google-interactions-finish-reason.test.ts::mapGoogleInteractionsFinishReason::maps \"in_progress\" / unknown to \"other\"", Coverage = UpstreamCoverage.Covered)]
    public void Maps_in_progress_and_unknown_statuses_to_other()
    {
        Assert.Equal(FinishReason.Other, GoogleInteractionsFinish.Map("in_progress", false));
        Assert.Equal(FinishReason.Other, GoogleInteractionsFinish.Map("no-such-status", false));
        Assert.Equal(FinishReason.Other, GoogleInteractionsFinish.Map(null, false));
    }

    [Fact]
    [UpstreamTest("packages/google/src/interactions/convert-to-google-interactions-input.test.ts::convertToGoogleInteractionsInput > text-only prompts::emits a single-turn array of text content blocks", Coverage = UpstreamCoverage.Covered)]
    public void Emits_a_user_text_turn()
    {
        var body = GoogleInteractionsInput.Convert(new ModelMessage[] { new UserModelMessage("Hello, how are you?") });
        GoogleUpstream.JsonEqual(body["input"]!, "[{\"type\":\"user_input\",\"content\":[{\"type\":\"text\",\"text\":\"Hello, how are you?\"}]}]");
        Assert.False(body.ContainsKey("systemInstruction"));
    }

    [Fact]
    [UpstreamTest("packages/google/src/interactions/convert-to-google-interactions-input.test.ts::convertToGoogleInteractionsInput > text-only prompts::extracts system messages into systemInstruction", Coverage = UpstreamCoverage.Covered)]
    public void Extracts_system_messages_into_systemInstruction()
    {
        var body = GoogleInteractionsInput.Convert(new ModelMessage[] { new SystemModelMessage("You are a helpful assistant."), new UserModelMessage("Hi") });
        Assert.Equal("You are a helpful assistant.", (string?)body["systemInstruction"]);
    }

    [Fact]
    [UpstreamTest("packages/google/src/interactions/convert-to-google-interactions-input.test.ts::convertToGoogleInteractionsInput > assistant tool-call parts::emits a function_call content block from an assistant tool-call part with object input", Coverage = UpstreamCoverage.Covered)]
    public void Emits_a_function_call_block()
    {
        var body = GoogleInteractionsInput.Convert(new ModelMessage[]
        {
            new UserModelMessage("What's the weather in NYC?"),
            new AssistantModelMessage(null, new[] { new GeneratedToolCall("call_abc", "getWeather", "{\"location\":\"New York\"}") }, null),
        });
        GoogleUpstream.JsonEqual(body["input"]!, "[{\"type\":\"user_input\",\"content\":[{\"type\":\"text\",\"text\":\"What's the weather in NYC?\"}]},{\"type\":\"function_call\",\"name\":\"getWeather\",\"id\":\"call_abc\",\"arguments\":{\"location\":\"New York\"}}]");
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > parseServerEvent::maps setupComplete to session-created", Coverage = UpstreamCoverage.Covered)]
    public void Maps_setupComplete_to_session_created()
    {
        var raw = "{\"setupComplete\":true}";
        var evt = One(raw);
        Assert.Equal("session-created", evt.Type);
        GoogleUpstream.JsonEqual(evt.Raw!.Value, raw);
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > parseServerEvent::maps serverContent with audio to audio-delta", Coverage = UpstreamCoverage.Covered)]
    public void Maps_server_audio_to_an_audio_delta()
    {
        var raw = "{\"serverContent\":{\"modelTurn\":{\"parts\":[{\"inlineData\":{\"data\":\"base64audio\"}}]}}}";
        var evt = One(raw);
        Assert.Equal("audio-delta", evt.Type);
        Assert.Equal("google-resp-0", evt.ResponseId);
        Assert.Equal("google-item-0", evt.ItemId);
        Assert.Equal("base64audio", evt.Delta);
        GoogleUpstream.JsonEqual(evt.Raw!.Value, raw);
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > parseServerEvent::maps serverContent with text to text-delta", Coverage = UpstreamCoverage.Covered)]
    public void Maps_server_text_to_a_text_delta()
    {
        var raw = "{\"serverContent\":{\"modelTurn\":{\"parts\":[{\"text\":\"hello world\"}]}}}";
        var evt = One(raw);
        Assert.Equal("text-delta", evt.Type);
        Assert.Equal("hello world", evt.Delta);
        Assert.Equal("google-resp-0", evt.ResponseId);
        Assert.Equal("google-item-0", evt.ItemId);
        GoogleUpstream.JsonEqual(evt.Raw!.Value, raw);
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > parseServerEvent::maps serverContent with outputTranscription to audio-transcript-delta", Coverage = UpstreamCoverage.Covered)]
    public void Maps_output_transcription_to_an_audio_transcript_delta()
    {
        var raw = "{\"serverContent\":{\"outputTranscription\":{\"text\":\"transcribed text\"}}}";
        var evt = One(raw);
        Assert.Equal("audio-transcript-delta", evt.Type);
        Assert.Equal("transcribed text", evt.Delta);
        Assert.Equal("google-resp-0", evt.ResponseId);
        Assert.Equal("google-item-0", evt.ItemId);
        GoogleUpstream.JsonEqual(evt.Raw!.Value, raw);
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > parseServerEvent::maps serverContent with inputTranscription to input-transcription-completed", Coverage = UpstreamCoverage.Covered)]
    public void Maps_server_input_transcription()
    {
        var raw = "{\"serverContent\":{\"inputTranscription\":{\"text\":\"Can you hear me?\"}}}";
        var evt = One(raw);
        Assert.Equal("input-transcription-completed", evt.Type);
        Assert.Equal("google-input-0", evt.ItemId);
        Assert.Equal("Can you hear me?", evt.Transcript);
        Assert.Null(evt.ResponseId);
        GoogleUpstream.JsonEqual(evt.Raw!.Value, raw);
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > parseServerEvent::maps top-level inputTranscription to input-transcription-completed", Coverage = UpstreamCoverage.Covered)]
    public void Maps_top_level_input_transcription()
    {
        var raw = "{\"inputTranscription\":{\"text\":\"Can you hear me?\"}}";
        var evt = One(raw);
        Assert.Equal("input-transcription-completed", evt.Type);
        Assert.Equal("google-input-0", evt.ItemId);
        Assert.Equal("Can you hear me?", evt.Transcript);
        GoogleUpstream.JsonEqual(evt.Raw!.Value, raw);
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > parseServerEvent::maps serverContent with interrupted to speech-started", Coverage = UpstreamCoverage.Covered)]
    public void Maps_interrupted_to_speech_started()
    {
        var raw = "{\"serverContent\":{\"interrupted\":true}}";
        var evt = One(raw);
        Assert.Equal("speech-started", evt.Type);
        GoogleUpstream.JsonEqual(evt.Raw!.Value, raw);
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > parseServerEvent::maps serverContent with turnComplete after audio to done events + response-done", Coverage = UpstreamCoverage.Covered)]
    public void Emits_audio_done_and_response_done()
    {
        var mapper = new GoogleRealtime();
        mapper.ParseServerEvent(GoogleUpstream.Element("{\"serverContent\":{\"modelTurn\":{\"parts\":[{\"inlineData\":{\"data\":\"audio\"}}]}}}"));
        var raw = "{\"serverContent\":{\"turnComplete\":true}}";
        var events = mapper.ParseServerEvent(GoogleUpstream.Element(raw));
        Assert.Equal(new[] { "audio-done", "response-done" }, events.Select(item => item.Type).ToArray());
        Assert.Equal("google-resp-0", events[0].ResponseId);
        Assert.Equal("google-item-0", events[0].ItemId);
        Assert.Equal("google-resp-0", events[1].ResponseId);
        Assert.Equal("completed", events[1].Status);
        GoogleUpstream.JsonEqual(events[0].Raw!.Value, raw);
        GoogleUpstream.JsonEqual(events[1].Raw!.Value, raw);
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > parseServerEvent::maps serverContent with turnComplete after text to done events + response-done", Coverage = UpstreamCoverage.Covered)]
    public void Emits_text_done_and_response_done()
    {
        var mapper = new GoogleRealtime();
        mapper.ParseServerEvent(GoogleUpstream.Element("{\"serverContent\":{\"modelTurn\":{\"parts\":[{\"text\":\"hello\"}]}}}"));
        var raw = "{\"serverContent\":{\"turnComplete\":true}}";
        var events = mapper.ParseServerEvent(GoogleUpstream.Element(raw));
        Assert.Equal(new[] { "text-done", "response-done" }, events.Select(item => item.Type).ToArray());
        Assert.Equal("google-resp-0", events[0].ResponseId);
        Assert.Equal("google-item-0", events[0].ItemId);
        Assert.Equal("google-resp-0", events[1].ResponseId);
        Assert.Equal("completed", events[1].Status);
        GoogleUpstream.JsonEqual(events[1].Raw!.Value, raw);
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > parseServerEvent::increments IDs after turnComplete", Coverage = UpstreamCoverage.Covered)]
    public void Increments_response_ids_after_a_completed_turn()
    {
        var mapper = new GoogleRealtime();
        mapper.ParseServerEvent(GoogleUpstream.Element("{\"serverContent\":{\"modelTurn\":{\"parts\":[{\"inlineData\":{\"data\":\"audio1\"}}]}}}"));
        mapper.ParseServerEvent(GoogleUpstream.Element("{\"serverContent\":{\"turnComplete\":true}}"));
        var evt = mapper.ParseServerEvent(GoogleUpstream.Element("{\"serverContent\":{\"modelTurn\":{\"parts\":[{\"inlineData\":{\"data\":\"audio2\"}}]}}}")).Single();
        Assert.Equal("audio-delta", evt.Type);
        Assert.Equal("google-resp-1", evt.ResponseId);
        Assert.Equal("google-item-1", evt.ItemId);
        Assert.Equal("audio2", evt.Delta);
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > parseServerEvent::keeps a late transcript attached to the just-completed turn", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_a_late_transcript_on_the_completed_turn()
    {
        var mapper = new GoogleRealtime();
        mapper.ParseServerEvent(GoogleUpstream.Element("{\"serverContent\":{\"modelTurn\":{\"parts\":[{\"inlineData\":{\"data\":\"audio1\"}}]}}}"));
        mapper.ParseServerEvent(GoogleUpstream.Element("{\"serverContent\":{\"turnComplete\":true}}"));
        var late = mapper.ParseServerEvent(GoogleUpstream.Element("{\"serverContent\":{\"outputTranscription\":{\"text\":\"late transcript\"}}}")).Single();
        Assert.Equal("audio-transcript-delta", late.Type);
        Assert.Equal("google-resp-0", late.ResponseId);
        Assert.Equal("late transcript", late.Delta);
        var next = mapper.ParseServerEvent(GoogleUpstream.Element("{\"serverContent\":{\"modelTurn\":{\"parts\":[{\"inlineData\":{\"data\":\"audio2\"}}]}}}")).Single();
        Assert.Equal("google-resp-1", next.ResponseId);
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > parseServerEvent::maps multi-part serverContent to multiple events", Coverage = UpstreamCoverage.Covered)]
    public void Maps_audio_and_text_parts_to_separate_events()
    {
        var raw = "{\"serverContent\":{\"modelTurn\":{\"parts\":[{\"inlineData\":{\"data\":\"audio\"}},{\"text\":\"text\"}]}}}";
        var events = new GoogleRealtime().ParseServerEvent(GoogleUpstream.Element(raw));
        Assert.Equal(new[] { "audio-delta", "text-delta" }, events.Select(item => item.Type).ToArray());
        Assert.Equal("audio", events[0].Delta);
        Assert.Equal("text", events[1].Delta);
        Assert.Equal("google-resp-0", events[0].ResponseId);
        Assert.Equal("google-item-0", events[0].ItemId);
        Assert.Equal("google-resp-0", events[1].ResponseId);
        Assert.Equal("google-item-0", events[1].ItemId);
        GoogleUpstream.JsonEqual(events[0].Raw!.Value, raw);
        GoogleUpstream.JsonEqual(events[1].Raw!.Value, raw);
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > parseServerEvent::maps toolCall to function-call-arguments-delta and done events", Coverage = UpstreamCoverage.Covered)]
    public void Maps_a_tool_call_to_argument_delta_and_done_events()
    {
        var raw = "{\"toolCall\":{\"functionCalls\":[{\"id\":\"call_1\",\"name\":\"getWeather\",\"args\":{\"city\":\"NYC\"}},{\"id\":\"call_2\",\"name\":\"rollDice\",\"args\":{}}]}}";
        var events = new GoogleRealtime().ParseServerEvent(GoogleUpstream.Element(raw));
        Assert.Equal(4, events.Count);
        Assert.Equal("function-call-arguments-delta", events[0].Type);
        Assert.Equal("google-resp-0", events[0].ResponseId);
        Assert.Equal("google-item-0", events[0].ItemId);
        Assert.Equal("call_1", events[0].CallId);
        Assert.Equal("{\"city\":\"NYC\"}", events[0].Delta);
        Assert.Equal("function-call-arguments-done", events[1].Type);
        Assert.Equal("call_1", events[1].CallId);
        Assert.Equal("getWeather", events[1].Name);
        Assert.Equal("{\"city\":\"NYC\"}", events[1].Arguments);
        Assert.Equal("function-call-arguments-delta", events[2].Type);
        Assert.Equal("call_2", events[2].CallId);
        Assert.Equal("{}", events[2].Delta);
        Assert.Equal("function-call-arguments-done", events[3].Type);
        Assert.Equal("call_2", events[3].CallId);
        Assert.Equal("rollDice", events[3].Name);
        Assert.Equal("{}", events[3].Arguments);
        GoogleUpstream.JsonEqual(events[3].Raw!.Value, raw);
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > parseServerEvent::maps toolCallCancellation to custom event", Coverage = UpstreamCoverage.Covered)]
    public void Maps_tool_call_cancellation_to_a_custom_event()
    {
        var raw = "{\"toolCallCancellation\":{\"ids\":[\"call_1\"]}}";
        var evt = One(raw);
        Assert.Equal("custom", evt.Type);
        Assert.Equal("toolCallCancellation", evt.RawType);
        GoogleUpstream.JsonEqual(evt.Raw!.Value, raw);
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > parseServerEvent::maps goAway to a stable custom lifecycle event", Coverage = UpstreamCoverage.Covered)]
    public void Maps_goAway_to_a_custom_event()
    {
        var raw = "{\"goAway\":{\"timeLeft\":\"30s\"}}";
        var evt = One(raw);
        Assert.Equal("custom", evt.Type);
        Assert.Equal("goAway", evt.RawType);
        GoogleUpstream.JsonEqual(evt.Raw!.Value, raw);
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > parseServerEvent::maps sessionResumptionUpdate to a stable custom lifecycle event", Coverage = UpstreamCoverage.Covered)]
    public void Maps_session_resumption_to_a_custom_event()
    {
        var raw = "{\"sessionResumptionUpdate\":{\"newHandle\":\"resume-handle\",\"resumable\":true,\"lastConsumedClientMessageIndex\":\"42\"}}";
        var evt = One(raw);
        Assert.Equal("custom", evt.Type);
        Assert.Equal("sessionResumptionUpdate", evt.RawType);
        GoogleUpstream.JsonEqual(evt.Raw!.Value, raw);
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > parseServerEvent::surfaces interactionStatus as a custom event", Coverage = UpstreamCoverage.Covered)]
    public void Surfaces_interaction_status()
    {
        var raw = "{\"serverContent\":{\"interactionStatus\":\"IN_PROGRESS\"}}";
        var evt = One(raw);
        Assert.Equal("custom", evt.Type);
        Assert.Equal("interactionStatus", evt.RawType);
        GoogleUpstream.JsonEqual(evt.Raw!.Value, raw);
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > parseServerEvent::emits interactionStatus alongside turnComplete events", Coverage = UpstreamCoverage.Covered)]
    public void Emits_interaction_status_with_turn_completion()
    {
        var mapper = new GoogleRealtime();
        mapper.ParseServerEvent(GoogleUpstream.Element("{\"serverContent\":{\"modelTurn\":{\"parts\":[{\"inlineData\":{\"data\":\"AAAA\"}}]}}}"));
        var raw = "{\"serverContent\":{\"turnComplete\":true,\"interactionStatus\":\"IDLE\"}}";
        var events = mapper.ParseServerEvent(GoogleUpstream.Element(raw));
        var status = events.Single(item => item.Type == "custom" && item.RawType == "interactionStatus");
        var done = events.Single(item => item.Type == "response-done");
        Assert.Equal("completed", done.Status);
        Assert.Equal("google-resp-0", done.ResponseId);
        GoogleUpstream.JsonEqual(status.Raw!.Value, raw);
        GoogleUpstream.JsonEqual(done.Raw!.Value, raw);
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > parseServerEvent::surfaces waitingForInput as a custom event", Coverage = UpstreamCoverage.Covered)]
    public void Surfaces_waiting_for_input()
    {
        var raw = "{\"serverContent\":{\"waitingForInput\":true}}";
        var evt = One(raw);
        Assert.Equal("custom", evt.Type);
        Assert.Equal("waitingForInput", evt.RawType);
        GoogleUpstream.JsonEqual(evt.Raw!.Value, raw);
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > parseServerEvent::keeps generationComplete distinct from turnComplete", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_generation_complete_on_the_same_turn()
    {
        var mapper = new GoogleRealtime();
        var raw = "{\"serverContent\":{\"generationComplete\":true}}";
        var complete = mapper.ParseServerEvent(GoogleUpstream.Element(raw)).Single();
        Assert.Equal("custom", complete.Type);
        Assert.Equal("generationComplete", complete.RawType);
        GoogleUpstream.JsonEqual(complete.Raw!.Value, raw);
        var next = mapper.ParseServerEvent(GoogleUpstream.Element("{\"serverContent\":{\"modelTurn\":{\"parts\":[{\"text\":\"still turn zero\"}]}}}")).Single();
        Assert.Equal("text-delta", next.Type);
        Assert.Equal("google-resp-0", next.ResponseId);
        Assert.Equal("still turn zero", next.Delta);
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > parseServerEvent::maps unrecognized top-level key to custom event", Coverage = UpstreamCoverage.Covered)]
    public void Maps_an_unrecognized_event_to_custom()
    {
        var raw = "{\"somethingNew\":{\"data\":123}}";
        var evt = One(raw);
        Assert.Equal("custom", evt.Type);
        Assert.Equal("somethingNew", evt.RawType);
        GoogleUpstream.JsonEqual(evt.Raw!.Value, raw);
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > serializeClientEvent::serializes session-update as setup message", Coverage = UpstreamCoverage.Covered)]
    public void Serializes_a_session_update_as_setup()
    {
        var message = new GoogleRealtime().SerializeClient("session-update", new GoogleRealtimeSession { ModelId = "gemini-2.0-flash-live-001" }, null, null, null, null, null);
        GoogleUpstream.JsonEqual(message, "{\"setup\":{\"model\":\"models/gemini-2.0-flash-live-001\",\"generationConfig\":{\"responseModalities\":[\"AUDIO\"]}}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > serializeClientEvent::serializes session-update with normalized session config", Coverage = UpstreamCoverage.Covered)]
    public void Serializes_instructions_voice_modalities_and_tools()
    {
        var session = new GoogleRealtimeSession
        {
            ModelId = "gemini-2.0-flash-live-001",
            Instructions = "Be helpful",
            Voice = "Puck",
            OutputModalities = new[] { "audio", "text" },
            Tools = new[] { new GoogleRealtimeTool("getWeather", "Get weather", GoogleUpstream.Element("{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}}}")) },
        };
        var message = new GoogleRealtime().SerializeClient("session-update", session, null, null, null, null, null);
        GoogleUpstream.JsonEqual(message, "{\"setup\":{\"model\":\"models/gemini-2.0-flash-live-001\",\"generationConfig\":{\"responseModalities\":[\"AUDIO\",\"TEXT\"],\"speechConfig\":{\"voiceConfig\":{\"prebuiltVoiceConfig\":{\"voiceName\":\"Puck\"}}}},\"systemInstruction\":{\"parts\":[{\"text\":\"Be helpful\"}]},\"tools\":[{\"functionDeclarations\":[{\"name\":\"getWeather\",\"description\":\"Get weather\",\"parametersJsonSchema\":{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}}}}]}]}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > serializeClientEvent::serializes live translation config into generationConfig", Coverage = UpstreamCoverage.Covered)]
    public void Serializes_live_translation_config()
    {
        var session = new GoogleRealtimeSession
        {
            ModelId = "gemini-3.5-live-translate-preview",
            InputAudioTranscription = true,
            OutputAudioTranscription = true,
            ProviderOptions = GoogleUpstream.Element("{\"google\":{\"translationConfig\":{\"targetLanguageCode\":\"pl\",\"echoTargetLanguage\":true}}}"),
        };
        var message = new GoogleRealtime().SerializeClient("session-update", session, null, null, null, null, null);
        GoogleUpstream.JsonEqual(message, "{\"setup\":{\"model\":\"models/gemini-3.5-live-translate-preview\",\"generationConfig\":{\"responseModalities\":[\"AUDIO\"],\"translationConfig\":{\"targetLanguageCode\":\"pl\",\"echoTargetLanguage\":true}},\"inputAudioTranscription\":{},\"outputAudioTranscription\":{}}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > serializeClientEvent::serializes input-audio-append as realtimeInput", Coverage = UpstreamCoverage.Covered)]
    public void Serializes_input_audio_append()
    {
        var message = new GoogleRealtime().SerializeClient("input-audio-append", null, "base64data", null, null, null, null);
        GoogleUpstream.JsonEqual(message, "{\"realtimeInput\":{\"audio\":{\"data\":\"base64data\",\"mimeType\":\"audio/pcm;rate=16000\"}}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > serializeClientEvent::serializes input-audio-commit as audioStreamEnd", Coverage = UpstreamCoverage.Covered)]
    public void Serializes_input_audio_commit()
    {
        var message = new GoogleRealtime().SerializeClient("input-audio-commit", null, null, null, null, null, null);
        GoogleUpstream.JsonEqual(message, "{\"realtimeInput\":{\"audioStreamEnd\":true}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > serializeClientEvent::returns null for input-audio-clear", Coverage = UpstreamCoverage.Covered)]
    public void Returns_null_for_input_audio_clear()
    {
        Assert.Null(new GoogleRealtime().SerializeClient("input-audio-clear", null, null, null, null, null, null));
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > serializeClientEvent::returns null for response-create", Coverage = UpstreamCoverage.Covered)]
    public void Returns_null_for_response_create()
    {
        Assert.Null(new GoogleRealtime().SerializeClient("response-create", null, null, null, null, null, null));
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > serializeClientEvent::returns null for response-cancel", Coverage = UpstreamCoverage.Covered)]
    public void Returns_null_for_response_cancel()
    {
        Assert.Null(new GoogleRealtime().SerializeClient("response-cancel", null, null, null, null, null, null));
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > serializeClientEvent::serializes text message as realtimeInput", Coverage = UpstreamCoverage.Covered)]
    public void Serializes_a_text_message()
    {
        var message = new GoogleRealtime().SerializeClient("conversation-item-create", null, null, "hello", null, null, null);
        GoogleUpstream.JsonEqual(message, "{\"realtimeInput\":{\"text\":\"hello\"}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > serializeClientEvent::serializes function-call-output as toolResponse", Coverage = UpstreamCoverage.Covered)]
    public void Serializes_an_object_function_response()
    {
        var message = new GoogleRealtime().SerializeClient("conversation-item-create", null, null, null, "call_1", "getWeather", "{\"temp\":72}");
        GoogleUpstream.JsonEqual(message, "{\"toolResponse\":{\"functionResponses\":[{\"id\":\"call_1\",\"name\":\"getWeather\",\"response\":{\"temp\":72}}]}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > serializeClientEvent::wraps %s tool result so the response stays an object", Coverage = UpstreamCoverage.Covered)]
    public void Wraps_non_object_tool_results()
    {
        foreach (var pair in new[] { ("\"SEARCH_UNAVAILABLE\"", "{\"output\":\"SEARCH_UNAVAILABLE\"}"), ("42", "{\"output\":42}"), ("[\"a\",\"b\"]", "{\"output\":[\"a\",\"b\"]}"), ("null", "{\"output\":null}"), ("false", "{\"output\":false}") })
        {
            var message = new GoogleRealtime().SerializeClient("conversation-item-create", null, null, null, "call_1", "getWeather", pair.Item1);
            GoogleUpstream.JsonEqual(message, "{\"toolResponse\":{\"functionResponses\":[{\"id\":\"call_1\",\"name\":\"getWeather\",\"response\":" + pair.Item2 + "}]}}");
        }
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > serializeClientEvent::keeps unparseable function-call-output as text instead of an empty object", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_unparseable_tool_output_as_text()
    {
        var message = new GoogleRealtime().SerializeClient("conversation-item-create", null, null, null, "call_1", "getWeather", "{");
        GoogleUpstream.JsonEqual(message, "{\"toolResponse\":{\"functionResponses\":[{\"id\":\"call_1\",\"name\":\"getWeather\",\"response\":{\"output\":\"{\"}}]}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > serializeClientEvent::returns null for conversation-item-truncate", Coverage = UpstreamCoverage.Covered)]
    public void Returns_null_for_conversation_item_truncate()
    {
        Assert.Null(new GoogleRealtime().SerializeClient("conversation-item-truncate", null, null, null, null, null, null));
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::buildGoogleSessionConfig::builds config with model path", Coverage = UpstreamCoverage.Covered)]
    public void Builds_a_session_model_path()
    {
        var setup = GoogleRealtime.BuildSession(new GoogleRealtimeSession { ModelId = "gemini-2.0-flash" });
        GoogleUpstream.JsonEqual(setup, "{\"model\":\"models/gemini-2.0-flash\",\"generationConfig\":{\"responseModalities\":[\"AUDIO\"]}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::buildGoogleSessionConfig::builds config with instructions and voice", Coverage = UpstreamCoverage.Covered)]
    public void Builds_config_with_instructions_and_voice()
    {
        var setup = GoogleRealtime.BuildSession(new GoogleRealtimeSession { ModelId = "gemini-2.0-flash", Instructions = "Be helpful", Voice = "Puck" });
        GoogleUpstream.JsonEqual(setup, "{\"model\":\"models/gemini-2.0-flash\",\"generationConfig\":{\"responseModalities\":[\"AUDIO\"],\"speechConfig\":{\"voiceConfig\":{\"prebuiltVoiceConfig\":{\"voiceName\":\"Puck\"}}}},\"systemInstruction\":{\"parts\":[{\"text\":\"Be helpful\"}]}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::buildGoogleSessionConfig::builds config with tools", Coverage = UpstreamCoverage.Covered)]
    public void Builds_config_with_tools()
    {
        var setup = GoogleRealtime.BuildSession(new GoogleRealtimeSession
        {
            ModelId = "gemini-2.0-flash",
            Tools = new[] { new GoogleRealtimeTool("getWeather", "Get weather", GoogleUpstream.Element("{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}},\"required\":[\"city\"]}")) },
        });
        GoogleUpstream.JsonEqual(setup["tools"]!, "[{\"functionDeclarations\":[{\"name\":\"getWeather\",\"description\":\"Get weather\",\"parametersJsonSchema\":{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}},\"required\":[\"city\"]}}]}]");
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::buildGoogleSessionConfig::builds config with preserved local JSON Schema references", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_local_json_schema_references()
    {
        var setup = GoogleRealtime.BuildSession(new GoogleRealtimeSession
        {
            ModelId = "gemini-2.0-flash",
            Tools = new[] { new GoogleRealtimeTool("formatDate", "Format a date", GoogleUpstream.Element("{\"type\":\"object\",\"properties\":{\"locale\":{\"$ref\":\"#/$defs/Locale\"}},\"required\":[\"locale\"],\"$defs\":{\"Locale\":{\"type\":\"string\",\"enum\":[\"de\",\"en\"]}}}")) },
        });
        GoogleUpstream.JsonEqual(setup["tools"]!, "[{\"functionDeclarations\":[{\"name\":\"formatDate\",\"description\":\"Format a date\",\"parametersJsonSchema\":{\"type\":\"object\",\"properties\":{\"locale\":{\"$ref\":\"#/$defs/Locale\"}},\"required\":[\"locale\"],\"$defs\":{\"Locale\":{\"type\":\"string\",\"enum\":[\"de\",\"en\"]}}}}]}]");
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::buildGoogleSessionConfig::preserves model path that already includes slash", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_a_model_path_that_already_includes_a_slash()
    {
        var setup = GoogleRealtime.BuildSession(new GoogleRealtimeSession { ModelId = "models/gemini-2.0-flash" });
        Assert.Equal("models/gemini-2.0-flash", (string?)setup["model"]);
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::buildGoogleSessionConfig::defaults thinkingLevel to low on background-reasoning Live models", Coverage = UpstreamCoverage.Covered)]
    public void Defaults_thinking_level_on_background_reasoning_live_models()
    {
        foreach (var modelId in new[] { "gemini-3.8-live-extended-thinking", "models/gemini-3.8-live-extended-thinking" })
        {
            GoogleUpstream.JsonEqual(GoogleRealtime.BuildSession(new GoogleRealtimeSession { ModelId = modelId }), "{\"model\":\"models/gemini-3.8-live-extended-thinking\",\"generationConfig\":{\"responseModalities\":[\"AUDIO\"],\"thinkingConfig\":{\"thinkingLevel\":\"low\"}}}");
        }

        var withInstructions = GoogleRealtime.BuildSession(new GoogleRealtimeSession
        {
            ModelId = "gemini-3.8-live-extended-thinking",
            OutputModalities = new[] { "audio" },
            Instructions = "Be brief.",
        });
        GoogleUpstream.JsonEqual(withInstructions["generationConfig"]!, "{\"responseModalities\":[\"AUDIO\"],\"thinkingConfig\":{\"thinkingLevel\":\"low\"}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::buildGoogleSessionConfig::does not default thinkingConfig on Live models without background reasoning", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_default_thinking_config_without_background_reasoning()
    {
        foreach (var modelId in new[] { "gemini-3.8-live", "gemini-3.1-flash-live-preview", "gemini-3.5-live-translate-preview" })
        {
            var setup = GoogleRealtime.BuildSession(new GoogleRealtimeSession { ModelId = modelId, OutputModalities = new[] { "audio" } });
            GoogleUpstream.JsonEqual(setup["generationConfig"]!, "{\"responseModalities\":[\"AUDIO\"]}");
        }
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::buildGoogleSessionConfig::does not add a default thinkingLevel when thinkingBudget is set", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_add_a_thinking_level_when_a_budget_is_set()
    {
        var setup = GoogleRealtime.BuildSession(new GoogleRealtimeSession
        {
            ModelId = "gemini-3.8-live-extended-thinking",
            ProviderOptions = GoogleUpstream.Element("{\"google\":{\"thinkingConfig\":{\"thinkingBudget\":256}}}"),
        });
        GoogleUpstream.JsonEqual(setup["generationConfig"]!, "{\"responseModalities\":[\"AUDIO\"],\"thinkingConfig\":{\"thinkingBudget\":256}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::buildGoogleSessionConfig::maps output modalities to uppercase", Coverage = UpstreamCoverage.Covered)]
    public void Maps_output_modalities_to_uppercase()
    {
        var setup = GoogleRealtime.BuildSession(new GoogleRealtimeSession { ModelId = "model", OutputModalities = new[] { "audio", "text" } });
        GoogleUpstream.JsonEqual(setup["generationConfig"]!["responseModalities"]!, "[\"AUDIO\",\"TEXT\"]");
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::buildGoogleSessionConfig::enables input audio transcription", Coverage = UpstreamCoverage.Covered)]
    public void Enables_input_audio_transcription()
    {
        var setup = GoogleRealtime.BuildSession(new GoogleRealtimeSession { ModelId = "model", InputAudioTranscription = true });
        GoogleUpstream.JsonEqual(setup["inputAudioTranscription"]!, "{}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::buildGoogleSessionConfig::enables output audio transcription", Coverage = UpstreamCoverage.Covered)]
    public void Enables_output_audio_transcription()
    {
        var setup = GoogleRealtime.BuildSession(new GoogleRealtimeSession { ModelId = "model", OutputAudioTranscription = true });
        GoogleUpstream.JsonEqual(setup["outputAudioTranscription"]!, "{}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::buildGoogleSessionConfig::maps providerOptions.google.translationConfig to generationConfig", Coverage = UpstreamCoverage.Covered)]
    public void Maps_translation_config_onto_generationConfig()
    {
        var setup = GoogleRealtime.BuildSession(new GoogleRealtimeSession
        {
            ModelId = "gemini-3.5-live-translate-preview",
            ProviderOptions = GoogleUpstream.Element("{\"google\":{\"translationConfig\":{\"targetLanguageCode\":\"es\",\"echoTargetLanguage\":true}}}"),
        });
        GoogleUpstream.JsonEqual(setup, "{\"model\":\"models/gemini-3.5-live-translate-preview\",\"generationConfig\":{\"responseModalities\":[\"AUDIO\"],\"translationConfig\":{\"targetLanguageCode\":\"es\",\"echoTargetLanguage\":true}}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::buildGoogleSessionConfig::merges translation config into raw generationConfig provider options", Coverage = UpstreamCoverage.Covered)]
    public void Merges_translation_config_into_raw_generationConfig()
    {
        var setup = GoogleRealtime.BuildSession(new GoogleRealtimeSession
        {
            ModelId = "gemini-3.5-live-translate-preview",
            ProviderOptions = GoogleUpstream.Element("{\"generationConfig\":{\"responseModalities\":[\"AUDIO\"],\"temperature\":0.2},\"google\":{\"translationConfig\":{\"targetLanguageCode\":\"fr\"}}}"),
        });
        GoogleUpstream.JsonEqual(setup["generationConfig"]!, "{\"responseModalities\":[\"AUDIO\"],\"temperature\":0.2,\"translationConfig\":{\"targetLanguageCode\":\"fr\"}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::buildGoogleSessionConfig::maps providerOptions.google.thinkingConfig to generationConfig", Coverage = UpstreamCoverage.Covered)]
    public void Maps_thinking_config_onto_generationConfig()
    {
        var setup = GoogleRealtime.BuildSession(new GoogleRealtimeSession
        {
            ModelId = "gemini-3.8-live-extended-thinking",
            ProviderOptions = GoogleUpstream.Element("{\"google\":{\"thinkingConfig\":{\"thinkingLevel\":\"high\",\"includeThoughts\":true}}}"),
        });
        GoogleUpstream.JsonEqual(setup, "{\"model\":\"models/gemini-3.8-live-extended-thinking\",\"generationConfig\":{\"responseModalities\":[\"AUDIO\"],\"thinkingConfig\":{\"thinkingLevel\":\"high\",\"includeThoughts\":true}}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::buildGoogleSessionConfig::keeps the default thinkingLevel under a raw generationConfig provider option", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_the_default_thinking_level_under_raw_generationConfig()
    {
        var setup = GoogleRealtime.BuildSession(new GoogleRealtimeSession
        {
            ModelId = "gemini-3.8-live-extended-thinking",
            ProviderOptions = GoogleUpstream.Element("{\"generationConfig\":{\"temperature\":0.2}}"),
        });
        GoogleUpstream.JsonEqual(setup["generationConfig"]!, "{\"temperature\":0.2,\"thinkingConfig\":{\"thinkingLevel\":\"low\"}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::buildGoogleSessionConfig::merges thinking config into raw generationConfig provider options", Coverage = UpstreamCoverage.Covered)]
    public void Merges_thinking_config_into_raw_generationConfig()
    {
        var setup = GoogleRealtime.BuildSession(new GoogleRealtimeSession
        {
            ModelId = "gemini-3.8-live-extended-thinking",
            ProviderOptions = GoogleUpstream.Element("{\"generationConfig\":{\"responseModalities\":[\"AUDIO\"],\"temperature\":0.2},\"google\":{\"thinkingConfig\":{\"thinkingBudget\":512}}}"),
        });
        GoogleUpstream.JsonEqual(setup["generationConfig"]!, "{\"responseModalities\":[\"AUDIO\"],\"temperature\":0.2,\"thinkingConfig\":{\"thinkingBudget\":512}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::buildGoogleSessionConfig::merges translation and thinking config together into generationConfig", Coverage = UpstreamCoverage.Covered)]
    public void Merges_translation_and_thinking_config_together()
    {
        var setup = GoogleRealtime.BuildSession(new GoogleRealtimeSession
        {
            ModelId = "gemini-3.8-live-extended-thinking",
            ProviderOptions = GoogleUpstream.Element("{\"google\":{\"translationConfig\":{\"targetLanguageCode\":\"fr\"},\"thinkingConfig\":{\"thinkingBudget\":1024}}}"),
        });
        GoogleUpstream.JsonEqual(setup["generationConfig"]!, "{\"responseModalities\":[\"AUDIO\"],\"translationConfig\":{\"targetLanguageCode\":\"fr\"},\"thinkingConfig\":{\"thinkingBudget\":1024}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::buildGoogleSessionConfig::stamps defaultToolBehavior onto every function declaration", Coverage = UpstreamCoverage.Covered)]
    public void Stamps_default_tool_behavior_onto_function_declarations()
    {
        var setup = GoogleRealtime.BuildSession(new GoogleRealtimeSession
        {
            ModelId = "gemini-3.8-live",
            Tools = new[]
            {
                new GoogleRealtimeTool("getWeather", "Get weather", GoogleUpstream.Element("{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}},\"required\":[\"city\"]}")),
                new GoogleRealtimeTool("getTime", null, GoogleUpstream.Element("{\"type\":\"object\",\"properties\":{}}")),
            },
            ProviderOptions = GoogleUpstream.Element("{\"google\":{\"defaultToolBehavior\":\"BLOCKING\"}}"),
        });
        var declarations = setup["tools"]![0]!["functionDeclarations"]!.AsArray();
        Assert.Equal("BLOCKING", (string?)declarations[0]!["behavior"]);
        Assert.Equal("BLOCKING", (string?)declarations[1]!["behavior"]);
    }

    [Fact]
    [UpstreamTest("packages/google/src/realtime/google-realtime-event-mapper.test.ts::buildGoogleSessionConfig::omits behavior from function declarations when defaultToolBehavior is unset", Coverage = UpstreamCoverage.Covered)]
    public void Omits_function_behavior_when_it_is_unset()
    {
        var setup = GoogleRealtime.BuildSession(new GoogleRealtimeSession
        {
            ModelId = "gemini-3.8-live",
            Tools = new[] { new GoogleRealtimeTool("getWeather", null, GoogleUpstream.Element("{\"type\":\"object\",\"properties\":{}}")) },
        });
        Assert.False(setup["tools"]![0]!["functionDeclarations"]![0]!.AsObject().ContainsKey("behavior"));
    }

    private static GoogleRealtimeEvent One(string json)
    {
        return new GoogleRealtime().ParseServerEvent(GoogleUpstream.Element(json)).Single();
    }
}
