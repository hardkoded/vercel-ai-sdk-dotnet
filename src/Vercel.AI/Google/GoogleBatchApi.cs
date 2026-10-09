// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Google;

/// <summary>Gemini Batch API. Downloads the file-based JSONL results of a finished batch.</summary>
public sealed class GoogleBatchApi : BatchResultsApiBase
{
    private readonly GoogleProvider _provider;

    internal GoogleBatchApi(GoogleProvider provider)
    {
        _provider = provider;
    }

    /// <inheritdoc />
    public override string Provider => _provider.ModelProvider + ".batch";

    /// <inheritdoc />
    public override async Task<IReadOnlyList<BatchItem>> DoGetResultsAsync(BatchOperationCall call, CancellationToken cancellationToken)
    {
        var batchId = call.BatchId ?? string.Empty;
        var headers = await _provider.HeadersAsync(cancellationToken).ConfigureAwait(false);
        foreach (var pair in call.Headers)
        {
            headers[pair.Key] = pair.Value;
        }

        var baseUrl = _provider.Options.BaseUrl.TrimEnd('/');
        JsonElement operation;
        using (var document = await _provider.Http.SendJsonAsync(HttpMethod.Get, new Uri(baseUrl + "/" + string.Join("/", batchId.Split('/').Select(Uri.EscapeDataString))), null, headers, cancellationToken).ConfigureAwait(false))
        {
            operation = document.RootElement.Clone();
        }

        var done = operation.TryGetProperty("done", out var doneValue) && doneValue.ValueKind == JsonValueKind.True;
        var file = Find(operation, "metadata", "output") ?? Find(operation, "response");
        var responsesFile = file == null ? null : Str(file.Value, "responsesFile");
        if (responsesFile == null)
        {
            if (!done)
            {
                throw new InvalidArgumentException("batchId", batchId, "Google batch \"" + batchId + "\" is not complete.");
            }

            throw new AiSdkException("Google batch \"" + batchId + "\" completed without batch output.");
        }

        var origin = new Uri(baseUrl).GetLeftPart(UriPartial.Authority);
        var encoded = string.Join("/", responsesFile.Split('/').Select(Uri.EscapeDataString));
        var uri = new Uri(origin + "/download/v1beta/" + encoded + ":download?alt=media");
        var lines = await CollectAsync(_provider.Http.SendJsonLinesAsync(HttpMethod.Get, uri, null, headers, _provider.Options.BatchResultDownloads?.MaxLineBytes, cancellationToken), cancellationToken).ConfigureAwait(false);
        return lines.Select(Convert).ToList();
    }

    private static JsonElement? Find(JsonElement root, params string[] path)
    {
        var current = root;
        foreach (var name in path)
        {
            if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(name, out current))
            {
                return null;
            }
        }

        return current;
    }

    private static BatchItem Convert(JsonElement line)
    {
        var id = Str(line, "key") ?? string.Empty;
        if (line.TryGetProperty("response", out var response) && response.ValueKind == JsonValueKind.Object)
        {
            var texts = new List<string>();
            string? finish = null;
            if (response.TryGetProperty("candidates", out var candidates) && candidates.ValueKind == JsonValueKind.Array && candidates.GetArrayLength() > 0)
            {
                var candidate = candidates[0];
                finish = Str(candidate, "finishReason");
                var parts = Find(candidate, "content", "parts");
                if (parts != null && parts.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var part in parts.Value.EnumerateArray())
                    {
                        var text = Str(part, "text");
                        if (text != null)
                        {
                            texts.Add(text);
                        }
                    }
                }
            }

            return TextItem(id, texts, finish, Str(response, "responseId"), Str(response, "modelVersion"));
        }

        var error = line.TryGetProperty("error", out var value) ? value : default;
        return new BatchItem("text", id, "failed") { ErrorMessage = Str(error, "message") };
    }
}
