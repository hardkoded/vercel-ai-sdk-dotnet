// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;
using Envelope = Vercel.AI.ProviderUtils.TranscriptionStreamEnvelope;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.TranscriptionStreamEnvelope;

/// <summary>Port of <c>transcription-stream-envelope.test.ts</c> &gt; <c>parseTranscriptionStreamPart</c>.</summary>
public sealed class ParseTranscriptionStreamPartTests
{
    private const string Prefix = "packages/provider-utils/src/transcription-stream-envelope.test.ts::parseTranscriptionStreamPart::";

    [Fact]
    [UpstreamTest(Prefix + "should round-trip a $type part", Coverage = UpstreamCoverage.Covered)]
    public void Should_round_trip_a_type_part()
    {
        var parts = new[]
        {
            new ModelStreamPart { Type = "stream-start", Warnings = Array.Empty<OperationWarning>() },
            new ModelStreamPart { Type = "transcript-delta", Id = "seg-1", Delta = "Hel" },
            new ModelStreamPart { Type = "transcript-partial", Id = "seg-1", Text = "Hel" },
            new ModelStreamPart { Type = "transcript-final", Id = "seg-1", Text = "Hello" },
            new ModelStreamPart { Type = "raw", Raw = JsonDocument.Parse("""{"some":"chunk"}""").RootElement.Clone() },
            new ModelStreamPart { Type = "error", ErrorValue = JsonDocument.Parse("\"model overloaded\"").RootElement.Clone() },
            new ModelStreamPart
            {
                Type = "finish",
                Text = "Hello",
                Segments = new[] { new TranscriptSegment("Hello", 0, 1) },
                Language = "en",
                Usage = new OperationUsage(inputTokens: 3),
            },
        };

        foreach (var part in parts)
        {
            var parsed = Envelope.ParseTranscriptionStreamPart(Envelope.SerializeTranscriptionStreamPart(part) ?? string.Empty)!;

            Assert.Equal(part.Type, parsed.Type);
            Assert.Equal(part.Id, parsed.Id);
            Assert.Equal(part.Delta, parsed.Delta);
            Assert.Equal(part.Text, parsed.Text);
            Assert.Equal(part.Language, parsed.Language);
            Assert.Equal(part.Raw?.GetRawText(), parsed.Raw?.GetRawText());
            Assert.Equal(part.ErrorValue?.GetRawText(), parsed.ErrorValue?.GetRawText());
            Assert.Equal(part.Warnings?.Count, parsed.Warnings?.Count);
            Assert.Equal(part.Usage?.InputTokens, parsed.Usage?.InputTokens);
            Assert.Equal(part.Segments?.Select(segment => (segment.Text, segment.StartSecond, segment.EndSecond)), parsed.Segments?.Select(segment => (segment.Text, segment.StartSecond, segment.EndSecond)));
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "should revive response-metadata timestamps to Date", Coverage = UpstreamCoverage.Covered)]
    public void Should_revive_response_metadata_timestamps_to_Date()
    {
        var part = Envelope.ParseTranscriptionStreamPart(
            Envelope.SerializeTranscriptionStreamPart(new ModelStreamPart
            {
                Type = "response-metadata",
                Timestamp = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                ModelId = "openai/gpt-realtime-whisper",
            }) ?? string.Empty)!;

        Assert.Equal("response-metadata", part.Type);
        Assert.Equal(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), part.Timestamp);
        Assert.Equal("openai/gpt-realtime-whisper", part.ModelId);
    }

    [Fact]
    [UpstreamTest(Prefix + "should keep a missing response-metadata timestamp undefined", Coverage = UpstreamCoverage.Covered)]
    public void Should_keep_a_missing_response_metadata_timestamp_undefined()
    {
        var part = Envelope.ParseTranscriptionStreamPart("""{"type":"response-metadata","modelId":"model-1"}""")!;

        Assert.Equal("response-metadata", part.Type);
        Assert.Null(part.Timestamp);
        Assert.Equal("model-1", part.ModelId);
    }

    [Fact]
    [UpstreamTest(Prefix + "should return undefined for malformed JSON", Coverage = UpstreamCoverage.Covered)]
    public void Should_return_undefined_for_malformed_JSON()
    {
        Assert.Null(Envelope.ParseTranscriptionStreamPart("{not json"));
    }

    [Fact]
    [UpstreamTest(Prefix + "should return undefined for prototype-pollution payloads", Coverage = UpstreamCoverage.Covered)]
    public void Should_return_undefined_for_prototype_pollution_payloads()
    {
        var part = "{\"type\":\"transcript-delta\",\"id\":\"seg-1\",\"delta\":\"Hel\",\"__proto__\":{\"polluted\":true}}";

        Assert.Null(Envelope.ParseTranscriptionStreamPart(part));
    }

    [Theory]
    [InlineData("\"a string\"")]
    [InlineData("[1, 2]")]
    [InlineData("null")]
    [InlineData("42")]
    [UpstreamTest(Prefix + "should return undefined for non-object JSON: %s", Coverage = UpstreamCoverage.Covered)]
    public void Should_return_undefined_for_non_object_JSON(string text)
    {
        Assert.Null(Envelope.ParseTranscriptionStreamPart(text));
    }

    [Fact]
    [UpstreamTest(Prefix + "should return undefined for unknown part types (forward compat)", Coverage = UpstreamCoverage.Covered)]
    public void Should_return_undefined_for_unknown_part_types_forward_compat()
    {
        Assert.Null(Envelope.ParseTranscriptionStreamPart("""{"type":"some-future-part"}"""));
    }

    // Known-type frames with a malformed shape (for example from a drifted server version) are rejected rather than
    // passed through: downstream SDK code dereferences the required fields and would otherwise fail mid-stream.
    [Theory]
    [InlineData("stream-start without warnings", """{"type":"stream-start"}""")]
    [InlineData("stream-start with non-array warnings", """{"type":"stream-start","warnings":"none"}""")]
    [InlineData("transcript-delta without delta", """{"type":"transcript-delta","id":"s1"}""")]
    [InlineData("transcript-delta with non-string delta", """{"type":"transcript-delta","delta":42}""")]
    [InlineData("transcript-partial without text", """{"type":"transcript-partial"}""")]
    [InlineData("transcript-partial with non-string text", """{"type":"transcript-partial","text":42}""")]
    [InlineData("transcript-final without text", """{"type":"transcript-final"}""")]
    [InlineData("finish without text", """{"type":"finish","segments":[]}""")]
    [InlineData("finish without segments", """{"type":"finish","text":"Hello"}""")]
    [InlineData("finish with non-array segments", """{"type":"finish","text":"Hello","segments":{}}""")]
    [InlineData("response-metadata with a non-string timestamp", """{"type":"response-metadata","timestamp":{}}""")]
    [InlineData("response-metadata with an unparsable timestamp", """{"type":"response-metadata","timestamp":"not-a-date"}""")]
    [InlineData("stream-start with null warning elements", """{"type":"stream-start","warnings":[null]}""")]
    [InlineData("stream-start with non-object warning elements", """{"type":"stream-start","warnings":["unsupported"]}""")]
    [InlineData("finish with null segment elements", """{"type":"finish","text":"Hello","segments":[null]}""")]
    [InlineData("raw without rawValue", """{"type":"raw"}""")]
    [InlineData("error without error", """{"type":"error"}""")]
    [InlineData("stream-start with warning elements missing a type", """{"type":"stream-start","warnings":[{"message":"no type"}]}""")]
    [InlineData("stream-start with non-string warning types", """{"type":"stream-start","warnings":[{"type":42}]}""")]
    [InlineData("transcript-delta with a numeric id", """{"type":"transcript-delta","delta":"Hel","id":42}""")]
    [InlineData("transcript-partial with a non-numeric startSecond", """{"type":"transcript-partial","text":"Hel","startSecond":"zero"}""")]
    [InlineData("transcript-final with a numeric id", """{"type":"transcript-final","text":"Hello","id":7}""")]
    [InlineData("transcript-final with a non-numeric endSecond", """{"type":"transcript-final","text":"Hello","endSecond":"one"}""")]
    [InlineData("finish with incomplete segment elements", """{"type":"finish","text":"Hello","segments":[{}]}""")]
    [InlineData("finish with mistyped segment timings", """{"type":"finish","text":"Hello","segments":[{"text":"Hello","startSecond":"zero","endSecond":1}]}""")]
    [InlineData("finish with a non-string language", """{"type":"finish","text":"Hello","segments":[],"language":42}""")]
    [InlineData("finish with a non-numeric durationInSeconds", """{"type":"finish","text":"Hello","segments":[],"durationInSeconds":"7"}""")]
    [InlineData("response-metadata with a non-string modelId", """{"type":"response-metadata","modelId":42}""")]
    [UpstreamTest(Prefix + "should return undefined for %s", Coverage = UpstreamCoverage.Covered)]
    public void Should_return_undefined_for_a_malformed_known_part(string name, string part)
    {
        Assert.NotEmpty(name);
        Assert.Null(Envelope.ParseTranscriptionStreamPart(part));
    }
}
