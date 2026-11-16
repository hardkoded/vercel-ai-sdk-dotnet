// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Google;

/// <summary>Settings for downloading remote files that a tool result returns. Maps to <c>toolResultDownloads</c>.</summary>
public sealed class ToolResultDownloads
{
    /// <summary>Maximum size of each downloaded file in bytes. Defaults to 7 MiB.</summary>
    public long? MaxBytes { get; set; }
}

/// <summary>
/// The <c>downloadToolResultFiles</c> config of the Google language model. <paramref name="SupportsGoogleCloudStorageUrls"/> lets
/// <c>gs://</c> URLs reach the model without a download. <paramref name="Fetch"/> is the transport for untrusted file URLs.
/// </summary>
internal sealed record GoogleToolResultDownloadSettings(
    long MaxBytes,
    bool SupportsGoogleCloudStorageUrls,
    DownloadFetch? Fetch
);

/// <summary>Maps to <c>download-tool-result-files.ts</c>.</summary>
internal static class DownloadToolResultFiles
{
    /// <summary>
    /// Downloads tool result file URLs that Vertex cannot reference directly before converting the prompt to the Google request format.
    /// Files are downloaded one after the other. User files are left alone.
    /// </summary>
    public static async Task<IReadOnlyList<ModelMessage>> DownloadToolResultFilesAsync(
        IReadOnlyList<ModelMessage> prompt,
        CancellationToken abortSignal,
        long maxBytes,
        DownloadFetch? fetch = null,
        IReadOnlyDictionary<string, IReadOnlyList<Regex>>? supportedUrls = null
    )
    {
        var result = new List<ModelMessage>(prompt.Count);
        foreach (var message in prompt)
        {
            if (message is not ToolModelMessage tool)
            {
                result.Add(message);
                continue;
            }

            var changed = false;
            var results = new List<ToolModelMessage>(tool.Results.Count);
            foreach (var part in tool.Results)
            {
                var next = await DownloadToolResultOutputAsync(
                        part,
                        abortSignal,
                        maxBytes,
                        fetch,
                        supportedUrls ?? EmptyUrls
                    )
                    .ConfigureAwait(false);
                changed |= !ReferenceEquals(next, part);
                results.Add(next);
            }

            if (!changed)
            {
                result.Add(message);
            }
            else if (results.Count == 1)
            {
                result.Add(results[0]);
            }
            else
            {
                result.Add(
                    new ToolModelMessage(results)
                    {
                        MessageProviderOptions = tool.MessageProviderOptions,
                    }
                );
            }
        }

        return result;
    }

    private static readonly IReadOnlyDictionary<string, IReadOnlyList<Regex>> EmptyUrls =
        new Dictionary<string, IReadOnlyList<Regex>>();

    private static async Task<ToolModelMessage> DownloadToolResultOutputAsync(
        ToolModelMessage output,
        CancellationToken abortSignal,
        long maxBytes,
        DownloadFetch? fetch,
        IReadOnlyDictionary<string, IReadOnlyList<Regex>> supportedUrls
    )
    {
        if (output.OutputContent is null)
        {
            return output;
        }

        var downloaded = false;
        var value = new List<UserContentPart>(output.OutputContent.Count);
        foreach (var part in output.OutputContent)
        {
            if (
                part
                is not FileContentPart { Url: not null, Data: null, ProviderReference: null } file
            )
            {
                value.Add(part);
                continue;
            }

            if (UrlSupport.IsUrlSupported(file.MediaType, file.Url, supportedUrls))
            {
                value.Add(part);
                continue;
            }

            var blob = await Download
                .GetAsync(new Uri(file.Url), maxBytes, abortSignal, fetch)
                .ConfigureAwait(false);
            var detected = MediaTypes.DetectMediaType(blob.Data, "image");
            var blobType = string.IsNullOrEmpty(blob.MediaType)
                ? null
                : blob.MediaType!.ToLowerInvariant();
            var mediaType = detected
                ?? (
                    blobType != null && !MediaTypes.IsFullMediaType(file.MediaType)
                        ? blobType
                        : file.MediaType
                );
            downloaded = true;
            value.Add(
                new FileContentPart(mediaType, null, blob.Data, file.FileName)
                {
                    ProviderOptions = file.ProviderOptions,
                }
            );
        }

        if (!downloaded)
        {
            return output;
        }

        return new ToolModelMessage(
            output.ToolCallId,
            output.ToolName,
            value,
            output.ProviderMetadata
        )
        {
            ProviderOptions = output.ProviderOptions,
            OutputProviderOptions = output.OutputProviderOptions,
            MessageProviderOptions = output.MessageProviderOptions,
        };
    }
}
