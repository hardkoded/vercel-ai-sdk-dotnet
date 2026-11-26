// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Tests.StreamText;
using Vercel.AI.Tests.Upstream.GenerateTextSteeringInterrupt;
using Vercel.AI.Tests.Upstream.Operations.Decide;
using Vercel.AI.Tests.Upstream.Steering;
using Vercel.AI.Util;
using static Vercel.AI.Tests.DecisionFixtures;
using static Vercel.AI.Tests.Upstream.GenerateTextSteeringInterrupt.SteeringInterruptTestSupport;
using static Vercel.AI.Tests.Upstream.Operations.Decide.DecideSupport;
using static Vercel.AI.Tests.Upstream.Operations.Extract.ExtractSupport;
using static Vercel.AI.Tests.Upstream.Telemetry.TracingChannel.TracingChannelSupport;
using DecideOperation = Vercel.AI.Operations.Decide;
using ExtractOperation = Vercel.AI.Operations.Extract;

namespace Vercel.AI.Tests.Upstream.Telemetry.TracingChannel;

/// <summary>
/// Port of <c>tracing-channel.test.ts</c> &gt; <c>telemetry tracing channel publisher</c>. The tests that need a store bound to the channel
/// or the OpenAI request redaction are not ported yet.
/// </summary>
public sealed class TelemetryTracingChannelPublisherTests
{
    private const string UpstreamCommit = "15bfd6b262f688cfb8281dbdb32c7dffe082337e";

    private static ExtractRequest InvoiceRequest(string callId)
    {
        return new ExtractRequest
        {
            Model = Setup(new ExtractionModelResult(InvoiceNumberAnswer(), [])),
            State = "Invoice INV-2041",
            Fields = InvoiceNumberField(),
            GenerateCallId = () => callId,
        };
    }

