// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenResponses;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.QuiverAI;

/// <summary>Applies QuiverAI's request policy while reusing the Open Responses transport. Maps to <c>QuiverAILanguageModel</c>.</summary>
public sealed class QuiverAILanguageModel : ILanguageModel
{
    private static readonly IReadOnlyDictionary<string, string> ReasoningEffortMap = new Dictionary<
        string,
        string
    >
    {
        ["low"] = "low",
        ["medium"] = "medium",
        ["high"] = "high",
        ["xhigh"] = "xhigh",
    };

    private readonly ILanguageModel _model;
    private readonly OpenResponsesOptions _config;

    /// <summary>Creates a model over an Open Responses model that was configured by <see cref="QuiverAIProvider"/>.</summary>
    internal QuiverAILanguageModel(ILanguageModel model, OpenResponsesOptions config)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string ModelId => _model.ModelId;

    /// <inheritdoc />
    public string Provider => _model.Provider;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> SupportedUrls => _model.SupportedUrls;

    /// <summary>
    /// Serializes the model id and config for a workflow boundary. Maps to <c>[WORKFLOW_SERIALIZE]</c>.
    /// The API key is not part of the serialized config. Pass it to <see cref="Deserialize"/>.
    /// </summary>
    public SerializedModelOptions Serialize()
    {
        var headers = new Dictionary<string, object?>();
        if (_config.Headers is { } configured)
        {
            foreach (
                var pair in configured.ResolveAsync(CancellationToken.None).GetAwaiter().GetResult()
            )
            {
                if (!IsSensitiveHeader(pair.Key))
                {
                    headers[pair.Key] = pair.Value;
                }
            }
        }

        var config = new Dictionary<string, object?>
        {
            ["provider"] = Provider,
            ["providerOptionsName"] = _config.Name,
            ["baseURL"] = _config.BaseUrl,
            ["headers"] = headers,
        };
        return ModelOptionsSerialization.SerializeModelOptions(ModelId, config);
    }

    /// <summary>Restores a model from <see cref="Serialize"/>, with QuiverAI's handlers. Maps to <c>[WORKFLOW_DESERIALIZE]</c>.</summary>
    /// <param name="serialized">The serialized model id and config.</param>
    /// <param name="apiKey">API key. Defaults to the <c>QUIVERAI_API_KEY</c> environment variable.</param>
    /// <param name="handler">HTTP handler. Pass one from tests.</param>
    public static QuiverAILanguageModel Deserialize(
        SerializedModelOptions serialized,
        string? apiKey = null,
        HttpMessageHandler? handler = null
    )
    {
        if (serialized is null)
        {
            throw new ArgumentNullException(nameof(serialized));
        }

        var config = QuiverAIProvider.CreateResponsesOptions(
            serialized.Config.TryGetValue("baseURL", out var baseUrl) ? Text(baseUrl) : null,
            apiKey is null ? null : Resolvable<string>.From(apiKey),
            Headers(serialized.Config.TryGetValue("headers", out var headers) ? headers : null)
        );
        var provider = OpenResponsesProvider.Create(config, handler);
        QuiverAIProvider.ApplyErrorHandling(provider);
        return new QuiverAILanguageModel(provider.LanguageModel(serialized.ModelId), config);
    }

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(
        LanguageModelCallOptions options,
        CancellationToken cancellationToken
    )
    {
        var prepared = PrepareQuiverAICall(ModelId, options);
        var result = await _model
            .DoGenerateAsync(prepared.Options, cancellationToken)
            .ConfigureAwait(false);
        return result.WithWarnings([.. prepared.Warnings, .. result.Warnings]);
    }

    /// <inheritdoc />
    public async Task<LanguageModelStreamResult> DoStreamAsync(
        LanguageModelCallOptions options,
        CancellationToken cancellationToken
    )
    {
        var prepared = PrepareQuiverAICall(ModelId, options);
        var result = await _model
            .DoStreamAsync(prepared.Options, cancellationToken)
            .ConfigureAwait(false);
        if (prepared.Warnings.Count == 0)
        {
            return result;
        }

        return new LanguageModelStreamResult(
            WithWarnings(result.Stream, prepared.Warnings, cancellationToken),
            result.Request,
            result.Response,
            result
        );
    }

    private static async IAsyncEnumerable<LanguageModelStreamPart> WithWarnings(
        IAsyncEnumerable<LanguageModelStreamPart> stream,
        IReadOnlyList<CallWarning> warnings,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        await foreach (var part in stream.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            yield return part is StreamStartStreamPart start
                ? new StreamStartStreamPart([.. warnings, .. start.Warnings])
                : part;
        }
    }

