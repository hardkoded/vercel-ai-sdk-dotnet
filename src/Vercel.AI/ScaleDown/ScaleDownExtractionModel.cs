// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.ScaleDown;

/// <summary>Settings for a <see cref="ScaleDownExtractionModel"/>. Maps to <c>ScaleDownExtractionModelConfig</c>.</summary>
public class ScaleDownExtractionModelConfig
{
    /// <summary>Provider id, such as <c>scaledown.extraction</c>.</summary>
    public string Provider { get; set; } = ScaleDownProvider.ExtractionProviderId;

    /// <summary>API base URL, without a trailing slash.</summary>
    public string BaseUrl { get; set; } = ScaleDownProvider.DefaultBaseUrl;

    /// <summary>
    /// Provider headers, resolved on every call. When null, the model sends the <c>x-api-key</c> read from
    /// <c>SCALEDOWN_API_KEY</c> at call time.
    /// </summary>
    public Func<IReadOnlyDictionary<string, string>>? Headers { get; set; }

    /// <summary>
    /// Provider headers that need an asynchronous step, such as an async API key. Used when <see cref="Headers"/> is null.
    /// Not serializable: <c>Serialize</c> throws <see cref="SerializationError"/>, as upstream does for an async <c>headers</c>.
    /// </summary>
    public Func<CancellationToken, Task<IReadOnlyDictionary<string, string>>>? HeadersAsync { get; set; }

    /// <summary>HTTP client. Not serialized.</summary>
    public HttpClient? HttpClient { get; set; }
}

/// <summary>Extracts text fields and lists through the ScaleDown <c>/v1/chat/completions</c> route. Maps to <c>ScaleDownExtractionModel</c>.</summary>
public sealed class ScaleDownExtractionModel : IExtractionCaller
{
    private static readonly HttpClient SharedClient = new();

    private readonly ScaleDownExtractionModelConfig _config;

    /// <summary>Creates an extraction model. <paramref name="modelId"/> may be <c>extract</c>, <c>extract-1</c>, or an id this library does not know yet.</summary>
    public ScaleDownExtractionModel(string modelId, ScaleDownExtractionModelConfig? config = null)
    {
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _config = config ?? new ScaleDownExtractionModelConfig();
    }

    /// <inheritdoc />
    public string Provider => _config.Provider;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public IReadOnlyList<string> SupportedFieldTypes { get; } = ["text", "list"];

    /// <summary>Serializes the model id and the JSON-serializable part of the config for a workflow boundary. Maps to <c>WORKFLOW_SERIALIZE</c>.</summary>
    public SerializedModelOptions Serialize()
    {
        var config = new Dictionary<string, object?>
        {
            ["provider"] = _config.Provider,
            ["baseURL"] = _config.BaseUrl,
        };
        if (_config.Headers is null && _config.HeadersAsync is not null)
        {
            throw new SerializationError("Cannot serialize asynchronous model options.");
        }

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
    public static ScaleDownExtractionModel Deserialize(SerializedModelOptions options, HttpClient? httpClient = null)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var config = new ScaleDownExtractionModelConfig { HttpClient = httpClient };
        if (options.Config.TryGetValue("provider", out var provider) && ReadString(provider) is { } providerId)
        {
            config.Provider = providerId;
        }

        if (options.Config.TryGetValue("baseURL", out var baseUrl) && ReadString(baseUrl) is { } url)
        {
            config.BaseUrl = url;
        }

        if (options.Config.TryGetValue("headers", out var headers) && ReadHeaders(headers) is { } copy)
        {
            config.Headers = () => copy;
        }

        return new ScaleDownExtractionModel(options.ModelId, config);
    }

