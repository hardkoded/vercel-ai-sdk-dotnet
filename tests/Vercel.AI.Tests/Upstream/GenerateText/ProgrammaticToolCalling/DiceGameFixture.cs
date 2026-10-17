// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText.ProgrammaticToolCalling;

/// <summary>The "5 steps: code_execution triggers client tool across multiple turns (dice game fixture)" setup.</summary>
internal sealed class DiceGameFixture
{
    public const string CodeExecId = "srvtoolu_01CberhXc9TgYXrCZU8bQoks";
    public const string ContainerId = "container_011CWHQB57xVregfCMPrKgew";

    public const string Step1Text = "I'll help you simulate this dice game between two players! Let me run the game where both players roll dice each round until one player wins 3 rounds.";

    private static readonly string[][] RollDieCalls =
    {
        new[] { "toolu_01PMcE1JBKCeLjn83cgUCvR5", "player2", "toolu_01MZf5QJ1EQyd2yGyeLzBxAS", "player1" },
        new[] { "toolu_01UvVQ2xwA6preZppeajCkYK", "player1", "toolu_01BghspNownQFtRgv8jVicr3", "player2" },
        new[] { "toolu_01T7Upuuv8C71nq7DZ9ZPNQW", "player1", "toolu_016Da1tDet9Bf7dAdYTkF5Ar", "player2" },
        new[] { "toolu_01DiUBRds64sNajVPTZRrDSM", "player1", "toolu_01XQa3r3y1Fe8rnkGSncq626", "player2" },
    };

    public TestLanguageModel Model { get; } = new("claude-sonnet-4-5-20250929");

    public List<(string Player, int Value)> RollDieExecutions { get; } = new();

    public IReadOnlyList<LanguageModelCallOptions> DoGenerateCalls => Model.Calls;

    private List<(string Player, int Value)>? _rollDieExecutions;

    /// <summary>Records the executions of <c>rollDie</c> in <paramref name="executions"/> instead of <see cref="RollDieExecutions"/>.</summary>
    public DiceGameFixture WithRollDie(List<(string Player, int Value)> executions)
    {
        _rollDieExecutions = executions;
        return this;
    }

    public async Task<GenerateTextResult> RunAsync()
    {
        var responseCount = 0;
        var container = JsonDocument.Parse("{\"anthropic\":{\"container\":{\"id\":\"" + ContainerId + "\"}}}").RootElement.Clone();
        Model.OnGenerate = _ =>
        {
            var index = responseCount++;
            if (index == 4)
            {
                return new LanguageModelGenerateResult(
                    new GeneratedContent[]
                    {
                        new GeneratedToolResult(CodeExecId, "code_execution", "{\"type\":\"code_execution_result\",\"stdout\":\"\",\"stderr\":\"\",\"return_code\":0,\"content\":[]}", false),
                        new GeneratedText("**Game Over!**"),
                    },
                    FinishReason.Stop,
                    new LanguageModelUsage(4243, 229, 4472),
                    "stop");
            }

            var content = new List<GeneratedContent>();
            if (index == 0)
            {
                content.Add(new GeneratedText(Step1Text));
                content.Add(new GeneratedToolCall(CodeExecId, "code_execution", "{\"type\":\"programmatic-tool-call\",\"code\":\"game_loop()\"}") { ProviderExecuted = true });
            }

            var calls = RollDieCalls[index];
            content.Add(new GeneratedToolCall(calls[0], "rollDie", "{ \"player\": \"" + calls[1] + "\" }"));
            content.Add(new GeneratedToolCall(calls[2], "rollDie", "{ \"player\": \"" + calls[3] + "\" }"));
            return new LanguageModelGenerateResult(content, FinishReason.ToolCalls, new LanguageModelUsage(0, 0, 0), null, providerMetadata: container);
        };

        var client = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
        return await client.GenerateTextAsync(Options());
    }

    public GenerateTextOptions Options()
    {
        return new GenerateTextOptions
        {
            Model = Model,
            Tools = Tools(),
            Prompt = "Play a dice game between two players.",
            StopWhen = StopWhen.IsStepCount(10),
            PrepareStep = PrepareStep,
        };
    }

