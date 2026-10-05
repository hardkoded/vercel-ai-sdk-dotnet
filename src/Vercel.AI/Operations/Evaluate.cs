// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Vercel.AI.Operations;

/// <summary>Markers for values that are not JSON.</summary>
public static class EvaluationValues
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

/// <summary>One evaluation question.</summary>
public sealed class EvaluationQuestion
{
    /// <summary>Creates a question.</summary>
    public EvaluationQuestion(string type, object? instructions, object? criteria = null)
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
public sealed class EvaluationAnswer
{
    /// <summary>Creates an answer.</summary>
    public EvaluationAnswer(string type, string? choice = null, double? score = null, double? probability = null, IReadOnlyDictionary<string, double>? probabilities = null)
    {
        Type = type;
        Choice = choice;
        Score = score;
        Probability = probability;
        Probabilities = probabilities;
    }

    /// <summary>Answer type.</summary>
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
public sealed class EvaluationRounding
{
    /// <summary>Creates rounding limits.</summary>
    public EvaluationRounding(int? probabilityDecimals = null, int? scoreDecimals = null)
    {
        ProbabilityDecimals = probabilityDecimals;
        ScoreDecimals = scoreDecimals;
    }

    /// <summary>Probability decimal places, from 0 through 15.</summary>
    public int? ProbabilityDecimals { get; }

    /// <summary>Score decimal places, from 0 through 15.</summary>
    public int? ScoreDecimals { get; }
}

/// <summary>Model response for <c>doEvaluate</c>.</summary>
public sealed class EvaluationModelResult
{
    /// <summary>Creates a model result.</summary>
    public EvaluationModelResult(IReadOnlyDictionary<string, EvaluationAnswer> answers, IReadOnlyList<OperationWarning>? warnings = null, OperationUsage? usage = null, EvaluationRounding? rounding = null, JsonElement? providerMetadata = null, ProviderResponse? response = null)
    {
        Answers = answers ?? new Dictionary<string, EvaluationAnswer>();
        Warnings = warnings ?? Array.Empty<OperationWarning>();
        Usage = usage;
        Rounding = rounding;
        ProviderMetadata = providerMetadata;
        Response = response;
    }

    /// <summary>Answers keyed by question id.</summary>
    public IReadOnlyDictionary<string, EvaluationAnswer> Answers { get; }

    /// <summary>Warnings.</summary>
    public IReadOnlyList<OperationWarning> Warnings { get; }

    /// <summary>Token usage.</summary>
    public OperationUsage? Usage { get; }

    /// <summary>Rounding declaration.</summary>
    public EvaluationRounding? Rounding { get; }

    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Response metadata. Timestamp and model id are filled when omitted.</summary>
    public ProviderResponse? Response { get; }
}

/// <summary>Arguments for <c>doEvaluate</c>.</summary>
public sealed class EvaluationModelCall
{
    /// <summary>Creates a call.</summary>
    public EvaluationModelCall(object? state, IReadOnlyDictionary<string, EvaluationQuestion> questions, JsonElement providerOptions, IReadOnlyDictionary<string, string> headers, CancellationToken cancellationToken)
    {
        State = state;
        Questions = questions;
        ProviderOptions = providerOptions;
        Headers = headers;
        CancellationToken = cancellationToken;
    }

    /// <summary>Shared state.</summary>
    public object? State { get; }

    /// <summary>Questions.</summary>
    public IReadOnlyDictionary<string, EvaluationQuestion> Questions { get; }

    /// <summary>Provider options.</summary>
    public JsonElement ProviderOptions { get; }

    /// <summary>Headers including the user-agent suffix.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>Cancellation token.</summary>
    public CancellationToken CancellationToken { get; }
}

/// <summary>Evaluation model used by <see cref="Evaluate"/>.</summary>
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

    /// <summary>Evaluates every question in one call.</summary>
    Task<EvaluationModelResult> DoEvaluateAsync(EvaluationModelCall call, CancellationToken cancellationToken);
}

/// <summary>Lifecycle event for evaluate.</summary>
public sealed class EvaluateEvent
{
    /// <summary>Creates an event.</summary>
    public EvaluateEvent(string callId, string operationId, IReadOnlyDictionary<string, object?> runtimeContext, string provider, string modelId, object? state, IReadOnlyDictionary<string, EvaluationQuestion> questions, int maxRetries, IReadOnlyDictionary<string, string>? headers, JsonElement providerOptions, IReadOnlyDictionary<string, EvaluationAnswer>? answers = null, OperationUsage? usage = null, IReadOnlyList<OperationWarning>? warnings = null, bool? recordInputs = null, bool? recordOutputs = null, string? functionId = null)
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

