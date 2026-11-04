// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateText.ToolExecutionCallbackSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>
/// Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; errors</c>.
/// </summary>
public sealed class ErrorsTests
{
    private const string Prefix = "packages/ai/src/generate-text/stream-text.test.ts::streamText > errors::";

    private const string NoFinishMessage = "No output generated. The model stream ended without a finish chunk.";

    [Fact]
    [UpstreamTest(Prefix + "should swallow error to prevent server crash", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_swallow_error_to_prevent_server_crash()
    {
        var result = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = ThrowingModel(),
            Prompt = "test-input",
            OnError = (_, _) => Task.CompletedTask,
        });

        var text = new List<string>();
        await foreach (var delta in result.TextStream())
        {
            text.Add(delta);
        }

        Assert.Empty(text);
    }

    [Fact]
    [UpstreamTest(Prefix + "should forward error in doStream as error stream part", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_forward_error_in_doStream_as_error_stream_part()
    {
        var result = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = ThrowingModel(),
            Prompt = "test-input",
            OnError = (_, _) => Task.CompletedTask,
        });

        var parts = new List<TextStreamPart>();
        await foreach (var part in result.Stream())
        {
            parts.Add(part);
        }

        Assert.Collection(
            parts,
            part => Assert.IsType<StartPart>(part),
            part => Assert.Equal("test error", Assert.IsType<ErrorPart>(part).Message));
    }

    [Fact]
    [UpstreamTest(Prefix + "should invoke onError callback when error is thrown", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_invoke_onError_callback_when_error_is_thrown()
    {
        var errors = new List<Exception>();
        var result = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = ThrowingModel(),
            Prompt = "test-input",
            OnError = (error, _) =>
            {
                errors.Add(error);
                return Task.CompletedTask;
            },
        });

        await ConsumeAsync(result);

        Assert.Equal("test error", Assert.Single(errors).Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should call onFinish even when error chunk occurs mid-stream", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_call_onFinish_even_when_error_chunk_occurs_mid_stream()
    {
        var errors = new List<Exception>();
        GenerateTextResult? finished = null;
        var result = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = new Vercel.AI.Tests.StreamText.StreamTextAbortSupport.ScriptedStreamModel(ErrorPartStream),
            Prompt = "test-input",
            OnError = (error, _) =>
            {
                errors.Add(error);
                return Task.CompletedTask;
            },
            OnFinish = (value, _) =>
            {
                finished = value;
                return Task.CompletedTask;
            },
        });

        await ConsumeAsync(result);

        Assert.Equal("chunk error", Assert.Single(errors).Message);
        Assert.NotNull(finished);
        Assert.Equal(FinishReason.Error, finished.FinishReason);
        Assert.Equal("Hello", finished.Text);
        Assert.Equal(3, finished.Usage.InputTokens);
        Assert.Equal(10, finished.Usage.OutputTokens);
        Assert.Equal(13, finished.Usage.TotalTokens);
    }

