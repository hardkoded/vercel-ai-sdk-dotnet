// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Google;

/// <summary>
/// Veo video model. <see cref="DoStartAsync"/> posts <c>predictLongRunning</c> and <see cref="DoStatusAsync"/> reads the operation.
/// The operation is a JSON object with the <c>operationName</c>.
/// </summary>
public sealed class GoogleVideoModel : IVideoModel, IVideoCaller
{
    private static readonly Dictionary<string, string> ResolutionMap = new(StringComparer.Ordinal)
    {
        ["1280x720"] = "720p",
        ["1920x1080"] = "1080p",
        ["3840x2160"] = "4k",
    };

    private readonly GoogleProvider _provider;

    /// <summary>Creates a model.</summary>
    public GoogleVideoModel(GoogleProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public string Provider => _provider.ModelProvider;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Google supports multiple videos through <c>sampleCount</c>.</summary>
    public int? MaxVideosPerCall => 4;

    /// <inheritdoc />
    public bool CanGenerate => false;

    /// <inheritdoc />
    public bool CanStart => true;

    /// <inheritdoc />
    public bool CanStatus => true;

    /// <summary>False. Google does not send webhooks to this model.</summary>
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
        throw new NotSupportedException("Google generates videos with doStart and doStatus.");
    }

    /// <inheritdoc />
    public Task<VideoWebhook> HandleWebhookAsync(
        Func<Task<VideoWebhook>> webhook,
        CancellationToken cancellationToken
    )
    {
        throw new NotSupportedException("Google does not handle webhooks.");
    }

    /// <inheritdoc />
    public async Task<VideoStartResult> DoStartAsync(
        VideoModelCall call,
        CancellationToken cancellationToken
    )
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var timestamp = _provider.Clock();
        var (instances, parameters, warnings) = BuildRequest(call);

        var headers = ProviderExchange.Merge(
            await _provider.HeadersAsync(cancellationToken).ConfigureAwait(false),
            call.Headers
        );
        var body = new JsonObject { ["instances"] = instances, ["parameters"] = parameters };
        var response = await _provider
            .Http.SendJsonStringAsync(
                HttpMethod.Post,
                ApiKeys.Combine(
                    _provider.Options.BaseUrl,
                    "/models/" + ModelId + ":predictLongRunning"
                ),
                body.ToJsonString(),
                headers,
                cancellationToken
            )
            .ConfigureAwait(false);

        var operationName = ReadOperation(response.Body).Name;
        if (string.IsNullOrEmpty(operationName))
        {
            throw new AiSdkException("No operation name returned from API");
        }

