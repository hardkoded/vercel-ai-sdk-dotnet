// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.GenerateText.ProgrammaticToolCalling;

/// <summary>The streamText "5 steps: code_execution triggers client tool across multiple turns (dice game fixture)" setup.</summary>
internal sealed class StreamDiceGameFixture : ILanguageModel
{
    public const string CodeExecId = "srvtoolu_01MzSrFWsmzBdcoQkGWLyRjK";
    public const string ContainerId = "container_011CWHPPTDTn1XufeRB9uHeH";

    public const string Step1Text = "I'll help you simulate this game between two players where one is using a loaded die.";

    private static readonly string[][] RollDieCalls =
    {
        new[] { "toolu_019jKkXz4jAdwHweHBw92CVY", "player1" },
        new[] { "toolu_015dGLMbwBKv1ZRQr6KdJzeH", "player2" },
        new[] { "toolu_01YYqBNq5mk1wMtv3PAqY44m", "player1" },
        new[] { "toolu_018WxjDkQG8h7i63poySGT2x", "player2" },
    };

    private int _responseCount;

    public string SpecificationVersion => "V4";

    public string Provider => "test";

    public string ModelId => "claude-sonnet-4-5-20250929";

    public List<LanguageModelCallOptions> DoStreamCalls { get; } = new();

    public List<(string Player, int Value)> RollDieExecutions { get; } = new();

    public Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    public Task<LanguageModelStreamResult> DoStreamAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        DoStreamCalls.Add(options);
        var index = _responseCount++;
        return Task.FromResult(new LanguageModelStreamResult(Parts(index)));
    }

    public async Task<StreamTextResult> RunAsync()
    {
        var client = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
        var result = client.StreamTextAsync(new StreamTextOptions
        {
            Model = this,
            Tools = new DiceGameFixture().WithRollDie(RollDieExecutions).Tools(),
            Prompt = "Play a dice game between two players.",
            StopWhen = StopWhen.IsStepCount(10),
            PrepareStep = DiceGameFixture.PrepareStep,
        });
        await result.Steps;
        return result;
    }

    private static async IAsyncEnumerable<LanguageModelStreamPart> Parts(int index, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        if (index == 4)
        {
            yield return new ToolResultStreamPart(CodeExecId, "code_execution", JsonDocument.Parse("{\"type\":\"code_execution_result\",\"stdout\":\"\",\"stderr\":\"\",\"return_code\":0,\"content\":[]}").RootElement.Clone());
            yield return new TextDeltaStreamPart("2", "## Game Results");
            yield return new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(4551, 197, 4748), "stop");
            yield break;
        }

        if (index == 0)
        {
            yield return new TextDeltaStreamPart("1", Step1Text);
            yield return new ToolCallStreamPart(CodeExecId, "code_execution", "{\"type\":\"programmatic-tool-call\",\"code\":\"game_loop()\"}") { ProviderExecuted = true };
        }

        yield return new ToolCallStreamPart(RollDieCalls[index][0], "rollDie", "{ \"player\": \"" + RollDieCalls[index][1] + "\" }");
        yield return new FinishStreamPart(
            FinishReason.ToolCalls,
            new LanguageModelUsage(0, 0, 0),
            null,
            JsonDocument.Parse("{\"anthropic\":{\"container\":{\"id\":\"" + ContainerId + "\"}}}").RootElement.Clone());
    }
}
