// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel;

namespace Vercel.AI.Tests;

/// <summary>Shares the collection of the tests that change <c>AI_GATEWAY_API_KEY</c>, so none runs in parallel with another.</summary>
[Collection("DecisionDefaultProvider")]
public sealed class GatewayLanguageModelRegressionTests
{
    private static readonly LanguageModelCallOptions Prompt =
        GatewayLanguageModelSupport.TestPrompt;

    [Theory]
    [InlineData("null")]
    [InlineData("\"503\"")]
    public async Task A_stream_error_with_a_non_numeric_status_code_still_yields_an_error_part(
        string status
    )
    {
        var server = GatewayLanguageModelSupport.StreamChunks(
            "{\"type\":\"error\",\"error\":{\"message\":\"x\",\"statusCode\":" + status + "}}"
        );

        var result = await GatewayLanguageModelSupport
            .CreateTestModel(server)
            .DoStreamAsync(Prompt, CancellationToken.None);
        var parts = await GatewayLanguageModelSupport.ReadAll(result);

        var error = Assert.IsType<ErrorStreamPart>(Assert.Single(parts)).Error!;
        Assert.Equal("x", error.Message);
        Assert.Null(error.StatusCode);
    }

    [Fact]
    public async Task A_stream_start_warning_that_is_not_an_object_does_not_fail_the_stream()
    {
        var server = GatewayLanguageModelSupport.StreamChunks(
            "{\"type\":\"stream-start\",\"warnings\":[\"oops\"]}"
        );

        var result = await GatewayLanguageModelSupport
            .CreateTestModel(server)
            .DoStreamAsync(Prompt, CancellationToken.None);
        var parts = await GatewayLanguageModelSupport.ReadAll(result);

        var warning = Assert.Single(
            Assert.IsType<StreamStartStreamPart>(Assert.Single(parts)).Warnings
        );
        Assert.Equal("other", warning.Type);
    }
}
