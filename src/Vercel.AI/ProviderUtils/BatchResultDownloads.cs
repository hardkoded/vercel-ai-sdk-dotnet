// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json;
using Vercel.AI.Operations;

namespace Vercel.AI.ProviderUtils;

/// <summary>Settings for downloading JSON Lines batch results.</summary>
public sealed class BatchResultDownloads
{
    /// <summary>Maximum UTF-8 bytes per row, excluding the LF delimiter. Defaults to 64 MiB. Must be positive.</summary>
    public int? MaxLineBytes { get; set; }
}

/// <summary>Batch API that downloads results. Start, status, cancel, and list throw unless a provider overrides them.</summary>
public abstract class BatchResultsApiBase : IBatchApi
{
    /// <inheritdoc />
    public abstract string Provider { get; }

    /// <inheritdoc />
    public virtual bool CanCancel => false;

    /// <inheritdoc />
    public virtual bool CanList => false;

    /// <inheritdoc />
    public abstract Task<IAsyncEnumerable<BatchItem>> DoGetResultsAsync(BatchOperationCall call, CancellationToken cancellationToken);

    /// <inheritdoc />
    public virtual Task<BatchStartResult> DoStartAsync(BatchOperationCall call, CancellationToken cancellationToken)
    {
        throw Unsupported("batch start");
    }

    /// <inheritdoc />
    public virtual Task<BatchStatus> DoGetStatusAsync(BatchOperationCall call, CancellationToken cancellationToken)
    {
        throw Unsupported("batch status");
    }

    /// <inheritdoc />
    public virtual Task<BatchCancelResult> DoCancelAsync(BatchOperationCall call, CancellationToken cancellationToken)
    {
        throw Unsupported("batch cancellation");
    }

    /// <inheritdoc />
    public virtual Task<BatchListResult> DoListAsync(BatchOperationCall call, CancellationToken cancellationToken)
    {
        throw Unsupported("batch listing");
    }

    /// <summary>Converts each row of a JSON Lines download as it is read.</summary>
    protected static async IAsyncEnumerable<BatchItem> ConvertRows(IAsyncEnumerable<JsonElement> rows, Func<JsonElement, BatchItem> convert, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var row in rows.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            yield return convert(row);
        }
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
