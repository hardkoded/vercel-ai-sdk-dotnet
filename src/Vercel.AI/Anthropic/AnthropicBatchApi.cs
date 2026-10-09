// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Anthropic;

/// <summary>Anthropic Message Batches API. Downloads the JSONL results of a finished batch.</summary>
public sealed class AnthropicBatchApi : BatchResultsApiBase
{
    private readonly AnthropicProvider _provider;

    internal AnthropicBatchApi(AnthropicProvider provider)
    {
        _provider = provider;
    }

    /// <inheritdoc />
    public override string Provider => _provider.ModelProvider.Replace(".messages", string.Empty) + ".batch";

    /// <inheritdoc />
    public override async Task<IReadOnlyList<BatchItem>> DoGetResultsAsync(BatchOperationCall call, CancellationToken cancellationToken)
    {
        var batchId = call.BatchId ?? string.Empty;
        var headers = await _provider.CreateHeadersAsync(Array.Empty<string>(), call.Headers.ToDictionary(pair => pair.Key, pair => (string?)pair.Value), null, cancellationToken).ConfigureAwait(false);
        var baseUrl = _provider.NormalizedBase().TrimEnd('/');
        JsonElement batch;
        using (var document = await _provider.Http.SendJsonAsync(HttpMethod.Get, new Uri(baseUrl + "/messages/batches/" + Uri.EscapeDataString(batchId)), null, headers, cancellationToken).ConfigureAwait(false))
        {
            batch = document.RootElement.Clone();
        }

        if (AnthropicBatch.ResultsUnavailable(batch))
        {
            throw new InvalidArgumentException("batchId", batchId, "Anthropic batch \"" + batchId + "\" is not complete.");
        }

        if (batch.TryGetProperty("archived_at", out var archived) && archived.ValueKind != JsonValueKind.Null)
        {
            throw new InvalidArgumentException("batchId", batchId, "Anthropic batch \"" + batchId + "\" results are no longer available.");
        }

        var url = Str(batch, "results_url");
        if (url == null)
        {
            throw new AiSdkException("Anthropic batch \"" + batchId + "\" completed without batch output.");
        }

        var lines = await CollectAsync(_provider.Http.SendJsonLinesAsync(HttpMethod.Get, new Uri(url), null, headers, _provider.Options.BatchResultDownloads?.MaxLineBytes, cancellationToken), cancellationToken).ConfigureAwait(false);
        return lines.Select(Convert).ToList();
    }

    private static BatchItem Convert(JsonElement line)
    {
        var id = Str(line, "custom_id") ?? string.Empty;
        var result = line.TryGetProperty("result", out var value) ? value : default;
        switch (Str(result, "type"))
        {
            case "succeeded":
                if (!result.TryGetProperty("message", out var message) || message.ValueKind != JsonValueKind.Object)
                {
                    return new BatchItem("text", id, "failed") { ErrorCode = "invalid_response", ErrorMessage = "Anthropic returned an invalid Message batch result." };
                }

                var texts = new List<string>();
                if (message.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.Array)
                {
                    foreach (var part in content.EnumerateArray())
                    {
                        if (Str(part, "type") == "text")
                        {
                            texts.Add(Str(part, "text") ?? string.Empty);
                        }
                    }
                }

                return TextItem(id, texts, Str(message, "stop_reason"), Str(message, "id"), Str(message, "model"));
            case "canceled":
                return new BatchItem("text", id, "cancelled");
            case "expired":
                return new BatchItem("text", id, "expired");
            default:
                return new BatchItem("text", id, "failed")
                {
                    ErrorMessage = result.ValueKind == JsonValueKind.Object && result.TryGetProperty("error", out var error) ? Str(error, "message") ?? Str(error.TryGetProperty("error", out var inner) ? inner : default, "message") : null,
                };
        }
    }
}
