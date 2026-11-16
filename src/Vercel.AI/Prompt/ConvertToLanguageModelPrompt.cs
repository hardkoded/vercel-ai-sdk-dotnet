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
            converted.Add(message switch
            {
                UserModelMessage user => ConvertUserMessage(user, downloaded),
                ToolModelMessage tool => ConvertToolMessage(tool, downloaded),
                AssistantModelMessage assistant => ConvertAssistantMessage(assistant, downloaded),
                _ => message,
            });
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
            if (message is UserModelMessage user)
            {
                foreach (var part in user.Content)
                {
                    PlanDownload(part, planned, supportedUrls, abortSignal);
                }
            }
            else if (message is ToolModelMessage tool)
            {
                foreach (var toolResult in tool.Results)
                {
                    PlanToolResultDownloads(toolResult, planned, supportedUrls, abortSignal);
                }
            }
            else if (message is AssistantModelMessage assistant)
            {
                foreach (var part in assistant.Parts)
                {
                    if (part is AssistantToolResultPart assistantResult)
                    {
                        PlanToolResultDownloads(assistantResult.Output, planned, supportedUrls, abortSignal);
                    }
                }
            }
        }

        // Like upstream, the download function is called even when the batch is empty.
        var result = new Dictionary<string, DownloadResult>(StringComparer.Ordinal);
        var downloadedFiles = await download(planned).ConfigureAwait(false);
        if (downloadedFiles.Count != planned.Count)
        {
            throw new InvalidOperationException("The download function returned " + downloadedFiles.Count + " results for " + planned.Count + " requested downloads.");
        }

        for (var i = 0; i < downloadedFiles.Count && i < planned.Count; i++)
        {
            if (downloadedFiles[i] is { } file)
            {
                result[planned[i].Url.ToString()] = file;
            }
        }

        return result;
    }

    private static void PlanDownload(UserContentPart part, List<DownloadRequestItem> planned, IReadOnlyDictionary<string, string> supportedUrls, CancellationToken abortSignal)
    {
        if (part is FileContentPart file && TryGetUrl(file, out var url))
        {
            planned.Add(new DownloadRequestItem(url, UrlSupport.IsUrlSupported(file.MediaType, url.ToString(), supportedUrls)) { AbortSignal = abortSignal });
        }
    }

    // Tool results (and provider-executed tool results in assistant messages) with output type "content" carry files that are downloaded like user files.
    private static void PlanToolResultDownloads(ToolModelMessage result, List<DownloadRequestItem> planned, IReadOnlyDictionary<string, string> supportedUrls, CancellationToken abortSignal)
    {
        if (result.OutputContent is null)
        {
            return;
        }

        foreach (var part in result.OutputContent)
        {
            PlanDownload(part, planned, supportedUrls, abortSignal);
        }
    }

    private static ToolModelMessage ConvertToolResult(ToolModelMessage result, IReadOnlyDictionary<string, DownloadResult> downloaded)
    {
        if (result.OutputContent is null)
        {
            return result;
        }

        var changed = false;
        var content = new List<UserContentPart>(result.OutputContent.Count);
        foreach (var part in result.OutputContent)
        {
            var converted = part is FileContentPart file ? ConvertFilePart(file, downloaded) : part;
            changed |= !ReferenceEquals(converted, part);
            content.Add(converted);
        }

        if (!changed)
        {
            return result;
        }

        return new ToolModelMessage(result.ToolCallId, result.ToolName, content, result.ProviderMetadata)
        {
            ProviderOptions = result.ProviderOptions,
            OutputProviderOptions = result.OutputProviderOptions,
            MessageProviderOptions = result.MessageProviderOptions,
        };
    }

    private static ToolModelMessage ConvertToolMessage(ToolModelMessage tool, IReadOnlyDictionary<string, DownloadResult> downloaded)
    {
        if (tool.Results.Count == 1 && ReferenceEquals(tool.Results[0], tool))
        {
            return ConvertToolResult(tool, downloaded);
        }

        var results = new List<ToolModelMessage>(tool.Results.Count);
        var changed = false;
        foreach (var result in tool.Results)
        {
            var converted = ConvertToolResult(result, downloaded);
            changed |= !ReferenceEquals(converted, result);
            results.Add(converted);
        }

        return changed ? new ToolModelMessage(results) { MessageProviderOptions = tool.MessageProviderOptions } : tool;
    }

    private static AssistantModelMessage ConvertAssistantMessage(AssistantModelMessage assistant, IReadOnlyDictionary<string, DownloadResult> downloaded)
    {
        var changed = false;
        var parts = new List<AssistantContentPart>(assistant.Parts.Count);
        foreach (var part in assistant.Parts)
        {
            if (part is AssistantToolResultPart toolResult)
            {
                var output = ConvertToolResult(toolResult.Output, downloaded);
                if (!ReferenceEquals(output, toolResult.Output))
                {
                    changed = true;
                    parts.Add(new AssistantToolResultPart(toolResult.ToolCallId, toolResult.ToolName, output, toolResult.ProviderOptions));
                    continue;
                }
            }

            parts.Add(part);
        }

        return changed
            ? new AssistantModelMessage(assistant.Text, assistant.ToolCalls, assistant.Reasoning, assistant.ReasoningProviderOptions, parts) { ProviderOptions = assistant.ProviderOptions }
            : assistant;
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
