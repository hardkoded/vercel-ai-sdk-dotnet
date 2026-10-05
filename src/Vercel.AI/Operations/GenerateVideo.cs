// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.Operations;

/// <summary>A file or URL passed to a video model.</summary>
public sealed class VideoModelFile
{
    /// <summary>Creates a URL file.</summary>
    public static VideoModelFile FromUrl(string url, string? mediaType = null)
    {
        return new VideoModelFile("url", url, null, mediaType);
    }

    /// <summary>Creates a binary file.</summary>
    public static VideoModelFile FromFile(byte[] data, string mediaType)
    {
        return new VideoModelFile("file", null, data ?? Array.Empty<byte>(), mediaType);
    }

    private VideoModelFile(string type, string? url, byte[]? data, string? mediaType)
    {
        Type = type;
        Url = url;
        Data = data;
        MediaType = mediaType;
    }

    /// <summary><c>url</c> or <c>file</c>.</summary>
    public string Type { get; }

    /// <summary>URL.</summary>
    public string? Url { get; }

    /// <summary>Bytes.</summary>
    public byte[]? Data { get; }

    /// <summary>Media type.</summary>
    public string? MediaType { get; }
}

/// <summary>A role-tagged frame image.</summary>
public sealed class VideoFrameImage
{
    /// <summary>Creates a frame.</summary>
    public VideoFrameImage(object? image, string frameType)
    {
        Image = image;
        FrameType = frameType ?? string.Empty;
    }

    /// <summary>Image bytes, base64, data URL, or http URL.</summary>
    public object? Image { get; }

    /// <summary><c>first_frame</c> or <c>last_frame</c>.</summary>
    public string FrameType { get; }
}

/// <summary>A normalized frame image.</summary>
public sealed class VideoFrameFile
{
    /// <summary>Creates a normalized frame.</summary>
    public VideoFrameFile(VideoModelFile image, string frameType)
    {
        Image = image;
        FrameType = frameType;
    }

    /// <summary>Normalized image.</summary>
    public VideoModelFile Image { get; }

    /// <summary>Frame role.</summary>
    public string FrameType { get; }
}

/// <summary>Reference media, either raw content or an object with a media type.</summary>
public sealed class VideoReference
{
    /// <summary>Creates a raw reference.</summary>
    public VideoReference(object? data)
    {
        Data = data;
    }

    /// <summary>Creates an object-form reference.</summary>
    public VideoReference(object? data, string? mediaType)
    {
        Data = data;
        MediaType = mediaType;
        HasMediaType = true;
    }

    /// <summary>Reference content.</summary>
    public object? Data { get; }

    /// <summary>Explicit media type.</summary>
    public string? MediaType { get; }

    /// <summary>True when the object form was used.</summary>
    public bool HasMediaType { get; }
}

/// <summary>Text prompt or a prompt with a start image.</summary>
public sealed class VideoPrompt
{
    /// <summary>Creates a text prompt.</summary>
    public VideoPrompt(string? text)
    {
        Text = text;
    }

    /// <summary>Creates a prompt with an image.</summary>
    public VideoPrompt(string? text, object? image)
    {
        Text = text;
        Image = image;
        HasImage = true;
    }

    /// <summary>Prompt text.</summary>
    public string? Text { get; }

    /// <summary>Start image.</summary>
    public object? Image { get; }

    /// <summary>True when the object form was used.</summary>
    public bool HasImage { get; }
}

/// <summary>One generated video payload from the model.</summary>
public sealed class VideoOutput
{
    /// <summary>Creates an output.</summary>
    public VideoOutput(string type, string? url = null, object? data = null, string? mediaType = null)
    {
        Type = type;
        Url = url;
        Data = data;
        MediaType = mediaType;
    }

    /// <summary><c>url</c>, <c>base64</c>, or <c>binary</c>.</summary>
    public string Type { get; }

    /// <summary>URL for <c>url</c> outputs.</summary>
    public string? Url { get; }

    /// <summary>Base64 string or bytes.</summary>
    public object? Data { get; }

    /// <summary>Provider media type.</summary>
    public string? MediaType { get; }
}

/// <summary>A generated video file.</summary>
public sealed class GeneratedVideo
{
    /// <summary>Creates a video file.</summary>
    public GeneratedVideo(byte[] data, string mediaType)
    {
        Data = data ?? Array.Empty<byte>();
        MediaType = string.IsNullOrEmpty(mediaType) ? "video/mp4" : mediaType;
    }

    /// <summary>Video bytes.</summary>
    public byte[] Data { get; }

    /// <summary>Base64 form of <see cref="Data"/>.</summary>
    public string Base64
    {
        get { return Convert.ToBase64String(Data); }
    }

