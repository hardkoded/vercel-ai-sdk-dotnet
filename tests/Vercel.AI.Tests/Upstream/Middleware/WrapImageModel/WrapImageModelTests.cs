// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.Middleware.WrapImageModel;

/// <summary>Port of <c>wrap-image-model.test.ts</c> &gt; <c>wrapImageModel</c>.</summary>
public sealed class WrapImageModelTests
{
    private const string Id =
        "packages/ai/src/middleware/wrap-image-model.test.ts::wrapImageModel::";

    internal static ImageModelCall Params() =>
        new(
            "original",
            null,
            null,
            1,
            null,
            null,
            null,
            JsonDocument.Parse("{}").RootElement.Clone(),
            new Dictionary<string, string>(),
            CancellationToken.None
        );

    internal static ImageModelCall With(
        ImageModelCall call,
        string? prompt = null,
        string? header = null
    )
    {
        var headers = new Dictionary<string, string>(call.Headers);
        if (header is not null)
        {
            headers[header] = "true";
        }

        return new ImageModelCall(
            prompt ?? call.Prompt,
            call.Files,
            call.Mask,
            call.N,
            call.Size,
            call.AspectRatio,
            call.Seed,
            call.ProviderOptions,
            headers,
            call.CancellationToken
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should call transformParams middleware for doGenerate",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_transformParams_middleware_for_doGenerate()
    {
        var mockModel = new MockImageModel();
        var transformCalls = new List<(ImageModelCall Call, IImageCaller Model)>();
        var wrappedModel = mockModel.WrapImageModel(
            new LambdaImageMiddleware
            {
                OnTransformParams = (call, model) =>
                {
                    transformCalls.Add((call, model));
                    return With(call, prompt: "transformed");
                },
            }
        );
        var parameters = Params();

        await wrappedModel.DoGenerateAsync(parameters, CancellationToken.None);

        var transformCall = Assert.Single(transformCalls);
        Assert.Same(parameters, transformCall.Call);
        Assert.NotNull(transformCall.Model);
        var captured = Assert.Single(mockModel.DoGenerateCalls);
        Assert.Equal("transformed", captured.Prompt);
        Assert.Equal(parameters.N, captured.N);
        Assert.Equal(parameters.Size, captured.Size);
        Assert.Equal(parameters.AspectRatio, captured.AspectRatio);
        Assert.Equal(parameters.Seed, captured.Seed);
        Assert.Equal(parameters.Files, captured.Files);
        Assert.Equal(parameters.Mask, captured.Mask);
        Assert.Equal(
            parameters.ProviderOptions!.Value.GetRawText(),
            captured.ProviderOptions!.Value.GetRawText()
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should call wrapGenerate middleware",
        Coverage = UpstreamCoverage.Covered,
        Note = "The upstream doGenerate function in the context is ImageModelWrapContext.DoGenerateAsync."
    )]
    public async Task Should_call_wrapGenerate_middleware()
    {
        var mockModel = new MockImageModel();
        var contexts = new List<ImageModelWrapContext>();
        var wrappedModel = mockModel.WrapImageModel(
            new LambdaImageMiddleware
            {
                OnWrapGenerate = context =>
                {
                    contexts.Add(context);
                    return context.DoGenerateAsync(CancellationToken.None);
                },
            }
        );
        var parameters = Params();

        await wrappedModel.DoGenerateAsync(parameters, CancellationToken.None);

        var context = Assert.Single(contexts);
        Assert.Same(parameters, context.Call);
        Assert.Same(mockModel, context.Model);
        Assert.Single(mockModel.DoGenerateCalls);
    }

    [Fact]
    [UpstreamTest(
        Id + "should support models that use \"this\" context in maxImagesPerCall",
        Coverage = UpstreamCoverage.Covered,
        Note = "The upstream function-valued maxImagesPerCall is IImageCaller.ResolveMaxImagesPerCallAsync."
    )]
    public async Task Should_support_models_that_use_this_context_in_maxImagesPerCall()
    {
        var model = new MockImageModelWithThisContext();
        var wrappedModel = model.WrapImageModel(new LambdaImageMiddleware());

        var result = await wrappedModel.ResolveMaxImagesPerCallAsync(CancellationToken.None);

        Assert.Equal(42, result);
        Assert.Same(model, model.ResolveThis);
    }

    private sealed class MockImageModelWithThisContext : MockImageModel
    {
        public int Value { get; } = 42;

        public object? ResolveThis { get; private set; }

        public override Task<int?> ResolveMaxImagesPerCallAsync(CancellationToken cancellationToken)
        {
            ResolveThis = this;
            return Task.FromResult<int?>(Value);
        }
    }
}