    /// <summary><c>ai.evaluate</c> or <c>ai.evaluate.doEvaluate</c>.</summary>
    public string OperationId { get; }

    /// <summary>Runtime context. Telemetry receives the filtered copy.</summary>
    public IReadOnlyDictionary<string, object?> RuntimeContext { get; }

    /// <summary>Provider id.</summary>
    public string Provider { get; }

    /// <summary>Model id.</summary>
    public string ModelId { get; }

    /// <summary>State.</summary>
    public object? State { get; }

    /// <summary>Questions.</summary>
    public IReadOnlyDictionary<string, EvaluationQuestion> Questions { get; }

    /// <summary>Resolved retry limit.</summary>
    public int MaxRetries { get; }

    /// <summary>Caller headers, without the user-agent suffix.</summary>
    public IReadOnlyDictionary<string, string>? Headers { get; }

    /// <summary>Provider options.</summary>
    public JsonElement ProviderOptions { get; }

    /// <summary>Answers, once the call has finished.</summary>
    public IReadOnlyDictionary<string, EvaluationAnswer>? Answers { get; }

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
public sealed class EvaluateModelEvent
{
    /// <summary>Creates a model event.</summary>
    public EvaluateModelEvent(string callId, string operationId, string provider, string modelId, object? state, IReadOnlyDictionary<string, EvaluationQuestion> questions, IReadOnlyDictionary<string, EvaluationAnswer>? answers = null, OperationUsage? usage = null, bool? recordInputs = null, bool? recordOutputs = null, string? functionId = null)
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

    /// <summary><c>ai.evaluate.doEvaluate</c>.</summary>
    public string OperationId { get; }

    /// <summary>Provider id.</summary>
    public string Provider { get; }

    /// <summary>Model id.</summary>
    public string ModelId { get; }

    /// <summary>State.</summary>
    public object? State { get; }

    /// <summary>Questions.</summary>
    public IReadOnlyDictionary<string, EvaluationQuestion> Questions { get; }

    /// <summary>Answers after the model returns.</summary>
    public IReadOnlyDictionary<string, EvaluationAnswer>? Answers { get; }

    /// <summary>Provider usage before total tokens are derived.</summary>
    public OperationUsage? Usage { get; }

    /// <summary>Telemetry input recording flag.</summary>
    public bool? RecordInputs { get; }

    /// <summary>Telemetry output recording flag.</summary>
    public bool? RecordOutputs { get; }

    /// <summary>Telemetry function id.</summary>
    public string? FunctionId { get; }
}

/// <summary>Telemetry callbacks. Generic <c>onStart</c> and <c>onEnd</c> are not invoked.</summary>
public sealed class EvaluateTelemetry
{
    /// <summary><c>experimental_onEvaluateStart</c>.</summary>
    public Func<EvaluateEvent, Task>? OnEvaluateStart { get; set; }

    /// <summary><c>experimental_onEvaluateEnd</c>.</summary>
    public Func<EvaluateEvent, Task>? OnEvaluateEnd { get; set; }

    /// <summary><c>experimental_onEvaluationModelCallStart</c>.</summary>
    public Func<EvaluateModelEvent, Task>? OnModelStart { get; set; }

    /// <summary><c>experimental_onEvaluationModelCallEnd</c>.</summary>
    public Func<EvaluateModelEvent, Task>? OnModelEnd { get; set; }

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

/// <summary>Evaluation result.</summary>
public sealed class EvaluateResult
{
    /// <summary>Creates a result.</summary>
    public EvaluateResult(IReadOnlyDictionary<string, EvaluationAnswer> answers, OperationUsage usage, IReadOnlyList<OperationWarning> warnings, EvaluationRounding? rounding, JsonElement? providerMetadata, ProviderResponse response)
    {
        Answers = answers;
        Usage = usage;
        Warnings = warnings ?? Array.Empty<OperationWarning>();
        Rounding = rounding;
        ProviderMetadata = providerMetadata;
        Response = response;
    }

