// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Gateway;

/// <summary>
/// Gateway video model. <see cref="DoGenerateAsync(VideoModelCall, CancellationToken)"/> reads the first data event of an
/// SSE response from <c>/video-model</c>. <see cref="DoStartAsync"/> and <see cref="DoStatusAsync"/> use
/// <c>/video-model/start</c> and <c>/video-model/status</c>. Maps to <c>GatewayVideoModel</c> in <c>gateway-video-model.ts</c>.
/// </summary>
public sealed class GatewayVideoModel : IVideoModel, IVideoCaller
{
    private readonly GatewayProvider _provider;

    /// <summary>Creates a Gateway video model.</summary>
    /// <param name="provider">Gateway provider that sends the requests.</param>
    /// <param name="modelId">Model id.</param>
    /// <param name="providerName">Value of <see cref="Provider"/> (upstream <c>GatewayConfig.provider</c>). Defaults to <c>gateway</c>.</param>
    public GatewayVideoModel(GatewayProvider provider, string modelId, string? providerName = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        Provider = providerName ?? GatewayProvider.ProviderId;
    }

    /// <inheritdoc />
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public string Provider { get; }

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Large so that requests are never split client-side.</summary>
    public int? MaxVideosPerCall => int.MaxValue;

    /// <inheritdoc />
    public bool CanGenerate => true;

    /// <inheritdoc />
    public bool CanStart => true;

    /// <inheritdoc />
    public bool CanStatus => true;

    /// <inheritdoc />
    public bool CanWebhook => true;

