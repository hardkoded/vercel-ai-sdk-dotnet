// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry;

/// <summary>Stands in for <c>new MockLanguageModelV4({ provider: 'openai.chat', modelId: 'gpt-4', doGenerate | doStream })</c> of <c>open-telemetry.test.ts</c>.</summary>
internal sealed class OpenAiChatMockModel(
    IReadOnlyList<LanguageModelGenerateResult>? generateResults = null,
    Func<CancellationToken, IAsyncEnumerable<LanguageModelStreamPart>>? doStream = null,
    string modelId = "gpt-4"
) : ILanguageModel
{
    private int _generateCalls;

    public IReadOnlyDictionary<string, string> SupportedUrls =>
        Vercel.AI.ProviderUtils.UrlSupport.None;

    public string SpecificationVersion => "V4";

    public string Provider => "openai.chat";

    public string ModelId => modelId;

    public static LanguageModelUsage Usage { get; } = new(2, 1, 3);

    public static OpenAiChatMockModel Generate(
        params IReadOnlyList<GeneratedContent>[] responses
    ) =>
        new([
            .. responses.Select(content => new LanguageModelGenerateResult(
                content,
                content.Any(part => part is GeneratedToolCall)
                    ? FinishReason.ToolCalls
                    : FinishReason.Stop,
                Usage
            )),
        ]);

    public Task<LanguageModelGenerateResult> DoGenerateAsync(
        LanguageModelCallOptions options,
        CancellationToken cancellationToken
    )
    {
        var results = generateResults ?? throw new NotSupportedException();
        var index = Math.Min(Interlocked.Increment(ref _generateCalls) - 1, results.Count - 1);
        return Task.FromResult(results[index]);
    }

    public Task<LanguageModelStreamResult> DoStreamAsync(
        LanguageModelCallOptions options,
        CancellationToken cancellationToken
    ) =>
        Task.FromResult(
            new LanguageModelStreamResult(
                (doStream ?? throw new NotSupportedException())(cancellationToken)
            )
        );
}
