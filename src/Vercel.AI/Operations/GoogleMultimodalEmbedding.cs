// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.Operations;

/// <summary>
/// Gemini embedding caller used by <see cref="EmbedMany"/>.
/// Splits multimodal <c>google.content</c> to the chunk that is about to be sent
/// and posts <c>embedContent</c> or <c>batchEmbedContents</c>.
/// </summary>
public sealed class GoogleMultimodalEmbedding : IEmbeddingCaller
{
    /// <summary>Values accepted by one Gemini embedding request.</summary>
    public const int EmbeddingsPerCall = 100;

    private readonly HttpClient _http;
    private readonly string _baseUrl;

    /// <summary>Creates a caller that posts to <paramref name="baseUrl"/>.</summary>
    public GoogleMultimodalEmbedding(string modelId, HttpClient http, string? baseUrl = null)
    {
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _baseUrl = (baseUrl ?? "https://generativelanguage.googleapis.com/v1beta").TrimEnd('/');
    }

    /// <inheritdoc />
    public string Provider => "google.generative-ai";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public int? MaxEmbeddingsPerCall => EmbeddingsPerCall;

    /// <inheritdoc />
    public double? MaxInputBytesPerCall => null;

    /// <inheritdoc />
    public bool SupportsParallelCalls => true;

    /// <inheritdoc />
    public Task<JsonElement?> TransformProviderOptionsAsync(JsonElement? providerOptions, IReadOnlyList<string> values, int startIndex, int endIndex, CancellationToken cancellationToken)
    {
        var content = ContentArray(providerOptions);
        if (content == null)
        {
            return Task.FromResult(providerOptions);
        }

        if (content.Value.GetArrayLength() != values.Count)
        {
            throw new InvalidOperationException(
                "The number of multimodal content entries (" + content.Value.GetArrayLength().ToString(CultureInfo.InvariantCulture) + ") must match the number of values (" + values.Count.ToString(CultureInfo.InvariantCulture) + ").");
        }

        var sliced = new JsonArray();
        for (var i = startIndex; i < endIndex && i < content.Value.GetArrayLength(); i++)
        {
            sliced.Add(JsonNode.Parse(content.Value[i].GetRawText()));
        }

        var root = providerOptions!.Value.Clone();
        var copy = JsonNode.Parse(root.GetRawText())!.AsObject();
        var google = copy["google"]!.AsObject();
        google["content"] = sliced;
        return Task.FromResult<JsonElement?>(OperationJson.Parse(copy.ToJsonString()));
    }

