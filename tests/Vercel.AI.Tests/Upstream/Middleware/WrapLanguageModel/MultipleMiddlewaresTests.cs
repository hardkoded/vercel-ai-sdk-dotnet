// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Testing;
using static Vercel.AI.Tests.Upstream.Middleware.WrapLanguageModel.WrapLanguageModelTestSupport;

namespace Vercel.AI.Tests.Upstream.Middleware.WrapLanguageModel;

/// <summary>Port of <c>wrap-language-model.test.ts</c> &gt; <c>wrapLanguageModel &gt; multiple middlewares</c>.</summary>
public sealed class MultipleMiddlewaresTests
{
    private const string Id =
        "packages/ai/src/middleware/wrap-language-model.test.ts::wrapLanguageModel > multiple middlewares::";

    private static HookMiddleware Marking(
        Action<LanguageModelCallOptions> mark,
        List<(
            LanguageModelCallOptions Options,
            LanguageModelCallType Type,
            ILanguageModel Model
        )> calls
    ) => new() { OnTransformParams = Transform(mark, calls) };

    [Fact]
    [UpstreamTest(
        Id + "should call multiple transformParams middlewares in sequence for doGenerate",
        Coverage = UpstreamCoverage.Covered,
        Note = "The extra flags of the upstream params are replaced by Temperature (step 1) and MaxOutputTokens (step 2)."
    )]
    public async Task Should_call_multiple_transformParams_middlewares_in_sequence_for_doGenerate()
    {
        var mockModel = new TestLanguageModel();
        var calls1 = new List<(LanguageModelCallOptions, LanguageModelCallType, ILanguageModel)>();
        var calls2 = new List<(LanguageModelCallOptions, LanguageModelCallType, ILanguageModel)>();
        var wrappedModel = mockModel.WrapLanguageModel(
            Marking(options => options.Temperature = 0.5, calls1),
            Marking(options => options.MaxOutputTokens = 100, calls2)
        );
        var parameters = HelloParams();

        await wrappedModel.DoGenerateAsync(parameters, CancellationToken.None);

        var call1 = Assert.Single(calls1);
        Assert.Same(parameters, call1.Item1);
        Assert.Equal(LanguageModelCallType.Generate, call1.Item2);
        Assert.NotNull(call1.Item3);

        var call2 = Assert.Single(calls2);
        Assert.NotSame(parameters, call2.Item1);
        Assert.Same(parameters.Prompt, call2.Item1.Prompt);
        Assert.Equal(0.5, call2.Item1.Temperature);
        Assert.Null(call2.Item1.MaxOutputTokens);
        Assert.Equal(LanguageModelCallType.Generate, call2.Item2);

        var modelCall = Assert.Single(mockModel.Calls);
        Assert.Equal(0.5, modelCall.Temperature);
        Assert.Equal(100, modelCall.MaxOutputTokens);
    }

    [Fact]
    [UpstreamTest(
        Id + "should call multiple transformParams middlewares in sequence for doStream",
        Coverage = UpstreamCoverage.Covered,
        Note = "The extra flags of the upstream params are replaced by Temperature (step 1) and MaxOutputTokens (step 2)."
    )]
    public async Task Should_call_multiple_transformParams_middlewares_in_sequence_for_doStream()
    {
        var mockModel = new TestLanguageModel();
        var calls1 = new List<(LanguageModelCallOptions, LanguageModelCallType, ILanguageModel)>();
        var calls2 = new List<(LanguageModelCallOptions, LanguageModelCallType, ILanguageModel)>();
        var wrappedModel = mockModel.WrapLanguageModel(
            Marking(options => options.Temperature = 0.5, calls1),
            Marking(options => options.MaxOutputTokens = 100, calls2)
        );
        var parameters = HelloParams();

        await wrappedModel.DoStreamAsync(parameters, CancellationToken.None);

        var call1 = Assert.Single(calls1);
        Assert.Same(parameters, call1.Item1);
        Assert.Equal(LanguageModelCallType.Stream, call1.Item2);
        Assert.NotNull(call1.Item3);

        var call2 = Assert.Single(calls2);
        Assert.Equal(0.5, call2.Item1.Temperature);
        Assert.Equal(LanguageModelCallType.Stream, call2.Item2);
        Assert.Same(mockModel, call2.Item3);

        var modelCall = Assert.Single(mockModel.Calls);
        Assert.Equal(0.5, modelCall.Temperature);
        Assert.Equal(100, modelCall.MaxOutputTokens);
    }

