// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.GenerateText;
using Vercel.AI.OpenAI;
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
    public override async Task<BatchStartResult> DoStartAsync(BatchOperationCall call, CancellationToken cancellationToken)
    {
        var requests = call.Requests ?? Array.Empty<NormalizedBatchRequest>();
        foreach (var request in requests)
        {
            if (request.Type != "text")
            {
                throw new UnsupportedFunctionalityException("batch request type: " + request.Type, "The Anthropic Message Batches API does not support batch requests with type \"" + request.Type + "\".");
            }
        }

        AnthropicBatch.ValidateRequestIds(requests.Select(request => request.Id));

        var explicitBetas = ProviderBetas(call.ProviderOptions);
        var batchBetas = new List<string>(explicitBetas);
        var warnings = new List<BatchRequestWarning>();
        if (call.WebhookUrl != null)
        {
            warnings.Add(new BatchRequestWarning(null, new OperationWarning("unsupported", feature: "webhookUrl", details: "The Anthropic Message Batches API does not support completion webhooks.")));
        }

        var prepared = new JsonArray();
        foreach (var request in requests)
        {
            var options = request.Options;
            var requestOptions = options.ValueKind == JsonValueKind.Object && options.TryGetProperty("providerOptions", out var po) ? po : (JsonElement?)null;
            if (ProviderBetas(requestOptions).Count > 0)
            {
                throw new UnsupportedFunctionalityException(
                    "per-request providerOptions.anthropic.anthropicBeta",
                    "Anthropic Message Batches do not support per-request betas (request \"" + request.Id + "\"). Set providerOptions.anthropic.anthropicBeta on startBatch instead.");
            }

            var result = AnthropicMessagesRequest.Prepare(
                request.ModelId,
                OpenAIBatchApi.ToCallOptions(options, "Anthropic"),
                false,
                _provider.ModelProvider,
                _provider.SupportsNativeStructuredOutput,
                _provider.SupportsStrictTools,
                AnthropicProvider.ThreadSettings);
            if (result.Body["thread"] != null)
            {
                throw new UnsupportedFunctionalityException(
                    "message threads in batches",
                    "Anthropic Message Batches do not support message threads (request \"" + request.Id + "\").");
            }

            if (result.UsesJsonResponseTool)
            {
                throw new UnsupportedFunctionalityException(
                    "batch responseFormat JSON-tool fallback",
                    "Anthropic Message Batches cannot decode the JSON-tool structured-output fallback (request \"" + request.Id + "\") because batch results are retrieved independently of the start call. Use a model that supports native output_format structured outputs.");
            }

            var body = _provider.TransformRequestBody(result.Body, result.Betas);
            AnthropicBatch.ValidateBody(body, request.Id);
            prepared.Add(new JsonObject { ["custom_id"] = request.Id, ["params"] = body.DeepClone() });
            foreach (var beta in result.Betas)
            {
                if (!batchBetas.Contains(beta))
                {
                    batchBetas.Add(beta);
                }
            }

            foreach (var warning in result.Warnings)
            {
                warnings.Add(new BatchRequestWarning(request.Id, new OperationWarning(warning.Type, warning.Message, warning.Feature, warning.Details)));
            }
        }

        var headers = await _provider.CreateHeadersAsync(batchBetas, call.Headers.ToDictionary(pair => pair.Key, pair => (string?)pair.Value), null, cancellationToken).ConfigureAwait(false);
        var baseUrl = _provider.NormalizedBase().TrimEnd('/');
        var payload = new JsonObject { ["requests"] = prepared }.ToJsonString();
        JsonElement batch;
        using (var document = await _provider.Http.SendJsonAsync(HttpMethod.Post, new Uri(baseUrl + "/messages/batches"), payload, headers, cancellationToken).ConfigureAwait(false))
        {
            batch = document.RootElement.Clone();
        }

        var rawStatus = Str(batch, "processing_status");
        var counts = AnthropicBatch.Count(batch);
        return new BatchStartResult
        {
            Id = Str(batch, "id") ?? string.Empty,
            Status = AnthropicBatch.MapStatus(rawStatus),
            RawStatus = rawStatus,
            RequestCounts = new BatchRequestCounts(counts.Total, counts.Pending, counts.Completed, counts.Failed),
            CreatedAt = Str(batch, "created_at"),
            ExpiresAt = Str(batch, "expires_at"),
            Warnings = warnings,
        };
    }

    private IReadOnlyList<string> ProviderBetas(JsonElement? providerOptions)
    {
        var betas = new List<string>();
        if (providerOptions is not { ValueKind: JsonValueKind.Object } options)
        {
            return betas;
        }

        var name = _provider.ModelProvider.Split('.')[0];
        foreach (var key in name == "anthropic" ? new[] { "anthropic" } : new[] { "anthropic", name })
        {
            if (options.TryGetProperty(key, out var entry) && entry.ValueKind == JsonValueKind.Object
                && entry.TryGetProperty("anthropicBeta", out var list) && list.ValueKind == JsonValueKind.Array)
            {
                foreach (var beta in list.EnumerateArray())
                {
                    if (beta.ValueKind == JsonValueKind.String && !betas.Contains(beta.GetString()!))
                    {
                        betas.Add(beta.GetString()!);
                    }
                }
            }
        }

        return betas;
    }

    /// <inheritdoc />
    public override async Task<IAsyncEnumerable<BatchItem>> DoGetResultsAsync(BatchOperationCall call, CancellationToken cancellationToken)
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

        // The results URL comes from the response. Off the provider origin it must pass download validation and gets no credentials.
        if (!ProviderValues.IsSameOrigin(url, baseUrl))
        {
            DownloadUrls.ValidateDownloadUrl(url);
            headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase) { ["user-agent"] = headers.TryGetValue("user-agent", out var userAgent) ? userAgent : null };
        }

        var lines = await _provider.Http.OpenJsonLinesAsync(HttpMethod.Get, new Uri(url), null, headers, _provider.Options.BatchResultDownloads?.MaxLineBytes, cancellationToken).ConfigureAwait(false);
        return ConvertRows(lines, Convert, cancellationToken);
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
