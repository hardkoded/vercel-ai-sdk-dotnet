// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Replicate;

/// <summary>
/// Replicate video model. <see cref="DoStartAsync"/> creates a prediction and <see cref="DoStatusAsync"/> reads it.
/// A <c>model:version</c> id posts to <c>/predictions</c>; other ids post to <c>/models/{model}/predictions</c>.
/// The operation is a JSON object with the <c>getUrl</c> of the prediction.
/// </summary>
public sealed class ReplicateVideoModel : IVideoModel, IVideoCaller
{
    private readonly ReplicateProvider _provider;
    private readonly Func<DateTimeOffset> _clock;

    /// <summary>Creates a model. <paramref name="clock"/> sets the response timestamp.</summary>
    public ReplicateVideoModel(
        ReplicateProvider provider,
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
    public string Provider => "replicate.video";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Replicate video models create one video per call.</summary>
    public int? MaxVideosPerCall => 1;

    /// <inheritdoc />
    public bool CanGenerate => false;

    /// <inheritdoc />
    public bool CanStart => true;

    /// <inheritdoc />
    public bool CanStatus => true;

    /// <summary>True. The webhook URL is sent as <c>webhook</c>.</summary>
    public bool CanWebhook => true;

    /// <inheritdoc />
    public Task<int?> ResolveMaxVideosPerCallAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(MaxVideosPerCall);
    }

