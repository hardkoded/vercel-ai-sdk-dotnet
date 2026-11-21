// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AspNetCore;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.Telemetry;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.GenerateText.OptionsTimeoutSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamTextWithMinimalRetention;

/// <summary>
/// Port of <c>stream-text-with-minimal-retention.test.ts</c> &gt; <c>%s behavior</c>. The same checks run against both facades of the
/// execution engine: <c>streamText</c> and <c>experimental_streamTextLowMemory</c>.
/// </summary>
public sealed class ExperimentalStreamTextLowMemoryBehaviorTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/stream-text-with-minimal-retention.test.ts::%s behavior::";

    public static TheoryData<string> Facades => new() { "streamText", "streamTextLowMemory" };

    private static Facade Generate(string name, StreamTextOptions options)
    {
        var client = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
        if (name == "streamText")
        {
            var result = client.StreamTextAsync(options);
            return new Facade(
                result.Stream,
                result.TextStream,
                result.PartialOutputStream,
                result.ElementStream,
                result.ConsumeStreamAsync,
                result.Usage,
                result.FinishReason,
                (status, statusText, headers) =>
                    result.ToTextStreamResponse(status, statusText, headers),
                result.ToUIMessageStream
            );
        }

        var lowMemory = client.StreamTextLowMemoryAsync(options);
        return new Facade(
            lowMemory.Stream,
            lowMemory.TextStream,
            lowMemory.PartialOutputStream,
            lowMemory.ElementStream,
            lowMemory.ConsumeStreamAsync,
            lowMemory.Usage,
            lowMemory.FinishReason,
            (status, statusText, headers) =>
                lowMemory.ToTextStreamResponse(status, statusText, headers),
            lowMemory.ToUIMessageStream
        );
    }

    private static DelegateModel ModelWithSteps(params LanguageModelStreamPart[][] steps)
    {
        var next = -1;
        return new DelegateModel
        {
            Stream = (_, _) =>
            {
                var index = Interlocked.Increment(ref next);
                return Task.FromResult(
                    new LanguageModelStreamResult(
                        StreamParts(index < steps.Length ? steps[index] : [])
                    )
                );
            },
        };
    }

    private static async IAsyncEnumerable<LanguageModelStreamPart> StreamParts(
        LanguageModelStreamPart[] parts
    )
    {
        foreach (var part in parts)
        {
            yield return part;
            await Task.Yield();
        }
    }

    private static LanguageModelStreamPart[] WithFinish(
        LanguageModelStreamPart[] parts,
        FinishReason reason = FinishReason.Stop
    )
    {
        return
        [
            .. parts,
            new FinishStreamPart(
                reason,
                new LanguageModelUsage(3, 2, 5),
                reason == FinishReason.Stop ? "stop" : "tool-calls"
            ),
        ];
    }

    private static async Task<List<T>> ToList<T>(IAsyncEnumerable<T> values)
    {
        var list = new List<T>();
        await foreach (var value in values)
        {
            list.Add(value);
        }

        return list;
    }

    [Theory]
    [MemberData(nameof(Facades))]
    [UpstreamTest(
        Prefix + "executes tools and sends complete history into the next step",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Executes_tools_and_sends_complete_history_into_the_next_step(string name)
    {
        var executions = 0;
        var model = ModelWithSteps(
            WithFinish(
                [
                    .. StreamingOnlyContractTests.TextParts("Checking.").SkipLast(1),
                    new ToolCallStreamPart("call", "weather", "{\"city\":\"Paris\"}"),
                ],
                FinishReason.ToolCalls
            ),
            StreamingOnlyContractTests.TextParts("It is sunny.")
        );
        var stepTexts = new List<string>();
        var result = Generate(
            name,
            new StreamTextOptions
            {
                Model = model,
                Prompt = "Weather?",
                Tools =
                [
                    Tool.Function(
                        "weather",
                        null,
                        "{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}},\"required\":[\"city\"]}",
                        (input, _) =>
                        {
                            executions++;
                            return Task.FromResult(
                                JsonSerializer.Serialize(
                                    input.GetProperty("city").GetString() + ": sunny"
                                )
                            );
                        }
                    ),
                ],
                StopWhen = StopWhen.IsStepCount(2),
                OnStepEnd = (step, _) =>
                {
                    stepTexts.Add(step.Text);
                    return Task.CompletedTask;
                },
            }
        );

        var parts = await ToList(result.Stream());

        Assert.Equal(1, executions);
        Assert.Contains(parts, part =>
            part is ToolCallPart call
            && call.ToolCall.ToolName == "weather"
            && call.ToolCall.ArgumentsJson.Contains("Paris")
        );
        Assert.Contains(parts, part =>
            part is ToolResultPart { Result.OutputJson: "\"Paris: sunny\"" }
        );
        Assert.Equal(2, model.Calls.Count);
        Assert.Contains(model.Calls[1].Prompt, message =>
            message is AssistantModelMessage assistant
            && assistant.Parts.OfType<AssistantTextPart>().Any(text => text.Text == "Checking.")
        );
        Assert.Equal(new[] { "Checking.", "It is sunny." }, stepTexts);
        var usage = await result.Usage;
        Assert.Equal(6, usage.InputTokens);
        Assert.Equal(4, usage.OutputTokens);
        Assert.Equal(FinishReason.Stop, await result.FinishReason);
    }

    [Theory]
    [MemberData(nameof(Facades))]
    [UpstreamTest(
        Prefix + "preserves approval requests and does not execute an unapproved tool",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_approval_requests_and_does_not_execute_an_unapproved_tool(
        string name
    )
    {
        var executions = 0;
        var action = Tool.Function("action", null, "{\"type\":\"object\"}", (_, _) =>
        {
            executions++;
            return Task.FromResult("\"done\"");
        });
        action.NeedsApproval = true;
        var result = Generate(
            name,
            new StreamTextOptions
            {
                Model = ModelWithSteps(
                    WithFinish(
                        [new ToolCallStreamPart("call", "action", "{}")],
                        FinishReason.ToolCalls
                    )
                ),
                Prompt = "Act",
                Tools = [action],
            }
        );

        var parts = await ToList(result.Stream());

        Assert.Contains(parts, part =>
            part is ToolApprovalRequestPart { ToolCall.ToolName: "action" }
        );
        Assert.Equal(0, executions);
    }

    [Theory]
    [MemberData(nameof(Facades))]
    [UpstreamTest(
        Prefix + "delivers complete lifecycle callback content in order",
        Note = "The port has no onStart callback on streamText, so the start event is not part of the order."
    )]
    public async Task Delivers_complete_lifecycle_callback_content_in_order(string name)
    {
        var events = new List<string>();
        var result = Generate(
            name,
            new StreamTextOptions
            {
                Model = ModelWithSteps(StreamingOnlyContractTests.TextParts()),
                Prompt = "test",
                OnLanguageModelCallStart = (_, _) =>
                {
                    events.Add("model-start");
                    return Task.CompletedTask;
                },
                OnLanguageModelCallEnd = (call, _) =>
                {
                    Assert.Contains(call.Content, content =>
                        content is GeneratedText { Text: "hello" }
                    );
                    events.Add("model-end");
                    return Task.CompletedTask;
                },
                OnStepEnd = (step, _) =>
                {
                    Assert.Equal("hello", step.Text);
                    events.Add("step-end");
                    return Task.CompletedTask;
                },
                OnEnd = (end, _) =>
                {
                    Assert.Equal("hello", end.Text);
                    events.Add("end");
                    return Task.CompletedTask;
                },
            }
        );

        Assert.Equal(new[] { "hello" }, await ToList(result.TextStream()));
        Assert.Equal(new[] { "model-start", "model-end", "step-end", "end" }, events);
    }

    [Theory]
    [MemberData(nameof(Facades))]
    [UpstreamTest(Prefix + "preserves per-call telemetry completion events and their content")]
    public async Task Preserves_per_call_telemetry_completion_events_and_their_content(string name)
    {
        GenerateTextResult? end = null;
        LanguageModelCallEndEvent? callEnd = null;
        var result = Generate(
            name,
            new StreamTextOptions
            {
                Model = ModelWithSteps(StreamingOnlyContractTests.TextParts()),
                Prompt = "test",
                Telemetry = new TelemetryOptions
                {
                    Integrations =
                    [
                        new TestTelemetry
                        {
                            OnEnd = (e, _) =>
                            {
                                end = e;
                                return Task.CompletedTask;
                            },
                            OnLanguageModelCallEnd = (e, _) =>
                            {
                                callEnd = e;
                                return Task.CompletedTask;
                            },
                        },
                    ],
                },
            }
        );

        await result.ConsumeStreamOf(CancellationToken.None);

        Assert.Equal("hello", end?.Text);
        Assert.Contains(callEnd!.Content, content => content is GeneratedText { Text: "hello" });
    }

    [Theory]
    [MemberData(nameof(Facades))]
    [UpstreamTest(
        Prefix + "supports structured partial output and parsed output in onEnd",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Supports_structured_partial_output_and_parsed_output_in_onEnd(string name)
    {
        GenerateTextResult? end = null;
        var result = Generate(
            name,
            new StreamTextOptions
            {
                Model = ModelWithSteps(StreamingOnlyContractTests.TextParts("{\"answer\":42}")),
                Prompt = "test",
                Output = OutputSpec.Object(
                    "{\"type\":\"object\",\"properties\":{\"answer\":{\"type\":\"number\"}},\"required\":[\"answer\"]}"
                ),
                OnEnd = (result, _) =>
                {
                    end = result;
                    return Task.CompletedTask;
                },
            }
        );

        var partials = await ToList(result.PartialOutputStream());

        Assert.Equal(new[] { "{\"answer\":42}" }, partials.Select(p => p.GetRawText()));
        Assert.NotNull(end);
        Assert.Equal("{\"answer\":42}", end.Output!.Value.GetRawText());
        Assert.Equal("{\"answer\":42}", end.Text);
    }

    [Theory]
    [MemberData(nameof(Facades))]
    [UpstreamTest(Prefix + "supports element streams", Coverage = UpstreamCoverage.Covered)]
    public async Task Supports_element_streams(string name)
    {
        var result = Generate(
            name,
            new StreamTextOptions
            {
                Model = ModelWithSteps(
                    StreamingOnlyContractTests.TextParts(
                        "{\"elements\":[{\"value\":1},{\"value\":2}]}"
                    )
                ),
                Prompt = "test",
                Output = OutputSpec.Array(
                    "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"number\"}},\"required\":[\"value\"],\"additionalProperties\":false}"
                ),
            }
        );

        var elements = await ToList(result.ElementStream());

        Assert.Equal(
            new[] { "{\"value\":1}", "{\"value\":2}" },
            elements.Select(element => element.GetRawText())
        );
    }

    [Theory]
    [MemberData(nameof(Facades))]
    [UpstreamTest(
        Prefix + "supports stream transforms and text responses",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Supports_stream_transforms_and_text_responses(string name)
    {
        var result = Generate(
            name,
            new StreamTextOptions
            {
                Model = ModelWithSteps(StreamingOnlyContractTests.TextParts()),
                Prompt = "test",
                ExperimentalTransform =
                [
                    _ => (stream, cancellationToken) => UpperCase(stream, cancellationToken),
                ],
            }
        );

        var response = result.ToTextStreamResponse(null, null, null);
        var text = new System.Text.StringBuilder();
        var reader = response.Body.GetReader();
        while (true)
        {
            var read = await reader.ReadAsync();
            if (read.Done)
            {
                break;
            }

            text.Append(System.Text.Encoding.UTF8.GetString(read.Value));
        }

        Assert.Equal("HELLO", text.ToString());
    }

    private static async IAsyncEnumerable<TextStreamPart> UpperCase(
        IAsyncEnumerable<TextStreamPart> stream,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        await foreach (var part in stream.WithCancellation(cancellationToken))
        {
            yield return part is TextDeltaPart delta
                ? new TextDeltaPart(delta.Id, delta.Text.ToUpperInvariant(), delta.ProviderMetadata)
                : part;
        }
    }

    [Theory]
    [MemberData(nameof(Facades))]
    [UpstreamTest(
        Prefix + "supports UI message conversion and completion callbacks",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Supports_UI_message_conversion_and_completion_callbacks(string name)
    {
        UIMessageStreamOnEndEvent? end = null;
        var result = Generate(
            name,
            new StreamTextOptions
            {
                Model = ModelWithSteps(StreamingOnlyContractTests.TextParts()),
                Prompt = "test",
            }
        );

        var parts = await ToList(
            result.ToUIMessageStream(
                new UIMessageStreamOptions
                {
                    OnEnd = finished =>
                    {
                        end = finished;
                        return Task.CompletedTask;
                    },
                }
            )
        );

        Assert.Contains(parts, part =>
            part["type"]?.GetValue<string>() == "text-delta"
            && part["delta"]?.GetValue<string>() == "hello"
        );
        Assert.NotNull(end);
        Assert.Contains(end.ResponseMessage.Parts, part =>
            part is Vercel.AI.Ui.TextUIPart { Text: "hello" }
        );
    }

    [Theory]
    [MemberData(nameof(Facades))]
    [UpstreamTest(
        Prefix + "preserves stream error delivery and onError callbacks",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_stream_error_delivery_and_onError_callbacks(string name)
    {
        var errors = new List<Exception>();
        var result = Generate(
            name,
            new StreamTextOptions
            {
                Model = ModelWithSteps([
                    .. StreamingOnlyContractTests.TextParts().SkipLast(1),
                    new ErrorStreamPart("provider failed"),
                ]),
                Prompt = "test",
                OnError = (error, _) =>
                {
                    errors.Add(error);
                    return Task.CompletedTask;
                },
            }
        );

        var parts = await ToList(result.Stream());

        Assert.Contains(parts, part => part is ErrorPart { Message: "provider failed" });
        Assert.Equal("provider failed", Assert.Single(errors).Message);
    }
}

internal sealed record Facade(
    Func<CancellationToken, IAsyncEnumerable<TextStreamPart>> StreamOf,
    Func<CancellationToken, IAsyncEnumerable<string>> TextStreamOf,
    Func<CancellationToken, IAsyncEnumerable<JsonElement>> PartialOutputStreamOf,
    Func<CancellationToken, IAsyncEnumerable<JsonElement>> ElementStreamOf,
    Func<CancellationToken, Task> ConsumeStreamOf,
    Task<LanguageModelUsage> Usage,
    Task<FinishReason> FinishReason,
    Func<int?, string?, HeaderCollection?, TextStreamResponse> ToTextStreamResponse,
    Func<
        UIMessageStreamOptions?,
        CancellationToken,
        IAsyncEnumerable<JsonObject>
    > ToUIMessageStreamOf
)
{
    public IAsyncEnumerable<TextStreamPart> Stream() => StreamOf(default);

    public IAsyncEnumerable<string> TextStream() => TextStreamOf(default);

    public IAsyncEnumerable<JsonElement> PartialOutputStream() => PartialOutputStreamOf(default);

    public IAsyncEnumerable<JsonElement> ElementStream() => ElementStreamOf(default);

    public IAsyncEnumerable<JsonObject> ToUIMessageStream(UIMessageStreamOptions? options) =>
        ToUIMessageStreamOf(options, default);
}
