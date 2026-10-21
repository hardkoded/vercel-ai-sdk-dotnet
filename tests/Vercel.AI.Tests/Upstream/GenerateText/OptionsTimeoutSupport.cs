// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.GenerateText;

/// <summary>Shared pieces of the <c>options.timeout</c> tests of <c>generateText</c> and <c>streamText</c>.</summary>
internal static class OptionsTimeoutSupport
{
    internal const string ValueSchema = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}";

    internal static readonly LanguageModelUsage Usage = new LanguageModelUsage(3, 10, 13);

    internal static readonly TimeSpan SettleTimeout = TimeSpan.FromSeconds(5);

    internal static AiClient Client()
    {
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
    }

    internal static Tool ToolOne(Func<JsonElement, CancellationToken, Task<string>>? execute = null)
    {
        return Tool.Function("tool1", null, ValueSchema, execute ?? ((_, _) => Task.FromResult("\"tool result\"")));
    }

    internal static LanguageModelGenerateResult TextResult(string text)
    {
        return new LanguageModelGenerateResult(new GeneratedContent[] { new GeneratedText(text) }, FinishReason.Stop, Usage, "stop");
    }

    internal static LanguageModelGenerateResult ToolCallResult(string input)
    {
        return new LanguageModelGenerateResult(new GeneratedContent[] { new GeneratedToolCall("call-1", "tool1", input) }, FinishReason.ToolCalls, Usage, "tool-calls");
    }

    internal static LanguageModelStreamPart[] HelloParts()
    {
        return new LanguageModelStreamPart[]
        {
            new TextStartStreamPart("1"),
            new TextDeltaStreamPart("1", "Hello"),
            new TextEndStreamPart("1"),
            new FinishStreamPart(FinishReason.Stop, Usage, "stop"),
        };
    }

    internal static LanguageModelStreamPart[] ToolCallParts()
    {
        return new LanguageModelStreamPart[]
        {
            new ToolCallStreamPart("call-1", "tool1", "{ \"value\": \"test\" }"),
            new FinishStreamPart(FinishReason.ToolCalls, Usage, "tool-calls"),
        };
    }

    internal static LanguageModelStreamPart[] FinalResponseParts()
    {
        return new LanguageModelStreamPart[]
        {
            new TextStartStreamPart("1"),
            new TextDeltaStreamPart("1", "Final response"),
            new TextEndStreamPart("1"),
            new FinishStreamPart(FinishReason.Stop, Usage, "stop"),
        };
    }

    internal static async IAsyncEnumerable<LanguageModelStreamPart> Yield(IEnumerable<LanguageModelStreamPart> parts)
    {
        foreach (var part in parts)
        {
            yield return part;
            await Task.Yield();
        }
    }

    // Yields the parts, waits for the gate, then yields the rest. The wait ignores the cancellation token, like a stalled provider.
    internal static async IAsyncEnumerable<LanguageModelStreamPart> StallAfter(
        IEnumerable<LanguageModelStreamPart> first,
        Task gate,
        IEnumerable<LanguageModelStreamPart> rest,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var part in first)
        {
            yield return part;
        }

        await gate.ConfigureAwait(false);
        foreach (var part in rest)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return part;
        }
    }

    internal static async Task Consume(StreamTextResult result)
    {
        await foreach (var _ in result.Stream())
        {
        }
    }

    internal static async Task WaitUntil(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow + SettleTimeout;
        while (!condition())
        {
            if (DateTime.UtcNow > deadline)
            {
                throw new TimeoutException("The condition did not become true.");
            }

            await Task.Delay(5);
        }
    }

    /// <summary>A language model whose calls are delegates. It records the options and the token of every call.</summary>
    internal sealed class DelegateModel : ILanguageModel
    {
        private readonly object _gate = new();
        private readonly List<LanguageModelCallOptions> _calls = new();
        private readonly List<CancellationToken> _tokens = new();

        public Func<LanguageModelCallOptions, CancellationToken, Task<LanguageModelGenerateResult>>? Generate { get; set; }

        public Func<LanguageModelCallOptions, CancellationToken, Task<LanguageModelStreamResult>>? Stream { get; set; }

        public string SpecificationVersion => "V4";

        public string Provider => "mock-provider";

        public string ModelId => "mock-model-id";

        public IReadOnlyList<LanguageModelCallOptions> Calls
        {
            get
            {
                lock (_gate)
                {
                    return _calls.ToArray();
                }
            }
        }

        public IReadOnlyList<CancellationToken> Tokens
        {
            get
            {
                lock (_gate)
                {
                    return _tokens.ToArray();
                }
            }
        }

        public Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
        {
            Record(options, cancellationToken);
            return Generate!(options, cancellationToken);
        }

        public Task<LanguageModelStreamResult> DoStreamAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
        {
            Record(options, cancellationToken);
            return Stream!(options, cancellationToken);
        }

        private void Record(LanguageModelCallOptions options, CancellationToken cancellationToken)
        {
            lock (_gate)
            {
                _calls.Add(options);
                _tokens.Add(cancellationToken);
            }
        }
    }
}