    /// <summary>Media type.</summary>
    public string MediaType { get; }
}

/// <summary>Arguments shared by generate, start, and status.</summary>
public sealed class VideoModelCall
{
    /// <summary>Creates a call.</summary>
    public VideoModelCall(string? prompt, int n, string? aspectRatio, string? resolution, double? duration, int? fps, int? seed, VideoModelFile? image, IReadOnlyList<VideoFrameFile>? frameImages, IReadOnlyList<VideoModelFile>? inputReferences, bool? generateAudio, JsonElement providerOptions, IReadOnlyDictionary<string, string> headers, CancellationToken cancellationToken, string? webhookUrl = null, object? operation = null)
    {
        Prompt = prompt;
        N = n;
        AspectRatio = aspectRatio;
        Resolution = resolution;
        Duration = duration;
        Fps = fps;
        Seed = seed;
        Image = image;
        FrameImages = frameImages;
        InputReferences = inputReferences;
        GenerateAudio = generateAudio;
        ProviderOptions = providerOptions;
        Headers = headers;
        CancellationToken = cancellationToken;
        WebhookUrl = webhookUrl;
        Operation = operation;
    }

    /// <summary>Prompt text.</summary>
    public string? Prompt { get; }

    /// <summary>Video count for this call.</summary>
    public int N { get; }

    /// <summary>Aspect ratio.</summary>
    public string? AspectRatio { get; }

    /// <summary>Resolution.</summary>
    public string? Resolution { get; }

    /// <summary>Duration in seconds.</summary>
    public double? Duration { get; }

    /// <summary>Frames per second.</summary>
    public int? Fps { get; }

    /// <summary>Seed.</summary>
    public int? Seed { get; }

    /// <summary>Resolved start image.</summary>
    public VideoModelFile? Image { get; }

    /// <summary>Normalized frames.</summary>
    public IReadOnlyList<VideoFrameFile>? FrameImages { get; }

    /// <summary>Normalized references. Null when frames were provided.</summary>
    public IReadOnlyList<VideoModelFile>? InputReferences { get; }

    /// <summary>Whether to generate audio.</summary>
    public bool? GenerateAudio { get; }

    /// <summary>Provider options.</summary>
    public JsonElement ProviderOptions { get; }

    /// <summary>Headers.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>Cancellation token.</summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>Webhook URL for a start call.</summary>
    public string? WebhookUrl { get; }

    /// <summary>Opaque operation for a status call.</summary>
    public object? Operation { get; }
}

/// <summary>Synchronous <c>doGenerate</c> result or a completed status payload.</summary>
public sealed class VideoModelResult
{
    /// <summary>Creates a result.</summary>
    public VideoModelResult(IReadOnlyList<VideoOutput>? videos, IReadOnlyList<OperationWarning>? warnings = null, JsonElement? providerMetadata = null, ProviderResponse? response = null)
    {
        Videos = videos ?? Array.Empty<VideoOutput>();
        Warnings = warnings ?? Array.Empty<OperationWarning>();
        ProviderMetadata = providerMetadata;
        Response = response ?? new ProviderResponse();
    }

    /// <summary>Videos.</summary>
    public IReadOnlyList<VideoOutput> Videos { get; }

    /// <summary>Warnings.</summary>
    public IReadOnlyList<OperationWarning> Warnings { get; }

    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Response metadata.</summary>
    public ProviderResponse Response { get; }
}

/// <summary>Result of <c>doStart</c>.</summary>
public sealed class VideoStartResult
{
    /// <summary>Creates a start result.</summary>
    public VideoStartResult(object? operation, IReadOnlyList<OperationWarning>? warnings = null, JsonElement? providerMetadata = null, ProviderResponse? response = null)
    {
        Operation = operation;
        Warnings = warnings ?? Array.Empty<OperationWarning>();
        ProviderMetadata = providerMetadata;
        Response = response ?? new ProviderResponse();
    }

    /// <summary>Opaque operation.</summary>
    public object? Operation { get; }

    /// <summary>Warnings.</summary>
    public IReadOnlyList<OperationWarning> Warnings { get; }

    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Response metadata.</summary>
    public ProviderResponse Response { get; }
}

/// <summary>Result of <c>doStatus</c>.</summary>
public sealed class VideoStatusResult
{
    /// <summary>Creates a status result.</summary>
    public VideoStatusResult(string status, IReadOnlyList<VideoOutput>? videos = null, string? error = null, IReadOnlyList<OperationWarning>? warnings = null, JsonElement? providerMetadata = null, ProviderResponse? response = null)
    {
        Status = status ?? string.Empty;
        Videos = videos ?? Array.Empty<VideoOutput>();
        Error = error;
        Warnings = warnings ?? Array.Empty<OperationWarning>();
        ProviderMetadata = providerMetadata;
        Response = response ?? new ProviderResponse();
    }