    /// <inheritdoc />
    public async Task<ExtractionModelResult> DoExtractAsync(ExtractionModelCall call, CancellationToken cancellationToken)
    {
        if (call == null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        var properties = new JsonObject();
        foreach (var pair in call.Fields)
        {
            var field = pair.Value;
            if (field.Type != "text" && !(field.Type == "list" && field.Items == "text"))
            {
                throw new ExtractionUnsupportedFieldTypeError(pair.Key, field.Type == "list" ? "list<" + field.Items + ">" : field.Type, Provider, ModelId);
            }

            var property = new JsonObject { ["type"] = "string" };
            if (field.Instructions != null)
            {
                property["description"] = DecisionScaleDownModel.ToText(field.Instructions);
            }

            // Scalar schemas retain entity confidence and offsets for every match.
            property["top_n"] = field.Type == "text" ? 1 : 0;
            properties[pair.Key] = property;
        }

        var threshold = ParseThreshold(call.ProviderOptions);
        var state = ConvertToScaleDownState.Convert(call.State);
        var inputText = state["text"]?.GetValue<string>() ?? string.Empty;
        // ScaleDown requires a non-empty user message even with a document.
        if (state["document"] != null && inputText.Trim() == string.Empty)
        {
            inputText = "Extract the requested fields from the document.";
        }

        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["messages"] = new JsonArray(new JsonObject { ["role"] = "user", ["content"] = inputText }),
            ["response_format"] = new JsonObject
            {
                ["type"] = "json_schema",
                ["json_schema"] = new JsonObject
                {
                    ["name"] = "extraction",
                    ["schema"] = new JsonObject { ["type"] = "object", ["properties"] = properties },
                },
            },
        };
        if (threshold != null)
        {
            body["threshold"] = threshold;
        }

        body["context_chars"] = 0;
        if (state["document"] != null)
        {
            body["document"] = state["document"]!.GetValue<string>();
            body["document_mime_type"] = state["document_mime_type"]!.GetValue<string>();
        }

        var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (_config.Headers is { } providerHeaders)
        {
            foreach (var pair in providerHeaders())
            {
                headers[pair.Key] = pair.Value;
            }
        }
        else if (_config.HeadersAsync is { } providerHeadersAsync)
        {
            foreach (var pair in await providerHeadersAsync(cancellationToken).ConfigureAwait(false))
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

        var uri = ApiKeys.Combine(_config.BaseUrl, "/v1/chat/completions");
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

    // A System.Text.Json round trip turns the serialized values into JsonElement.
    private static string? ReadString(object? value)
    {
        return value switch
        {
            string text => text,
            JsonElement { ValueKind: JsonValueKind.String } element => element.GetString(),
            _ => null,
        };
    }

    private static Dictionary<string, string>? ReadHeaders(object? value)
    {
        if (value is JsonElement { ValueKind: JsonValueKind.Object } json)
        {
            return json.EnumerateObject()
                .Where(property => property.Value.ValueKind == JsonValueKind.String)
                .ToDictionary(property => property.Name, property => property.Value.GetString()!);
        }

        if (value is IDictionary<string, object?> values)
        {
            return values.Where(pair => pair.Value is string).ToDictionary(pair => pair.Key, pair => (string)pair.Value!);
        }

        return null;
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

    // Maps to scaledownExtractionModelOptionsSchema: { threshold?: number between 0 and 1 }.
    private static double? ParseThreshold(JsonElement providerOptions)
    {
        if (providerOptions.ValueKind != JsonValueKind.Object
            || !providerOptions.TryGetProperty("scaledown", out var options)
            || options.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (options.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidArgumentException("providerOptions", options.GetRawText(), "Invalid scaledown provider options: expected an object.");
        }

        if (!options.TryGetProperty("threshold", out var threshold) || threshold.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (threshold.ValueKind != JsonValueKind.Number || threshold.GetDouble() < 0 || threshold.GetDouble() > 1)
        {
            throw new InvalidArgumentException("providerOptions", options.GetRawText(), "Invalid scaledown provider options: threshold must be a number from 0 to 1.");
        }

        return threshold.GetDouble();
    }

    private ExtractionModelResult ParseResponse(string url, JsonObject requestBody, int status, IReadOnlyDictionary<string, string> headers, string text, ExtractionModelCall call)
    {
        JsonElement root;
        ScaleDownExtractionResponse value;
        try
        {
            using var document = JsonDocument.Parse(text);
            root = document.RootElement.Clone();
            value = ScaleDownExtractionApi.ParseResponse(root);
        }
        catch (Exception exception) when (exception is JsonException || exception is FormatException || exception is InvalidOperationException)
        {
            throw new ApiCallError("Invalid JSON response", url, requestBody, status, headers, text, exception);
        }

        var content = ScaleDownExtractionApi.ParseContent(value.Content)
            ?? throw new InvalidResponseDataException(value.Content, "ScaleDown did not return valid extraction content.");

        var answers = new Dictionary<string, ExtractionAnswer>();
        foreach (var pair in call.Fields)
        {
            var matches = content.Entities.Where(entity => entity.Type == pair.Key && entity.Text != string.Empty).ToList();
            if (pair.Value.Type == "list")
            {
                if (matches.Count == 0)
                {
                    answers[pair.Key] = new ExtractionAnswer("list");
                    continue;
                }

                answers[pair.Key] = new ExtractionAnswer(
                    "list",
                    textItems: matches.Select(entity => entity.Text).ToList(),
                    items: matches.Select(entity => new ExtractionItem(entity.Confidence, ConvertScaleDownExtractionSource.Convert(entity, call.State, content.OcrText))).ToList());
                continue;
            }

            ScaleDownEntity? match = null;
            foreach (var entity in matches)
            {
                if (match == null || entity.Confidence > match.Confidence)
                {
                    match = entity;
                }
            }

            answers[pair.Key] = new ExtractionAnswer(
                "text",
                text: match?.Text,
                confidence: match?.Confidence,
                source: match == null ? null : ConvertScaleDownExtractionSource.Convert(match, call.State, content.OcrText));
        }

        var warnings = new List<OperationWarning>();
        if (call.ReferenceDate != null)
        {
            warnings.Add(new OperationWarning("unsupported", feature: "referenceDate"));
        }

        if (call.MaxOutputTokens != null)
        {
            warnings.Add(new OperationWarning("unsupported", feature: "maxOutputTokens"));
        }

        if (call.Reasoning != null)
        {
            warnings.Add(new OperationWarning("unsupported", feature: "reasoning"));
        }

        JsonElement? providerMetadata = content.OcrText == null
            ? null
            : JsonSerializer.SerializeToElement(new JsonObject { ["scaledown"] = new JsonObject { ["ocrText"] = content.OcrText } });
        // An out-of-range value leaves the timestamp unset, as an invalid Date does upstream.
        DateTime? timestamp = value.Created is { } created && Math.Abs(created) < 253402300799
            ? new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(created * 1000)
            : null;
        return new ExtractionModelResult(
            answers,
            warnings: warnings,
            usage: new OperationUsage(value.PromptTokens ?? (content.InputTokens), value.CompletionTokens),
            providerMetadata: providerMetadata,
            response: new ProviderResponse(headers, root, id: value.Id, timestamp: timestamp, modelId: value.Model ?? ModelId));
    }
}
