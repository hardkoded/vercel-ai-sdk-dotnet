// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;

namespace Vercel.AI.ProviderUtils;

/// <summary>Settings for downloading JSON Lines batch results.</summary>
public sealed class BatchResultDownloads
{
    /// <summary>Maximum UTF-8 bytes per row, excluding the LF delimiter. Defaults to 64 MiB. Must be positive.</summary>
    public long? MaxLineBytes { get; set; }
}

/// <summary>Batch API that only downloads results. Start, status, cancel, and list are not implemented.</summary>
public abstract class BatchResultsApiBase : IBatchApi
{
    /// <inheritdoc />
    public abstract string Provider { get; }

    /// <inheritdoc />
    public bool CanCancel => false;

    /// <inheritdoc />
    public bool CanList => false;

    /// <inheritdoc />
    public abstract Task<IReadOnlyList<BatchItem>> DoGetResultsAsync(BatchOperationCall call, CancellationToken cancellationToken);

    /// <inheritdoc />
    public Task<BatchStartResult> DoStartAsync(BatchOperationCall call, CancellationToken cancellationToken)
    {
        throw Unsupported("batch start");
    }

    /// <inheritdoc />
    public Task<BatchStatus> DoGetStatusAsync(BatchOperationCall call, CancellationToken cancellationToken)
    {
        throw Unsupported("batch status");
    }

    /// <inheritdoc />
    public Task<BatchCancelResult> DoCancelAsync(BatchOperationCall call, CancellationToken cancellationToken)
    {
        throw Unsupported("batch cancellation");
    }

    /// <inheritdoc />
    public Task<BatchListResult> DoListAsync(BatchOperationCall call, CancellationToken cancellationToken)
    {
        throw Unsupported("batch listing");
    }

    /// <summary>Reads every row of a JSON Lines download.</summary>
    protected static async Task<List<JsonElement>> CollectAsync(IAsyncEnumerable<JsonElement> lines, CancellationToken cancellationToken)
    {
        var rows = new List<JsonElement>();
        await foreach (var line in lines.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            rows.Add(line);
        }

        return rows;
    }

    /// <summary>Text of a string property, or null.</summary>
    protected static string? Str(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    }

    /// <summary>Builds a succeeded text item from joined text parts.</summary>
    protected static BatchItem TextItem(string id, IEnumerable<string> texts, string? finishReason, string? responseId, string? modelId)
    {
        var item = new BatchItem("text", id, "succeeded");
        var parts = new List<BatchContentPart>();
        foreach (var text in texts)
        {
            parts.Add(new BatchContentPart("text") { Text = text });
        }

        item.Text = new BatchTextGeneration { Content = parts, RawFinishReason = finishReason, ResponseId = responseId, ModelId = modelId };
        return item;
    }

    private static UnsupportedFunctionalityException Unsupported(string functionality)
    {
        return new UnsupportedFunctionalityException(functionality, "The provider batch API does not support " + functionality + " yet.");
    }
}
