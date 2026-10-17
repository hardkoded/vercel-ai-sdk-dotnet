// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText;

/// <summary>
/// The "5 steps: code_execution triggers client tool across multiple turns (dice game fixture)" scenario of
/// <c>generate-text.test.ts</c> and <c>stream-text.test.ts</c>, run through <c>GenerateTextAsync</c> and <c>StreamTextAsync</c>.
/// </summary>
internal static class DiceGameFixture
{
    internal const string GenerateCodeExecId = "srvtoolu_01CberhXc9TgYXrCZU8bQoks";
    internal const string GenerateContainerId = "container_011CWHQB57xVregfCMPrKgew";
    internal const string StreamCodeExecId = "srvtoolu_01MzSrFWsmzBdcoQkGWLyRjK";
    internal const string StreamContainerId = "container_011CWHPPTDTn1XufeRB9uHeH";

    private const string RollDieSchema = "{\"$schema\":\"http://json-schema.org/draft-07/schema#\",\"type\":\"object\",\"properties\":{\"player\":{\"type\":\"string\",\"enum\":[\"player1\",\"player2\"]}},\"required\":[\"player\"],\"additionalProperties\":false}";

    internal sealed record PrepareCall(string ModelId, int StepNumber, IReadOnlyList<StepResult> Steps, IReadOnlyList<ModelMessage> Messages);

    internal sealed class Run
    {
        public List<LanguageModelCallOptions> ModelCalls { get; } = new();

        public List<PrepareCall> PrepareCalls { get; } = new();

        public List<StepResult> StepResults { get; } = new();

        public List<string> RollDieExecutions { get; } = new();
    }

