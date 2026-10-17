// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.GenerateText;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.E2e.OpenAI.PruneMessagesReasoning;

/// <summary>Port of examples/ai-functions/src/generate-text/openai/prune-messages-reasoning.ts.</summary>
[Trait("Category", "RealModel")]
public sealed class PruneMessagesReasoningTests
{
    [SkippableFact]
    public async Task Should_generate_text_after_pruning_the_reasoning_from_the_history()
    {
        Skip.If(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OPENAI_API_KEY")));
        var client = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "unused" }));
        var model = OpenAIProvider.Create().ResponsesModel("gpt-5.1");
        var providerOptions = new Dictionary<string, JsonElement>
        {
            ["openai"] = JsonDocument.Parse("{\"reasoningEffort\":\"medium\"}").RootElement.Clone(),
        };
        var tools = new[]
        {
            Tool.Function(
                "weather",
                "Get the weather in a location",
                "{\"type\":\"object\",\"properties\":{\"location\":{\"type\":\"string\"}},\"required\":[\"location\"]}",
                (_, _) => Task.FromResult("{\"temperature\":72}")),
        };
        var messages = new List<ModelMessage>
        {
            new UserModelMessage("Use the weather tool to get the weather in San Francisco and New York, then compare them."),
        };

        // step 1: reasoning + tool calls
        var toolStep = await client.GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            ProviderOptions = providerOptions,
            Tools = tools,
            ToolChoice = ToolChoice.Required,
            Messages = messages,
        });
        messages.AddRange(toolStep.ResponseMessages);

        // step 2: reasoning + final answer
        var answerStep = await client.GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            ProviderOptions = providerOptions,
            Tools = tools,
            ToolChoice = ToolChoice.None,
            Messages = messages,
        });
        messages.AddRange(answerStep.ResponseMessages);
        messages.Add(new UserModelMessage("Which city should I visit this weekend?"));

        // remove the reasoning from the history.
        // the stored assistant messages can no longer be referenced by id,
        // because OpenAI requires their reasoning items to be sent with them.
        var result = await client.GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            ProviderOptions = providerOptions,
            Tools = tools,
            ToolChoice = ToolChoice.None,
            Messages = Ai.PruneMessages(new PruneMessagesOptions { Messages = messages, Reasoning = PruneReasoning.All }),
        });

        Assert.False(string.IsNullOrEmpty(result.Text));
    }
}
