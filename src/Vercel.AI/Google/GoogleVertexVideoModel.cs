// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Google;

/// <summary>
/// Google Vertex video model (Veo). <see cref="DoStartAsync"/> starts a long-running prediction and
/// <see cref="DoStatusAsync"/> fetches it. The operation is a JSON object with the <c>operationName</c>.
/// </summary>
public sealed class GoogleVertexVideoModel : IVideoModel, IVideoCaller
{
    private static readonly Dictionary<string, string> ResolutionMap = new(StringComparer.Ordinal)
    {
        ["1280x720"] = "720p",
        ["1920x1080"] = "1080p",
        ["3840x2160"] = "4k",
    };

    private static readonly HashSet<string> HandledOptions = new(StringComparer.Ordinal)
    {
        "pollIntervalMs",
        "pollTimeoutMs",
        "personGeneration",
        "negativePrompt",
        "generateAudio",
        "gcsOutputDirectory",
        "referenceImages",
    };

    private readonly GoogleVertexProvider _provider;
    private readonly Func<DateTimeOffset> _clock;

    /// <summary>Creates a model. <paramref name="clock"/> sets the response timestamp.</summary>
    public GoogleVertexVideoModel(
        GoogleVertexProvider provider,
        string modelId,
        Func<DateTimeOffset>? clock = null
    )
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    /// <inheritdoc />
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public string Provider => "google.vertex.video";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Vertex supports multiple videos via <c>sampleCount</c>.</summary>
    public int? MaxVideosPerCall => 4;

    /// <inheritdoc />
    public bool CanGenerate => false;

    /// <inheritdoc />
    public bool CanStart => true;

    /// <inheritdoc />
    public bool CanStatus => true;

    /// <summary>False. Vertex does not send webhooks to this model.</summary>
    public bool CanWebhook => false;

