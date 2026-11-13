// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.ScaleDown;

/// <summary>Provider metadata of a ScaleDown decision, under <c>providerMetadata.scaledown</c>.</summary>
public sealed class ScaleDownDecisionProviderMetadata
{
    /// <summary>Confidence per question id. Boolean questions have none.</summary>
    public IReadOnlyDictionary<string, double> Confidence { get; init; } = new Dictionary<string, double>();

    /// <summary>Score legend per question id.</summary>
    public IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Legend { get; init; } = new Dictionary<string, IReadOnlyDictionary<string, string>>();

    /// <summary>Total request cost in USD, when supplied by ScaleDown.</summary>
    public double? Cost { get; init; }
}

/// <summary>Settings for a <see cref="DecisionScaleDownModel"/>.</summary>
public class ScaleDownDecisionModelConfig
{
    /// <summary>Provider id, such as <c>scaledown.decision</c>.</summary>
    public string Provider { get; set; } = ScaleDownProvider.DecisionProviderId;

    /// <summary>API base URL, without a trailing slash.</summary>
    public string BaseUrl { get; set; } = ScaleDownProvider.DefaultBaseUrl;

    /// <summary>
    /// Provider headers, resolved on every call. When null, the model sends the <c>x-api-key</c> read from
    /// <c>SCALEDOWN_API_KEY</c> at call time.
    /// </summary>
    public Func<IReadOnlyDictionary<string, string>>? Headers { get; set; }

    /// <summary>HTTP client. Not serialized.</summary>
    public HttpClient? HttpClient { get; set; }
}

/// <summary>Answers questions through the ScaleDown <c>/v1/scaledown</c> route.</summary>
public sealed class DecisionScaleDownModel : IDecisionCaller
{
    private static readonly HttpClient SharedClient = new();

    private static readonly JsonSerializerOptions Compact = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    private readonly ScaleDownDecisionModelConfig _config;

    /// <summary>Creates a decision model. <paramref name="modelId"/> may be <c>classify</c>, <c>classify-1</c>, or an id this library does not know yet.</summary>
    public DecisionScaleDownModel(string modelId, ScaleDownDecisionModelConfig? config = null)
    {
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _config = config ?? new ScaleDownDecisionModelConfig();
    }

    /// <inheritdoc />
    public string Provider => _config.Provider;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public IReadOnlyList<string> SupportedQuestionTypes { get; } = ["choice", "boolean", "score"];

    /// <summary>Serializes the model id and the JSON-serializable part of the config for a workflow boundary. Maps to <c>WORKFLOW_SERIALIZE</c>.</summary>
    public SerializedModelOptions Serialize()
    {
        var config = new Dictionary<string, object?>
        {
            ["provider"] = _config.Provider,
            ["baseURL"] = _config.BaseUrl,
        };
        if (_config.Headers is { } headers)
        {
            config["headers"] = new Func<Dictionary<string, string>>(() => headers().ToDictionary(pair => pair.Key, pair => pair.Value));
        }

        return ModelOptionsSerialization.SerializeModelOptions(ModelId, config);
    }

    /// <summary>
    /// Restores a model from <see cref="Serialize"/>. When no headers were serialized, authentication is read from
    /// <c>SCALEDOWN_API_KEY</c> at call time. Maps to <c>WORKFLOW_DESERIALIZE</c>.
    /// </summary>
    public static DecisionScaleDownModel Deserialize(SerializedModelOptions options, HttpClient? httpClient = null)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var config = new ScaleDownDecisionModelConfig { HttpClient = httpClient };
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

