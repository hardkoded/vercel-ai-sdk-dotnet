// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests;

public sealed class MiddlewareWrapTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/middleware/wrap-language-model.test.ts::wrapLanguageModel > model property::should pass through by default",
        Coverage = UpstreamCoverage.Covered)]
    public void Wrapped_model_keeps_the_inner_model_id()
    {
        var wrapped = new TestLanguageModel("test-model").WrapLanguageModel(new PassThroughMiddleware());
        Assert.Equal("test-model", wrapped.ModelId);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/middleware/wrap-language-model.test.ts::wrapLanguageModel > provider property::should pass through by default",
        Coverage = UpstreamCoverage.Covered)]
    public void Wrapped_model_keeps_the_inner_provider()
    {
        var wrapped = new TestLanguageModel("test-model").WrapLanguageModel(new PassThroughMiddleware());
        Assert.Equal("test", wrapped.Provider);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/middleware/wrap-language-model.test.ts::wrapLanguageModel::should call wrapGenerate middleware",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Wrap_generate_runs_and_forwards_the_call()
    {
        var inner = new TestLanguageModel("test-model");
        var middleware = new RecordingMiddleware();
        var wrapped = inner.WrapLanguageModel(middleware);
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
        };

        var result = await wrapped.DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal(1, middleware.GenerateCalls);
        Assert.Same(options, Assert.Single(inner.Calls));
        Assert.Equal("ok", result.Text);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/middleware/wrap-language-model.test.ts::wrapLanguageModel::should call wrapStream middleware",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Wrap_stream_runs_and_forwards_the_call()
    {
        var inner = new TestLanguageModel("test-model");
        var middleware = new RecordingMiddleware();
        var wrapped = inner.WrapLanguageModel(middleware);
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
        };

        var deltas = new List<string>();
        await foreach (var part in wrapped.DoStreamAsync(options, CancellationToken.None))
        {
            if (part is TextDeltaStreamPart delta)
            {
                deltas.Add(delta.Delta);
            }
        }

        Assert.Equal(1, middleware.StreamCalls);
        Assert.Same(options, Assert.Single(inner.Calls));
        Assert.Equal(new[] { "ok" }, deltas);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/middleware/wrap-language-model.test.ts::wrapLanguageModel > multiple middlewares::should chain multiple wrapGenerate middlewares in the correct order",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Wrap_generate_applies_the_first_middleware_outside()
    {
        var wrapped = new TestLanguageModel().WrapLanguageModel(
            new PrefixMiddleware("wrapGenerate1"),
            new PrefixMiddleware("wrapGenerate2"));
        var result = await wrapped.DoGenerateAsync(new LanguageModelCallOptions(), CancellationToken.None);
        Assert.Equal("wrapGenerate1(wrapGenerate2(ok))", result.Text);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/middleware/wrap-language-model.test.ts::wrapLanguageModel > multiple middlewares::should chain multiple wrapStream middlewares in the correct order",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Wrap_stream_applies_the_first_middleware_outside()
    {
        var wrapped = new TestLanguageModel().WrapLanguageModel(
            new PrefixMiddleware("wrapStream1"),
            new PrefixMiddleware("wrapStream2"));
        var deltas = new List<string>();
        await foreach (var part in wrapped.DoStreamAsync(new LanguageModelCallOptions(), CancellationToken.None))
        {
            if (part is TextDeltaStreamPart delta)
            {
                deltas.Add(delta.Delta);
            }
        }

        Assert.Equal(new[] { "wrapStream1(wrapStream2(ok))" }, deltas);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/middleware/wrap-language-model.test.ts::wrapLanguageModel > multiple middlewares::should not mutate the middleware array argument",
        Coverage = UpstreamCoverage.Covered)]
    public void Wrap_does_not_reorder_the_middleware_array()
    {
        var first = new PassThroughMiddleware();
        var second = new PassThroughMiddleware();
        var middleware = new ILanguageModelMiddleware[] { first, second };
        var wrapped = new TestLanguageModel("test-model").WrapLanguageModel(middleware);
        Assert.Equal("test-model", wrapped.ModelId);
        Assert.Equal(2, middleware.Length);
        Assert.Same(first, middleware[0]);
        Assert.Same(second, middleware[1]);
    }

    private sealed class PassThroughMiddleware : LanguageModelMiddleware
    {
    }

    private sealed class RecordingMiddleware : LanguageModelMiddleware
    {
        public int GenerateCalls { get; private set; }

        public int StreamCalls { get; private set; }

        public override async Task<LanguageModelGenerateResult> WrapGenerateAsync(
            LanguageModelCallOptions options,
            Func<LanguageModelCallOptions, CancellationToken, Task<LanguageModelGenerateResult>> next,
            CancellationToken cancellationToken)
        {
            GenerateCalls++;
            return await next(options, cancellationToken);
        }

        public override async IAsyncEnumerable<LanguageModelStreamPart> WrapStreamAsync(
            LanguageModelCallOptions options,
            Func<LanguageModelCallOptions, CancellationToken, IAsyncEnumerable<LanguageModelStreamPart>> next,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            StreamCalls++;
            await foreach (var part in next(options, cancellationToken))
            {
                yield return part;
            }
        }
    }

    private sealed class PrefixMiddleware : LanguageModelMiddleware
    {
        private readonly string _name;

        public PrefixMiddleware(string name)
        {
            _name = name;
        }

        public override async Task<LanguageModelGenerateResult> WrapGenerateAsync(
            LanguageModelCallOptions options,
            Func<LanguageModelCallOptions, CancellationToken, Task<LanguageModelGenerateResult>> next,
            CancellationToken cancellationToken)
        {
            var result = await next(options, cancellationToken);
            var content = new List<GeneratedContent>();
            foreach (var part in result.Content)
            {
                if (part is GeneratedText text)
                {
                    content.Add(new GeneratedText(_name + "(" + text.Text + ")"));
                }
                else
                {
                    content.Add(part);
                }
            }

            return new LanguageModelGenerateResult(content, result.FinishReason, result.Usage, result.RawFinishReason);
        }

        public override async IAsyncEnumerable<LanguageModelStreamPart> WrapStreamAsync(
            LanguageModelCallOptions options,
            Func<LanguageModelCallOptions, CancellationToken, IAsyncEnumerable<LanguageModelStreamPart>> next,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var part in next(options, cancellationToken))
            {
                if (part is TextDeltaStreamPart delta)
                {
                    yield return new TextDeltaStreamPart(delta.Id, _name + "(" + delta.Delta + ")");
                }
                else
                {
                    yield return part;
                }
            }
        }
    }
}
