// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.GenerateText;

/// <summary>Test fixture standing in for upstream's <c>MockLanguageModelV4({ doGenerate, doStream })</c>.</summary>
internal sealed class StepErrorModel(
    Func<
        LanguageModelCallOptions,
        CancellationToken,
        Task<LanguageModelGenerateResult>
    >? doGenerate = null,
    Func<LanguageModelCallOptions, CancellationToken, Task<LanguageModelStreamResult>>? doStream =
        null
) : ILanguageModel
{
    public IReadOnlyDictionary<string, string> SupportedUrls =>
        Vercel.AI.ProviderUtils.UrlSupport.None;

    public string SpecificationVersion => "V4";

    public string Provider => "test";

    public string ModelId => "mock-model-id";

    public Task<LanguageModelGenerateResult> DoGenerateAsync(
        LanguageModelCallOptions options,
        CancellationToken cancellationToken
    ) => (doGenerate ?? throw new NotSupportedException())(options, cancellationToken);

    public Task<LanguageModelStreamResult> DoStreamAsync(
        LanguageModelCallOptions options,
        CancellationToken cancellationToken
    ) => (doStream ?? throw new NotSupportedException())(options, cancellationToken);

    // Waits until the call is aborted. Maps to the hanging `doGenerate`/`doStream` that rejects with the abort reason.
    public static async Task<T> HangUntilAborted<T>(CancellationToken cancellationToken)
    {
        await Task.Delay(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
        throw new InvalidOperationException("unreachable");
    }

    public static Task<LanguageModelStreamResult> StreamOf(
        IAsyncEnumerable<LanguageModelStreamPart> parts
    ) => Task.FromResult(new LanguageModelStreamResult(parts));
}
