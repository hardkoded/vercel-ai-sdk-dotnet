// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using Vercel.AI.Prompt;
using Vercel.AI.Util;

namespace Vercel.AI.Operations;

/// <summary>One decision question. Maps to <c>DecisionModelV4Question</c>.</summary>
public class DecisionQuestion
{
    /// <summary>Creates a question.</summary>
    /// <param name="type"><c>choice</c>, <c>score</c>, <c>boolean</c>, or <c>box</c>.</param>
    /// <param name="instructions">
    /// Instructions as a string, JSON object, or JSON array. A box question requires them. The other types may leave them null
    /// when the provider supports it and the criteria define the judgment.
    /// </param>
    /// <param name="criteria">
    /// <see cref="DecisionChoiceCriteria"/> for a choice, <see cref="DecisionScoreCriteria"/> for a score, and an optional
    /// <see cref="DecisionBooleanCriteria"/> for a boolean. A box question has no criteria.
    /// </param>
    public DecisionQuestion(string type, DecisionInput? instructions, DecisionCriteria? criteria = null)
    {
        Type = type ?? string.Empty;
        Instructions = instructions;
        HasInstructions = instructions != null;
        Criteria = criteria;
    }

    /// <summary><c>choice</c>, <c>score</c>, <c>boolean</c>, or <c>box</c>.</summary>
    public string Type { get; }

    /// <summary>Instructions. A string, JSON object, or JSON array. May be null for choice, score, and boolean questions.</summary>
    public DecisionInput? Instructions { get; }

    /// <summary>
    /// True when <c>instructions</c> is present. It is true whenever <see cref="Instructions"/> is set. Set it to true with null
    /// instructions to send an explicit <c>null</c>, as upstream does for <c>instructions: null</c>. A question that leaves it false omits
    /// the field, as upstream does for an absent or <c>undefined</c> value.
    /// </summary>
    public bool HasInstructions { get; init; }

    /// <summary>Choice options, ordered score levels, or optional boolean descriptions. A box question has no criteria.</summary>
    public DecisionCriteria? Criteria { get; }
}

/// <summary>One model answer. Numbers are returned unchanged.</summary>
public class DecisionAnswer
{
    /// <summary>Creates an answer.</summary>
    public DecisionAnswer(string type, string? choice = null, double? score = null, double? probability = null, IReadOnlyDictionary<string, double>? probabilities = null, DecisionBox? box = null)
    {
        Type = type;
        Box = box;
        Choice = choice;
        Score = score;
        Probability = probability;
        Probabilities = probabilities;
    }

    /// <summary>Answer type. It is the question type, or <c>refusal</c> when the model declined the question.</summary>
    public string Type { get; }

    /// <summary>Selected choice.</summary>
    public string? Choice { get; }

    /// <summary>Score.</summary>
    public double? Score { get; }

    /// <summary>P(true) for a boolean question.</summary>
    public double? Probability { get; }

    /// <summary>Distribution. Keys are option ids or score indexes.</summary>
    public IReadOnlyDictionary<string, double>? Probabilities { get; }

    /// <summary>Normalized box for a box question.</summary>
    public DecisionBox? Box { get; }
}

/// <summary>Edges in [0, 1], measured from the image's top-left corner.</summary>
public sealed class DecisionBox
{
    /// <summary>Creates a box.</summary>
    public DecisionBox(double top, double bottom, double left, double right)
    {
        Top = top;
        Bottom = bottom;
        Left = left;
        Right = right;
    }

    /// <summary>Top edge.</summary>
    public double Top { get; }

    /// <summary>Bottom edge.</summary>
    public double Bottom { get; }

    /// <summary>Left edge.</summary>
    public double Left { get; }

    /// <summary>Right edge.</summary>
    public double Right { get; }
}

/// <summary>Declared rounding precision.</summary>
public class DecisionRounding
{
    /// <summary>Creates rounding limits.</summary>
    public DecisionRounding(int? probabilityDecimals = null, int? scoreDecimals = null)
    {
        ProbabilityDecimals = probabilityDecimals;
        ScoreDecimals = scoreDecimals;
    }

    /// <summary>Probability decimal places, from 0 through 15.</summary>
    public int? ProbabilityDecimals { get; }

    /// <summary>Score decimal places, from 0 through 15.</summary>
    public int? ScoreDecimals { get; }
}

