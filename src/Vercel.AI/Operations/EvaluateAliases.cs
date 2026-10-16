// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Util;

namespace Vercel.AI.Operations;

/// <summary>Deprecated alias of <see cref="DecisionQuestion"/>.</summary>
[Obsolete("Use DecisionQuestion instead.")]
public sealed class EvaluationQuestion : DecisionQuestion
{
    /// <summary>Creates a question.</summary>
    public EvaluationQuestion(string type, DecisionInput instructions, DecisionCriteria? criteria = null)
        : base(type, instructions, criteria)
    {
    }
}

/// <summary>Deprecated alias of <see cref="DecisionAnswer"/>.</summary>
[Obsolete("Use DecisionAnswer instead.")]
public sealed class EvaluationAnswer : DecisionAnswer
{
    /// <summary>Creates an answer.</summary>
    public EvaluationAnswer(string type, string? choice = null, double? score = null, double? probability = null, IReadOnlyDictionary<string, double>? probabilities = null)
        : base(type, choice, score, probability, probabilities)
    {
    }
}

/// <summary>Deprecated alias of <see cref="DecisionRounding"/>.</summary>
[Obsolete("Use DecisionRounding instead.")]
public sealed class EvaluationRounding : DecisionRounding
{
    /// <summary>Creates rounding limits.</summary>
    public EvaluationRounding(int? probabilityDecimals = null, int? scoreDecimals = null)
        : base(probabilityDecimals, scoreDecimals)
    {
    }
}

/// <summary>Deprecated alias of <see cref="DecisionModelCall"/>.</summary>
[Obsolete("Use DecisionModelCall instead.")]
public sealed class EvaluationModelCall : DecisionModelCall
{
    /// <summary>Creates a call.</summary>
    public EvaluationModelCall(IReadOnlyList<DecisionModelStatePart> state, IReadOnlyDictionary<string, EvaluationQuestion> questions, JsonElement providerOptions, IReadOnlyDictionary<string, string> headers, CancellationToken cancellationToken)
        : base(state, questions == null ? null! : EvaluateAliases.ToDecision(questions), providerOptions, headers, cancellationToken)
    {
    }

    private EvaluationModelCall(IReadOnlyList<DecisionModelStatePart> state, IReadOnlyDictionary<string, DecisionQuestion> questions, JsonElement providerOptions, IReadOnlyDictionary<string, string> headers, CancellationToken cancellationToken)
        : base(state, questions, providerOptions, headers, cancellationToken)
    {
    }

    internal static EvaluationModelCall From(DecisionModelCall call)
    {
        return new EvaluationModelCall(call.State, call.Questions, call.ProviderOptions, call.Headers, call.CancellationToken);
    }
}

/// <summary>Deprecated alias of <see cref="DecisionModelResult"/>.</summary>
[Obsolete("Use DecisionModelResult instead.")]
public sealed class EvaluationModelResult : DecisionModelResult
{
    /// <summary>Creates a model result.</summary>
    public EvaluationModelResult(IReadOnlyDictionary<string, EvaluationAnswer> answers, IReadOnlyList<OperationWarning>? warnings = null, OperationUsage? usage = null, EvaluationRounding? rounding = null, JsonElement? providerMetadata = null, ProviderResponse? response = null)
        : base(answers == null ? null! : EvaluateAliases.ToDecision(answers), warnings, usage, rounding, providerMetadata, response)
    {
    }

    private EvaluationModelResult(IReadOnlyDictionary<string, DecisionAnswer> answers, IReadOnlyList<OperationWarning>? warnings = null, OperationUsage? usage = null, DecisionRounding? rounding = null, JsonElement? providerMetadata = null, ProviderResponse? response = null)
        : base(answers, warnings, usage, rounding, providerMetadata, response)
    {
    }

    internal static EvaluationModelResult From(DecisionModelResult result)
    {
        return result as EvaluationModelResult ?? new EvaluationModelResult(result.Answers, result.Warnings, result.Usage, result.Rounding, result.ProviderMetadata, result.Response);
    }
}

/// <summary>Deprecated decision model contract. Implement <see cref="IDecisionCaller"/> instead.</summary>
[Obsolete("Implement IDecisionCaller instead.")]
public interface IEvaluationCaller
{
    /// <summary>Provider id.</summary>
    string Provider { get; }

