// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Testing;
using static Vercel.AI.Tests.Upstream.Middleware.WrapLanguageModel.WrapLanguageModelTestSupport;

namespace Vercel.AI.Tests.Upstream.Middleware.WrapLanguageModel;

/// <summary>Port of <c>wrap-language-model.test.ts</c> &gt; <c>wrapLanguageModel</c>.</summary>
public sealed class WrapLanguageModelTests
{
    private const string Id =
        "packages/ai/src/middleware/wrap-language-model.test.ts::wrapLanguageModel::";

    [Fact]
    [UpstreamTest(
        Id + "should call transformParams middleware for doGenerate",
        Coverage = UpstreamCoverage.Covered,
        Note = "The extra 'transformed' flag of the upstream params is replaced by Temperature."
    )]
    public async Task Should_call_transformParams_middleware_for_doGenerate()
    {
        var mockModel = new TestLanguageModel();
        var calls = new List<(LanguageModelCallOptions, LanguageModelCallType, ILanguageModel)>();
        var wrappedModel = mockModel.WrapLanguageModel(
            new HookMiddleware { OnTransformParams = Transform(o => o.Temperature = 0.5, calls) }
        );
        var parameters = HelloParams();

        await wrappedModel.DoGenerateAsync(parameters, CancellationToken.None);

        var call = Assert.Single(calls);
        Assert.Same(parameters, call.Item1);
        Assert.Equal(LanguageModelCallType.Generate, call.Item2);
        Assert.NotNull(call.Item3);

        var modelCall = Assert.Single(mockModel.Calls);
        Assert.Same(parameters.Prompt, modelCall.Prompt);
        Assert.Equal(0.5, modelCall.Temperature);
    }

    [Fact]
    [UpstreamTest(
        Id + "should call wrapGenerate middleware",
        Coverage = UpstreamCoverage.Covered,
        Note = "The doGenerate and doStream functions of the upstream argument are the DoGenerateAsync and DoStreamAsync methods of the context."
    )]
    public async Task Should_call_wrapGenerate_middleware()
    {
        var mockModel = new TestLanguageModel();
        var contexts = new List<LanguageModelWrapContext>();
        var wrappedModel = mockModel.WrapLanguageModel(
            new HookMiddleware
            {
                OnWrapGenerate = context =>
                {
                    contexts.Add(context);
                    return context.DoGenerateAsync(CancellationToken.None);
                },
            }
        );
        var parameters = HelloParams();

        await wrappedModel.DoGenerateAsync(parameters, CancellationToken.None);

        var context = Assert.Single(contexts);
        Assert.Same(parameters, context.Options);
        Assert.Same(mockModel, context.Model);
        Assert.Same(parameters, Assert.Single(mockModel.Calls));
    }

    [Fact]
    [UpstreamTest(
        Id + "should call transformParams middleware for doStream",
        Coverage = UpstreamCoverage.Covered,
        Note = "The extra 'transformed' flag of the upstream params is replaced by Temperature."
    )]
    public async Task Should_call_transformParams_middleware_for_doStream()
    {
        var mockModel = new TestLanguageModel();
        var calls = new List<(LanguageModelCallOptions, LanguageModelCallType, ILanguageModel)>();
        var wrappedModel = mockModel.WrapLanguageModel(
            new HookMiddleware { OnTransformParams = Transform(o => o.Temperature = 0.5, calls) }
        );
        var parameters = HelloParams();

        await wrappedModel.DoStreamAsync(parameters, CancellationToken.None);

        var call = Assert.Single(calls);
        Assert.Same(parameters, call.Item1);
        Assert.Equal(LanguageModelCallType.Stream, call.Item2);
        Assert.NotNull(call.Item3);

        var modelCall = Assert.Single(mockModel.Calls);
        Assert.Same(parameters.Prompt, modelCall.Prompt);
        Assert.Equal(0.5, modelCall.Temperature);
    }

    [Fact]
    [UpstreamTest(
        Id + "should call wrapStream middleware",
        Coverage = UpstreamCoverage.Covered,
        Note = "The doGenerate and doStream functions of the upstream argument are the DoGenerateAsync and DoStreamAsync methods of the context."
    )]
    public async Task Should_call_wrapStream_middleware()
    {
        var mockModel = new TestLanguageModel();
        var contexts = new List<LanguageModelWrapContext>();
        var wrappedModel = mockModel.WrapLanguageModel(
            new HookMiddleware
            {
                OnWrapStream = context =>
                {
                    contexts.Add(context);
                    return context.DoStreamAsync(CancellationToken.None);
                },
            }
        );
        var parameters = HelloParams();

        await wrappedModel.DoStreamAsync(parameters, CancellationToken.None);

        var context = Assert.Single(contexts);
        Assert.Same(parameters, context.Options);
        Assert.Same(mockModel, context.Model);
        Assert.Same(parameters, Assert.Single(mockModel.Calls));
    }

    [Fact]
    [UpstreamTest(
        Id + "should support models that use \"this\" context in supportedUrls",
        Coverage = UpstreamCoverage.Partial,
        Note = "JavaScript 'this' binding of a getter has no .NET equivalent. Checks that a getter that reads an instance field is read through the wrapper."
    )]
    public void Should_support_models_that_use_this_context_in_supportedUrls()
    {
        var model = new ModelWithThisContext();

        var wrappedModel = model.WrapLanguageModel(new HookMiddleware());

        Assert.Equal(model.Value, wrappedModel.SupportedUrls);
        Assert.True(model.SupportedUrlsCalled);
    }

    private sealed class ModelWithThisContext : ILanguageModel
    {
        public bool SupportedUrlsCalled { get; private set; }

        public Dictionary<string, string> Value { get; } = new() { ["image/*"] = "^https://.*$" };

        public string SpecificationVersion => "V4";

        public string Provider => "test-provider";

        public string ModelId => "test-model";

        public IReadOnlyDictionary<string, string> SupportedUrls
        {
            get
            {
                SupportedUrlsCalled = true;
                // Reference the instance to verify context
                return Value;
            }
        }

        public Task<LanguageModelGenerateResult> DoGenerateAsync(
            LanguageModelCallOptions options,
            CancellationToken cancellationToken
        ) => throw new NotSupportedException();

        public Task<LanguageModelStreamResult> DoStreamAsync(
            LanguageModelCallOptions options,
            CancellationToken cancellationToken
        ) => throw new NotSupportedException();
    }
}
