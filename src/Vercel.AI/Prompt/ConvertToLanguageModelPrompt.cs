// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Prompt;

/// <summary>Prompt conversion before a model call. Maps to <c>convert-to-language-model-prompt.ts</c>.</summary>
public static class LanguageModelPrompts
{
    /// <summary>
    /// Downloads the files whose URLs the model cannot read itself and inlines their bytes. Maps to <c>convertToLanguageModelPrompt</c>.
    /// </summary>
    /// <param name="messages">The prompt messages.</param>
    /// <param name="supportedUrls">The model's <see cref="ILanguageModel.SupportedUrls"/>.</param>
    /// <param name="download">Custom download function. When it is null, URLs are downloaded with <see cref="DefaultDownloadFunction"/>.</param>
    /// <param name="abortSignal">Cancellation for the downloads.</param>
    public static async Task<IReadOnlyList<ModelMessage>> ConvertToLanguageModelPromptAsync(
        IReadOnlyList<ModelMessage> messages,
        IReadOnlyDictionary<string, string> supportedUrls,
        Func<IReadOnlyList<DownloadRequestItem>, Task<IReadOnlyList<DownloadResult>>>? download,
        CancellationToken abortSignal = default)
    {
        if (messages is null)
        {
            throw new ArgumentNullException(nameof(messages));
        }

        var downloaded = await DownloadAssetsAsync(
            messages,
            download ?? DefaultDownloadFunction.Create(null, abortSignal),
            supportedUrls ?? UrlSupport.None,
            abortSignal).ConfigureAwait(false);
        if (downloaded.Count == 0)
        {
            return messages;
        }

        var converted = new List<ModelMessage>(messages.Count);
        foreach (var message in messages)
        {
            converted.Add(message is UserModelMessage user ? ConvertUserMessage(user, downloaded) : message);
        }

        return converted;
    }

    /// <summary>Downloads files from URLs in the user messages. Maps to <c>downloadAssets</c>.</summary>
    /// <returns>Downloaded data by URL. Only URLs the download function returned data for.</returns>
    public static async Task<IReadOnlyDictionary<string, DownloadResult>> DownloadAssetsAsync(
        IReadOnlyList<ModelMessage> messages,
        Func<IReadOnlyList<DownloadRequestItem>, Task<IReadOnlyList<DownloadResult>>> download,
        IReadOnlyDictionary<string, string> supportedUrls,
        CancellationToken abortSignal = default)
    {
        var planned = new List<DownloadRequestItem>();
        foreach (var message in messages)
        {
            if (message is not UserModelMessage user)
            {
                continue;
            }

            foreach (var part in user.Content)
            {
                if (part is FileContentPart file && TryGetUrl(file, out var url))
                {
                    planned.Add(new DownloadRequestItem(url, UrlSupport.IsUrlSupported(file.MediaType, url.ToString(), supportedUrls)) { AbortSignal = abortSignal });
                }
            }
        }

        // Like upstream, the download function is called even when the batch is empty.
        var result = new Dictionary<string, DownloadResult>(StringComparer.Ordinal);
        var downloadedFiles = await download(planned).ConfigureAwait(false);
        for (var i = 0; i < downloadedFiles.Count && i < planned.Count; i++)
        {
            if (downloadedFiles[i] is { } file)
            {
                result[planned[i].Url.ToString()] = file;
            }
        }

        return result;
    }

    private static bool TryGetUrl(FileContentPart file, out Uri url)
    {
        url = null!;
        if (file.Url is null)
        {
            return false;
        }

        var converted = FileParts.ConvertToLanguageModelV4FilePart(FilePartInput.FromString(file.Url));
        if (converted.DataType != "url" || converted.Url is null)
        {
            return false;
        }

        url = converted.Url;
        return true;
    }

    private static UserModelMessage ConvertUserMessage(UserModelMessage user, IReadOnlyDictionary<string, DownloadResult> downloaded)
    {
        var content = new List<UserContentPart>(user.Content.Count);
        foreach (var part in user.Content)
        {
            content.Add(part is FileContentPart file ? ConvertFilePart(file, downloaded) : part);
        }

        return new UserModelMessage(content) { ProviderOptions = user.ProviderOptions };
    }

    // Maps to convertPartToLanguageModelPart for a URL that was downloaded.
    private static UserContentPart ConvertFilePart(FileContentPart file, IReadOnlyDictionary<string, DownloadResult> downloaded)
    {
        if (!TryGetUrl(file, out var url) || !downloaded.TryGetValue(url.ToString(), out var result) || result.Data is null)
        {
            return file;
        }

        var mediaType = file.MediaType;
        if (result.MediaType != null && (string.IsNullOrEmpty(mediaType) || !MediaTypes.IsFullMediaType(mediaType)))
        {
            mediaType = result.MediaType;
        }

        var imageMediaType = MediaTypes.DetectMediaType(result.Data, "image");
        if (imageMediaType != null)
        {
            mediaType = imageMediaType;
        }

        return new FileContentPart(mediaType, null, result.Data, file.FileName) { ProviderOptions = file.ProviderOptions };
    }
}
