// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.AmazonBedrock;

/// <summary>Amazon Bedrock <c>InvokeModel</c> embedding model for Titan, Cohere, and Nova.</summary>
public sealed class AmazonBedrockEmbeddingModel : IEmbeddingModel
{
    private readonly AmazonBedrockProvider _provider;

    private readonly string? _modelFamily;

    /// <summary>Creates an embedding model.</summary>
    public AmazonBedrockEmbeddingModel(AmazonBedrockProvider provider, string modelId)
        : this(provider, modelId, null)
    {
    }

    /// <summary>Creates an embedding model with <paramref name="settings"/>.</summary>
    public AmazonBedrockEmbeddingModel(AmazonBedrockProvider provider, string modelId, AmazonBedrockEmbeddingModelSettings? settings)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _modelFamily = settings?.ModelFamily switch
        {
            AmazonBedrockEmbeddingModelFamily.Titan => "titan",
            AmazonBedrockEmbeddingModelFamily.Cohere => "cohere",
            AmazonBedrockEmbeddingModelFamily.Nova => "nova",
            _ => null,
        };
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => AmazonBedrockProvider.ProviderName;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Maximum values per call. Cohere models take 96. Titan and Nova take one.</summary>
    public int MaxEmbeddingsPerCall => Family == "cohere" ? 96 : 1;

    private string Family => _modelFamily ?? (IsNova(ModelId) ? "nova" : ModelId.Contains("cohere.embed-", StringComparison.Ordinal) ? "cohere" : "titan");

    /// <inheritdoc />
    public Task<EmbeddingResult> DoEmbedAsync(IReadOnlyList<string> values, IReadOnlyDictionary<string, JsonElement>? providerOptions, CancellationToken cancellationToken, int? dimensions = null)
    {
        return DoEmbedAsync(new EmbeddingCallOptions(values ?? Array.Empty<string>()) { ProviderOptions = providerOptions, Dimensions = dimensions }, cancellationToken);
    }

