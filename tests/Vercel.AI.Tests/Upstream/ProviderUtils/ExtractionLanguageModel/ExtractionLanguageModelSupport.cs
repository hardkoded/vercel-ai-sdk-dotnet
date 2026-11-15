// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.ProviderUtils.DecisionLanguageModelSuite;
using static Vercel.AI.Tests.DecisionFixtures;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.ExtractionLanguageModelSuite;

/// <summary>Fixtures shared by the <c>extraction-language-model.test.ts</c> port.</summary>
internal static class ExtractionLanguageModelSupport
{
    public const string Prefix = "packages/provider-utils/src/extraction-language-model.test.ts::";

    /// <summary>The <c>values</c> fixture, as the JSON text the wrapped model returns.</summary>
    public const string Values = "{\"f0\":\"INV-2041\",\"f1\":1250,\"f2\":null,\"f3\":\"2027-03-03\",\"f4\":\"17:30\",\"f5\":\"2027-03-03T17:30:00\",\"f6\":2,\"f7\":[\"Ana\",\"Bo\"],\"f8\":null}";

    /// <summary>The <c>fields</c> fixture.</summary>
    public static Dictionary<string, ExtractionField> Fields()
    {
        return new Dictionary<string, ExtractionField>
        {
            ["invoice number"] = new ExtractionField("text", DecisionInput.From("The invoice number")),
            ["total"] = new ExtractionField("number") { Unit = "USD", Decimals = 2 },
            ["count"] = new ExtractionField("number") { Integer = true },
            ["dueDate"] = new ExtractionField("date"),
            ["dueTime"] = new ExtractionField("time"),
            ["dueAt"] = new ExtractionField("datetime"),
            ["wait"] = new ExtractionField("duration") { Unit = "hours" },
            ["contacts"] = new ExtractionField("list") { Items = "text" },
            ["scores"] = new ExtractionField("list") { Items = "number", Integer = true },
        };
    }

    /// <summary>The <c>options</c> fixture, with overrides.</summary>
    public static ExtractionModelCall Call(
        IReadOnlyDictionary<string, ExtractionField>? fields = null,
        IReadOnlyList<object?>? state = null,
        string? referenceDate = "2026-11-11",
        string providerOptions = "{}",
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default,
        double? maxOutputTokens = null,
        string? reasoning = null)
    {
        using var document = JsonDocument.Parse(providerOptions);
        return new ExtractionModelCall(
            P(state ?? new List<object?> { new Dictionary<string, object?> { ["type"] = "text", ["text"] = "Invoice INV-2041, total $1,250.00." } }),
            fields ?? Fields(),
            document.RootElement.Clone(),
            headers ?? new Dictionary<string, string>(),
            cancellationToken,
            referenceDate,
            maxOutputTokens,
            reasoning);
    }

    /// <summary>The <c>result</c> of <c>setup()</c>.</summary>
    public static LanguageModelGenerateResult Result(
        IReadOnlyList<GeneratedContent>? content = null,
        FinishReason finishReason = FinishReason.Stop,
        string rawFinishReason = "end")
    {
        using var metadata = JsonDocument.Parse("{\"test\":{\"original\":true}}");
        return new LanguageModelGenerateResult(
            content ?? Text(Values),
            finishReason,
            new LanguageModelUsage(20, 12, 32, cacheReadTokens: 5, cacheWriteTokens: 0, reasoningTokens: 4, noCacheInputTokens: 15, textTokens: 8),
            rawFinishReason: rawFinishReason,
            warnings: new[] { new CallWarning("other", "test warning") },
            responseId: "response",
            providerMetadata: metadata.RootElement.Clone(),
            responseModelId: "resolved");
    }

    public static GeneratedContent[] Text(string text)
    {
        return new GeneratedContent[] { new GeneratedText(text) };
    }

    /// <summary>Port of <c>setup(overrides, maxFieldsPerCall)</c>.</summary>
    public static (ExtractionLanguageModel Model, RecordingLanguageModel Language, LanguageModelGenerateResult Result) Setup(
        IReadOnlyList<GeneratedContent>? content = null,
        FinishReason finishReason = FinishReason.Stop,
        int? maxFieldsPerCall = null)
    {
        var result = Result(content, finishReason);
        var language = new RecordingLanguageModel { OnGenerate = (_, _) => Task.FromResult(result) };
        return (new ExtractionLanguageModel(language, maxFieldsPerCall: maxFieldsPerCall), language, result);
    }

    /// <summary>A model response that never completes and fails with the call's cancellation, like a promise that rejects on abort.</summary>
    public static Task<LanguageModelGenerateResult> UntilCancelled(CancellationToken cancellationToken)
    {
        var completion = new TaskCompletionSource<LanguageModelGenerateResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        cancellationToken.Register(() => completion.TrySetCanceled(cancellationToken));
        return completion.Task;
    }

    /// <summary>Asserts an answer's type and value. The value is a string, a double, a string list, a double list, or null.</summary>
    public static void AssertAnswer(string type, object? value, ExtractionAnswer answer)
    {
        Assert.Equal(type, answer.Type);
        Assert.Equal(value as string, answer.Text);
        Assert.Equal(value is double number ? number : null, answer.Number);
        Assert.Equal(value as IReadOnlyList<string>, answer.TextItems);
        Assert.Equal(value as IReadOnlyList<double>, answer.NumberItems);
    }
}

/// <summary>A language model of a former specification version.</summary>
internal sealed class V3LanguageModel : ILanguageModel
{
    public string SpecificationVersion => "v3";

    public string Provider => "test.language";

    public string ModelId => "test-model";

    public Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    public Task<LanguageModelStreamResult> DoStreamAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }
}