/// <summary>Model response for <c>doDecide</c>.</summary>
public class DecisionModelResult
{
    /// <summary>Creates a model result.</summary>
    public DecisionModelResult(IReadOnlyDictionary<string, DecisionAnswer> answers, IReadOnlyList<OperationWarning>? warnings = null, OperationUsage? usage = null, DecisionRounding? rounding = null, JsonElement? providerMetadata = null, ProviderResponse? response = null)
    {
        Answers = answers ?? new Dictionary<string, DecisionAnswer>();
        Warnings = warnings ?? Array.Empty<OperationWarning>();
        Usage = usage;
        Rounding = rounding;
        ProviderMetadata = providerMetadata;
        Response = response;
    }

    /// <summary>Answers keyed by question id.</summary>
    public IReadOnlyDictionary<string, DecisionAnswer> Answers { get; }

    /// <summary>Warnings.</summary>
    public IReadOnlyList<OperationWarning> Warnings { get; }

    /// <summary>Token usage.</summary>
    public OperationUsage? Usage { get; }

    /// <summary>Rounding declaration.</summary>
    public DecisionRounding? Rounding { get; }

    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Response metadata. Timestamp and model id are filled when omitted.</summary>
    public ProviderResponse? Response { get; }
}

/// <summary>Arguments for <c>doDecide</c>.</summary>
public class DecisionModelCall
{
    /// <summary>Creates a call.</summary>
    public DecisionModelCall(IReadOnlyList<DecisionModelStatePart> state, IReadOnlyDictionary<string, DecisionQuestion> questions, JsonElement providerOptions, IReadOnlyDictionary<string, string> headers, CancellationToken cancellationToken, double? maxOutputTokens = null, string? reasoning = null)
    {
        State = state;
        Questions = questions;
        ProviderOptions = providerOptions;
        Headers = headers;
        CancellationToken = cancellationToken;
        MaxOutputTokens = maxOutputTokens;
        Reasoning = reasoning;
    }

    /// <summary>Shared state as an ordered list of text, JSON, and file parts. See <see cref="DecisionState.PrepareDecisionStateAsync"/>.</summary>
    public IReadOnlyList<DecisionModelStatePart> State { get; }

    /// <summary>Questions.</summary>
    public IReadOnlyDictionary<string, DecisionQuestion> Questions { get; }

    /// <summary>Provider options.</summary>
    public JsonElement ProviderOptions { get; }

    /// <summary>Headers including the user-agent suffix.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>Cancellation token.</summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>Maximum output tokens, an integer of at least 1. Null when the caller set none.</summary>
    public double? MaxOutputTokens { get; }

    /// <summary>
    /// Reasoning effort: <c>provider-default</c>, <c>none</c>, <c>minimal</c>, <c>low</c>, <c>medium</c>, <c>high</c>, <c>xhigh</c>, or <c>max</c>.
    /// Language-model adapters default to <c>none</c> when it is null. Native decision providers report an unsupported-setting warning.
    /// </summary>
    public string? Reasoning { get; }
}

/// <summary>Decision model used by <see cref="Decide"/>.</summary>
public interface IDecisionCaller
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
    Task<DecisionModelResult> DoDecideAsync(DecisionModelCall call, CancellationToken cancellationToken);
}

/// <summary>
/// A provider that returns decision models. <see cref="ProviderRegistry"/> and <see cref="CustomProvider"/> resolve
/// decision models through it. Maps to the optional <c>decisionModel</c> factory of a provider.
/// </summary>
public interface IDecisionProvider
{
    /// <summary>Returns a decision model, or null when the provider has no such model.</summary>
    IDecisionCaller? DecisionModel(string modelId);
}

/// <summary>Lifecycle event for decide.</summary>
public class DecideEvent
{
    /// <summary>Creates an event.</summary>
    public DecideEvent(string callId, string operationId, IReadOnlyDictionary<string, object?> runtimeContext, string provider, string modelId, DecisionState? state, IReadOnlyDictionary<string, DecisionQuestion> questions, int maxRetries, IReadOnlyDictionary<string, string>? headers, JsonElement providerOptions, IReadOnlyDictionary<string, DecisionAnswer>? answers = null, OperationUsage? usage = null, IReadOnlyList<OperationWarning>? warnings = null, bool? recordInputs = null, bool? recordOutputs = null, string? functionId = null, double? maxOutputTokens = null, string? reasoning = null)
    {
        CallId = callId;
        OperationId = operationId;
        RuntimeContext = runtimeContext;
        Provider = provider;
        ModelId = modelId;
        State = state;
        Questions = questions;
        MaxRetries = maxRetries;
        Headers = headers;
        ProviderOptions = providerOptions;
        Answers = answers;
        Usage = usage;
        Warnings = warnings;
        RecordInputs = recordInputs;
        RecordOutputs = recordOutputs;
        FunctionId = functionId;
        MaxOutputTokens = maxOutputTokens;
        Reasoning = reasoning;
    }