        return new DecisionScaleDownModel(options.ModelId, config);
    }

    /// <inheritdoc />
    public async Task<DecisionModelResult> DoDecideAsync(DecisionModelCall call, CancellationToken cancellationToken)
    {
        if (call == null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        var nativeQuestions = new JsonObject();
        foreach (var pair in call.Questions)
        {
            nativeQuestions[pair.Key] = NativeQuestion(pair.Key, pair.Value);
        }

        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["state"] = ConvertToScaleDownState.Convert(call.State),
            ["questions"] = nativeQuestions,
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
            headers["x-api-key"] = ApiKeys.Require(null, ScaleDownProvider.ApiKeyEnvironmentVariable);
            headers["User-Agent"] = ProviderExchange.UserAgent("scaledown");
        }

        foreach (var pair in call.Headers)
        {
            headers[pair.Key] = pair.Value;
        }

        var uri = ApiKeys.Combine(_config.BaseUrl, "/v1/scaledown");
        using var request = new HttpRequestMessage(HttpMethod.Post, uri) { Content = ProviderExchange.Json(body.ToJsonString()) };
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
            var text = await CancellableReads.ReadAsStringAsync(response.Content, cancellationToken).ConfigureAwait(false);
            var responseHeaders = CopyHeaders(response);
            var status = (int)response.StatusCode;
            if (!response.IsSuccessStatusCode)
            {
                throw ScaleDownError.FailedResponse(uri.ToString(), body, status, response.ReasonPhrase, responseHeaders, text);
            }

            return ParseResponse(uri.ToString(), body, status, responseHeaders, text, call);
        }
    }

    private static string ToText(DecisionInput value)
    {
        return value.Value.ValueKind == JsonValueKind.String ? value.Value.GetString()! : JsonSerializer.Serialize(value.Value, Compact);
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

    private JsonObject NativeQuestion(string id, DecisionQuestion question)
    {
        var instructions = question.Instructions == null ? null : ToText(question.Instructions);
        switch (question.Type)
        {
            case "choice":
                var criteria = new JsonObject();
                if (question.Criteria is DecisionChoiceCriteria choices)
                {
                    foreach (var option in choices.Options)
                    {
                        criteria[option.Key] = option.Value == null ? option.Key : ToText(option.Value);
                    }
                }

                var choice = new JsonObject { ["type"] = "choice" };
                if (instructions != null)
                {
                    choice["instructions"] = instructions;
                }

                choice["criteria"] = criteria;
                return choice;
            case "boolean":
                var lines = new List<string>();
                if (instructions != null)
                {
                    lines.Add(instructions);
                }

                // The native endpoint ignores noul criteria, so include them in its instructions to preserve their meaning.
                if (question.Criteria is DecisionBooleanCriteria truth)
                {
                    if (truth.True != null)
                    {
                        lines.Add("Answer yes when: " + ToText(truth.True));
                    }

                    if (truth.False != null)
                    {
                        lines.Add("Answer no when: " + ToText(truth.False));
                    }
                }

                return new JsonObject { ["type"] = "noul", ["instructions"] = string.Join("\n", lines) };
            case "score":
                var levels = (question.Criteria as DecisionScoreCriteria)?.Levels ?? [];
                if (levels.Count < 2 || levels.Count > 10 || levels.Any(level => level == null))
                {
                    throw new InvalidArgumentError("questions." + id + ".criteria", question.Criteria, "ScaleDown Score questions require 2 to 10 non-null levels.");
                }

                var score = new JsonObject { ["type"] = "score" };
                if (instructions != null)
                {
                    score["instructions"] = instructions;
                }

                score["criteria"] = new JsonArray(levels.Select(level => (JsonNode?)ToText(level!)).ToArray());
                return score;
            default:
                throw new DecisionUnsupportedQuestionTypeError(id, question.Type, Provider, ModelId);
        }
    }

    private DecisionModelResult ParseResponse(string url, JsonObject requestBody, int status, IReadOnlyDictionary<string, string> headers, string text, DecisionModelCall call)
    {
        JsonElement root;
        ScaleDownDecisionResponse value;
        try
        {
            using var document = JsonDocument.Parse(text);
            root = document.RootElement.Clone();
            value = ScaleDownDecisionApi.Parse(root);
        }
        catch (Exception exception) when (exception is JsonException || exception is FormatException || exception is InvalidOperationException)
        {
            throw new ApiCallError("Invalid JSON response", url, requestBody, status, headers, text, exception);
        }

        var confidence = new Dictionary<string, double>();
        var legend = new Dictionary<string, IReadOnlyDictionary<string, string>>();
        var answers = new Dictionary<string, DecisionAnswer>();
        foreach (var pair in value.Answers)
        {
            var answer = pair.Value;
            if (answer.Type != "noul" && answer.Confidence != null)
            {
                confidence[pair.Key] = answer.Confidence.Value;
            }

            if (answer.Type == "score" && answer.Legend != null)
            {
                legend[pair.Key] = answer.Legend;
            }

            answers[pair.Key] = answer.Type switch
            {
                "noul" => new DecisionAnswer("boolean", probability: answer.Noul),
                "choice" => new DecisionAnswer("choice", choice: answer.Choice, probabilities: answer.Probabilities),
                _ => new DecisionAnswer("score", score: answer.Score, probabilities: answer.Probabilities),
            };
        }

        var metadata = new ScaleDownDecisionProviderMetadata { Confidence = confidence, Legend = legend, Cost = value.Cost };
        var metadataNode = new JsonObject
        {
            ["confidence"] = JsonSerializer.SerializeToNode(metadata.Confidence),
            ["legend"] = JsonSerializer.SerializeToNode(metadata.Legend),
        };
        if (metadata.Cost != null)
        {
            metadataNode["cost"] = metadata.Cost;
        }

        var warnings = new List<OperationWarning>();
        if (call.MaxOutputTokens != null)
        {
            warnings.Add(new OperationWarning("unsupported", feature: "maxOutputTokens"));
        }

        if (call.Reasoning != null)
        {
            warnings.Add(new OperationWarning("unsupported", feature: "reasoning"));
        }

        if (call.ProviderOptions.ValueKind == JsonValueKind.Object
            && call.ProviderOptions.TryGetProperty("scaledown", out var options)
            && options.ValueKind == JsonValueKind.Object)
        {
            foreach (var option in options.EnumerateObject())
            {
                warnings.Add(new OperationWarning("unsupported", feature: "providerOptions.scaledown." + option.Name));
            }
        }

        return new DecisionModelResult(
            answers,
            warnings: warnings,
            usage: new OperationUsage(value.InputTokens, value.OutputTokens),
            providerMetadata: JsonSerializer.SerializeToElement(new JsonObject { ["scaledown"] = metadataNode }),
            response: new ProviderResponse(headers, root, modelId: value.Model ?? ModelId));
    }
}
