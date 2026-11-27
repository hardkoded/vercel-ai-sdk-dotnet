// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel.GatewayLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel;

/// <summary>Port of <c>gateway-language-model.test.ts</c> &gt; <c>GatewayLanguageModel &gt; doGenerate &gt; Gateway error handling for malformed responses</c>.</summary>
public sealed class GatewayLanguageModelDoGenerateGatewayErrorHandlingForMalformedResponsesTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doGenerate > Gateway error handling for malformed responses::";

    private static readonly LanguageModelCallOptions Prompt = new()
    {
        Prompt = new ModelMessage[] { new UserModelMessage("test") },
    };

    [Fact]
    [UpstreamTest(
        Prefix + "should include actual response body when APICallError has no data",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_actual_response_body_when_APICallError_has_no_data()
    {
        var malformedResponse =
            "{\"ferror\":{\"message\":\"Model not found\",\"type\":\"model_not_found\"}}";
        // Mock the server to return malformed response that can't be parsed by AI SDK
        var server = ErrorResponse(404, malformedResponse);

        var error = await Assert.ThrowsAsync<GatewayResponseError>(() =>
            CreateTestModel(server).DoGenerateAsync(Prompt, CancellationToken.None)
        );

        JsonAssert.Equal(error.Response!.Value, malformedResponse);
        Assert.NotNull(error.ValidationError);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should use raw response body when JSON parsing fails",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_raw_response_body_when_JSON_parsing_fails()
    {
        var invalidJson = "invalid json response";
        // Mock the server to return invalid JSON
        var server = ErrorResponse(500, invalidJson);

        var error = await Assert.ThrowsAsync<GatewayResponseError>(() =>
            CreateTestModel(server).DoGenerateAsync(Prompt, CancellationToken.None)
        );

        Assert.Equal(JsonValueKind.String, error.Response!.Value.ValueKind);
        Assert.Equal(invalidJson, error.Response.Value.GetString());
        Assert.NotNull(error.ValidationError);
    }
}