    [Fact]
    [UpstreamTest(Prefix + "should invoke onError callback when error is thrown in 2nd step", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_invoke_onError_callback_when_error_is_thrown_in_2nd_step()
    {
        var errors = new List<Exception>();
        var responseCount = 0;
        var model = new MockLanguageModel((_, _) =>
        {
            if (responseCount++ == 0)
            {
                return Task.FromResult(new LanguageModelStreamResult(ToolCallStream(("call-1", """{ "value": "value" }"""))));
            }

            throw new InvalidOperationException("test error");
        });
        var result = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Tools = new[] { Tool1(execute: (_, _) => Task.FromResult("result1")) },
            StopWhen = StopWhen.IsStepCount(3),
            OnError = (error, _) =>
            {
                errors.Add(error);
                return Task.CompletedTask;
            },
        });

        await ConsumeAsync(result);

        Assert.Equal("test error", Assert.Single(errors).Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should reject text promise when error is thrown", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_reject_text_promise_when_error_is_thrown()
    {
        var result = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = ThrowingModel(),
            Prompt = "test-input",
            OnError = (_, _) => Task.CompletedTask,
        });

        var error = await Assert.ThrowsAsync<NoOutputGeneratedException>(() => result.Text);

        Assert.Equal("No output generated. Check the stream for errors.", error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should reject when provider stream closes before finish chunk", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_reject_when_provider_stream_closes_before_finish_chunk()
    {
        var errors = new List<Exception>();
        var stepFinishes = new List<StepResult>();

        var result = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = new Vercel.AI.Tests.StreamText.StreamTextAbortSupport.ScriptedStreamModel(MetadataOnlyStream),
            Prompt = "test-input",
            OnError = (error, _) =>
            {
                errors.Add(error);
                return Task.CompletedTask;
            },
            OnStepEnd = (step, _) =>
            {
                stepFinishes.Add(step);
                return Task.CompletedTask;
            },
        });

        await ConsumeIgnoringErrorsAsync(result);

        AssertNoFinish(await Assert.ThrowsAsync<NoOutputGeneratedException>(() => result.Text));
        AssertNoFinish(await Assert.ThrowsAsync<NoOutputGeneratedException>(() => result.Steps));
        AssertNoFinish(await Assert.ThrowsAsync<NoOutputGeneratedException>(() => result.FinishReason));
        AssertNoFinish(await Assert.ThrowsAsync<NoOutputGeneratedException>(() => result.Usage));
        AssertNoFinish(Assert.IsType<NoOutputGeneratedException>(Assert.Single(errors)));
        Assert.Empty(stepFinishes);
    }

    [Fact]
    [UpstreamTest(Prefix + "should resolve with partial output when provider stream closes before finish chunk after producing output", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_resolve_with_partial_output_when_provider_stream_closes_before_finish_chunk_after_producing_output()
    {
        var errors = new List<Exception>();

        var result = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = new Vercel.AI.Tests.StreamText.StreamTextAbortSupport.ScriptedStreamModel(PartialOutputStream),
            Prompt = "test-input",
            OnError = (error, _) =>
            {
                errors.Add(error);
                return Task.CompletedTask;
            },
        });

        await ConsumeIgnoringErrorsAsync(result);

        Assert.Equal("Hello, world", await result.Text);
        Assert.Equal(FinishReason.Other, await result.FinishReason);
        Assert.Single(await result.Steps);
        Assert.Empty(errors);
    }

    [Fact]
    [UpstreamTest(Prefix + "should reject result promises when provider stream errors after metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_reject_result_promises_when_provider_stream_errors_after_metadata()
    {
        var consumeErrors = new List<Exception>();

        var result = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = new Vercel.AI.Tests.StreamText.StreamTextAbortSupport.ScriptedStreamModel(ErroringStream),
            Prompt = "test-input",
            OnError = (_, _) => Task.CompletedTask,
        });

        await ConsumeIgnoringErrorsAsync(result, consumeErrors);

        Assert.Equal("simulated provider stream error", (await Assert.ThrowsAsync<InvalidOperationException>(() => result.Text)).Message);
        Assert.Equal("simulated provider stream error", (await Assert.ThrowsAsync<InvalidOperationException>(() => result.Steps)).Message);
        Assert.Equal("simulated provider stream error", (await Assert.ThrowsAsync<InvalidOperationException>(() => result.FinishReason)).Message);
        Assert.Equal("simulated provider stream error", (await Assert.ThrowsAsync<InvalidOperationException>(() => result.Usage)).Message);
        Assert.Equal("simulated provider stream error", Assert.IsType<InvalidOperationException>(Assert.Single(consumeErrors)).Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should reject when provider stream is incomplete on a continuation step", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_reject_when_provider_stream_is_incomplete_on_a_continuation_step()
    {
        var errors = new List<Exception>();
        var stepFinishes = new List<StepResult>();
        var responseCount = 0;

        var result = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = new Vercel.AI.Tests.StreamText.StreamTextAbortSupport.ScriptedStreamModel(() => ++responseCount == 1 ? ToolCallStream(("call-1", "{ \"value\": \"a\" }")) : MetadataOnlyStream()),
            Prompt = "test-input",
            Tools = new[] { Tool1() },
            StopWhen = StopWhen.IsStepCount(3),
            OnError = (error, _) =>
            {
                errors.Add(error);
                return Task.CompletedTask;
            },
            OnStepEnd = (step, _) =>
            {
                stepFinishes.Add(step);
                return Task.CompletedTask;
            },
        });

        await ConsumeIgnoringErrorsAsync(result);

        AssertNoFinish(await Assert.ThrowsAsync<NoOutputGeneratedException>(() => result.Steps));
        AssertNoFinish(await Assert.ThrowsAsync<NoOutputGeneratedException>(() => result.FinishReason));
        AssertNoFinish(await Assert.ThrowsAsync<NoOutputGeneratedException>(() => result.Usage));
        AssertNoFinish(Assert.IsType<NoOutputGeneratedException>(Assert.Single(errors)));
        Assert.Single(stepFinishes);
    }

