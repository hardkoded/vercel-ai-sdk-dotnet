// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText</c> &gt; <c>tool callbacks</c>.</summary>
public sealed class ToolCallbacksTests
{
    private const string Schema =
        "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false}";

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > tool callbacks::should invoke callbacks in the correct order with the step tool context",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_invoke_callbacks_in_the_correct_order_with_the_step_tool_context()
    {
        var recorded = new List<string>();
        var model = new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[]
                {
                    new GeneratedToolCall("call-1", "test-tool", "{ \"value\": \"value\" }"),
                },
                FinishReason.ToolCalls,
                new LanguageModelUsage(3, 10, 13),
                "tool-calls"
            ),
        };

        void Record(string type, ToolExecutionOptions options, string? input = null)
        {
            Assert.Equal("call-1", options.ToolCallId);
            var message = Assert.IsType<UserModelMessage>(Assert.Single(options.Messages));
            Assert.Equal(
                "test-input",
                Assert.IsType<TextContentPart>(Assert.Single(message.Content)).Text
            );
            Assert.Equal("{\"prefix\":\"step-tool-context\"}", options.Context?.GetRawText());
            recorded.Add(type + (input == null ? string.Empty : ":" + input));
        }

        var tool = Tool.Function("test-tool", null, Schema, null);
        tool.ContextSchema = context =>
            context.TryGetProperty("prefix", out var prefix)
            && prefix.ValueKind == JsonValueKind.String
                ? context
                : throw new InvalidOperationException("invalid");
        tool.OnInputAvailable = options =>
        {
            Record("onInputAvailable", options, options.Input.GetProperty("value").GetString());
            return Task.CompletedTask;
        };
        tool.OnInputStart = options =>
        {
            Record("onInputStart", options);
            return Task.CompletedTask;
        };
        tool.OnInputDelta = options =>
        {
            Record("onInputDelta", options, options.InputTextDelta);
            return Task.CompletedTask;
        };

        await new AiClient(
            GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })
        ).GenerateTextAsync(
            new GenerateTextOptions
            {
                Model = model,
                Tools = [tool],
                RuntimeContext = new Dictionary<string, object?> { ["prefix"] = "runtime-context" },
                ToolsContext = new Dictionary<string, JsonElement>
                {
                    ["test-tool"] = JsonDocument
                        .Parse("{\"prefix\":\"initial-tool-context\"}")
                        .RootElement.Clone(),
                },
                PrepareStep = _ => new PrepareStepUpdate
                {
                    ToolsContext = new Dictionary<string, JsonElement>
                    {
                        ["test-tool"] = JsonDocument
                            .Parse("{\"prefix\":\"step-tool-context\"}")
                            .RootElement.Clone(),
                    },
                },
                ToolChoice = ToolChoice.Required,
                Prompt = "test-input",
            }
        );

        Assert.Equal(["onInputStart", "onInputAvailable:value"], recorded);
    }
}
