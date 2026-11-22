// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.GenerateText;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Fal;

/// <summary>
/// Fal video model. <see cref="DoStartAsync"/> submits a job to the Fal queue and <see cref="DoStatusAsync"/> reads it.
/// The operation is a JSON object with the <c>responseUrl</c> and the <c>submitUrl</c>.
/// </summary>
public sealed class FalVideoModel : IVideoModel, IVideoCaller
{
    private const string StillInProgress = "Request is still in progress";

    private readonly FalProvider _provider;
    private readonly Func<DateTimeOffset> _clock;

    /// <summary>Creates a model. <paramref name="clock"/> sets the response timestamp. Maps to <c>_internal.currentDate</c>.</summary>
    public FalVideoModel(FalProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    /// <inheritdoc />
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public string Provider => "fal.video";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Fal video models create one video at a time.</summary>
    public int? MaxVideosPerCall => 1;

    /// <inheritdoc />
    public bool CanGenerate => false;

    /// <inheritdoc />
    public bool CanStart => true;

    /// <inheritdoc />
    public bool CanStatus => true;

    /// <summary>True. Fal notifies the webhook URL when the job completes.</summary>
    public bool CanWebhook => true;

    private string NormalizedModelId =>
        FalModelCatalog.GetFalVideoModelEntry(ModelId).Video.NormalizedModelId;

    /// <inheritdoc />
    public Task<int?> ResolveMaxVideosPerCallAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(MaxVideosPerCall);
    }

