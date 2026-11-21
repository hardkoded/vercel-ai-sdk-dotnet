// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Parallel;

/// <summary>Settings for a <see cref="DecisionParallelModel"/>. Maps to <c>ParallelDecisionModelConfig</c>.</summary>
public class ParallelDecisionModelConfig
{
    /// <summary>Provider id, such as <c>parallel.decision</c>.</summary>
    public string Provider { get; set; } = ParallelProvider.DecisionProviderId;

    /// <summary>API base URL, without a trailing slash.</summary>
    public string BaseUrl { get; set; } = ParallelProvider.DefaultBaseUrl;

    /// <summary>
    /// Provider headers, resolved on every call. When null, the model sends the <c>x-api-key</c> read from
    /// <c>PARALLEL_API_KEY</c> at call time.
    /// </summary>
    public Func<IReadOnlyDictionary<string, string>>? Headers { get; set; }

    /// <summary>
    /// Provider headers that need an asynchronous step, such as an async API key. Used when <see cref="Headers"/> is null.
    /// Not serializable: <c>Serialize</c> throws <see cref="SerializationError"/>, as upstream does for an async <c>headers</c>.
    /// </summary>
    public Func<
        CancellationToken,
        Task<IReadOnlyDictionary<string, string>>
    >? HeadersAsync { get; set; }

    /// <summary>HTTP client. Not serialized.</summary>
    public HttpClient? HttpClient { get; set; }
}

/// <summary>Answers questions through the Parallel <c>/decisions</c> route. Maps to <c>DecisionParallelModel</c>.</summary>
public sealed class DecisionParallelModel : IDecisionCaller
{
    private static readonly HttpClient SharedClient = new();

