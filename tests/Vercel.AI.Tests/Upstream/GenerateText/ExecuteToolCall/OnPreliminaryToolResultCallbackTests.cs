// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText.ExecuteToolCall;

/// <summary>
/// Port of <c>execute-tool-call.test.ts</c> &gt; <c>executeToolCall</c> &gt; <c>onPreliminaryToolResult callback</c>.
/// The port has no <c>executeToolCall</c> function, so each test runs one tool call through <c>StreamTextAsync</c>
/// and reads the preliminary <c>tool-result</c> parts that the callback feeds.
/// </summary>
public sealed class OnPreliminaryToolResultCallbackTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/execute-tool-call.test.ts::executeToolCall > onPreliminaryToolResult callback::";

    private const string Note =
        "The port has no executeToolCall function. The test observes the preliminary tool-result parts of streamText, which come from the same callback.";

    [Fact]
    [UpstreamTest(
        Prefix + "should be called for preliminary results",
        Coverage = UpstreamCoverage.Partial,
        Note = Note
    )]
    public async Task Should_be_called_for_preliminary_results()
    {
        var (preliminary, _) = await Run(Outputs("\"partial-1\"", "\"partial-2\""));

        Assert.Equal(2, preliminary.Count);
        Assert.Equal("\"partial-1\"", preliminary[0].OutputJson);
        Assert.True(preliminary[0].Preliminary);
        Assert.Equal("\"partial-2\"", preliminary[1].OutputJson);
        Assert.True(preliminary[1].Preliminary);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return final result even with preliminary results",
        Coverage = UpstreamCoverage.Partial,
        Note = Note
    )]
    public async Task Should_return_final_result_even_with_preliminary_results()
    {
        var (preliminary, final) = await Run(Outputs("\"partial-1\"", "\"test-final\""));

        Assert.Equal(2, preliminary.Count);
        Assert.Equal("\"partial-1\"", preliminary[0].OutputJson);
        Assert.Equal("\"test-final\"", preliminary[1].OutputJson);
        Assert.Equal("\"test-final\"", final.OutputJson);
        Assert.Null(final.Preliminary);
    }

    internal static Func<JsonElement, ToolExecutionOptions, IAsyncEnumerable<string>> Outputs(
        params string[] outputs
    )
    {
        return (_, _) => Stream(outputs);
    }

    private static async IAsyncEnumerable<string> Stream(
        string[] outputs,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        foreach (var output in outputs)
        {
            await Task.Yield();
            yield return output;
        }
    }

    private static async Task<(List<ExecutedTool> Preliminary, ExecutedTool Final)> Run(
        Func<JsonElement, ToolExecutionOptions, IAsyncEnumerable<string>> execute
    )
    {
        var tool = Tool.Function(
            "testTool",
            null,
            "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}",
            null
        );
        tool.ExecuteStream = execute;
        var result = new AiClient(
            GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })
        ).StreamTextAsync(
            new StreamTextOptions
            {
                Model = new TestLanguageModel
                {
                    StreamParts = new LanguageModelStreamPart[]
                    {
                        new ToolCallStreamPart("call-1", "testTool", "{ \"value\": \"test\" }"),
                        new FinishStreamPart(
                            FinishReason.ToolCalls,
                            new LanguageModelUsage(3, 10, 13),
                            "tool-calls"
                        ),
                    },
                },
                Prompt = "test-input",
                Tools = new[] { tool },
            }
        );

        var preliminary = new List<ExecutedTool>();
        await foreach (var part in result.Stream())
        {
            if (part is ToolResultPart { Result.Preliminary: true } toolResult)
            {
                preliminary.Add(toolResult.Result);
            }
        }

        var step = Assert.Single(await result.Steps);
        return (preliminary, Assert.Single(step.ToolResults));
    }
}
