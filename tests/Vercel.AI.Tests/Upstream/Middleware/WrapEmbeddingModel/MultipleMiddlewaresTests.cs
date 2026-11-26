// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.Middleware.WrapEmbeddingModel;

/// <summary>Port of <c>wrap-embedding-model.test.ts</c> &gt; <c>wrapEmbeddingModel &gt; multiple middlewares</c>.</summary>
public sealed class MultipleMiddlewaresTests
{
    private const string Id =
        "packages/ai/src/middleware/wrap-embedding-model.test.ts::wrapEmbeddingModel > multiple middlewares::";

    private static JsonElement Text(string text)
    {
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(text));
        return document.RootElement.Clone();
    }

    [Fact]
    [UpstreamTest(
        Id + "should call multiple transformParams middlewares in sequence for doEmbed",
        Coverage = UpstreamCoverage.Covered,
        Note = "The upstream transformationStep1/2 fields become headers."
    )]
    public async Task Should_call_multiple_transformParams_middlewares_in_sequence_for_doEmbed()
    {
        var mockModel = new MockEmbeddingModel();
        var calls1 = new List<EmbeddingModelCall>();
        var calls2 = new List<EmbeddingModelCall>();
        EmbeddingModelCall? step1Result = null;
        var wrappedModel = mockModel.WrapEmbeddingModel(
            new[]
            {
                new LambdaEmbeddingMiddleware
                {
                    OnTransformParams = (call, _) =>
                    {
                        calls1.Add(call);
                        return step1Result = WrapEmbeddingModelTests.WithHeader(
                            call,
                            "transformationStep1"
                        );
                    },
                },
                new LambdaEmbeddingMiddleware
                {
                    OnTransformParams = (call, _) =>
                    {
                        calls2.Add(call);
                        return WrapEmbeddingModelTests.WithHeader(call, "transformationStep2");
                    },
                },
            }
        );
        var parameters = WrapEmbeddingModelTests.Params();

        await wrappedModel.DoEmbedAsync(parameters, CancellationToken.None);

        Assert.Same(parameters, Assert.Single(calls1));
        Assert.Same(step1Result, Assert.Single(calls2));
        Assert.True(calls2[0].Headers.ContainsKey("transformationStep1"));
        var sent = Assert.Single(mockModel.DoEmbedCalls);
        Assert.True(sent.Headers.ContainsKey("transformationStep1"));
        Assert.True(sent.Headers.ContainsKey("transformationStep2"));
    }

    [Fact]
    [UpstreamTest(
        Id + "should chain multiple wrapEmbed middlewares in the correct order",
        Coverage = UpstreamCoverage.Covered,
        Note = "The upstream string results become provider metadata strings on EmbeddingModelResponse."
    )]
    public async Task Should_chain_multiple_wrapEmbed_middlewares_in_the_correct_order()
    {
        var mockModel = new MockEmbeddingModel
        {
            OnDoEmbed = _ =>
                Task.FromResult(
                    new EmbeddingModelResponse([], providerMetadata: Text("final generate result"))
                ),
        };
        var wrapEmbed1Calls = 0;
        var wrapEmbed2Calls = 0;
        var wrappedModel = mockModel.WrapEmbeddingModel(
            new[]
            {
                new LambdaEmbeddingMiddleware
                {
                    OnWrapEmbed = async context =>
                    {
                        wrapEmbed1Calls++;
                        var result = await context.DoEmbedAsync(CancellationToken.None);
                        return new EmbeddingModelResponse(
                            [],
                            providerMetadata: Text(
                                $"wrapEmbed1({result.ProviderMetadata!.Value.GetString()})"
                            )
                        );
                    },
                },
                new LambdaEmbeddingMiddleware
                {
                    OnWrapEmbed = async context =>
                    {
                        wrapEmbed2Calls++;
                        var result = await context.DoEmbedAsync(CancellationToken.None);
                        return new EmbeddingModelResponse(
                            [],
                            providerMetadata: Text(
                                $"wrapEmbed2({result.ProviderMetadata!.Value.GetString()})"
                            )
                        );
                    },
                },
            }
        );

        var result = await wrappedModel.DoEmbedAsync(
            WrapEmbeddingModelTests.Params(),
            CancellationToken.None
        );

        // The middlewares should wrap in order, applying wrapEmbed2 last
        Assert.Equal(
            "wrapEmbed1(wrapEmbed2(final generate result))",
            result.ProviderMetadata!.Value.GetString()
        );
        Assert.Equal(1, wrapEmbed1Calls);
        Assert.Equal(1, wrapEmbed2Calls);
    }

    [Fact]
    [UpstreamTest(
        Id + "should not mutate the middleware array argument",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_not_mutate_the_middleware_array_argument()
    {
        var middleware1 = new LambdaEmbeddingMiddleware();
        var middleware2 = new LambdaEmbeddingMiddleware();
        IEmbeddingModelMiddleware[] middlewares = [middleware1, middleware2];

        new MockEmbeddingModel().WrapEmbeddingModel(middlewares);

        Assert.Equal(2, middlewares.Length);
        Assert.Same(middleware1, middlewares[0]);
        Assert.Same(middleware2, middlewares[1]);
    }
}
