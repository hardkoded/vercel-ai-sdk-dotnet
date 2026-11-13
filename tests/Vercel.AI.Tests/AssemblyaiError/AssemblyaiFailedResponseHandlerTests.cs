// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.AssemblyAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.AssemblyaiError;

public sealed class AssemblyaiFailedResponseHandlerTests
{
    private const string Prefix = "packages/assemblyai/src/assemblyai-error.test.ts::assemblyaiFailedResponseHandler::";
    private const string Url = "https://api.assemblyai.com/v2/transcript";
    private const string UpstreamCommit = "c87b19ee759b37e353f0b111943c2533dfc561f8";

    [Fact]
    [UpstreamTest(Prefix + "should use the flat error message", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Should_use_the_flat_error_message()
    {
        var error = Handle("{\"error\":\"Invalid API key\"}", 401);

        Assert.Equal("Invalid API key", error.Message);
        Assert.Equal(401, error.StatusCode);
        var data = Assert.IsType<AssemblyAIErrorData>(error.ErrorData);
        Assert.Equal("Invalid API key", data.Message);
        Assert.Null(data.Code);
    }

    [Fact]
    [UpstreamTest(Prefix + "should use the flat error message with a status", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Should_use_the_flat_error_message_with_a_status()
    {
        var error = Handle("{\"error\":\"format_text must be a Boolean\",\"status\":\"error\"}", 400);

        Assert.Equal("format_text must be a Boolean", error.Message);
        Assert.Equal(400, error.StatusCode);
        // The parsed data carries only the error. Unknown keys such as `status` are stripped.
        var data = Assert.IsType<AssemblyAIErrorData>(error.ErrorData);
        Assert.Equal("format_text must be a Boolean", data.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should keep using the nested error message", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Should_keep_using_the_nested_error_message()
    {
        const string message = "{\n  \"error\": {\n    \"code\": 429,\n    \"message\": \"Resource has been exhausted (e.g. check quota).\",\n    \"status\": \"RESOURCE_EXHAUSTED\"\n  }\n}\n";
        var body = new System.Text.Json.Nodes.JsonObject
        {
            ["error"] = new System.Text.Json.Nodes.JsonObject { ["message"] = message, ["code"] = 429 },
        }.ToJsonString();

        var error = Handle(body, 429);

        Assert.Equal(message, error.Message);
        Assert.Equal(429, error.StatusCode);
        var data = Assert.IsType<AssemblyAIErrorData>(error.ErrorData);
        Assert.Equal(message, data.Message);
        Assert.Equal(429, data.Code);
    }

    [Fact]
    [UpstreamTest(Prefix + "should fall back to the status text for an unrecognized error body", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Should_fall_back_to_the_status_text_for_an_unrecognized_error_body()
    {
        var error = Handle("{\"foo\":\"bar\"}", 500, "Internal Server Error");

        Assert.Equal("Internal Server Error", error.Message);
        Assert.Equal(500, error.StatusCode);
        Assert.Equal("{\"foo\":\"bar\"}", error.ResponseBody);
    }

    [Fact]
    [UpstreamTest(Prefix + "should fall back to the status text for a non-JSON error body", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Should_fall_back_to_the_status_text_for_a_non_JSON_error_body()
    {
        var error = Handle("not json", 502, "Bad Gateway");

        Assert.Equal("Bad Gateway", error.Message);
        Assert.Equal(502, error.StatusCode);
        Assert.Equal("not json", error.ResponseBody);
    }

    private static ApiException Handle(string body, int statusCode, string? statusText = null)
    {
        return AssemblyAIFailedResponseHandler.Handle(body, statusCode, Url, new Dictionary<string, string>(), null, statusText);
    }
}
