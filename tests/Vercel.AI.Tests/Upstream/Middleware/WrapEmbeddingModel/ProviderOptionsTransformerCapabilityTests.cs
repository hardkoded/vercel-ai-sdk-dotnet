// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.Tests.Upstream.Middleware.WrapEmbeddingModel;

/// <summary>Port of <c>wrap-embedding-model.test.ts</c> &gt; <c>wrapEmbeddingModel &gt; provider options transformer capability</c>.</summary>
public sealed class ProviderOptionsTransformerCapabilityTests
{
    private const string Id =
        "packages/ai/src/middleware/wrap-embedding-model.test.ts::wrapEmbeddingModel > provider options transformer capability::";

    [Fact]
    [UpstreamTest(
        Id + "should pass through by default",
        Coverage = UpstreamCoverage.Partial,
        Note = "Upstream checks the transformer function is the same object. The port cannot compare a method group, so it asserts the wrapper forwards the call to the model's TransformProviderOptionsAsync and returns its result."
    )]
    public async Task Should_pass_through_by_default()
    {
        using var document = JsonDocument.Parse("{\"google\":{\"taskType\":\"X\"}}");
        var transformed = document.RootElement.Clone();
        var calls = new List<(int Start, int End)>();
        var model = new MockEmbeddingModel
        {
            OnTransformProviderOptions = (_, _, start, end) =>
            {
                calls.Add((start, end));
                return transformed;
            },
        };
        var wrappedModel = model.WrapEmbeddingModel(new LambdaEmbeddingMiddleware());

        var result = await wrappedModel.TransformProviderOptionsAsync(
            null,
            ["a", "b"],
            0,
            1,
            CancellationToken.None
        );

        Assert.Equal([(0, 1)], calls);
        Assert.Equal(transformed.GetRawText(), result!.Value.GetRawText());
    }
}