    /// <summary>Model id.</summary>
    string ModelId { get; }

    /// <summary>Specification version. Only <c>v4</c> is accepted.</summary>
    string SpecificationVersion { get; }

    /// <summary>Question types this model can answer.</summary>
    IReadOnlyList<string> SupportedQuestionTypes { get; }

    /// <summary>Decides every question in one call.</summary>
    Task<EvaluationModelResult> DoEvaluateAsync(EvaluationModelCall call, CancellationToken cancellationToken);
}

/// <summary>
/// A decision model that also has the deprecated <see cref="IEvaluationCaller.DoEvaluateAsync"/>. Returned by the deprecated
/// <c>EvaluationModel</c> accessors of <see cref="ProviderRegistry"/> and <see cref="CustomProvider"/>. Both methods decide
/// the same way.
/// </summary>
[Obsolete("Use IDecisionCaller instead.")]
public interface IEvaluationDecisionCaller : IDecisionCaller, IEvaluationCaller
{
}

/// <summary>
/// Deprecated provider factory for decision models. Implement <see cref="IDecisionProvider"/> instead. A provider that
/// implements both is asked through <see cref="IDecisionProvider"/> only.
/// </summary>
[Obsolete("Implement IDecisionProvider instead.")]
public interface IEvaluationProvider
{
    /// <summary>Returns a decision model, or null when the provider has no such model.</summary>
    IEvaluationCaller? EvaluationModel(string modelId);
}

/// <summary>Deprecated alias of <see cref="DecisionUnsupportedQuestionTypeException"/>.</summary>
[Obsolete("Use DecisionUnsupportedQuestionTypeException instead.")]
public sealed class EvaluationUnsupportedQuestionTypeException : DecisionUnsupportedQuestionTypeException
{
    /// <summary>Creates the exception.</summary>
    public EvaluationUnsupportedQuestionTypeException(string questionId, string questionType, string provider, string modelId)
        : base(questionId, questionType, provider, modelId)
    {
    }
}

/// <summary>Deprecated alias of <see cref="DecideEvent"/>.</summary>
[Obsolete("Use DecideEvent instead.")]
public sealed class EvaluateEvent : DecideEvent
{
    /// <summary>Creates an event.</summary>
    public EvaluateEvent(string callId, string operationId, IReadOnlyDictionary<string, object?> runtimeContext, string provider, string modelId, DecisionState? state, IReadOnlyDictionary<string, DecisionQuestion> questions, int maxRetries, IReadOnlyDictionary<string, string>? headers, JsonElement providerOptions, IReadOnlyDictionary<string, DecisionAnswer>? answers = null, OperationUsage? usage = null, IReadOnlyList<OperationWarning>? warnings = null, bool? recordInputs = null, bool? recordOutputs = null, string? functionId = null)
        : base(callId, operationId, runtimeContext, provider, modelId, state, questions, maxRetries, headers, providerOptions, answers, usage, warnings, recordInputs, recordOutputs, functionId)
    {
    }

    internal static EvaluateEvent From(DecideEvent e)
    {
        return e as EvaluateEvent ?? new EvaluateEvent(e.CallId, e.OperationId, e.RuntimeContext, e.Provider, e.ModelId, e.State, e.Questions, e.MaxRetries, e.Headers, e.ProviderOptions, e.Answers, e.Usage, e.Warnings, e.RecordInputs, e.RecordOutputs, e.FunctionId);
    }
}

/// <summary>Deprecated alias of <see cref="DecisionModelEvent"/>.</summary>
[Obsolete("Use DecisionModelEvent instead.")]
public sealed class EvaluateModelEvent : DecisionModelEvent
{
    /// <summary>Creates a model event.</summary>
    public EvaluateModelEvent(string callId, string operationId, string provider, string modelId, IReadOnlyList<DecisionModelStatePart>? state, IReadOnlyDictionary<string, DecisionQuestion> questions, IReadOnlyDictionary<string, DecisionAnswer>? answers = null, OperationUsage? usage = null, bool? recordInputs = null, bool? recordOutputs = null, string? functionId = null)
        : base(callId, operationId, provider, modelId, state, questions, answers, usage, recordInputs, recordOutputs, functionId)
    {
    }