    /// <summary>Call id.</summary>
    public string CallId { get; }

    /// <summary><c>ai.decide</c>.</summary>
    public string OperationId { get; }

    /// <summary>Runtime context. Telemetry receives the filtered copy.</summary>
    public IReadOnlyDictionary<string, object?> RuntimeContext { get; }

    /// <summary>Provider id.</summary>
    public string Provider { get; }

    /// <summary>Model id.</summary>
    public string ModelId { get; }

    /// <summary>The public state, as the caller passed it.</summary>
    public DecisionState? State { get; }

    /// <summary>Questions.</summary>
    public IReadOnlyDictionary<string, DecisionQuestion> Questions { get; }

    /// <summary>Maximum output tokens requested for the decision.</summary>
    public double? MaxOutputTokens { get; }

    /// <summary>Reasoning effort requested for the decision.</summary>
    public string? Reasoning { get; }

    /// <summary>Resolved retry limit.</summary>
    public int MaxRetries { get; }

    /// <summary>Caller headers, without the user-agent suffix.</summary>
    public IReadOnlyDictionary<string, string>? Headers { get; }

    /// <summary>Provider options.</summary>
    public JsonElement ProviderOptions { get; }

    /// <summary>Answers, once the call has finished.</summary>
    public IReadOnlyDictionary<string, DecisionAnswer>? Answers { get; }

    /// <summary>Usage, once the call has finished.</summary>
    public OperationUsage? Usage { get; }

    /// <summary>Warnings, once the call has finished.</summary>
    public IReadOnlyList<OperationWarning>? Warnings { get; }

    /// <summary>Present only on telemetry events.</summary>
    public bool? RecordInputs { get; }

    /// <summary>Present only on telemetry events.</summary>
    public bool? RecordOutputs { get; }

    /// <summary>Present only on telemetry events.</summary>
    public string? FunctionId { get; }
}

/// <summary>Model-call telemetry event.</summary>
public class DecisionModelEvent
{
    /// <summary>Creates a model event.</summary>
    public DecisionModelEvent(string callId, string operationId, string provider, string modelId, IReadOnlyList<DecisionModelStatePart>? state, IReadOnlyDictionary<string, DecisionQuestion> questions, IReadOnlyDictionary<string, DecisionAnswer>? answers = null, OperationUsage? usage = null, bool? recordInputs = null, bool? recordOutputs = null, string? functionId = null)
    {
        CallId = callId;
        OperationId = operationId;
        Provider = provider;
        ModelId = modelId;
        State = state;
        Questions = questions;
        Answers = answers;
        Usage = usage;
        RecordInputs = recordInputs;
        RecordOutputs = recordOutputs;
        FunctionId = functionId;
    }

    /// <summary>Call id.</summary>
    public string CallId { get; }

    /// <summary><c>ai.decide.doDecide</c>.</summary>
    public string OperationId { get; }

    /// <summary>Provider id.</summary>
    public string Provider { get; }

    /// <summary>Model id.</summary>
    public string ModelId { get; }

    /// <summary>The normalized list of state parts the model receives.</summary>
    public IReadOnlyList<DecisionModelStatePart>? State { get; }

    /// <summary>Questions.</summary>
    public IReadOnlyDictionary<string, DecisionQuestion> Questions { get; }

    /// <summary>Answers after the model returns.</summary>
    public IReadOnlyDictionary<string, DecisionAnswer>? Answers { get; }

    /// <summary>Provider usage before total tokens are derived.</summary>
    public OperationUsage? Usage { get; }

    /// <summary>Telemetry input recording flag.</summary>
    public bool? RecordInputs { get; }

    /// <summary>Telemetry output recording flag.</summary>
    public bool? RecordOutputs { get; }

