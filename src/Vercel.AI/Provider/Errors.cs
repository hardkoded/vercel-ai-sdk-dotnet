// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Provider;

/// <summary>Base exception for the community .NET port of the Vercel AI SDK.</summary>
public class AiSdkException : Exception
{
    /// <summary>
    /// Marker shared by every AI SDK error. Matches <c>Symbol.for("vercel.ai.error")</c>.
    /// </summary>
    public const string Marker = "vercel.ai.error";

    private readonly Dictionary<string, bool> _markers = new Dictionary<string, bool>();

    /// <summary>Creates an exception with a message.</summary>
    public AiSdkException(string message)
        : base(message)
    {
        Initialize("AISDKError", null);
    }

    /// <summary>Creates an exception with a message and an inner exception.</summary>
    public AiSdkException(string message, Exception innerException)
        : base(message, innerException)
    {
        Initialize("AISDKError", innerException);
    }

    /// <summary>Creates an exception with the upstream error name, message, and cause.</summary>
    public AiSdkException(string name, string message, object? cause)
        : base(message, cause as Exception)
    {
        Initialize(name, cause);
    }

    /// <summary>Upstream <c>error.name</c>, such as <c>AI_APICallError</c>.</summary>
    public string Name { get; private set; } = "AISDKError";

    /// <summary>Underlying cause. When the cause is an exception it is also <see cref="Exception.InnerException"/>.</summary>
    public object? Cause { get; private set; }

    /// <summary>
    /// Returns true when <paramref name="error"/> carries <see cref="Marker"/>.
    /// A dictionary entry whose key is the marker and whose value is <see langword="true"/>
    /// is recognized, which is how another copy of the SDK identifies the same error.
    /// </summary>
    public static bool IsInstance(object? error)
    {
        return HasMarker(error, Marker);
    }

    /// <summary>Records a marker that <see cref="HasMarker(object?, string)"/> can see.</summary>
    protected void SetMarker(string marker)
    {
        _markers[marker] = true;
    }

    /// <summary>Returns true when <paramref name="error"/> carries <paramref name="marker"/> set to true.</summary>
    protected static bool HasMarker(object? error, string marker)
    {
        if (error == null || string.IsNullOrEmpty(marker))
        {
            return false;
        }

        if (error is AiSdkException sdk)
        {
            return sdk._markers.TryGetValue(marker, out var present) && present;
        }

        if (error is IDictionary<string, object> values
            && values.TryGetValue(marker, out var value))
        {
            return value is bool flag && flag;
        }

        if (error is IReadOnlyDictionary<string, object> readOnly
            && readOnly.TryGetValue(marker, out var readOnlyValue))
        {
            return readOnlyValue is bool readOnlyFlag && readOnlyFlag;
        }

        return false;
    }

    private void Initialize(string name, object? cause)
    {
        Name = string.IsNullOrEmpty(name) ? "AISDKError" : name;
        Cause = cause;
        _markers[Marker] = true;
    }
}

/// <summary>Formats unknown failures the way <c>getErrorMessage</c> does.</summary>
public static class ErrorMessages
{
    /// <summary>
    /// Returns a display string for <paramref name="error"/>.
    /// Null and <see cref="JsUndefined"/> become <c>unknown error</c>, strings are returned unchanged,
    /// exceptions use their name and message (or a <see cref="object.ToString"/> override),
    /// and every other value is JSON.
    /// </summary>
    public static string GetErrorMessage(object? error)
    {
        if (error == null || error is JsUndefined)
        {
            return "unknown error";
        }

        if (error is string text)
        {
            return text;
        }

        if (error is Exception exception)
        {
            return FormatException(exception);
        }

        return Stringify(error);
    }

    internal static string Stringify(object? value)
    {
        if (value == null)
        {
            return "null";
        }

        return JsonSerializer.Serialize(value, value.GetType());
    }

    private static string FormatException(Exception exception)
    {
        var toString = exception.GetType().GetMethod("ToString", Type.EmptyTypes);
        if (toString != null && toString.DeclaringType != typeof(Exception))
        {
            return exception.ToString();
        }

        var name = exception is AiSdkException sdk ? sdk.Name : exception.GetType().Name;
        if (string.IsNullOrEmpty(exception.Message))
        {
            return name;
        }

        return name + ": " + exception.Message;
    }
}

/// <summary>The provider returned a non-success HTTP status after retries. Maps to <c>APICallError</c>.</summary>
public class ApiException : AiSdkException
{
    /// <summary>Upstream error name.</summary>
    public const string ErrorName = "AI_APICallError";