    /// <inheritdoc />
    public Task<int?> ResolveMaxVideosPerCallAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(MaxVideosPerCall);
    }

    /// <inheritdoc />
    public async Task<VideoResult> DoGenerateAsync(
        VideoCallOptions options,
        CancellationToken cancellationToken
    )
    {
        options = options ?? throw new ArgumentNullException(nameof(options));
        var call = new VideoModelCall(
            options.Prompt,
            1,
            null,
            null,
            options.DurationSeconds,
            null,
            null,
            null,
            null,
            null,
            null,
            OperationJson.Parse("{}"),
            new Dictionary<string, string>(),
            cancellationToken
        );
        var video = (await DoGenerateAsync(call, cancellationToken).ConfigureAwait(false)).Videos[
            0
        ];
        return video.Type switch
        {
            "url" => new VideoResult(video.Url, null, video.MediaType!),
            "base64" => new VideoResult(
                null,
                Convert.FromBase64String((string)video.Data!),
                video.MediaType!
            ),
            _ => throw new AiSdkException(
                "The Gateway delivered the video to caller-managed storage. It has no URL or bytes to return."
            ),
        };
    }

    /// <summary>Sends the call and reads the first data event of the SSE response.</summary>
    public async Task<VideoModelResult> DoGenerateAsync(
        VideoModelCall call,
        CancellationToken cancellationToken
    )
    {
        call = call ?? throw new ArgumentNullException(nameof(call));

        // Built outside the try so a missing key surfaces as it is.
        var headers = await HeadersAsync(call.Headers, cancellationToken).ConfigureAwait(false);
        headers["accept"] = "text/event-stream";
        var url = _provider.Route("video-model");
        var body = BuildRequestBody(call).ToJsonString();
        try
        {
            var sse = await _provider
                .Http.OpenSseResponseAsync(url, body, headers, cancellationToken)
                .ConfigureAwait(false);
            using (sse.Resource)
            {
                var enumerator = sse.Events.GetAsyncEnumerator(cancellationToken);
                try
                {
                    if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
                    {
                        throw new Util.ApiCallError(
                            "SSE stream ended without a data event",
                            url.AbsoluteUri,
                            GatewayProvider.RequestBodyValues(body),
                            200
                        );
                    }

                    var result = ReadEvent(enumerator.Current, url.AbsoluteUri, body, sse.Headers);
                    return new VideoModelResult(
                        result.Videos,
                        result.Warnings,
                        result.ProviderMetadata,
                        ResponseInfo(sse.Headers)
                    );
                }
                finally
                {
                    await enumerator.DisposeAsync().ConfigureAwait(false);
                }
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            throw GatewayProvider.MapFailure(exception, headers, body);
        }
    }

    // The Gateway notifies the caller's URL on completion (DoStartAsync maps it to callbackUrl), so the
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
        var headers = await HeadersAsync(call.Headers, cancellationToken).ConfigureAwait(false);
        var body = BuildRequestBody(call);

        // The spec option is webhookUrl. The Gateway's wire contract for a completion webhook is callbackUrl.
        if (!string.IsNullOrEmpty(call.WebhookUrl))
        {
            body["callbackUrl"] = call.WebhookUrl;
        }

        try
        {
            var root = await PostJsonAsync(
                    "video-model/start",
                    body.ToJsonString(),
                    headers,
                    cancellationToken
                )
                .ConfigureAwait(false);
            using (root.Document)
            {
                var element = root.Element;
                var operation = element.TryGetProperty("operation", out var found)
                    ? found.Clone()
                    : JsonSerializer.SerializeToElement<object?>(null);
                return new VideoStartResult(
                    operation,
                    ReadWarnings(element),
                    ReadProviderMetadata(element),
                    ResponseInfo(root.Headers)
                );
            }
        }
        catch (JsonException exception)
        {
            throw GatewayProvider.MapFailure(exception, headers);
        }
    }

    /// <inheritdoc />
    public async Task<VideoStatusResult> DoStatusAsync(
        VideoModelCall call,
        CancellationToken cancellationToken
    )
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var headers = await HeadersAsync(call.Headers, cancellationToken).ConfigureAwait(false);
        var body = new JsonObject
        {
            ["operation"] = JsonSerializer.SerializeToNode(call.Operation),
        };
        try
        {
            var root = await PostJsonAsync(
                    "video-model/status",
                    body.ToJsonString(),
                    headers,
                    cancellationToken
                )
                .ConfigureAwait(false);
            using (root.Document)
            {
                var element = root.Element;
                var response = ResponseInfo(root.Headers);
                var providerMetadata = ReadProviderMetadata(element);
                switch (ReadString(element, "status"))
                {
                    case "completed":
                        return new VideoStatusResult(
                            "completed",
                            ReadVideos(element),
                            warnings: ReadWarnings(element),
                            providerMetadata: providerMetadata,
                            response: response
                        );
                    case "error":
                        return new VideoStatusResult(
                            "error",
                            error: ReadString(element, "error"),
                            providerMetadata: providerMetadata,
                            response: response
                        );

                    // The Gateway reports cooperative cancellation as its own terminal status. The operation union has
                    // no cancelled state, so it surfaces as a terminal error rather than polling forever.
                    case "cancelled":
                        return new VideoStatusResult(
                            "error",
                            error: "Video generation was cancelled.",
                            providerMetadata: providerMetadata,
                            response: response
                        );
                    case "pending":
                        return new VideoStatusResult(
                            "pending",
                            warnings: ReadWarnings(element),
                            providerMetadata: providerMetadata,
                            response: response
                        );
                    default:
                        throw GatewayProvider.MapFailure(
                            new Util.ApiCallError(
                                "Invalid video status response",
                                _provider.Route("video-model/status").AbsoluteUri,
                                GatewayProvider.RequestBodyValues(body.ToJsonString()),
                                200,
                                root.Headers,
                                element.GetRawText()
                            ),
                            headers
                        );
                }
            }
        }
        catch (JsonException exception)
        {
            throw GatewayProvider.MapFailure(exception, headers);
        }
    }

    private static JsonObject BuildRequestBody(VideoModelCall call)
    {
        var body = new JsonObject { ["prompt"] = call.Prompt, ["n"] = call.N };
        if (!string.IsNullOrEmpty(call.AspectRatio))
        {
            body["aspectRatio"] = call.AspectRatio;
        }

        if (!string.IsNullOrEmpty(call.Resolution))
        {
            body["resolution"] = call.Resolution;
        }

        if (call.Duration is { } duration && duration != 0)
        {
            body["duration"] = Number(duration);
        }

        if (call.Fps is { } fps && fps != 0)
        {
            body["fps"] = fps;
        }

        // A seed of 0 is a seed.
        if (call.Seed is { } seed)
        {
            body["seed"] = Number(seed);
        }

        if (call.GenerateAudio is { } generateAudio)
        {
            body["generateAudio"] = generateAudio;
        }

        if (call.ProviderOptions.ValueKind == JsonValueKind.Object)
        {
            body["providerOptions"] = JsonNode.Parse(call.ProviderOptions.GetRawText());
        }

        if (call.Image != null)
        {
            body["image"] = MaybeEncodeVideoFile(call.Image);
        }

        if (call.FrameImages != null)
        {
            var frames = new JsonArray();
            foreach (var frame in call.FrameImages)
            {
                frames.Add(
                    new JsonObject
                    {
                        ["frameType"] = frame.FrameType,
                        ["image"] = MaybeEncodeVideoFile(frame.Image),
                    }
                );
            }

            body["frameImages"] = frames;
        }

        if (call.InputReferences != null)
        {
            var references = new JsonArray();
            foreach (var reference in call.InputReferences)
            {
                references.Add(MaybeEncodeVideoFile(reference));
            }

            body["inputReferences"] = references;
        }

        return body;
    }

    // Binary data is sent as base64. A file that already carries base64 text and a URL file go through unchanged.
    private static JsonNode MaybeEncodeVideoFile(VideoModelFile file)
    {
        var node = new JsonObject { ["type"] = file.Type };
        if (file.Type == "url")
        {
            node["url"] = file.Url;
            if (file.MediaType != null)
            {
                node["mediaType"] = file.MediaType;
            }
        }
        else
        {
            node["mediaType"] = file.MediaType;
            node["data"] = file.Base64 ?? Convert.ToBase64String(file.Data ?? Array.Empty<byte>());
        }

        if (file.ProviderOptions != null)
        {
            node["providerOptions"] = JsonSerializer.SerializeToNode(file.ProviderOptions);
        }

        return node;
    }

    private static JsonNode Number(double value)
    {
        return value == Math.Floor(value) && Math.Abs(value) < long.MaxValue
            ? JsonValue.Create((long)value)
            : JsonValue.Create(value);
    }

    private static string ReadString(JsonElement element, string name)
    {
        return
            element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()!
            : throw new JsonException(
                "Invalid Gateway video response: \"" + name + "\" must be a string."
            );
    }

    private static string? ReadOptionalString(JsonElement element, string name)
    {
        return
            element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static IReadOnlyList<VideoOutput> ReadVideos(JsonElement element)
    {
        if (
            !element.TryGetProperty("videos", out var array)
            || array.ValueKind != JsonValueKind.Array
        )
        {
            throw new JsonException("Invalid Gateway video response: \"videos\" must be an array.");
        }

        var videos = new List<VideoOutput>();
        foreach (var video in array.EnumerateArray())
        {
            var mediaType = ReadString(video, "mediaType");
            videos.Add(
                ReadString(video, "type") switch
                {
                    "uploaded" => new VideoOutput("uploaded", mediaType: mediaType),
                    "url" => new VideoOutput("url", ReadString(video, "url"), mediaType: mediaType),
                    "base64" => new VideoOutput(
                        "base64",
                        data: ReadString(video, "data"),
                        mediaType: mediaType
                    ),
                    _ => throw new JsonException(
                        "Invalid Gateway video response: unknown video type."
                    ),
                }
            );
        }

        return videos;
    }

    private static IReadOnlyList<OperationWarning> ReadWarnings(JsonElement element)
    {
        var warnings = new List<OperationWarning>();
        if (
            !element.TryGetProperty("warnings", out var array)
            || array.ValueKind == JsonValueKind.Null
        )
        {
            return warnings;
        }

        if (array.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException(
                "Invalid Gateway video response: \"warnings\" must be an array."
            );
        }

        foreach (var warning in array.EnumerateArray())
        {
            var type = ReadString(warning, "type");
            warnings.Add(
                type switch
                {
                    "unsupported" or "compatibility" => new OperationWarning(
                        type,
                        feature: ReadString(warning, "feature"),
                        details: ReadOptionalString(warning, "details")
                    ),
                    "deprecated" => new OperationWarning(
                        type,
                        message: ReadString(warning, "message"),
                        setting: ReadString(warning, "setting")
                    ),
                    "other" => new OperationWarning(type, message: ReadString(warning, "message")),
                    _ => throw new JsonException(
                        "Invalid Gateway video response: unknown warning type."
                    ),
                }
            );
        }

        return warnings;
    }

    private static JsonElement? ReadProviderMetadata(JsonElement element)
    {
        if (
            !element.TryGetProperty("providerMetadata", out var metadata)
            || metadata.ValueKind == JsonValueKind.Null
        )
        {
            return null;
        }

        return metadata.ValueKind == JsonValueKind.Object
            ? metadata.Clone()
            : throw new JsonException(
                "Invalid Gateway video response: \"providerMetadata\" must be an object."
            );
    }

    private (
        IReadOnlyList<VideoOutput> Videos,
        IReadOnlyList<OperationWarning> Warnings,
        JsonElement? ProviderMetadata
    ) ReadEvent(
        string data,
        string url,
        string requestBody,
        IReadOnlyDictionary<string, string> responseHeaders
    )
    {
        using var document = ParseEvent(data, url, requestBody);
        var root = document.RootElement;
        try
        {
            switch (ReadString(root, "type"))
            {
                case "error":
                    var message = ReadString(root, "message");
                    var param = root.TryGetProperty("param", out var found)
                        ? JsonNode.Parse(found.GetRawText())
                        : null;
                    throw new Util.ApiCallError(
                        message,
                        url,
                        GatewayProvider.RequestBodyValues(requestBody),
                        root.TryGetProperty("statusCode", out var status)
                        && status.ValueKind == JsonValueKind.Number
                            ? status.GetInt32()
                            : throw new JsonException(
                                "Invalid Gateway video event: \"statusCode\" must be a number."
                            ),
                        responseHeaders,
                        root.GetRawText(),
                        data: JsonSerializer.SerializeToElement(
                            new JsonObject
                            {
                                ["error"] = new JsonObject
                                {
                                    ["message"] = message,
                                    ["type"] = ReadString(root, "errorType"),
                                    ["param"] = param,
                                },
                            }
                        )
                    );
                case "result":
                    return (ReadVideos(root), ReadWarnings(root), ReadProviderMetadata(root));
                default:
                    throw new JsonException("Invalid Gateway video event: unknown type.");
            }
        }
        catch (JsonException exception)
        {
            throw new Util.ApiCallError(
                "Failed to parse video SSE event",
                url,
                GatewayProvider.RequestBodyValues(requestBody),
                200,
                responseHeaders,
                cause: exception
            );
        }
    }

    private static JsonDocument ParseEvent(string data, string url, string requestBody)
    {
        try
        {
            return JsonDocument.Parse(data);
        }
        catch (JsonException exception)
        {
            throw new Util.ApiCallError(
                "Failed to parse video SSE event",
                url,
                GatewayProvider.RequestBodyValues(requestBody),
                200,
                cause: exception
            );
        }
    }

    private ProviderResponse ResponseInfo(IReadOnlyDictionary<string, string> headers)
    {
        return new ProviderResponse(headers, timestamp: DateTime.UtcNow, modelId: ModelId);
    }

    private async Task<Dictionary<string, string?>> HeadersAsync(
        IReadOnlyDictionary<string, string> callHeaders,
        CancellationToken cancellationToken
    )
    {
        var extra = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in callHeaders)
        {
            extra[pair.Key] = pair.Value;
        }

        return await _provider
            .HeadersAsync(
                "ai-video-model-specification-version",
                "4",
                "ai-model-id",
                ModelId,
                null,
                extra,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    private async Task<(
        JsonDocument Document,
        JsonElement Element,
        IReadOnlyDictionary<string, string> Headers
    )> PostJsonAsync(
        string path,
        string body,
        Dictionary<string, string?> headers,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var response = await _provider
                .Http.SendJsonStringAsync(
                    HttpMethod.Post,
                    _provider.Route(path),
                    body,
                    headers,
                    cancellationToken
                )
                .ConfigureAwait(false);
            var document = JsonDocument.Parse(
                string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body
            );
            return (document, document.RootElement, response.Headers);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            throw GatewayProvider.MapFailure(exception, headers, body);
        }
    }
}