    /// <summary>Telemetry function id.</summary>
    public string? FunctionId { get; }
}

#pragma warning disable CS0618
/// <summary>Telemetry callbacks. Generic <c>onStart</c> and <c>onEnd</c> are not invoked.</summary>
public class DecideTelemetry
{
    /// <summary><c>experimental_onDecideStart</c>.</summary>
    public Func<DecideEvent, Task>? OnDecideStart { get; set; }

    /// <summary><c>experimental_onDecideEnd</c>.</summary>
    public Func<DecideEvent, Task>? OnDecideEnd { get; set; }

    /// <summary><c>experimental_onDecisionModelCallStart</c>.</summary>
    public Func<DecisionModelEvent, Task>? OnDecisionModelCallStart { get; set; }

    /// <summary><c>experimental_onDecisionModelCallEnd</c>.</summary>
    public Func<DecisionModelEvent, Task>? OnDecisionModelCallEnd { get; set; }

    /// <summary>Deprecated alias of <see cref="OnDecideStart"/>. It runs only when <see cref="OnDecideStart"/> is not set.</summary>
    [Obsolete("Use OnDecideStart instead.")]
    public Func<EvaluateEvent, Task>? OnEvaluateStart { get; set; }

    /// <summary>Deprecated alias of <see cref="OnDecideEnd"/>. It runs only when <see cref="OnDecideEnd"/> is not set.</summary>
    [Obsolete("Use OnDecideEnd instead.")]
    public Func<EvaluateEvent, Task>? OnEvaluateEnd { get; set; }

    /// <summary>Deprecated alias of <see cref="OnDecisionModelCallStart"/>. It runs only when <see cref="OnDecisionModelCallStart"/> is not set.</summary>
    [Obsolete("Use OnDecisionModelCallStart instead.")]
    public Func<EvaluateModelEvent, Task>? OnModelStart { get; set; }

    /// <summary>Deprecated alias of <see cref="OnDecisionModelCallEnd"/>. It runs only when <see cref="OnDecisionModelCallEnd"/> is not set.</summary>
    [Obsolete("Use OnDecisionModelCallEnd instead.")]
    public Func<EvaluateModelEvent, Task>? OnModelEnd { get; set; }

    internal Func<EvaluateEvent, Task>? LegacyOnStart => OnEvaluateStart;

    internal Func<EvaluateEvent, Task>? LegacyOnEnd => OnEvaluateEnd;

    internal Func<EvaluateModelEvent, Task>? LegacyOnModelStart => OnModelStart;

    internal Func<EvaluateModelEvent, Task>? LegacyOnModelEnd => OnModelEnd;

    /// <summary><c>onError</c>.</summary>
    public Func<string, Exception, Task>? OnError { get; set; }

    /// <summary>Keys copied into telemetry runtime context. Null yields an empty context.</summary>
    public IReadOnlyDictionary<string, bool>? IncludeRuntimeContext { get; set; }

    /// <summary>Copied onto telemetry events.</summary>
    public bool? RecordInputs { get; set; }

    /// <summary>Copied onto telemetry events.</summary>
    public bool? RecordOutputs { get; set; }

    /// <summary>Copied onto telemetry events.</summary>
    public string? FunctionId { get; set; }

    /// <summary>When false, telemetry callbacks are skipped.</summary>
    public bool IsEnabled { get; set; } = true;
}
#pragma warning restore CS0618

/// <summary>Decision result.</summary>
public class DecisionResult
{
    /// <summary>Creates a result.</summary>
    public DecisionResult(IReadOnlyDictionary<string, DecisionAnswer> answers, OperationUsage usage, IReadOnlyList<OperationWarning> warnings, DecisionRounding? rounding, JsonElement? providerMetadata, ProviderResponse response)
    {
        Answers = answers;
        Usage = usage;
        Warnings = warnings ?? Array.Empty<OperationWarning>();
        Rounding = rounding;
        ProviderMetadata = providerMetadata;
        Response = response;
    }

    /// <summary>Answers, with provider numbers unchanged.</summary>
    public IReadOnlyDictionary<string, DecisionAnswer> Answers { get; }

    /// <summary>Usage. <see cref="OperationUsage.TotalTokens"/> is input plus output when both are set.</summary>
    public OperationUsage Usage { get; }

    /// <summary>Warnings.</summary>
    public IReadOnlyList<OperationWarning> Warnings { get; }

