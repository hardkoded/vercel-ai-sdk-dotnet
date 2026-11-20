// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Operations;
using Vercel.AI.Tests.Upstream.StreamText;
using static Vercel.AI.Tests.Upstream.GenerateText.ToolExecutionCallbackSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>
/// Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; errors</c>.
/// </summary>
public sealed class ErrorsTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > errors::";

    private const string NoFinishMessage =
        "No output generated. The model stream ended without a finish chunk.";

    [Fact]
    [UpstreamTest(
        Prefix + "should swallow error to prevent server crash",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_swallow_error_to_prevent_server_crash()
    {
        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = ThrowingModel(),
                    Prompt = "test-input",
                    OnError = (_, _) => Task.CompletedTask,
                }
            );

        var text = new List<string>();
        await foreach (var delta in result.TextStream())
        {
            text.Add(delta);
        }

        Assert.Empty(text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should forward error in doStream as error stream part",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_forward_error_in_doStream_as_error_stream_part()
    {
        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = ThrowingModel(),
                    Prompt = "test-input",
                    OnError = (_, _) => Task.CompletedTask,
                }
            );

        var parts = new List<TextStreamPart>();
        await foreach (var part in result.Stream())
        {
            parts.Add(part);
        }

        Assert.Collection(
            parts,
            part => Assert.IsType<StartPart>(part),
            part => Assert.Equal("test error", Assert.IsType<ErrorPart>(part).Message)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should invoke onError callback when error is thrown",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_invoke_onError_callback_when_error_is_thrown()
    {
        var errors = new List<Exception>();
        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = ThrowingModel(),
                    Prompt = "test-input",
                    OnError = (error, _) =>
                    {
                        errors.Add(error);
                        return Task.CompletedTask;
                    },
                }
            );

        await ConsumeAsync(result);

        Assert.Equal("test error", Assert.Single(errors).Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should call onFinish even when error chunk occurs mid-stream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_onFinish_even_when_error_chunk_occurs_mid_stream()
    {
        var errors = new List<Exception>();
        GenerateTextResult? finished = null;
        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model =
                        new Vercel.AI.Tests.StreamText.StreamTextAbortSupport.ScriptedStreamModel(
                            ErrorPartStream
                        ),
                    Prompt = "test-input",
                    OnError = (error, _) =>
                    {
                        errors.Add(error);
                        return Task.CompletedTask;
                    },
                    OnEnd = (value, _) =>
                    {
                        finished = value;
                        return Task.CompletedTask;
                    },
                }
            );

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
    [UpstreamTest(
        Prefix + "should invoke onError callback when error is thrown in 2nd step",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_invoke_onError_callback_when_error_is_thrown_in_2nd_step()
    {
        var errors = new List<Exception>();
        var responseCount = 0;
        var model = new MockLanguageModel(
            (_, _) =>
            {
                if (responseCount++ == 0)
                {
                    return Task.FromResult(
                        new LanguageModelStreamResult(
                            ToolCallStream(("call-1", """{ "value": "value" }"""))
                        )
                    );
                }

                throw new InvalidOperationException("test error");
            }
        );
        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
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
                }
            );

        await ConsumeAsync(result);

        Assert.Equal("test error", Assert.Single(errors).Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject text promise when error is thrown",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_reject_text_promise_when_error_is_thrown()
    {
        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = ThrowingModel(),
                    Prompt = "test-input",
                    OnError = (_, _) => Task.CompletedTask,
                }
            );

        var error = await Assert.ThrowsAsync<NoOutputGeneratedException>(() => result.Text);

        Assert.Equal("No output generated. Check the stream for errors.", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject when provider stream closes before finish chunk",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_reject_when_provider_stream_closes_before_finish_chunk()
    {
        var errors = new List<Exception>();
        var stepFinishes = new List<StepResult>();

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model =
                        new Vercel.AI.Tests.StreamText.StreamTextAbortSupport.ScriptedStreamModel(
                            MetadataOnlyStream
                        ),
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
                }
            );

        await ConsumeIgnoringErrorsAsync(result);

        AssertNoFinish(await Assert.ThrowsAsync<NoOutputGeneratedException>(() => result.Text));
        AssertNoFinish(await Assert.ThrowsAsync<NoOutputGeneratedException>(() => result.Steps));
        AssertNoFinish(
            await Assert.ThrowsAsync<NoOutputGeneratedException>(() => result.FinishReason)
        );
        AssertNoFinish(await Assert.ThrowsAsync<NoOutputGeneratedException>(() => result.Usage));
        AssertNoFinish(Assert.IsType<NoOutputGeneratedException>(Assert.Single(errors)));
        Assert.Empty(stepFinishes);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should resolve with partial output when provider stream closes before finish chunk after producing output",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_resolve_with_partial_output_when_provider_stream_closes_before_finish_chunk_after_producing_output()
    {
        var errors = new List<Exception>();

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model =
                        new Vercel.AI.Tests.StreamText.StreamTextAbortSupport.ScriptedStreamModel(
                            PartialOutputStream
                        ),
                    Prompt = "test-input",
                    OnError = (error, _) =>
                    {
                        errors.Add(error);
                        return Task.CompletedTask;
                    },
                }
            );

        await ConsumeIgnoringErrorsAsync(result);

        Assert.Equal("Hello, world", await result.Text);
        Assert.Equal(FinishReason.Other, await result.FinishReason);
        Assert.Single(await result.Steps);
        Assert.Empty(errors);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject result promises when provider stream errors after metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_reject_result_promises_when_provider_stream_errors_after_metadata()
    {
        var consumeErrors = new List<Exception>();

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model =
                        new Vercel.AI.Tests.StreamText.StreamTextAbortSupport.ScriptedStreamModel(
                            ErroringStream
                        ),
                    Prompt = "test-input",
                    OnError = (_, _) => Task.CompletedTask,
                }
            );

        await ConsumeIgnoringErrorsAsync(result, consumeErrors);

        Assert.Equal(
            "simulated provider stream error",
            (await Assert.ThrowsAsync<InvalidOperationException>(() => result.Text)).Message
        );
        Assert.Equal(
            "simulated provider stream error",
            (await Assert.ThrowsAsync<InvalidOperationException>(() => result.Steps)).Message
        );
        Assert.Equal(
            "simulated provider stream error",
            (await Assert.ThrowsAsync<InvalidOperationException>(() => result.FinishReason)).Message
        );
        Assert.Equal(
            "simulated provider stream error",
            (await Assert.ThrowsAsync<InvalidOperationException>(() => result.Usage)).Message
        );
        Assert.Equal(
            "simulated provider stream error",
            Assert.IsType<InvalidOperationException>(Assert.Single(consumeErrors)).Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject when provider stream is incomplete on a continuation step",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_reject_when_provider_stream_is_incomplete_on_a_continuation_step()
    {
        var errors = new List<Exception>();
        var stepFinishes = new List<StepResult>();
        var responseCount = 0;

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model =
                        new Vercel.AI.Tests.StreamText.StreamTextAbortSupport.ScriptedStreamModel(
                            () =>
                                ++responseCount == 1
                                    ? ToolCallStream(("call-1", "{ \"value\": \"a\" }"))
                                    : MetadataOnlyStream()
                        ),
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
                }
            );

        await ConsumeIgnoringErrorsAsync(result);

        AssertNoFinish(await Assert.ThrowsAsync<NoOutputGeneratedException>(() => result.Steps));
        AssertNoFinish(
            await Assert.ThrowsAsync<NoOutputGeneratedException>(() => result.FinishReason)
        );
        AssertNoFinish(await Assert.ThrowsAsync<NoOutputGeneratedException>(() => result.Usage));
        AssertNoFinish(Assert.IsType<NoOutputGeneratedException>(Assert.Single(errors)));
        Assert.Single(stepFinishes);
    }

    private static void AssertNoFinish(Exception error)
    {
        Assert.Equal(NoFinishMessage, error.Message);
    }

    // Reads the stream to its end and keeps the error that ends it, as upstream consumeStream does.
    private static async Task ConsumeIgnoringErrorsAsync(
        StreamTextResult result,
        List<Exception>? errors = null
    )
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
        yield return new ResponseMetadataStreamPart(
            "id-0",
            "mock-model-id",
            DateTimeOffset.FromUnixTimeMilliseconds(0)
        );
        await Task.CompletedTask;
    }

    // The stream is cut short: no text-end and no finish chunk.
    private static async IAsyncEnumerable<LanguageModelStreamPart> PartialOutputStream()
    {
        yield return new StreamStartStreamPart(null);
        yield return new ResponseMetadataStreamPart(
            "id-0",
            "mock-model-id",
            DateTimeOffset.FromUnixTimeMilliseconds(0)
        );
        yield return new TextStartStreamPart("1");
        yield return new TextDeltaStreamPart("1", "Hello");
        yield return new TextDeltaStreamPart("1", ", world");
        await Task.CompletedTask;
    }

    private static async IAsyncEnumerable<LanguageModelStreamPart> ErroringStream()
    {
        yield return new StreamStartStreamPart(null);
        yield return new ResponseMetadataStreamPart(
            "id-0",
            "mock-model-id",
            DateTimeOffset.FromUnixTimeMilliseconds(0)
        );
        await Task.Yield();
        throw new InvalidOperationException("simulated provider stream error");
    }

    private static async IAsyncEnumerable<LanguageModelStreamPart> ErrorPartStream()
    {
        yield return new ResponseMetadataStreamPart(
            "id-0",
            "mock-model-id",
            DateTimeOffset.FromUnixTimeMilliseconds(0)
        );
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

    private const string RetrySchema =
        "{\"type\":\"object\",\"properties\":{\"x\":{\"type\":\"string\"}},\"required\":[\"x\"],\"additionalProperties\":false}";

    private static ErrorStreamPart ProviderError(string message = "provider error") => new(message);

    private static LanguageModelStreamPart[] Recovered(string id, string text) =>
        [
            new TextStartStreamPart(id),
            new TextDeltaStreamPart(id, text),
            new TextEndStreamPart(id),
            StreamTextTransformTestSupport.Finish(),
        ];

    private static Task<List<string>> TextOf(StreamTextResult result) =>
        StreamTextTransformTestSupport.TextStreamAsync(result);

    private static AiClient RetryClient() => Vercel.AI.Tests.Upstream.GenerateTextSteering.SteeringTestSupport.Client();

    [Fact]
    [UpstreamTest(
        Prefix + "should retry the current step after a provider error stream part",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_retry_the_current_step_after_a_provider_error_stream_part()
    {
        var model = new StreamTextTransformTestSupport.CallsModel(
            (call, _) =>
                new StreamTextTransformTestSupport.CallScript(
                    call == 0
                        ?
                        [
                            new TextStartStreamPart("1"),
                            new TextDeltaStreamPart("1", "partial "),
                            ProviderError(),
                        ]
                        : Recovered("2", "recovered")
                )
        );

        var result = RetryClient()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Prompt = "test-input",
                    MaxRetries = 0,
                    StreamRetries = 1,
                    OnError = (_, _) => Task.CompletedTask,
                }
            );

        Assert.Equal(["partial ", "recovered"], await TextOf(result));
        Assert.Equal(2, model.CallCount);
        Assert.Equal(FinishReason.Stop, await result.FinishReason);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should exclude failed-attempt text from structured output state",
        Coverage = UpstreamCoverage.Partial,
        Note = "StreamTextResult has no FinalStep property, so it asserts the last step of Steps."
    )]
    public async Task Should_exclude_failed_attempt_text_from_structured_output_state()
    {
        var model = new StreamTextTransformTestSupport.CallsModel(
            (call, _) =>
                new StreamTextTransformTestSupport.CallScript(
                    call == 0
                        ?
                        [
                            new TextStartStreamPart("1"),
                            new TextDeltaStreamPart("1", "{\"x\":"),
                            ProviderError(),
                        ]
                        : Recovered("2", "{\"x\":\"ok\"}")
                )
        );

        var result = RetryClient()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Prompt = "test-input",
                    Output = OutputSpec.Object(RetrySchema),
                    StreamRetries = 1,
                    OnError = (_, _) => Task.CompletedTask,
                }
            );

        await result.ConsumeStreamAsync();

        Assert.Equal("ok", (await result.Output)!.Value.GetProperty("x").GetString());
        Assert.Equal("{\"x\":\"ok\"}", (await result.Steps).Last().Text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve attempt isolation when a transform reconstructs recovered parts",
        Coverage = UpstreamCoverage.Partial,
        Note = "StreamTextResult has no FinalStep property, so it asserts the last step of Steps."
    )]
    public async Task Should_preserve_attempt_isolation_when_a_transform_reconstructs_recovered_parts()
    {
        var model = new StreamTextTransformTestSupport.CallsModel(
            (call, _) =>
                new StreamTextTransformTestSupport.CallScript(
                    call == 0
                        ?
                        [
                            new ResponseMetadataStreamPart(
                                "failed-id",
                                "failed-model",
                                DateTimeOffset.FromUnixTimeMilliseconds(1)
                            ),
                            new TextStartStreamPart("failed-text"),
                            new TextDeltaStreamPart("failed-text", "{\"x\":"),
                            ProviderError(),
                        ]
                        :
                        [
                            new ResponseMetadataStreamPart(
                                "recovered-id",
                                "recovered-model",
                                DateTimeOffset.FromUnixTimeMilliseconds(2)
                            ),
                            new TextStartStreamPart("recovered-text"),
                            new TextDeltaStreamPart("recovered-text", "{\"x\":\"ok\"}"),
                            new TextEndStreamPart("recovered-text"),
                            StreamTextTransformTestSupport.Finish(),
                        ],
                    $"request-{call + 1}",
                    new Dictionary<string, string> { ["call"] = $"{call + 1}" }
                )
        );
        StreamTextTransform reconstruct = _ =>
            StreamTextTransformTestSupport.Map(chunk =>
                chunk is TextStartPart start ? [new TextStartPart(start.Id, start.ProviderMetadata)] : [chunk]
            );

        var result = RetryClient()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Prompt = "test-input",
                    Output = OutputSpec.Object(RetrySchema),
                    StreamRetries = 1,
                    Include = new StreamTextInclude { RequestBody = true },
                    ExperimentalTransform = [reconstruct],
                    OnError = (_, _) => Task.CompletedTask,
                }
            );

        Assert.Equal(["{\"x\":", "{\"x\":\"ok\"}"], await TextOf(result));
        Assert.Equal("ok", (await result.Output)!.Value.GetProperty("x").GetString());
        var finalStep = (await result.Steps).Last();
        Assert.Equal("{\"x\":\"ok\"}", finalStep.Text);
        Assert.Equal("request-2", finalStep.Request.Body!.Value.GetString());
        Assert.Equal("recovered-id", finalStep.Response.Id);
        Assert.Equal("recovered-model", finalStep.Response.ModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(2), finalStep.Response.Timestamp);
        Assert.Equal("2", finalStep.Response.Headers!["call"]);
        var message = Assert.IsType<AssistantModelMessage>(Assert.Single(finalStep.Response.Messages));
        Assert.Equal("{\"x\":\"ok\"}", Assert.IsType<AssistantTextPart>(Assert.Single(message.Parts)).Text);
        Assert.Equal(finalStep.Response.Messages, await result.ResponseMessages);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reset failed-attempt state when the recovered stream ends without output",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_reset_failed_attempt_state_when_the_recovered_stream_ends_without_output()
    {
        var model = new StreamTextTransformTestSupport.CallsModel(
            (call, _) =>
                new StreamTextTransformTestSupport.CallScript(
                    call == 0
                        ?
                        [
                            new TextStartStreamPart("failed-text"),
                            new TextDeltaStreamPart("failed-text", "partial"),
                            ProviderError(),
                        ]
                        : []
                )
        );
        var errors = new List<Exception>();

        var result = RetryClient()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Prompt = "test-input",
                    StreamRetries = 1,
                    OnError = (error, _) =>
                    {
                        errors.Add(error);
                        return Task.CompletedTask;
                    },
                }
            );

        var parts = await StreamTextTransformTestSupport.PartsAsync(result);

        Assert.Equal(2, model.CallCount);
        Assert.Contains(
            parts,
            part => part is TextDeltaPart { Id: "failed-text", Text: "partial" }
        );
        Assert.Contains(
            parts,
            part => part is ErrorPart { Error: NoOutputGeneratedException }
        );
        await Assert.ThrowsAsync<NoOutputGeneratedException>(() => result.Steps);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should use request and response metadata from the recovered attempt",
        Coverage = UpstreamCoverage.Partial,
        Note = "StreamTextResult has no FinalStep property, so it asserts the last step of Steps."
    )]
    public async Task Should_use_request_and_response_metadata_from_the_recovered_attempt()
    {
        var model = new StreamTextTransformTestSupport.CallsModel(
            (call, _) =>
                new StreamTextTransformTestSupport.CallScript(
                    call == 0
                        ?
                        [
                            new ResponseMetadataStreamPart(
                                "failed-id",
                                "failed-model",
                                DateTimeOffset.FromUnixTimeMilliseconds(1)
                            ),
                            new TextStartStreamPart("1"),
                            new TextDeltaStreamPart("1", "partial"),
                            ProviderError(),
                        ]
                        :
                        [
                            new ResponseMetadataStreamPart(
                                "recovered-id",
                                "recovered-model",
                                DateTimeOffset.FromUnixTimeMilliseconds(2)
                            ),
                            new TextStartStreamPart("2"),
                            new TextDeltaStreamPart("2", "recovered"),
                            new TextEndStreamPart("2"),
                            StreamTextTransformTestSupport.Finish(),
                        ],
                    $"request-{call + 1}",
                    new Dictionary<string, string> { ["call"] = $"{call + 1}" }
                )
        );

        var result = RetryClient()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Prompt = "test-input",
                    StreamRetries = 1,
                    Include = new StreamTextInclude { RequestBody = true },
                    OnError = (_, _) => Task.CompletedTask,
                }
            );

        await result.ConsumeStreamAsync();

        var finalStep = (await result.Steps).Last();
        Assert.Equal("request-2", finalStep.Request.Body!.Value.GetString());
        Assert.Equal("recovered-id", finalStep.Response.Id);
        Assert.Equal("recovered-model", finalStep.Response.ModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(2), finalStep.Response.Timestamp);
        Assert.Equal("2", finalStep.Response.Headers!["call"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should retry the current step when onError requests a retry",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_retry_the_current_step_when_onError_requests_a_retry()
    {
        var onErrorCalls = 0;
        var model = new StreamTextTransformTestSupport.CallsModel(
            (call, _) =>
                new StreamTextTransformTestSupport.CallScript(
                    call == 0 ? [ProviderError()] : Recovered("1", "recovered")
                )
        );

        var result = RetryClient()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Prompt = "test-input",
                    StreamRetries = 0,
                    OnErrorRetry = (_, _) =>
                    {
                        onErrorCalls++;
                        return Task.FromResult<StreamTextOnErrorResult?>(
                            new StreamTextOnErrorResult { Retry = true }
                        );
                    },
                }
            );

        Assert.Equal(["recovered"], await TextOf(result));
        Assert.Equal(2, model.CallCount);
        Assert.Equal(1, onErrorCalls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should allow at most one callback-directed retry per logical step",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_allow_at_most_one_callback_directed_retry_per_logical_step()
    {
        var onErrorCalls = 0;
        var model = new StreamTextTransformTestSupport.CallsModel(
            (call, _) =>
                new StreamTextTransformTestSupport.CallScript([ProviderError($"provider error {call + 1}")])
        );

        var result = RetryClient()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Prompt = "test-input",
                    StreamRetries = 0,
                    OnErrorRetry = (_, _) =>
                    {
                        onErrorCalls++;
                        return Task.FromResult<StreamTextOnErrorResult?>(
                            model.CallCount <= 2 ? new StreamTextOnErrorResult { Retry = true } : null
                        );
                    },
                }
            );

        var parts = await StreamTextTransformTestSupport.PartsAsync(result);

        Assert.Equal(2, model.CallCount);
        Assert.Equal(2, onErrorCalls);
        Assert.Contains(
            parts,
            part => part is ErrorPart { Error: { } error } && error.Message == "provider error 2"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not retry from an existing onError observer without streamRetries",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_retry_from_an_existing_onError_observer_without_streamRetries()
    {
        var onErrorCalls = 0;
        var model = new StreamTextTransformTestSupport.CallsModel(
            (call, _) =>
                new StreamTextTransformTestSupport.CallScript(
                    call == 0 ? [ProviderError()] : [StreamTextTransformTestSupport.Finish()]
                )
        );

        var result = RetryClient()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Prompt = "test-input",
                    OnErrorRetry = (_, _) =>
                    {
                        onErrorCalls++;
                        return Task.FromResult<StreamTextOnErrorResult?>(
                            new StreamTextOnErrorResult { Retry = true }
                        );
                    },
                }
            );

        await result.ConsumeStreamAsync();

        Assert.Equal(1, model.CallCount);
        Assert.Equal(1, onErrorCalls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should use only recovered transformed content in the next step context",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_only_recovered_transformed_content_in_the_next_step_context()
    {
        var nextStepPrompts = new List<string>();
        var model = new StreamTextTransformTestSupport.CallsModel(
            (call, options) =>
            {
                switch (call)
                {
                    case 0:
                        return new StreamTextTransformTestSupport.CallScript(
                            [
                                new TextStartStreamPart("failed-text"),
                                new TextDeltaStreamPart("failed-text", "partial"),
                                ProviderError(),
                            ]
                        );
                    case 1:
                        return new StreamTextTransformTestSupport.CallScript(
                            [
                                new TextStartStreamPart("recovered-text"),
                                new TextDeltaStreamPart("recovered-text", "recovered"),
                                new TextEndStreamPart("recovered-text"),
                                new ToolCallStreamPart("tool-call-1", "tool1", "{}"),
                                StreamTextTransformTestSupport.Finish(FinishReason.ToolCalls, "tool-calls"),
                            ]
                        );
                    default:
                        nextStepPrompts.Add(
                            string.Concat(
                                options
                                    .Prompt.OfType<AssistantModelMessage>()
                                    .SelectMany(message => message.Parts)
                                    .OfType<AssistantTextPart>()
                                    .Select(part => part.Text)
                            )
                        );
                        return new StreamTextTransformTestSupport.CallScript(
                            Recovered("final-text", "done")
                        );
                }
            }
        );
        StreamTextTransform transform = _ =>
            StreamTextTransformTestSupport.Map(chunk =>
                chunk switch
                {
                    TextStartPart start => [new TextStartPart(start.Id, start.ProviderMetadata)],
                    TextDeltaPart delta =>
                    [
                        new TextDeltaPart(delta.Id, delta.Text.ToUpperInvariant(), delta.ProviderMetadata),
                    ],
                    _ => [chunk],
                }
            );

        var result = RetryClient()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Prompt = "test-input",
                    Tools =
                    [
                        Tool.Function("tool1", null, "{\"type\":\"object\",\"properties\":{}}", (_, _) =>
                            Task.FromResult("tool result")
                        ),
                    ],
                    StopWhen = StopWhen.IsStepCount(2),
                    StreamRetries = 1,
                    ExperimentalTransform = [transform],
                    OnError = (_, _) => Task.CompletedTask,
                }
            );

        await result.ConsumeStreamAsync();

        var prompt = Assert.Single(nextStepPrompts);
        Assert.Contains("RECOVERED", prompt);
        Assert.DoesNotContain("partial", prompt);
        var steps = await result.Steps;
        Assert.Equal("RECOVERED", steps[0].Text);
        Assert.Equal("DONE", steps[1].Text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve completed steps when retrying a later step",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_completed_steps_when_retrying_a_later_step()
    {
        var executions = 0;
        var prepareSteps = 0;
        var model = new StreamTextTransformTestSupport.CallsModel(
            (call, _) =>
                call switch
                {
                    0 => new StreamTextTransformTestSupport.CallScript(
                        [
                            new ToolCallStreamPart("tool-call-1", "tool1", "{}"),
                            StreamTextTransformTestSupport.Finish(FinishReason.ToolCalls, "tool-calls"),
                        ]
                    ),
                    1 => new StreamTextTransformTestSupport.CallScript([ProviderError()]),
                    _ => new StreamTextTransformTestSupport.CallScript(Recovered("1", "recovered")),
                }
        );

        var result = RetryClient()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Prompt = "test-input",
                    Tools =
                    [
                        Tool.Function("tool1", null, "{\"type\":\"object\",\"properties\":{}}", (_, _) =>
                        {
                            executions++;
                            return Task.FromResult("tool result");
                        }),
                    ],
                    StopWhen = StopWhen.IsStepCount(2),
                    PrepareStep = _ =>
                    {
                        prepareSteps++;
                        return null;
                    },
                    StreamRetries = 1,
                    OnError = (_, _) => Task.CompletedTask,
                }
            );

        await result.ConsumeStreamAsync();

        Assert.Equal(3, model.CallCount);
        Assert.Equal(1, executions);
        Assert.Equal(2, prepareSteps);
        var steps = await result.Steps;
        Assert.Equal(2, steps.Count);
        Assert.Single(steps[0].ToolResults);
        Assert.Equal("recovered", steps[1].Text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should discard tool calls from a failed attempt",
        Coverage = UpstreamCoverage.Partial,
        Note = "StreamTextResult has no FinalStep property, so it asserts the last step of Steps."
    )]
    public async Task Should_discard_tool_calls_from_a_failed_attempt()
    {
        var executions = 0;
        var model = new StreamTextTransformTestSupport.CallsModel(
            (call, _) =>
                new StreamTextTransformTestSupport.CallScript(
                    call == 0
                        ?
                        [
                            new ToolCallStreamPart("failed-tool-call", "tool1", "{}"),
                            ProviderError(),
                        ]
                        : Recovered("1", "recovered")
                )
        );

        var result = RetryClient()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Prompt = "test-input",
                    Tools =
                    [
                        Tool.Function("tool1", null, "{\"type\":\"object\",\"properties\":{}}", (_, _) =>
                        {
                            executions++;
                            return Task.FromResult("tool result");
                        }),
                    ],
                    StreamRetries = 1,
                    OnError = (_, _) => Task.CompletedTask,
                }
            );

        await result.ConsumeStreamAsync();

        Assert.Equal(2, model.CallCount);
        Assert.Equal(0, executions);
        var finalStep = (await result.Steps).Last();
        Assert.Empty(finalStep.ToolCalls);
        Assert.Empty(finalStep.ToolResults);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject invalid streamRetries values",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_reject_invalid_streamRetries_values()
    {
        var error = Assert.Throws<InvalidArgumentException>(() =>
            RetryClient()
                .StreamTextAsync(
                    new StreamTextOptions
                    {
                        Model = StreamTextTransformTestSupport.CreateTestModel(),
                        Prompt = "test-input",
                        StreamRetries = -1,
                    }
                )
        );

        Assert.Contains("streamRetries must be >= 0", error.Message);
    }

    private sealed class MockLanguageModel(
        Func<LanguageModelCallOptions, CancellationToken, Task<LanguageModelStreamResult>> doStream
    ) : ILanguageModel
    {
        public IReadOnlyDictionary<string, string> SupportedUrls =>
            Vercel.AI.ProviderUtils.UrlSupport.None;

        public string SpecificationVersion => "V4";

        public string Provider => "test";

        public string ModelId => "mock-model-id";

        public Task<LanguageModelGenerateResult> DoGenerateAsync(
            LanguageModelCallOptions options,
            CancellationToken cancellationToken
        )
        {
            throw new NotSupportedException();
        }

        public Task<LanguageModelStreamResult> DoStreamAsync(
            LanguageModelCallOptions options,
            CancellationToken cancellationToken
        )
        {
            return doStream(options, cancellationToken);
        }
    }
}
