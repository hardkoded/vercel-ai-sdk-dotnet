// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateText.ToolExecutionCallbackSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>
/// Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; errors</c>, the tests for a provider stream that ends before its finish chunk.
/// The other tests of that describe are not in this class yet. See the pull request for the list.
/// </summary>
public sealed class ErrorsTests
{
    private const string Prefix = "packages/ai/src/generate-text/stream-text.test.ts::streamText > errors::";

    private const string NoFinishMessage = "No output generated. The model stream ended without a finish chunk.";

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
}