    /// <summary><c>pending</c>, <c>completed</c>, or <c>error</c>.</summary>
    public string Status { get; }

    /// <summary>Videos when completed.</summary>
    public IReadOnlyList<VideoOutput> Videos { get; }

    /// <summary>Error text when <see cref="Status"/> is <c>error</c>.</summary>
    public string? Error { get; }

    /// <summary>Warnings.</summary>
    public IReadOnlyList<OperationWarning> Warnings { get; }

    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Response metadata.</summary>
    public ProviderResponse Response { get; }
}

/// <summary>Webhook notification.</summary>
public sealed class VideoWebhook
{
    /// <summary>Creates a webhook.</summary>
    public VideoWebhook(string url, Task received)
    {
        Url = url ?? string.Empty;
        Received = received ?? Task.CompletedTask;
    }

    /// <summary>URL the provider should call.</summary>
    public string Url { get; }

    /// <summary>Completes when the notification arrives.</summary>
    public Task Received { get; }
}

/// <summary>Video model used by generate, start, and status.</summary>
public interface IVideoCaller
{
    /// <summary>Provider id.</summary>
    string Provider { get; }

    /// <summary>Model id.</summary>
    string ModelId { get; }

    /// <summary>Maximum videos per call when it is a constant.</summary>
    int? MaxVideosPerCall { get; }

    /// <summary>Resolves a functional limit. The default is <see cref="MaxVideosPerCall"/>.</summary>
    Task<int?> ResolveMaxVideosPerCallAsync(CancellationToken cancellationToken);

    /// <summary>True when <c>doGenerate</c> exists.</summary>
    bool CanGenerate { get; }

    /// <summary>True when <c>doStart</c> exists.</summary>
    bool CanStart { get; }

    /// <summary>True when <c>doStatus</c> exists.</summary>
    bool CanStatus { get; }

    /// <summary>True when <c>handleWebhookOption</c> exists.</summary>
    bool CanWebhook { get; }

    /// <summary>Generates videos in one call.</summary>
    Task<VideoModelResult> DoGenerateAsync(VideoModelCall call, CancellationToken cancellationToken);

    /// <summary>Starts an asynchronous generation.</summary>
    Task<VideoStartResult> DoStartAsync(VideoModelCall call, CancellationToken cancellationToken);

    /// <summary>Reads operation status.</summary>
    Task<VideoStatusResult> DoStatusAsync(VideoModelCall call, CancellationToken cancellationToken);

    /// <summary>Turns a webhook factory into a URL and a received task.</summary>
    Task<VideoWebhook> HandleWebhookAsync(Func<Task<VideoWebhook>> webhook, CancellationToken cancellationToken);
}

/// <summary>Normalized prompt, frames, and references.</summary>
public sealed class VideoCallInputs
{
    /// <summary>Creates normalized inputs.</summary>
    public VideoCallInputs(string? prompt, VideoModelFile? image, IReadOnlyList<VideoFrameFile>? frameImages, IReadOnlyList<VideoModelFile>? inputReferences, IReadOnlyList<OperationWarning> warnings)
    {
        Prompt = prompt;
        Image = image;
        FrameImages = frameImages;
        InputReferences = inputReferences;
        Warnings = warnings ?? Array.Empty<OperationWarning>();
    }

    /// <summary>Prompt text.</summary>
    public string? Prompt { get; }

    /// <summary>Start image. A <c>first_frame</c> wins over the prompt image.</summary>
    public VideoModelFile? Image { get; }

    /// <summary>Frames.</summary>
    public IReadOnlyList<VideoFrameFile>? FrameImages { get; }

    /// <summary>References. Null when frames are present.</summary>
    public IReadOnlyList<VideoModelFile>? InputReferences { get; }

    /// <summary>Normalization warnings.</summary>
    public IReadOnlyList<OperationWarning> Warnings { get; }
}

/// <summary>Result of <see cref="GenerateVideo.GenerateVideoAsync"/>.</summary>
public sealed class GenerateVideoResult
{
    /// <summary>Creates a result.</summary>
    public GenerateVideoResult(IReadOnlyList<GeneratedVideo> videos, IReadOnlyList<OperationWarning> warnings, IReadOnlyList<ProviderResponse> responses, JsonElement providerMetadata)
    {
        Videos = videos ?? Array.Empty<GeneratedVideo>();
        Warnings = warnings ?? Array.Empty<OperationWarning>();
        Responses = responses ?? Array.Empty<ProviderResponse>();
        ProviderMetadata = providerMetadata;
    }

