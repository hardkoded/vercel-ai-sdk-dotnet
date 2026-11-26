// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Default = global::Vercel.AI.DefaultEmbeddingSettingsMiddleware;

namespace Vercel.AI.Tests.Upstream.Middleware.DefaultEmbeddingSettingsMiddleware;

/// <summary>Port of <c>default-embedding-settings-middleware.test.ts</c> &gt; <c>providerOptions</c>.</summary>
public sealed class ProviderOptionsTests
{
    private const string Id =
        "packages/ai/src/middleware/default-embedding-settings-middleware.test.ts::providerOptions::";

    private const string Google =
        "{\"google\":{\"outputDimensionality\":512,\"taskType\":\"SEMANTIC_SIMILARITY\"}}";

    private static readonly MockEmbeddingModel MockModel = new();

    private static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private static EmbeddingModelCall Params(JsonElement? providerOptions = null) =>
        new(
            ["hello world"],
            new Dictionary<string, string>(),
            providerOptions,
            CancellationToken.None
        );

    private static void AssertGoogle(JsonElement? actual) =>
        Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(Google), JsonNode.Parse(actual!.Value.GetRawText()))
        );

    [Fact]
    [UpstreamTest(
        Id + "should handle empty default providerOptions",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_empty_default_providerOptions()
    {
        var middleware = new Default(providerOptions: Json("{}"));

        var result = await middleware.TransformParamsAsync(
            Params(Json(Google)),
            MockModel,
            CancellationToken.None
        );

        AssertGoogle(result.ProviderOptions);
    }

    [Fact]
    [UpstreamTest(
        Id + "should handle empty param providerOptions",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_empty_param_providerOptions()
    {
        var middleware = new Default(providerOptions: Json(Google));

        var result = await middleware.TransformParamsAsync(
            Params(Json("{}")),
            MockModel,
            CancellationToken.None
        );

        AssertGoogle(result.ProviderOptions);
    }

    [Fact]
    [UpstreamTest(
        Id + "should handle both providerOptions being undefined",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_both_providerOptions_being_undefined()
    {
        var middleware = new Default();

        var result = await middleware.TransformParamsAsync(
            Params(),
            MockModel,
            CancellationToken.None
        );

        Assert.Null(result.ProviderOptions);
    }
}