    /// <summary>Marker used to recognize this error across assembly copies.</summary>
    public const string TypeMarker = "vercel.ai.error.AI_APICallError";

    /// <summary>Creates an API exception.</summary>
    public ApiException(string message, int statusCode, string? responseBody)
        : this(message, null, null, statusCode, null, responseBody, null, null, null)
    {
    }

    /// <summary>Creates an API call error with the upstream request and response fields.</summary>
    public ApiException(
        string message,
        string? url,
        object? requestBodyValues,
        int? statusCode,
        IReadOnlyDictionary<string, string>? responseHeaders,
        string? responseBody,
        object? cause,
        bool? isRetryable,
        object? data)
        : base(ErrorName, message, cause)
    {
        SetMarker(TypeMarker);
        Url = url;
        RequestBodyValues = requestBodyValues;
        StatusCode = statusCode ?? 0;
        ResponseHeaders = responseHeaders;
        ResponseBody = responseBody;
        Data = data;
        IsRetryable = isRetryable ?? IsRetryableStatusCode(statusCode);
    }

    /// <summary>Request URL, when the call recorded one.</summary>
    public string? Url { get; }

    /// <summary>Request body values sent to the provider, when recorded.</summary>
    public object? RequestBodyValues { get; }

    /// <summary>HTTP status code. Zero when the upstream error did not include one.</summary>
    public int StatusCode { get; }

    /// <summary>Response headers, when the provider sent them.</summary>
    public IReadOnlyDictionary<string, string>? ResponseHeaders { get; }

    /// <summary>Raw response body, when the provider sent one.</summary>
    public string? ResponseBody { get; }

    /// <summary>
    /// Whether the call may succeed if it is tried again.
    /// Defaults to true for 408, 409, 429, and status codes of 500 or higher.
    /// </summary>
    public bool IsRetryable { get; }

    /// <summary>Provider-specific error payload, when one was supplied.</summary>
    public new object? Data { get; }

    /// <summary>Returns true when <paramref name="error"/> carries <see cref="TypeMarker"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, TypeMarker);
    }

    private static bool IsRetryableStatusCode(int? statusCode)
    {
        return statusCode != null
            && (statusCode == 408 || statusCode == 409 || statusCode == 429 || statusCode >= 500);
    }
}

/// <summary>HTTP 400.</summary>
public sealed class BadRequestException : ApiException
{
    /// <summary>Creates the exception.</summary>
    public BadRequestException(string message, string? responseBody)
        : base(message, 400, responseBody)
    {
    }
}

/// <summary>HTTP 401.</summary>
public sealed class AuthenticationException : ApiException
{
    /// <summary>Creates the exception.</summary>
    public AuthenticationException(string message, string? responseBody)
        : base(message, 401, responseBody)
    {
    }
}

/// <summary>HTTP 403.</summary>
public sealed class PermissionDeniedException : ApiException
{
    /// <summary>Creates the exception.</summary>
    public PermissionDeniedException(string message, string? responseBody)
        : base(message, 403, responseBody)
    {
    }
}

/// <summary>HTTP 404.</summary>
public sealed class NotFoundException : ApiException
{
    /// <summary>Creates the exception.</summary>
    public NotFoundException(string message, string? responseBody)
        : base(message, 404, responseBody)
    {
    }
}

/// <summary>HTTP 422.</summary>
public sealed class UnprocessableEntityException : ApiException
{
    /// <summary>Creates the exception.</summary>
    public UnprocessableEntityException(string message, string? responseBody)
        : base(message, 422, responseBody)
    {
    }
}

/// <summary>HTTP 429.</summary>
public sealed class RateLimitException : ApiException
{
    /// <summary>Creates the exception.</summary>
    public RateLimitException(string message, string? responseBody)
        : base(message, 429, responseBody)
    {
    }
}

/// <summary>HTTP 500-599.</summary>
public sealed class InternalServerException : ApiException
{
    /// <summary>Creates the exception.</summary>
    public InternalServerException(string message, int statusCode, string? responseBody)
        : base(message, statusCode, responseBody)
    {
    }
}

/// <summary>The connection to the provider failed.</summary>
public class ApiConnectionException : AiSdkException
{
    /// <summary>Creates the exception.</summary>
    public ApiConnectionException(string message, Exception? innerException = null)
        : base(message, innerException ?? new Exception(message))
    {
    }
}