    /// <inheritdoc />
    public Task<int?> ResolveMaxVideosPerCallAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(MaxVideosPerCall);
    }

    /// <summary>Starts an operation and polls it until the video is ready.</summary>
    public async Task<VideoResult> DoGenerateAsync(
        VideoCallOptions options,
        CancellationToken cancellationToken
    )
    {
        options = options ?? throw new ArgumentNullException(nameof(options));
        var result = await GenerateVideo
            .GenerateVideoAsync(
                new GenerateVideoRequest
                {
                    Model = this,
                    Prompt = new VideoPrompt(options.Prompt),
                    Duration = options.DurationSeconds,
                },
                cancellationToken
            )
            .ConfigureAwait(false);
        return new VideoResult(null, result.Video.Data, result.Video.MediaType);
    }

    /// <inheritdoc />
    public Task<VideoModelResult> DoGenerateAsync(
        VideoModelCall call,
        CancellationToken cancellationToken
    )
    {
        throw new NotSupportedException(
            "Google Vertex generates videos with doStart and doStatus."
        );
    }

    /// <inheritdoc />
    public Task<VideoWebhook> HandleWebhookAsync(
        Func<Task<VideoWebhook>> webhook,
        CancellationToken cancellationToken
    )
    {
        throw new NotSupportedException("Google Vertex does not handle webhooks.");
    }

    /// <inheritdoc />
    public async Task<VideoStartResult> DoStartAsync(
        VideoModelCall call,
        CancellationToken cancellationToken
    )
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var timestamp = _clock();
        var (instances, parameters, warnings) = BuildRequest(call);

        var response = await PostAsync(
                ":predictLongRunning",
                new JsonObject { ["instances"] = instances, ["parameters"] = parameters },
                call,
                cancellationToken
            )
            .ConfigureAwait(false);

        var operationName = ReadOperation(response.Body).Name;
        if (string.IsNullOrEmpty(operationName))
        {
            throw new AiSdkException("No operation name returned from API");
        }

        return new VideoStartResult(
            OperationJson.Parse(
                new JsonObject { ["operationName"] = operationName }.ToJsonString()
            ),
            warnings,
            response: Response(response, timestamp)
        );
    }

    /// <inheritdoc />
    public async Task<VideoStatusResult> DoStatusAsync(
        VideoModelCall call,
        CancellationToken cancellationToken
    )
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var timestamp = _clock();
        var operationName =
            call.Operation is JsonElement { ValueKind: JsonValueKind.Object } source
            && source.TryGetProperty("operationName", out var name)
            && name.ValueKind == JsonValueKind.String
                ? name.GetString()
                : null;
        if (string.IsNullOrEmpty(operationName))
        {
            throw new InvalidArgumentException(
                "operation",
                call.Operation,
                "Google Vertex operations are objects with an operationName."
            );
        }

        var response = await PostAsync(
                ":fetchPredictOperation",
                new JsonObject { ["operationName"] = operationName },
                call,
                cancellationToken
            )
            .ConfigureAwait(false);
        var statusOperation = ReadOperation(response.Body);

        if (!statusOperation.Done)
        {
            return new VideoStatusResult("pending", response: Response(response, timestamp));
        }

        if (statusOperation.ErrorMessage != null)
        {
            return new VideoStatusResult(
                "error",
                error: "Video generation failed: " + statusOperation.ErrorMessage,
                response: Response(response, timestamp)
            );
        }

        return BuildCompletedResult(statusOperation, response, [], timestamp);
    }

    private static string? String(JsonElement element, string name)
    {
        return
            element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static VideoModelFile? GetFirstFrameImage(VideoModelCall call)
    {
        return call.FrameImages?.FirstOrDefault(frame => frame.FrameType == "first_frame")?.Image;
    }

    private static VideoModelFile? ResolveStartImage(VideoModelCall call)
    {
        return GetFirstFrameImage(call) ?? call.Image;
    }

    private static VideoModelFile? GetLastFrameImage(VideoModelCall call)
    {
        return call.FrameImages?.FirstOrDefault(frame => frame.FrameType == "last_frame")?.Image;
    }

    private static IReadOnlyList<VideoModelFile>? GetInputReferences(VideoModelCall call)
    {
        if (call.FrameImages is { Count: > 0 })
        {
            return null;
        }

        return call.InputReferences is { Count: > 0 } ? call.InputReferences : null;
    }

    private static JsonObject? ConvertFileToVertexImage(
        VideoModelFile file,
        List<OperationWarning> warnings
    )
    {
        if (file.Type == "url")
        {
            if (file.Url!.StartsWith("gs://", StringComparison.Ordinal))
            {
                return new JsonObject { ["gcsUri"] = file.Url, ["mimeType"] = "image/png" };
            }

            warnings.Add(
                OperationWarning.Unsupported(
                    "URL-based image input",
                    "Vertex AI video models require base64-encoded images or GCS URIs. URL will be ignored."
                )
            );
            return null;
        }

        var base64Data = file.Base64 ?? Convert.ToBase64String(file.Data ?? []);
        return new JsonObject
        {
            ["bytesBase64Encoded"] = base64Data,
            ["mimeType"] = string.IsNullOrEmpty(file.MediaType) ? "image/png" : file.MediaType,
        };
    }

    private static JsonObject? ConvertInputReferenceImage(
        VideoModelFile file,
        List<OperationWarning> warnings
    )
    {
        var image = ConvertFileToVertexImage(file, warnings);
        return image != null
            ? new JsonObject { ["image"] = image, ["referenceType"] = "asset" }
            : null;
    }

    private static Operation ReadOperation(string text)
    {
        try
        {
            using var document = JsonDocument.Parse(text);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                throw new JsonException("The operation is not an object.");
            }

            return new Operation
            {
                Name = String(root, "name"),
                Done =
                    root.TryGetProperty("done", out var done)
                    && done.ValueKind == JsonValueKind.True,
                ErrorMessage =
                    root.TryGetProperty("error", out var error)
                    && error.ValueKind == JsonValueKind.Object
                        ? String(error, "message")
                            ?? throw new JsonException("The operation error has no message.")
                        : null,
                Raw = root.Clone(),
            };
        }
        catch (JsonException)
        {
            throw new ApiException("Invalid JSON response", 200, text);
        }
    }

    private (
        JsonArray Instances,
        JsonObject Parameters,
        List<OperationWarning> Warnings
    ) BuildRequest(VideoModelCall call)
    {
        var warnings = new List<OperationWarning>();
        var googleVertexOptions = GoogleVertexVideoModelOptions.Parse(call.ProviderOptions);

        var instance = new JsonObject();
        if (call.Prompt != null)
        {
            instance["prompt"] = call.Prompt;
        }

        if (ResolveStartImage(call) is { } startImage)
        {
            if (ConvertFileToVertexImage(startImage, warnings) is { } image)
            {
                instance["image"] = image;
            }
        }

        if (GetLastFrameImage(call) is { } lastFrameImage)
        {
            if (ConvertFileToVertexImage(lastFrameImage, warnings) is { } lastFrame)
            {
                instance["lastFrame"] = lastFrame;
            }
        }

        if (GetInputReferences(call) is { } inputReferences)
        {
            var references = new JsonArray();
            foreach (var reference in inputReferences)
            {
                if (ConvertInputReferenceImage(reference, warnings) is { } converted)
                {
                    references.Add(converted);
                }
            }

            instance["referenceImages"] = references;
        }
        else if (googleVertexOptions?.ReferenceImages is { } referenceImages)
        {
            instance["referenceImages"] = JsonNode.Parse(referenceImages.GetRawText());
        }

        var parameters = new JsonObject { ["sampleCount"] = call.N };

        if (!string.IsNullOrEmpty(call.AspectRatio))
        {
            parameters["aspectRatio"] = call.AspectRatio;
        }

        if (!string.IsNullOrEmpty(call.Resolution))
        {
            parameters["resolution"] = ResolutionMap.TryGetValue(call.Resolution, out var mapped)
                ? mapped
                : call.Resolution;
        }

        if (call.Duration is { } duration && duration != 0)
        {
            parameters["durationSeconds"] = duration;
        }

        if (call.Seed != null)
        {
            parameters["seed"] = call.Seed.Value;
        }

        var generateAudio = call.GenerateAudio ?? googleVertexOptions?.GenerateAudio;
        if (generateAudio != null)
        {
            parameters["generateAudio"] = generateAudio.Value;
        }

        if (googleVertexOptions != null)
        {
            if (googleVertexOptions.PersonGeneration != null)
            {
                parameters["personGeneration"] = googleVertexOptions.PersonGeneration;
            }

            if (googleVertexOptions.NegativePrompt != null)
            {
                parameters["negativePrompt"] = googleVertexOptions.NegativePrompt;
            }

            if (googleVertexOptions.GcsOutputDirectory != null)
            {
                parameters["gcsOutputDirectory"] = googleVertexOptions.GcsOutputDirectory;
            }

            foreach (var (key, value) in googleVertexOptions.AdditionalProperties)
            {
                if (!HandledOptions.Contains(key))
                {
                    parameters[key] = JsonNode.Parse(value.GetRawText());
                }
            }
        }

        return (new JsonArray(instance), parameters, warnings);
    }

    private VideoStatusResult BuildCompletedResult(
        Operation finalOperation,
        ProviderTextResponse response,
        List<OperationWarning> warnings,
        DateTimeOffset timestamp
    )
    {
        if (
            !finalOperation.Raw.TryGetProperty("response", out var operationResponse)
            || operationResponse.ValueKind != JsonValueKind.Object
            || !operationResponse.TryGetProperty("videos", out var list)
            || list.ValueKind != JsonValueKind.Array
            || list.GetArrayLength() == 0
        )
        {
            throw new AiSdkException(
                "No videos in response. Response: " + finalOperation.Raw.GetRawText()
            );
        }

        var videos = new List<VideoOutput>();
        var videoMetadata = new JsonArray();
        foreach (var video in list.EnumerateArray())
        {
            var bytes = String(video, "bytesBase64Encoded");
            var gcsUri = String(video, "gcsUri");
            var mimeType = String(video, "mimeType");
            var mediaType = string.IsNullOrEmpty(mimeType) ? "video/mp4" : mimeType;
            if (!string.IsNullOrEmpty(bytes))
            {
                videos.Add(new VideoOutput("base64", data: bytes, mediaType: mediaType));
                videoMetadata.Add(
                    video.TryGetProperty("mimeType", out var type)
                        ? new JsonObject { ["mimeType"] = JsonNode.Parse(type.GetRawText()) }
                        : new JsonObject()
                );
            }
            else if (!string.IsNullOrEmpty(gcsUri))
            {
                videos.Add(new VideoOutput("url", gcsUri, mediaType: mediaType));
                var entry = new JsonObject { ["gcsUri"] = gcsUri };
                if (video.TryGetProperty("mimeType", out var type))
                {
                    entry["mimeType"] = JsonNode.Parse(type.GetRawText());
                }

                videoMetadata.Add(entry);
            }
        }

        if (videos.Count == 0)
        {
            throw new AiSdkException("No valid videos in response");
        }

        var payload = new JsonObject { ["videos"] = videoMetadata };
        var metadata = new JsonObject
        {
            ["googleVertex"] = payload,

            // Legacy keys preserved for backward compatibility.
            ["google-vertex"] = payload.DeepClone(),
            ["vertex"] = payload.DeepClone(),
        };
        return new VideoStatusResult(
            "completed",
            videos,
            warnings: warnings,
            providerMetadata: OperationJson.Parse(metadata.ToJsonString()),
            response: Response(response, timestamp)
        );
    }

    private async Task<ProviderTextResponse> PostAsync(
        string method,
        JsonObject body,
        VideoModelCall call,
        CancellationToken cancellationToken
    )
    {
        var headers = await _provider.HeadersAsync(cancellationToken).ConfigureAwait(false);
        foreach (var header in call.Headers)
        {
            headers[header.Key] = header.Value;
        }

        return await _provider
            .Http.SendJsonStringAsync(
                HttpMethod.Post,
                new Uri(_provider.Options.BaseUrl + "/models/" + ModelId + method),
                body.ToJsonString(),
                headers,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    private ProviderResponse Response(ProviderTextResponse response, DateTimeOffset timestamp)
    {
        return new ProviderResponse(
            response.Headers,
            timestamp: timestamp.UtcDateTime,
            modelId: ModelId
        );
    }

    // The Vertex operation (googleVertexOperationSchema). Every field is nullish.
    private sealed class Operation
    {
        public string? Name { get; set; }

        public bool Done { get; set; }

        public string? ErrorMessage { get; set; }

        public JsonElement Raw { get; set; }
    }
}