    /// <inheritdoc />
    public async Task<EmbeddingModelResponse> DoEmbedAsync(EmbeddingModelCall call, CancellationToken cancellationToken)
    {
        var google = GoogleOptions(call.ProviderOptions);
        var content = google == null ? null : ContentProperty(google.Value);
        if (content != null && content.Value.GetArrayLength() != call.Values.Count)
        {
            throw new InvalidOperationException(
                "The number of multimodal content entries (" + content.Value.GetArrayLength().ToString(CultureInfo.InvariantCulture) + ") must match the number of values (" + call.Values.Count.ToString(CultureInfo.InvariantCulture) + ").");
        }

        ValidateContent(content);
        var dimensionality = Dimensionality(google);
        var single = call.Values.Count == 1;
        var body = single
            ? SingleBody(call.Values[0], content, dimensionality)
            : BatchBody(call.Values, content, dimensionality);
        var path = single ? ":embedContent" : ":batchEmbedContents";
        using (var request = new HttpRequestMessage(HttpMethod.Post, _baseUrl + "/models/" + ModelId + path))
        {
            request.Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");
            foreach (var header in call.Headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            using (var response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false))
            {
                var payload = response.Content == null ? string.Empty : await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var header in response.Headers)
                    {
                        headers[header.Key.ToLowerInvariant()] = string.Join(",", header.Value);
                    }

                    throw new RetryableCallException("Google embedding request failed.", (int)response.StatusCode, headers);
                }

                using (var document = JsonDocument.Parse(string.IsNullOrEmpty(payload) ? "{}" : payload))
                {
                    var embeddings = new List<double[]>();
                    if (single)
                    {
                        embeddings.Add(ReadVector(document.RootElement.GetProperty("embedding").GetProperty("values")));
                    }
                    else
                    {
                        foreach (var item in document.RootElement.GetProperty("embeddings").EnumerateArray())
                        {
                            embeddings.Add(ReadVector(item.GetProperty("values")));
                        }
                    }

                    return new EmbeddingModelResponse(embeddings, usageOmitted: true, warnings: Array.Empty<OperationWarning>(), response: new ProviderResponse(body: document.RootElement.Clone()));
                }
            }
        }
    }

    private JsonObject SingleBody(string value, JsonElement? content, int? dimensionality)
    {
        var parts = Parts(value, content == null || content.Value.GetArrayLength() == 0 ? (JsonElement?)null : content.Value[0]);
        var body = new JsonObject
        {
            ["model"] = "models/" + ModelId,
            ["content"] = new JsonObject { ["parts"] = parts },
        };
        if (dimensionality != null)
        {
            body["outputDimensionality"] = dimensionality.Value;
        }

        return body;
    }

    private JsonObject BatchBody(IReadOnlyList<string> values, JsonElement? content, int? dimensionality)
    {
        var requests = new JsonArray();
        for (var i = 0; i < values.Count; i++)
        {
            JsonElement? entry = content == null ? (JsonElement?)null : content.Value[i];
            var item = new JsonObject
            {
                ["model"] = "models/" + ModelId,
                ["content"] = new JsonObject
                {
                    ["role"] = "user",
                    ["parts"] = Parts(values[i], entry),
                },
            };
            if (dimensionality != null)
            {
                item["outputDimensionality"] = dimensionality.Value;
            }

            requests.Add(item);
        }

        return new JsonObject { ["requests"] = requests };
    }

    private static JsonArray Parts(string value, JsonElement? entry)
    {
        var parts = new JsonArray();
        if (!string.IsNullOrEmpty(value))
        {
            parts.Add(new JsonObject { ["text"] = value });
        }

        if (entry == null || entry.Value.ValueKind == JsonValueKind.Null)
        {
            if (parts.Count == 0)
            {
                parts.Add(new JsonObject { ["text"] = value ?? string.Empty });
            }

            return parts;
        }

        if (entry.Value.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidArgumentException("providerOptions", entry, "invalid google provider options");
        }

        foreach (var part in entry.Value.EnumerateArray())
        {
            parts.Add(JsonNode.Parse(part.GetRawText()));
        }

        return parts;
    }

    private static void ValidateContent(JsonElement? content)
    {
        if (content == null)
        {
            return;
        }

        foreach (var entry in content.Value.EnumerateArray())
        {
            if (entry.ValueKind == JsonValueKind.Null)
            {
                continue;
            }

            if (entry.ValueKind != JsonValueKind.Array || entry.GetArrayLength() == 0)
            {
                throw new InvalidArgumentException("providerOptions", content, "invalid google provider options");
            }

            foreach (var part in entry.EnumerateArray())
            {
                if (!IsValidPart(part))
                {
                    throw new InvalidArgumentException("providerOptions", content, "invalid google provider options");
                }
            }
        }
    }

    private static bool IsValidPart(JsonElement part)
    {
        if (part.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (part.TryGetProperty("text", out var text))
        {
            var properties = 0;
            foreach (var property in part.EnumerateObject())
            {
                properties++;
                _ = property.Name;
            }

            return text.ValueKind == JsonValueKind.String && properties == 1;
        }

        if (part.TryGetProperty("inlineData", out var inline) && inline.ValueKind == JsonValueKind.Object)
        {
            return inline.TryGetProperty("mimeType", out var mime) && mime.ValueKind == JsonValueKind.String
                && inline.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.String;
        }

        if (part.TryGetProperty("fileData", out var file) && file.ValueKind == JsonValueKind.Object)
        {
            return file.TryGetProperty("fileUri", out var uri) && uri.ValueKind == JsonValueKind.String
                && file.TryGetProperty("mimeType", out var fileType) && fileType.ValueKind == JsonValueKind.String;
        }

        return false;
    }

    private static int? Dimensionality(JsonElement? google)
    {
        if (google == null || !google.Value.TryGetProperty("outputDimensionality", out var value))
        {
            return null;
        }

        if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out var number))
        {
            throw new InvalidArgumentException("providerOptions", google, "invalid google provider options");
        }

        return number;
    }

    private static JsonElement? GoogleOptions(JsonElement? providerOptions)
    {
        if (providerOptions == null || providerOptions.Value.ValueKind != JsonValueKind.Object || !providerOptions.Value.TryGetProperty("google", out var google))
        {
            return null;
        }

        return google;
    }

    private static JsonElement? ContentArray(JsonElement? providerOptions)
    {
        var google = GoogleOptions(providerOptions);
        if (google == null || !google.Value.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        return content;
    }

    private static JsonElement? ContentProperty(JsonElement google)
    {
        if (!google.TryGetProperty("content", out var content) || content.ValueKind == JsonValueKind.Null || content.ValueKind == JsonValueKind.Undefined)
        {
            return null;
        }

        if (content.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidArgumentException("providerOptions", google, "invalid google provider options");
        }

        return content;
    }

    private static double[] ReadVector(JsonElement values)
    {
        var vector = new double[values.GetArrayLength()];
        var index = 0;
        foreach (var number in values.EnumerateArray())
        {
            vector[index++] = number.GetDouble();
        }

        return vector;
    }
}
