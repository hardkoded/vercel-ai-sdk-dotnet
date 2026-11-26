// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Default = global::Vercel.AI.DefaultEmbeddingSettingsMiddleware;

namespace Vercel.AI.Tests.Upstream.Middleware.DefaultEmbeddingSettingsMiddleware;

/// <summary>Port of <c>default-embedding-settings-middleware.test.ts</c> &gt; <c>headers</c>.</summary>
public sealed class HeadersTests
{
    private const string Id =
        "packages/ai/src/middleware/default-embedding-settings-middleware.test.ts::headers::";

    private static readonly MockEmbeddingModel MockModel = new();

    private static EmbeddingModelCall Params(Dictionary<string, string> headers) =>
        new(["hello world"], headers, null, CancellationToken.None);

    [Fact]
    [UpstreamTest(Id + "should merge headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_merge_headers()
    {
        var middleware = new Default(
            new Dictionary<string, string>
            {
                ["X-Custom-Header"] = "test",
                ["X-Another-Header"] = "test2",
            }
        );

        var result = await middleware.TransformParamsAsync(
            Params(new() { ["X-Custom-Header"] = "test2" }),
            MockModel,
            CancellationToken.None
        );

        Assert.Equal(
            new Dictionary<string, string>
            {
                ["X-Custom-Header"] = "test2",
                ["X-Another-Header"] = "test2",
            },
            result.Headers.ToDictionary(p => p.Key, p => p.Value)
        );
    }

    [Fact]
    [UpstreamTest(Id + "should handle empty default headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_empty_default_headers()
    {
        var middleware = new Default(new Dictionary<string, string>());

        var result = await middleware.TransformParamsAsync(
            Params(new() { ["X-Param-Header"] = "param" }),
            MockModel,
            CancellationToken.None
        );

        Assert.Equal(
            new Dictionary<string, string> { ["X-Param-Header"] = "param" },
            result.Headers.ToDictionary(p => p.Key, p => p.Value)
        );
    }

    [Fact]
    [UpstreamTest(Id + "should handle empty param headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_empty_param_headers()
    {
        var middleware = new Default(
            new Dictionary<string, string> { ["X-Default-Header"] = "default" }
        );

        var result = await middleware.TransformParamsAsync(
            Params([]),
            MockModel,
            CancellationToken.None
        );

        Assert.Equal(
            new Dictionary<string, string> { ["X-Default-Header"] = "default" },
            result.Headers.ToDictionary(p => p.Key, p => p.Value)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should handle both headers being undefined",
        Coverage = UpstreamCoverage.Partial,
        Note = "EmbeddingModelCall.Headers is never null (the constructor turns a missing map into an empty one), so the assertion is that no headers result instead of toBeUndefined."
    )]
    public async Task Should_handle_both_headers_being_undefined()
    {
        var middleware = new Default();

        var result = await middleware.TransformParamsAsync(
            new EmbeddingModelCall(["hello world"], null!, null, CancellationToken.None),
            MockModel,
            CancellationToken.None
        );

        Assert.Empty(result.Headers);
    }
}
