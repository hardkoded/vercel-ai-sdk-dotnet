// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Mapper = global::Vercel.AI.Google.GoogleRealtimeEventMapper;

namespace Vercel.AI.Tests.Upstream.Google.GoogleRealtimeEventMapper;

/// <summary>Port of <c>google-realtime-event-mapper.test.ts</c> &gt; <c>GoogleRealtimeEventMapper &gt; parseServerEvent</c>.</summary>
public sealed class ParseServerEventTests
{
    private const string Id =
        "packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > parseServerEvent::";

    [Fact]
    [UpstreamTest(
        Id + "maps setupComplete to session-created",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_setupComplete_to_session_created()
    {
        var mapper = new Mapper();
        var raw = """{"setupComplete":true}""";
        var result = mapper.ParseOne(raw);

        GoogleRealtimeTestSupport.AssertEvent(result, raw, "session-created");
    }

    [Fact]
    [UpstreamTest(
        Id + "maps serverContent with audio to audio-delta",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_serverContent_with_audio_to_audio_delta()
    {
        var mapper = new Mapper();
        var raw =
            """{"serverContent":{"modelTurn":{"parts":[{"inlineData":{"data":"base64audio"}}]}}}""";
        var result = mapper.ParseOne(raw);

        GoogleRealtimeTestSupport.AssertEvent(
            result,
            raw,
            "audio-delta",
            responseId: "google-resp-0",
            itemId: "google-item-0",
            delta: "base64audio"
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "maps serverContent with text to text-delta",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_serverContent_with_text_to_text_delta()
    {
        var mapper = new Mapper();
        var raw = """{"serverContent":{"modelTurn":{"parts":[{"text":"hello world"}]}}}""";
        var result = mapper.ParseOne(raw);

        GoogleRealtimeTestSupport.AssertEvent(
            result,
            raw,
            "text-delta",
            responseId: "google-resp-0",
            itemId: "google-item-0",
            delta: "hello world"
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "maps serverContent with outputTranscription to audio-transcript-delta",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_serverContent_with_outputTranscription_to_audio_transcript_delta()
    {
        var mapper = new Mapper();
        var raw = """{"serverContent":{"outputTranscription":{"text":"transcribed text"}}}""";
        var result = mapper.ParseOne(raw);

        GoogleRealtimeTestSupport.AssertEvent(
            result,
            raw,
            "audio-transcript-delta",
            responseId: "google-resp-0",
            itemId: "google-item-0",
            delta: "transcribed text"
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "maps serverContent with inputTranscription to input-transcription-completed",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_serverContent_with_inputTranscription_to_input_transcription_completed()
    {
        var mapper = new Mapper();
        var raw = """{"serverContent":{"inputTranscription":{"text":"Can you hear me?"}}}""";
        var result = mapper.ParseOne(raw);

        GoogleRealtimeTestSupport.AssertEvent(
            result,
            raw,
            "input-transcription-completed",
            itemId: "google-input-0",
            transcript: "Can you hear me?"
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "maps top-level inputTranscription to input-transcription-completed",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_top_level_inputTranscription_to_input_transcription_completed()
    {
        var mapper = new Mapper();
        var raw = """{"inputTranscription":{"text":"Can you hear me?"}}""";
        var result = mapper.ParseOne(raw);

        GoogleRealtimeTestSupport.AssertEvent(
            result,
            raw,
            "input-transcription-completed",
            itemId: "google-input-0",
            transcript: "Can you hear me?"
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "increments top-level input transcription IDs after turnComplete",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Increments_top_level_input_transcription_IDs_after_turnComplete()
    {
        var mapper = new Mapper();

        mapper.Parse("""{"inputTranscription":{"text":"What time is it?"}}""");
        mapper.Parse(
            """{"serverContent":{"modelTurn":{"parts":[{"inlineData":{"data":"audio1"}}]}}}"""
        );
        mapper.Parse("""{"serverContent":{"turnComplete":true}}""");

        var raw = """{"inputTranscription":{"text":"And the date?"}}""";

        GoogleRealtimeTestSupport.AssertEvent(
            mapper.ParseOne(raw),
            raw,
            "input-transcription-completed",
            itemId: "google-input-1",
            transcript: "And the date?"
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "maps serverContent with interrupted to speech-started",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_serverContent_with_interrupted_to_speech_started()
    {
        var mapper = new Mapper();
        var raw = """{"serverContent":{"interrupted":true}}""";
        var result = mapper.ParseOne(raw);

        GoogleRealtimeTestSupport.AssertEvent(result, raw, "speech-started");
    }

    [Fact]
    [UpstreamTest(
        Id + "maps serverContent with turnComplete after audio to done events + response-done",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_serverContent_with_turnComplete_after_audio_to_done_events_and_response_done()
    {
        var mapper = new Mapper();

        mapper.Parse(
            """{"serverContent":{"modelTurn":{"parts":[{"inlineData":{"data":"audio"}}]}}}"""
        );

        var raw = """{"serverContent":{"turnComplete":true}}""";
        var result = mapper.Parse(raw);

        Assert.Collection(
            result,
            e =>
                GoogleRealtimeTestSupport.AssertEvent(
                    e,
                    raw,
                    "audio-done",
                    responseId: "google-resp-0",
                    itemId: "google-item-0"
                ),
            e =>
                GoogleRealtimeTestSupport.AssertEvent(
                    e,
                    raw,
                    "response-done",
                    responseId: "google-resp-0",
                    status: "completed"
                )
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "maps serverContent with turnComplete after text to done events + response-done",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_serverContent_with_turnComplete_after_text_to_done_events_and_response_done()
    {
        var mapper = new Mapper();

        mapper.Parse("""{"serverContent":{"modelTurn":{"parts":[{"text":"hello"}]}}}""");

        var raw = """{"serverContent":{"turnComplete":true}}""";
        var result = mapper.Parse(raw);

        Assert.Collection(
            result,
            e =>
                GoogleRealtimeTestSupport.AssertEvent(
                    e,
                    raw,
                    "text-done",
                    responseId: "google-resp-0",
                    itemId: "google-item-0"
                ),
            e =>
                GoogleRealtimeTestSupport.AssertEvent(
                    e,
                    raw,
                    "response-done",
                    responseId: "google-resp-0",
                    status: "completed"
                )
        );
    }

    [Fact]
    [UpstreamTest(Id + "increments IDs after turnComplete", Coverage = UpstreamCoverage.Covered)]
    public void Increments_IDs_after_turnComplete()
    {
        var mapper = new Mapper();

        mapper.Parse(
            """{"serverContent":{"modelTurn":{"parts":[{"inlineData":{"data":"audio1"}}]}}}"""
        );

        mapper.Parse("""{"serverContent":{"turnComplete":true}}""");

        var raw =
            """{"serverContent":{"modelTurn":{"parts":[{"inlineData":{"data":"audio2"}}]}}}""";
        var result = mapper.ParseOne(raw);

        GoogleRealtimeTestSupport.AssertEvent(
            result,
            raw,
            "audio-delta",
            responseId: "google-resp-1",
            itemId: "google-item-1",
            delta: "audio2"
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "increments input transcription IDs after turnComplete",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Increments_input_transcription_IDs_after_turnComplete()
    {
        var mapper = new Mapper();

        mapper.Parse("""{"serverContent":{"inputTranscription":{"text":"What time is it?"}}}""");
        mapper.Parse(
            """{"serverContent":{"modelTurn":{"parts":[{"inlineData":{"data":"audio1"}}]}}}"""
        );
        mapper.Parse("""{"serverContent":{"turnComplete":true}}""");

        var raw = """{"serverContent":{"inputTranscription":{"text":"And the date?"}}}""";

        GoogleRealtimeTestSupport.AssertEvent(
            mapper.ParseOne(raw),
            raw,
            "input-transcription-completed",
            itemId: "google-input-1",
            transcript: "And the date?"
        );
        var next = mapper.ParseOne(
            """{"serverContent":{"modelTurn":{"parts":[{"inlineData":{"data":"audio2"}}]}}}"""
        );
        Assert.Equal("google-resp-1", next.ResponseId);
        Assert.Equal("google-item-1", next.ItemId);
    }

    [Fact]
    [UpstreamTest(
        Id + "keeps the interrupted response active until its trailing turnComplete",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Keeps_the_interrupted_response_active_until_its_trailing_turnComplete()
    {
        var mapper = new Mapper();

        mapper.Parse("""{"serverContent":{"inputTranscription":{"text":"Tell me a story."}}}""");
        mapper.Parse(
            """{"serverContent":{"modelTurn":{"parts":[{"inlineData":{"data":"audio1"}}]}}}"""
        );
        mapper.Parse("""{"serverContent":{"interrupted":true}}""");

        var raw = """{"serverContent":{"inputTranscription":{"text":"Stop."}}}""";

        GoogleRealtimeTestSupport.AssertEvent(
            mapper.ParseOne(raw),
            raw,
            "input-transcription-completed",
            itemId: "google-input-1",
            transcript: "Stop."
        );

        var turnCompleteRaw = """{"serverContent":{"turnComplete":true}}""";
        Assert.Collection(
            mapper.Parse(turnCompleteRaw),
            e =>
                GoogleRealtimeTestSupport.AssertEvent(
                    e,
                    turnCompleteRaw,
                    "audio-done",
                    responseId: "google-resp-0",
                    itemId: "google-item-0"
                ),
            e =>
                GoogleRealtimeTestSupport.AssertEvent(
                    e,
                    turnCompleteRaw,
                    "response-done",
                    responseId: "google-resp-0",
                    status: "completed"
                )
        );

        var next = mapper.ParseOne(
            """{"serverContent":{"modelTurn":{"parts":[{"inlineData":{"data":"audio2"}}]}}}"""
        );
        Assert.Equal("google-resp-1", next.ResponseId);
        Assert.Equal("google-item-1", next.ItemId);
    }

    [Fact]
    [UpstreamTest(
        Id + "keeps an interrupting utterance together across the trailing turnComplete",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Keeps_an_interrupting_utterance_together_across_the_trailing_turnComplete()
    {
        var mapper = new Mapper();

        mapper.Parse("""{"serverContent":{"inputTranscription":{"text":"Tell me a story."}}}""");
        mapper.Parse(
            """{"serverContent":{"modelTurn":{"parts":[{"inlineData":{"data":"audio1"}}]}}}"""
        );
        mapper.Parse("""{"serverContent":{"interrupted":true}}""");

        var firstFragment =
            """{"serverContent":{"inputTranscription":{"text":"Stop","finished":false}}}""";
        GoogleRealtimeTestSupport.AssertEvent(
            mapper.ParseOne(firstFragment),
            firstFragment,
            "input-transcription-completed",
            itemId: "google-input-1",
            transcript: "Stop"
        );

        mapper.Parse("""{"serverContent":{"turnComplete":true}}""");

        var finalFragment =
            """{"serverContent":{"inputTranscription":{"text":" now.","finished":true}}}""";
        GoogleRealtimeTestSupport.AssertEvent(
            mapper.ParseOne(finalFragment),
            finalFragment,
            "input-transcription-completed",
            itemId: "google-input-1",
            transcript: "Stop now."
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "keeps a delayed final fragment with its utterance and separates the next one",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Keeps_a_delayed_final_fragment_with_its_utterance_and_separates_the_next_one()
    {
        var mapper = new Mapper();

        mapper.Parse(
            """{"serverContent":{"inputTranscription":{"text":"What time","finished":false}}}"""
        );
        mapper.Parse(
            """{"serverContent":{"modelTurn":{"parts":[{"inlineData":{"data":"audio1"}}]}}}"""
        );
        mapper.Parse("""{"serverContent":{"turnComplete":true}}""");

        var delayedFinalFragment =
            """{"serverContent":{"inputTranscription":{"text":" is it?","finished":true}}}""";
        GoogleRealtimeTestSupport.AssertEvent(
            mapper.ParseOne(delayedFinalFragment),
            delayedFinalFragment,
            "input-transcription-completed",
            itemId: "google-input-0",
            transcript: "What time is it?"
        );

        var nextUtterance =
            """{"serverContent":{"inputTranscription":{"text":"And the date?","finished":true}}}""";
        GoogleRealtimeTestSupport.AssertEvent(
            mapper.ParseOne(nextUtterance),
            nextUtterance,
            "input-transcription-completed",
            itemId: "google-input-1",
            transcript: "And the date?"
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "assigns separate IDs to consecutive finished transcriptions",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Assigns_separate_IDs_to_consecutive_finished_transcriptions()
    {
        var mapper = new Mapper();

        var first =
            """{"serverContent":{"inputTranscription":{"text":"First utterance.","finished":true}}}""";
        GoogleRealtimeTestSupport.AssertEvent(
            mapper.ParseOne(first),
            first,
            "input-transcription-completed",
            itemId: "google-input-0",
            transcript: "First utterance."
        );

        var second =
            """{"serverContent":{"inputTranscription":{"text":"Second utterance.","finished":true}}}""";
        GoogleRealtimeTestSupport.AssertEvent(
            mapper.ParseOne(second),
            second,
            "input-transcription-completed",
            itemId: "google-input-1",
            transcript: "Second utterance."
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "starts a new response after interruption without a trailing turnComplete",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Starts_a_new_response_after_interruption_without_a_trailing_turnComplete()
    {
        var mapper = new Mapper();
        mapper.Parse(
            """{"serverContent":{"inputTranscription":{"text":"Tell me a story."},"modelTurn":{"parts":[{"text":"Once upon a time"}]}}}"""
        );
        mapper.Parse("""{"serverContent":{"interrupted":true}}""");
        var stop = mapper.ParseOne("""{"serverContent":{"inputTranscription":{"text":"Stop."}}}""");
        Assert.Equal("google-input-1", stop.ItemId);
        Assert.Equal("Stop.", stop.Transcript);

        var ok = mapper.ParseOne("""{"serverContent":{"modelTurn":{"parts":[{"text":"OK."}]}}}""");
        Assert.Equal("google-resp-1", ok.ResponseId);
        Assert.Equal("google-item-1", ok.ItemId);
        mapper.Parse("""{"serverContent":{"turnComplete":true}}""");
        var next = mapper.ParseOne(
            """{"serverContent":{"inputTranscription":{"text":"Next question."}}}"""
        );
        Assert.Equal("google-input-2", next.ItemId);
        Assert.Equal("Next question.", next.Transcript);
    }

    [Fact]
    [UpstreamTest(
        Id + "accumulates consecutive input transcription fragments into one utterance",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Accumulates_consecutive_input_transcription_fragments_into_one_utterance()
    {
        var mapper = new Mapper();

        var fragment1 =
            """{"serverContent":{"inputTranscription":{"text":"The quick brown fox"}}}""";
        GoogleRealtimeTestSupport.AssertEvent(
            mapper.ParseOne(fragment1),
            fragment1,
            "input-transcription-completed",
            itemId: "google-input-0",
            transcript: "The quick brown fox"
        );

        var fragment2 = """{"serverContent":{"inputTranscription":{"text":" jumps over the"}}}""";
        GoogleRealtimeTestSupport.AssertEvent(
            mapper.ParseOne(fragment2),
            fragment2,
            "input-transcription-completed",
            itemId: "google-input-0",
            transcript: "The quick brown fox jumps over the"
        );

        var fragment3 = """{"serverContent":{"inputTranscription":{"text":" lazy dog."}}}""";
        GoogleRealtimeTestSupport.AssertEvent(
            mapper.ParseOne(fragment3),
            fragment3,
            "input-transcription-completed",
            itemId: "google-input-0",
            transcript: "The quick brown fox jumps over the lazy dog."
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "starts a fresh accumulation for the next utterance after a completed turn",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Starts_a_fresh_accumulation_for_the_next_utterance_after_a_completed_turn()
    {
        var mapper = new Mapper();

        mapper.Parse(
            """{"serverContent":{"modelTurn":{"parts":[{"inlineData":{"data":"audio1"}}]}}}"""
        );
        mapper.Parse("""{"serverContent":{"turnComplete":true}}""");

        // A late transcript for the just-completed turn still opens at index 0.
        var delayed = """{"serverContent":{"inputTranscription":{"text":"What time is it?"}}}""";
        GoogleRealtimeTestSupport.AssertEvent(
            mapper.ParseOne(delayed),
            delayed,
            "input-transcription-completed",
            itemId: "google-input-0",
            transcript: "What time is it?"
        );

        // A second fragment with no intervening turn boundary is part of the
        // same utterance and accumulates under the same id.
        var continued = """{"serverContent":{"inputTranscription":{"text":" And the date?"}}}""";
        GoogleRealtimeTestSupport.AssertEvent(
            mapper.ParseOne(continued),
            continued,
            "input-transcription-completed",
            itemId: "google-input-0",
            transcript: "What time is it? And the date?"
        );

        var next = mapper.ParseOne(
            """{"serverContent":{"modelTurn":{"parts":[{"inlineData":{"data":"audio2"}}]}}}"""
        );
        Assert.Equal("google-resp-1", next.ResponseId);
        Assert.Equal("google-item-1", next.ItemId);
    }

    [Fact]
    [UpstreamTest(
        Id + "keeps a late transcript attached to the just-completed turn",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Keeps_a_late_transcript_attached_to_the_just_completed_turn()
    {
        var mapper = new Mapper();

        mapper.Parse(
            """{"serverContent":{"modelTurn":{"parts":[{"inlineData":{"data":"audio1"}}]}}}"""
        );

        mapper.Parse("""{"serverContent":{"turnComplete":true}}""");

        // A transcript that arrives after turnComplete (Google delivers
        // transcription independently) must still reference the turn it belongs
        // to, not the next one. The counter only advances when new model content
        // actually arrives.
        var raw = """{"serverContent":{"outputTranscription":{"text":"late transcript"}}}""";
        var result = mapper.ParseOne(raw);

        GoogleRealtimeTestSupport.AssertEvent(
            result,
            raw,
            "audio-transcript-delta",
            responseId: "google-resp-0",
            itemId: "google-item-0",
            delta: "late transcript"
        );

        // The next model response then opens turn 1.
        var next = mapper.ParseOne(
            """{"serverContent":{"modelTurn":{"parts":[{"inlineData":{"data":"audio2"}}]}}}"""
        );
        Assert.Equal("google-resp-1", next.ResponseId);
    }

    [Fact]
    [UpstreamTest(
        Id + "maps multi-part serverContent to multiple events",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_multi_part_serverContent_to_multiple_events()
    {
        var mapper = new Mapper();
        var raw =
            """{"serverContent":{"modelTurn":{"parts":[{"inlineData":{"data":"audio"}},{"text":"text"}]}}}""";
        var result = mapper.Parse(raw);

        Assert.Collection(
            result,
            e =>
                GoogleRealtimeTestSupport.AssertEvent(
                    e,
                    raw,
                    "audio-delta",
                    responseId: "google-resp-0",
                    itemId: "google-item-0",
                    delta: "audio"
                ),
            e =>
                GoogleRealtimeTestSupport.AssertEvent(
                    e,
                    raw,
                    "text-delta",
                    responseId: "google-resp-0",
                    itemId: "google-item-0",
                    delta: "text"
                )
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "maps toolCall to function-call-arguments-delta and done events",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_toolCall_to_function_call_arguments_delta_and_done_events()
    {
        var mapper = new Mapper();
        var raw =
            """{"toolCall":{"functionCalls":[{"id":"call_1","name":"getWeather","args":{"city":"NYC"}},{"id":"call_2","name":"rollDice","args":{}}]}}""";
        var result = mapper.Parse(raw);

        Assert.Collection(
            result,
            e =>
                GoogleRealtimeTestSupport.AssertEvent(
                    e,
                    raw,
                    "function-call-arguments-delta",
                    responseId: "google-resp-0",
                    itemId: "google-item-0",
                    callId: "call_1",
                    delta: """{"city":"NYC"}"""
                ),
            e =>
                GoogleRealtimeTestSupport.AssertEvent(
                    e,
                    raw,
                    "function-call-arguments-done",
                    responseId: "google-resp-0",
                    itemId: "google-item-0",
                    callId: "call_1",
                    name: "getWeather",
                    arguments: """{"city":"NYC"}"""
                ),
            e =>
                GoogleRealtimeTestSupport.AssertEvent(
                    e,
                    raw,
                    "function-call-arguments-delta",
                    responseId: "google-resp-0",
                    itemId: "google-item-0",
                    callId: "call_2",
                    delta: "{}"
                ),
            e =>
                GoogleRealtimeTestSupport.AssertEvent(
                    e,
                    raw,
                    "function-call-arguments-done",
                    responseId: "google-resp-0",
                    itemId: "google-item-0",
                    callId: "call_2",
                    name: "rollDice",
                    arguments: "{}"
                )
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "maps toolCallCancellation to custom event",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_toolCallCancellation_to_custom_event()
    {
        var mapper = new Mapper();
        var raw = """{"toolCallCancellation":{"ids":["call_1"]}}""";
        var result = mapper.ParseOne(raw);

        GoogleRealtimeTestSupport.AssertEvent(
            result,
            raw,
            "custom",
            rawType: "toolCallCancellation"
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "maps goAway to a stable custom lifecycle event",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_goAway_to_a_stable_custom_lifecycle_event()
    {
        var mapper = new Mapper();
        var raw = """{"goAway":{"timeLeft":"30s"}}""";

        GoogleRealtimeTestSupport.AssertEvent(
            mapper.ParseOne(raw),
            raw,
            "custom",
            rawType: "goAway"
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "maps sessionResumptionUpdate to a stable custom lifecycle event",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_sessionResumptionUpdate_to_a_stable_custom_lifecycle_event()
    {
        var mapper = new Mapper();
        var raw =
            """{"sessionResumptionUpdate":{"newHandle":"resume-handle","resumable":true,"lastConsumedClientMessageIndex":"42"}}""";

        GoogleRealtimeTestSupport.AssertEvent(
            mapper.ParseOne(raw),
            raw,
            "custom",
            rawType: "sessionResumptionUpdate"
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "surfaces interactionStatus as a custom event",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Surfaces_interactionStatus_as_a_custom_event()
    {
        var mapper = new Mapper();
        var raw = """{"serverContent":{"interactionStatus":"IN_PROGRESS"}}""";

        GoogleRealtimeTestSupport.AssertEvent(
            mapper.ParseOne(raw),
            raw,
            "custom",
            rawType: "interactionStatus"
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "emits interactionStatus alongside turnComplete events",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Emits_interactionStatus_alongside_turnComplete_events()
    {
        var mapper = new Mapper();
        mapper.Parse(
            """{"serverContent":{"modelTurn":{"parts":[{"inlineData":{"data":"AAAA"}}]}}}"""
        );
        var raw = """{"serverContent":{"turnComplete":true,"interactionStatus":"IDLE"}}""";

        var events = mapper.Parse(raw);
        Assert.Contains(events, e => e.Type == "custom" && e.RawType == "interactionStatus");
        GoogleRealtimeTestSupport.AssertEvent(
            events.Single(e => e.Type == "custom"),
            raw,
            "custom",
            rawType: "interactionStatus"
        );
        GoogleRealtimeTestSupport.AssertEvent(
            events.Single(e => e.Type == "response-done"),
            raw,
            "response-done",
            responseId: "google-resp-0",
            status: "completed"
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "surfaces waitingForInput as a custom event",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Surfaces_waitingForInput_as_a_custom_event()
    {
        var mapper = new Mapper();
        var raw = """{"serverContent":{"waitingForInput":true}}""";

        GoogleRealtimeTestSupport.AssertEvent(
            mapper.ParseOne(raw),
            raw,
            "custom",
            rawType: "waitingForInput"
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "keeps generationComplete distinct from turnComplete",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Keeps_generationComplete_distinct_from_turnComplete()
    {
        var mapper = new Mapper();
        var raw = """{"serverContent":{"generationComplete":true}}""";

        GoogleRealtimeTestSupport.AssertEvent(
            mapper.ParseOne(raw),
            raw,
            "custom",
            rawType: "generationComplete"
        );

        var next = mapper.ParseOne(
            """{"serverContent":{"modelTurn":{"parts":[{"text":"still turn zero"}]}}}"""
        );
        Assert.Equal("text-delta", next.Type);
        Assert.Equal("google-resp-0", next.ResponseId);
    }

    [Fact]
    [UpstreamTest(
        Id + "maps unrecognized top-level key to custom event",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_unrecognized_top_level_key_to_custom_event()
    {
        var mapper = new Mapper();
        var raw = """{"somethingNew":{"data":123}}""";
        var result = mapper.ParseOne(raw);

        GoogleRealtimeTestSupport.AssertEvent(result, raw, "custom", rawType: "somethingNew");
    }
}
