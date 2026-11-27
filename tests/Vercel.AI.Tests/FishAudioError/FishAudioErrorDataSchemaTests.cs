// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.FishAudio;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.FishAudioError;

public sealed class FishAudioErrorDataSchemaTests
{
    private const string Prefix =
        "packages/fish-audio/src/fish-audio-error.test.ts::fishAudioErrorDataSchema::";

    [Fact]
    [UpstreamTest(Prefix + "should parse a Fish Audio error", Coverage = UpstreamCoverage.Covered)]
    public void Should_parse_a_Fish_Audio_error()
    {
        const string error =
            "{\"status\":401,\"message\":\"No permission -- see authorization schemes\"}";
        var result = JsonParsing.SafeParse(
            error,
            Vercel.AI.FishAudio.FishAudioProvider.ErrorSchema
        );
        Assert.True(result.Success);
        JsonAssert.Equal(result.Value!.Value, error);
        JsonAssert.Equal(result.RawValue!.Value, error);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should tolerate a payload without a message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_tolerate_a_payload_without_a_message()
    {
        Assert.True(
            JsonParsing
                .SafeParse("{\"status\":402}", Vercel.AI.FishAudio.FishAudioProvider.ErrorSchema)
                .Success
        );
    }
}