    /// <summary>Answers, with provider numbers unchanged.</summary>
    public IReadOnlyDictionary<string, EvaluationAnswer> Answers { get; }

    /// <summary>Usage. <see cref="OperationUsage.TotalTokens"/> is input plus output when both are set.</summary>
    public OperationUsage Usage { get; }

    /// <summary>Warnings.</summary>
    public IReadOnlyList<OperationWarning> Warnings { get; }

    /// <summary>Rounding declaration.</summary>
    public EvaluationRounding? Rounding { get; }

    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Response. Timestamp and model id are filled when the provider omitted them.</summary>
    public ProviderResponse Response { get; }
}

/// <summary>Options for <see cref="Evaluate.EvaluateAsync"/>.</summary>
public sealed class EvaluateRequest : OperationRequest
{
    /// <summary>Evaluation model.</summary>
    public IEvaluationCaller? Model { get; set; }

    /// <summary>Shared JSON state.</summary>
    public object? State { get; set; }

    /// <summary>Nonempty question map.</summary>
    public IReadOnlyDictionary<string, EvaluationQuestion>? Questions { get; set; }

    /// <summary>Called when evaluation begins. Receives the full runtime context.</summary>
    public Func<EvaluateEvent, Task>? OnStart { get; set; }

    /// <summary>Called when evaluation completes. Receives the full runtime context.</summary>
    public Func<EvaluateEvent, Task>? OnEnd { get; set; }

    /// <summary>Telemetry. Generic operation callbacks on the integration are not called.</summary>
    public EvaluateTelemetry? Telemetry { get; set; }
}

/// <summary>Evaluates typed questions. Maps to <c>evaluate</c>.</summary>
public static class Evaluate
{
    private const double Tolerance = 1e-6;

    /// <summary>Evaluates every question in one model call.</summary>
    public static async Task<EvaluateResult> EvaluateAsync(EvaluateRequest request, CancellationToken cancellationToken = default)
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

        var questions = request.Questions ?? new Dictionary<string, EvaluationQuestion>();
        ValidateInput(request.State, questions);
        foreach (var pair in questions)
        {
            if (!model.SupportedQuestionTypes.Contains(pair.Value.Type))
            {
                throw new EvaluationUnsupportedQuestionTypeException(pair.Key, pair.Value.Type, model.Provider, model.ModelId);
            }
        }

        var token = request.CancellationToken.CanBeCanceled ? request.CancellationToken : cancellationToken;
        var callId = (request.GenerateCallId ?? (() => "call-" + Guid.NewGuid().ToString("N")))();
        var maxRetries = OperationRetry.ResolveMaxRetries(request.MaxRetries);
        var context = request.RuntimeContext ?? new Dictionary<string, object?>();
        var providerOptions = request.ProviderOptions ?? OperationJson.Parse("{}");
        var telemetry = request.Telemetry;
        var enabled = telemetry == null || telemetry.IsEnabled;
        var userStart = new EvaluateEvent(callId, "ai.evaluate", context, model.Provider, model.ModelId, request.State, questions, maxRetries, request.Headers, providerOptions);
        await OperationCallbacks.NotifyAsync(userStart, request.OnStart).ConfigureAwait(false);
        if (enabled)
        {
            await OperationCallbacks.NotifyAsync(WithTelemetry(userStart, Filter(context, telemetry?.IncludeRuntimeContext), telemetry), telemetry?.OnEvaluateStart).ConfigureAwait(false);
        }

