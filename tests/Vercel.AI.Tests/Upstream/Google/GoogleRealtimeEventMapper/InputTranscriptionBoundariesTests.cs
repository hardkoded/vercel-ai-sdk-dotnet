// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Mapper = global::Vercel.AI.Google.GoogleRealtimeEventMapper;

namespace Vercel.AI.Tests.Upstream.Google.GoogleRealtimeEventMapper;

/// <summary>
/// Port of <c>google-realtime-event-mapper.test.ts</c> &gt; <c>GoogleRealtimeEventMapper &gt; parseServerEvent &gt; %s input
/// transcription boundaries</c>, run once for <c>serverContent</c> and once for <c>top-level</c>.
/// </summary>
public sealed class InputTranscriptionBoundariesTests
{
    private const string Id =
        "packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper > parseServerEvent > %s input transcription boundaries::";

    public static TheoryData<string> Locations
    {
        get { return new TheoryData<string> { "serverContent", "top-level" }; }
    }

    public static TheoryData<string, string> CompletionMarkers
    {
        get
        {
            var data = new TheoryData<string, string>();
            foreach (var location in new[] { "serverContent", "top-level" })
            {
                data.Add(location, """{"finished":true}""");
                data.Add(location, """{"text":"","finished":true}""");
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(CompletionMarkers))]
    [UpstreamTest(
        Id + "honors a standalone completion marker %j after turnComplete",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Honors_a_standalone_completion_marker_after_turnComplete(
        string location,
        string completion
    )
    {
        var mapper = new Mapper();
        mapper.Parse(Wrap(location, """{"text":"First","finished":false}"""));
        mapper.Parse("""{"serverContent":{"turnComplete":true}}""");

        // Completion-only messages must not create an empty user message.
        Assert.Equal("custom", mapper.ParseOne(Wrap(location, completion)).Type);

        var next = Wrap(location, """{"text":"Second","finished":true}""");
        GoogleRealtimeTestSupport.AssertEvent(
            mapper.ParseOne(next),
            next,
            "input-transcription-completed",
            itemId: "google-input-1",
            transcript: "Second"
        );
    }

    [Theory]
    [MemberData(nameof(Locations))]
    [UpstreamTest(
        Id + "separates utterances using standalone completion markers without response events",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Separates_utterances_using_standalone_completion_markers_without_response_events(
        string location
    )
    {
        var mapper = new Mapper();
        mapper.Parse(Wrap(location, """{"text":"First"}"""));
        mapper.Parse(Wrap(location, """{"finished":true}"""));

        // Empty or repeated markers must not consume another ID or reopen
        // the completed utterance.
        mapper.Parse(Wrap(location, """{"finished":true}"""));
        mapper.Parse(Wrap(location, """{"text":"","finished":false}"""));

        var next = Wrap(location, """{"text":"Second"}""");
        GoogleRealtimeTestSupport.AssertEvent(
            mapper.ParseOne(next),
            next,
            "input-transcription-completed",
            itemId: "google-input-1",
            transcript: "Second"
        );
    }

    [Theory]
    [MemberData(nameof(Locations))]
    [UpstreamTest(
        Id + "does not allocate an utterance for empty input",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Does_not_allocate_an_utterance_for_empty_input(string location)
    {
        var mapper = new Mapper();
        foreach (
            var transcription in new[]
            {
                "{}",
                """{"finished":false}""",
                """{"finished":true}""",
                """{"text":"","finished":true}""",
            }
        )
        {
            Assert.Equal("custom", mapper.ParseOne(Wrap(location, transcription)).Type);
        }

        var first = mapper.ParseOne(Wrap(location, """{"text":"First"}"""));
        Assert.Equal("google-input-0", first.ItemId);
        Assert.Equal("First", first.Transcript);
    }

    [Theory]
    [MemberData(nameof(Locations))]
    [UpstreamTest(
        Id + "preserves an interrupting utterance until its standalone completion marker",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Preserves_an_interrupting_utterance_until_its_standalone_completion_marker(
        string location
    )
    {
        var mapper = new Mapper();
        mapper.Parse(Wrap(location, """{"text":"First"}"""));
        mapper.Parse("""{"serverContent":{"interrupted":true}}""");
        mapper.Parse(Wrap(location, """{"text":"Stop"}"""));
        mapper.Parse("""{"serverContent":{"turnComplete":true}}""");
        var stopNow = mapper.ParseOne(Wrap(location, """{"text":" now"}"""));
        Assert.Equal("google-input-1", stopNow.ItemId);
        Assert.Equal("Stop now", stopNow.Transcript);
        mapper.Parse(Wrap(location, """{"finished":true}"""));
        var next = mapper.ParseOne(Wrap(location, """{"text":"Next"}"""));
        Assert.Equal("google-input-2", next.ItemId);
        Assert.Equal("Next", next.Transcript);
    }

    private static string Wrap(string location, string inputTranscription)
    {
        return location == "serverContent"
            ? "{\"serverContent\":{\"inputTranscription\":" + inputTranscription + "}}"
            : "{\"inputTranscription\":" + inputTranscription + "}";
    }
}