    private static void AssertNoFinish(Exception error)
    {
        Assert.Equal(NoFinishMessage, error.Message);
    }

    // Reads the stream to its end and keeps the error that ends it, as upstream consumeStream does.
    private static async Task ConsumeIgnoringErrorsAsync(StreamTextResult result, List<Exception>? errors = null)
    {
        try
        {
            await ConsumeAsync(result);
        }
        catch (Exception error)
        {
            errors?.Add(error);
        }
    }

    private static async IAsyncEnumerable<LanguageModelStreamPart> MetadataOnlyStream()
    {
        yield return new StreamStartStreamPart(null);
        yield return new ResponseMetadataStreamPart("id-0", "mock-model-id", DateTimeOffset.FromUnixTimeMilliseconds(0));
        await Task.CompletedTask;
    }

    // The stream is cut short: no text-end and no finish chunk.
    private static async IAsyncEnumerable<LanguageModelStreamPart> PartialOutputStream()
    {
        yield return new StreamStartStreamPart(null);
        yield return new ResponseMetadataStreamPart("id-0", "mock-model-id", DateTimeOffset.FromUnixTimeMilliseconds(0));
        yield return new TextStartStreamPart("1");
        yield return new TextDeltaStreamPart("1", "Hello");
        yield return new TextDeltaStreamPart("1", ", world");
        await Task.CompletedTask;
    }

    private static async IAsyncEnumerable<LanguageModelStreamPart> ErroringStream()
    {
        yield return new StreamStartStreamPart(null);
        yield return new ResponseMetadataStreamPart("id-0", "mock-model-id", DateTimeOffset.FromUnixTimeMilliseconds(0));
        await Task.Yield();
        throw new InvalidOperationException("simulated provider stream error");
    }

    private static async IAsyncEnumerable<LanguageModelStreamPart> ErrorPartStream()
    {
        yield return new ResponseMetadataStreamPart("id-0", "mock-model-id", DateTimeOffset.FromUnixTimeMilliseconds(0));
        yield return new TextStartStreamPart("1");
        yield return new TextDeltaStreamPart("1", "Hello");
        yield return new ErrorStreamPart("chunk error");
        yield return new FinishStreamPart(FinishReason.Error, new LanguageModelUsage(3, 10, 13));
        await Task.CompletedTask;
    }

    private static MockLanguageModel ThrowingModel()
    {
        return new MockLanguageModel((_, _) => throw new InvalidOperationException("test error"));
    }

    private sealed class MockLanguageModel(
        Func<LanguageModelCallOptions, CancellationToken, Task<LanguageModelStreamResult>> doStream) : ILanguageModel
    {
        public string SpecificationVersion => "V4";

        public string Provider => "test";

        public string ModelId => "mock-model-id";

        public Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<LanguageModelStreamResult> DoStreamAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
        {
            return doStream(options, cancellationToken);
        }
    }
}