    /// <summary>Rounding declaration.</summary>
    public DecisionRounding? Rounding { get; }

    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Response. Timestamp and model id are filled when the provider omitted them.</summary>
    public ProviderResponse Response { get; }
}

/// <summary>Options for <see cref="Decide.DecideAsync"/>.</summary>
public sealed class DecideRequest : OperationRequest
{
    /// <summary>Explicit decision model. Wins over <see cref="ModelId"/> when both are set.</summary>
    public IDecisionCaller? Model { get; set; }

    /// <summary>
    /// Decision model id, used when <see cref="Model"/> is null. It goes through <see cref="DecisionModels.DefaultProvider"/>,
    /// or Gateway when none is set.
    /// </summary>
    public string? ModelId { get; set; }

    /// <summary>
    /// Shared state: a string, a JSON object, or a list of parts. A JSON array is not valid state.
    /// Wrap it in an object or in a <see cref="DecisionJsonPart"/>. See <see cref="DecisionState"/>.
    /// </summary>
    public DecisionState? State { get; set; }

    /// <summary>Nonempty question map.</summary>
    public IReadOnlyDictionary<string, DecisionQuestion>? Questions { get; set; }

    /// <summary>Maximum output tokens for supported models. Must be an integer of at least 1 when set.</summary>
    public double? MaxOutputTokens { get; set; }

    /// <summary>
    /// Reasoning effort for supported models: <c>provider-default</c>, <c>none</c>, <c>minimal</c>, <c>low</c>, <c>medium</c>, <c>high</c>,
    /// <c>xhigh</c>, or <c>max</c>. Language-model adapters default to <c>none</c>; <c>provider-default</c> uses the wrapped language
    /// model's own default instead. Native decision providers return an unsupported-setting warning.
    /// </summary>
    public string? Reasoning { get; set; }

    /// <summary>Called when the decision begins. Receives the full runtime context.</summary>
    public Func<DecideEvent, Task>? OnStart { get; set; }

    /// <summary>Called when the decision completes. Receives the full runtime context.</summary>
    public Func<DecideEvent, Task>? OnEnd { get; set; }

    /// <summary>Telemetry. Generic operation callbacks on the integration are not called.</summary>
    public DecideTelemetry? Telemetry { get; set; }
}

/// <summary>Decides typed questions. Maps to <c>experimental_decide</c>.</summary>
public static class Decide
{
    private const double Tolerance = 1e-6;

