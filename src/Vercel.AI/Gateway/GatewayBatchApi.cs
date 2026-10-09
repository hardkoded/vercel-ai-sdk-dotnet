// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Gateway;

/// <summary>AI Gateway batch API. Downloads the JSONL results of a batch.</summary>
public sealed class GatewayBatchApi : BatchResultsApiBase
{
    private readonly GatewayProvider _provider;

    internal GatewayBatchApi(GatewayProvider provider)
    {
        _provider = provider;
    }

    /// <inheritdoc />
    public override string Provider => "gateway.batch";

    /// <inheritdoc />
    public override async Task<IReadOnlyList<BatchItem>> DoGetResultsAsync(BatchOperationCall call, CancellationToken cancellationToken)
    {
        var headers = _provider.BatchHeaders(call.Headers.ToDictionary(pair => pair.Key, pair => (string?)pair.Value));
        var body = new JsonObject { ["batchId"] = call.BatchId }.ToJsonString();
        try
        {
            var lines = await CollectAsync(_provider.Http.SendJsonLinesAsync(HttpMethod.Post, _provider.Route("batch/results"), body, headers, _provider.Options.BatchResultDownloads?.MaxLineBytes, cancellationToken), cancellationToken).ConfigureAwait(false);
            return lines.Select(Convert).ToList();
        }
        catch (Exception exception) when (!(exception is OperationCanceledException) && !Util.DownloadError.IsInstance(exception) && !(exception is Operations.InvalidArgumentException))
        {
            throw GatewayProvider.MapFailure(exception);
        }
    }

    private static BatchItem Convert(JsonElement line)
    {
        var id = Str(line, "id") ?? string.Empty;
        var status = Str(line, "status") ?? "failed";
        if (status == "succeeded" && line.TryGetProperty("result", out var result))
        {
            var texts = new List<string>();
            if (result.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.Array)
            {
                foreach (var part in content.EnumerateArray())
                {
                    if (Str(part, "type") == "text")
                    {
                        texts.Add(Str(part, "text") ?? string.Empty);
                    }
                }
            }

            var finish = result.TryGetProperty("finishReason", out var reason) ? Str(reason, "unified") : null;
            var response = result.TryGetProperty("response", out var value) ? value : default;
            return TextItem(id, texts, finish, Str(response, "id"), Str(response, "modelId"));
        }

        var item = new BatchItem("text", id, status);
        if (line.TryGetProperty("error", out var error))
        {
            item.ErrorMessage = Str(error, "message");
            item.ErrorCode = Str(error, "code");
        }

        return item;
    }
}
