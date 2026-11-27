// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Google;

/// <summary>
/// Vertex AI embedding model. Most models use <c>:predict</c>. <c>gemini-embedding-2</c> and
/// <c>gemini-embedding-2-preview</c> use <c>:embedContent</c>, one value per call.
/// Maps to <c>GoogleVertexEmbeddingModel</c>.
/// </summary>
public sealed class GoogleVertexEmbeddingModel : IEmbeddingModel, IEmbeddingCaller
{
    private static readonly string[] OptionNamespaces = { "googleVertex", "vertex", "google" };

    private readonly GoogleVertexProvider _provider;

    /// <summary>Creates a Vertex embedding model.</summary>
    public GoogleVertexEmbeddingModel(GoogleVertexProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string SpecificationVersion
    {
        get { return "v4"; }
    }

    /// <inheritdoc />
    public string Provider
    {
        get { return "google.vertex.embedding"; }
    }

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>One value for the <c>embedContent</c> models, 250 for the <c>predict</c> models.</summary>
    public int? MaxEmbeddingsPerCall
    {
        get { return UsesEmbedContentEndpoint(ModelId) ? 1 : 250; }
    }

    /// <inheritdoc />
    public double? MaxInputBytesPerCall
    {
        get { return null; }
    }

    /// <inheritdoc />
    public bool SupportsParallelCalls
    {
        get { return true; }
    }

    /// <inheritdoc />
    public Task<JsonElement?> TransformProviderOptionsAsync(
        JsonElement? providerOptions,
        IReadOnlyList<string> values,
        int startIndex,
        int endIndex,
        CancellationToken cancellationToken
    )
    {
        return Task.FromResult<JsonElement?>(null);
    }

    /// <inheritdoc />
    public async Task<EmbeddingResult> DoEmbedAsync(
        IReadOnlyList<string> values,
        IReadOnlyDictionary<string, JsonElement>? providerOptions,
        CancellationToken cancellationToken,
        int? dimensions = null,
        IReadOnlyDictionary<string, string?>? headers = null
    )
    {
        var response = await DoEmbedAsync(
                new EmbeddingModelCall(
                    values,
                    (headers ?? new Dictionary<string, string?>())
                        .Where(pair => pair.Value is not null)
                        .ToDictionary(pair => pair.Key, pair => pair.Value!),
                    providerOptions is null
                        ? null
                        : JsonSerializer.SerializeToElement(providerOptions),
                    cancellationToken,
                    dimensions
                ),
                cancellationToken
            )
            .ConfigureAwait(false);
        return new EmbeddingResult(
            response.Embeddings.Select(vector => Array.ConvertAll(vector, v => (float)v)).ToList(),
            double.IsNaN(response.Tokens) ? null : (int)response.Tokens,
            new List<CallWarning>(),
            new EmbeddingResponseInfo(
                response.Response?.Headers,
                response.Response?.Body?.GetRawText()
            )
        );
    }

    /// <inheritdoc />
    public async Task<EmbeddingModelResponse> DoEmbedAsync(
        EmbeddingModelCall call,
        CancellationToken cancellationToken
    )
    {
        if (call == null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        var options = ReadOptions(call.ProviderOptions);
        var limit = MaxEmbeddingsPerCall!.Value;
        if (call.Values.Count > limit)
        {
            throw new TooManyEmbeddingValuesForCallException(Provider, ModelId, limit, call.Values);
        }

        var headers = await _provider.HeadersAsync(cancellationToken).ConfigureAwait(false);
        foreach (var header in call.Headers)
        {
            headers[header.Key] = header.Value;
        }

        var outputDimensionality = options.OutputDimensionality ?? call.Dimensions;
        var embedContent = UsesEmbedContentEndpoint(ModelId);
        JsonObject body;
        if (embedContent)
        {
            var config = new JsonObject();
            Set(config, "outputDimensionality", outputDimensionality);
            Set(config, "taskType", options.TaskType);
            Set(config, "title", options.Title);
            Set(config, "autoTruncate", options.AutoTruncate);
            body = new JsonObject
            {
                ["content"] = new JsonObject
                {
                    ["parts"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["text"] = call.Values.Count == 0 ? null : call.Values[0],
                        },
                    },
                },
                ["embedContentConfig"] = config,
            };
        }
        else
        {
            var instances = new JsonArray();
            foreach (var value in call.Values)
            {
                var instance = new JsonObject { ["content"] = value };
                Set(instance, "task_type", options.TaskType);
                Set(instance, "title", options.Title);
                instances.Add(instance);
            }

            var parameters = new JsonObject();
            Set(parameters, "outputDimensionality", outputDimensionality);
            Set(parameters, "autoTruncate", options.AutoTruncate);
            body = new JsonObject { ["instances"] = instances, ["parameters"] = parameters };
        }

        var response = await _provider
            .Http.SendJsonStringAsync(
                HttpMethod.Post,
                new Uri(
                    _provider.Options.BaseUrl
                        + "/models/"
                        + ModelId
                        + (embedContent ? ":embedContent" : ":predict")
                ),
                GoogleJson.Write(body),
                headers,
                cancellationToken
            )
            .ConfigureAwait(false);
        using var document = JsonDocument.Parse(response.Body);
        var root = document.RootElement;
        var providerResponse = new ProviderResponse(response.Headers, root.Clone());
        if (embedContent)
        {
            var vector = ReadVector(Require(root, "embedding"));
            double? tokens = null;
            if (
                root.TryGetProperty("usageMetadata", out var usage)
                && usage.ValueKind == JsonValueKind.Object
                && usage.TryGetProperty("promptTokenCount", out var count)
                && count.ValueKind == JsonValueKind.Number
            )
            {
                tokens = count.GetDouble();
            }

            return new EmbeddingModelResponse(
                new[] { vector },
                tokens,
                response: providerResponse,
                usageOmitted: tokens is null
            );
        }

        var embeddings = new List<double[]>();
        double tokenCount = 0;
        if (
            !root.TryGetProperty("predictions", out var predictions)
            || predictions.ValueKind != JsonValueKind.Array
        )
        {
            throw new InvalidResponseDataException(
                root.Clone(),
                "Invalid Vertex embedding response: predictions are missing."
            );
        }

        foreach (var prediction in predictions.EnumerateArray())
        {
            var item = Require(prediction, "embeddings");
            embeddings.Add(ReadVector(item));
            var count = Require(Require(item, "statistics"), "token_count");
            if (count.ValueKind != JsonValueKind.Number)
            {
                throw new InvalidResponseDataException(
                    count.Clone(),
                    "Invalid Vertex embedding response: 'token_count' is not a number."
                );
            }

            tokenCount += count.GetDouble();
        }

        return new EmbeddingModelResponse(embeddings, tokenCount, response: providerResponse);
    }

    private static bool UsesEmbedContentEndpoint(string modelId)
    {
        return modelId == "gemini-embedding-2" || modelId == "gemini-embedding-2-preview";
    }

    private static JsonElement Require(JsonElement element, string name)
    {
        if (
            element.ValueKind != JsonValueKind.Object
            || !element.TryGetProperty(name, out var value)
        )
        {
            throw new InvalidResponseDataException(
                element.Clone(),
                "Invalid Vertex embedding response: '" + name + "' is missing."
            );
        }

        return value;
    }

    private static double[] ReadVector(JsonElement container)
    {
        return Require(container, "values")
            .EnumerateArray()
            .Select(number => number.GetDouble())
            .ToArray();
    }

    private static void Set(JsonObject target, string name, object? value)
    {
        if (value is null)
        {
            return;
        }

        target[name] = value switch
        {
            int number => number,
            bool flag => flag,
            string text => text,
            _ => throw new ArgumentOutOfRangeException(nameof(value)),
        };
    }

    private static VertexEmbeddingOptions ReadOptions(JsonElement? providerOptions)
    {
        if (providerOptions is not { ValueKind: JsonValueKind.Object } bag)
        {
            return new VertexEmbeddingOptions();
        }

        foreach (var name in OptionNamespaces)
        {
            if (
                bag.TryGetProperty(name, out var section)
                && section.ValueKind == JsonValueKind.Object
            )
            {
                return new VertexEmbeddingOptions
                {
                    OutputDimensionality =
                        section.TryGetProperty("outputDimensionality", out var width)
                        && width.TryGetInt32(out var count)
                            ? count
                            : null,
                    TaskType = GoogleJson.String(section, "taskType"),
                    Title = GoogleJson.String(section, "title"),
                    AutoTruncate =
                        section.TryGetProperty("autoTruncate", out var truncate)
                        && (
                            truncate.ValueKind == JsonValueKind.True
                            || truncate.ValueKind == JsonValueKind.False
                        )
                            ? truncate.GetBoolean()
                            : null,
                };
            }
        }

        return new VertexEmbeddingOptions();
    }

    private sealed class VertexEmbeddingOptions
    {
        public int? OutputDimensionality { get; init; }

        public string? TaskType { get; init; }

        public string? Title { get; init; }

        public bool? AutoTruncate { get; init; }
    }
}