/// <summary>The provider call timed out.</summary>
public sealed class ApiTimeoutException : ApiConnectionException
{
    /// <summary>Creates the exception.</summary>
    public ApiTimeoutException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

/// <summary>The call was cancelled through a <see cref="CancellationToken"/>.</summary>
public sealed class ApiUserAbortException : AiSdkException
{
    /// <summary>Creates the exception.</summary>
    public ApiUserAbortException()
        : base("The operation was cancelled.")
    {
    }
}

/// <summary>The provider response had no body.</summary>
public sealed class EmptyResponseBodyException : AiSdkException
{
    /// <summary>Upstream error name.</summary>
    public const string ErrorName = "AI_EmptyResponseBodyError";

    /// <summary>Marker used to recognize this error across assembly copies.</summary>
    public const string TypeMarker = "vercel.ai.error.AI_EmptyResponseBodyError";

    /// <summary>Creates the exception.</summary>
    public EmptyResponseBodyException(string? message = null)
        : base(ErrorName, message ?? "Empty response body", null)
    {
        SetMarker(TypeMarker);
    }

    /// <summary>Returns true when <paramref name="error"/> carries <see cref="TypeMarker"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, TypeMarker);
    }
}

/// <summary>An evaluation model does not support the type of a requested question.</summary>
public sealed class EvaluationUnsupportedQuestionTypeException : AiSdkException
{
    /// <summary>Upstream error name.</summary>
    public const string ErrorName = "AI_EvaluationUnsupportedQuestionTypeError";

    /// <summary>Marker used to recognize this error across assembly copies.</summary>
    public const string TypeMarker = "vercel.ai.error.AI_EvaluationUnsupportedQuestionTypeError";

    /// <summary>Creates the exception. A null message uses the question, type, provider, and model.</summary>
    public EvaluationUnsupportedQuestionTypeException(
        string questionId,
        string questionType,
        string provider,
        string modelId,
        string? message = null)
        : base(ErrorName, message ?? DefaultMessage(questionId, questionType, provider, modelId), null)
    {
        SetMarker(TypeMarker);
        QuestionId = questionId;
        QuestionType = questionType;
        Provider = provider;
        ModelId = modelId;
    }

    /// <summary>Question that the model cannot answer.</summary>
    public string QuestionId { get; }

    /// <summary>Question type that is not supported.</summary>
    public string QuestionType { get; }

    /// <summary>Provider id.</summary>
    public string Provider { get; }

    /// <summary>Model id.</summary>
    public string ModelId { get; }

    /// <summary>Returns true when <paramref name="error"/> carries <see cref="TypeMarker"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, TypeMarker);
    }

    private static string DefaultMessage(string questionId, string questionType, string provider, string modelId)
    {
        return "Question \"" + questionId + "\" has type \"" + questionType
            + "\", which is not supported by provider \"" + provider + "\" and model \"" + modelId + "\".";
    }
}

/// <summary>
/// A function argument is invalid. Maps to the provider-package <c>InvalidArgumentError</c>.
/// The AI-layer error with parameter and value lives in <c>Vercel.AI.Error</c>.
/// </summary>
public sealed class InvalidArgumentException : AiSdkException
{
    /// <summary>Upstream error name.</summary>
    public const string ErrorName = "AI_InvalidArgumentError";

    /// <summary>Marker used to recognize this error across assembly copies.</summary>
    public const string TypeMarker = "vercel.ai.error.AI_InvalidArgumentError";

    /// <summary>Creates the exception.</summary>
    public InvalidArgumentException(string argument, string message, object? cause = null)
        : base(ErrorName, message, cause)
    {
        SetMarker(TypeMarker);
        Argument = argument;
    }

    /// <summary>Argument that failed validation.</summary>
    public string Argument { get; }

    /// <summary>Returns true when <paramref name="error"/> carries <see cref="TypeMarker"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, TypeMarker);
    }
}

/// <summary>A prompt could not be processed by the provider.</summary>
public sealed class InvalidPromptException : AiSdkException
{
    /// <summary>Upstream error name.</summary>
    public const string ErrorName = "AI_InvalidPromptError";

    /// <summary>Marker used to recognize this error across assembly copies.</summary>
    public const string TypeMarker = "vercel.ai.error.AI_InvalidPromptError";

    /// <summary>Creates the exception.</summary>
    public InvalidPromptException(object? prompt, string message, object? cause = null)
        : base(ErrorName, "Invalid prompt: " + message, cause)
    {
        SetMarker(TypeMarker);
        Prompt = prompt;
    }