    /// <summary>Starts a prediction and polls it until the video is ready.</summary>
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
        throw new NotSupportedException("Replicate generates videos with doStart and doStatus.");
    }

    // Replicate notifies the caller's URL on completion (DoStartAsync sends it as webhook), so the
    // factory's URL and received task pass straight through.

    /// <inheritdoc />
    public Task<VideoWebhook> HandleWebhookAsync(
        Func<Task<VideoWebhook>> webhook,
        CancellationToken cancellationToken
    )
    {
        return (webhook ?? throw new ArgumentNullException(nameof(webhook)))();
    }

    /// <inheritdoc />
    public async Task<VideoStartResult> DoStartAsync(
        VideoModelCall call,
        CancellationToken cancellationToken
    )
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var timestamp = _clock();
        var (input, warnings) = BuildInput(call);

        var separator = ModelId.IndexOf(':');
        var model = separator < 0 ? ModelId : ModelId.Substring(0, separator);
        var version = separator < 0 ? null : ModelId.Substring(separator + 1);

        var body = new JsonObject { ["input"] = input };
        if (version != null)
        {
            body["version"] = version;
        }

        if (call.WebhookUrl != null)
        {
            body["webhook"] = call.WebhookUrl;
            body["webhook_events_filter"] = new JsonArray("completed");
        }

        var path = version != null ? "/predictions" : "/models/" + model + "/predictions";
        var response = await _provider
            .SendAsync(
                HttpMethod.Post,
                ApiKeys.Combine(_provider.Options.BaseUrl, path),
                ProviderExchange.Json(body.ToJsonString()),
                call.Headers,
                cancellationToken
            )
            .ConfigureAwait(false);

        var prediction = ParsePrediction(response.Body);
        var operation = new JsonObject
        {
            ["getUrl"] = prediction.GetProperty("urls").GetProperty("get").GetString(),
        };
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
        var timestamp = _clock();
        var getUrl =
            call.Operation is JsonElement { ValueKind: JsonValueKind.Object } operation
            && operation.TryGetProperty("getUrl", out var url)
            && url.ValueKind == JsonValueKind.String
                ? url.GetString()
                : null;
        if (string.IsNullOrEmpty(getUrl))
        {
            throw new InvalidArgumentException(
                "operation",
                call.Operation,
                "Replicate operations are objects with a getUrl."
            );
        }

        // getUrl comes from the provider response body, so it is validated and only the provider origin gets credentials.
        var response = await _provider
            .GetResponseUrlAsync(getUrl!, call.Headers, cancellationToken)
            .ConfigureAwait(false);
        var prediction = ParsePrediction(response.Body);
        var status = prediction.GetProperty("status").GetString();

        if (status == "failed")
        {
            return new VideoStatusResult(
                "error",
                error: "Video generation failed: "
                    + (String(prediction, "error") ?? "Unknown error"),
                response: Response(response, timestamp)
            );
        }

        if (status == "canceled")
        {
            return new VideoStatusResult(
                "error",
                error: "Video generation was canceled",
                response: Response(response, timestamp)
            );
        }

        if (status == "succeeded")
        {
            var output = String(prediction, "output");
            if (string.IsNullOrEmpty(output))
            {
                throw new AiSdkException("No video URL in response");
            }

            var metadata = new JsonObject
            {
                ["videos"] = new JsonArray(new JsonObject { ["url"] = output }),
                ["predictionId"] = prediction.GetProperty("id").GetString(),
            };
            if (prediction.TryGetProperty("metrics", out var metrics))
            {
                metadata["metrics"] = JsonNode.Parse(metrics.GetRawText());
            }

            return new VideoStatusResult(
                "completed",
                [new VideoOutput("url", output, mediaType: "video/mp4")],
                warnings: [],
                providerMetadata: OperationJson.Parse(
                    new JsonObject { [ReplicateProvider.ProviderId] = metadata }.ToJsonString()
                ),
                response: Response(response, timestamp)
            );
        }

        // starting or processing
        return new VideoStatusResult("pending", response: Response(response, timestamp));
    }

    private static (JsonObject Input, List<OperationWarning> Warnings) BuildInput(
        VideoModelCall call
    )
    {
        var warnings = new List<OperationWarning>();
        var options = ReplicateVideoModelOptions.Parse(call.ProviderOptions);
        var input = new JsonObject();

        if (call.Prompt != null)
        {
            input["prompt"] = call.Prompt;
        }

        if (call.Image is { } image)
        {
            input["image"] =
                image.Type == "url"
                    ? image.Url
                    : "data:"
                        + image.MediaType
                        + ";base64,"
                        + (image.Base64 ?? Convert.ToBase64String(image.Data ?? []));
        }

        if (!string.IsNullOrEmpty(call.AspectRatio))
        {
            input["aspect_ratio"] = call.AspectRatio;
        }

        if (!string.IsNullOrEmpty(call.Resolution))
        {
            input["size"] = call.Resolution;
        }

        if (call.Duration is { } duration && duration != 0)
        {
            input["duration"] = duration;
        }

        if (call.Fps is { } fps && fps != 0)
        {
            input["fps"] = fps;
        }

        // Seed 0 is a valid seed, so only null is skipped.
        if (call.Seed != null)
        {
            input["seed"] = call.Seed;
        }

        if (options != null)
        {
            Set(input, "guidance_scale", options.GuidanceScale);
            Set(input, "num_inference_steps", options.NumInferenceSteps);
            Set(input, "motion_bucket_id", options.MotionBucketId);
            Set(input, "cond_aug", options.CondAug);
            Set(input, "decoding_t", options.DecodingT);
            Set(input, "video_length", options.VideoLength);
            Set(input, "sizing_strategy", options.SizingStrategy);
            Set(input, "frames_per_second", options.FramesPerSecond);
            Set(input, "prompt_optimizer", options.PromptOptimizer);

            foreach (var pair in options.AdditionalProperties)
            {
                input[pair.Key] = JsonNode.Parse(pair.Value.GetRawText());
            }
        }

        return (input, warnings);
    }

    private static void Set(JsonObject input, string name, double? value)
    {
        if (value != null)
        {
            input[name] = value;
        }
    }

    private static void Set(JsonObject input, string name, string? value)
    {
        if (value != null)
        {
            input[name] = value;
        }
    }

    private static void Set(JsonObject input, string name, bool? value)
    {
        if (value != null)
        {
            input[name] = value;
        }
    }

    // The replicatePredictionSchema: id, status, and urls.get are required.
    private static JsonElement ParsePrediction(string body)
    {
        JsonElement prediction;
        try
        {
            using var document = JsonDocument.Parse(body);
            prediction = document.RootElement.Clone();
        }
        catch (JsonException)
        {
            throw new ApiException("Invalid JSON response", 200, body);
        }

        if (
            prediction.ValueKind != JsonValueKind.Object
            || String(prediction, "id") == null
            || !(
                String(prediction, "status")
                is "starting"
                    or "processing"
                    or "succeeded"
                    or "failed"
                    or "canceled"
            )
            || !prediction.TryGetProperty("urls", out var urls)
            || String(urls, "get") == null
        )
        {
            throw new InvalidResponseDataException(
                prediction,
                "Replicate prediction response is invalid."
            );
        }

        return prediction;
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

    private ProviderResponse Response(ProviderExchangeResult response, DateTimeOffset timestamp)
    {
        return new ProviderResponse(
            response.Headers,
            timestamp: timestamp.UtcDateTime,
            modelId: ModelId
        );
    }
}
