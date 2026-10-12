// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using DecisionModel = Vercel.AI.Operations.DecisionLanguageModel;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.DecisionLanguageModelSuite;

/// <summary>Fixtures shared by the <c>decision-language-model.test.ts</c> port.</summary>
internal static class DecisionLanguageModelSupport
{
    internal static DecisionQuestion Category(object? criteria = null)
    {
        return new DecisionQuestion(
            "choice",
            new Dictionary<string, object?> { ["task"] = new List<object?> { "Pick the exact label" } },
            criteria ?? new Dictionary<string, object?>
            {
                ["Needs Review"] = new Dictionary<string, object?> { ["meaning"] = "manual" },
                ["Needs review"] = new List<object?> { "automatic" },
                ["other"] = null,
            });
    }

    internal static DecisionQuestion Severity(object? criteria = null)
    {
        return new DecisionQuestion(
            "score",
            new List<object?> { "Rate the impact" },
            criteria ?? new List<object?> { "low", new Dictionary<string, object?> { ["meaning"] = "medium" }, null });
    }

    internal static Dictionary<string, DecisionQuestion> Questions()
    {
        return new Dictionary<string, DecisionQuestion> { ["category"] = Category(), ["severity"] = Severity() };
    }

    internal static Dictionary<string, object?> StateValue()
    {
        return new Dictionary<string, object?> { ["text"] = "test", ["events"] = new List<object?> { 1, null } };
    }

    internal static List<object?> State()
    {
        return new List<object?> { new Dictionary<string, object?> { ["type"] = "json", ["value"] = StateValue() } };
    }

    internal static DecisionModelCall Call(
        IReadOnlyDictionary<string, DecisionQuestion>? questions = null,
        IReadOnlyList<object?>? state = null,
        string providerOptions = "{}",
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        using var document = JsonDocument.Parse(providerOptions);
        return new DecisionModelCall(state ?? State(), questions ?? Questions(), document.RootElement.Clone(), headers ?? new Dictionary<string, string>(), cancellationToken);
    }

    internal static LanguageModelGenerateResult Result(
        IReadOnlyList<GeneratedContent>? content = null,
        FinishReason finishReason = FinishReason.Stop)
    {
        using var metadata = JsonDocument.Parse("{\"test\":{\"original\":true}}");
        return new LanguageModelGenerateResult(
            content ?? new GeneratedContent[] { new GeneratedText("{\"q1\":1.25,\"q0\":\"c1\"}") },
            finishReason,
            new LanguageModelUsage(20, 12, 32, cacheReadTokens: 5, cacheWriteTokens: 0, reasoningTokens: 4, noCacheInputTokens: 15, textTokens: 8),
            rawFinishReason: "end",
            warnings: new[] { new CallWarning("other", "test warning") },
            responseId: "response",
            providerMetadata: metadata.RootElement.Clone(),
            rawResponse: "{\"raw\":true}",
            responseModelId: "resolved",
            responseTimestamp: DateTimeOffset.FromUnixTimeMilliseconds(0),
            responseHeaders: new Dictionary<string, string> { ["x-request-id"] = "id" });
    }

    internal static (DecisionModel Model, RecordingLanguageModel Language, LanguageModelGenerateResult Result) Setup(
        IReadOnlyList<GeneratedContent>? content = null,
        FinishReason finishReason = FinishReason.Stop)
    {
        var result = Result(content, finishReason);
        var language = new RecordingLanguageModel { OnGenerate = (_, _) => Task.FromResult(result) };
        return (new DecisionModel(language, "test.decision"), language, result);
    }
}

/// <summary>Language model that records each generate call with its cancellation token.</summary>
internal sealed class RecordingLanguageModel : ILanguageModel
{
    public string SpecificationVersion => "v4";

    public string Provider => "test.language";

    public string ModelId => "test-model";

    public List<LanguageModelCallOptions> Calls { get; } = new();

    public List<CancellationToken> Tokens { get; } = new();

    public Func<LanguageModelCallOptions, CancellationToken, Task<LanguageModelGenerateResult>> OnGenerate { get; set; } = (_, _) => throw new InvalidOperationException("No result.");

    public Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        Calls.Add(options);
        Tokens.Add(cancellationToken);
        return OnGenerate(options, cancellationToken);
    }

    public IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }
}
