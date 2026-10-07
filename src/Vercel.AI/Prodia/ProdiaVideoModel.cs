// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.Prodia;

/// <summary>
/// Prodia video model. A prompt alone sends a JSON job (text to video). An input image sends a multipart job
/// (image to video). Image URLs are checked against private addresses before they are downloaded.
/// </summary>
public sealed class ProdiaVideoModel : IVideoModel, IVideoCaller
{
    private readonly ProdiaProvider _provider;
    private readonly Func<DateTimeOffset> _clock;

    /// <summary>Creates a model. <paramref name="clock"/> sets the response timestamp.</summary>
    public ProdiaVideoModel(ProdiaProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    /// <inheritdoc />
    public string Provider => "prodia.video";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Prodia returns one video per job.</summary>
    public int? MaxVideosPerCall => 1;

    /// <inheritdoc />
    public bool CanGenerate => true;

    /// <inheritdoc />
    public bool CanStart => false;

    /// <inheritdoc />
    public bool CanStatus => false;

    /// <inheritdoc />
    public bool CanWebhook => false;

    /// <inheritdoc />
    public Task<int?> ResolveMaxVideosPerCallAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(MaxVideosPerCall);
    }

    /// <inheritdoc />
    public async Task<VideoResult> DoGenerateAsync(VideoCallOptions options, CancellationToken cancellationToken)
    {
        options = options ?? throw new ArgumentNullException(nameof(options));
        var call = new VideoModelCall(options.Prompt, 1, null, null, options.DurationSeconds, null, null, null, null, null, null, OperationJson.Parse("{}"), new Dictionary<string, string>(), cancellationToken);
        var video = (await DoGenerateAsync(call, cancellationToken).ConfigureAwait(false)).Videos[0];
        return new VideoResult(null, (byte[])video.Data!, video.MediaType!);
    }

    /// <inheritdoc />
    public async Task<VideoModelResult> DoGenerateAsync(VideoModelCall call, CancellationToken cancellationToken)
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var config = new JsonObject();
        if (call.Prompt != null)
        {
            config["prompt"] = call.Prompt;
        }

        if (call.Seed != null)
        {
            config["seed"] = call.Seed;
        }

        if (call.ProviderOptions.ValueKind == JsonValueKind.Object
            && call.ProviderOptions.TryGetProperty("prodia", out var prodia)
            && prodia.ValueKind == JsonValueKind.Object
            && prodia.TryGetProperty("resolution", out var resolution))
        {
            if (resolution.ValueKind != JsonValueKind.String)
            {
                throw new InvalidArgumentException("providerOptions", null, "Invalid Prodia provider option resolution.");
            }

            config["resolution"] = resolution.GetString();
        }

        var job = ProdiaApi.Job(ModelId, config);
        HttpContent content;
        if (call.Image != null)
        {
            var (bytes, mediaType) = await ImageAsync(call.Image, cancellationToken).ConfigureAwait(false);
            content = ProdiaApi.Form(job, bytes, mediaType, Extension(mediaType));
        }
        else
        {
            content = ProviderExchange.Json(job);
        }

        var timestamp = _clock();
        var (parts, headers) = await ProdiaApi.PostJobAsync(_provider, content, "multipart/form-data; video/mp4", call.Headers, cancellationToken).ConfigureAwait(false);
        var jobResult = ProdiaApi.ReadJob(parts);
        byte[]? video = null;
        var videoMediaType = "video/mp4";
        foreach (var part in parts)
        {
            if (part.ContentDisposition.Contains("name=\"output\""))
            {
                video = part.Body;
                if (part.ContentType.StartsWith("video/", StringComparison.Ordinal))
                {
                    videoMediaType = part.ContentType;
                }
            }
            else if (part.ContentType.StartsWith("video/", StringComparison.Ordinal))
            {
                video = part.Body;
                videoMediaType = part.ContentType;
            }
        }

        if (video == null)
        {
            throw new InvalidResponseDataException(null, "Prodia multipart response missing output video");
        }

        var metadata = new JsonObject { ["prodia"] = new JsonObject { ["videos"] = new JsonArray(ProdiaApi.Metadata(jobResult)) } };
        return new VideoModelResult(
            new[] { new VideoOutput("binary", data: video, mediaType: videoMediaType) },
            providerMetadata: OperationJson.Parse(metadata.ToJsonString()),
            response: new ProviderResponse(headers, timestamp: timestamp.UtcDateTime, modelId: ModelId));
    }

    /// <inheritdoc />
    public Task<VideoStartResult> DoStartAsync(VideoModelCall call, CancellationToken cancellationToken)
    {
        throw new NotSupportedException("Prodia generates videos in one call.");
    }

    /// <inheritdoc />
    public Task<VideoStatusResult> DoStatusAsync(VideoModelCall call, CancellationToken cancellationToken)
    {
        throw new NotSupportedException("Prodia generates videos in one call.");
    }

    /// <inheritdoc />
    public Task<VideoWebhook> HandleWebhookAsync(Func<Task<VideoWebhook>> webhook, CancellationToken cancellationToken)
    {
        throw new NotSupportedException("Prodia does not support video webhooks.");
    }

    private async Task<(byte[] Bytes, string MediaType)> ImageAsync(VideoModelFile image, CancellationToken cancellationToken)
    {
        if (image.Type == "file")
        {
            return (image.Data!, image.MediaType!);
        }

        Download.ValidateDownloadUrl(image.Url!);
        var response = await ProviderExchange.SendAsync(_provider.HttpClient, HttpMethod.Get, new Uri(image.Url!), null, null, cancellationToken).ConfigureAwait(false);
        var mediaType = response.Headers.TryGetValue("Content-Type", out var value) && value.Length > 0 ? value : "application/octet-stream";
        return (response.Bytes, mediaType);
    }

    private static string Extension(string mediaType)
    {
        return mediaType switch
        {
            "image/png" => ".png",
            "image/jpeg" => ".jpg",
            "image/webp" => ".webp",
            "video/mp4" => ".mp4",
            "video/webm" => ".webm",
            _ => string.Empty,
        };
    }
}
