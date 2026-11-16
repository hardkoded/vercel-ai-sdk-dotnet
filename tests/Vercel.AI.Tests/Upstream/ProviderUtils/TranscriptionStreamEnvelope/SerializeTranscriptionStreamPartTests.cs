// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;
using Envelope = Vercel.AI.ProviderUtils.TranscriptionStreamEnvelope;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.TranscriptionStreamEnvelope;

/// <summary>
/// Port of <c>transcription-stream-envelope.test.ts</c> &gt; <c>serializeTranscriptionStreamPart</c>. The cross-realm
/// <c>Error</c> test and the bigint and cyclic payload tests are not ported: .NET has no realms, and a raw payload is a
/// <see cref="JsonElement"/>, which is always serializable.
/// </summary>
public sealed class SerializeTranscriptionStreamPartTests
{
    private const string Prefix = "packages/provider-utils/src/transcription-stream-envelope.test.ts::serializeTranscriptionStreamPart::";

    [Fact]
    [UpstreamTest(Prefix + "should serialize parts as JSON", Coverage = UpstreamCoverage.Covered)]
    public void Should_serialize_parts_as_JSON()
    {
        Assert.Equal(
            """{"type":"transcript-delta","id":"seg-1","delta":"Hel"}""",
            Envelope.SerializeTranscriptionStreamPart(new ModelStreamPart { Type = "transcript-delta", Id = "seg-1", Delta = "Hel" }));
    }

    [Fact]
    [UpstreamTest(Prefix + "should serialize response-metadata timestamps as ISO strings", Coverage = UpstreamCoverage.Covered)]
    public void Should_serialize_response_metadata_timestamps_as_ISO_strings()
    {
        Assert.Equal(
            """{"type":"response-metadata","timestamp":"2026-01-01T00:00:00.000Z","modelId":"openai/gpt-realtime-whisper"}""",
            Envelope.SerializeTranscriptionStreamPart(new ModelStreamPart
            {
                Type = "response-metadata",
                Timestamp = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                ModelId = "openai/gpt-realtime-whisper",
            }));
    }

    [Fact]
    [UpstreamTest(Prefix + "should serialize Error payloads in error parts with their name and message", Coverage = UpstreamCoverage.Covered)]
    public void Should_serialize_Error_payloads_in_error_parts_with_their_name_and_message()
    {
        Assert.Equal(
            """{"type":"error","error":{"name":"Error","message":"rate limited"}}""",
            Envelope.SerializeTranscriptionStreamPart(new ModelStreamPart { Type = "error", Error = new Exception("rate limited") }));
    }

    [Fact]
    [UpstreamTest(Prefix + "should round-trip an error part with an Error payload", Coverage = UpstreamCoverage.Covered)]
    public void Should_round_trip_an_error_part_with_an_Error_payload()
    {
        // The port has no TypeError; a FormatException stands for it.
        var parsed = Envelope.ParseTranscriptionStreamPart(
            Envelope.SerializeTranscriptionStreamPart(new ModelStreamPart { Type = "error", Error = new FormatException("bad input") }) ?? string.Empty);

        Assert.Equal("error", parsed!.Type);
        Assert.Equal("""{"name":"FormatException","message":"bad input"}""", parsed.ErrorValue!.Value.GetRawText());
    }
}
