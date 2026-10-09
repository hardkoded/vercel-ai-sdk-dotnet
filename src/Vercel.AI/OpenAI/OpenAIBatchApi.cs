// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenAI;

/// <summary>OpenAI Batch API. Downloads the JSONL output and error files of a finished batch.</summary>
public sealed class OpenAIBatchApi : BatchResultsApiBase
{
    private readonly OpenAIProvider _provider;

    internal OpenAIBatchApi(OpenAIProvider provider)
    {
        _provider = provider;
    }

    /// <inheritdoc />
    public override string Provider => _provider.Name + ".batch";

    /// <inheritdoc />
    public override async Task<IReadOnlyList<BatchItem>> DoGetResultsAsync(BatchOperationCall call, CancellationToken cancellationToken)
    {
        var batchId = call.BatchId ?? string.Empty;
        var headers = _provider.CreateOpenAIHeaders(call.Headers.ToDictionary(pair => pair.Key, pair => (string?)pair.Value));
        JsonElement batch;
        using (var document = await _provider.Http.SendJsonAsync(HttpMethod.Get, ApiKeys.Combine(_provider.Options.BaseUrl, "batches/" + Uri.EscapeDataString(batchId)), null, headers, cancellationToken).ConfigureAwait(false))
        {
            batch = document.RootElement.Clone();
        }

        var status = Str(batch, "status");
        if (status == "validating" || status == "in_progress" || status == "finalizing" || status == "cancelling")
        {
            throw new InvalidArgumentException("batchId", batchId, "OpenAI batch \"" + batchId + "\" is not complete.");
        }

        var fileIds = new List<string>();
        foreach (var name in new[] { "output_file_id", "error_file_id" })
        {
            var fileId = Str(batch, name);
            if (fileId != null)
            {
                fileIds.Add(fileId);
            }
        }

        if (status == "completed" && fileIds.Count == 0)
        {
            throw new AiSdkException("OpenAI batch \"" + batchId + "\" completed without batch output.");
        }

        var items = new List<BatchItem>();
        foreach (var fileId in fileIds)
        {
            var uri = ApiKeys.Combine(_provider.Options.BaseUrl, "files/" + Uri.EscapeDataString(fileId) + "/content");
            var lines = await CollectAsync(_provider.Http.SendJsonLinesAsync(HttpMethod.Get, uri, null, headers, _provider.MaxBatchLineBytes, cancellationToken), cancellationToken).ConfigureAwait(false);
            items.AddRange(lines.Select(Convert));
        }

        return items;
    }

    private static BatchItem Convert(JsonElement line)
    {
        var id = Str(line, "custom_id") ?? string.Empty;
        if (line.TryGetProperty("response", out var response) && response.ValueKind == JsonValueKind.Object
            && response.TryGetProperty("body", out var body) && response.TryGetProperty("status_code", out var code) && code.ValueKind == JsonValueKind.Number && code.GetInt32() < 400)
        {
            // Upstream converts only the Responses API shape (output[].content[].output_text) for OpenAI batches.
            var texts = new List<string>();
            if (body.TryGetProperty("output", out var output) && output.ValueKind == JsonValueKind.Array)
            {
                foreach (var entry in output.EnumerateArray())
                {
                    if (Str(entry, "type") == "message" && entry.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var part in content.EnumerateArray())
                        {
                            if (Str(part, "type") == "output_text")
                            {
                                texts.Add(Str(part, "text") ?? string.Empty);
                            }
                        }
                    }
                }
            }

            return TextItem(id, texts, null, Str(body, "id"), Str(body, "model"));
        }

        var error = line.TryGetProperty("error", out var value) ? value : default;
        return new BatchItem("text", id, "failed") { ErrorCode = Str(error, "code"), ErrorMessage = Str(error, "message") };
    }
}