    /// <summary>Decides every question in one model call.</summary>
    public static async Task<DecisionResult> DecideAsync(DecideRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var model = DecisionModels.Resolve(request.Model, request.ModelId);

        LanguageModelCallPreparation.PrepareLanguageModelCallOptions(new LanguageModelCallOptionsInput { MaxOutputTokens = request.MaxOutputTokens });
        var questions = request.Questions ?? new Dictionary<string, DecisionQuestion>();
        ValidateInput(request.State, questions);
        foreach (var pair in questions)
        {
            if (!model.SupportedQuestionTypes.Contains(pair.Value.Type))
            {
                // The deprecated subclass lets both the old and the new catch clauses match.
#pragma warning disable CS0618
                throw new EvaluationUnsupportedQuestionTypeException(pair.Key, pair.Value.Type, model.Provider, model.ModelId);
#pragma warning restore CS0618
            }
        }

        var token = request.CancellationToken.CanBeCanceled ? request.CancellationToken : cancellationToken;
        var callId = (request.GenerateCallId ?? (() => "call-" + Guid.NewGuid().ToString("N")))();
        var maxRetries = OperationRetry.ResolveMaxRetries(request.MaxRetries);
        var context = request.RuntimeContext ?? new Dictionary<string, object?>();
        var providerOptions = request.ProviderOptions ?? OperationJson.Parse("{}");
        var telemetry = request.Telemetry;
        var enabled = telemetry == null || telemetry.IsEnabled;
        var userStart = new DecideEvent(callId, "ai.decide", context, model.Provider, model.ModelId, request.State, questions, maxRetries, request.Headers, providerOptions, maxOutputTokens: request.MaxOutputTokens, reasoning: request.Reasoning);
        await OperationCallbacks.NotifyAsync(userStart, request.OnStart).ConfigureAwait(false);
        if (enabled)
        {
            await NotifyTelemetryAsync(WithTelemetry(userStart, Filter(context, telemetry?.IncludeRuntimeContext), telemetry), telemetry?.OnDecideStart, telemetry?.LegacyOnStart).ConfigureAwait(false);
        }

        try
        {
            var preparedState = await DecisionState.PrepareDecisionStateAsync(request.State!, token).ConfigureAwait(false);
            var modelEvent = new DecisionModelEvent(callId, "ai.decide.doDecide", model.Provider, model.ModelId, preparedState, questions, recordInputs: telemetry?.RecordInputs, recordOutputs: telemetry?.RecordOutputs, functionId: telemetry?.FunctionId);
            if (enabled)
            {
                await NotifyTelemetryAsync(modelEvent, telemetry?.OnDecisionModelCallStart, telemetry?.LegacyOnModelStart).ConfigureAwait(false);
            }

            var headers = OperationHeaders.WithUserAgent(request.Headers, AiSdkVersion.UserAgent);
            var result = await OperationRetry.ExecuteAsync(request.MaxRetries, token, request.AbortReason, async ct =>
            {
                OperationRetry.ThrowIfAborted(ct, request.AbortReason);
                return await model.DoDecideAsync(new DecisionModelCall(preparedState, questions, providerOptions, headers, ct, request.MaxOutputTokens, request.Reasoning), ct).ConfigureAwait(false);
            }, null).ConfigureAwait(false);
            OperationRetry.ThrowIfAborted(token, request.AbortReason);
            ValidateAnswers(questions, result.Answers, result.Rounding);
            var refused = questions.Keys.Where(id => result.Answers[id].Type == "refusal").ToArray();
            if (refused.Length > 0)
            {
                throw new DecisionRefusalError(refused, model.Provider, model.ModelId);
            }

            if (enabled)
            {
                await NotifyTelemetryAsync(new DecisionModelEvent(callId, "ai.decide.doDecide", model.Provider, model.ModelId, preparedState, questions, result.Answers, result.Usage, telemetry?.RecordInputs, telemetry?.RecordOutputs, telemetry?.FunctionId), telemetry?.OnDecisionModelCallEnd, telemetry?.LegacyOnModelEnd).ConfigureAwait(false);
            }

            WarningLog.Write(result.Warnings, model.Provider, model.ModelId);
            var input = result.Usage?.InputTokens;
            var output = result.Usage?.OutputTokens;
            var usage = new OperationUsage(input, output, input != null && output != null ? input + output : null);
            var now = request.Now?.Invoke() ?? DateTime.UtcNow;
            var response = new ProviderResponse(result.Response?.Headers, result.Response?.Body, result.Response?.Id, result.Response?.Timestamp ?? now, result.Response?.ModelId ?? model.ModelId, result.Response?.ProviderMetadata);
            var decision = new DecisionResult(result.Answers, usage, result.Warnings, result.Rounding, result.ProviderMetadata, response);
            var end = new DecideEvent(callId, "ai.decide", context, model.Provider, model.ModelId, request.State, questions, maxRetries, request.Headers, providerOptions, result.Answers, usage, result.Warnings, maxOutputTokens: request.MaxOutputTokens, reasoning: request.Reasoning);
            await OperationCallbacks.NotifyAsync(end, request.OnEnd).ConfigureAwait(false);
            if (enabled)
            {
                await NotifyTelemetryAsync(WithTelemetry(end, Filter(context, telemetry?.IncludeRuntimeContext), telemetry), telemetry?.OnDecideEnd, telemetry?.LegacyOnEnd).ConfigureAwait(false);
            }

            return decision;
        }
        catch (Exception error)
        {
            if (enabled && telemetry?.OnError != null)
            {
                try
                {
                    await telemetry.OnError(callId, error).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    // Callback failures are isolated.
                }
            }

            throw;
        }
    }

