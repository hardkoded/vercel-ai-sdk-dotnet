// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Vercel.AI.Prompt;
using Vercel.AI.Util;

namespace Vercel.AI.Operations;

/// <summary>Markers for values that are not JSON.</summary>
public static class DecisionValues
{
    /// <summary>A function, which JSON cannot represent.</summary>
    public static readonly object Function = new FunctionMarker();

    /// <summary>An omitted value, which JSON cannot represent.</summary>
    public static readonly object Undefined = new UndefinedMarker();

    /// <summary>Not a number.</summary>
    public static readonly object NotANumber = double.NaN;

    /// <summary>A sparse array. Holes are not JSON.</summary>
    public static readonly object SparseArray = new SparseMarker();

    /// <summary>A <see cref="DateTime"/>, rejected the way a JavaScript Date is rejected.</summary>
    public static object Date()
    {
        return DateTime.UtcNow;
    }

    private sealed class FunctionMarker
    {
    }

    private sealed class UndefinedMarker
    {
    }

    private sealed class SparseMarker
    {
    }
}

/// <summary>One decision question.</summary>
public class DecisionQuestion
{
    /// <summary>Creates a question.</summary>
    public DecisionQuestion(string type, object? instructions, object? criteria = null)
    {
        Type = type ?? string.Empty;
        Instructions = instructions;
        Criteria = criteria;
    }

    /// <summary><c>choice</c>, <c>score</c>, or <c>boolean</c>.</summary>
    public string Type { get; }

    /// <summary>Instructions. A JSON string, object, or array.</summary>
    public object? Instructions { get; }

    /// <summary>Choice map, ordered score levels, or optional boolean descriptions.</summary>
    public object? Criteria { get; }
}

