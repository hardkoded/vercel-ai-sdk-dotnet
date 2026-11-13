// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Tencent;

/// <summary>
/// Tencent video model. <see cref="DoStartAsync"/> creates a task and <see cref="DoStatusAsync"/> reads it.
/// TokenHub Kling models use their TokenHub id (for example <c>kling-video-v3</c>); VOD AIGC models use <c>vod/&lt;ModelName&gt;/&lt;ModelVersion&gt;</c>.
/// The operation is a JSON object with the <c>taskId</c>.
/// </summary>
public sealed class TencentVideoModel : IVideoModel, IVideoCaller
{
    private readonly TencentProvider _provider;
    private readonly Func<DateTimeOffset> _clock;
    private readonly ParsedTencentVideoModelId _parsedModelId;

    /// <summary>Creates a model. <paramref name="clock"/> sets the response timestamp and the VOD signature time.</summary>
    public TencentVideoModel(TencentProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _parsedModelId = TencentModelCatalog.ParseTencentVideoModelId(modelId) ?? throw new NoSuchModelError(modelId, "videoModel");
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    /// <inheritdoc />
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public string Provider => "tencent.video";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Tencent creates one video per task.</summary>
    public int? MaxVideosPerCall => 1;

    /// <inheritdoc />
    public bool CanGenerate => false;

    /// <inheritdoc />
    public bool CanStart => true;

    /// <inheritdoc />
    public bool CanStatus => true;

    /// <summary>False. Tencent video tasks take no per-request callback URL.</summary>
    public bool CanWebhook => false;

    /// <inheritdoc />
    public Task<int?> ResolveMaxVideosPerCallAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(MaxVideosPerCall);
    }