    /// <summary>Validates state and questions before any model call.</summary>
    public static void ValidateInput(DecisionState? state, IReadOnlyDictionary<string, DecisionQuestion>? questions)
    {
        if (state == null || (state.Parts != null && state.Parts.Any(part => part == null)))
        {
            throw new InvalidArgumentException("state", state, DecisionState.InvalidMessage);
        }

        if (questions == null || questions.Count == 0)
        {
            throw new InvalidArgumentException("questions", questions, "must be a nonempty question map");
        }

        foreach (var pair in questions)
        {
            var parameter = "questions." + pair.Key;
            var question = pair.Value;
            if (question == null || (question.Type == "box" && question.Instructions == null))
            {
                throw new InvalidArgumentException(parameter, question, "instructions must be a JSON-compatible string, object, or array");
            }

            var criteria = question.Criteria;
            switch (question.Type)
            {
                case "box":
                    break;
                case "choice":
                    if (criteria is not DecisionChoiceCriteria { Count: > 0 })
                    {
                        throw new InvalidArgumentException(parameter, question, "choice criteria must be a nonempty option map");
                    }

                    break;
                case "score":
                    if (criteria is not DecisionScoreCriteria { Count: >= 1 })
                    {
                        throw new InvalidArgumentException(parameter, question, "score criteria must contain at least one ordered level");
                    }

                    break;
                case "boolean":
                    if (question.Instructions == null && !(criteria is DecisionBooleanCriteria { True: not null } or DecisionBooleanCriteria { False: not null }))
                    {
                        throw new InvalidArgumentException(parameter, question, "boolean questions require instructions or at least one non-null criterion");
                    }

                    if (criteria != null && criteria is not DecisionBooleanCriteria)
                    {
                        throw new InvalidArgumentException(parameter, question, "boolean criteria may only describe true and false");
                    }

                    break;
                default:
                    throw new InvalidArgumentException(parameter, question, "question type must be choice, score, boolean, or box");
            }
        }
    }

    /// <summary>Validates answers without rewriting provider numbers.</summary>
    public static void ValidateAnswers(IReadOnlyDictionary<string, DecisionQuestion> questions, IReadOnlyDictionary<string, DecisionAnswer>? answers, DecisionRounding? rounding)
    {
        double Error(int? decimals)
        {
            if (decimals == null)
            {
                return 0;
            }

            if (decimals < 0 || decimals > 15)
            {
                throw new InvalidResponseDataException(answers, "Decision rounding decimals must be integers between 0 and 15.");
            }

            return 0.5 * Math.Pow(10, -decimals.Value);
        }

        var probabilityError = Error(rounding?.ProbabilityDecimals);
        var scoreError = Error(rounding?.ScoreDecimals);
        if (answers == null || !SameKeys(answers.Keys, questions.Keys))
        {
            throw new InvalidResponseDataException(answers, "Decision must return exactly one answer for every question.");
        }

        foreach (var pair in questions)
        {
            if (!answers.TryGetValue(pair.Key, out var answer) || answer == null || (answer.Type != pair.Value.Type && answer.Type != "refusal"))
            {
                throw new InvalidResponseDataException(answers, "Question \"" + pair.Key + "\" returned an answer with the wrong type.");
            }

            if (answer.Type == "refusal")
            {
                continue;
            }

            switch (pair.Value.Type)
            {
                case "choice":
                    var options = ((DecisionChoiceCriteria)pair.Value.Criteria!).Options;
                    if (answer.Choice == null || !options.ContainsKey(answer.Choice))
                    {
                        throw new InvalidResponseDataException(answers, "Question \"" + pair.Key + "\" selected an unknown option.");
                    }

                    if (answer.Probabilities != null)
                    {
                        ValidateDistribution(answer.Probabilities, options.Keys, answers, pair.Key, probabilityError);
                        var selected = answer.Probabilities[answer.Choice];
                        if (answer.Probabilities.Values.Any(probability => probability > selected + Tolerance + (2 * probabilityError)))
                        {
                            throw new InvalidResponseDataException(answers, "Question \"" + pair.Key + "\" did not select a highest-probability option.");
                        }
                    }

                    break;
                case "score":
                    var levels = ((DecisionScoreCriteria)pair.Value.Criteria!).Levels;
                    if (answer.Score == null || double.IsNaN(answer.Score.Value) || double.IsInfinity(answer.Score.Value) || answer.Score < 0 || answer.Score > levels.Count - 1)
                    {
                        throw new InvalidResponseDataException(answers, "Question \"" + pair.Key + "\" score must be in [0, " + (levels.Count - 1).ToString(CultureInfo.InvariantCulture) + "].");
                    }

                    if (answer.Probabilities != null)
                    {
                        var keys = Enumerable.Range(0, levels.Count).Select(index => index.ToString(CultureInfo.InvariantCulture)).ToArray();
                        ValidateDistribution(answer.Probabilities, keys, answers, pair.Key, probabilityError);
                        var mean = answer.Probabilities.Sum(item => int.Parse(item.Key, CultureInfo.InvariantCulture) * item.Value);
                        var meanError = keys.Sum(index => int.Parse(index, CultureInfo.InvariantCulture) * probabilityError);
                        if (Math.Abs(mean - answer.Score.Value) > Tolerance + meanError + scoreError)
                        {
                            throw new InvalidResponseDataException(answers, "Question \"" + pair.Key + "\" score must equal the probability-weighted mean within the declared rounding precision.");
                        }
                    }

                    break;
                case "box":
                    var box = answer.Box;
                    if (box == null
                        || !IsProbability(box.Left) || !IsProbability(box.Right) || !IsProbability(box.Top) || !IsProbability(box.Bottom)
                        || !(box.Left < box.Right) || !(box.Top < box.Bottom))
                    {
                        throw new InvalidResponseDataException(answers, "Question \"" + pair.Key + "\" must return box edges in [0, 1] with left < right and top < bottom.");
                    }

                    break;
                case "boolean":
                    if (answer.Probability == null || !IsProbability(answer.Probability.Value))
                    {
                        throw new InvalidResponseDataException(answers, "Question \"" + pair.Key + "\" must return P(true) as a finite probability in [0, 1].");
                    }

                    break;
            }
        }
    }