/// <summary>One model answer. Numbers are returned unchanged.</summary>
public class DecisionAnswer
{
    /// <summary>Creates an answer.</summary>
    public DecisionAnswer(string type, string? choice = null, double? score = null, double? probability = null, IReadOnlyDictionary<string, double>? probabilities = null)
    {
        Type = type;
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
    public DecisionModelCall(object? state, IReadOnlyDictionary<string, DecisionQuestion> questions, JsonElement providerOptions, IReadOnlyDictionary<string, string> headers, CancellationToken cancellationToken)
    {
        State = state;
        Questions = questions;
        ProviderOptions = providerOptions;
        Headers = headers;
        CancellationToken = cancellationToken;
    }

    /// <summary>Shared state as an ordered list of <c>text</c>, <c>json</c>, and <c>file</c> part dictionaries. See <see cref="DecisionState"/>.</summary>
    public object? State { get; }

    /// <summary>Questions.</summary>
    public IReadOnlyDictionary<string, DecisionQuestion> Questions { get; }

    /// <summary>Provider options.</summary>
    public JsonElement ProviderOptions { get; }

    /// <summary>Headers including the user-agent suffix.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>Cancellation token.</summary>
    public CancellationToken CancellationToken { get; }
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

/// <summary>Lifecycle event for decide.</summary>
public class DecideEvent
{
    /// <summary>Creates an event.</summary>
    public DecideEvent(string callId, string operationId, IReadOnlyDictionary<string, object?> runtimeContext, string provider, string modelId, object? state, IReadOnlyDictionary<string, DecisionQuestion> questions, int maxRetries, IReadOnlyDictionary<string, string>? headers, JsonElement providerOptions, IReadOnlyDictionary<string, DecisionAnswer>? answers = null, OperationUsage? usage = null, IReadOnlyList<OperationWarning>? warnings = null, bool? recordInputs = null, bool? recordOutputs = null, string? functionId = null)
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
    public object? State { get; }

    /// <summary>Questions.</summary>
    public IReadOnlyDictionary<string, DecisionQuestion> Questions { get; }

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
    public DecisionModelEvent(string callId, string operationId, string provider, string modelId, object? state, IReadOnlyDictionary<string, DecisionQuestion> questions, IReadOnlyDictionary<string, DecisionAnswer>? answers = null, OperationUsage? usage = null, bool? recordInputs = null, bool? recordOutputs = null, string? functionId = null)
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
    public object? State { get; }

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
public class DecideRequest : OperationRequest
{
    /// <summary>Decision model.</summary>
    public IDecisionCaller? Model { get; set; }

    /// <summary>
    /// Shared state: a string, a JSON object (<c>IDictionary&lt;string, object?&gt;</c>), or a list of parts (<c>IList&lt;object?&gt;</c>).
    /// A JSON array is not valid state. Wrap it in an object or in a <c>json</c> part. See <see cref="DecisionState"/>.
    /// </summary>
    public object? State { get; set; }

    /// <summary>Nonempty question map.</summary>
    public IReadOnlyDictionary<string, DecisionQuestion>? Questions { get; set; }

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

        var model = request.Model ?? throw new InvalidArgumentException("model", null, "model is required");
        if (model.SpecificationVersion != "v4")
        {
            throw new UnsupportedModelVersionException(model.SpecificationVersion, model.Provider, model.ModelId);
        }

        var questions = request.Questions ?? new Dictionary<string, DecisionQuestion>();
        ValidateInput(request.State, questions);
        foreach (var pair in questions)
        {
            if (!model.SupportedQuestionTypes.Contains(pair.Value.Type))
            {
                throw new DecisionUnsupportedQuestionTypeException(pair.Key, pair.Value.Type, model.Provider, model.ModelId);
            }
        }

        var token = request.CancellationToken.CanBeCanceled ? request.CancellationToken : cancellationToken;
        var callId = (request.GenerateCallId ?? (() => "call-" + Guid.NewGuid().ToString("N")))();
        var maxRetries = OperationRetry.ResolveMaxRetries(request.MaxRetries);
        var context = request.RuntimeContext ?? new Dictionary<string, object?>();
        var providerOptions = request.ProviderOptions ?? OperationJson.Parse("{}");
        var telemetry = request.Telemetry;
        var enabled = telemetry == null || telemetry.IsEnabled;
        var userStart = new DecideEvent(callId, "ai.decide", context, model.Provider, model.ModelId, request.State, questions, maxRetries, request.Headers, providerOptions);
        await OperationCallbacks.NotifyAsync(userStart, request.OnStart).ConfigureAwait(false);
        if (enabled)
        {
            await NotifyTelemetryAsync(WithTelemetry(userStart, Filter(context, telemetry?.IncludeRuntimeContext), telemetry), telemetry?.OnDecideStart, telemetry?.LegacyOnStart).ConfigureAwait(false);
        }

        try
        {
            var preparedState = await DecisionState.PrepareDecisionStateAsync(request.State, token).ConfigureAwait(false);
            var modelEvent = new DecisionModelEvent(callId, "ai.decide.doDecide", model.Provider, model.ModelId, preparedState, questions, recordInputs: telemetry?.RecordInputs, recordOutputs: telemetry?.RecordOutputs, functionId: telemetry?.FunctionId);
            if (enabled)
            {
                await NotifyTelemetryAsync(modelEvent, telemetry?.OnDecisionModelCallStart, telemetry?.LegacyOnModelStart).ConfigureAwait(false);
            }

            var headers = OperationHeaders.WithUserAgent(request.Headers, AiSdkVersion.UserAgent);
            var result = await OperationRetry.ExecuteAsync(request.MaxRetries, token, request.AbortReason, async ct =>
            {
                OperationRetry.ThrowIfAborted(ct, request.AbortReason);
                return await model.DoDecideAsync(new DecisionModelCall(preparedState, questions, providerOptions, headers, ct), ct).ConfigureAwait(false);
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
            var end = new DecideEvent(callId, "ai.decide", context, model.Provider, model.ModelId, request.State, questions, maxRetries, request.Headers, providerOptions, result.Answers, usage, result.Warnings);
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
    public static void ValidateInput(object? state, IReadOnlyDictionary<string, DecisionQuestion>? questions)
    {
        var validState = state is IList<object?> parts
            ? parts.All(IsStatePart)
            : (state is string || state is IDictionary<string, object?>) && IsJson(state, new HashSet<object>(ReferenceComparer.Instance));
        if (!validState)
        {
            throw new InvalidArgumentException("state", state, "must be a string, JSON object, or array of text, file, or json parts");
        }

        if (questions == null || questions.Count == 0)
        {
            throw new InvalidArgumentException("questions", questions, "must be a nonempty question map");
        }

        foreach (var pair in questions)
        {
            var parameter = "questions." + pair.Key;
            var question = pair.Value;
            if (question == null || !IsInput(question.Instructions, new HashSet<object>(ReferenceComparer.Instance)))
            {
                throw new InvalidArgumentException(parameter, question, "instructions must be a JSON-compatible string, object, or array");
            }

            var criteria = question.Criteria;
            switch (question.Type)
            {
                case "choice":
                    if (criteria is not IDictionary<string, object?> options || options.Count == 0)
                    {
                        throw new InvalidArgumentException(parameter, question, "choice criteria must be a nonempty option map");
                    }

                    break;
                case "score":
                    if (criteria is not IList<object?> levels || levels.Count < 2)
                    {
                        throw new InvalidArgumentException(parameter, question, "score criteria must contain at least two ordered levels");
                    }

                    break;
                case "boolean":
                    if (criteria == null)
                    {
                        continue;
                    }

                    if (criteria is not IDictionary<string, object?> descriptions || descriptions.Keys.Any(key => key != "true" && key != "false"))
                    {
                        throw new InvalidArgumentException(parameter, question, "boolean criteria may only describe true and false");
                    }

                    break;
                default:
                    throw new InvalidArgumentException(parameter, question, "question type must be choice, score, or boolean");
            }

            if (!IsJson(criteria, new HashSet<object>(ReferenceComparer.Instance)) || Values(criteria).Any(value => value != null && !IsInput(value, new HashSet<object>(ReferenceComparer.Instance))))
            {
                throw new InvalidArgumentException(parameter, question, "criteria descriptions must be JSON-compatible strings, objects, arrays, or null");
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
                    var options = (IDictionary<string, object?>)pair.Value.Criteria!;
                    if (answer.Choice == null || !options.ContainsKey(answer.Choice))
                    {
                        throw new InvalidResponseDataException(answers, "Question \"" + pair.Key + "\" selected an unknown option.");
                    }

                    if (answer.Probabilities != null)
                    {
                        ValidateDistribution(answer.Probabilities, options.Keys, answers, pair.Key, probabilityError);
                        var selected = answer.Probabilities[answer.Choice];
                        if (answer.Probabilities.Values.Any(probability => probability > selected + Tolerance))
                        {
                            throw new InvalidResponseDataException(answers, "Question \"" + pair.Key + "\" did not select a highest-probability option.");
                        }
                    }

                    break;
                case "score":
                    var levels = (IList<object?>)pair.Value.Criteria!;
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

    private static bool IsStatePart(object? value)
    {
        if (value is not IDictionary<string, object?> part || !part.TryGetValue("type", out var type))
        {
            return false;
        }

        switch (type as string)
        {
            case "text":
                return part.TryGetValue("text", out var text) && text is string;
            case "json":
                return part.TryGetValue("value", out var json) && IsJson(json, new HashSet<object>(ReferenceComparer.Instance));
            case "file":
                return part.TryGetValue("mediaType", out var mediaType) && mediaType is string
                    && part.TryGetValue("data", out var data) && (data is FilePartInput || data is byte[] || data is Uri || data is string)
                    && (!part.TryGetValue("filename", out var filename) || filename == null || filename is string);
            default:
                return false;
        }
    }

    private static bool IsInput(object? value, HashSet<object> ancestors)
    {
        return (value is string || value is IList<object?> || value is IDictionary<string, object?>) && IsJson(value, ancestors);
    }

    private static bool IsJson(object? value, HashSet<object> ancestors)
    {
        if (value == null || value is string || value is bool)
        {
            return true;
        }

        if (value is double number)
        {
            return !double.IsNaN(number) && !double.IsInfinity(number);
        }

        if (value is int || value is long || value is JsonElement)
        {
            return true;
        }

        if (value is DateTime || ReferenceEquals(value, DecisionValues.Function) || ReferenceEquals(value, DecisionValues.Undefined) || ReferenceEquals(value, DecisionValues.SparseArray))
        {
            return false;
        }

        if (value is not IList<object?> && value is not IDictionary<string, object?>)
        {
            return false;
        }

        if (!ancestors.Add(value))
        {
            return false;
        }

        var valid = Values(value).All(item => IsJson(item, ancestors));
        ancestors.Remove(value);
        return valid;
    }

    private static IEnumerable<object?> Values(object? value)
    {
        if (value is IList<object?> list)
        {
            return list;
        }

        if (value is IDictionary<string, object?> map)
        {
            return map.Values;
        }

        return Array.Empty<object?>();
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

    private sealed class ReferenceComparer : IEqualityComparer<object>
    {
        public static readonly ReferenceComparer Instance = new ReferenceComparer();

        bool IEqualityComparer<object>.Equals(object? x, object? y)
        {
            return ReferenceEquals(x, y);
        }

        int IEqualityComparer<object>.GetHashCode(object obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
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
        return new DecideEvent(source.CallId, source.OperationId, context, source.Provider, source.ModelId, source.State, source.Questions, source.MaxRetries, source.Headers, source.ProviderOptions, source.Answers, source.Usage, source.Warnings, telemetry?.RecordInputs, telemetry?.RecordOutputs, telemetry?.FunctionId);
    }
}