    /// <summary>Embeds each value. Values, provider options, headers, and dimensions travel in <paramref name="call"/>, so concurrent calls do not share state.</summary>
    public async Task<EmbeddingResult> DoEmbedAsync(EmbeddingCallOptions call, CancellationToken cancellationToken)
    {
        if (call == null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        var values = call.Values;
        var providerOptions = call.ProviderOptions;
        var dimensions = call.Dimensions;
        if (values.Count > MaxEmbeddingsPerCall)
        {
            throw new AiSdkException("Too many embedding values for one call. The limit is " + MaxEmbeddingsPerCall.ToString(CultureInfo.InvariantCulture) + ".");
        }

        var options = Bag(providerOptions);
        var first = values.Count > 0 ? values[0] : string.Empty;
        var body = new JsonObject();
        switch (Family)
        {
            case "nova":
                body["taskType"] = "SINGLE_EMBEDDING";
                body["singleEmbeddingParams"] = new JsonObject
                {
                    ["embeddingPurpose"] = String(options, "embeddingPurpose") ?? "GENERIC_INDEX",
                    ["embeddingDimension"] = Int(options, "embeddingDimension") ?? dimensions ?? 1024,
                    ["text"] = new JsonObject
                    {
                        ["truncationMode"] = String(options, "truncate") ?? "END",
                        ["value"] = first,
                    },
                };
                break;
            case "cohere":
                var texts = new JsonArray();
                foreach (var value in values)
                {
                    texts.Add(value);
                }

                body["input_type"] = String(options, "inputType") ?? "search_query";
                body["texts"] = texts;
                Set(body, "truncate", String(options, "truncate"));
                Set(body, "output_dimension", Int(options, "outputDimension") ?? dimensions);
                break;
            default:
                body["inputText"] = first;
                Set(body, "dimensions", Int(options, "dimensions") ?? dimensions);
                if (options is { } titan && titan.TryGetProperty("normalize", out var normalize) && normalize.ValueKind is JsonValueKind.True or JsonValueKind.False)
                {
                    body["normalize"] = normalize.GetBoolean();
                }

                break;
        }

        var json = body.ToJsonString();
        var uri = new Uri(AmazonBedrockEndpoints.Resolve(_provider.Options.BaseUrl, _provider.Options.Region, "bedrock-runtime") + "/model/" + Uri.EscapeDataString(ModelId) + "/invoke");
        using var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };
        _provider.AddHeaders(request, call.Headers);
        var payload = Encoding.UTF8.GetBytes(json);
        var accessKey = _provider.Options.AccessKeyId ?? Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
        var secret = _provider.Options.SecretAccessKey ?? Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
        if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secret))
        {
            throw new AiSdkException("AWS credentials are required. Set AWS_ACCESS_KEY_ID and AWS_SECRET_ACCESS_KEY.");
        }

        var token = _provider.Options.SessionToken ?? Environment.GetEnvironmentVariable("AWS_SESSION_TOKEN");
        AwsSigV4.Sign(request, payload, _provider.Options.Region, "bedrock", accessKey!, secret!, token, _provider.Options.UtcNow?.Invoke() ?? DateTimeOffset.UtcNow);
        using var response = await _provider.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var text = await CancellableReads.ReadAsStringAsync(response.Content, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw AmazonBedrockErrors.Create((int)response.StatusCode, text);
        }

        using var document = JsonDocument.Parse(text);
        var root = document.RootElement;
        var vectors = new List<float[]>();
        int? tokens = null;
        if (root.TryGetProperty("embedding", out var titanVector))
        {
            vectors.Add(Vector(titanVector));
            tokens = root.TryGetProperty("inputTextTokenCount", out var count) && count.TryGetInt32(out var titanTokens) ? titanTokens : null;
        }
        else if (root.TryGetProperty("embeddings", out var embeddings))
        {
            if (embeddings.ValueKind == JsonValueKind.Array)
            {
                var firstEntry = embeddings.GetArrayLength() > 0 ? embeddings[0] : default;
                if (firstEntry.ValueKind == JsonValueKind.Object && firstEntry.TryGetProperty("embeddingType", out _))
                {
                    vectors.Add(Vector(firstEntry.GetProperty("embedding")));
                    tokens = root.TryGetProperty("inputTokenCount", out var novaCount) && novaCount.TryGetInt32(out var novaTokens) ? novaTokens : 0;
                }
                else
                {
                    foreach (var entry in embeddings.EnumerateArray())
                    {
                        vectors.Add(Vector(entry));
                    }
                }
            }
            else if (embeddings.TryGetProperty("float", out var floats))
            {
                foreach (var entry in floats.EnumerateArray())
                {
                    vectors.Add(Vector(entry));
                }
            }
        }

        if (tokens == null && response.Headers.TryGetValues("x-amzn-bedrock-input-token-count", out var header)
            && int.TryParse(header.FirstOrDefault(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var headerTokens))
        {
            tokens = headerTokens;
        }

        return new EmbeddingResult(vectors, tokens);
    }

    private static bool IsNova(string modelId)
    {
        return modelId.Contains("amazon.nova-", StringComparison.Ordinal) && modelId.Contains("embed", StringComparison.Ordinal);
    }

    private static JsonElement? Bag(IReadOnlyDictionary<string, JsonElement>? providerOptions)
    {
        if (providerOptions == null)
        {
            return null;
        }

        foreach (var key in new[] { "amazonBedrock", "bedrock" })
        {
            if (providerOptions.TryGetValue(key, out var bag) && bag.ValueKind == JsonValueKind.Object)
            {
                return bag;
            }
        }

        return null;
    }

    private static string? String(JsonElement? bag, string name)
    {
        return bag is { } element && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    }

    private static int? Int(JsonElement? bag, string name)
    {
        return bag is { } element && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number) ? number : null;
    }

    private static void Set(JsonObject body, string name, string? value)
    {
        if (value != null)
        {
            body[name] = value;
        }
    }

    private static void Set(JsonObject body, string name, int? value)
    {
        if (value != null)
        {
            body[name] = value.Value;
        }
    }

    private static float[] Vector(JsonElement values)
    {
        var vector = new float[values.GetArrayLength()];
        var index = 0;
        foreach (var number in values.EnumerateArray())
        {
            vector[index++] = number.GetSingle();
        }

        return vector;
    }
}