    internal static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    /// <summary>Runs the generateText scenario to the end.</summary>
    internal static async Task<Run> RunGenerateAsync()
    {
        var run = new Run();
        var responseCount = 0;
        var model = new TestLanguageModel("claude-sonnet-4-5-20250929")
        {
            OnGenerate = options =>
            {
                run.ModelCalls.Add(options);
                return GenerateResponse(responseCount++);
            },
        };
        await new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })).GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "Play a dice game between two players.",
            Tools = Tools(run),
            StopWhen = StopWhen.IsStepCount(10),
            OnStepEnd = (step, _) =>
            {
                run.StepResults.Add(step);
                return Task.CompletedTask;
            },
            PrepareStep = context => PrepareStep(run, context),
        });
        return run;
    }

    /// <summary>Runs the streamText scenario to the end.</summary>
    internal static async Task<Run> RunStreamAsync()
    {
        var run = new Run();
        var responseCount = 0;
        var model = new ScriptedStreamModel(options =>
        {
            run.ModelCalls.Add(options);
            return StreamResponse(responseCount++);
        });
        var result = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })).StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "Play a dice game between two players.",
            Tools = Tools(run),
            StopWhen = StopWhen.IsStepCount(10),
            OnStepEnd = (step, _) =>
            {
                run.StepResults.Add(step);
                return Task.CompletedTask;
            },
            PrepareStep = context => PrepareStep(run, context),
        });
        await foreach (var _ in result.Stream())
        {
        }

        return run;
    }

    /// <summary>
    /// Renders messages as one string each so a prompt can be compared in one assertion. Consecutive tool messages
    /// are joined, because upstream sends the results of one step as a single tool message.
    /// </summary>
    internal static string[] Render(IReadOnlyList<ModelMessage> messages)
    {
        var lines = new List<string>();
        var results = new List<string>();
        void FlushResults()
        {
            if (results.Count > 0)
            {
                lines.Add("tool|" + string.Join(";", results));
                results.Clear();
            }
        }

        foreach (var message in messages)
        {
            if (message is ToolModelMessage tool)
            {
                results.Add(Result(tool.ToolCallId, tool.ToolName, tool.OutputType, tool.OutputJson));
                continue;
            }

            FlushResults();
            switch (message)
            {
                case UserModelMessage user:
                    lines.Add("user|" + string.Join(";", user.Content.Select(part => "text:" + Assert.IsType<TextContentPart>(part).Text)));
                    break;
                case AssistantModelMessage assistant:
                    lines.Add("assistant|" + string.Join(";", assistant.Parts.Select(RenderPart)));
                    break;
                default:
                    throw new InvalidOperationException("Unexpected message " + message.GetType().Name);
            }
        }

        FlushResults();
        return lines.ToArray();
    }

    /// <summary>Checks the tools of the first step: the provider-defined code execution tool and the rollDie function tool.</summary>
    internal static void AssertTools(IReadOnlyList<LanguageModelTool>? tools)
    {
        Assert.NotNull(tools);
        Assert.Equal(2, tools!.Count);
        var codeExecution = tools[0];
        Assert.True(codeExecution.IsProviderDefined);
        Assert.Equal("anthropic.code_execution_20250825", codeExecution.ProviderToolId);
        Assert.Equal("code_execution", codeExecution.Name);
        JsonAssert.Equal(codeExecution.Args, "{}");
        var rollDie = tools[1];
        Assert.False(rollDie.IsProviderDefined);
        Assert.Equal("Roll a die and return the result.", rollDie.Description);
        JsonAssert.Equal(rollDie.InputSchema, RollDieSchema);
        Assert.Equal("rollDie", rollDie.Name);
        JsonAssert.Equal(rollDie.ProviderOptions!.Value, "{\"anthropic\":{\"allowedCallers\":[\"code_execution_20250825\"]}}");
    }

    /// <summary>Checks that the provider options of a model call forward the container id.</summary>
    internal static void AssertContainerForwarded(LanguageModelCallOptions call, string containerId)
    {
        var options = Assert.IsAssignableFrom<IReadOnlyDictionary<string, JsonElement>>(call.ProviderOptions);
        Assert.Equal(new[] { "anthropic" }, options.Keys.ToArray());
        JsonAssert.Equal(options["anthropic"], "{\"container\":{\"id\":\"" + containerId + "\"}}");
    }

    /// <summary>Checks the provider metadata of a step carries the container id.</summary>
    internal static void AssertContainerMetadata(StepResult step, string containerId)
    {
        JsonAssert.Equal(step.ProviderMetadata!.Value, "{\"anthropic\":{\"container\":{\"id\":\"" + containerId + "\"}}}");
    }

    internal static string User(string text) => "user|text:" + text;

    internal static string Assistant(params string[] parts) => "assistant|" + string.Join(";", parts);

    internal static string ToolResults(params string[] results) => "tool|" + string.Join(";", results);

    internal static string Text(string text) => "text:" + text;

    internal static string Call(string id, string name, string inputJson, bool providerExecuted = false)
    {
        return "call:" + id + ":" + name + ":" + Normalize(inputJson) + ":" + (providerExecuted ? "provider" : "client");
    }

    internal static string Result(string id, string name, string outputType, string outputJson)
    {
        return id + ":" + name + ":" + outputType + ":" + Normalize(outputJson);
    }

    private static string RenderPart(AssistantContentPart part)
    {
        return part switch
        {
            AssistantTextPart text => Text(text.Text),
            AssistantToolCallPart call => Call(call.ToolCall.ToolCallId, call.ToolCall.ToolName, call.ToolCall.ArgumentsJson, call.ToolCall.ProviderExecuted),
            _ => throw new InvalidOperationException("Unexpected part " + part.GetType().Name),
        };
    }

    private static string Normalize(string json)
    {
        return JsonNode.Parse(json)!.ToJsonString();
    }

    private static IReadOnlyList<Tool> Tools(Run run)
    {
        var rollDie = Tool.Function(
            "rollDie",
            "Roll a die and return the result.",
            RollDieSchema,
            (input, _) =>
            {
                var player = input.GetProperty("player").GetString()!;
                run.RollDieExecutions.Add(player);
                return Task.FromResult(player == "player1" ? "6" : "3");
            });
        rollDie.ProviderOptions = Json("{\"anthropic\":{\"allowedCallers\":[\"code_execution_20250825\"]}}");
        return new[]
        {
            Tool.ProviderDefined("anthropic.code_execution_20250825", "code_execution", Json("{}")),
            rollDie,
        };
    }

    // Forwards the container id of the last step, as forwardAnthropicContainerIdFromLastStep does.
    private static PrepareStepUpdate? PrepareStep(Run run, PrepareStepContext context)
    {
        run.PrepareCalls.Add(new PrepareCall(context.Model!.ModelId, context.StepNumber, context.Steps.ToArray(), context.Messages.ToArray()));
        if (context.StepNumber > 0 && context.Steps.Count > 0)
        {
            var metadata = context.Steps[context.Steps.Count - 1].ProviderMetadata;
            if (metadata is { ValueKind: JsonValueKind.Object } root
                && root.TryGetProperty("anthropic", out var anthropic)
                && anthropic.TryGetProperty("container", out var container)
                && container.TryGetProperty("id", out var id)
                && id.ValueKind == JsonValueKind.String)
            {
                return new PrepareStepUpdate
                {
                    ProviderOptions = new Dictionary<string, JsonElement>
                    {
                        ["anthropic"] = Json("{\"container\":{\"id\":" + JsonSerializer.Serialize(id.GetString()) + "}}"),
                    },
                };
            }
        }

        return null;
    }

    private static LanguageModelGenerateResult GenerateResponse(int index)
    {
        JsonElement? container = Json("{\"anthropic\":{\"container\":{\"id\":\"" + GenerateContainerId + "\"}}}");
        switch (index)
        {
            case 0:
                return new LanguageModelGenerateResult(
                    new GeneratedContent[]
                    {
                        new GeneratedText("I'll help you simulate this dice game between two players! Let me run the game where both players roll dice each round until one player wins 3 rounds."),
                        new GeneratedToolCall(GenerateCodeExecId, "code_execution", "{\"type\":\"programmatic-tool-call\",\"code\":\"game_loop()\"}") { ProviderExecuted = true },
                        new GeneratedToolCall("toolu_01PMcE1JBKCeLjn83cgUCvR5", "rollDie", "{ \"player\": \"player2\" }"),
                        new GeneratedToolCall("toolu_01MZf5QJ1EQyd2yGyeLzBxAS", "rollDie", "{ \"player\": \"player1\" }"),
                    },
                    FinishReason.ToolCalls,
                    new LanguageModelUsage(3369, 577, 3946),
                    providerMetadata: container);
            case 1:
                return RollRound(container, "toolu_01UvVQ2xwA6preZppeajCkYK", "player1", "toolu_01BghspNownQFtRgv8jVicr3", "player2");
            case 2:
                return RollRound(container, "toolu_01T7Upuuv8C71nq7DZ9ZPNQW", "player1", "toolu_016Da1tDet9Bf7dAdYTkF5Ar", "player2");
            case 3:
                return RollRound(container, "toolu_01DiUBRds64sNajVPTZRrDSM", "player1", "toolu_01XQa3r3y1Fe8rnkGSncq626", "player2");
            case 4:
                return new LanguageModelGenerateResult(
                    new GeneratedContent[]
                    {
                        new GeneratedToolResult(GenerateCodeExecId, "code_execution", "{\"type\":\"code_execution_result\",\"stdout\":\"PLAYER 1 WINS THE GAME!\",\"stderr\":\"\",\"return_code\":0,\"content\":[]}"),
                        new GeneratedText("**Game Over!**\n\nPlayer 1 dominated this game with a decisive 3-0 victory!"),
                    },
                    FinishReason.Stop,
                    new LanguageModelUsage(4243, 229, 4472),
                    "stop");
            default:
                throw new InvalidOperationException("Unexpected response count: " + index);
        }
    }

    private static LanguageModelGenerateResult RollRound(JsonElement? container, string firstId, string firstPlayer, string secondId, string secondPlayer)
    {
        return new LanguageModelGenerateResult(
            new GeneratedContent[]
            {
                new GeneratedToolCall(firstId, "rollDie", "{ \"player\": \"" + firstPlayer + "\" }"),
                new GeneratedToolCall(secondId, "rollDie", "{ \"player\": \"" + secondPlayer + "\" }"),
            },
            FinishReason.ToolCalls,
            new LanguageModelUsage(0, 0, 0),
            providerMetadata: container);
    }

    private static IAsyncEnumerable<LanguageModelStreamPart> StreamResponse(int index)
    {
        JsonElement? container = Json("{\"anthropic\":{\"container\":{\"id\":\"" + StreamContainerId + "\"}}}");
        switch (index)
        {
            case 0:
                return Parts(
                    new TextStartStreamPart("1"),
                    new TextDeltaStreamPart("1", "I'll help you simulate this game between two players where one is using a loaded die."),
                    new TextEndStreamPart("1"),
                    new ToolCallStreamPart(StreamCodeExecId, "code_execution", "{\"type\":\"programmatic-tool-call\",\"code\":\"game_loop()\"}") { ProviderExecuted = true },
                    new ToolCallStreamPart("toolu_019jKkXz4jAdwHweHBw92CVY", "rollDie", "{ \"player\": \"player1\" }"),
                    new FinishStreamPart(FinishReason.ToolCalls, new LanguageModelUsage(3369, 725, 4094), null, container));
            case 1:
                return Parts(
                    new ToolCallStreamPart("toolu_015dGLMbwBKv1ZRQr6KdJzeH", "rollDie", "{ \"player\": \"player2\" }"),
                    new FinishStreamPart(FinishReason.ToolCalls, new LanguageModelUsage(0, 0, 0), null, container));
            case 2:
                return Parts(
                    new ToolCallStreamPart("toolu_01YYqBNq5mk1wMtv3PAqY44m", "rollDie", "{ \"player\": \"player1\" }"),
                    new FinishStreamPart(FinishReason.ToolCalls, new LanguageModelUsage(0, 0, 0), null, container));
            case 3:
                return Parts(
                    new ToolCallStreamPart("toolu_018WxjDkQG8h7i63poySGT2x", "rollDie", "{ \"player\": \"player2\" }"),
                    new FinishStreamPart(FinishReason.ToolCalls, new LanguageModelUsage(0, 0, 0), null, container));
            case 4:
                return Parts(
                    new ToolResultStreamPart(StreamCodeExecId, "code_execution", Json("{\"type\":\"code_execution_result\",\"stdout\":\"Player 2 wins\",\"stderr\":\"\",\"return_code\":0,\"content\":[]}")),
                    new TextStartStreamPart("2"),
                    new TextDeltaStreamPart("2", "## Game Results\n\n**Player 2 wins the game 3-2!**"),
                    new TextEndStreamPart("2"),
                    new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(4551, 197, 4748), "stop"));
            default:
                throw new InvalidOperationException("Unexpected response count: " + index);
        }
    }

    private static async IAsyncEnumerable<LanguageModelStreamPart> Parts(params LanguageModelStreamPart[] parts)
    {
        yield return new StreamStartStreamPart(null);
        foreach (var part in parts)
        {
            yield return part;
        }

        await Task.CompletedTask;
    }

    private sealed class ScriptedStreamModel : ILanguageModel
    {
        private readonly Func<LanguageModelCallOptions, IAsyncEnumerable<LanguageModelStreamPart>> _next;

        public ScriptedStreamModel(Func<LanguageModelCallOptions, IAsyncEnumerable<LanguageModelStreamPart>> next)
        {
            _next = next;
        }

        public string SpecificationVersion => "V4";

        public string Provider => "test";

        public string ModelId => "claude-sonnet-4-5-20250929";

        public Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<LanguageModelStreamResult> DoStreamAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
        {
            return Task.FromResult(new LanguageModelStreamResult(_next(options)));
        }
    }
}
