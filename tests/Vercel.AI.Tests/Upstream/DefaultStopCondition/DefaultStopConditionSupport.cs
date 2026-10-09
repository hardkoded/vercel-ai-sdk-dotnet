// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.DefaultStopCondition;

internal static class DefaultStopConditionSupport
{
    internal const string ModelId = "default-stop-model";

    internal static Tool ExecutableTool()
    {
        return Tool.Function("test", null, "{\"type\":\"object\",\"properties\":{}}", (_, _) => Task.FromResult("\"result\""));
    }

    internal static Tool ToolWithoutExecute()
    {
        return Tool.Function("test", null, "{\"type\":\"object\",\"properties\":{}}", null);
    }

    /// <summary>Calls the <c>test</c> tool on every step before <see cref="FinishAtStep"/>, then answers with text.</summary>
    internal sealed class ToolLoopModel : ILanguageModel
    {
        private static readonly LanguageModelUsage Usage = new LanguageModelUsage(1, 1, 2);

        public int FinishAtStep { get; set; } = int.MaxValue;

        public int Calls { get; private set; }

        public string SpecificationVersion => "V4";

        public string Provider => "test";

        public string ModelId => DefaultStopConditionSupport.ModelId;

        public Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
        {
            var step = ++Calls;
            GeneratedContent content = step == FinishAtStep
                ? new GeneratedText("done")
                : new GeneratedToolCall("call-" + step, "test", "{}");
            return Task.FromResult(new LanguageModelGenerateResult(
                new[] { content },
                step == FinishAtStep ? FinishReason.Stop : FinishReason.ToolCalls,
                Usage));
        }

        public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(
            LanguageModelCallOptions options,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var step = ++Calls;
            yield return new StreamStartStreamPart(null);
            if (step == FinishAtStep)
            {
                yield return new TextStartStreamPart("text");
                yield return new TextDeltaStreamPart("text", "done");
                yield return new TextEndStreamPart("text");
                yield return new FinishStreamPart(FinishReason.Stop, Usage);
            }
            else
            {
                yield return new ToolCallStreamPart("call-" + step, "test", "{}");
                yield return new FinishStreamPart(FinishReason.ToolCalls, Usage);
            }

            await Task.CompletedTask;
        }
    }

    /// <summary>Counts evaluations and delegates to the wrapped condition.</summary>
    internal sealed class CountingCondition : StopCondition
    {
        private readonly StopCondition _inner;

        public CountingCondition(StopCondition inner)
        {
            _inner = inner;
        }

        public int Calls { get; private set; }

        public override bool ShouldStop(IReadOnlyList<StepResult> steps)
        {
            Calls++;
            return _inner.ShouldStop(steps);
        }
    }

    /// <summary>Records what the logger and the default emitters receive for the model of this test.</summary>
    internal sealed class LogRecorder : IDisposable
    {
        private readonly object _gate = new object();
        private readonly List<LogWarningsOptions> _logged = new List<LogWarningsOptions>();
        private int _emitted;

        public LogRecorder()
        {
            LogWarnings.ResetState();
            LogWarnings.Logger = new Action<LogWarningsOptions>(Record);
            LogWarnings.ProcessEmitWarning = (_, _) => CountEmit();
            LogWarnings.ConsoleWarn = _ => CountEmit();
        }

        public IReadOnlyList<LogWarningsOptions> Logged
        {
            get
            {
                lock (_gate)
                {
                    return _logged.Where(options => options.Model == ModelId).ToList();
                }
            }
        }

        public int Emitted
        {
            get
            {
                lock (_gate)
                {
                    return _emitted;
                }
            }
        }

        public void Disable()
        {
            LogWarnings.Logger = false;
        }

        public void Dispose()
        {
            LogWarnings.Logger = null;
            LogWarnings.ProcessEmitWarning = delegate { };
            LogWarnings.ConsoleWarn = delegate { };
            LogWarnings.ResetState();
        }

        private void Record(LogWarningsOptions options)
        {
            lock (_gate)
            {
                _logged.Add(options);
            }
        }

        private void CountEmit()
        {
            lock (_gate)
            {
                _emitted++;
            }
        }
    }

    internal static AiClient Client()
    {
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
    }

    internal static async Task<IReadOnlyList<StepResult>> Run(
        string method,
        ToolLoopModel model,
        Tool tool,
        StopCondition? stopWhen = null)
    {
        var tools = new[] { tool };
        const string prompt = "Call the test tool.";
        switch (method)
        {
            case "generateText":
                return (await Client().GenerateTextAsync(new GenerateTextOptions { Model = model, Tools = tools, StopWhen = stopWhen, Prompt = prompt })).Steps;
            case "streamText":
                return await Consume(Client().StreamTextAsync(new StreamTextOptions { Model = model, Tools = tools, StopWhen = stopWhen, Prompt = prompt }));
            case "agent.generate":
                return (await new Agent(new AgentOptions { Model = model, Tools = tools, StopWhen = stopWhen }, Client()).GenerateAsync(prompt)).Steps;
            default:
                return await Consume(new Agent(new AgentOptions { Model = model, Tools = tools, StopWhen = stopWhen }, Client()).Stream(prompt));
        }
    }

    private static async Task<IReadOnlyList<StepResult>> Consume(StreamTextResult result)
    {
        await foreach (var _ in result.Stream())
        {
        }

        return await result.Steps;
    }
}
