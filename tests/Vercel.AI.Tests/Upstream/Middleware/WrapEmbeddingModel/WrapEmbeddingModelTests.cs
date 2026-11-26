// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.Middleware.WrapEmbeddingModel;

/// <summary>Port of <c>wrap-embedding-model.test.ts</c> &gt; <c>wrapEmbeddingModel</c>.</summary>
public sealed class WrapEmbeddingModelTests
{
    private const string Id =
        "packages/ai/src/middleware/wrap-embedding-model.test.ts::wrapEmbeddingModel::";

    internal static EmbeddingModelCall Params() =>
        new(
            [
                "sunny day at the beach",
                "rainy afternoon in the city",
                "snowy night in the mountains",
            ],
            new Dictionary<string, string>(),
            null,
            CancellationToken.None
        );

    internal static EmbeddingModelCall WithHeader(EmbeddingModelCall call, string name) =>
        new(
            call.Values,
            new Dictionary<string, string>(call.Headers) { [name] = "true" },
            call.ProviderOptions,
            call.CancellationToken,
            call.Dimensions
        );

    [Fact]
    [UpstreamTest(
        Id + "should call transformParams middleware for doEmbed",
        Coverage = UpstreamCoverage.Covered,
        Note = "The upstream transformed: true field becomes a transformed header."
    )]
    public async Task Should_call_transformParams_middleware_for_doEmbed()
    {
        var mockModel = new MockEmbeddingModel();
        var transformCalls = new List<(EmbeddingModelCall Call, IEmbeddingCaller Model)>();
        var wrappedModel = mockModel.WrapEmbeddingModel(
            new LambdaEmbeddingMiddleware
            {
                OnTransformParams = (call, model) =>
                {
                    transformCalls.Add((call, model));
                    return WithHeader(call, "transformed");
                },
            }
        );
        var parameters = Params();

        await wrappedModel.DoEmbedAsync(parameters, CancellationToken.None);

        var transformCall = Assert.Single(transformCalls);
        Assert.Same(parameters, transformCall.Call);
        Assert.NotNull(transformCall.Model);
        var sent = Assert.Single(mockModel.DoEmbedCalls);
        Assert.Equal(parameters.Values, sent.Values);
        Assert.Equal("true", sent.Headers["transformed"]);
    }

    [Fact]
    [UpstreamTest(
        Id + "should call wrapEmbed middleware",
        Coverage = UpstreamCoverage.Covered,
        Note = "The upstream doEmbed function in the context is EmbeddingModelWrapContext.DoEmbedAsync."
    )]
    public async Task Should_call_wrapEmbed_middleware()
    {
        var mockModel = new MockEmbeddingModel
        {
            OnDoEmbed = _ =>
                Task.FromResult(
                    new EmbeddingModelResponse([
                        [1.0],
                    ])
                ),
        };
        var contexts = new List<EmbeddingModelWrapContext>();
        var wrappedModel = mockModel.WrapEmbeddingModel(
            new LambdaEmbeddingMiddleware
            {
                OnWrapEmbed = context =>
                {
                    contexts.Add(context);
                    return context.DoEmbedAsync(CancellationToken.None);
                },
            }
        );
        var parameters = Params();

        await wrappedModel.DoEmbedAsync(parameters, CancellationToken.None);

        var context = Assert.Single(contexts);
        Assert.Same(parameters, context.Call);
        Assert.Same(mockModel, context.Model);
        Assert.Single(mockModel.DoEmbedCalls);
    }
}