    /// <summary>First video.</summary>
    public GeneratedVideo Video
    {
        get { return Videos[0]; }
    }

    /// <summary>All videos.</summary>
    public IReadOnlyList<GeneratedVideo> Videos { get; }

    /// <summary>Warnings.</summary>
    public IReadOnlyList<OperationWarning> Warnings { get; }

    /// <summary>Responses. Each one carries that call's provider metadata.</summary>
    public IReadOnlyList<ProviderResponse> Responses { get; }

    /// <summary>Merged provider metadata.</summary>
    public JsonElement ProviderMetadata { get; }
}

/// <summary>Result of <see cref="StartVideo.StartVideoAsync"/>.</summary>
public sealed class StartVideoResult
{
    /// <summary>Creates a start result.</summary>
    public StartVideoResult(object? operation, IReadOnlyList<OperationWarning> warnings, JsonElement? providerMetadata, ProviderResponse response)
    {
        Operation = operation;
        Warnings = warnings ?? Array.Empty<OperationWarning>();
        ProviderMetadata = providerMetadata;
        Response = response;
    }

    /// <summary>Opaque operation.</summary>
    public object? Operation { get; }

    /// <summary>Normalization warnings followed by start warnings.</summary>
    public IReadOnlyList<OperationWarning> Warnings { get; }

    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Response metadata.</summary>
    public ProviderResponse Response { get; }
}

/// <summary>Options shared by generate and start.</summary>
public class GenerateVideoRequest : OperationRequest
{
    /// <summary>Video model.</summary>
    public IVideoCaller? Model { get; set; }

    /// <summary>Prompt.</summary>
    public VideoPrompt? Prompt { get; set; }

    /// <summary>Number of videos. Default 1.</summary>
    public double N { get; set; } = 1;

    /// <summary>Overrides the model limit.</summary>
    public int? MaxVideosPerCall { get; set; }

    /// <summary>Aspect ratio.</summary>
    public string? AspectRatio { get; set; }

    /// <summary>Resolution.</summary>
    public string? Resolution { get; set; }

    /// <summary>Duration in seconds.</summary>
    public double? Duration { get; set; }

    /// <summary>Frames per second.</summary>
    public int? Fps { get; set; }

    /// <summary>Seed.</summary>
    public int? Seed { get; set; }

    /// <summary>Frame images.</summary>
    public IReadOnlyList<VideoFrameImage>? FrameImages { get; set; }

    /// <summary>Input references.</summary>
    public IReadOnlyList<VideoReference>? InputReferences { get; set; }

    /// <summary>Whether to generate audio.</summary>
    public bool? GenerateAudio { get; set; }

    /// <summary>Downloads URL videos and reference bytes are already local.</summary>
    public Func<string, CancellationToken, Task<DownloadedMedia>>? Download { get; set; }

    /// <summary>True when polling was requested.</summary>
    public bool Poll { get; set; }

    /// <summary>Poll interval. Default 5000.</summary>
    public int? PollIntervalMs { get; set; }

    /// <summary>Poll or webhook timeout. Default 600000.</summary>
    public int? PollTimeoutMs { get; set; }

    /// <summary>Custom delay. Defaults to <see cref="Task.Delay(int, CancellationToken)"/>.</summary>
    public Func<int, CancellationToken, Task>? Delay { get; set; }

    /// <summary>Webhook factory.</summary>
    public Func<Task<VideoWebhook>>? Webhook { get; set; }

    /// <summary>Webhook URL forwarded by <see cref="StartVideo"/>.</summary>
    public string? WebhookUrl { get; set; }

    /// <summary>Supplies idempotency suffixes. Default is a guid.</summary>
    public Func<string>? GenerateId { get; set; }
}

/// <summary>Options for <see cref="GetVideoStatus.GetVideoStatusAsync"/>.</summary>
public sealed class GetVideoStatusRequest : OperationRequest
{
    /// <summary>Video model.</summary>
    public IVideoCaller? Model { get; set; }

    /// <summary>Operation returned by start.</summary>
    public object? Operation { get; set; }
}