    /// <summary>Starts a task and polls it until the video is ready.</summary>
    public async Task<VideoResult> DoGenerateAsync(VideoCallOptions options, CancellationToken cancellationToken)
    {
        options = options ?? throw new ArgumentNullException(nameof(options));
        var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest { Model = this, Prompt = new VideoPrompt(options.Prompt), Duration = options.DurationSeconds }, cancellationToken).ConfigureAwait(false);
        return new VideoResult(null, result.Video.Data, result.Video.MediaType);
    }

    /// <inheritdoc />
    public Task<VideoModelResult> DoGenerateAsync(VideoModelCall call, CancellationToken cancellationToken)
    {
        throw new NotSupportedException("Tencent generates videos with doStart and doStatus.");
    }

    /// <inheritdoc />
    public Task<VideoWebhook> HandleWebhookAsync(Func<Task<VideoWebhook>> webhook, CancellationToken cancellationToken)
    {
        throw new NotSupportedException("Tencent video tasks do not support webhooks.");
    }

    /// <inheritdoc />
    public async Task<VideoStartResult> DoStartAsync(VideoModelCall call, CancellationToken cancellationToken)
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var currentDate = _clock();
        var warnings = new List<OperationWarning>();
        var parsed = _parsedModelId;
        var tencentOptions = TencentVideoModelOptions.Parse(call.ProviderOptions);
        TencentVideoInputs.WarnUnsupportedCommonOptions(call, parsed.IsVod, warnings);

        string taskId;
        IReadOnlyDictionary<string, string> responseHeaders;
        if (!parsed.IsVod)
        {
            TencentVideoInputs.WarnIgnoredProviderOptions(tencentOptions, TencentModelCatalog.VodOnlyOptions, "This option only applies to VOD models; it was ignored.", warnings);
            var request = TencentTokenHubVideo.BuildTokenHubKlingRequest(ModelId, parsed.Kind, call, tencentOptions, warnings);
            var url = ApiKeys.Combine(_provider.Options.BaseUrl, "/wand/kling/" + request.Endpoint);
            var exchange = await SendTokenHubAsync(HttpMethod.Post, url, ProviderExchange.Json(request.Body.ToJsonString()), call, cancellationToken).ConfigureAwait(false);
            var root = TencentTokenHubError.ReadResponse(exchange, url.AbsoluteUri);
            string? id = null;
            if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object
                && data.TryGetProperty("id", out var idValue) && idValue.ValueKind == JsonValueKind.String)
            {
                id = idValue.GetString();
            }

            if (string.IsNullOrEmpty(id))
            {
                throw new AiSdkException("No task ID returned from TokenHub.");
            }

            taskId = id!;
            responseHeaders = exchange.Headers;
        }
        else
        {
            TencentVideoInputs.WarnIgnoredProviderOptions(tencentOptions, TencentModelCatalog.TokenHubOnlyOptions, "This option only applies to TokenHub Kling models; it was ignored.", warnings);
            var credentials = _provider.GetVodCredentials();
            var body = TencentVodVideo.BuildVodCreateTaskBody(parsed.ModelName!, parsed.ModelVersion!, credentials.SubAppId, call, tencentOptions, warnings);
            var result = await TencentCloudApi.PostTencentCloudApiAsync(
                _provider.HttpClient,
                _provider.VodBaseUrl,
                TencentVodVideo.TencentVodService,
                "CreateAigcVideoTask",
                TencentVodVideo.TencentVodVersion,
                _provider.TencentOptions.Region,
                credentials,
                body,
                ProviderExchange.Merge(_provider.CreateVodHeaders(), call.Headers),
                currentDate.ToUnixTimeSeconds(),
                cancellationToken).ConfigureAwait(false);
            if (!result.Value.TryGetProperty("TaskId", out var taskIdValue) || taskIdValue.ValueKind != JsonValueKind.String)
            {
                throw new ApiException("Invalid JSON response", 200, result.Value.GetRawText());
            }

            taskId = taskIdValue.GetString()!;
            responseHeaders = result.Headers;
        }

        var operation = new JsonObject { ["taskId"] = taskId };
        return new VideoStartResult(
            OperationJson.Parse(operation.ToJsonString()),
            warnings,
            response: new ProviderResponse(responseHeaders, timestamp: currentDate.UtcDateTime, modelId: ModelId));
    }

    /// <inheritdoc />
    public async Task<VideoStatusResult> DoStatusAsync(VideoModelCall call, CancellationToken cancellationToken)
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var currentDate = _clock();
        var taskId = call.Operation is JsonElement { ValueKind: JsonValueKind.Object } operation
            && operation.TryGetProperty("taskId", out var value) && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
        if (string.IsNullOrEmpty(taskId))
        {
            throw new InvalidArgumentException("operation", call.Operation, "Tencent operations are objects with a taskId.");
        }

        if (!_parsedModelId.IsVod)
        {
            var baseUrl = _provider.Options.BaseUrl;
            var url = ApiKeys.Combine(baseUrl, "/wand/kling/tasks/" + PathSegments.Encode(taskId!));
            var exchange = await SendTokenHubAsync(HttpMethod.Get, url, null, call, cancellationToken).ConfigureAwait(false);
            var root = TencentTokenHubError.ReadResponse(exchange, url.AbsoluteUri);
            return TencentTokenHubVideo.InterpretTokenHubKlingStatus(root, taskId!, new ProviderResponse(exchange.Headers, timestamp: currentDate.UtcDateTime, modelId: ModelId));
        }

        var credentials = _provider.GetVodCredentials();
        var result = await TencentCloudApi.PostTencentCloudApiAsync(
            _provider.HttpClient,
            _provider.VodBaseUrl,
            TencentVodVideo.TencentVodService,
            "DescribeTaskDetail",
            TencentVodVideo.TencentVodVersion,
            _provider.TencentOptions.Region,
            credentials,
            new JsonObject { ["TaskId"] = taskId, ["SubAppId"] = credentials.SubAppId },
            ProviderExchange.Merge(_provider.CreateVodHeaders(), call.Headers),
            currentDate.ToUnixTimeSeconds(),
            cancellationToken).ConfigureAwait(false);
        return TencentVodVideo.InterpretVodTaskDetail(result.Value, taskId!, new ProviderResponse(result.Headers, timestamp: currentDate.UtcDateTime, modelId: ModelId));
    }

    private async Task<ProviderExchangeResult> SendTokenHubAsync(HttpMethod method, Uri url, HttpContent? content, VideoModelCall call, CancellationToken cancellationToken)
    {
        try
        {
            var headers = ProviderExchange.Merge(_provider.CreateHeaders(), call.Headers);
            if (method == HttpMethod.Get)
            {
                return await ProviderExchange.GetResponseUrlAsync(_provider.HttpClient, url.AbsoluteUri, _provider.Options.BaseUrl, Download.ValidateDownloadUrl, headers, cancellationToken, ProviderExchange.DownloadClient(_provider.Options), ProviderExchange.UserAgentOnly(headers)).ConfigureAwait(false);
            }

            return await ProviderExchange.SendAsync(_provider.HttpClient, method, url, content, headers, cancellationToken).ConfigureAwait(false);
        }
        catch (ApiException exception) when (TencentTokenHubError.ReadErrorMessage(exception.ResponseBody) is { } message)
        {
            throw ProviderHttp.MapStatus(exception.StatusCode, message, exception.ResponseBody);
        }
    }
}
