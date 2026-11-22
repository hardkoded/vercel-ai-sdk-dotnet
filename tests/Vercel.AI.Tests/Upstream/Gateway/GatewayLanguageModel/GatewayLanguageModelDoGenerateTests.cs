// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel.GatewayLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel;

/// <summary>Port of <c>gateway-language-model.test.ts</c> &gt; <c>GatewayLanguageModel &gt; doGenerate</c>.</summary>
public sealed class GatewayLanguageModelDoGenerateTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doGenerate::";

    [Fact]
    [UpstreamTest(
        Prefix + "should include o11y headers in the request",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_o11y_headers_in_the_request()
    {
        var server = new GatewayLanguageModelServer
        {
            ContentType = "application/json",
            ResponseBody =
                "{\"id\":\"test-id\",\"created\":1711115037,\"model\":\"test-model\",\"content\":[{\"type\":\"text\",\"text\":\"Hello, World!\"}],\"finish_reason\":\"stop\",\"usage\":{\"prompt_tokens\":4,\"completion_tokens\":30}}",
        };
        var o11yHeaders = new Dictionary<string, string>
        {
            ["ai-o11y-deployment-id"] = "test-deployment",
            ["ai-o11y-environment"] = "production",
            ["ai-o11y-region"] = "iad1",
        };

        await CreateTestModel(server, o11yHeaders)
            .DoGenerateAsync(TestPrompt, CancellationToken.None);

        foreach (var header in o11yHeaders)
        {
            Assert.Equal(header.Value, server.RequestHeaders[header.Key]);
        }
    }

    private static GatewayLanguageModelServer PrepareJsonResponse() =>
        new()
        {
            ContentType = "application/json",
            ResponseBody =
                "{\"id\":\"test-id\",\"created\":1711115037,\"model\":\"test-model\",\"content\":[{\"type\":\"text\",\"text\":\"Test response\"}],\"finish_reason\":\"stop\",\"usage\":{\"prompt_tokens\":4,\"completion_tokens\":30}}",
        };

    [Fact]
    [UpstreamTest(
        Prefix + "should remove abortSignal from the request body",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_remove_abortSignal_from_the_request_body()
    {
        var server = PrepareJsonResponse();
        using var source = new CancellationTokenSource();

        await CreateTestModel(server).DoGenerateAsync(TestPrompt, source.Token);

        Assert.Null(server.RequestJson["abortSignal"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass abortSignal to fetch when provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_abortSignal_to_fetch_when_provided()
    {
        var server = PrepareJsonResponse();
        var tokens = new List<CancellationToken>();
        using var source = new CancellationTokenSource();

        await CreateTestModel(Recording(server, tokens)).DoGenerateAsync(TestPrompt, source.Token);

        Assert.Equal(source.Token, Assert.Single(tokens));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not pass abortSignal to fetch when not provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_pass_abortSignal_to_fetch_when_not_provided()
    {
        var server = PrepareJsonResponse();
        var tokens = new List<CancellationToken>();

        await CreateTestModel(Recording(server, tokens))
            .DoGenerateAsync(TestPrompt, CancellationToken.None);

        Assert.False(Assert.Single(tokens).CanBeCanceled);
    }
}
