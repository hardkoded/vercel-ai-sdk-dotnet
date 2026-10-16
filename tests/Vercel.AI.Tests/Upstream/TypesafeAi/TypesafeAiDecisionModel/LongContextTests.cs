// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.TypeSafeAi;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.TypesafeAi.TypesafeAiDecisionModel.TypesafeAiDecisionModelSupport;

namespace Vercel.AI.Tests.Upstream.TypesafeAi.TypesafeAiDecisionModel;

/// <summary>Port of <c>typesafe-ai-decision-model.test.ts</c> &gt; <c>long context</c>.</summary>
public sealed class LongContextTests
{
    private const string Prefix = "packages/typesafe-ai/src/typesafe-ai-decision-model.test.ts::long context::";

    private static string Options(string longContext) => "{\"typesafe\":{\"longContext\":" + longContext + "}}";

    [Theory]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData(null)]
    [UpstreamTest(Prefix + "enables the header only when longContext is true: %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Enables_the_header_only_when_longContext_is_true(string? longContext)
    {
        var server = Server();
        var result = await CreateProvider(server).DecisionModel("jev-latest").DoDecideAsync(
            Call(providerOptions: longContext == null ? "{\"typesafe\":{}}" : Options(longContext)),
            CancellationToken.None);
        var call = Assert.Single(server.Calls);
        if (longContext == "true")
        {
            Assert.Equal("true", call.Headers["typesafe-danger-zone-long-context"]);
        }
        else
        {
            Assert.False(call.Headers.ContainsKey("typesafe-danger-zone-long-context"));
        }

        Assert.True(JsonNode.DeepEquals(NativeRequest, JsonNode.Parse(call.Body)), call.Body);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "combines long context with image support and warns only for unknown options", Coverage = UpstreamCoverage.Covered)]
    public async Task Combines_long_context_with_image_support_and_warns_only_for_unknown_options()
    {
        var server = Server();
        var result = await CreateProvider(server).DecisionModel("jev-latest").DoDecideAsync(
            Call(
                state: new List<object?> { FilePart("image/png", "iVBORw==") },
                providerOptions: "{\"typesafe\":{\"longContext\":true,\"temperature\":0}}",
                headers: new Dictionary<string, string> { ["x-custom"] = "custom" }),
            CancellationToken.None);
        var call = Assert.Single(server.Calls);
        Assert.Equal("true", call.Headers["typesafe-danger-zone-images"]);
        Assert.Equal("true", call.Headers["typesafe-danger-zone-long-context"]);
        Assert.Equal("custom", call.Headers["x-custom"]);
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("providerOptions.typesafe.temperature", warning.Feature);
    }

    [Theory]
    [InlineData("\"true\"")]
    [InlineData("1")]
    [InlineData("null")]
    [UpstreamTest(Prefix + "rejects invalid longContext values: %j", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_invalid_longContext_values(string longContext)
    {
        var server = Server();
        await Assert.ThrowsAsync<InvalidArgumentError>(() => CreateProvider(server).DecisionModel("jev-latest").DoDecideAsync(
            Call(providerOptions: Options(longContext)),
            CancellationToken.None));
        Assert.Empty(server.Calls);
    }
}