    internal static EvaluateModelEvent From(DecisionModelEvent e)
    {
        return e as EvaluateModelEvent ?? new EvaluateModelEvent(e.CallId, e.OperationId, e.Provider, e.ModelId, e.State, e.Questions, e.Answers, e.Usage, e.RecordInputs, e.RecordOutputs, e.FunctionId);
    }
}

/// <summary>Deprecated alias of <see cref="DecideTelemetry"/>.</summary>
[Obsolete("Use DecideTelemetry instead.")]
public sealed class EvaluateTelemetry : DecideTelemetry
{
}

/// <summary>Deprecated alias of <see cref="DecisionResult"/>.</summary>
[Obsolete("Use DecisionResult instead.")]
public sealed class EvaluateResult : DecisionResult
{
    /// <summary>Creates a result.</summary>
    public EvaluateResult(IReadOnlyDictionary<string, DecisionAnswer> answers, OperationUsage usage, IReadOnlyList<OperationWarning> warnings, DecisionRounding? rounding, JsonElement? providerMetadata, ProviderResponse response)
        : base(answers, usage, warnings, rounding, providerMetadata, response)
    {
    }

    internal static EvaluateResult From(DecisionResult result)
    {
        return new EvaluateResult(result.Answers, result.Usage, result.Warnings, result.Rounding, result.ProviderMetadata, result.Response);
    }
}

/// <summary>Deprecated alias of <see cref="DecideRequest"/>.</summary>
[Obsolete("Use DecideRequest instead.")]
public sealed class EvaluateRequest : OperationRequest
{
    /// <summary>Decision model. A model that implements only <see cref="IEvaluationCaller"/> still runs.</summary>
    public IEvaluationCaller? Model { get; set; }

    /// <summary>Shared state. See <see cref="DecideRequest.State"/>.</summary>
    public DecisionState? State { get; set; }

    /// <summary>Nonempty question map.</summary>
    public IReadOnlyDictionary<string, EvaluationQuestion>? Questions { get; set; }

    /// <summary>Called when the decision begins.</summary>
    public Func<EvaluateEvent, Task>? OnStart { get; set; }

    /// <summary>Called when the decision completes.</summary>
    public Func<EvaluateEvent, Task>? OnEnd { get; set; }

    /// <summary>Telemetry.</summary>
    public EvaluateTelemetry? Telemetry { get; set; }
}

/// <summary>Deprecated alias of <see cref="Decide"/>.</summary>
[Obsolete("Use Decide instead.")]
public static class Evaluate
{
    /// <summary>Deprecated alias of <see cref="Decide.DecideAsync"/>. Both run the same operation.</summary>
    public static async Task<EvaluateResult> EvaluateAsync(EvaluateRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var onStart = request.OnStart;
        var onEnd = request.OnEnd;
        var result = await Decide.DecideAsync(
            new DecideRequest
            {
                Model = request.Model == null ? null : DecisionCaller.FromLegacy(request.Model),
                State = request.State,
                Questions = request.Questions == null ? null : EvaluateAliases.ToDecision(request.Questions),
                OnStart = onStart == null ? null : e => onStart(EvaluateEvent.From(e)),
                OnEnd = onEnd == null ? null : e => onEnd(EvaluateEvent.From(e)),
                Telemetry = request.Telemetry,
                MaxRetries = request.MaxRetries,
                CancellationToken = request.CancellationToken,
                AbortReason = request.AbortReason,
                Headers = request.Headers,
                ProviderOptions = request.ProviderOptions,
                RuntimeContext = request.RuntimeContext,
                GenerateCallId = request.GenerateCallId,
                Now = request.Now,
            },
            cancellationToken).ConfigureAwait(false);
        return EvaluateResult.From(result);
    }

    /// <summary>Deprecated alias of <see cref="Decide.ValidateInput"/>.</summary>
    public static void ValidateInput(DecisionState? state, IReadOnlyDictionary<string, EvaluationQuestion>? questions)
    {
        Decide.ValidateInput(state, questions == null ? null : EvaluateAliases.ToDecision(questions));
    }