    /// <summary>Prompt that the provider rejected.</summary>
    public object? Prompt { get; }

    /// <summary>Returns true when <paramref name="error"/> carries <see cref="TypeMarker"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, TypeMarker);
    }
}

/// <summary>The provider response could not be parsed.</summary>
public sealed class InvalidResponseDataException : AiSdkException
{
    /// <summary>Upstream error name.</summary>
    public const string ErrorName = "AI_InvalidResponseDataError";

    /// <summary>Marker used to recognize this error across assembly copies.</summary>
    public const string TypeMarker = "vercel.ai.error.AI_InvalidResponseDataError";

    /// <summary>Creates the exception. A null message includes the JSON form of <paramref name="data"/>.</summary>
    public InvalidResponseDataException(object? data, string? message = null)
        : base(ErrorName, message ?? ("Invalid response data: " + ErrorMessages.Stringify(data) + "."), null)
    {
        SetMarker(TypeMarker);
        Data = data;
    }

    /// <summary>Response value that failed to parse.</summary>
    public new object? Data { get; }

    /// <summary>Returns true when <paramref name="error"/> carries <see cref="TypeMarker"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, TypeMarker);
    }
}

/// <summary>JSON text could not be parsed.</summary>
public sealed class JsonParseException : AiSdkException
{
    /// <summary>Upstream error name.</summary>
    public const string ErrorName = "AI_JSONParseError";

    /// <summary>Marker used to recognize this error across assembly copies.</summary>
    public const string TypeMarker = "vercel.ai.error.AI_JSONParseError";

    /// <summary>Creates the exception.</summary>
    public JsonParseException(string text, object? cause)
        : base(
            ErrorName,
            "JSON parsing failed: Text: " + text + ".\nError message: " + ErrorMessages.GetErrorMessage(cause),
            cause)
    {
        SetMarker(TypeMarker);
        Text = text;
    }

    /// <summary>Text that failed to parse.</summary>
    public string Text { get; }

    /// <summary>Returns true when <paramref name="error"/> carries <see cref="TypeMarker"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, TypeMarker);
    }
}

/// <summary>An API key could not be loaded.</summary>
public sealed class LoadApiKeyException : AiSdkException
{
    /// <summary>Upstream error name.</summary>
    public const string ErrorName = "AI_LoadAPIKeyError";

    /// <summary>Marker used to recognize this error across assembly copies.</summary>
    public const string TypeMarker = "vercel.ai.error.AI_LoadAPIKeyError";

    /// <summary>Creates the exception.</summary>
    public LoadApiKeyException(string message)
        : base(ErrorName, message, null)
    {
        SetMarker(TypeMarker);
    }

    /// <summary>Returns true when <paramref name="error"/> carries <see cref="TypeMarker"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, TypeMarker);
    }
}

/// <summary>A provider setting could not be loaded.</summary>
public sealed class LoadSettingException : AiSdkException
{
    /// <summary>Upstream error name.</summary>
    public const string ErrorName = "AI_LoadSettingError";

    /// <summary>Marker used to recognize this error across assembly copies.</summary>
    public const string TypeMarker = "vercel.ai.error.AI_LoadSettingError";

    /// <summary>Creates the exception.</summary>
    public LoadSettingException(string message)
        : base(ErrorName, message, null)
    {
        SetMarker(TypeMarker);
    }

    /// <summary>Returns true when <paramref name="error"/> carries <see cref="TypeMarker"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, TypeMarker);
    }
}

/// <summary>The provider generated no content.</summary>
public sealed class NoContentGeneratedException : AiSdkException
{
    /// <summary>Upstream error name.</summary>
    public const string ErrorName = "AI_NoContentGeneratedError";

    /// <summary>Marker used to recognize this error across assembly copies.</summary>
    public const string TypeMarker = "vercel.ai.error.AI_NoContentGeneratedError";

    /// <summary>Creates the exception.</summary>
    public NoContentGeneratedException(string? message = null)
        : base(ErrorName, message ?? "No content generated.", null)
    {
        SetMarker(TypeMarker);
    }

    /// <summary>Returns true when <paramref name="error"/> carries <see cref="TypeMarker"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, TypeMarker);
    }
}

/// <summary>The requested model id is not available for a modality.</summary>
public sealed class NoSuchModelException : AiSdkException
{
    /// <summary>Upstream error name.</summary>
    public const string ErrorName = "AI_NoSuchModelError";

