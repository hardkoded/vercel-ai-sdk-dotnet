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
    /// <param name="provider">Provider of the model. It is only used to convert the deprecated <c>file-id</c> and <c>image-file-id</c> tool result parts with a string file id.</param>
    /// <param name="abortSignal">Cancellation for the downloads.</param>
    public static async Task<IReadOnlyList<ModelMessage>> ConvertToLanguageModelPromptAsync(
        IReadOnlyList<ModelMessage> messages,
        IReadOnlyDictionary<string, string> supportedUrls,
        Func<IReadOnlyList<DownloadRequestItem>, Task<IReadOnlyList<DownloadResult>>>? download,
        string? provider = null,
        CancellationToken abortSignal = default
    )
    {
        if (messages is null)
        {
            throw new ArgumentNullException(nameof(messages));
        }

        var downloaded = await DownloadAssetsAsync(
                messages,
                download ?? DefaultDownloadFunction.Create(null, abortSignal),
                supportedUrls ?? UrlSupport.None,
                abortSignal
            )
            .ConfigureAwait(false);
        var converted = new List<ModelMessage>(messages.Count);
        foreach (var message in messages)
        {
            converted.Add(ConvertToLanguageModelMessage(message, downloaded, provider));
        }

        return converted;
    }

    /// <summary>
    /// Converts a message to its language model form. Maps to <c>convertToLanguageModelMessage</c>.
    /// </summary>
    /// <param name="message">The message to convert.</param>
    /// <param name="downloadedAssets">Downloaded data by URL. Only available if the model does not support URLs.</param>
    /// <param name="provider">Provider of the model. It is only used to convert the deprecated <c>file-id</c> and <c>image-file-id</c> tool result parts with a string file id.</param>
    public static ModelMessage ConvertToLanguageModelMessage(
        ModelMessage message,
        IReadOnlyDictionary<string, DownloadResult> downloadedAssets,
        string? provider = null
    )
    {
        var warnings = new List<ModelWarning>();
        var converted = message switch
        {
            UserModelMessage user => ConvertUserMessage(user, downloadedAssets),
            ToolModelMessage tool => ConvertToolMessage(tool, downloadedAssets, provider, warnings),
            AssistantModelMessage assistant => ConvertAssistantMessage(
                assistant,
                downloadedAssets,
                provider,
                warnings
            ),
            _ => message,
        };
        if (warnings.Count > 0)
        {
            LogWarnings.Log(new LogWarningsOptions(warnings));
        }

        return converted;
    }

    /// <summary>Downloads files from URLs in the user messages. Maps to <c>downloadAssets</c>.</summary>
    /// <returns>Downloaded data by URL. Only URLs the download function returned data for.</returns>
    public static async Task<IReadOnlyDictionary<string, DownloadResult>> DownloadAssetsAsync(
        IReadOnlyList<ModelMessage> messages,
        Func<IReadOnlyList<DownloadRequestItem>, Task<IReadOnlyList<DownloadResult>>> download,
        IReadOnlyDictionary<string, string> supportedUrls,
        CancellationToken abortSignal = default
    )
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
                        PlanToolResultDownloads(
                            assistantResult.Output,
                            planned,
                            supportedUrls,
                            abortSignal
                        );
                    }
                }
            }
        }

        // Like upstream, the download function is called even when the batch is empty.
        var result = new Dictionary<string, DownloadResult>(StringComparer.Ordinal);
        var downloadedFiles = await download(planned).ConfigureAwait(false);
        if (downloadedFiles.Count != planned.Count)
        {
            throw new InvalidOperationException(
                "The download function returned "
                    + downloadedFiles.Count
                    + " results for "
                    + planned.Count
                    + " requested downloads."
            );
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

    private static void PlanDownload(
        UserContentPart part,
        List<DownloadRequestItem> planned,
        IReadOnlyDictionary<string, string> supportedUrls,
        CancellationToken abortSignal
    )
    {
        if (part is FileContentPart file && TryGetUrl(file, out var url))
        {
            planned.Add(
                new DownloadRequestItem(
                    url,
                    UrlSupport.IsUrlSupported(file.MediaType, url.ToString(), supportedUrls)
                )
                {
                    AbortSignal = abortSignal,
                }
            );
        }
    }

    // Tool results (and provider-executed tool results in assistant messages) with output type "content" carry files that are downloaded like user files.
    private static void PlanToolResultDownloads(
        ToolModelMessage result,
        List<DownloadRequestItem> planned,
        IReadOnlyDictionary<string, string> supportedUrls,
        CancellationToken abortSignal
    )
    {
        if (result.OutputContent is null)
        {
            return;
        }

        foreach (var part in result.OutputContent)
        {
            PlanDownload(
                GetDownloadableFilePart(part) ?? part,
                planned,
                supportedUrls,
                abortSignal
            );
        }
    }

    // Maps to getDownloadableFilePart: resolves the deprecated "file-url" and "image-url" tool result parts to the file shape.
    private static FileContentPart? GetDownloadableFilePart(UserContentPart part)
    {
        switch (part)
        {
            case FileContentPart file:
                return file;
            case FileUrlContentPart fileUrl:
                return new FileContentPart(
                    fileUrl.MediaType ?? GetMediaTypeFromUrl(fileUrl.Url),
                    fileUrl.Url,
                    null,
                    null
                )
                {
                    ProviderOptions = fileUrl.ProviderOptions,
                };
            case ImageUrlContentPart imageUrl:
                return new FileContentPart("image", imageUrl.Url, null, null)
                {
                    ProviderOptions = imageUrl.ProviderOptions,
                };
            default:
                return null;
        }
    }

    private static ToolModelMessage ConvertToolResult(
        ToolModelMessage result,
        IReadOnlyDictionary<string, DownloadResult> downloaded,
        string? provider,
        List<ModelWarning> warnings
    )
    {
        if (result.OutputContent is null)
        {
            return result;
        }

        var changed = false;
        var content = new List<UserContentPart>(result.OutputContent.Count);
        foreach (var part in result.OutputContent)
        {
            var converted = ConvertToolResultPart(part, downloaded, provider, warnings);
            changed |= !ReferenceEquals(converted, part);
            content.Add(converted);
        }

        if (!changed)
        {
            return result;
        }

        return new ToolModelMessage(
            result.ToolCallId,
            result.ToolName,
            content,
            result.ProviderMetadata
        )
        {
            ProviderOptions = result.ProviderOptions,
            OutputProviderOptions = result.OutputProviderOptions,
            MessageProviderOptions = result.MessageProviderOptions,
        };
    }

    // Maps to mapToolResultOutput for the deprecated tool result content parts.
    private static UserContentPart ConvertToolResultPart(
        UserContentPart part,
        IReadOnlyDictionary<string, DownloadResult> downloaded,
        string? provider,
        List<ModelWarning> warnings
    )
    {
        switch (part)
        {
            case FileContentPart file:
                return ConvertFilePart(file, downloaded);
            case FileUrlContentPart fileUrl:
            {
                var mediaType = fileUrl.MediaType ?? GetMediaTypeFromUrl(fileUrl.Url);
                var message =
                    "The \"file-url\" type for tool result content is deprecated. Use the \"file\" type with mediaType and { type: 'url', url } instead.";
                if (fileUrl.MediaType is null)
                {
                    var inferenceSuffix =
                        mediaType == "application/octet-stream"
                            ? "Unable to infer media type from URL. Defaulting to 'application/octet-stream'."
                            : $"Inferred media type '{mediaType}' from URL.";
                    message =
                        $"The \"file-url\" tool result content part with URL \"{fileUrl.Url}\" is missing a \"mediaType\". {inferenceSuffix} {message}";
                }

                warnings.Add(
                    new DeprecatedWarning("\"tool-result\" content of type \"file-url\"", message)
                );
                return GetDownloadableFilePart(fileUrl) is { } converted
                    ? ConvertFilePart(converted, downloaded)
                    : part;
            }

            case ImageUrlContentPart imageUrl:
                warnings.Add(
                    new DeprecatedWarning(
                        "\"tool-result\" content of type \"image-url\"",
                        "The \"image-url\" type for tool result content is deprecated. Use the \"file\" type with mediaType 'image' (or a specific image/* subtype) and { type: 'url', url } instead."
                    )
                );
                return GetDownloadableFilePart(imageUrl) is { } image
                    ? ConvertFilePart(image, downloaded)
                    : part;
            case FileDataContentPart fileData:
                warnings.Add(DeprecatedData("file-data"));
                return new FileContentPart(
                    fileData.MediaType,
                    null,
                    Convert.FromBase64String(fileData.Data),
                    fileData.FileName
                )
                {
                    ProviderOptions = fileData.ProviderOptions,
                };
            case FileIdContentPart fileId:
                warnings.Add(DeprecatedReference("file-id"));
                return FileContentPart.FromProviderReference(
                    "application",
                    ConvertFileIdToProviderReference(fileId.FileId, fileId.FileIds, provider),
                    null,
                    fileId.ProviderOptions
                );
            case FileReferenceContentPart fileReference:
                warnings.Add(DeprecatedReference("file-reference"));
                return FileContentPart.FromProviderReference(
                    "application",
                    fileReference.ProviderReference,
                    null,
                    fileReference.ProviderOptions
                );
            case ImageDataContentPart imageData:
                warnings.Add(DeprecatedData("image-data"));
                return new FileContentPart(
                    imageData.MediaType,
                    null,
                    Convert.FromBase64String(imageData.Data),
                    null
                )
                {
                    ProviderOptions = imageData.ProviderOptions,
                };
            case ImageFileIdContentPart imageFileId:
                warnings.Add(DeprecatedReference("image-file-id"));
                return FileContentPart.FromProviderReference(
                    "image",
                    ConvertFileIdToProviderReference(
                            imageFileId.FileId,
                            imageFileId.FileIds,
                            provider
                        ),
                    null,
                    imageFileId.ProviderOptions
                );
            case ImageFileReferenceContentPart imageFileReference:
                warnings.Add(DeprecatedReference("image-file-reference"));
                return FileContentPart.FromProviderReference(
                    "image",
                    imageFileReference.ProviderReference,
                    null,
                    imageFileReference.ProviderOptions
                );
            default:
                return part;
        }
    }

    private static DeprecatedWarning DeprecatedData(string type)
    {
        return new DeprecatedWarning(
            "\"tool-result\" content of type \"" + type + "\"",
            "The \"" + type + "\" type for tool result content is deprecated. Use the \"file\" type with mediaType and { type: 'data', data } instead."
        );
    }

    private static DeprecatedWarning DeprecatedReference(string type)
    {
        return new DeprecatedWarning(
            "\"tool-result\" content of type \"" + type + "\"",
            "The \"" + type + "\" type for tool result content is deprecated. Use the \"file\" type with mediaType and { type: 'reference', reference } instead."
        );
    }

    // Maps to convertFileIdToProviderReference.
    private static IReadOnlyDictionary<string, string> ConvertFileIdToProviderReference(
        string? fileId,
        IReadOnlyDictionary<string, string>? fileIds,
        string? provider
    )
    {
        if (fileIds is not null)
        {
            return fileIds;
        }

        if (provider is null)
        {
            throw new InvalidOperationException(
                "Cannot convert string fileId to provider reference without a provider ID. "
                    + "Use a Record<string, string> fileId or switch to the file-reference type."
            );
        }

        return new Dictionary<string, string> { [provider] = fileId! };
    }

    private static readonly Dictionary<string, string> UrlExtensionToMediaType = new(
        StringComparer.Ordinal
    )
    {
        ["jpg"] = "image/jpeg",
        ["jpeg"] = "image/jpeg",
        ["png"] = "image/png",
        ["gif"] = "image/gif",
        ["webp"] = "image/webp",
        ["svg"] = "image/svg+xml",
        ["avif"] = "image/avif",
        ["heic"] = "image/heic",
        ["bmp"] = "image/bmp",
        ["tiff"] = "image/tiff",
        ["tif"] = "image/tiff",
        ["pdf"] = "application/pdf",
        ["mp4"] = "video/mp4",
        ["webm"] = "video/webm",
        ["mp3"] = "audio/mpeg",
        ["wav"] = "audio/wav",
        ["ogg"] = "audio/ogg",
    };

    // Maps to getMediaTypeFromUrl: infers an IANA media type from the file extension in the URL path.
    private static string GetMediaTypeFromUrl(
        string url,
        string fallbackMediaType = "application/octet-stream"
    )
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            var path = uri.AbsolutePath;
            var extension = path.Substring(path.LastIndexOf('.') + 1).ToLowerInvariant();
            if (UrlExtensionToMediaType.TryGetValue(extension, out var mediaType))
            {
                return mediaType;
            }
        }

        return fallbackMediaType;
    }

    private static ToolModelMessage ConvertToolMessage(
        ToolModelMessage tool,
        IReadOnlyDictionary<string, DownloadResult> downloaded,
        string? provider,
        List<ModelWarning> warnings
    )
    {
        if (tool.Results.Count == 1 && ReferenceEquals(tool.Results[0], tool))
        {
            return ConvertToolResult(tool, downloaded, provider, warnings);
        }

        var results = new List<ToolModelMessage>(tool.Results.Count);
        var changed = false;
        foreach (var result in tool.Results)
        {
            var converted = ConvertToolResult(result, downloaded, provider, warnings);
            changed |= !ReferenceEquals(converted, result);
            results.Add(converted);
        }

        return changed
            ? new ToolModelMessage(results) { MessageProviderOptions = tool.MessageProviderOptions }
            : tool;
    }

    private static AssistantModelMessage ConvertAssistantMessage(
        AssistantModelMessage assistant,
        IReadOnlyDictionary<string, DownloadResult> downloaded,
        string? provider,
        List<ModelWarning> warnings
    )
    {
        var changed = false;
        var parts = new List<AssistantContentPart>(assistant.Parts.Count);
        foreach (var part in assistant.Parts)
        {
            if (part is AssistantToolResultPart toolResult)
            {
                var output = ConvertToolResult(toolResult.Output, downloaded, provider, warnings);
                if (!ReferenceEquals(output, toolResult.Output))
                {
                    changed = true;
                    parts.Add(
                        new AssistantToolResultPart(
                            toolResult.ToolCallId,
                            toolResult.ToolName,
                            output,
                            toolResult.ProviderOptions
                        )
                    );
                    continue;
                }
            }

            parts.Add(part);
        }

        return changed
            ? new AssistantModelMessage(
                assistant.Text,
                assistant.ToolCalls,
                assistant.Reasoning,
                assistant.ReasoningProviderOptions,
                parts
            )
            {
                ProviderOptions = assistant.ProviderOptions,
            }
            : assistant;
    }

    private static bool TryGetUrl(FileContentPart file, out Uri url)
    {
        url = null!;
        if (file.Url is null)
        {
            return false;
        }

        var converted = FileParts.ConvertToLanguageModelV4FilePart(
            FilePartInput.FromString(file.Url)
        );
        if (converted.DataType != "url" || converted.Url is null)
        {
            return false;
        }

        url = converted.Url;
        return true;
    }

    private static UserModelMessage ConvertUserMessage(
        UserModelMessage user,
        IReadOnlyDictionary<string, DownloadResult> downloaded
    )
    {
        var content = new List<UserContentPart>(user.Content.Count);
        foreach (var part in user.Content)
        {
            content.Add(part is FileContentPart file ? ConvertFilePart(file, downloaded) : part);
        }

        return new UserModelMessage(content) { ProviderOptions = user.ProviderOptions };
    }

    // Maps to convertPartToLanguageModelPart for a URL that was downloaded.
    private static UserContentPart ConvertFilePart(
        FileContentPart file,
        IReadOnlyDictionary<string, DownloadResult> downloaded
    )
    {
        if (
            !TryGetUrl(file, out var url)
            || !downloaded.TryGetValue(url.ToString(), out var result)
            || result.Data is null
        )
        {
            return file;
        }

        var mediaType = file.MediaType;
        if (
            result.MediaType != null
            && (string.IsNullOrEmpty(mediaType) || !MediaTypes.IsFullMediaType(mediaType))
        )
        {
            mediaType = result.MediaType;
        }

        var imageMediaType = MediaTypes.DetectMediaType(result.Data, "image");
        if (imageMediaType != null)
        {
            mediaType = imageMediaType;
        }

        return new FileContentPart(mediaType, null, result.Data, file.FileName)
        {
            ProviderOptions = file.ProviderOptions,
        };
    }
}