    /// <summary>Deprecated alias of <see cref="Decide.ValidateAnswers"/>.</summary>
    public static void ValidateAnswers(IReadOnlyDictionary<string, EvaluationQuestion> questions, IReadOnlyDictionary<string, EvaluationAnswer>? answers, EvaluationRounding? rounding)
    {
        Decide.ValidateAnswers(EvaluateAliases.ToDecision(questions), answers == null ? null : EvaluateAliases.ToDecision(answers), rounding);
    }
}

/// <summary>Helpers for <see cref="IDecisionCaller"/>.</summary>
public static class DecisionCaller
{
    /// <summary>
    /// Wraps a model that implements only the deprecated <see cref="IEvaluationCaller"/> so
    /// <see cref="Decide.DecideAsync"/> can run it. A model that already implements
    /// <see cref="IDecisionCaller"/> is returned unchanged.
    /// </summary>
    [Obsolete("Implement IDecisionCaller on the model instead.")]
    public static IDecisionCaller FromLegacy(IEvaluationCaller model)
    {
        if (model == null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        return model as IDecisionCaller ?? new LegacyDecisionCaller(model);
    }
}

#pragma warning disable CS0618
internal sealed class LegacyDecisionCaller : IEvaluationDecisionCaller
{
    private readonly IEvaluationCaller _inner;

    public LegacyDecisionCaller(IEvaluationCaller inner)
    {
        _inner = inner;
    }

    public string Provider => _inner.Provider;

    public string ModelId => _inner.ModelId;

    public string SpecificationVersion => _inner.SpecificationVersion;

    public IReadOnlyList<string> SupportedQuestionTypes => _inner.SupportedQuestionTypes;

    public Task<DecisionModelResult> DoDecideAsync(DecisionModelCall call, CancellationToken cancellationToken)
    {
        return AdaptAsync(call, cancellationToken);
    }

    public Task<EvaluationModelResult> DoEvaluateAsync(EvaluationModelCall call, CancellationToken cancellationToken)
    {
        return _inner.DoEvaluateAsync(call, cancellationToken);
    }

    private async Task<DecisionModelResult> AdaptAsync(DecisionModelCall call, CancellationToken cancellationToken)
    {
        return await _inner.DoEvaluateAsync(EvaluationModelCall.From(call), cancellationToken).ConfigureAwait(false);
    }
}

// Maps to the object asEvaluationModel builds for a model without doEvaluate. A model that implements both interfaces
// without IEvaluationDecisionCaller keeps its own DoEvaluateAsync.
internal sealed class EvaluationDecisionCaller : IEvaluationDecisionCaller
{
    private readonly IDecisionCaller _inner;

    public EvaluationDecisionCaller(IDecisionCaller inner)
    {
        _inner = inner;
    }

    public string Provider => _inner.Provider;

    public string ModelId => _inner.ModelId;

    public string SpecificationVersion => _inner.SpecificationVersion;

    public IReadOnlyList<string> SupportedQuestionTypes => _inner.SupportedQuestionTypes;

    public Task<DecisionModelResult> DoDecideAsync(DecisionModelCall call, CancellationToken cancellationToken)
    {
        return _inner.DoDecideAsync(call, cancellationToken);
    }

    public async Task<EvaluationModelResult> DoEvaluateAsync(EvaluationModelCall call, CancellationToken cancellationToken)
    {
        if (_inner is IEvaluationCaller legacy)
        {
            return await legacy.DoEvaluateAsync(call, cancellationToken).ConfigureAwait(false);
        }

        return EvaluationModelResult.From(await _inner.DoDecideAsync(call, cancellationToken).ConfigureAwait(false));
    }
}

internal static class EvaluateAliases
{
    public static IReadOnlyDictionary<string, DecisionQuestion> ToDecision(IReadOnlyDictionary<string, EvaluationQuestion> questions)
    {
        var copy = new Dictionary<string, DecisionQuestion>(questions.Count, StringComparer.Ordinal);
        foreach (var pair in questions)
        {
            copy[pair.Key] = pair.Value;
        }

        return copy;
    }

    public static IReadOnlyDictionary<string, DecisionAnswer> ToDecision(IReadOnlyDictionary<string, EvaluationAnswer> answers)
    {
        var copy = new Dictionary<string, DecisionAnswer>(answers.Count, StringComparer.Ordinal);
        foreach (var pair in answers)
        {
            copy[pair.Key] = pair.Value;
        }

        return copy;
    }
}
#pragma warning restore CS0618
