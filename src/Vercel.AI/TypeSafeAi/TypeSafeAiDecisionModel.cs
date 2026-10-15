// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Collections;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.TypeSafeAi;

/// <summary>Settings for a <see cref="TypeSafeAiDecisionModel"/>.</summary>
public class TypeSafeAiDecisionModelConfig
{
    /// <summary>Provider id, such as <c>typesafe.decision</c>.</summary>
    public string Provider { get; set; } = TypeSafeAiProvider.DecisionProviderId;

    /// <summary>API base URL, without a trailing slash.</summary>
    public string BaseUrl { get; set; } = TypeSafeAiProvider.DefaultBaseUrl;

    /// <summary>
    /// Provider headers, resolved on every call. When null, the model sends a bearer token read from
    /// <c>TYPESAFE_AI_API_KEY</c> at call time.
    /// </summary>
    public Func<IReadOnlyDictionary<string, string>>? Headers { get; set; }

    /// <summary>HTTP client. Not serialized.</summary>
    public HttpClient? HttpClient { get; set; }
}

/// <summary>Answers questions through the TypeSafe <c>/systemone</c> route.</summary>
#pragma warning disable CS0618
public class TypeSafeAiDecisionModel : IEvaluationDecisionCaller
#pragma warning restore CS0618
{
    private static readonly HttpClient SharedClient = new HttpClient();

    private readonly TypeSafeAiDecisionModelConfig _config;

    /// <summary>Creates a decision model. <paramref name="modelId"/> may be a model id this library does not know yet.</summary>
    public TypeSafeAiDecisionModel(string modelId, TypeSafeAiDecisionModelConfig? config = null)
    {
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _config = config ?? new TypeSafeAiDecisionModelConfig();
    }

    /// <inheritdoc />
    public string Provider => _config.Provider;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public IReadOnlyList<string> SupportedQuestionTypes { get; } = new[] { "choice", "score", "boolean" };

    /// <summary>Serializes the model id and the JSON-serializable part of the config for a workflow boundary.</summary>
    public SerializedModelOptions Serialize()
    {
        var config = new Dictionary<string, object?>
        {
            ["provider"] = _config.Provider,
            ["baseURL"] = _config.BaseUrl,
        };
        if (_config.Headers is { } headers)
        {
            config["headers"] = new Func<Dictionary<string, string>>(() =>
            {
                var copy = new Dictionary<string, string>();
                foreach (var pair in headers())
                {
                    copy[pair.Key] = pair.Value;
                }

                return copy;
            });
        }

        return ModelOptionsSerialization.SerializeModelOptions(ModelId, config);
    }

    /// <summary>
    /// Restores a model from <see cref="Serialize"/>. When no headers were serialized, authentication is read from
    /// <c>TYPESAFE_AI_API_KEY</c> at call time. The HTTP client is not serialized, so pass one to use a custom handler.
    /// </summary>
    public static TypeSafeAiDecisionModel Deserialize(SerializedModelOptions options, HttpClient? httpClient = null)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return new TypeSafeAiDecisionModel(options.ModelId, DeserializeConfig(options, httpClient));
    }

    /// <summary>Reads the config written by <see cref="Serialize"/>.</summary>
    internal static TypeSafeAiDecisionModelConfig DeserializeConfig(SerializedModelOptions options, HttpClient? httpClient)
    {
        var config = new TypeSafeAiDecisionModelConfig { HttpClient = httpClient };
        if (options.Config.TryGetValue("provider", out var provider) && provider is string providerId)
        {
            config.Provider = providerId;
        }

        if (options.Config.TryGetValue("baseURL", out var baseUrl) && baseUrl is string url)
        {
            config.BaseUrl = url;
        }

        if (options.Config.TryGetValue("headers", out var headers) && headers is IDictionary<string, object?> values)
        {
            var copy = values.Where(pair => pair.Value is string).ToDictionary(pair => pair.Key, pair => (string)pair.Value!);
            config.Headers = () => copy;
        }

        return config;
    }

    /// <summary>Deprecated. Use <see cref="DoDecideAsync(DecisionModelCall, CancellationToken)"/>.</summary>
    [Obsolete("Use DoDecideAsync instead.")]
    public async Task<EvaluationModelResult> DoEvaluateAsync(EvaluationModelCall call, CancellationToken cancellationToken)
    {
        return EvaluationModelResult.From(await DoDecideAsync(call, cancellationToken).ConfigureAwait(false));
    }

    /// <inheritdoc />
    public async Task<DecisionModelResult> DoDecideAsync(DecisionModelCall call, CancellationToken cancellationToken)
    {
        if (call == null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        foreach (var pair in call.Questions)
        {
            var count = CriteriaCount(pair.Value.Criteria);
            if (pair.Value.Type == "choice" && count > 255)
            {
                throw new InvalidArgumentError("questions." + pair.Key + ".criteria", pair.Value.Criteria, "TypeSafe Choice questions support at most 255 options.");
            }

            if (pair.Value.Type == "score" && count > 10)
            {
                throw new InvalidArgumentError("questions." + pair.Key + ".criteria", pair.Value.Criteria, "TypeSafe Score questions support at most 10 levels.");
            }
        }

        var warnings = new List<OperationWarning>();
        if (call.ProviderOptions.ValueKind == JsonValueKind.Object
            && call.ProviderOptions.TryGetProperty("typesafe", out var options)
            && options.ValueKind == JsonValueKind.Object)
        {
            foreach (var option in options.EnumerateObject())
            {
                warnings.Add(new OperationWarning("unsupported", feature: "providerOptions.typesafe." + option.Name));
            }
        }

        var questions = new JsonObject();
        foreach (var pair in call.Questions)
        {
            var question = new JsonObject
            {
                ["type"] = pair.Value.Type == "boolean" ? "noul" : pair.Value.Type,
                ["instructions"] = ToNode(pair.Value.Instructions),
            };
            if (pair.Value.Criteria != null)
            {
                question["criteria"] = ToNode(pair.Value.Criteria);
            }

            questions[pair.Key] = question;
        }

        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["state"] = ToNode(LegacyState(call.State)),
            ["questions"] = questions,
        };

        var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (_config.Headers is { } providerHeaders)
        {
            foreach (var pair in providerHeaders())
            {
                headers[pair.Key] = pair.Value;
            }
        }
        else
        {
            headers["Authorization"] = "Bearer " + ApiKeys.Require(null, TypeSafeAiProvider.ApiKeyEnvironmentVariable);
            headers["User-Agent"] = ProviderExchange.UserAgent("typesafe-ai");
        }

        foreach (var pair in call.Headers)
        {
            headers[pair.Key] = pair.Value;
        }

        var uri = ApiKeys.Combine(_config.BaseUrl, "/systemone");
        var requestBody = body.ToJsonString();
        using var request = new HttpRequestMessage(HttpMethod.Post, uri) { Content = ProviderExchange.Json(requestBody) };
        foreach (var pair in headers)
        {
            if (!string.IsNullOrEmpty(pair.Value) && !pair.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
            {
                request.Headers.TryAddWithoutValidation(pair.Key, pair.Value);
            }
        }

        HttpResponseMessage response;
        try
        {
            response = await (_config.HttpClient ?? SharedClient).SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException exception)
        {
            throw new ApiCallError("Cannot connect to API: " + exception.Message, uri.ToString(), body, cause: exception, isRetryable: true);
        }

        using (response)
        {
            var text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var responseHeaders = CopyHeaders(response);
            var status = (int)response.StatusCode;
            if (!response.IsSuccessStatusCode)
            {
                throw FailedResponse(uri.ToString(), body, status, response.ReasonPhrase, responseHeaders, text);
            }

            return ParseResponse(uri.ToString(), body, status, responseHeaders, text, warnings);
        }
    }

    private DecisionModelResult ParseResponse(string url, JsonObject requestBody, int status, IReadOnlyDictionary<string, string> headers, string text, IReadOnlyList<OperationWarning> warnings)
    {
        try
        {
            using var document = JsonDocument.Parse(text);
            var root = document.RootElement.Clone();
            if (root.ValueKind != JsonValueKind.Object)
            {
                throw new FormatException("Expected an object.");
            }

            var answers = new Dictionary<string, DecisionAnswer>();
            var confidence = new JsonObject();
            foreach (var pair in Property(root, "answers", JsonValueKind.Object).EnumerateObject())
            {
                var answer = pair.Value;
                if (answer.ValueKind != JsonValueKind.Object)
                {
                    throw new FormatException("Invalid answer.");
                }

                switch (Property(answer, "type", JsonValueKind.String).GetString())
                {
                    case "choice":
                        answers[pair.Name] = new DecisionAnswer(
                            "choice",
                            choice: Property(answer, "choice", JsonValueKind.String).GetString(),
                            probabilities: Probabilities(answer));
                        AddConfidence(confidence, pair.Name, answer);
                        break;
                    case "score":
                        answers[pair.Name] = new DecisionAnswer(
                            "score",
                            score: Property(answer, "score", JsonValueKind.Number).GetDouble(),
                            probabilities: Probabilities(answer));
                        AddConfidence(confidence, pair.Name, answer);
                        break;
                    case "noul":
                        answers[pair.Name] = new DecisionAnswer("boolean", probability: Property(answer, "noul", JsonValueKind.Number).GetDouble());
                        break;
                    default:
                        throw new FormatException("Unknown answer type.");
                }
            }

            string? model = null;
            if (root.TryGetProperty("model", out var modelElement) && modelElement.ValueKind != JsonValueKind.Null)
            {
                model = Property(root, "model", JsonValueKind.String).GetString();
            }

            int? inputTokens = null;
            int? outputTokens = null;
            if (root.TryGetProperty("usage", out var usage) && usage.ValueKind != JsonValueKind.Null)
            {
                if (usage.ValueKind != JsonValueKind.Object)
                {
                    throw new FormatException("Invalid usage.");
                }

                inputTokens = OptionalInteger(usage, "input_tokens");
                outputTokens = OptionalInteger(usage, "output_tokens");
            }

            var metadata = JsonDocument.Parse(new JsonObject { ["typesafe"] = new JsonObject { ["confidence"] = confidence } }.ToJsonString()).RootElement.Clone();
            return new DecisionModelResult(
                answers,
                warnings: warnings,
                usage: new OperationUsage(inputTokens, outputTokens),
                rounding: new DecisionRounding(2, 2),
                providerMetadata: metadata,
                response: new ProviderResponse(headers, root, modelId: model ?? ModelId));
        }
        catch (Exception exception) when (exception is JsonException || exception is FormatException || exception is InvalidOperationException)
        {
            throw new ApiCallError("Invalid JSON response", url, requestBody, status, headers, text, exception);
        }
    }

    private static ApiCallError FailedResponse(string url, JsonObject requestBody, int status, string? statusText, IReadOnlyDictionary<string, string> headers, string text)
    {
        var message = statusText ?? string.Empty;
        object? data = null;
        try
        {
            using var document = JsonDocument.Parse(text);
            var root = document.RootElement;
            if (root.ValueKind == JsonValueKind.Object && IsErrorEnvelope(root))
            {
                message = ErrorMessage(root);
                data = root.Clone();
            }
        }
        catch (JsonException)
        {
        }

        return new ApiCallError(message, url, requestBody, status, headers, text, isRetryable: status == 408 || status == 409 || status == 429 || status >= 500, data: data);
    }

    /// <summary>Mirrors the upstream error schema: every field is optional, but a present field must have its declared type.</summary>
    private static bool IsErrorEnvelope(JsonElement root)
    {
        return Nullish(root, "message", JsonValueKind.String)
            && Nullish(root, "error_type", JsonValueKind.String)
            && (!root.TryGetProperty("error", out var error)
                || error.ValueKind == JsonValueKind.Null
                || error.ValueKind == JsonValueKind.String
                || (error.ValueKind == JsonValueKind.Object && Nullish(error, "message", JsonValueKind.String)));
    }

    private static bool Nullish(JsonElement element, string name, JsonValueKind kind)
    {
        return !element.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null || value.ValueKind == kind;
    }

    private static string ErrorMessage(JsonElement root)
    {
        if (Text(root, "message") is { } message)
        {
            return message;
        }

        if (root.TryGetProperty("error", out var error))
        {
            if (error.ValueKind == JsonValueKind.String)
            {
                return error.GetString()!;
            }

            if (error.ValueKind == JsonValueKind.Object && Text(error, "message") is { } nested)
            {
                return nested;
            }
        }

        if (root.TryGetProperty("detail", out var detail))
        {
            return detail.ValueKind == JsonValueKind.String ? detail.GetString()! : detail.GetRawText();
        }

        return Text(root, "error_type") ?? "TypeSafe request failed";
    }

    private static string? Text(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    }

    private static JsonElement Property(JsonElement element, string name, JsonValueKind kind)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind != kind)
        {
            throw new FormatException("Invalid " + name + ".");
        }

        return value;
    }

    private static int? OptionalInteger(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (value.ValueKind != JsonValueKind.Number)
        {
            throw new FormatException("Invalid " + name + ".");
        }

        return (int)value.GetDouble();
    }

    private static IReadOnlyDictionary<string, double> Probabilities(JsonElement answer)
    {
        var values = new Dictionary<string, double>();
        foreach (var item in Property(answer, "probabilities", JsonValueKind.Object).EnumerateObject())
        {
            if (item.Value.ValueKind != JsonValueKind.Number)
            {
                throw new FormatException("Invalid probability.");
            }

            values[item.Name] = item.Value.GetDouble();
        }

        return values;
    }

    private static void AddConfidence(JsonObject confidence, string id, JsonElement answer)
    {
        if (answer.TryGetProperty("confidence", out var value) && value.ValueKind != JsonValueKind.Null)
        {
            confidence[id] = Property(answer, "confidence", JsonValueKind.Number).GetDouble();
        }
    }

    private static IReadOnlyDictionary<string, string> CopyHeaders(HttpResponseMessage response)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in response.Headers)
        {
            headers[header.Key] = string.Join(",", header.Value);
        }

        foreach (var header in response.Content.Headers)
        {
            headers[header.Key] = string.Join(",", header.Value);
        }

        return headers;
    }

    private static int CriteriaCount(object? criteria)
    {
        return criteria switch
        {
            JsonElement { ValueKind: JsonValueKind.Object } element => element.EnumerateObject().Count(),
            JsonElement { ValueKind: JsonValueKind.Array } element => element.GetArrayLength(),
            ICollection collection => collection.Count,
            _ => 0,
        };
    }

    /// <summary>TypeSafe takes the state as a string, a JSON object, or a list. A single text or JSON part is sent as its value.</summary>
    private static object? LegacyState(object? state)
    {
        var values = new List<object?>();
        foreach (var item in (IEnumerable<object?>)state!)
        {
            var part = (IDictionary<string, object?>)item!;
            switch ((string)part["type"]!)
            {
                case "file":
                    throw new UnsupportedFunctionalityException("TypeSafe evaluation file input", "'TypeSafe evaluation file input' functionality not supported.");
                case "text":
                    values.Add(part["text"]);
                    break;
                default:
                    values.Add(part["value"]);
                    break;
            }
        }

        return values.Count == 1 && (values[0] is string || values[0] is IDictionary<string, object?> || values[0] is IList<object?>) ? values[0] : values;
    }

    private static JsonNode? ToNode(object? value)
    {
        return value == null ? null : JsonNode.Parse(JsonSerializer.Serialize(value));
    }
}

/// <summary>Deprecated alias of <see cref="TypeSafeAiDecisionModelConfig"/>.</summary>
[Obsolete("Use TypeSafeAiDecisionModelConfig instead.")]
public sealed class TypeSafeAiEvaluationModelConfig : TypeSafeAiDecisionModelConfig
{
}

/// <summary>Deprecated alias of <see cref="TypeSafeAiDecisionModel"/>.</summary>
[Obsolete("Use TypeSafeAiDecisionModel instead.")]
public sealed class TypeSafeAiEvaluationModel : TypeSafeAiDecisionModel
{
    /// <summary>Creates a decision model. <paramref name="modelId"/> may be a model id this library does not know yet.</summary>
    public TypeSafeAiEvaluationModel(string modelId, TypeSafeAiDecisionModelConfig? config = null)
        : base(modelId, config)
    {
    }

    /// <summary>Deprecated alias of <see cref="TypeSafeAiDecisionModel.Deserialize"/>.</summary>
    public static new TypeSafeAiEvaluationModel Deserialize(SerializedModelOptions options, HttpClient? httpClient = null)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return new TypeSafeAiEvaluationModel(options.ModelId, DeserializeConfig(options, httpClient));
    }
}