    public IReadOnlyList<Tool> Tools()
    {
        var codeExecution = Tool.ProviderDefined(
            "anthropic.code_execution_20250825",
            "code_execution",
            JsonDocument.Parse("{}").RootElement,
            JsonDocument.Parse("{\"type\":\"object\",\"properties\":{\"code\":{\"type\":\"string\"}},\"required\":[\"code\"]}").RootElement);
        var rollDie = Tool.Function(
            "rollDie",
            "Roll a die and return the result.",
            "{\"$schema\":\"http://json-schema.org/draft-07/schema#\",\"type\":\"object\",\"properties\":{\"player\":{\"type\":\"string\",\"enum\":[\"player1\",\"player2\"]}},\"required\":[\"player\"],\"additionalProperties\":false}",
            (input, _) =>
            {
                var player = input.GetProperty("player").GetString()!;
                var value = player == "player1" ? 6 : 3;
                (_rollDieExecutions ?? RollDieExecutions).Add((player, value));
                return Task.FromResult(value.ToString());
            });
        rollDie.ProviderOptions = JsonDocument.Parse("{\"anthropic\":{\"allowedCallers\":[\"code_execution_20250825\"]}}").RootElement.Clone();
        return new[] { codeExecution, rollDie };
    }

    // Forwards the container id from the previous step, like forwardAnthropicContainerIdFromLastStep.
    public static PrepareStepUpdate? PrepareStep(PrepareStepContext context)
    {
        if (context.StepNumber > 0 && context.Steps.Count > 0)
        {
            var metadata = context.Steps[context.Steps.Count - 1].ProviderMetadata;
            if (metadata is { ValueKind: JsonValueKind.Object } root
                && root.TryGetProperty("anthropic", out var anthropic)
                && anthropic.TryGetProperty("container", out var containerElement)
                && containerElement.TryGetProperty("id", out var id))
            {
                return new PrepareStepUpdate
                {
                    ProviderOptions = new Dictionary<string, JsonElement>
                    {
                        ["anthropic"] = JsonDocument.Parse("{\"container\":{\"id\":\"" + id.GetString() + "\"}}").RootElement.Clone(),
                    },
                };
            }
        }

        return null;
    }

    /// <summary>The prompt as upstream's snapshot shows it. Consecutive tool messages are one message with several results.</summary>
    public static JsonArray View(IReadOnlyList<ModelMessage> prompt)
    {
        var view = new JsonArray();
        JsonArray? toolContent = null;
        foreach (var message in prompt)
        {
            switch (message)
            {
                case UserModelMessage user:
                    toolContent = null;
                    view.Add(new JsonObject
                    {
                        ["role"] = "user",
                        ["content"] = new JsonArray(new JsonObject { ["type"] = "text", ["text"] = ((TextContentPart)user.Content[0]).Text }),
                    });
                    break;
                case AssistantModelMessage assistant:
                    toolContent = null;
                    var parts = new JsonArray();
                    foreach (var part in assistant.Parts)
                    {
                        switch (part)
                        {
                            case AssistantTextPart text:
                                parts.Add(new JsonObject { ["type"] = "text", ["text"] = text.Text });
                                break;
                            case AssistantToolCallPart call:
                                var item = new JsonObject
                                {
                                    ["type"] = "tool-call",
                                    ["toolCallId"] = call.ToolCall.ToolCallId,
                                    ["toolName"] = call.ToolCall.ToolName,
                                    ["input"] = JsonNode.Parse(call.ToolCall.ArgumentsJson),
                                };
                                if (call.ToolCall.ProviderExecuted)
                                {
                                    item["providerExecuted"] = true;
                                }

                                parts.Add(item);
                                break;
                        }
                    }

                    view.Add(new JsonObject { ["role"] = "assistant", ["content"] = parts });
                    break;
                case ToolModelMessage tool:
                    if (toolContent == null)
                    {
                        toolContent = new JsonArray();
                        view.Add(new JsonObject { ["role"] = "tool", ["content"] = toolContent });
                    }

                    toolContent.Add(new JsonObject
                    {
                        ["type"] = "tool-result",
                        ["toolCallId"] = tool.ToolCallId,
                        ["toolName"] = tool.ToolName,
                        ["output"] = new JsonObject { ["type"] = tool.OutputType, ["value"] = JsonNode.Parse(tool.OutputJson) },
                    });
                    break;
            }
        }

        return view;
    }
}
