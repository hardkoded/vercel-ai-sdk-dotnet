// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using AssemblyAIErrorType = Vercel.AI.AssemblyAI.AssemblyAIError;

namespace Vercel.AI.Tests.AssemblyaiError;

public sealed class AssemblyaiErrorDataSchemaTests
{
    private const string Prefix = "packages/assemblyai/src/assemblyai-error.test.ts::assemblyaiErrorDataSchema::";
    private const string UpstreamCommit = "c87b19ee759b37e353f0b111943c2533dfc561f8";

    private const string ExhaustedMessage = "{\n  \"error\": {\n    \"code\": 429,\n    \"message\": \"Resource has been exhausted (e.g. check quota).\",\n    \"status\": \"RESOURCE_EXHAUSTED\"\n  }\n}\n";

    [Fact]
    [UpstreamTest(Prefix + "should parse AssemblyAI resource exhausted error", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Should_parse_AssemblyAI_resource_exhausted_error()
    {
        var json = new JsonObject
        {
            ["error"] = new JsonObject { ["message"] = ExhaustedMessage, ["code"] = 429 },
        }.ToJsonString();

        var result = AssemblyAIErrorType.Parse(json);

        Assert.True(result.Success);
        Assert.Equal(ExhaustedMessage, result.Value!.Message);
        Assert.Equal(429, result.Value.Code);
        Assert.Equal(result.Value.Message, result.RawValue!.Message);
        Assert.Equal(result.Value.Code, result.RawValue.Code);
    }

    [Fact]
    [UpstreamTest(Prefix + "should parse the documented flat error shape", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Should_parse_the_documented_flat_error_shape()
    {
        var result = AssemblyAIErrorType.Parse("{\"error\":\"Invalid API key\"}");

        Assert.True(result.Success);
        Assert.Equal("Invalid API key", result.Value!.Message);
        Assert.Null(result.Value.Code);
        Assert.Equal("Invalid API key", result.RawValue!.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should parse the documented flat error shape with a status", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Should_parse_the_documented_flat_error_shape_with_a_status()
    {
        // The parsed value carries only the error, so the schema strips unknown keys such as `status`.
        var result = AssemblyAIErrorType.Parse("{\"error\":\"format_text must be a Boolean\",\"status\":\"error\"}");

        Assert.True(result.Success);
        Assert.Equal("format_text must be a Boolean", result.Value!.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should parse the flat error shape with an unexpected status value", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Should_parse_the_flat_error_shape_with_an_unexpected_status_value()
    {
        var result = AssemblyAIErrorType.Parse("{\"error\":\"Invalid API key\",\"status\":\"unexpected\"}");

        Assert.True(result.Success);
        Assert.Equal("Invalid API key", result.Value!.Message);
    }
}