    [Fact]
    [UpstreamTest(
        Id + "should chain multiple wrapGenerate middlewares in the correct order",
        Coverage = UpstreamCoverage.Covered,
        Note = "The string results are carried in the result's response id."
    )]
    public async Task Should_chain_multiple_wrapGenerate_middlewares_in_the_correct_order()
    {
        var mockModel = new TestLanguageModel
        {
            OnGenerate = _ => GenerateResult("final generate result"),
        };
        var called = new List<string>();
        HookMiddleware Wrap(string name) =>
            new()
            {
                OnWrapGenerate = async context =>
                {
                    called.Add(name);
                    var result = await context.DoGenerateAsync(CancellationToken.None);
                    return GenerateResult($"{name}({result.ResponseId})");
                },
            };
        var wrappedModel = mockModel.WrapLanguageModel(
            Wrap("wrapGenerate1"),
            Wrap("wrapGenerate2")
        );

        var result = await wrappedModel.DoGenerateAsync(HelloParams(), CancellationToken.None);

        // The middlewares should wrap in order, applying wrapGenerate2 last
        Assert.Equal("wrapGenerate1(wrapGenerate2(final generate result))", result.ResponseId);
        Assert.Equal(["wrapGenerate1", "wrapGenerate2"], called);
    }

    [Fact]
    [UpstreamTest(
        Id + "should chain multiple wrapStream middlewares in the correct order",
        Coverage = UpstreamCoverage.Covered,
        Note = "The string results are carried in a response header."
    )]
    public async Task Should_chain_multiple_wrapStream_middlewares_in_the_correct_order()
    {
        var called = new List<string>();
        HookMiddleware Wrap(string name) =>
            new()
            {
                OnWrapStream = async context =>
                {
                    called.Add(name);
                    var result = await context.DoStreamAsync(CancellationToken.None);
                    return StreamResult(
                        $"{name}({result.Response?.Headers.GetValueOrDefault("marker")})"
                    );
                },
            };
        // The innermost model returns a fixed stream result.
        var innerModel = new FixedStreamModel(StreamResult("final stream result"));
        var wrappedModel = innerModel.WrapLanguageModel(Wrap("wrapStream1"), Wrap("wrapStream2"));

        var result = await wrappedModel.DoStreamAsync(HelloParams(), CancellationToken.None);

        // The middlewares should wrap in order, applying wrapStream2 last
        Assert.Equal(
            "wrapStream1(wrapStream2(final stream result))",
            result.Response?.Headers["marker"]
        );
        Assert.Equal(["wrapStream1", "wrapStream2"], called);
    }

    [Fact]
    [UpstreamTest(
        Id + "should not mutate the middleware array argument",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_not_mutate_the_middleware_array_argument()
    {
        var middleware1 = new HookMiddleware
        {
            OnWrapStream = _ => throw new NotSupportedException(),
        };
        var middleware2 = new HookMiddleware
        {
            OnWrapStream = _ => throw new NotSupportedException(),
        };
        IReadOnlyList<ILanguageModelMiddleware> middlewares = [middleware1, middleware2];

        new TestLanguageModel().WrapLanguageModel(middlewares);

        Assert.Equal(2, middlewares.Count);
        Assert.Same(middleware1, middlewares[0]);
        Assert.Same(middleware2, middlewares[1]);
    }

    private sealed class FixedStreamModel(LanguageModelStreamResult result) : ILanguageModel
    {
        public string SpecificationVersion => "V4";

        public string Provider => "test";

        public string ModelId => "test";

        public IReadOnlyDictionary<string, string> SupportedUrls =>
            Vercel.AI.ProviderUtils.UrlSupport.None;

        public Task<LanguageModelGenerateResult> DoGenerateAsync(
            LanguageModelCallOptions options,
            CancellationToken cancellationToken
        ) => throw new NotSupportedException();

        public Task<LanguageModelStreamResult> DoStreamAsync(
            LanguageModelCallOptions options,
            CancellationToken cancellationToken
        ) => Task.FromResult(result);
    }
}