        var operation = new JsonObject { ["operationName"] = operationName };
        return new VideoStartResult(
            OperationJson.Parse(operation.ToJsonString()),
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
        var timestamp = _provider.Clock();
        var operationName =
            call.Operation is JsonElement { ValueKind: JsonValueKind.Object } operation
            && operation.TryGetProperty("operationName", out var name)
            && name.ValueKind == JsonValueKind.String
                ? name.GetString()
                : null;
        if (string.IsNullOrEmpty(operationName))
        {
            throw new InvalidArgumentException(
                "operation",
                call.Operation,
                "Google operations are objects with an operationName."
            );
        }

        var headers = ProviderExchange.Merge(
            await _provider.HeadersAsync(cancellationToken).ConfigureAwait(false),
            call.Headers
        );
        var response = await _provider
            .Http.SendJsonStringAsync(
                HttpMethod.Get,
                ApiKeys.Combine(_provider.Options.BaseUrl, "/" + operationName),
                null,
                headers,
                cancellationToken
            )
            .ConfigureAwait(false);
        var statusOperation = ReadOperation(response.Body);

        if (statusOperation.Done != true)
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

        return BuildCompletedResult(
            statusOperation,
            response,
            headers,
            [],
            timestamp,
            response.Body
        );
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

    private static JsonObject? ConvertFileToGoogleImage(
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
                    "Google Generative AI video models require base64-encoded images or GCS URIs. URL will be ignored."
                )
            );
            return null;
        }

        var base64Data = file.Base64 ?? Convert.ToBase64String(file.Data ?? []);

        // Veo's predictLongRunning endpoint uses Vertex-style image payloads, not Gemini generateContent inlineData.
        return new JsonObject
        {
            ["bytesBase64Encoded"] = base64Data,
            ["mimeType"] = string.IsNullOrEmpty(file.MediaType) ? "image/png" : file.MediaType,
        };
    }

    private static JsonNode ConvertProviderReferenceImage(GoogleVideoModelReferenceImage refImg)
    {
        if (!string.IsNullOrEmpty(refImg.BytesBase64Encoded))
        {
            return new JsonObject
            {
                ["image"] = new JsonObject
                {
                    ["bytesBase64Encoded"] = refImg.BytesBase64Encoded,
                    ["mimeType"] = "image/png",
                },
                ["referenceType"] = "asset",
            };
        }

        if (!string.IsNullOrEmpty(refImg.GcsUri))
        {
            return new JsonObject
            {
                ["image"] = new JsonObject
                {
                    ["gcsUri"] = refImg.GcsUri,
                    ["mimeType"] = "image/png",
                },
                ["referenceType"] = "asset",
            };
        }

        return JsonNode.Parse(refImg.Raw.GetRawText())!;
    }

    private static JsonObject? ConvertInputReferenceImage(
        VideoModelFile file,
        List<OperationWarning> warnings
    )
    {
        var image = ConvertFileToGoogleImage(file, warnings);
        return image != null
            ? new JsonObject { ["image"] = image, ["referenceType"] = "asset" }
            : null;
    }

    private static GoogleOperation ReadOperation(string text)
    {
        try
        {
            using var document = JsonDocument.Parse(text);
            return GoogleOperation.From(document.RootElement);
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
        var googleOptions = GoogleVideoModelOptions.Parse(call.ProviderOptions);

        var instance = new JsonObject();
        if (call.Prompt != null)
        {
            instance["prompt"] = call.Prompt;
        }

        if (ResolveStartImage(call) is { } startImage)
        {
            if (ConvertFileToGoogleImage(startImage, warnings) is { } image)
            {
                instance["image"] = image;
            }
        }

        if (GetLastFrameImage(call) is { } lastFrameImage)
        {
            if (ConvertFileToGoogleImage(lastFrameImage, warnings) is { } lastFrame)
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
        else if (googleOptions?.ReferenceImages != null)
        {
            var references = new JsonArray();
            foreach (var refImg in googleOptions.ReferenceImages)
            {
                references.Add(ConvertProviderReferenceImage(refImg));
            }

            instance["referenceImages"] = references;
        }

        var parameters = new JsonObject { ["sampleCount"] = call.N };

        if (!string.IsNullOrEmpty(call.AspectRatio))
        {
            parameters["aspectRatio"] = call.AspectRatio;
        }

        if (!string.IsNullOrEmpty(call.Resolution))
        {
            parameters["resolution"] = ResolutionMap.TryGetValue(call.Resolution!, out var mapped)
                ? mapped
                : call.Resolution;
        }

        if (call.Duration is { } duration && duration != 0)
        {
            parameters["durationSeconds"] = duration;
        }

        // A null check, not a truthiness check, so a seed of 0 is sent.
        if (call.Seed != null)
        {
            parameters["seed"] = call.Seed;
        }

        if (googleOptions != null)
        {
            if (googleOptions.PersonGeneration != null)
            {
                parameters["personGeneration"] = googleOptions.PersonGeneration;
            }

            if (googleOptions.NegativePrompt != null)
            {
                parameters["negativePrompt"] = googleOptions.NegativePrompt;
            }

            foreach (var pair in googleOptions.AdditionalProperties)
            {
                parameters[pair.Key] = JsonNode.Parse(pair.Value.GetRawText());
            }
        }

        return ([instance], parameters, warnings);
    }

    private VideoStatusResult BuildCompletedResult(
        GoogleOperation finalOperation,
        ProviderTextResponse response,
        IReadOnlyDictionary<string, string?> resolvedHeaders,
        List<OperationWarning> warnings,
        DateTimeOffset currentDate,
        string rawOperation
    )
    {
        if (finalOperation.GeneratedSamples is not { Count: > 0 } samples)
        {
            throw new AiSdkException("No videos in response. Response: " + rawOperation);
        }

        var videos = new List<VideoOutput>();
        var videoMetadata = new JsonArray();

        // Get the API key from the headers to append to the download URLs.
        resolvedHeaders.TryGetValue("x-goog-api-key", out var apiKey);

        foreach (var uri in samples)
        {
            if (string.IsNullOrEmpty(uri))
            {
                continue;
            }

            // Append the API key to the download URL for authentication, but only when the response-supplied URI
            // stays on the provider's own origin, otherwise the key would leak to whatever host the response names.
            var urlWithAuth =
                !string.IsNullOrEmpty(apiKey)
                && ProviderValues.IsSameOrigin(uri!, _provider.Options.BaseUrl)
                    ? uri + (uri!.Contains('?') ? "&" : "?") + "key=" + apiKey
                    : uri;

            videos.Add(new VideoOutput("url", urlWithAuth, mediaType: "video/mp4"));
            videoMetadata.Add(new JsonObject { ["uri"] = uri });
        }

        if (videos.Count == 0)
        {
            throw new AiSdkException("No valid videos in response");
        }

        var metadata = new JsonObject
        {
            ["google"] = new JsonObject { ["videos"] = videoMetadata },
        };
        return new VideoStatusResult(
            "completed",
            videos,
            warnings: warnings,
            providerMetadata: OperationJson.Parse(metadata.ToJsonString()),
            response: Response(response, currentDate)
        );
    }

    private ProviderResponse Response(ProviderTextResponse response, DateTimeOffset timestamp)
    {
        return new ProviderResponse(
            response.Headers,
            timestamp: timestamp.UtcDateTime,
            modelId: ModelId
        );
    }

    // The Google operation response. Every field is nullish, as in upstream (googleOperationSchema).
    private sealed class GoogleOperation
    {
        public string? Name { get; private set; }

        public bool? Done { get; private set; }

        public string? ErrorMessage { get; private set; }

        public List<string?>? GeneratedSamples { get; private set; }

        public static GoogleOperation From(JsonElement root)
        {
            var operation = new GoogleOperation();
            if (root.ValueKind != JsonValueKind.Object)
            {
                throw new JsonException("The operation is not an object.");
            }

            if (root.TryGetProperty("name", out var name) && name.ValueKind == JsonValueKind.String)
            {
                operation.Name = name.GetString();
            }

            if (
                root.TryGetProperty("done", out var done)
                && done.ValueKind is JsonValueKind.True or JsonValueKind.False
            )
            {
                operation.Done = done.GetBoolean();
            }

            if (
                root.TryGetProperty("error", out var error)
                && error.ValueKind == JsonValueKind.Object
            )
            {
                operation.ErrorMessage =
                    error.TryGetProperty("message", out var message)
                    && message.ValueKind == JsonValueKind.String
                        ? message.GetString()
                        : string.Empty;
            }

            if (
                root.TryGetProperty("response", out var response)
                && response.ValueKind == JsonValueKind.Object
                && response.TryGetProperty("generateVideoResponse", out var generate)
                && generate.ValueKind == JsonValueKind.Object
                && generate.TryGetProperty("generatedSamples", out var samples)
                && samples.ValueKind == JsonValueKind.Array
            )
            {
                operation.GeneratedSamples = [];
                foreach (var sample in samples.EnumerateArray())
                {
                    string? uri = null;
                    if (
                        sample.ValueKind == JsonValueKind.Object
                        && sample.TryGetProperty("video", out var video)
                        && video.ValueKind == JsonValueKind.Object
                        && video.TryGetProperty("uri", out var uriElement)
                        && uriElement.ValueKind == JsonValueKind.String
                    )
                    {
                        uri = uriElement.GetString();
                    }

                    operation.GeneratedSamples.Add(uri);
                }
            }

            return operation;
        }
    }
}
