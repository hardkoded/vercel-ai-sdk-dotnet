// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.GenerateText;
using OtelIntegration = Vercel.AI.OpenTelemetry.OpenTelemetry;
using OtelOptions = Vercel.AI.OpenTelemetry.OpenTelemetryOptions;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>onStepFinish</c>: the provider-executed tool tests.</summary>
public sealed class OnStepFinishTests
{
    private const string UpstreamCommit = "15bfd6b262f688cfb8281dbdb32c7dffe082337e";
    private const string Test =
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry::onStepFinish::";

    private static readonly LanguageModelUsage Usage = new(2, 1, 3);

    private static async Task<IReadOnlyList<MockSpan>> RunAsync(params GeneratedContent[] content)
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });
        await new AiClient(
            GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })
        ).GenerateTextAsync(
            new GenerateTextOptions
            {
                Model = new StepErrorModel(
                    doGenerate: (_, _) =>
                        Task.FromResult(
                            new LanguageModelGenerateResult(content, FinishReason.Stop, Usage)
                        )
                ),
                Prompt = "hello",
                Telemetry = new TelemetryOptions { Integrations = [integration] },
            }
        );
        return tracer.Spans;
    }

    [Fact]
    [UpstreamTest(
        Test + "records provider-executed tool results and observed definitions",
        Coverage = UpstreamCoverage.Partial,
        Commit = UpstreamCommit,
        Note = "The model call span has no gen_ai.output.messages or gen_ai.tool.definitions in the port, so the observed provider tool definitions and the output messages are not checked."
    )]
    public async Task Records_provider_executed_tool_results_and_observed_definitions()
    {
        var spans = await RunAsync(
            new GeneratedToolCall(
                "tc1",
                "mcp.ask_question",
                "{\"question\":\"What is this repository?\"}"
            )
            {
                ProviderExecuted = true,
                Dynamic = true,
            },
            new GeneratedToolResult(
                "tc1",
                "mcp.ask_question",
                "{\"answer\":\"An AI SDK repository.\"}"
            )
            {
                Dynamic = true,
            }
        );

        var chatSpan = spans[2];
        var toolSpan = spans[3];
        Assert.True(toolSpan.Ended);
        Assert.Equal("execute_tool mcp.ask_question", toolSpan.Name);
        Assert.Equal(
            new Dictionary<string, object?>
            {
                ["gen_ai.operation.name"] = "execute_tool",
                ["gen_ai.tool.call.arguments"] = "{\"question\":\"What is this repository?\"}",
                ["gen_ai.tool.call.id"] = "tc1",
                ["gen_ai.tool.name"] = "mcp.ask_question",
                ["gen_ai.tool.type"] = "extension",
            },
            toolSpan.InitAttributes
        );
        Assert.Equal(
            new Dictionary<string, object?>
            {
                ["gen_ai.tool.call.result"] = "{\"answer\":\"An AI SDK repository.\"}",
            },
            toolSpan.RuntimeAttributes
        );
        Assert.Equal(chatSpan.Activity.SpanId, toolSpan.Activity.ParentSpanId);
    }

    [Fact]
    [UpstreamTest(
        Test + "records only the final provider-executed tool result",
        Coverage = UpstreamCoverage.Partial,
        Commit = UpstreamCommit,
        Note = "The port's tool result has no preliminary flag, so only the final result is passed."
    )]
    public async Task Records_only_the_final_provider_executed_tool_result()
    {
        var spans = await RunAsync(
            new GeneratedToolCall("tc1", "code_execution", "{\"code\":\"1 + 1\"}")
            {
                ProviderExecuted = true,
            },
            new GeneratedToolResult("tc1", "code_execution", "{\"value\":2}")
        );

        Assert.Equal("{\"value\":2}", spans[3].Attributes["gen_ai.tool.call.result"]);
    }

    [Fact]
    [UpstreamTest(
        Test + "records provider-executed tool errors on the observed span",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Records_provider_executed_tool_errors_on_the_observed_span()
    {
        var error = new InvalidOperationException("provider tool failed");
        var spans = await RunAsync(
            new GeneratedToolCall("tc1", "web_search", "{\"query\":\"AI SDK\"}")
            {
                ProviderExecuted = true,
            },
            new GeneratedToolResult("tc1", "web_search", "null", isError: true) { Error = error }
        );

        var toolSpan = spans[3];
        Assert.Equal(System.Diagnostics.ActivityStatusCode.Error, toolSpan.Activity.Status);
        Assert.Equal(1, toolSpan.ExceptionCount);
        Assert.True(toolSpan.Ended);
    }
}
