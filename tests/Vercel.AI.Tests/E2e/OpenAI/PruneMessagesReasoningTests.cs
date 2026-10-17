// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.E2e.OpenAI;

/// <summary>
/// Port of examples/ai-functions/src/generate-text/openai/prune-messages-reasoning.ts. The port has no
/// <c>pruneMessages</c>, so the reasoning parts are removed by hand (<c>reasoning: 'all'</c>).
/// </summary>
[Trait("Category", "RealModel")]
public sealed class PruneMessagesReasoningTests
{
    private const string WeatherSchema = "{\"type\":\"object\",\"properties\":{\"location\":{\"type\":\"string\"}},\"required\":[\"location\"]}";

    [SkippableFact]
    public async Task Should_generate_text_after_the_reasoning_has_been_pruned_from_the_history()
    {
        Skip.If(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OPENAI_API_KEY")));
        var model = OpenAIProvider.Create().ResponsesModel("gpt-5.1");
        var client = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "unused" }));
        var providerOptions = new Dictionary<string, JsonElement>
        {
            ["openai"] = JsonDocument.Parse("{\"reasoningEffort\":\"medium\"}").RootElement,
        };
        var tools = new[]
        {
            Tool.Function("weather", "Get the weather in a location", WeatherSchema, (_, _) => Task.FromResult("{\"temperature\":72}")),
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
        var pruned = new List<ModelMessage>();
        foreach (var message in messages)
        {
            if (message is not AssistantModelMessage assistant)
            {
                pruned.Add(message);
                continue;
            }

            var parts = assistant.Parts.Where(part => part is not AssistantReasoningPart).ToArray();
            if (parts.Length > 0)
            {
                pruned.Add(new AssistantModelMessage(parts));
            }
        }

        var result = await client.GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            ProviderOptions = providerOptions,
            Tools = tools,
            ToolChoice = ToolChoice.None,
            Messages = pruned,
        });

        Assert.False(string.IsNullOrWhiteSpace(result.Text));
    }
}
