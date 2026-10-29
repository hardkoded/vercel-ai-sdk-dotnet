// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.ElevenLabs;

namespace Vercel.AI.Tests.ElevenLabsError;

public sealed class ElevenLabsErrorDataSchemaTests
{
    private const string Prefix = "packages/elevenlabs/src/elevenlabs-error.test.ts::elevenlabsErrorDataSchema::";
    private const string ExhaustedMessage = "{\n  \"error\": {\n    \"code\": 429,\n    \"message\": \"Resource has been exhausted (e.g. check quota).\",\n    \"status\": \"RESOURCE_EXHAUSTED\"\n  }\n}\n";

    [Fact]
    [UpstreamTest(Prefix + "should parse ElevenLabs resource exhausted error", Coverage = UpstreamCoverage.Covered)]
    public void Should_parse_ElevenLabs_resource_exhausted_error()
    {
        var json = JsonSerializer.Serialize(new { error = new { message = ExhaustedMessage, code = 429 } });

        var result = global::Vercel.AI.ElevenLabs.ElevenLabsError.Parse(json);

        Assert.True(result.Success);
        Assert.Equal(ExhaustedMessage, result.Value!.Error!.Message);
        Assert.Equal(429d, result.Value.Error.Code);
        Assert.Equal(result.Value.Error.Message, result.RawValue!.Error!.Message);
        Assert.Equal(result.Value.Error.Code, result.RawValue.Error.Code);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should keep the request id of an object detail in the parsed data",
        Coverage = UpstreamCoverage.Covered,
        Commit = "7062ab6818c90e135d068433ce99b11ac0dfa081")]
    public void Should_keep_the_request_id_of_an_object_detail_in_the_parsed_data()
    {
        const string body = """
            {
              "detail": {
                "type": "authentication_error",
                "code": "invalid_api_key",
                "message": "API key must be exactly 51 characters, got 6.",
                "status": "invalid_api_key_length",
                "request_id": "req_test_401",
                "param": "api_key"
              }
            }
            """;

        var result = global::Vercel.AI.ElevenLabs.ElevenLabsError.Parse(body);

        Assert.True(result.Success);
        var detail = Assert.IsType<ElevenLabsErrorDetail>(result.Value!.Detail);
        Assert.Equal("authentication_error", detail.Type);
        Assert.Equal("invalid_api_key", detail.Code);
        Assert.Equal("API key must be exactly 51 characters, got 6.", detail.Message);
        Assert.Equal("invalid_api_key_length", detail.Status);
        Assert.Equal("req_test_401", detail.RequestId);
        Assert.Equal("api_key", detail.Parameter);
        Assert.Empty(detail.AdditionalProperties);
        Assert.Same(result.Value, result.RawValue);
    }
}