/// <summary>Generates videos. Maps to <c>experimental_generateVideo</c>.</summary>
public static class GenerateVideo
{
    /// <summary>Generates videos, polling or waiting on a webhook when the model requires it.</summary>
    public static async Task<GenerateVideoResult> GenerateVideoAsync(GenerateVideoRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var model = request.Model ?? throw new InvalidArgumentException("model", null, "model is required");
        var token = request.CancellationToken.CanBeCanceled ? request.CancellationToken : cancellationToken;
        var headers = OperationHeaders.WithUserAgent(request.Headers, AiSdkVersion.UserAgent);
        var providerOptions = request.ProviderOptions ?? OperationJson.Parse("{}");
        var inputs = Normalize(request.Prompt, request.FrameImages, request.InputReferences);
        var maxPerCall = request.MaxVideosPerCall ?? await model.ResolveMaxVideosPerCallAsync(token).ConfigureAwait(false) ?? 1;
        var hasStart = model.CanStart && model.CanStatus;
        var useStart = hasStart && (request.Poll || request.Webhook != null || !model.CanGenerate);
        if (!model.CanGenerate && !hasStart)
        {
            throw new InvalidOperationException("Video model " + model.ModelId + " does not implement doGenerate or doStart/doStatus.");
        }

        if ((request.Poll || request.Webhook != null) && !hasStart)
        {
            WarningLog.Write(new[] { OperationWarning.Other("poll/webhook options were provided but the model does not support doStart/doStatus. Falling back to doGenerate.") }, model.Provider, model.ModelId);
        }

        var count = (int)request.N;
        var counts = Split(count, maxPerCall);
        var results = await Task.WhenAll(counts.Select(callCount =>
        {
            var call = new VideoModelCall(inputs.Prompt, callCount, request.AspectRatio, request.Resolution, request.Duration, request.Fps, request.Seed, inputs.Image, inputs.FrameImages, inputs.InputReferences, request.GenerateAudio, providerOptions, headers, token);
            if (useStart)
            {
                return RunStartStatusAsync(model, request, call, token);
            }

            return OperationRetry.ExecuteAsync(request.MaxRetries, token, request.AbortReason, ct => model.DoGenerateAsync(call, ct), null);
        }).ToArray()).ConfigureAwait(false);

        var videos = new List<GeneratedVideo>();
        var warnings = new List<OperationWarning>(inputs.Warnings);
        var responses = new List<ProviderResponse>();
        var metadata = new JsonObject();
        foreach (var result in results)
        {
            foreach (var video in result.Videos)
            {
                videos.Add(await MaterializeAsync(video, request.Download, token).ConfigureAwait(false));
            }

            warnings.AddRange(result.Warnings);
            responses.Add(new ProviderResponse(result.Response.Headers, result.Response.Body, result.Response.Id, result.Response.Timestamp, result.Response.ModelId, result.ProviderMetadata));
            MergeMetadata(metadata, result.ProviderMetadata);
        }

        if (videos.Count == 0)
        {
            throw new NoVideoGeneratedException(responses);
        }

        if (warnings.Count > 0)
        {
            WarningLog.Write(warnings, model.Provider, model.ModelId);
        }

        return new GenerateVideoResult(videos, warnings, responses, OperationJson.Parse(metadata.ToJsonString()));
    }

    /// <summary>Normalizes prompt, frames, and references.</summary>
    public static VideoCallInputs Normalize(VideoPrompt? prompt, IReadOnlyList<VideoFrameImage>? frameImages, IReadOnlyList<VideoReference>? inputReferences)
    {
        string? text = null;
        VideoModelFile? image = null;
        if (prompt != null && prompt.HasImage)
        {
            text = prompt.Text;
            if (prompt.Image != null)
            {
                image = NormalizeFile(prompt.Image, true);
            }
        }
        else if (prompt != null)
        {
            text = prompt.Text;
        }

        IReadOnlyList<VideoFrameFile>? frames = null;
        if (frameImages != null)
        {
            var normalized = new List<VideoFrameFile>();
            foreach (var frame in frameImages)
            {
                var file = NormalizeFile(frame.Image, true);
                if (file != null)
                {
                    normalized.Add(new VideoFrameFile(file, frame.FrameType));
                }
            }

            frames = normalized;
        }

        IReadOnlyList<VideoModelFile>? references = null;
        if (inputReferences != null)
        {
            var normalized = new List<VideoModelFile>();
            foreach (var reference in inputReferences)
            {
                var file = NormalizeFile(reference.Data, false);
                if (file == null)
                {
                    continue;
                }

                if (reference.HasMediaType && reference.MediaType != null)
                {
                    file = file.Type == "url" ? VideoModelFile.FromUrl(file.Url!, reference.MediaType) : VideoModelFile.FromFile(file.Data!, reference.MediaType);
                }

                normalized.Add(file);
            }

            references = normalized;
        }

        var warnings = new List<OperationWarning>();
        var framesPresent = frames != null && frames.Count > 0;
        if (framesPresent && references != null && references.Count > 0)
        {
            warnings.Add(OperationWarning.Other("inputReferences were ignored because frameImages were provided; frameImages and inputReferences cannot be combined."));
        }

        VideoModelFile? first = null;
        if (frames != null)
        {
            foreach (var frame in frames)
            {
                if (frame.FrameType == "first_frame")
                {
                    first = frame.Image;
                    break;
                }
            }
        }

        if (image != null && first != null)
        {
            warnings.Add(OperationWarning.Other("prompt.image was ignored because a first_frame frameImage was provided; the first_frame frameImage takes precedence as the start image."));
        }

        return new VideoCallInputs(text, first ?? image, frames, framesPresent ? null : references, warnings);
    }

