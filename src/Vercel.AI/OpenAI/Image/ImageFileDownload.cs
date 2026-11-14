// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAICompatible;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenAI;

/// <summary>
/// Downloads an image file named by a URL for an image edit. Maps to <c>downloadBlob</c> called with the
/// <c>downloadFetch</c> transport in <c>openai-image-model.ts</c> and <c>azure-mai-image-model.ts</c>.
/// </summary>
internal static class ImageFileDownload
{
    /// <summary>
    /// The URL is untrusted. The request carries only the user agent, never the provider credentials or headers,
    /// and every redirect hop is validated before it is requested.
    /// </summary>
    public static async Task<(byte[] Data, string MediaType)> DownloadBlobAsync(
        OpenAICompatibleProvider provider,
        IReadOnlyDictionary<string, string?> providerHeaders,
        string url,
        CancellationToken cancellationToken)
    {
        var download = await ProviderExchange.GetResponseUrlAsync(
            provider.SharedHttpClient,
            url,
            null,
            Vercel.AI.GenerateText.DownloadUrls.ValidateDownloadUrl,
            null,
            cancellationToken,
            ProviderExchange.DownloadClient(provider.Options),
            ProviderExchange.UserAgentOnly(providerHeaders)).ConfigureAwait(false);
        return (download.Bytes, download.Headers.TryGetValue("content-type", out var type) ? type : "application/octet-stream");
    }
}
