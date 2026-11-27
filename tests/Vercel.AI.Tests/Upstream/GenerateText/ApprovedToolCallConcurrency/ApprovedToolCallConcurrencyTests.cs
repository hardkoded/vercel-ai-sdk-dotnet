// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Concurrent;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using static Vercel.AI.Tests.Upstream.GenerateText.ToolCallConcurrency.ToolCallConcurrencySupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.ApprovedToolCallConcurrency;

/// <summary>Port of <c>approved-tool-call-concurrency.test.ts</c> &gt; <c>approved tool call concurrency</c>, generateText half.</summary>
public sealed class ApprovedToolCallConcurrencyTests
{
    [Theory]
    [InlineData("generateText")]
    [InlineData("streamText")]
    [UpstreamTest(
        "packages/ai/src/generate-text/approved-tool-call-concurrency.test.ts::approved tool call concurrency::%s should execute approved calls in model order with reversed approval responses",
        Coverage = UpstreamCoverage.Covered,
        Note = "The two reversed approval responses are two consecutive approval response messages, because ToolApprovalResponseModelMessage holds one response."
    )]
    public async Task Should_execute_approved_calls_in_model_order_with_reversed_approval_responses(
        string api
    )
    {
        var releaseFirst = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var events = new ConcurrentQueue<string>();
        var document = "original";
        string? readDocument = null;
        var model = new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedText("Done") },
                FinishReason.Stop,
                new LanguageModelUsage(0, 0, 0)
            ),
            StreamParts = new LanguageModelStreamPart[]
            {
                new StreamStartStreamPart(Array.Empty<CallWarning>()),
                new TextStartStreamPart("text-1"),
                new TextDeltaStreamPart("text-1", "Done"),
                new TextEndStreamPart("text-1"),
                new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(0, 0, 0), null),
            },
        };
        var messages = new List<ModelMessage>
        {
            new UserModelMessage("Update the document and read it."),
            new AssistantModelMessage(
                new AssistantContentPart[]
                {
                    new AssistantToolCallPart(new GeneratedToolCall("call-1", "first", "{}")),
                    new AssistantToolCallPart(new GeneratedToolCall("call-2", "second", "{}")),
                    new AssistantToolApprovalRequestPart("approval-1", "call-1"),
                    new AssistantToolApprovalRequestPart("approval-2", "call-2"),
                }
            ),
            new ToolApprovalResponseModelMessage("approval-2", true),
            new ToolApprovalResponseModelMessage("approval-1", true),
        };
        var first = Tool.Function(
            "first",
            null,
            "{\"type\":\"object\",\"properties\":{}}",
            async (_, _) =>
            {
                events.Enqueue("first:start");
                await releaseFirst.Task;
                document = "updated";
                events.Enqueue("first:end");
                return document;
            }
        );
        first.NeedsApproval = true;
        var second = Tool.Function(
            "second",
            null,
            "{\"type\":\"object\",\"properties\":{}}",
            (_, _) =>
            {
                events.Enqueue("second:start");
                readDocument = document;
                events.Enqueue("second:end");
                return Task.FromResult(readDocument);
            }
        );
        second.NeedsApproval = true;

        var client = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
        Task<string> textTask;
        if (api == "generateText")
        {
            textTask = Text(
                client.GenerateTextAsync(
                    new GenerateTextOptions
                    {
                        Model = model,
                        Messages = messages,
                        ToolCallConcurrency = 1,
                        Tools = new[] { first, second },
                    }
                )
            );
        }
        else
        {
            textTask = client
                .StreamTextAsync(
                    new StreamTextOptions
                    {
                        Model = model,
                        Messages = messages,
                        ToolCallConcurrency = 1,
                        Tools = new[] { first, second },
                    }
                )
                .Text;
        }

        try
        {
            await WaitForEvents(events, "first:start");
        }
        finally
        {
            releaseFirst.SetResult(true);
            await textTask;
        }

        Assert.Equal(
            new[] { "first:start", "first:end", "second:start", "second:end" },
            events.ToArray()
        );
        Assert.Equal("updated", readDocument);
        Assert.Equal("Done", await textTask);
    }

    private static async Task<string> Text(Task<GenerateTextResult<string>> result)
    {
        return (await result).Text;
    }
}