    /// <summary>Marker used to recognize this error across assembly copies.</summary>
    public const string TypeMarker = "vercel.ai.error.AI_NoSuchModelError";

    /// <summary>
    /// Creates the exception.
    /// <paramref name="modelType"/> is a modality name such as <c>languageModel</c>, <c>embeddingModel</c>,
    /// <c>imageModel</c>, <c>transcriptionModel</c>, <c>speechModel</c>, <c>rerankingModel</c>,
    /// <c>videoModel</c>, or <c>evaluationModel</c>.
    /// <paramref name="errorName"/> replaces the upstream name when a caller needs a more specific one.
    /// </summary>
    public NoSuchModelException(string modelId, string modelType, string? message = null, string? errorName = null)
        : base(errorName ?? ErrorName, message ?? ("No such " + modelType + ": " + modelId), null)
    {
        SetMarker(TypeMarker);
        ModelId = modelId;
        ModelType = modelType;
    }

    /// <summary>Model id that was requested.</summary>
    public string ModelId { get; }

    /// <summary>Modality that was requested.</summary>
    public string ModelType { get; }

    /// <summary>Returns true when <paramref name="error"/> carries <see cref="TypeMarker"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, TypeMarker);
    }
}

/// <summary>A provider id is missing from a provider-reference map.</summary>
public sealed class NoSuchProviderReferenceException : AiSdkException
{
    /// <summary>Upstream error name.</summary>
    public const string ErrorName = "AI_NoSuchProviderReferenceError";

    /// <summary>Marker used to recognize this error across assembly copies.</summary>
    public const string TypeMarker = "vercel.ai.error.AI_NoSuchProviderReferenceError";

    /// <summary>Creates the exception.</summary>
    public NoSuchProviderReferenceException(
        string provider,
        IReadOnlyDictionary<string, string> reference,
        string? message = null)
        : base(ErrorName, message ?? DefaultMessage(provider, reference), null)
    {
        SetMarker(TypeMarker);
        Provider = provider;
        Reference = reference;
    }

    /// <summary>Provider id that was not in the map.</summary>
    public string Provider { get; }

    /// <summary>Provider ids mapped to that provider's file identifier.</summary>
    public IReadOnlyDictionary<string, string> Reference { get; }

    /// <summary>Returns true when <paramref name="error"/> carries <see cref="TypeMarker"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, TypeMarker);
    }

    private static string DefaultMessage(string provider, IReadOnlyDictionary<string, string> reference)
    {
        return "No provider reference found for provider '" + provider + "'. Available providers: " + string.Join(", ", reference.Keys);
    }
}

/// <summary>An embedding call received more values than the model allows.</summary>
public sealed class TooManyEmbeddingValuesForCallException : AiSdkException
{
    /// <summary>Upstream error name.</summary>
    public const string ErrorName = "AI_TooManyEmbeddingValuesForCallError";

    /// <summary>Marker used to recognize this error across assembly copies.</summary>
    public const string TypeMarker = "vercel.ai.error.AI_TooManyEmbeddingValuesForCallError";

    /// <summary>Creates the exception.</summary>
    public TooManyEmbeddingValuesForCallException(
        string provider,
        string modelId,
        int maxEmbeddingsPerCall,
        IReadOnlyList<object> values)
        : base(ErrorName, DefaultMessage(provider, modelId, maxEmbeddingsPerCall, values), null)
    {
        SetMarker(TypeMarker);
        Provider = provider;
        ModelId = modelId;
        MaxEmbeddingsPerCall = maxEmbeddingsPerCall;
        Values = values;
    }

    /// <summary>Provider id.</summary>
    public string Provider { get; }

    /// <summary>Model id.</summary>
    public string ModelId { get; }

    /// <summary>Maximum number of values accepted in one call.</summary>
    public int MaxEmbeddingsPerCall { get; }

    /// <summary>Values that were submitted.</summary>
    public IReadOnlyList<object> Values { get; }

    /// <summary>Returns true when <paramref name="error"/> carries <see cref="TypeMarker"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, TypeMarker);
    }

    private static string DefaultMessage(string provider, string modelId, int maxEmbeddingsPerCall, IReadOnlyList<object> values)
    {
        return "Too many values for a single embedding call. "
            + "The " + provider + " model \"" + modelId + "\" can only embed up to "
            + maxEmbeddingsPerCall + " values per call, but " + values.Count + " values were provided.";
    }
}