    internal static async Task<VideoModelResult> RunStartStatusAsync(IVideoCaller model, GenerateVideoRequest request, VideoModelCall call, CancellationToken cancellationToken)
    {
        var warnings = new List<OperationWarning>();
        string? webhookUrl = null;
        Task? received = null;
        if (request.Webhook != null)
        {
            if (model.CanWebhook)
            {
                var handle = await model.HandleWebhookAsync(request.Webhook, cancellationToken).ConfigureAwait(false);
                received = handle.Received;
                _ = received.ContinueWith(_ => { }, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
                webhookUrl = handle.Url;
            }
            else
            {
                warnings.Add(OperationWarning.Unsupported("webhook", "This model does not support webhooks. Falling back to polling."));
            }
        }

        var headers = WithIdempotency(call.Headers, request.GenerateId);
        var startCall = new VideoModelCall(call.Prompt, call.N, call.AspectRatio, call.Resolution, call.Duration, call.Fps, call.Seed, call.Image, call.FrameImages, call.InputReferences, call.GenerateAudio, call.ProviderOptions, headers, cancellationToken, webhookUrl);
        var started = await OperationRetry.ExecuteAsync(request.MaxRetries, cancellationToken, request.AbortReason, ct => model.DoStartAsync(startCall, ct), null).ConfigureAwait(false);
        warnings.AddRange(started.Warnings);
        var metadata = started.ProviderMetadata == null ? null : JsonNode.Parse(started.ProviderMetadata.Value.GetRawText()) as JsonObject;
        var interval = request.PollIntervalMs ?? 5000;
        var timeout = request.PollTimeoutMs ?? 600000;
        var delay = request.Delay ?? ((ms, ct) => Task.Delay(ms, ct));
        var clock = Stopwatch.StartNew();
        var timeoutError = new TimeoutException("Video generation timed out after " + timeout.ToString(CultureInfo.InvariantCulture) + "ms.");
        if (received != null)
        {
            await WaitForWebhookAsync(received, timeout, cancellationToken, delay, timeoutError).ConfigureAwait(false);
        }

        while (true)
        {
            if (received == null)
            {
                var elapsed = (int)clock.ElapsedMilliseconds;
                if (elapsed >= timeout)
                {
                    throw timeoutError;
                }

                await delay(Math.Min(interval, timeout - elapsed), cancellationToken).ConfigureAwait(false);
                if (clock.ElapsedMilliseconds >= timeout)
                {
                    throw timeoutError;
                }
            }

            VideoStatusResult status;
            if (received != null)
            {
                status = await OperationRetry.ExecuteAsync(request.MaxRetries, cancellationToken, request.AbortReason, ct => model.DoStatusAsync(new VideoModelCall(null, 0, null, null, null, null, null, null, null, null, null, OperationJson.Parse("{}"), call.Headers, ct, operation: started.Operation), ct), null).ConfigureAwait(false);
            }
            else
            {
                var remaining = timeout - (int)clock.ElapsedMilliseconds;
                using (var timeoutSource = new CancellationTokenSource())
                using (var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token))
                {
                    var statusTask = OperationRetry.ExecuteAsync(request.MaxRetries, linked.Token, request.AbortReason, ct => model.DoStatusAsync(new VideoModelCall(null, 0, null, null, null, null, null, null, null, null, null, OperationJson.Parse("{}"), call.Headers, ct, operation: started.Operation), ct), null);
                    var timeoutTask = delay(Math.Max(remaining, 0), linked.Token);
                    try
                    {
                        var finished = await Task.WhenAny(statusTask, timeoutTask).ConfigureAwait(false);
                        if (finished != statusTask)
                        {
                            timeoutSource.Cancel();
                            throw timeoutError;
                        }

                        status = await statusTask.ConfigureAwait(false);
                    }
                    catch (Exception) when (timeoutSource.IsCancellationRequested || clock.ElapsedMilliseconds >= timeout)
                    {
                        throw timeoutError;
                    }
                    finally
                    {
                        timeoutSource.Cancel();
                    }
                }
            }

            if (status.Status == "error")
            {
                throw new InvalidOperationException(status.Error);
            }

            warnings.AddRange(status.Warnings);
            if (status.ProviderMetadata != null)
            {
                metadata ??= new JsonObject();
                MergeMetadata(metadata, status.ProviderMetadata);
            }

            if (status.Status == "completed")
            {
                return new VideoModelResult(status.Videos, warnings, metadata == null ? null : OperationJson.Parse(metadata.ToJsonString()), status.Response);
            }

            if (received != null)
            {
                throw new InvalidOperationException("Video generation did not complete after webhook notification.");
            }
        }
    }