    /// <summary>Validates the QuiverAI provider options, maps top-level reasoning, and strips private reasoning from the history. Maps to <c>prepareQuiverAICall</c>.</summary>
    private static (
        LanguageModelCallOptions Options,
        List<CallWarning> Warnings
    ) PrepareQuiverAICall(string modelId, LanguageModelCallOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        JsonElement? reasoningEffort = null;
        JsonElement? reasoningSummary = null;
        if (
            options.ProviderOptions is not null
            && options.ProviderOptions.TryGetValue("quiverai", out var quiverai)
            && quiverai.ValueKind == JsonValueKind.Object
        )
        {
            reasoningEffort = NotNull(quiverai, "reasoningEffort");
            reasoningSummary = NotNull(quiverai, "reasoningSummary");
        }

        if (
            reasoningEffort is { } effort
            && !(
                effort.ValueKind == JsonValueKind.String
                && QuiverAIModelCatalog
                    .GetQuiverAILanguageModelReasoningEfforts(modelId)
                    .Contains(effort.GetString())
            )
        )
        {
            throw new InvalidArgumentException(
                "providerOptions",
                options.ProviderOptions,
                "Unsupported reasoning effort: " + Display(effort)
            );
        }

        if (
            reasoningSummary is { } summary
            && !(summary.ValueKind == JsonValueKind.String && summary.GetString() == "auto")
        )
        {
            throw new InvalidArgumentException(
                "providerOptions",
                options.ProviderOptions,
                "Unsupported reasoning summary: " + Display(summary)
            );
        }

        var warnings = new List<CallWarning>();
        var prepared = options.Copy();
        if (reasoningEffort is null && ReasoningMap.IsCustomReasoning(options.Reasoning))
        {
            var mappingWarnings = new List<ModelWarning>();
            prepared.Reasoning = ReasoningMap.MapReasoningToProviderEffort(
                options.Reasoning,
                ReasoningEffortMap,
                mappingWarnings
            );
            foreach (var warning in mappingWarnings)
            {
                if (ReasoningMap.ToCallWarning(warning) is { } callWarning)
                {
                    warnings.Add(callWarning);
                }
            }
        }

        prepared.Prompt = options.Prompt.Select(StripReasoning).ToList();
        return (prepared, warnings);
    }

    private static ModelMessage StripReasoning(ModelMessage message)
    {
        if (message is not AssistantModelMessage assistant)
        {
            return message;
        }

        return new AssistantModelMessage(
            assistant
                .Parts.Select(part =>
                    part is AssistantReasoningPart reasoning
                        ? new AssistantReasoningPart(string.Empty, OpaqueOptions(reasoning))
                        : part
                )
                .ToList()
        );
    }

    // Replays only the opaque item id and the safe summary, never private or encrypted reasoning.
    private static Dictionary<string, JsonElement> OpaqueOptions(AssistantReasoningPart part)
    {
        var options = new Dictionary<string, JsonElement>();
        if (part.ProviderOptions is not null)
        {
            foreach (var pair in part.ProviderOptions)
            {
                options[pair.Key] = pair.Value;
            }
        }

        var opaque = new JsonObject();
        if (
            part.ProviderOptions is not null
            && part.ProviderOptions.TryGetValue("quiverai", out var metadata)
            && metadata.ValueKind == JsonValueKind.Object
        )
        {
            foreach (var name in new[] { "itemId", "reasoningSummary" })
            {
                if (NotNull(metadata, name) is { } value)
                {
                    opaque[name] = JsonNode.Parse(value.GetRawText());
                }
            }
        }

        options["quiverai"] = JsonSerializer.SerializeToElement(opaque);
        return options;
    }

    private static JsonElement? NotNull(JsonElement owner, string name)
    {
        return
            owner.TryGetProperty(name, out var value)
            && value.ValueKind is not (JsonValueKind.Null or JsonValueKind.Undefined)
            ? value
            : null;
    }

    private static string Display(JsonElement value)
    {
        return value.ValueKind == JsonValueKind.String ? value.GetString()! : value.GetRawText();
    }

    private static string? Text(object? value)
    {
        return value is JsonElement { ValueKind: JsonValueKind.String } element
            ? element.GetString()
            : value as string;
    }

    private static Dictionary<string, string> Headers(object? value)
    {
        var headers = new Dictionary<string, string>();
        if (value is JsonElement { ValueKind: JsonValueKind.Object } json)
        {
            foreach (var property in json.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.String)
                {
                    headers[property.Name] = property.Value.GetString()!;
                }
            }
        }
        else if (value is IEnumerable<KeyValuePair<string, object?>> pairs)
        {
            foreach (var pair in pairs)
            {
                if (pair.Value is string text)
                {
                    headers[pair.Key] = text;
                }
            }
        }

        return headers;
    }

    private static bool IsSensitiveHeader(string name)
    {
        foreach (var marker in new[] { "authorization", "key", "token", "secret", "cookie" })
        {
            if (name.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }
}
