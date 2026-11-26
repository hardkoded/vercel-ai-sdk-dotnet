// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.Middleware.WrapImageModel;

/// <summary>Port of <c>wrap-image-model.test.ts</c> &gt; <c>wrapImageModel &gt; multiple middlewares</c>.</summary>
public sealed class MultipleMiddlewaresTests
{
    private const string Id =
        "packages/ai/src/middleware/wrap-image-model.test.ts::wrapImageModel > multiple middlewares::";

    private static JsonElement Text(string text)
    {
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(text));
        return document.RootElement.Clone();
    }

    [Fact]
    [UpstreamTest(
        Id + "should call multiple transformParams middlewares in sequence for doGenerate",
        Coverage = UpstreamCoverage.Covered,
        Note = "The upstream transformationStep1/2 fields become headers."
    )]
    public async Task Should_call_multiple_transformParams_middlewares_in_sequence_for_doGenerate()
    {
        var mockModel = new MockImageModel();
        var calls1 = new List<ImageModelCall>();
        var calls2 = new List<ImageModelCall>();
        ImageModelCall? step1Result = null;
        var wrappedModel = mockModel.WrapImageModel(
            new[]
            {
                new LambdaImageMiddleware
                {
                    OnTransformParams = (call, _) =>
                    {
                        calls1.Add(call);
                        return step1Result = WrapImageModelTests.With(
                            call,
                            header: "transformationStep1"
                        );
                    },
                },
                new LambdaImageMiddleware
                {
                    OnTransformParams = (call, _) =>
                    {
                        calls2.Add(call);
                        return WrapImageModelTests.With(call, header: "transformationStep2");
                    },
                },
            }
        );
        var parameters = WrapImageModelTests.Params();

        await wrappedModel.DoGenerateAsync(parameters, CancellationToken.None);

        Assert.Same(parameters, Assert.Single(calls1));
        Assert.Same(step1Result, Assert.Single(calls2));
        Assert.True(calls2[0].Headers.ContainsKey("transformationStep1"));
        var captured = Assert.Single(mockModel.DoGenerateCalls);
        Assert.True(captured.Headers.ContainsKey("transformationStep1"));
        Assert.True(captured.Headers.ContainsKey("transformationStep2"));
    }

    [Fact]
    [UpstreamTest(
        Id + "should chain multiple wrapGenerate middlewares in the correct order",
        Coverage = UpstreamCoverage.Covered,
        Note = "The upstream wrapped field becomes a provider metadata string on ImageModelResult."
    )]
    public async Task Should_chain_multiple_wrapGenerate_middlewares_in_the_correct_order()
    {
        var mockModel = new MockImageModel();
        var wrapGenerate1Calls = 0;
        var wrapGenerate2Calls = 0;
        var wrappedModel = mockModel.WrapImageModel(
            new[]
            {
                new LambdaImageMiddleware
                {
                    OnWrapGenerate = async context =>
                    {
                        wrapGenerate1Calls++;
                        var result = await context.DoGenerateAsync(CancellationToken.None);
                        var inner = result.ProviderMetadata?.GetString() ?? "result";
                        return new ImageModelResult(
                            result.Images,
                            result.Warnings,
                            providerMetadata: Text($"wrapGenerate1({inner})"),
                            response: result.Response
                        );
                    },
                },
                new LambdaImageMiddleware
                {
                    OnWrapGenerate = async context =>
                    {
                        wrapGenerate2Calls++;
                        var result = await context.DoGenerateAsync(CancellationToken.None);
                        var inner = result.ProviderMetadata?.GetString() ?? "result";
                        return new ImageModelResult(
                            result.Images,
                            result.Warnings,
                            providerMetadata: Text($"wrapGenerate2({inner})"),
                            response: result.Response
                        );
                    },
                },
            }
        );

        var result = await wrappedModel.DoGenerateAsync(
            WrapImageModelTests.Params(),
            CancellationToken.None
        );

        Assert.Equal(
            "wrapGenerate1(wrapGenerate2(result))",
            result.ProviderMetadata!.Value.GetString()
        );
        Assert.Equal(1, wrapGenerate1Calls);
        Assert.Equal(1, wrapGenerate2Calls);
    }

    [Fact]
    [UpstreamTest(
        Id + "should not mutate the middleware array argument",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_not_mutate_the_middleware_array_argument()
    {
        var middleware1 = new LambdaImageMiddleware();
        var middleware2 = new LambdaImageMiddleware();
        IImageModelMiddleware[] middlewares = [middleware1, middleware2];

        new MockImageModel().WrapImageModel(middlewares);

        Assert.Equal(2, middlewares.Length);
        Assert.Same(middleware1, middlewares[0]);
        Assert.Same(middleware2, middlewares[1]);
    }
}