    private static readonly Regex QuestionName = new(
        @"^[A-Za-z0-9_-]+\z",
        RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1)
    );

    private readonly ParallelDecisionModelConfig _config;

    /// <summary>Creates a decision model. <paramref name="modelId"/> may be <c>fast</c> or a mode this library does not know yet.</summary>
    public DecisionParallelModel(string modelId, ParallelDecisionModelConfig? config = null)
    {
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _config = config ?? new ParallelDecisionModelConfig();
    }

    /// <inheritdoc />
    public string Provider => _config.Provider;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public IReadOnlyList<string> SupportedQuestionTypes { get; } =
        ParallelModelCatalog.SupportedQuestionTypes;

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
            config["headers"] = new Func<Dictionary<string, string>>(() =>
                headers().ToDictionary(pair => pair.Key, pair => pair.Value)
            );
        }

        return ModelOptionsSerialization.SerializeModelOptions(ModelId, config);
    }

    /// <summary>
    /// Restores a model from <see cref="Serialize"/>. When no headers were serialized, authentication is read from
    /// <c>PARALLEL_API_KEY</c> at call time. Maps to <c>WORKFLOW_DESERIALIZE</c>.
    /// </summary>
    public static DecisionParallelModel Deserialize(
        SerializedModelOptions options,
        HttpClient? httpClient = null
    )
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var config = new ParallelDecisionModelConfig { HttpClient = httpClient };
        if (
            options.Config.TryGetValue("provider", out var provider)
            && provider is string providerId
        )
        {
            config.Provider = providerId;
        }

        if (options.Config.TryGetValue("baseURL", out var baseUrl) && baseUrl is string url)
        {
            config.BaseUrl = url;
        }

        if (
            options.Config.TryGetValue("headers", out var headers)
            && headers is IDictionary<string, object?> values
        )
        {
            var copy = values
                .Where(pair => pair.Value is string)
                .ToDictionary(pair => pair.Key, pair => (string)pair.Value!);
            config.Headers = () => copy;
        }

        return new DecisionParallelModel(options.ModelId, config);
    }

    /// <inheritdoc />
    public async Task<DecisionModelResult> DoDecideAsync(
        DecisionModelCall call,
        CancellationToken cancellationToken
    )
    {
        if (call == null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        ValidateQuestions(call.Questions);
        var includeExcerpts = ParseProviderOptions(call.ProviderOptions);

        var body = new JsonObject { ["mode"] = ModelId };
        if (includeExcerpts != null)
        {
            body["include_excerpts"] = includeExcerpts;
        }

        body["state"] = ToParallelState(call.State);
        var questions = new JsonObject();
        foreach (var pair in call.Questions)
        {
            var question = new JsonObject
            {
                ["type"] = pair.Value.Type == "boolean" ? "noul" : pair.Value.Type,
            };
            if (pair.Value.HasInstructions)
            {
                question["instructions"] = pair.Value.Instructions?.ToNode();
            }

            if (pair.Value.Criteria != null)
            {
                question["criteria"] = pair.Value.Criteria.ToNode();
            }

            questions[pair.Key] = question;
        }

        body["questions"] = questions;

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
            foreach (
                var pair in await providerHeadersAsync(cancellationToken).ConfigureAwait(false)
            )
            {
                headers[pair.Key] = pair.Value;
            }
        }
        else
        {
            headers["x-api-key"] = ApiKeys.Require(
                null,
                ParallelProvider.ApiKeyEnvironmentVariable
            );
            headers["User-Agent"] = ProviderExchange.UserAgent("parallel");
        }

        foreach (var pair in call.Headers)
        {
            headers[pair.Key] = pair.Value;
        }

        var uri = ApiKeys.Combine(_config.BaseUrl, "/decisions");
        using var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = ProviderExchange.Json(body.ToJsonString()),
        };
        foreach (var pair in headers)
        {
            if (
                !string.IsNullOrEmpty(pair.Value)
                && !pair.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)
            )
            {
                request.Headers.TryAddWithoutValidation(pair.Key, pair.Value);
            }
        }

        HttpResponseMessage response;
        try
        {
            response = await (_config.HttpClient ?? SharedClient)
                .SendAsync(request, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException exception)
        {
            throw new ApiCallError(
                "Cannot connect to API: " + exception.Message,
                uri.ToString(),
                body,
                cause: exception,
                isRetryable: true
            );
        }

        using (response)
        {
            var text = await CancellableReads
                .ReadAsStringAsync(response.Content, cancellationToken)
                .ConfigureAwait(false);
            var responseHeaders = CopyHeaders(response);
            var status = (int)response.StatusCode;
            if (!response.IsSuccessStatusCode)
            {
                throw ParallelError.FailedResponse(
                    uri.ToString(),
                    body,
                    status,
                    response.ReasonPhrase,
                    responseHeaders,
                    text
                );
            }

            return ParseResponse(
                uri.ToString(),
                body,
                status,
                responseHeaders,
                text,
                call,
                call.Questions.Keys
            );
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

    // Maps to toParallelState: a single part is sent as its value, several parts as an array.
    private static JsonNode? ToParallelState(IReadOnlyList<DecisionModelStatePart> state)
    {
        var values = new List<JsonNode?>();
        foreach (var part in state)
        {
            switch (part)
            {
                case DecisionModelFilePart:
                    throw new UnsupportedFunctionalityException(
                        "Parallel Decisions file input",
                        "'Parallel Decisions file input' functionality not supported."
                    );
                case DecisionModelTextPart text:
                    values.Add(JsonValue.Create(text.Text));
                    break;
                default:
                    values.Add(JsonNode.Parse(((DecisionModelJsonPart)part).Value.GetRawText()));
                    break;
            }
        }

        return values.Count == 1 ? values[0] : new JsonArray(values.ToArray());
    }

    // Maps to the request validation at the top of doDecide.
    private void ValidateQuestions(IReadOnlyDictionary<string, DecisionQuestion> questions)
    {
        if (questions.Count == 0 || questions.Count > 100)
        {
            throw new InvalidArgumentError(
                "questions",
                null,
                "Parallel Decisions supports 1 to 100 questions."
            );
        }

        foreach (var pair in questions)
        {
            var id = pair.Key;
            var question = pair.Value;
            if (!QuestionName.IsMatch(id))
            {
                throw new InvalidArgumentError(
                    "questions",
                    null,
                    "Parallel question names may contain only ASCII letters, numbers, underscores, and hyphens."
                );
            }

            if (question.Type == "box")
            {
                throw new DecisionUnsupportedQuestionTypeError(
                    id,
                    question.Type,
                    Provider,
                    ModelId
                );
            }

            if (question.Type is "choice" or "score" && question.Criteria is { } criteria)
            {
                var count = criteria.Count;
                var max = question.Type == "choice" ? 255 : 10;
                if (count < 2 || count > max)
                {
                    throw new InvalidArgumentError(
                        "questions." + id + ".criteria",
                        null,
                        question.Type == "choice"
                            ? "Parallel Choice questions require 2 to 255 options."
                            : "Parallel Score questions require 2 to 10 levels."
                    );
                }
            }
        }
    }

    // Maps to parallelDecisionModelOptions: { includeExcerpts?: boolean }.
    private static bool? ParseProviderOptions(JsonElement providerOptions)
    {
        if (
            providerOptions.ValueKind != JsonValueKind.Object
            || !providerOptions.TryGetProperty("parallel", out var options)
            || options.ValueKind == JsonValueKind.Null
        )
        {
            return null;
        }

        if (options.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidArgumentError(
                "providerOptions",
                options.GetRawText(),
                "Invalid parallel provider options: expected an object."
            );
        }

        if (
            !options.TryGetProperty("includeExcerpts", out var include)
            || include.ValueKind == JsonValueKind.Null
        )
        {
            return null;
        }

        if (include.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
        {
            throw new InvalidArgumentError(
                "providerOptions",
                options.GetRawText(),
                "Invalid parallel provider options: includeExcerpts must be a boolean."
            );
        }

        return include.GetBoolean();
    }

    private DecisionModelResult ParseResponse(
        string url,
        JsonObject requestBody,
        int status,
        IReadOnlyDictionary<string, string> headers,
        string text,
        DecisionModelCall call,
        IEnumerable<string> questionIds
    )
    {
        JsonElement root;
        ParallelDecisionResponse value;
        try
        {
            using var document = JsonDocument.Parse(text);
            root = document.RootElement.Clone();
            value = ParallelDecisionApi.Parse(root);
        }
        catch (Exception exception)
            when (exception is JsonException
                || exception is FormatException
                || exception is InvalidOperationException
            )
        {
            throw new ApiCallError(
                "Invalid JSON response",
                url,
                requestBody,
                status,
                headers,
                text,
                exception
            );
        }

        var ids = questionIds.ToList();
        if (value.Answers.Count != ids.Count || ids.Any(id => !value.Answers.ContainsKey(id)))
        {
            throw new InvalidResponseDataException(
                root,
                "Parallel Decisions must return exactly one answer for every question."
            );
        }

        var confidence = new JsonObject();
        var legend = new JsonObject();
        var answers = new Dictionary<string, DecisionAnswer>();
        foreach (var pair in value.Answers)
        {
            var answer = pair.Value;
            if (answer.Type is "choice" or "score" && answer.Confidence != null)
            {
                confidence[pair.Key] = answer.Confidence.Value;
            }

            if (answer.Type == "score")
            {
                legend[pair.Key] = JsonSerializer.SerializeToNode(answer.Legend);
            }

            answers[pair.Key] = answer.Type switch
            {
                "noul" => new DecisionAnswer("boolean", probability: answer.Noul),
                "choice" => new DecisionAnswer(
                    "choice",
                    choice: answer.Choice,
                    probabilities: answer.Probabilities
                ),
                "score" => new DecisionAnswer(
                    "score",
                    score: answer.Score,
                    probabilities: answer.Probabilities
                ),
                _ => new DecisionAnswer("refusal"),
            };
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

        if (
            call.ProviderOptions.ValueKind == JsonValueKind.Object
            && call.ProviderOptions.TryGetProperty("parallel", out var options)
            && options.ValueKind == JsonValueKind.Object
        )
        {
            foreach (var option in options.EnumerateObject())
            {
                if (option.Name != "includeExcerpts")
                {
                    warnings.Add(
                        new OperationWarning(
                            "unsupported",
                            feature: "providerOptions.parallel." + option.Name
                        )
                    );
                }
            }
        }

        var metadataNode = new JsonObject
        {
            ["citations"] = JsonNode.Parse(value.Citations.GetRawText()),
            ["confidence"] = confidence,
            ["decisionId"] = value.DecisionId,
            ["legend"] = legend,
            ["mode"] = value.Mode,
        };
        return new DecisionModelResult(
            answers,
            warnings: warnings,
            usage: new OperationUsage(value.InputTokens, value.OutputTokens),
            providerMetadata: JsonSerializer.SerializeToElement(
                new JsonObject { ["parallel"] = metadataNode }
            ),
            response: new ProviderResponse(headers, root, id: value.DecisionId, modelId: ModelId)
        );
    }
}