    private static void ValidateDistribution(IReadOnlyDictionary<string, double> value, IEnumerable<string> keys, object? answers, string id, double roundingError)
    {
        var keyList = keys.ToArray();
        if (!SameKeys(value.Keys, keyList) || value.Values.Any(probability => !IsProbability(probability)))
        {
            throw new InvalidResponseDataException(answers, "Question \"" + id + "\" must have a complete distribution of finite probabilities in [0, 1].");
        }

        var sum = value.Values.Sum();
        if (Math.Abs(sum - 1) > Tolerance + (keyList.Length * roundingError))
        {
            throw new InvalidResponseDataException(answers, "Question \"" + id + "\" probabilities must sum to 1 within the declared rounding precision.");
        }
    }

    private static bool IsProbability(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value) && value >= 0 && value <= 1;
    }

    private static bool SameKeys(IEnumerable<string> left, IEnumerable<string> right)
    {
        var leftList = left.ToArray();
        var rightList = right.ToArray();
        if (leftList.Length != rightList.Length)
        {
            return false;
        }

        var set = new HashSet<string>(leftList);
        return rightList.All(set.Contains);
    }

    private static IReadOnlyDictionary<string, object?> Filter(IReadOnlyDictionary<string, object?> context, IReadOnlyDictionary<string, bool>? include)
    {
        if (include == null)
        {
            return new Dictionary<string, object?>();
        }

        var filtered = new Dictionary<string, object?>();
        foreach (var pair in include)
        {
            if (pair.Value && context.TryGetValue(pair.Key, out var value))
            {
                filtered[pair.Key] = value;
            }
        }

        return filtered;
    }

#pragma warning disable CS0618
    private static Task NotifyTelemetryAsync(DecideEvent source, Func<DecideEvent, Task>? current, Func<EvaluateEvent, Task>? deprecated)
    {
        return current != null ? OperationCallbacks.NotifyAsync(source, current) : deprecated != null ? OperationCallbacks.NotifyAsync(EvaluateEvent.From(source), deprecated) : Task.CompletedTask;
    }

    private static Task NotifyTelemetryAsync(DecisionModelEvent source, Func<DecisionModelEvent, Task>? current, Func<EvaluateModelEvent, Task>? deprecated)
    {
        return current != null ? OperationCallbacks.NotifyAsync(source, current) : deprecated != null ? OperationCallbacks.NotifyAsync(EvaluateModelEvent.From(source), deprecated) : Task.CompletedTask;
    }

#pragma warning restore CS0618

    private static DecideEvent WithTelemetry(DecideEvent source, IReadOnlyDictionary<string, object?> context, DecideTelemetry? telemetry)
    {
        return new DecideEvent(source.CallId, source.OperationId, context, source.Provider, source.ModelId, source.State, source.Questions, source.MaxRetries, source.Headers, source.ProviderOptions, source.Answers, source.Usage, source.Warnings, telemetry?.RecordInputs, telemetry?.RecordOutputs, telemetry?.FunctionId, source.MaxOutputTokens, source.Reasoning);
    }
}
