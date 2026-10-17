// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel;

namespace Vercel.AI.Tests;

public sealed class GatewayLanguageModelRegressionTests
{
    private static readonly LanguageModelCallOptions Prompt = GatewayLanguageModelSupport.TestPrompt;

    [Fact]
    public async Task A_missing_API_key_is_not_mapped_to_a_Gateway_error()
    {
        var provider = GatewayProvider.Create(new GatewayOptions { BaseUrl = "https://api.test.com", ApiKey = "" }, new GatewayLanguageModelServer());
        var previous = Environment.GetEnvironmentVariable(GatewayProvider.ApiKeyEnvironmentVariable);
        Environment.SetEnvironmentVariable(GatewayProvider.ApiKeyEnvironmentVariable, null);
        try
        {
            var model = provider.LanguageModel("test-model");

            var generate = await Assert.ThrowsAsync<AiSdkException>(() => model.DoGenerateAsync(Prompt, CancellationToken.None));
            var stream = await Assert.ThrowsAsync<AiSdkException>(() => model.DoStreamAsync(Prompt, CancellationToken.None));

            Assert.False(GatewayError.IsInstance(generate));
            Assert.False(GatewayError.IsInstance(stream));
        }
        finally
        {
            Environment.SetEnvironmentVariable(GatewayProvider.ApiKeyEnvironmentVariable, previous);
        }
    }

    [Theory]
    [InlineData("null")]
    [InlineData("\"503\"")]
    public async Task A_stream_error_with_a_non_numeric_status_code_still_yields_an_error_part(string status)
    {
        var server = GatewayLanguageModelSupport.StreamChunks("{\"type\":\"error\",\"error\":{\"message\":\"x\",\"statusCode\":" + status + "}}");

        var result = await GatewayLanguageModelSupport.CreateTestModel(server).DoStreamAsync(Prompt, CancellationToken.None);
        var parts = await GatewayLanguageModelSupport.ReadAll(result);

        var error = Assert.IsType<ErrorStreamPart>(Assert.Single(parts)).Error!;
        Assert.Equal("x", error.Message);
        Assert.Null(error.StatusCode);
    }

    [Fact]
    public async Task A_stream_start_warning_that_is_not_an_object_does_not_fail_the_stream()
    {
        var server = GatewayLanguageModelSupport.StreamChunks("{\"type\":\"stream-start\",\"warnings\":[\"oops\"]}");

        var result = await GatewayLanguageModelSupport.CreateTestModel(server).DoStreamAsync(Prompt, CancellationToken.None);
        var parts = await GatewayLanguageModelSupport.ReadAll(result);

        var warning = Assert.Single(Assert.IsType<StreamStartStreamPart>(Assert.Single(parts)).Warnings);
        Assert.Equal("other", warning.Type);
    }
}