/// <summary>Where a value failed type validation.</summary>
public sealed class TypeValidationContext
{
    /// <summary>Creates validation context. Omitted parts stay null.</summary>
    public TypeValidationContext(string? field = null, string? entityName = null, string? entityId = null)
    {
        Field = field;
        EntityName = entityName;
        EntityId = entityId;
    }

    /// <summary>Field path, such as <c>message.metadata</c> or <c>message.parts[3].data</c>.</summary>
    public string? Field { get; }

    /// <summary>Entity name, such as a tool name or data type name.</summary>
    public string? EntityName { get; }

    /// <summary>Entity id, such as a message id or tool call id.</summary>
    public string? EntityId { get; }
}

/// <summary>A value did not match the type expected by the SDK.</summary>
public sealed class TypeValidationException : AiSdkException
{
    /// <summary>Upstream error name.</summary>
    public const string ErrorName = "AI_TypeValidationError";

    /// <summary>Marker used to recognize this error across assembly copies.</summary>
    public const string TypeMarker = "vercel.ai.error.AI_TypeValidationError";

    /// <summary>Creates the exception.</summary>
    public TypeValidationException(object? value, object? cause, TypeValidationContext? context = null)
        : base(ErrorName, BuildMessage(value, cause, context), cause)
    {
        SetMarker(TypeMarker);
        Value = value;
        Context = context;
    }

    /// <summary>Value that failed validation.</summary>
    public object? Value { get; }

    /// <summary>Where validation failed, when the caller supplied a location.</summary>
    public TypeValidationContext? Context { get; }

    /// <summary>Returns true when <paramref name="error"/> carries <see cref="TypeMarker"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, TypeMarker);
    }

    /// <summary>
    /// Returns <paramref name="cause"/> when it is already a <see cref="TypeValidationException"/>
    /// for the same value and context. Otherwise creates a new exception.
    /// </summary>
    public static TypeValidationException Wrap(object? value, object? cause, TypeValidationContext? context = null)
    {
        if (cause is TypeValidationException existing
            && SameValue(existing.Value, value)
            && SameContext(existing.Context, context))
        {
            return existing;
        }

        return new TypeValidationException(value, cause, context);
    }

    private static string BuildMessage(object? value, object? cause, TypeValidationContext? context)
    {
        var contextPrefix = "Type validation failed";
        if (context != null && !string.IsNullOrEmpty(context.Field))
        {
            contextPrefix += " for " + context.Field;
        }

        if (context != null && (context.EntityName != null || context.EntityId != null))
        {
            var parts = new List<string>();
            if (context.EntityName != null)
            {
                parts.Add(context.EntityName);
            }

            if (context.EntityId != null)
            {
                parts.Add("id: \"" + context.EntityId + "\"");
            }

            contextPrefix += " (" + string.Join(", ", parts) + ")";
        }

        return contextPrefix + ": Value: " + ErrorMessages.Stringify(value) + ".\nError message: " + ErrorMessages.GetErrorMessage(cause);
    }

    private static bool SameContext(TypeValidationContext? left, TypeValidationContext? right)
    {
        return SameText(left?.Field, right?.Field)
            && SameText(left?.EntityName, right?.EntityName)
            && SameText(left?.EntityId, right?.EntityId);
    }

    private static bool SameText(string? left, string? right)
    {
        return string.Equals(left, right, StringComparison.Ordinal);
    }

    private static bool SameValue(object? left, object? right)
    {
        if (left == null || right == null)
        {
            return left == null && right == null;
        }

        if (left is string || right is string || left.GetType().IsValueType || right.GetType().IsValueType)
        {
            return left.Equals(right);
        }

        return ReferenceEquals(left, right);
    }
}

/// <summary>The provider does not support a requested capability.</summary>
public sealed class UnsupportedFunctionalityException : AiSdkException
{
    /// <summary>Upstream error name.</summary>
    public const string ErrorName = "AI_UnsupportedFunctionalityError";

    /// <summary>Marker used to recognize this error across assembly copies.</summary>
    public const string TypeMarker = "vercel.ai.error.AI_UnsupportedFunctionalityError";

    /// <summary>Creates the exception.</summary>
    public UnsupportedFunctionalityException(string functionality, string? message = null)
        : base(ErrorName, message ?? ("'" + functionality + "' functionality not supported."), null)
    {
        SetMarker(TypeMarker);
        Functionality = functionality;
    }

    /// <summary>Capability that is not supported.</summary>
    public string Functionality { get; }

    /// <summary>Returns true when <paramref name="error"/> carries <see cref="TypeMarker"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, TypeMarker);
    }
}