    private static async Task WaitForWebhookAsync(Task received, int timeout, CancellationToken cancellationToken, Func<int, CancellationToken, Task> delay, Exception timeoutError)
    {
        using (var timeoutSource = new CancellationTokenSource())
        using (var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token))
        {
            try
            {
                var delayTask = delay(timeout, linked.Token);
                var finished = await Task.WhenAny(received, delayTask).ConfigureAwait(false);
                if (finished != received)
                {
                    throw timeoutError;
                }

                await received.ConfigureAwait(false);
            }
            finally
            {
                timeoutSource.Cancel();
            }
        }
    }

    private static async Task<GeneratedVideo> MaterializeAsync(VideoOutput video, Func<string, CancellationToken, Task<DownloadedMedia>>? download, CancellationToken cancellationToken)
    {
        if (video.Type == "url")
        {
            if (download == null)
            {
                throw new InvalidArgumentException("download", null, "download is required for URL videos");
            }

            var downloaded = await download(video.Url ?? string.Empty, cancellationToken).ConfigureAwait(false);
            var mediaType = Usable(video.MediaType) ?? Usable(downloaded.MediaType) ?? MediaTypeDetector.Detect(downloaded.Data, "video") ?? "video/mp4";
            return new GeneratedVideo(downloaded.Data, mediaType);
        }

        if (video.Type == "base64")
        {
            return new GeneratedVideo(MediaTypeDetector.ToBytes(video.Data), string.IsNullOrEmpty(video.MediaType) ? "video/mp4" : video.MediaType!);
        }

        var bytes = MediaTypeDetector.ToBytes(video.Data);
        return new GeneratedVideo(bytes, string.IsNullOrEmpty(video.MediaType) ? MediaTypeDetector.Detect(bytes, "video") ?? "video/mp4" : video.MediaType!);
    }

    private static string? Usable(string? mediaType)
    {
        return !string.IsNullOrEmpty(mediaType) && mediaType != "application/octet-stream" ? mediaType : null;
    }

    internal static IReadOnlyDictionary<string, string> WithIdempotency(IReadOnlyDictionary<string, string> headers, Func<string>? generateId)
    {
        foreach (var pair in headers)
        {
            if (string.Equals(pair.Key, "idempotency-key", StringComparison.OrdinalIgnoreCase))
            {
                return headers;
            }
        }

        var copy = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var header in headers)
        {
            copy[header.Key] = header.Value;
        }
        copy["idempotency-key"] = "aisdk_vid_" + (generateId ?? (() => Guid.NewGuid().ToString("N")))();
        return copy;
    }

    private static List<int> Split(int n, int maxPerCall)
    {
        var count = (int)Math.Ceiling(n / (double)Math.Max(maxPerCall, 1));
        var counts = new List<int>(count);
        for (var index = 0; index < count; index++)
        {
            var remaining = n - (index * maxPerCall);
            counts.Add(Math.Min(remaining, maxPerCall));
        }

        return counts;
    }

    private static VideoModelFile? NormalizeFile(object? data, bool imagesOnly)
    {
        if (data == null)
        {
            return null;
        }

        if (data is string text)
        {
            if (text.StartsWith("http://", StringComparison.Ordinal) || text.StartsWith("https://", StringComparison.Ordinal))
            {
                return VideoModelFile.FromUrl(text);
            }

            if (text.StartsWith("data:", StringComparison.Ordinal))
            {
                var comma = text.IndexOf(',');
                var header = comma < 0 ? text : text.Substring(0, comma);
                var payload = comma < 0 ? string.Empty : text.Substring(comma + 1);
                var media = header.Substring("data:".Length);
                var separator = media.IndexOf(';');
                if (separator >= 0)
                {
                    media = media.Substring(0, separator);
                }

                var bytes = string.IsNullOrEmpty(payload) ? Array.Empty<byte>() : Convert.FromBase64String(payload);
                return VideoModelFile.FromFile(bytes, media.Length == 0 ? Detect(bytes, imagesOnly) : media);
            }

            var decoded = Convert.FromBase64String(text);
            return VideoModelFile.FromFile(decoded, Detect(decoded, imagesOnly));
        }

        var raw = MediaTypeDetector.ToBytes(data);
        return VideoModelFile.FromFile(raw, Detect(raw, imagesOnly));
    }

    private static string Detect(byte[] data, bool imagesOnly)
    {
        return (imagesOnly ? MediaTypeDetector.Detect(data, "image") : MediaTypeDetector.Detect(data, null)) ?? "image/png";
    }

    private static void MergeMetadata(JsonObject target, JsonElement? source)
    {
        if (source is not JsonElement root || root.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        foreach (var provider in root.EnumerateObject())
        {
            var incoming = JsonNode.Parse(provider.Value.GetRawText());
            if (target[provider.Name] is JsonObject existing && incoming is JsonObject next)
            {
                var merged = new JsonObject();
                foreach (var pair in existing)
                {
                    merged[pair.Key] = pair.Value?.DeepClone();
                }

                foreach (var pair in next)
                {
                    merged[pair.Key] = pair.Value?.DeepClone();
                }

                if (existing["videos"] is JsonArray left && next["videos"] is JsonArray right)
                {
                    var videos = new JsonArray();
                    foreach (var item in left)
                    {
                        videos.Add(item?.DeepClone());
                    }

                    foreach (var item in right)
                    {
                        videos.Add(item?.DeepClone());
                    }

                    merged["videos"] = videos;
                }

                target[provider.Name] = merged;
            }
            else if (incoming != null)
            {
                target[provider.Name] = incoming;
            }
        }
    }
}

