// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Middleware.WrapLanguageModel;

// Maps to the object literals the upstream tests pass as `middleware`: each hook is optional.
internal sealed class HookMiddleware : LanguageModelMiddleware
{
    public Func<
        LanguageModelCallOptions,
        LanguageModelCallType,
        ILanguageModel,
        Task<LanguageModelCallOptions>
    >? OnTransformParams { get; init; }

    public Func<
        LanguageModelWrapContext,
        Task<LanguageModelGenerateResult>
    >? OnWrapGenerate { get; init; }

    public Func<
        LanguageModelWrapContext,
        Task<LanguageModelStreamResult>
    >? OnWrapStream { get; init; }

    public Func<ILanguageModel, string?>? OnOverrideProvider { get; init; }

    public Func<ILanguageModel, string?>? OnOverrideModelId { get; init; }

    public Func<
        ILanguageModel,
        IReadOnlyDictionary<string, string>?
    >? OnOverrideSupportedUrls { get; init; }

    public override Task<LanguageModelCallOptions> TransformParamsAsync(
        LanguageModelCallOptions options,
        LanguageModelCallType type,
        ILanguageModel model,
        CancellationToken cancellationToken
    ) =>
        OnTransformParams is null
            ? base.TransformParamsAsync(options, type, model, cancellationToken)
            : OnTransformParams(options, type, model);

    public override Task<LanguageModelGenerateResult> WrapGenerateAsync(
        LanguageModelWrapContext context,
        CancellationToken cancellationToken
    ) =>
        OnWrapGenerate is null
            ? base.WrapGenerateAsync(context, cancellationToken)
            : OnWrapGenerate(context);

    public override Task<LanguageModelStreamResult> WrapStreamAsync(
        LanguageModelWrapContext context,
        CancellationToken cancellationToken
    ) =>
        OnWrapStream is null
            ? base.WrapStreamAsync(context, cancellationToken)
            : OnWrapStream(context);

    public override string? OverrideProvider(ILanguageModel model) =>
        OnOverrideProvider is null ? base.OverrideProvider(model) : OnOverrideProvider(model);

    public override string? OverrideModelId(ILanguageModel model) =>
        OnOverrideModelId is null ? base.OverrideModelId(model) : OnOverrideModelId(model);

    public override IReadOnlyDictionary<string, string>? OverrideSupportedUrls(
        ILanguageModel model
    ) =>
        OnOverrideSupportedUrls is null
            ? base.OverrideSupportedUrls(model)
            : OnOverrideSupportedUrls(model);
}

internal static class WrapLanguageModelTestSupport
{
    // Maps to the params the upstream tests pass to doGenerate and doStream.
    public static LanguageModelCallOptions HelloParams() =>
        new() { Prompt = [new UserModelMessage("Hello")] };

    // Maps to a transformParams that spreads params and adds a flag.
    public static Func<
        LanguageModelCallOptions,
        LanguageModelCallType,
        ILanguageModel,
        Task<LanguageModelCallOptions>
    > Transform(
        Action<LanguageModelCallOptions> mark,
        List<(
            LanguageModelCallOptions Options,
            LanguageModelCallType Type,
            ILanguageModel Model
        )> calls
    ) =>
        (options, type, model) =>
        {
            calls.Add((options, type, model));
            var copy = options.Copy();
            mark(copy);
            return Task.FromResult(copy);
        };

    // Maps to the string results of the upstream chaining tests: the response id carries the text.
    public static LanguageModelGenerateResult GenerateResult(string marker) =>
        new([], FinishReason.Stop, new LanguageModelUsage(1, 1, 2), responseId: marker);

    // Maps to the string results of the upstream chaining tests: a response header carries the text.
    public static LanguageModelStreamResult StreamResult(string marker) =>
        new(
            AsyncEnumerable.Empty<LanguageModelStreamPart>(),
            response: new LanguageModelStreamResponse(
                new Dictionary<string, string> { ["marker"] = marker }
            )
        );
}