    /// <summary>Starts a job and polls it until the video is ready.</summary>
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
        throw new NotSupportedException("Fal generates videos with doStart and doStatus.");
    }

    /// <summary>Port of <c>handleWebhookOption</c>. The URL and the received task pass straight through.</summary>
    public async Task<VideoWebhook> HandleWebhookAsync(
        Func<Task<VideoWebhook>> webhook,
        CancellationToken cancellationToken
    )
    {
        var result = await (webhook ?? throw new ArgumentNullException(nameof(webhook)))()
            .ConfigureAwait(false);
        return new VideoWebhook(result.Url, result.Received);
    }

    /// <inheritdoc />
    public async Task<VideoStartResult> DoStartAsync(
        VideoModelCall call,
        CancellationToken cancellationToken
    )
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var timestamp = _clock();
        var warnings = new List<OperationWarning>();

        var body = BuildRequestBody(call);

        var (responseUrl, submitUrl) = await SubmitToQueueAsync(call, body, cancellationToken)
            .ConfigureAwait(false);

        var operation = new JsonObject { ["responseUrl"] = responseUrl, ["submitUrl"] = submitUrl };
        return new VideoStartResult(
            OperationJson.Parse(operation.ToJsonString()),
            warnings,
            response: new ProviderResponse(null, timestamp: timestamp.UtcDateTime, modelId: ModelId)
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
        var operation = call.Operation is JsonElement { ValueKind: JsonValueKind.Object } element
            ? element
            : (JsonElement?)null;
        var responseUrl = operation is { } source ? String(source, "responseUrl") : null;
        var submitUrl = operation is { } origin ? String(origin, "submitUrl") : null;
        if (string.IsNullOrEmpty(responseUrl))
        {
            throw new InvalidArgumentException(
                "operation",
                call.Operation,
                "Fal operations are objects with a responseUrl."
            );
        }

        try
        {
            var response = await FetchStatusAsync(call, responseUrl!, submitUrl, cancellationToken)
                .ConfigureAwait(false);
            return BuildResult(response, timestamp);
        }
        catch (ApiException exception) when (exception.Message == StillInProgress)
        {
            return new VideoStatusResult(
                "pending",
                response: new ProviderResponse(
                    null,
                    timestamp: timestamp.UtcDateTime,
                    modelId: ModelId
                )
            );
        }
        catch (ApiException exception)
        {
            return new VideoStatusResult(
                "error",
                error: exception.Message,
                response: new ProviderResponse(
                    exception.ResponseHeaders,
                    timestamp: timestamp.UtcDateTime,
                    modelId: ModelId
                )
            );
        }
    }

    // Port of encodeURIComponent: it leaves ! * ' ( ) unescaped.
    private static string EncodeUriComponent(string value)
    {
        return Uri.EscapeDataString(value)
            .Replace("%21", "!")
            .Replace("%2A", "*")
            .Replace("%27", "'")
            .Replace("%28", "(")
            .Replace("%29", ")");
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

    private static JsonObject BuildRequestBody(VideoModelCall call)
    {
        var falOptions = FalVideoModelOptions.Parse(call.ProviderOptions);

        var body = new JsonObject();

        if (call.Prompt != null)
        {
            body["prompt"] = call.Prompt;
        }

        if (call.Image is { } image)
        {
            body["image_url"] =
                image.Type == "url"
                    ? image.Url
                    : "data:"
                        + image.MediaType
                        + ";base64,"
                        + (image.Base64 ?? Convert.ToBase64String(image.Data ?? []));
        }

        if (!string.IsNullOrEmpty(call.AspectRatio))
        {
            body["aspect_ratio"] = call.AspectRatio;
        }

        if (call.Duration is { } duration && duration != 0)
        {
            body["duration"] = duration.ToString(CultureInfo.InvariantCulture) + "s";
        }

        if (call.Seed is { } seed)
        {
            body["seed"] = seed;
        }

        if (falOptions != null)
        {
            if (falOptions.Loop is { } loop)
            {
                body["loop"] = loop;
            }

            if (falOptions.MotionStrength is { } motionStrength)
            {
                body["motion_strength"] = motionStrength;
            }

            if (falOptions.Resolution != null)
            {
                body["resolution"] = falOptions.Resolution;
            }

            if (falOptions.NegativePrompt != null)
            {
                body["negative_prompt"] = falOptions.NegativePrompt;
            }

            if (falOptions.PromptOptimizer is { } promptOptimizer)
            {
                body["prompt_optimizer"] = promptOptimizer;
            }

            foreach (var pair in falOptions.AdditionalProperties)
            {
                body[pair.Key] = JsonNode.Parse(pair.Value.GetRawText());
            }
        }

        return body;
    }

    private static ApiException StatusFailedResponseHandler(int statusCode, string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            if (String(document.RootElement, "detail") == StillInProgress)
            {
                return new ApiException(StillInProgress, statusCode, body);
            }
        }
        catch (JsonException)
        {
            // Not JSON, so it is not the in-progress marker.
        }

        return FalError.FailedResponseHandler(statusCode, body);
    }

    private static void CopyIfPresent(
        JsonElement source,
        string name,
        JsonObject target,
        string field
    )
    {
        if (source.ValueKind == JsonValueKind.Object && source.TryGetProperty(name, out var value))
        {
            target[field] = JsonNode.Parse(value.GetRawText());
        }
    }

    private VideoStatusResult BuildResult(ProviderExchangeResult response, DateTimeOffset timestamp)
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(response.Body);
        }
        catch (JsonException)
        {
            throw new ApiException("Invalid JSON response", response.StatusCode, response.Body);
        }

        using (document)
        {
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                throw new ApiException("Invalid JSON response", response.StatusCode, response.Body);
            }

            var hasVideo =
                root.TryGetProperty("video", out var video)
                && video.ValueKind != JsonValueKind.Null;
            if (hasVideo && String(video, "url") == null)
            {
                throw new InvalidResponseDataException(
                    response.Body,
                    "The Fal video response is invalid."
                );
            }

            var videoUrl = hasVideo ? String(video, "url") : null;
            if (string.IsNullOrEmpty(videoUrl))
            {
                throw new AiSdkException("No video URL in response");
            }

            var contentType = String(video, "content_type");
            var videoMetadata = new JsonObject { ["url"] = videoUrl };
            CopyIfPresent(video, "width", videoMetadata, "width");
            CopyIfPresent(video, "height", videoMetadata, "height");
            CopyIfPresent(video, "duration", videoMetadata, "duration");
            CopyIfPresent(video, "fps", videoMetadata, "fps");
            CopyIfPresent(video, "content_type", videoMetadata, "contentType");

            var fal = new JsonObject { ["videos"] = new JsonArray(videoMetadata) };
            CopyIfPresent(root, "seed", fal, "seed");
            if (
                root.TryGetProperty("timings", out var timings)
                && timings.ValueKind == JsonValueKind.Object
            )
            {
                var timingsMetadata = new JsonObject();
                CopyIfPresent(timings, "inference", timingsMetadata, "inference");
                fal["timings"] = timingsMetadata;
            }

            CopyIfPresent(root, "has_nsfw_concepts", fal, "has_nsfw_concepts");
            if (!string.IsNullOrEmpty(String(root, "prompt")))
            {
                fal["prompt"] = String(root, "prompt");
            }

            return new VideoStatusResult(
                "completed",
                [
                    new VideoOutput(
                        "url",
                        videoUrl,
                        mediaType: string.IsNullOrEmpty(contentType) ? "video/mp4" : contentType
                    ),
                ],
                warnings: [],
                providerMetadata: OperationJson.Parse(
                    new JsonObject { [FalProvider.ProviderId] = fal }.ToJsonString()
                ),
                response: new ProviderResponse(
                    response.Headers,
                    timestamp: timestamp.UtcDateTime,
                    modelId: ModelId
                )
            );
        }
    }

    private async Task<ProviderExchangeResult> FetchStatusAsync(
        VideoModelCall call,
        string responseUrl,
        string? submitUrl,
        CancellationToken cancellationToken
    )
    {
        var headers = ProviderExchange.Merge(
            await _provider.CreateHeadersAsync(cancellationToken).ConfigureAwait(false),
            call.Headers
        );

        // The status URL comes from the queue response body, so it is validated and credentials go
        // only to the submit origin.
        return await ProviderExchange
            .GetResponseUrlAsync(
                _provider._httpClient,
                responseUrl,
                submitUrl,
                DownloadUrls.ValidateDownloadUrl,
                headers,
                cancellationToken,
                ProviderExchange.DownloadClient(_provider.Options),
                ProviderExchange.UserAgentOnly(headers),
                failedResponseHandler: StatusFailedResponseHandler
            )
            .ConfigureAwait(false);
    }

    private async Task<(string ResponseUrl, string SubmitUrl)> SubmitToQueueAsync(
        VideoModelCall call,
        JsonObject body,
        CancellationToken cancellationToken
    )
    {
        var queuePath = "https://queue.fal.run/fal-ai/" + NormalizedModelId;
        if (!string.IsNullOrEmpty(call.WebhookUrl))
        {
            queuePath += "?fal_webhook=" + EncodeUriComponent(call.WebhookUrl!);
        }

        var submitUrl = queuePath;
        var response = await _provider
            .SendAsync(
                HttpMethod.Post,
                new Uri(submitUrl),
                ProviderExchange.Json(body.ToJsonString()),
                call.Headers,
                cancellationToken,
                FalError.FailedResponseHandler
            )
            .ConfigureAwait(false);

        string? responseUrl;
        try
        {
            using var document = JsonDocument.Parse(response.Body);
            responseUrl = String(document.RootElement, "response_url");
        }
        catch (JsonException)
        {
            throw new ApiException("Invalid JSON response", response.StatusCode, response.Body);
        }

        if (string.IsNullOrEmpty(responseUrl))
        {
            throw new AiSdkException("No response URL returned from queue endpoint");
        }

        return (responseUrl!, submitUrl);
    }
}
