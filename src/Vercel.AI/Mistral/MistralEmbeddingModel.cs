// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Mistral;

/// <summary>Mistral text embedding model. Maps to <c>MistralEmbeddingModel</c>.</summary>
public sealed class MistralEmbeddingModel : IEmbeddingModel
{
    private readonly OpenAICompatibleProvider _provider;

    /// <summary>Creates an embedding model.</summary>
    public MistralEmbeddingModel(OpenAICompatibleProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public string Provider => "mistral.embedding";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Maximum inputs accepted by one request.</summary>
    public int MaxEmbeddingsPerCall => 32;

    /// <summary>Whether the model embeds in parallel when a call has more values than <see cref="MaxEmbeddingsPerCall"/>.</summary>
    public bool SupportsParallelCalls => false;

    /// <inheritdoc />
    public async Task<EmbeddingResult> DoEmbedAsync(
        IReadOnlyList<string> values,
        IReadOnlyDictionary<string, JsonElement>? providerOptions,
        CancellationToken cancellationToken,
        int? dimensions = null,
        IReadOnlyDictionary<string, string?>? headers = null
    )
    {
        values = values ?? throw new ArgumentNullException(nameof(values));
        if (values.Count > MaxEmbeddingsPerCall)
        {
            throw new AiSdkException(
                "Too many embedding values for a single call to '"
                    + Provider
                    + "'. The maximum is "
                    + MaxEmbeddingsPerCall.ToString(CultureInfo.InvariantCulture)
                    + "."
            );
        }

        var options = MistralEmbeddingModelOptions.Parse(providerOptions);
        var input = new JsonArray();
        foreach (var value in values)
        {
            input.Add(value);
        }

        var body = new JsonObject { ["model"] = ModelId, ["input"] = input };
        if (options?.Metadata != null)
        {
            var metadata = new JsonObject();
            foreach (var entry in options.Metadata)
            {
                metadata[entry.Key] = entry.Value?.DeepClone();
            }

            body["metadata"] = metadata;
        }

        if ((options?.OutputDimension ?? dimensions) is { } outputDimension)
        {
            body["output_dimension"] = outputDimension;
        }

        if (options?.OutputDtype != null)
        {
            body["output_dtype"] = options.OutputDtype;
        }

        body["encoding_format"] = "float";

        var url = ApiKeys.Combine(_provider.Options.BaseUrl, "embeddings");
        var requestBody = body.ToJsonString();
        var response = await ProviderExchange
            .SendAsync(
                _provider.SharedHttpClient,
                HttpMethod.Post,
                url,
                ProviderExchange.Json(requestBody),
                (await _provider.CreateHeadersAsync(headers, cancellationToken).ConfigureAwait(false)),
                cancellationToken,
                (status, text) => MistralError.FailedResponse(status, text, url, body)
            )
            .ConfigureAwait(false);

        using var parsed = JsonDocument.Parse(response.Body);
        var vectors = new List<float[]>();
        foreach (var item in parsed.RootElement.GetProperty("data").EnumerateArray())
        {
            var embedding = item.GetProperty("embedding");
            var vector = new float[embedding.GetArrayLength()];
            var index = 0;
            foreach (var number in embedding.EnumerateArray())
            {
                vector[index++] = number.GetSingle();
            }

            vectors.Add(vector);
        }

        int? tokens = null;
        if (
            parsed.RootElement.TryGetProperty("usage", out var usage)
            && usage.ValueKind == JsonValueKind.Object
            && usage.TryGetProperty("prompt_tokens", out var prompt)
            && prompt.ValueKind == JsonValueKind.Number
        )
        {
            tokens = prompt.GetInt32();
        }

        return new EmbeddingResult(
            vectors,
            tokens,
            new List<CallWarning>(),
            new EmbeddingResponseInfo(response.Headers, response.Body)
        );
    }
}
