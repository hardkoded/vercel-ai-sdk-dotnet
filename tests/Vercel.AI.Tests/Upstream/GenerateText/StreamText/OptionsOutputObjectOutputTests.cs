// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; options.output &gt; object output</c>. Only the tests below are ported.</summary>
public sealed class OptionsOutputObjectOutputTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > options.output > object output::";

    [Fact]
    [UpstreamTest(
        Prefix + "should set responseFormat to json and send schema as part of the responseFormat"
    )]
    public async Task Should_set_responseFormat_to_json_and_send_schema_as_part_of_the_responseFormat()
    {
        var model = new Vercel.AI.Testing.TestLanguageModel
        {
            StreamParts = OptionsOutputSupport.Text(
                "{ ",
                "\"value\": ",
                "\"Hello, ",
                "world",
                "!\"",
                " }"
            ),
        };
        var result = OptionsOutputSupport.Stream(
            model,
            OutputSpec.Object(OptionsOutputSupport.ValueSchema)
        );

        await result.ConsumeStreamAsync();

        var call = Assert.Single(model.Calls);
        Assert.NotNull(call.JsonSchema);
        Assert.Equal(
            "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false}",
            call.JsonSchema!.Value.GetRawText()
        );
        Assert.Null(call.Tools);
    }

    [Fact]
    [UpstreamTest(Prefix + "should send valid partial text fragments")]
    public async Task Should_send_valid_partial_text_fragments()
    {
        var result = OptionsOutputSupport.Stream(
            OptionsOutputSupport.Text("{ ", "\"value\": ", "\"Hello, ", "world", "!\"", " }"),
            OutputSpec.Object(OptionsOutputSupport.ValueSchema)
        );

        var fragments = new List<string>();
        await foreach (var fragment in result.TextStream())
        {
            fragments.Add(fragment);
        }

        Assert.Equal(
            new[]
            {
                "{ ",
                // key difference: need to combine after `:`
                "\"value\": \"Hello, ",
                "world",
                "!\"",
                " }",
            },
            fragments
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should send partial output stream")]
    public async Task Should_send_partial_output_stream()
    {
        var result = OptionsOutputSupport.Stream(
            OptionsOutputSupport.Text("{ ", "\"value\": ", "\"Hello, ", "world", "!\"", " }"),
            OutputSpec.Object(OptionsOutputSupport.ValueSchema)
        );

        Assert.Equal(
            new[]
            {
                "{}",
                "{\"value\":\"Hello, \"}",
                "{\"value\":\"Hello, world\"}",
                "{\"value\":\"Hello, world!\"}",
            },
            await OptionsOutputSupport.Collect(result)
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream structured output after an earlier tool step emits text")]
    public async Task Should_stream_structured_output_after_an_earlier_tool_step_emits_text()
    {
        var responseCount = 0;
        var model = new ScriptedStreamModel(_ =>
            responseCount++ switch
            {
                0 => Parts(
                    new TextStartStreamPart("intro"),
                    new TextDeltaStreamPart("intro", "Checking the value."),
                    new TextEndStreamPart("intro"),
                    new ToolCallStreamPart("call-1", "lookup", "{}"),
                    new FinishStreamPart(
                        FinishReason.ToolCalls,
                        new LanguageModelUsage(3, 10, 13),
                        "tool-calls"
                    )
                ),
                1 => Parts(
                    new TextStartStreamPart("answer"),
                    new TextDeltaStreamPart("answer", "{\"value\":\"done\"}"),
                    new TextEndStreamPart("answer"),
                    new FinishStreamPart(
                        FinishReason.Stop,
                        new LanguageModelUsage(3, 10, 13),
                        "stop"
                    )
                ),
                _ => throw new InvalidOperationException(
                    $"Unexpected response count: {responseCount}"
                ),
            }
        );
        var result = new AiClient(
            Vercel.AI.Gateway.GatewayProvider.Create(
                new Vercel.AI.Gateway.GatewayOptions { ApiKey = "test" }
            )
        ).StreamTextAsync(
            new StreamTextOptions
            {
                Model = model,
                Tools = new[]
                {
                    Tool.Function(
                        "lookup",
                        null,
                        "{\"type\":\"object\",\"properties\":{},\"additionalProperties\":false}",
                        (_, _) => Task.FromResult("\"done\"")
                    ),
                },
                Output = OutputSpec.Object(OptionsOutputSupport.ValueSchema),
                Prompt = "Look up the value and return it.",
                StopWhen = StopWhen.IsStepCount(2),
            }
        );

        Assert.Equal(new[] { "{\"value\":\"done\"}" }, await OptionsOutputSupport.Collect(result));
        Assert.Equal("done", (await result.Output)!.Value.GetProperty("value").GetString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should send partial output stream when last chunk contains content")]
    public async Task Should_send_partial_output_stream_when_last_chunk_contains_content()
    {
        var result = OptionsOutputSupport.Stream(
            OptionsOutputSupport.Text("{ ", "\"value\": ", "\"Hello, ", "world!\" }"),
            OutputSpec.Object(OptionsOutputSupport.ValueSchema)
        );

        Assert.Equal(
            new[] { "{}", "{\"value\":\"Hello, \"}", "{\"value\":\"Hello, world!\"}" },
            await OptionsOutputSupport.Collect(result)
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should resolve text promise with the correct content")]
    public async Task Should_resolve_text_promise_with_the_correct_content()
    {
        var result = OptionsOutputSupport.Stream(
            OptionsOutputSupport.Text("{ ", "\"value\": ", "\"Hello, ", "world!\" ", "}"),
            OutputSpec.Object(OptionsOutputSupport.ValueSchema)
        );

        Assert.Equal("{ \"value\": \"Hello, world!\" }", await result.Text);
    }

    [Fact]
    [UpstreamTest(Prefix + "should resolve output promise with the correct content")]
    public async Task Should_resolve_output_promise_with_the_correct_content()
    {
        var result = OptionsOutputSupport.Stream(
            OptionsOutputSupport.Text("{ ", "\"value\": ", "\"Hello, ", "world!\" ", "}"),
            OutputSpec.Object(OptionsOutputSupport.ValueSchema)
        );

        Assert.Equal(
            "Hello, world!",
            (await result.Output)!.Value.GetProperty("value").GetString()
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should expose parsed output to onEnd after the stream completes")]
    public async Task Should_expose_parsed_output_to_onEnd_after_the_stream_completes()
    {
        JsonElement? callbackOutput = null;
        var result = StreamWithOnEnd(
            OutputSpec.Object(OptionsOutputSupport.ValueSchema),
            (value, _) =>
            {
                callbackOutput = value.Output;
                return Task.CompletedTask;
            }
        );

        await result.ConsumeStreamAsync();

        Assert.Equal("{\"value\":\"Hello, world!\"}", callbackOutput!.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(Prefix + "should not delay output for an active onEnd callback")]
    public async Task Should_not_delay_output_for_an_active_onEnd_callback()
    {
        var callbackStarted = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var finishCallback = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var result = StreamWithOnEnd(
            OutputSpec.Object(OptionsOutputSupport.ValueSchema),
            async (_, _) =>
            {
                callbackStarted.SetResult();
                await finishCallback.Task;
            }
        );

        var outputTask = result.Output;
        await callbackStarted.Task;

        Assert.Equal("Hello, world!", (await outputTask)!.Value.GetProperty("value").GetString());

        finishCallback.SetResult();
        await result.ConsumeStreamAsync();
    }

    [Fact]
    [UpstreamTest(Prefix + "should parse complete output once for onEnd and the output promise")]
    public async Task Should_parse_complete_output_once_for_onEnd_and_the_output_promise()
    {
        var output = new CountingOutputSpec(OutputSpec.Object(OptionsOutputSupport.ValueSchema));
        JsonElement? callbackOutput = null;
        var result = StreamWithOnEnd(output, (value, _) =>
        {
            callbackOutput = value.Output;
            return Task.CompletedTask;
        });

        var resultOutput = result.Output;
        await result.ConsumeStreamAsync();

        Assert.Equal("Hello, world!", (await resultOutput)!.Value.GetProperty("value").GetString());
        Assert.Equal("{\"value\":\"Hello, world!\"}", callbackOutput!.Value.GetRawText());
        Assert.Equal(1, output.ParseCompleteOutputCalls);
    }

    [Fact]
    [UpstreamTest(Prefix + "should provide undefined output to onEnd when parsing fails")]
    public async Task Should_provide_undefined_output_to_onEnd_when_parsing_fails()
    {
        JsonElement? callbackOutput = null;
        var called = false;
        var result = StreamWithOnEnd(
            OutputSpec.Object(OptionsOutputSupport.ValueSchema),
            (value, _) =>
            {
                called = true;
                callbackOutput = value.Output;
                return Task.CompletedTask;
            },
            "{ \"value\": 42 }"
        );

        await result.ConsumeStreamAsync();

        Assert.True(called);
        Assert.Null(callbackOutput);
        var error = await Assert.ThrowsAsync<Vercel.AI.GenerateObject.NoObjectGeneratedException>(
            () =>
                result.Output
        );
        Assert.Equal("No object generated: response did not match schema.", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should allow onEnd to await the output promise after asynchronous work"
    )]
    public async Task Should_allow_onEnd_to_await_the_output_promise_after_asynchronous_work()
    {
        JsonElement? callbackOutput = null;
        StreamTextResult? result = null;
        result = StreamWithOnEnd(
            OutputSpec.Object(OptionsOutputSupport.ValueSchema),
            async (_, _) =>
            {
                await Task.Delay(1);
                callbackOutput = await result!.Output;
            }
        );

        await result.ConsumeStreamAsync();

        Assert.Equal("{\"value\":\"Hello, world!\"}", callbackOutput!.Value.GetRawText());
        Assert.Equal(
            "Hello, world!",
            (await result.Output)!.Value.GetProperty("value").GetString()
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should call onFinish with the correct content")]
    public async Task Should_call_onFinish_with_the_correct_content()
    {
        GenerateTextResult? finished = null;
        var result = OptionsOutputSupport.Stream(
            new Vercel.AI.Testing.TestLanguageModel
            {
                StreamParts = OptionsOutputSupport.Text(
                    "{ ",
                    "\"value\": ",
                    "\"Hello, ",
                    "world!\" ",
                    "}"
                ),
            },
            OutputSpec.Object(OptionsOutputSupport.ValueSchema),
            (value, _) =>
            {
                finished = value;
                return Task.CompletedTask;
            },
            () => "test-telemetry-call-id"
        );

        await result.ConsumeStreamAsync();

        Assert.NotNull(finished);
        Assert.Equal("test-telemetry-call-id", finished.CallId);
        Assert.Equal("test-telemetry-call-id", finished.FinalStep.CallId);
        Assert.Equal(0, finished.StepNumber);
        Assert.Equal(0, finished.FinalStep.StepNumber);
        Assert.Empty(finished.RuntimeContext);
        Assert.Empty(finished.ToolsContext);
        Assert.Empty(finished.FinalStep.RuntimeContext);
        Assert.Empty(finished.FinalStep.ToolsContext);
        Assert.Empty(finished.DynamicToolCalls);
        Assert.Empty(finished.DynamicToolResults);
        Assert.Empty(finished.StaticToolCalls);
        Assert.Empty(finished.StaticToolResults);
        Assert.Equal(13, finished.TotalUsage.TotalTokens);
        Assert.Equal("test", finished.Provider);
        Assert.Equal(finished.Response.ModelId, finished.ModelId);
        Assert.Equal("stop", finished.RawFinishReason);
        Assert.Null(finished.Request.Messages);
        Assert.Null(finished.Request.Body);
        Assert.Equal(0, finished.FinalStep.Performance.StepTimeMs);
        Assert.Equal(0, finished.FinalStep.Performance.ResponseTimeMs);
        Assert.Empty(finished.FinalStep.Performance.ToolExecutionMs);
        var text = Assert.IsType<GeneratedText>(Assert.Single(finished.Content));
        Assert.Equal("{ \"value\": \"Hello, world!\" }", text.Text);
        Assert.Empty(finished.Files);
        Assert.Empty(finished.ToolCalls);
        Assert.Empty(finished.ToolResults);
        Assert.Empty(finished.Sources);
        Assert.Empty(finished.Warnings);
        Assert.Equal(FinishReason.Stop, finished.FinishReason);
        Assert.Equal("stop", finished.FinalStep.RawFinishReason);
        Assert.Equal("{\"value\":\"Hello, world!\"}", finished.Output!.Value.GetRawText());
        Assert.Equal("{ \"value\": \"Hello, world!\" }", finished.Text);
        Assert.Null(finished.ReasoningText);
        Assert.Equal(3, finished.Usage.InputTokens);
        Assert.Equal(10, finished.Usage.OutputTokens);
        Assert.Equal(13, finished.Usage.TotalTokens);
        var step = Assert.Single(finished.Steps);
        Assert.Same(finished.FinalStep, step);
        var message = Assert.IsType<AssistantModelMessage>(
            Assert.Single(finished.ResponseMessages)
        );
        Assert.Equal("{ \"value\": \"Hello, world!\" }", message.Text);
        Assert.Equal(finished.Response.ModelId, finished.ModelId);
    }

    private static StreamTextResult StreamWithOnEnd(
        OutputSpec output,
        Func<GenerateTextResult, CancellationToken, Task> onEnd,
        string json = "{ \"value\": \"Hello, world!\" }"
    )
    {
        return OptionsOutputSupport.Stream(
            new Vercel.AI.Testing.TestLanguageModel
            {
                StreamParts = OptionsOutputSupport.Text(json),
            },
            output,
            onEnd
        );
    }

    // Maps to vi.spyOn(output, 'parseCompleteOutput').
    private sealed class CountingOutputSpec(OutputSpec inner) : OutputSpec
    {
        private int _parseCompleteOutputCalls;

        public int ParseCompleteOutputCalls => Volatile.Read(ref _parseCompleteOutputCalls);

        internal override JsonElement? Schema => inner.Schema;

        internal override string? SchemaName => inner.SchemaName;

        internal override string? SchemaDescription => inner.SchemaDescription;

        internal override JsonElement ParseCompleteOutput(string text)
        {
            Interlocked.Increment(ref _parseCompleteOutputCalls);
            return inner.ParseCompleteOutput(text);
        }

        internal override JsonElement? ParsePartialOutput(string text)
        {
            return inner.ParsePartialOutput(text);
        }
    }

    private static IAsyncEnumerable<LanguageModelStreamPart> Parts(
        params LanguageModelStreamPart[] parts
    )
    {
        return parts.ToAsyncEnumerable();
    }

    private sealed class ScriptedStreamModel(
        Func<LanguageModelCallOptions, IAsyncEnumerable<LanguageModelStreamPart>> next
    ) : ILanguageModel
    {
        public IReadOnlyDictionary<string, string> SupportedUrls { get; } =
            Vercel.AI.ProviderUtils.UrlSupport.None;

        public string SpecificationVersion => "V4";

        public string Provider => "test";

        public string ModelId => "test";

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
            return Task.FromResult(new LanguageModelStreamResult(next(options)));
        }
    }
}