    private static DecideRequest RefundRequest(string callId)
    {
        return new DecideRequest
        {
            Model = new DecideModel
            {
                Result = new DecisionModelResult(
                    new Dictionary<string, DecisionAnswer>
                    {
                        ["refund"] = new DecisionAnswer("boolean", probability: 0.9),
                    }
                ),
            },
            State = S("Please refund me"),
            Questions = new Dictionary<string, DecisionQuestion>
            {
                ["refund"] = Q("boolean", "Refund?"),
            },
            GenerateCallId = () => callId,
        };
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::traces the current extract lifecycle sequence",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit,
        Note = "Node reports bindStart. The port reports start, because a DiagnosticListener has no bindStore."
    )]
    public async Task Traces_the_current_extract_lifecycle_sequence()
    {
        var sequence = await CollectEventSequence("trace-extract-sequence", () =>
            ExtractOperation.ExtractAsync(InvoiceRequest("trace-extract-sequence"))
        );

        Assert.Equal(["start experimental_extract", "asyncEnd experimental_extract"], sequence);
    }

    [Theory]
    [InlineData("an omitted allowlist", "omitted")]
    [InlineData("an empty allowlist", "empty")]
    [InlineData("an explicitly excluded field", "excluded")]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::excludes extract runtime context with $name",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Excludes_extract_runtime_context_with_name(string name, string kind)
    {
        Assert.NotEmpty(name);
        var callId = "trace-extract-excluded-" + kind;
        var request = InvoiceRequest(callId);
        request.Telemetry = kind switch
        {
            "omitted" => null,
            "empty" => new TelemetryOptions(),
            _ => new TelemetryOptions
            {
                IncludeRuntimeContext = new Dictionary<string, bool> { ["secret"] = false },
            },
        };
        request.RuntimeContext = new Dictionary<string, object?> { ["secret"] = "hidden" };

        var messages = await CollectStartMessages(callId, () =>
            ExtractOperation.ExtractAsync(request)
        );

        var extract = Assert.IsType<ExtractStartEvent>(
            Assert
                .Single(messages, m => m.Type == TelemetryTracingEventType.ExperimentalExtract)
                .Event
        );
        Assert.Empty(extract.RuntimeContext);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::includes only allowlisted extract runtime context in tracing",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Includes_only_allowlisted_extract_runtime_context_in_tracing()
    {
        var request = InvoiceRequest("trace-extract-allowlisted");
        request.Telemetry = new TelemetryOptions
        {
            IncludeRuntimeContext = new Dictionary<string, bool> { ["requestId"] = true },
        };
        request.RuntimeContext = new Dictionary<string, object?>
        {
            ["requestId"] = "request-1",
            ["secret"] = "hidden",
        };

        var messages = await CollectStartMessages("trace-extract-allowlisted", () =>
            ExtractOperation.ExtractAsync(request)
        );

        var extract = Assert.IsType<ExtractStartEvent>(
            Assert
                .Single(messages, m => m.Type == TelemetryTracingEventType.ExperimentalExtract)
                .Event
        );
        Assert.Equal(
            new Dictionary<string, object?> { ["requestId"] = "request-1" },
            extract.RuntimeContext
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::keeps request headers and provider options out of extract tracing",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Keeps_request_headers_and_provider_options_out_of_extract_tracing()
    {
        var request = InvoiceRequest("trace-extract-headers");
        request.Headers = new Dictionary<string, string>
        {
            ["authorization"] = "Bearer secret-token",
        };
        request.ProviderOptions = OperationJson.Parse(
            "{\"gateway\":{\"byok\":{\"openai\":[{\"apiKey\":\"secret-key\"}]}}}"
        );

        var messages = await CollectStartMessages("trace-extract-headers", () =>
            ExtractOperation.ExtractAsync(request)
        );

        var extract = Assert.IsType<ExtractStartEvent>(
            Assert
                .Single(messages, m => m.Type == TelemetryTracingEventType.ExperimentalExtract)
                .Event
        );
        Assert.Equal("{}", extract.ProviderOptions.GetRawText());
        Assert.Null(extract.Headers);
        Assert.DoesNotContain("secret", System.Text.Json.JsonSerializer.Serialize(extract));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::redacts file urls and failed download urls from extract tracing",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Redacts_file_urls_and_failed_download_urls_from_extract_tracing()
    {
        const string url = "https://files.example.com/invoice.pdf?X-Amz-Signature=secret";
        var request = InvoiceRequest("trace-extract-redact");
        request.Model = Setup();
        request.State = new DecisionStatePart[]
        {
            new DecisionFilePart("application/pdf", FilePartInput.FromUrl(new Uri(url))),
        };
        request.Download = _ =>
            throw new DownloadError(url, $"Download of {url} exceeded maximum size of 10 bytes.");

        DownloadError? error = null;
        var published = await Collect(async () =>
        {
            error = await Assert.ThrowsAsync<DownloadError>(() =>
                ExtractOperation.ExtractAsync(request)
            );
        });

        Assert.Equal(url, error!.Url);
        var mine = published
            .Where(item => item.Message.Type == TelemetryTracingEventType.ExperimentalExtract)
            .Where(item =>
                CallIdOf(item.Message) == "trace-extract-redact"
                || item.Message.Error is DownloadError
            )
            .Where(item => item.Channel is "start" or "error")
            .ToList();
        Assert.Equal(2, mine.Count);
        foreach (var item in mine)
        {
            var text =
                item.Channel == "start"
                    ? System.Text.Json.JsonSerializer.Serialize(item.Message.Event)
                    : System.Text.Json.JsonSerializer.Serialize(item.Message.Error)
                        + $" {item.Message.Error!.Message} {item.Message.Error.StackTrace}";
            Assert.DoesNotContain("secret", text);
        }
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::hides extraction inputs in tracing when inputs are not recorded",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Hides_extraction_inputs_in_tracing_when_inputs_are_not_recorded()
    {
        var request = InvoiceRequest("trace-extract-hidden");
        request.ReferenceDate = "2026-11-18";
        request.Telemetry = new TelemetryOptions { RecordInputs = false };

        var messages = await CollectStartMessages("trace-extract-hidden", () =>
            ExtractOperation.ExtractAsync(request)
        );

        var extract = Assert.IsType<ExtractStartEvent>(
            Assert
                .Single(messages, m => m.Type == TelemetryTracingEventType.ExperimentalExtract)
                .Event
        );
        Assert.Equal("", extract.State!.Text);
        Assert.Null(extract.ReferenceDate);
        Assert.Empty(extract.Fields);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::traces the current decide lifecycle sequence",
        Coverage = UpstreamCoverage.Covered,
        Note = "Node reports bindStart. The port reports start, because a DiagnosticListener has no bindStore."
    )]
    public async Task Traces_the_current_decide_lifecycle_sequence()
    {
        var sequence = await CollectEventSequence("trace-decide-sequence", () =>
            DecideOperation.DecideAsync(RefundRequest("trace-decide-sequence"))
        );

        Assert.Equal(["start experimental_decide", "asyncEnd experimental_decide"], sequence);
    }

    [Theory]
    [InlineData("an omitted allowlist", "omitted")]
    [InlineData("an empty allowlist", "empty")]
    [InlineData("an explicitly excluded field", "excluded")]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::excludes decide runtime context with $name",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Excludes_decide_runtime_context_with_name(string name, string kind)
    {
        Assert.NotEmpty(name);
        var callId = "trace-decide-excluded-" + kind;
        var request = RefundRequest(callId);
        request.Telemetry = kind switch
        {
            "omitted" => null,
            "empty" => new TelemetryOptions(),
            _ => new TelemetryOptions
            {
                IncludeRuntimeContext = new Dictionary<string, bool> { ["secret"] = false },
            },
        };
        request.RuntimeContext = new Dictionary<string, object?> { ["secret"] = "hidden" };

        var messages = await CollectStartMessages(callId, () =>
            DecideOperation.DecideAsync(request)
        );

        var decide = Assert.IsType<DecideEvent>(
            Assert
                .Single(messages, m => m.Type == TelemetryTracingEventType.ExperimentalDecide)
                .Event
        );
        Assert.Empty(decide.RuntimeContext);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::includes only allowlisted decide runtime context in tracing",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Includes_only_allowlisted_decide_runtime_context_in_tracing()
    {
        var request = RefundRequest("trace-decide-allowlisted");
        request.Telemetry = new TelemetryOptions
        {
            IncludeRuntimeContext = new Dictionary<string, bool> { ["requestId"] = true },
        };
        request.RuntimeContext = new Dictionary<string, object?>
        {
            ["requestId"] = "request-1",
            ["secret"] = "hidden",
        };

        var messages = await CollectStartMessages("trace-decide-allowlisted", () =>
            DecideOperation.DecideAsync(request)
        );

        var decide = Assert.IsType<DecideEvent>(
            Assert
                .Single(messages, m => m.Type == TelemetryTracingEventType.ExperimentalDecide)
                .Event
        );
        Assert.Equal(
            new Dictionary<string, object?> { ["requestId"] = "request-1" },
            decide.RuntimeContext
        );
    }

    private static string[] ToolSequence() =>
        [
            "start generateText",
            "start step",
            "start languageModelCall",
            "asyncEnd languageModelCall",
            "start executeTool",
            "asyncEnd executeTool",
            "asyncEnd step",
            "start step",
            "start languageModelCall",
            "asyncEnd languageModelCall",
            "asyncEnd step",
            "asyncEnd generateText",
        ];

    private static Tool WeatherTool() =>
        Tool.Function("weather", "Get the weather", WeatherSchema, (_, _) =>
            Task.FromResult("\"Weather in Paris: sunny\"")
        );

    [Fact]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::does not trace callback-only lifecycle events",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Does_not_trace_callback_only_lifecycle_events()
    {
        var startEvent = new GenerateTextStartEvent { CallId = "tracing-channel-callback-only" };

        var messages = await CollectStartMessages(startEvent.CallId, async () =>
        {
            var telemetry = TelemetryDispatcher.Create((TelemetryOptions?)null);

            if (telemetry.OnStart is { } onStart)
            {
                await onStart(startEvent, CancellationToken.None);
            }
        });

        Assert.Empty(messages);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::does not trace semantic spans when telemetry is disabled",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Does_not_trace_semantic_spans_when_telemetry_is_disabled()
    {
        var startEvent = new GenerateTextStartEvent { CallId = "tracing-channel-disabled" };

        var messages = await CollectStartMessages(startEvent.CallId, async () =>
        {
            var telemetry = TelemetryDispatcher.Create(new TelemetryOptions { IsEnabled = false });

            await telemetry.RunInTracingChannelSpan<object?>(
                TelemetryTracingEventType.GenerateText,
                startEvent,
                () => Task.FromResult<object?>(null)
            );
        });

        Assert.Empty(messages);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::traces semantic spans with telemetry settings",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit,
        Note = "The event is a GenerateTextStartEvent, because the port types its events. The SDK version is the real one, because the port does not mock it."
    )]
    public async Task Traces_semantic_spans_with_telemetry_settings()
    {
        var startEvent = new GenerateTextStartEvent { CallId = "tracing-channel-generate-text" };

        var messages = await CollectStartMessages(startEvent.CallId, async () =>
        {
            var telemetry = TelemetryDispatcher.Create(
                new TelemetryOptions { FunctionId = "enabled-function" }
            );

            await telemetry.RunInTracingChannelSpan<object?>(
                TelemetryTracingEventType.GenerateText,
                startEvent,
                () => Task.FromResult<object?>(null)
            );
        });

        var message = Assert.Single(messages);
        Assert.Equal("generateText", message.Type);
        var traced = Assert.IsType<GenerateTextStartEvent>(message.Event);
        Assert.Equal(AiSdkVersion.Version, traced.AiVersion);
        Assert.Equal("tracing-channel-generate-text", traced.CallId);
        Assert.Equal("enabled-function", traced.FunctionId);
        Assert.Null(traced.RecordInputs);
        Assert.Null(traced.RecordOutputs);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::traces the current generateText lifecycle sequence with a tool call",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit,
        Note = "Node reports bindStart. The port reports start, because a DiagnosticListener has no bindStore."
    )]
    public async Task Traces_the_current_generateText_lifecycle_sequence_with_a_tool_call()
    {
        var responseCount = 0;
        var model = new InterruptTestModel
        {
            Generate = (_, _) =>
                Task.FromResult(
                    responseCount++ switch
                    {
                        0 => ToolCallResponse(),
                        1 => TextResponse("It is sunny."),
                        _ => throw new InvalidOperationException(
                            $"Unexpected response count: {responseCount}"
                        ),
                    }
                ),
        };

        var sequence = await CollectEventSequence("tracing-channel-generate-text-tool", async () =>
        {
            await Client()
                .GenerateTextAsync(
                    new GenerateTextOptions
                    {
                        Model = model,
                        Prompt = "What is the weather in San Francisco?",
                        StopWhen = StopWhen.IsStepCount(3),
                        Tools = [WeatherTool()],
                        Telemetry = new TelemetryOptions
                        {
                            FunctionId = "tracing-channel-generate-text-test",
                        },
                        GenerateCallId = () => "tracing-channel-generate-text-tool",
                    }
                );
        });

        Assert.Equal(ToolSequence(), sequence);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::traces the current streamText lifecycle sequence with a tool call",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit,
        Note = "Node reports bindStart. The port reports start, because a DiagnosticListener has no bindStore."
    )]
    public async Task Traces_the_current_streamText_lifecycle_sequence_with_a_tool_call()
    {
        var responseCount = 0;
        var model = new InterruptTestModel
        {
            Stream = (_, _) =>
                Task.FromResult(
                    responseCount++ switch
                    {
                        0 => ToResult([
                            new StreamStartStreamPart([]),
                            new ToolCallStreamPart(
                                "weather-tool-call",
                                "weather",
                                "{\"city\":\"San Francisco\"}"
                            ),
                            new FinishStreamPart(FinishReason.ToolCalls, UsageOf(10), "tool_calls"),
                        ]),
                        1 => ToResult(TextStream("It is sunny.")),
                        _ => throw new InvalidOperationException(
                            $"Unexpected response count: {responseCount}"
                        ),
                    }
                ),
        };

        var sequence = await CollectEventSequence("tracing-channel-stream-text-tool", async () =>
        {
            var result = Client()
                .StreamTextAsync(
                    new StreamTextOptions
                    {
                        Model = model,
                        Prompt = "What is the weather in San Francisco?",
                        StopWhen = StopWhen.IsStepCount(3),
                        Tools = [WeatherTool()],
                        Telemetry = new TelemetryOptions
                        {
                            FunctionId = "tracing-channel-stream-text-test",
                        },
                        GenerateCallId = () => "tracing-channel-stream-text-tool",
                    }
                );

            await result.ConsumeStreamAsync();
        });

        Assert.Equal(
            ToolSequence().Select(item => item.Replace("generateText", "streamText")),
            sequence
        );
    }

    private static readonly string[] Docs =
    [
        "sunny day at the beach",
        "rainy day in the city",
        "cloudy day in the mountains",
    ];

    [Fact]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::includes the SDK version with allowlisted context on every tracing span",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Includes_the_SDK_version_with_allowlisted_context_on_every_tracing_span()
    {
        const string callId = "trace-version";
        var runtimeContext = new Dictionary<string, object?>
        {
            ["requestId"] = "request-1",
            ["accessToken"] = "secret-token",
        };
        var telemetry = new TelemetryOptions
        {
            IncludeRuntimeContext = new Dictionary<string, bool> { ["requestId"] = true },
        };
        var textTelemetry = new TelemetryOptions
        {
            IncludeRuntimeContext = telemetry.IncludeRuntimeContext,
            IncludeToolsContext = new Dictionary<string, IReadOnlyDictionary<string, bool>>
            {
                ["weather"] = new Dictionary<string, bool> { ["tenantId"] = true },
            },
        };
        var toolsContext = new Dictionary<string, JsonElement>
        {
            ["weather"] = JsonDocument
                .Parse("{\"tenantId\":\"tenant-1\",\"apiKey\":\"secret-key\"}")
                .RootElement.Clone(),
        };
        var generateModel = new InterruptTestModel
        {
            Generate = (_, _) => Task.FromResult(ToolCallResponse()),
        };
        var streamModel = new InterruptTestModel
        {
            Stream = (_, _) =>
                Task.FromResult(
                    ToResult([
                        new StreamStartStreamPart([]),
                        new ToolCallStreamPart(
                            "stream-weather-call",
                            "weather",
                            "{\"city\":\"Berlin\"}"
                        ),
                        new FinishStreamPart(FinishReason.ToolCalls, UsageOf(1), "tool_calls"),
                    ])
                ),
        };

        var messages = await CollectStartMessages(callId, async () =>
        {
            await Client()
                .GenerateTextAsync(
                    new GenerateTextOptions
                    {
                        Model = generateModel,
                        Prompt = "What is the weather?",
                        Tools = [WeatherTool()],
                        RuntimeContext = runtimeContext,
                        ToolsContext = toolsContext,
                        Telemetry = textTelemetry,
                        GenerateCallId = () => callId,
                    }
                );

            var result = Client()
                .StreamTextAsync(
                    new StreamTextOptions
                    {
                        Model = streamModel,
                        Prompt = "What is the weather?",
                        Tools = [WeatherTool()],
                        RuntimeContext = runtimeContext,
                        ToolsContext = toolsContext,
                        Telemetry = textTelemetry,
                        GenerateCallId = () => callId,
                    }
                );
            await result.ConsumeStreamAsync();

            var embeddingModel = new MockEmbeddingModel();
            await Embed.EmbedAsync(
                new EmbedRequest
                {
                    Model = embeddingModel,
                    Value = "sunny day at the beach",
                    RuntimeContext = runtimeContext,
                    Telemetry = telemetry,
                    GenerateCallId = () => callId,
                }
            );
            await EmbedMany.EmbedManyAsync(
                new EmbedManyRequest
                {
                    Model = embeddingModel,
                    Values = ["sunny day at the beach", "rainy day in the city"],
                    RuntimeContext = runtimeContext,
                    Telemetry = telemetry,
                    GenerateCallId = () => callId,
                }
            );
            await Rerank.RerankAsync(
                new RerankRequest
                {
                    Model = new MockRerankingModel(),
                    Documents = ["sunny day at the beach"],
                    Query = "weather",
                    RuntimeContext = runtimeContext,
                    Telemetry = telemetry,
                    GenerateCallId = () => callId,
                }
            );

            var decide = RefundRequest(callId);
            decide.RuntimeContext = runtimeContext;
            decide.Telemetry = telemetry;
            await DecideOperation.DecideAsync(decide);

            var extract = InvoiceRequest(callId);
            extract.RuntimeContext = runtimeContext;
            extract.Telemetry = telemetry;
            await ExtractOperation.ExtractAsync(extract);
        });

        var types = messages.Select(m => m.Type).ToHashSet();
        foreach (
            var type in new[]
            {
                TelemetryTracingEventType.GenerateText,
                TelemetryTracingEventType.StreamText,
                TelemetryTracingEventType.Step,
                TelemetryTracingEventType.LanguageModelCall,
                TelemetryTracingEventType.ExecuteTool,
                TelemetryTracingEventType.Embed,
                TelemetryTracingEventType.EmbedMany,
                TelemetryTracingEventType.Rerank,
                TelemetryTracingEventType.ExperimentalDecide,
                TelemetryTracingEventType.ExperimentalExtract,
            }
        )
        {
            Assert.Contains(type, types);
        }

        foreach (var message in messages)
        {
            Assert.Equal(AiSdkVersion.Version, AiVersionOf(message.Event));
        }

        var expectedContext = new Dictionary<string, object?> { ["requestId"] = "request-1" };
        foreach (
            var type in new[]
            {
                TelemetryTracingEventType.GenerateText,
                TelemetryTracingEventType.StreamText,
                TelemetryTracingEventType.Embed,
                TelemetryTracingEventType.EmbedMany,
                TelemetryTracingEventType.Rerank,
                TelemetryTracingEventType.ExperimentalDecide,
                TelemetryTracingEventType.ExperimentalExtract,
            }
        )
        {
            var start = messages.First(m => m.Type == type);
            Assert.Equal(expectedContext, RuntimeContextOf(start.Event));
        }

        foreach (
            var type in new[]
            {
                TelemetryTracingEventType.GenerateText,
                TelemetryTracingEventType.StreamText,
            }
        )
        {
            var start = Assert.IsType<GenerateTextStartEvent>(
                messages.First(m => m.Type == type).Event
            );
            Assert.Equal(
                "{\"weather\":{\"tenantId\":\"tenant-1\"}}",
                JsonSerializer.Serialize(start.ToolsContext)
            );
        }

        Assert.Equal(
            ["{\"tenantId\":\"tenant-1\"}", "{\"tenantId\":\"tenant-1\"}"],
            messages
                .Where(m => m.Type == TelemetryTracingEventType.ExecuteTool)
                .Select(m =>
                    JsonSerializer.Serialize(((ToolExecutionStartEvent)m.Event!).ToolContext)
                )
        );
    }

    private static string? AiVersionOf(object? @event) =>
        @event
            ?.GetType()
            .GetProperty(
                "AiVersion",
                System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.NonPublic
            )
            ?.GetValue(@event) as string;

    private static object? RuntimeContextOf(object? @event) =>
        @event?.GetType().GetProperty("RuntimeContext")?.GetValue(@event);

    [Fact]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::traces the current embed lifecycle sequence",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit,
        Note = "Node reports bindStart. The port reports start, because a DiagnosticListener has no bindStore."
    )]
    public async Task Traces_the_current_embed_lifecycle_sequence()
    {
        var sequence = await CollectEventSequence("trace-embed", async () =>
        {
            await Embed.EmbedAsync(
                new EmbedRequest
                {
                    Model = new MockEmbeddingModel(),
                    Value = "sunny day at the beach",
                    Telemetry = new TelemetryOptions { FunctionId = "tracing-channel-embed-test" },
                    GenerateCallId = () => "trace-embed",
                }
            );
        });

        Assert.Equal(["start embed", "asyncEnd embed"], sequence);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::traces the current embedMany lifecycle sequence",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit,
        Note = "Node reports bindStart. The port reports start, because a DiagnosticListener has no bindStore."
    )]
    public async Task Traces_the_current_embedMany_lifecycle_sequence()
    {
        var sequence = await CollectEventSequence("trace-embed-many", async () =>
        {
            await EmbedMany.EmbedManyAsync(
                new EmbedManyRequest
                {
                    Model = new MockEmbeddingModel { MaxEmbeddingsPerCall = 2 },
                    Values = Docs,
                    Telemetry = new TelemetryOptions
                    {
                        FunctionId = "tracing-channel-embed-many-test",
                    },
                    GenerateCallId = () => "trace-embed-many",
                }
            );
        });

        Assert.Equal(["start embedMany", "asyncEnd embedMany"], sequence);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::traces the current rerank lifecycle sequence",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit,
        Note = "Node reports bindStart. The port reports start, because a DiagnosticListener has no bindStore."
    )]
    public async Task Traces_the_current_rerank_lifecycle_sequence()
    {
        var sequence = await CollectEventSequence("trace-rerank", async () =>
        {
            await Rerank.RerankAsync(
                new RerankRequest
                {
                    Model = new MockRerankingModel(),
                    Documents = Docs,
                    Query = "weather",
                    Telemetry = new TelemetryOptions { FunctionId = "tracing-channel-rerank-test" },
                    GenerateCallId = () => "trace-rerank",
                }
            );
        });

        Assert.Equal(["start rerank", "asyncEnd rerank"], sequence);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::publishes the embed result on asyncEnd",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Publishes_the_embed_result_on_asyncEnd()
    {
        var messages = await CollectAsyncEndMessages("trace-embed-result", async () =>
        {
            await Embed.EmbedAsync(
                new EmbedRequest
                {
                    Model = new MockEmbeddingModel(),
                    Value = "sunny day at the beach",
                    Telemetry = new TelemetryOptions
                    {
                        FunctionId = "tracing-channel-embed-result-test",
                    },
                    GenerateCallId = () => "trace-embed-result",
                }
            );
        });

        var message = Assert.Single(messages, m => m.Type == "embed");
        var result = Assert.IsType<EmbedResult>(message.Result);
        Assert.Equal("sunny day at the beach", result.Value);
        Assert.Equal([0.1, 0.2, 0.3], result.Embedding);
        Assert.Equal(10, result.Usage.Tokens);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::publishes the embedMany result on asyncEnd",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Publishes_the_embedMany_result_on_asyncEnd()
    {
        var messages = await CollectAsyncEndMessages("trace-embed-many-result", async () =>
        {
            await EmbedMany.EmbedManyAsync(
                new EmbedManyRequest
                {
                    Model = new MockEmbeddingModel { MaxEmbeddingsPerCall = 2 },
                    Values = Docs,
                    Telemetry = new TelemetryOptions
                    {
                        FunctionId = "tracing-channel-embed-many-result-test",
                    },
                    GenerateCallId = () => "trace-embed-many-result",
                }
            );
        });

        var message = Assert.Single(messages, m => m.Type == "embedMany");
        var result = Assert.IsType<EmbedManyResult>(message.Result);
        Assert.Equal(Docs, result.Values);
        Assert.Equal(3, result.Embeddings.Count);
        Assert.All(result.Embeddings, e => Assert.Equal([0.1, 0.2, 0.3], e));
        Assert.Equal(20, result.Usage.Tokens);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::publishes the rerank result on asyncEnd",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Publishes_the_rerank_result_on_asyncEnd()
    {
        var messages = await CollectAsyncEndMessages("trace-rerank-result", async () =>
        {
            await Rerank.RerankAsync(
                new RerankRequest
                {
                    Model = new MockRerankingModel(),
                    Documents = Docs,
                    Query = "weather",
                    Telemetry = new TelemetryOptions
                    {
                        FunctionId = "tracing-channel-rerank-result-test",
                    },
                    GenerateCallId = () => "trace-rerank-result",
                }
            );
        });

        var message = Assert.Single(messages, m => m.Type == "rerank");
        var result = Assert.IsType<Vercel.AI.Operations.RerankResult>(message.Result);
        Assert.Equal(Docs, result.OriginalDocuments);
        Assert.Collection(
            result.Ranking,
            first =>
            {
                Assert.Equal(2, first.OriginalIndex);
                Assert.Equal(0.9, first.Score);
                Assert.Equal("cloudy day in the mountains", first.Document);
            },
            second =>
            {
                Assert.Equal(0, second.OriginalIndex);
                Assert.Equal(0.8, second.Score);
                Assert.Equal("sunny day at the beach", second.Document);
            }
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::traces one step span when a steering interrupt redoes the generateText model call",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit,
        Note = "Node reports bindStart. The port reports start, because a DiagnosticListener has no bindStore."
    )]
    public async Task Traces_one_step_span_when_a_steering_interrupt_redoes_the_generateText_model_call()
    {
        var steering = CreateSteering();
        var model = InterruptTestModel.HangingFirstCall(
            options =>
            {
                _ = steering.SteerAsync(
                    "Use Paris, Texas instead.",
                    new SteerOptions { Interrupt = true }
                );
            },
            _ => TextResponse("It is sunny.")
        );

        var sequence = await CollectEventSequence(
            "tracing-channel-generate-text-interrupt",
            async () =>
            {
                await Client()
                    .GenerateTextAsync(
                        new GenerateTextOptions
                        {
                            Model = model,
                            Prompt = PromptText,
                            ExperimentalSteering = steering,
                            Telemetry = new TelemetryOptions
                            {
                                FunctionId = "tracing-channel-generate-text-interrupt-test",
                            },
                            GenerateCallId = () => "tracing-channel-generate-text-interrupt",
                        }
                    );
            }
        );

        Assert.Equal(2, model.DoGenerateCalls.Count);
        Assert.Equal(
            [
                "start generateText",
                "start step",
                "start languageModelCall",
                "asyncEnd languageModelCall",
                "start languageModelCall",
                "asyncEnd languageModelCall",
                "asyncEnd step",
                "asyncEnd generateText",
            ],
            sequence
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::traces one step span when a steering interrupt redoes the streamText model call",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit,
        Note = "Node reports bindStart. The port reports start, because a DiagnosticListener has no bindStore."
    )]
    public async Task Traces_one_step_span_when_a_steering_interrupt_redoes_the_streamText_model_call()
    {
        var steering = CreateSteering();
        InterruptTestModel? model = null;
        model = new InterruptTestModel
        {
            Stream = (options, token) =>
            {
                if (model!.DoStreamCalls.Count == 1)
                {
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(1);
                        await steering.SteerAsync(
                            "Use Paris, Texas instead.",
                            new SteerOptions { Interrupt = true }
                        );
                    });
                    return Task.FromResult(
                        HangingStream(options, token, [new StreamStartStreamPart([])])
                    );
                }

                return Task.FromResult(ToResult(TextStream("It is sunny.")));
            },
        };

        var sequence = await CollectEventSequence(
            "tracing-channel-stream-text-interrupt",
            async () =>
            {
                var result = Client()
                    .StreamTextAsync(
                        new StreamTextOptions
                        {
                            Model = model,
                            Prompt = PromptText,
                            ExperimentalSteering = steering,
                            Telemetry = new TelemetryOptions
                            {
                                FunctionId = "tracing-channel-stream-text-interrupt-test",
                            },
                            GenerateCallId = () => "tracing-channel-stream-text-interrupt",
                        }
                    );

                await result.ConsumeStreamAsync();
            }
        );

        Assert.Equal(2, model.DoStreamCalls.Count);
        Assert.Equal(
            [
                "start streamText",
                "start step",
                "start languageModelCall",
                "asyncEnd languageModelCall",
                "start languageModelCall",
                "asyncEnd languageModelCall",
                "asyncEnd step",
                "asyncEnd streamText",
            ],
            sequence
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::propagates language model execution context through async work",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit,
        Note = "Node binds an AsyncLocalStorage to the start sub-channel. The port exposes the span as TracingChannel.Current."
    )]
    public async Task Propagates_language_model_execution_context_through_async_work()
    {
        TelemetryTracingChannelMessage? capturedContext = null;
        TelemetryTracingChannelMessage? published = null;

        await Collect(async () =>
        {
            var telemetry = TelemetryDispatcher.Create(
                new TelemetryOptions { FunctionId = "execution-function" }
            );

            await telemetry.RunInTracingChannelSpan(
                TelemetryTracingEventType.LanguageModelCall,
                new LanguageModelCallStartEvent
                {
                    CallId = "language-model-call",
                    Provider = "mock-provider",
                    ModelId = "mock-model",
                },
                async () =>
                {
                    await Task.Yield();
                    capturedContext = Vercel.AI.TracingChannel.Current;
                    published = capturedContext;
                    return "result";
                }
            );
        });

        Assert.NotNull(capturedContext);
        Assert.Equal("languageModelCall", capturedContext.Type);
        var traced = Assert.IsType<LanguageModelCallStartEvent>(capturedContext.Event);
        Assert.Equal(AiSdkVersion.Version, traced.AiVersion);
        Assert.Equal("language-model-call", traced.CallId);
        Assert.Equal("execution-function", traced.FunctionId);
        Assert.Equal("mock-model", traced.ModelId);
        Assert.Equal("mock-provider", traced.Provider);
        Assert.Null(traced.RecordInputs);
        Assert.Null(traced.RecordOutputs);
        Assert.Equal("result", published!.Result);
        Assert.Null(Vercel.AI.TracingChannel.Current);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::propagates tool execution context through async work",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit,
        Note = "Node binds an AsyncLocalStorage to the start sub-channel. The port exposes the span as TracingChannel.Current."
    )]
    public async Task Propagates_tool_execution_context_through_async_work()
    {
        TelemetryTracingChannelMessage? capturedContext = null;

        await Collect(async () =>
        {
            var telemetry = TelemetryDispatcher.Create(
                new TelemetryOptions { FunctionId = "execution-function" }
            );

            await telemetry.RunInTracingChannelSpan(
                TelemetryTracingEventType.ExecuteTool,
                new ToolExecutionStartEvent(
                    new GeneratedToolCall("tool-call-id", "mockTool", "{}"),
                    []
                )
                {
                    CallId = "tool-call",
                },
                async () =>
                {
                    await Task.Yield();
                    capturedContext = Vercel.AI.TracingChannel.Current;
                    return "result";
                }
            );
        });

        Assert.NotNull(capturedContext);
        Assert.Equal("executeTool", capturedContext.Type);
        var traced = Assert.IsType<ToolExecutionStartEvent>(capturedContext.Event);
        Assert.Equal(AiSdkVersion.Version, traced.AiVersion);
        Assert.Equal("tool-call", traced.CallId);
        Assert.Equal("execution-function", traced.FunctionId);
        Assert.Empty(traced.Messages);
        Assert.Equal("tool-call-id", traced.ToolCall.ToolCallId);
        Assert.Equal("mockTool", traced.ToolCall.ToolName);
        Assert.Null(traced.ToolContext);
        Assert.Equal("result", capturedContext.Result);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::preserves streamText tracing-channel parentage with a tool call",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit,
        Note = "Node reads the parent from the store bound to the start sub-channel. The port reads TracingChannel.Current."
    )]
    public async Task Preserves_streamText_tracing_channel_parentage_with_a_tool_call()
    {
        var responseCount = 0;
        var model = new InterruptTestModel
        {
            Stream = (_, _) =>
                Task.FromResult(
                    responseCount++ switch
                    {
                        0 => ToResult([
                            new StreamStartStreamPart([]),
                            new ToolCallStreamPart(
                                "weather-tool-call",
                                "weather",
                                "{\"city\":\"San Francisco\"}"
                            ),
                            new FinishStreamPart(FinishReason.ToolCalls, UsageOf(10), "tool_calls"),
                        ]),
                        1 => ToResult(TextStream("It is sunny.")),
                        _ => throw new InvalidOperationException(
                            $"Unexpected response count: {responseCount}"
                        ),
                    }
                ),
        };

        var parentage = await CollectParentage("tracing-channel-stream-text-parentage", async () =>
        {
            var result = Client()
                .StreamTextAsync(
                    new StreamTextOptions
                    {
                        Model = model,
                        Prompt = "What is the weather in San Francisco?",
                        StopWhen = StopWhen.IsStepCount(3),
                        Tools = [WeatherTool()],
                        Telemetry = new TelemetryOptions
                        {
                            FunctionId = "tracing-channel-stream-text-parentage-test",
                        },
                        GenerateCallId = () => "tracing-channel-stream-text-parentage",
                    }
                );

            await result.ConsumeStreamAsync();
        });

        Assert.Equal(
            [
                "streamText parent undefined",
                "step parent streamText",
                "languageModelCall parent step",
                "executeTool parent step",
                "step parent streamText",
                "languageModelCall parent step",
            ],
            parentage
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::preserves streamText abort behavior with a tracing-channel subscriber",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Preserves_streamText_abort_behavior_with_a_tracing_channel_subscriber()
    {
        var abortController = new AbortController();
        var onAbortEvents = new List<StreamAbortContext>();
        var onErrorEvents = new List<Exception>();

        async IAsyncEnumerable<LanguageModelStreamPart> Parts()
        {
            yield return new StreamStartStreamPart(null);
            abortController.Abort(new JsError("The user aborted a request."));
            await Task.CompletedTask;
            throw new OperationCanceledException("The user aborted a request.");
        }

        await Collect(async () =>
        {
            var result = StreamTextAbortSupport
                .Client()
                .StreamTextAsync(
                    new StreamTextOptions
                    {
                        AbortSignal = abortController.Signal,
                        Model = new StreamTextAbortSupport.ScriptedStreamModel(Parts),
                        Prompt = "test-input",
                        OnAbort = (e, _) =>
                        {
                            onAbortEvents.Add(e);
                            return Task.CompletedTask;
                        },
                        OnError = (e, _) =>
                        {
                            onErrorEvents.Add(e);
                            return Task.CompletedTask;
                        },
                        Telemetry = new TelemetryOptions
                        {
                            FunctionId = "tracing-channel-stream-text-abort-test",
                        },
                    }
                );

            await result.ConsumeStreamAsync();
        });

        Assert.Single(onAbortEvents);
        Assert.Empty(onErrorEvents);
    }
}