        try
        {
            var modelEvent = new EvaluateModelEvent(callId, "ai.evaluate.doEvaluate", model.Provider, model.ModelId, request.State, questions, recordInputs: telemetry?.RecordInputs, recordOutputs: telemetry?.RecordOutputs, functionId: telemetry?.FunctionId);
            if (enabled)
            {
                await OperationCallbacks.NotifyAsync(modelEvent, telemetry?.OnModelStart).ConfigureAwait(false);
            }

            var headers = OperationHeaders.WithUserAgent(request.Headers, AiSdkVersion.UserAgent);
            var result = await OperationRetry.ExecuteAsync(request.MaxRetries, token, request.AbortReason, async ct =>
            {
                OperationRetry.ThrowIfAborted(ct, request.AbortReason);
                return await model.DoEvaluateAsync(new EvaluationModelCall(request.State, questions, providerOptions, headers, ct), ct).ConfigureAwait(false);
            }, null).ConfigureAwait(false);
            OperationRetry.ThrowIfAborted(token, request.AbortReason);
            ValidateAnswers(questions, result.Answers, result.Rounding);
            if (enabled)
            {
                await OperationCallbacks.NotifyAsync(new EvaluateModelEvent(callId, "ai.evaluate.doEvaluate", model.Provider, model.ModelId, request.State, questions, result.Answers, result.Usage, telemetry?.RecordInputs, telemetry?.RecordOutputs, telemetry?.FunctionId), telemetry?.OnModelEnd).ConfigureAwait(false);
            }

            WarningLog.Write(result.Warnings, model.Provider, model.ModelId);
            var input = result.Usage?.InputTokens;
            var output = result.Usage?.OutputTokens;
            var usage = new OperationUsage(input, output, input != null && output != null ? input + output : null);
            var now = request.Now?.Invoke() ?? DateTime.UtcNow;
            var response = new ProviderResponse(result.Response?.Headers, result.Response?.Body, result.Response?.Id, result.Response?.Timestamp ?? now, result.Response?.ModelId ?? model.ModelId, result.Response?.ProviderMetadata);
            var evaluation = new EvaluateResult(result.Answers, usage, result.Warnings, result.Rounding, result.ProviderMetadata, response);
            var end = new EvaluateEvent(callId, "ai.evaluate", context, model.Provider, model.ModelId, request.State, questions, maxRetries, request.Headers, providerOptions, result.Answers, usage, result.Warnings);
            await OperationCallbacks.NotifyAsync(end, request.OnEnd).ConfigureAwait(false);
            if (enabled)
            {
                await OperationCallbacks.NotifyAsync(WithTelemetry(end, Filter(context, telemetry?.IncludeRuntimeContext), telemetry), telemetry?.OnEvaluateEnd).ConfigureAwait(false);
            }

            return evaluation;
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
    public static void ValidateInput(object? state, IReadOnlyDictionary<string, EvaluationQuestion>? questions)
    {
        if (!IsInput(state, new HashSet<object>(ReferenceComparer.Instance)))
        {
            throw new InvalidArgumentException("state", state, "must be a JSON-compatible string, object, or array");
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
    public static void ValidateAnswers(IReadOnlyDictionary<string, EvaluationQuestion> questions, IReadOnlyDictionary<string, EvaluationAnswer>? answers, EvaluationRounding? rounding)
    {
        double Error(int? decimals)
        {
            if (decimals == null)
            {
                return 0;
            }

            if (decimals < 0 || decimals > 15)
            {
                throw new InvalidResponseDataException(answers, "Evaluation rounding decimals must be integers between 0 and 15.");
            }

            return 0.5 * Math.Pow(10, -decimals.Value);
        }

        var probabilityError = Error(rounding?.ProbabilityDecimals);
        var scoreError = Error(rounding?.ScoreDecimals);
        if (answers == null || !SameKeys(answers.Keys, questions.Keys))
        {
            throw new InvalidResponseDataException(answers, "Evaluation must return exactly one answer for every question.");
        }

        foreach (var pair in questions)
        {
            if (!answers.TryGetValue(pair.Key, out var answer) || answer == null || answer.Type != pair.Value.Type)
            {
                throw new InvalidResponseDataException(answers, "Question \"" + pair.Key + "\" returned an answer with the wrong type.");
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

        if (value is DateTime || ReferenceEquals(value, EvaluationValues.Function) || ReferenceEquals(value, EvaluationValues.Undefined) || ReferenceEquals(value, EvaluationValues.SparseArray))
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

    private static EvaluateEvent WithTelemetry(EvaluateEvent source, IReadOnlyDictionary<string, object?> context, EvaluateTelemetry? telemetry)
    {
        return new EvaluateEvent(source.CallId, source.OperationId, context, source.Provider, source.ModelId, source.State, source.Questions, source.MaxRetries, source.Headers, source.ProviderOptions, source.Answers, source.Usage, source.Warnings, telemetry?.RecordInputs, telemetry?.RecordOutputs, telemetry?.FunctionId);
    }
}