/// <summary>Starts a video generation. Maps to <c>experimental_startVideo</c>.</summary>
public static class StartVideo
{
    /// <summary>Starts one operation and returns without waiting.</summary>
    public static async Task<StartVideoResult> StartVideoAsync(GenerateVideoRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var model = request.Model ?? throw new InvalidArgumentException("model", null, "model is required");
        if (!model.CanStart)
        {
            throw new InvalidOperationException("Video model " + model.ModelId + " does not implement doStart. Use generateVideo for models without an asynchronous start/status flow.");
        }

        if (request.N != Math.Truncate(request.N) || request.N < 1)
        {
            throw new InvalidOperationException("Invalid n: expected a positive integer, received " + request.N.ToString(CultureInfo.InvariantCulture) + ".");
        }

        var token = request.CancellationToken.CanBeCanceled ? request.CancellationToken : cancellationToken;
        var limit = request.MaxVideosPerCall ?? await model.ResolveMaxVideosPerCallAsync(token).ConfigureAwait(false);
        if (limit != null && request.N > limit.Value)
        {
            throw new InvalidOperationException("Video model " + model.ModelId + " supports at most " + limit.Value.ToString(CultureInfo.InvariantCulture) + " video(s) per call, but " + ((int)request.N).ToString(CultureInfo.InvariantCulture) + " were requested. Split the batch across multiple startVideo calls.");
        }

        var inputs = GenerateVideo.Normalize(request.Prompt, request.FrameImages, request.InputReferences);
        var headers = GenerateVideo.WithIdempotency(OperationHeaders.WithUserAgent(request.Headers, AiSdkVersion.UserAgent), request.GenerateId);
        var call = new VideoModelCall(inputs.Prompt, (int)request.N, request.AspectRatio, request.Resolution, request.Duration, request.Fps, request.Seed, inputs.Image, inputs.FrameImages, inputs.InputReferences, request.GenerateAudio, request.ProviderOptions ?? OperationJson.Parse("{}"), headers, token, request.WebhookUrl);
        var started = await OperationRetry.ExecuteAsync(request.MaxRetries, token, request.AbortReason, ct => model.DoStartAsync(call, ct), null).ConfigureAwait(false);
        var warnings = new List<OperationWarning>(inputs.Warnings);
        warnings.AddRange(started.Warnings);
        return new StartVideoResult(started.Operation, warnings, started.ProviderMetadata, started.Response);
    }
}

/// <summary>Reads video operation status. Maps to <c>experimental_getVideoStatus</c>.</summary>
public static class GetVideoStatus
{
    /// <summary>Performs one status check.</summary>
    public static async Task<VideoStatusResult> GetVideoStatusAsync(GetVideoStatusRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var model = request.Model ?? throw new InvalidArgumentException("model", null, "model is required");
        if (!model.CanStatus)
        {
            throw new InvalidOperationException("Video model " + model.ModelId + " does not implement doStatus.");
        }

        var token = request.CancellationToken.CanBeCanceled ? request.CancellationToken : cancellationToken;
        var headers = OperationHeaders.WithUserAgent(request.Headers, AiSdkVersion.UserAgent);
        return await OperationRetry.ExecuteAsync(request.MaxRetries, token, request.AbortReason, ct => model.DoStatusAsync(new VideoModelCall(null, 0, null, null, null, null, null, null, null, null, null, OperationJson.Parse("{}"), headers, ct, operation: request.Operation), ct), null).ConfigureAwait(false);
    }
}
