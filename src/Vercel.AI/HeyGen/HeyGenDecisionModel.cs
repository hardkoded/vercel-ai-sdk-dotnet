// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.HeyGen;

/// <summary>
/// Answers questions through the HeyGen Decision API (<c>POST {base}/v3/models/decisions</c>). Boolean questions are sent as
/// native <c>noul</c> questions. Image and video file parts are sent as HeyGen assets. Maps to <c>DecisionHeyGenModel</c>.
/// </summary>
public sealed class DecisionHeyGenModel : IDecisionCaller
{
    private readonly HeyGenProvider _provider;

    /// <summary>Creates a decision model. <paramref name="modelId"/> may be a model id this library does not know yet.</summary>
    public DecisionHeyGenModel(HeyGenProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string Provider => "heygen.decision";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public IReadOnlyList<string> SupportedQuestionTypes =>
        HeyGenModelCatalog.DecisionSupportedQuestionTypes;

    /// <summary>
    /// Serializes the model id, the endpoint, and the resolved request headers (including authentication) for a workflow boundary.
    /// Maps to <c>[WORKFLOW_SERIALIZE]</c>. The HTTP client is not serialized.
    /// </summary>
    public SerializedModelOptions Serialize()
    {
        var headers = new Dictionary<string, string>();
        foreach (var pair in _provider.CreateHeaders())
        {
            if (pair.Value != null)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        var config = new Dictionary<string, object?>
        {
            ["provider"] = Provider,
            ["baseURL"] = _provider.Options.BaseUrl,
            ["headers"] = headers,
        };
        return ModelOptionsSerialization.SerializeModelOptions(ModelId, config);
    }

    /// <summary>
    /// Restores a model from <see cref="Serialize"/>. Maps to <c>[WORKFLOW_DESERIALIZE]</c>. Pass <paramref name="httpClient"/> to
    /// send the requests through a custom handler.
    /// </summary>
    public static DecisionHeyGenModel Deserialize(
        SerializedModelOptions options,
        HttpClient? httpClient = null
    )
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var providerOptions = new OpenAICompatibleOptions();
        if (options.Config.TryGetValue("baseURL", out var baseUrl) && AsString(baseUrl) is { } url)
        {
            providerOptions.BaseUrl = url;
        }

        if (options.Config.TryGetValue("headers", out var headers))
        {
            switch (headers)
            {
                case JsonElement { ValueKind: JsonValueKind.Object } element:
                    foreach (var property in element.EnumerateObject())
                    {
                        if (property.Value.ValueKind == JsonValueKind.String)
                        {
                            Restore(providerOptions, property.Name, property.Value.GetString()!);
                        }
                    }

                    break;
                case IDictionary<string, object?> values:
                    foreach (var pair in values)
                    {
                        if (AsString(pair.Value) is { } value)
                        {
                            Restore(providerOptions, pair.Key, value);
                        }
                    }

                    break;
            }
        }

        return new HeyGenProvider(httpClient ?? new HttpClient(), providerOptions).DecisionModel(
            options.ModelId
        );
    }

    /// <summary>Sends every question in one Decision request.</summary>
    public async Task<DecisionModelResult> DoDecideAsync(
        DecisionModelCall call,
        CancellationToken cancellationToken = default
    )
    {
        if (call == null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        if (
            call.Questions.Count == 0
            || call.Questions.Count > HeyGenModelCatalog.DecisionMaxQuestions
        )
        {
            throw new InvalidArgumentException(
                "questions",
                call.Questions,
                "HeyGen Decision supports 1 to "
                    + HeyGenModelCatalog.DecisionMaxQuestions
                    + " questions per request."
            );
        }

        var nativeQuestions = new JsonObject();
        foreach (var pair in call.Questions)
        {
            nativeQuestions[pair.Key] = BuildQuestion(pair.Key, pair.Value);
        }

        var (nativeState, assets) = ConvertToHeyGenDecisionInput(call.State);
        var body = new JsonObject { ["model"] = ModelId, ["state"] = nativeState };
        if (assets != null)
        {
            body["assets"] = assets;
        }

        body["questions"] = nativeQuestions;

        ProviderExchangeResult response;
        try
        {
            response = await ProviderExchange
                .SendAsync(
                    _provider._httpClient,
                    HttpMethod.Post,
                    ApiKeys.Combine(_provider.Options.BaseUrl, "v3/models/decisions"),
                    ProviderExchange.Json(body.ToJsonString()),
                    ProviderExchange.Merge(_provider.CreateHeaders(), call.Headers),
                    cancellationToken
                )
                .ConfigureAwait(false);
        }
        catch (ApiException exception)
            when (HeyGenError.ErrorMessage(exception.ResponseBody) is { } message)
        {
            throw ProviderHttp.MapStatus(exception.StatusCode, message, exception.ResponseBody);
        }

        JsonElement root;
        try
        {
            using var document = JsonDocument.Parse(
                string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body
            );
            root = document.RootElement.Clone();
        }
        catch (JsonException exception)
        {
            throw new ApiException("Invalid JSON response", 200, response.Body, exception);
        }

        var data = ParseResponse(root, response.Body);
        if (
            data.Answers.Count != call.Questions.Count
            || call.Questions.Keys.Any(id => !data.Answers.Any(answer => answer.Name == id))
        )
        {
            throw new InvalidResponseDataException(
                root,
                "HeyGen Decision must return exactly one answer for every question."
            );
        }

        var answers = new Dictionary<string, DecisionAnswer>();
        var confidence = new JsonObject();
        var legend = new JsonObject();
        var answerWarnings = new JsonObject();
        foreach (var answer in data.Answers)
        {
            answers[answer.Name] = answer.Type switch
            {
                "noul" => new DecisionAnswer("boolean", probability: answer.Probability),
                "choice" => new DecisionAnswer(
                    "choice",
                    choice: answer.Choice,
                    probabilities: answer.Probabilities
                ),
                _ => new DecisionAnswer(
                    "score",
                    score: answer.Score,
                    probabilities: answer.Probabilities
                ),
            };
            if (answer.Type != "noul" && answer.Confidence != null)
            {
                confidence[answer.Name] = answer.Confidence.Value;
            }

            if (answer.Type == "score" && answer.Legend != null)
            {
                legend[answer.Name] = JsonNode.Parse(answer.Legend.Value.GetRawText());
            }

            if (answer.Warning != null)
            {
                answerWarnings[answer.Name] = answer.Warning;
            }
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
            && call.ProviderOptions.TryGetProperty(HeyGenProvider.ProviderId, out var options)
            && options.ValueKind == JsonValueKind.Object
        )
        {
            foreach (var option in options.EnumerateObject())
            {
                warnings.Add(
                    new OperationWarning(
                        "unsupported",
                        feature: "providerOptions.heygen." + option.Name
                    )
                );
            }
        }

        var heygen = new JsonObject();
        if (data.RequestId != null)
        {
            heygen["requestId"] = data.RequestId;
        }

        heygen["confidence"] = confidence;
        heygen["legend"] = legend;
        heygen["answerWarnings"] = answerWarnings;
        var metadata = JsonSerializer.SerializeToElement(
            new JsonObject { [HeyGenProvider.ProviderId] = heygen }
        );

        // HeyGen returns full-precision probabilities, so no rounding is declared.
        return new DecisionModelResult(
            answers,
            warnings,
            data.Usage,
            null,
            metadata,
            new ProviderResponse(
                response.Headers,
                root,
                id: data.Id,
                modelId: data.Model ?? ModelId
            )
        );
    }

    private static void Restore(OpenAICompatibleOptions options, string name, string value)
    {
        if (name.Equals("x-api-key", StringComparison.OrdinalIgnoreCase))
        {
            options.ApiKey = value;
        }
        else
        {
            options.Headers[name] = value;
        }
    }

    private static string? AsString(object? value)
    {
        return value switch
        {
            string text => text,
            JsonElement { ValueKind: JsonValueKind.String } element => element.GetString(),
            _ => null,
        };
    }

    // Maps to convertToHeyGenDecisionInput.
    private static (JsonNode? State, JsonObject? Assets) ConvertToHeyGenDecisionInput(
        IReadOnlyList<DecisionModelStatePart> state
    )
    {
        var values = new List<JsonNode?>();
        var images = new JsonObject();
        var videos = new JsonObject();
        foreach (var part in state)
        {
            switch (part)
            {
                case DecisionModelTextPart text:
                    values.Add(JsonValue.Create(text.Text));
                    break;
                case DecisionModelJsonPart json:
                    values.Add(JsonNode.Parse(json.Value.GetRawText()));
                    break;
                case DecisionModelFilePart file:
                    var kind = MediaTypes.GetTopLevelMediaType(file.MediaType);
                    if (kind != "image" && kind != "video")
                    {
                        throw new UnsupportedFunctionalityException(
                            "HeyGen decision file media type: " + file.MediaType,
                            "'HeyGen decision file media type: "
                                + file.MediaType
                                + "' functionality not supported."
                        );
                    }

                    // Asset names are not shown to the model; it reads images, then videos, then state, each in request order.
                    var group = kind == "image" ? images : videos;
                    group[kind + "-" + (group.Count + 1)] = ToHeyGenAsset(file, kind);
                    break;
            }
        }

        if (
            images.Count > HeyGenModelCatalog.DecisionMaxImages
            || videos.Count > HeyGenModelCatalog.DecisionMaxVideos
        )
        {
            throw new InvalidArgumentException(
                "state",
                state,
                "HeyGen Decision supports at most "
                    + HeyGenModelCatalog.DecisionMaxImages
                    + " images and "
                    + HeyGenModelCatalog.DecisionMaxVideos
                    + " video per request."
            );
        }

        // A single string, object, or array is sent as native state. Other values stay in an array because the API rejects a root number, boolean, or null.
        JsonNode? nativeState;
        if (
            values.Count == 1
            && (
                values[0] is JsonObject or JsonArray
                || (values[0] is JsonValue single && single.GetValueKind() == JsonValueKind.String)
            )
        )
        {
            nativeState = values[0];
        }
        else
        {
            nativeState = new JsonArray(values.ToArray());
        }

        JsonObject? assets = null;
        if (images.Count > 0 || videos.Count > 0)
        {
            assets = new JsonObject();
            if (images.Count > 0)
            {
                assets["images"] = images;
            }

            if (videos.Count > 0)
            {
                assets["videos"] = videos;
            }
        }

        return (nativeState, assets);
    }

    // Maps to toHeyGenAsset.
    private static JsonObject ToHeyGenAsset(DecisionModelFilePart file, string kind)
    {
        var data = file.Data;
        switch (data.DataType)
        {
            case "url":
                if (data.Url == null || data.Url.Scheme != Uri.UriSchemeHttps)
                {
                    throw new InvalidArgumentException(
                        "state",
                        data.Url?.ToString(),
                        "HeyGen decision media URLs must use HTTPS."
                    );
                }

                return new JsonObject { ["type"] = "url", ["url"] = data.Url.AbsoluteUri };
            case "reference":
                return new JsonObject
                {
                    ["type"] = "asset_id",
                    ["asset_id"] = ProviderReferences.ResolveProviderReference(
                        data.Reference ?? new Dictionary<string, string>(),
                        HeyGenProvider.ProviderId
                    ),
                };
            case "data":
                var mediaType =
                    MediaTypes.IsFullMediaType(file.MediaType) ? file.MediaType
                    : data.Bytes != null ? MediaTypes.DetectMediaType(data.Bytes, kind)
                    : MediaTypes.DetectMediaType(data.Base64 ?? string.Empty, kind);
                if (mediaType == null)
                {
                    throw new InvalidArgumentException(
                        "state",
                        file.MediaType,
                        "Could not detect the "
                            + kind
                            + " media type. Specify a full mediaType such as "
                            + (kind == "image" ? "image/png" : "video/mp4")
                            + "."
                    );
                }

                return new JsonObject
                {
                    ["type"] = "base64",
                    ["media_type"] = mediaType,
                    ["data"] =
                        data.Bytes != null ? Convert.ToBase64String(data.Bytes) : data.Base64,
                };
            default:
                throw new UnsupportedFunctionalityException(
                    "HeyGen decision inline text files",
                    "'HeyGen decision inline text files' functionality not supported."
                );
        }
    }

    private JsonObject BuildQuestion(string id, DecisionQuestion question)
    {
        if (id.Length == 0 || id.Trim() != id || id.IndexOfAny([':', '\r', '\n']) >= 0)
        {
            throw new InvalidArgumentException(
                "questions",
                id,
                "HeyGen Decision question names must be nonempty, contain no colon or newline, and have no leading or trailing whitespace."
            );
        }

        switch (question.Type)
        {
            case "boolean":
                return Native("noul", question);
            case "choice":
                var count = question.Criteria?.Count ?? 0;
                if (count == 0 || count > HeyGenModelCatalog.DecisionMaxChoiceOptions)
                {
                    throw new InvalidArgumentException(
                        "questions." + id + ".criteria",
                        question.Criteria,
                        "HeyGen Choice questions support 1 to "
                            + HeyGenModelCatalog.DecisionMaxChoiceOptions
                            + " options."
                    );
                }

                return Native("choice", question);
            case "score":
                var levels = (question.Criteria as DecisionScoreCriteria)?.Levels ?? [];
                if (
                    levels.Count == 0
                    || levels.Count > HeyGenModelCatalog.DecisionMaxScoreLevels
                    || levels.Any(level => level == null)
                )
                {
                    throw new InvalidArgumentException(
                        "questions." + id + ".criteria",
                        question.Criteria,
                        "HeyGen Score questions require 1 to "
                            + HeyGenModelCatalog.DecisionMaxScoreLevels
                            + " non-null levels."
                    );
                }

                return Native("score", question);
            default:
                throw new DecisionUnsupportedQuestionTypeException(
                    id,
                    question.Type,
                    Provider,
                    ModelId
                );
        }
    }

    private static JsonObject Native(string type, DecisionQuestion question)
    {
        var native = new JsonObject { ["type"] = type };
        if (question.HasInstructions)
        {
            native["instructions"] = question.Instructions?.ToNode();
        }

        if (question.Criteria != null)
        {
            native["criteria"] = question.Criteria.ToNode();
        }

        return native;
    }

    // Maps to heyGenDecisionResponseSchema. A response that does not fit it is an API call error.
    private static ParsedResponse ParseResponse(JsonElement root, string body)
    {
        ApiException Malformed(string reason)
        {
            return new ApiException("Type validation failed: " + reason, 200, body);
        }

        string? Text(JsonElement parent, string property)
        {
            if (
                !parent.TryGetProperty(property, out var value)
                || value.ValueKind == JsonValueKind.Null
            )
            {
                return null;
            }

            return value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : throw Malformed(property + " must be a string.");
        }

        double Probability(JsonElement element, string what)
        {
            if (
                element.ValueKind != JsonValueKind.Number
                || !element.TryGetDouble(out var value)
                || value < 0
                || value > 1
            )
            {
                throw Malformed(what + " must be a number from 0 to 1.");
            }

            return value;
        }

        double? NullableProbability(JsonElement parent, string property)
        {
            return
                parent.TryGetProperty(property, out var value)
                && value.ValueKind != JsonValueKind.Null
                ? Probability(value, property)
                : null;
        }

        Dictionary<string, double> Distribution(JsonElement parent)
        {
            if (
                !parent.TryGetProperty("probabilities", out var map)
                || map.ValueKind != JsonValueKind.Object
            )
            {
                throw Malformed("probabilities must be an object.");
            }

            var result = new Dictionary<string, double>();
            foreach (var item in map.EnumerateObject())
            {
                result[item.Name] = Probability(item.Value, "probabilities");
            }

            return result;
        }

        if (
            root.ValueKind != JsonValueKind.Object
            || !root.TryGetProperty("data", out var data)
            || data.ValueKind != JsonValueKind.Object
        )
        {
            throw Malformed("data must be an object.");
        }

        if (!data.TryGetProperty("answers", out var list) || list.ValueKind != JsonValueKind.Object)
        {
            throw Malformed("answers must be an object.");
        }

        var parsed = new ParsedResponse
        {
            Id = Text(data, "id"),
            RequestId = Text(data, "request_id"),
            Model = Text(data, "model"),
        };
        foreach (var item in list.EnumerateObject())
        {
            var element = item.Value;
            if (
                element.ValueKind != JsonValueKind.Object
                || !element.TryGetProperty("type", out var type)
                || type.ValueKind != JsonValueKind.String
            )
            {
                throw Malformed("answer type must be noul, choice, or score.");
            }

            var answer = new ParsedAnswer
            {
                Name = item.Name,
                Type = type.GetString()!,
                Warning = Text(element, "warning"),
            };
            switch (answer.Type)
            {
                case "noul":
                    answer.Probability = element.TryGetProperty("noul", out var noul)
                        ? Probability(noul, "noul")
                        : throw Malformed("noul is required.");
                    break;
                case "choice":
                    if (
                        !element.TryGetProperty("choice", out var choice)
                        || choice.ValueKind != JsonValueKind.String
                    )
                    {
                        throw Malformed("choice must be a string.");
                    }

                    answer.Choice = choice.GetString();
                    answer.Probabilities = Distribution(element);
                    answer.Confidence = NullableProbability(element, "confidence");
                    break;
                case "score":
                    if (
                        !element.TryGetProperty("score", out var score)
                        || score.ValueKind != JsonValueKind.Number
                        || !score.TryGetDouble(out var scoreValue)
                    )
                    {
                        throw Malformed("score must be a number.");
                    }

                    answer.Score = scoreValue;
                    answer.Probabilities = Distribution(element);
                    answer.Confidence = NullableProbability(element, "confidence");
                    if (
                        element.TryGetProperty("legend", out var legend)
                        && legend.ValueKind != JsonValueKind.Null
                    )
                    {
                        answer.Legend =
                            legend.ValueKind == JsonValueKind.Object
                                ? legend
                                : throw Malformed("legend must be an object.");
                    }

                    break;
                default:
                    throw Malformed("answer type must be noul, choice, or score.");
            }

            parsed.Answers.Add(answer);
        }

        if (data.TryGetProperty("usage", out var usage) && usage.ValueKind != JsonValueKind.Null)
        {
            if (usage.ValueKind != JsonValueKind.Object)
            {
                throw Malformed("usage must be an object.");
            }

            parsed.Usage = new OperationUsage(
                Count(usage, "input_tokens"),
                Count(usage, "output_tokens")
            );
        }

        return parsed;

        int? Count(JsonElement parent, string name)
        {
            if (
                !parent.TryGetProperty(name, out var value)
                || value.ValueKind == JsonValueKind.Null
            )
            {
                return null;
            }

            return value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var count)
                ? count
                : throw Malformed(name + " must be a number.");
        }
    }

    private sealed class ParsedResponse
    {
        public string? Id { get; set; }

        public string? RequestId { get; set; }

        public string? Model { get; set; }

        public List<ParsedAnswer> Answers { get; } = [];

        public OperationUsage? Usage { get; set; }
    }

    private sealed class ParsedAnswer
    {
        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public double? Probability { get; set; }

        public string? Choice { get; set; }

        public double? Score { get; set; }

        public Dictionary<string, double>? Probabilities { get; set; }

        public double? Confidence { get; set; }

        public JsonElement? Legend { get; set; }

        public string? Warning { get; set; }
    }
}
