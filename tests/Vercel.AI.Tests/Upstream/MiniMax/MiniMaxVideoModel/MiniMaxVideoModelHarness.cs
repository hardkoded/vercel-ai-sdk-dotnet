// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.MiniMax;
using Vercel.AI.Operations;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Tests.Upstream;
using Model = Vercel.AI.MiniMax.MiniMaxVideoModel;

namespace Vercel.AI.Tests.MiniMaxVideoModel;

/// <summary>The shared setup of <c>minimax-video-model.test.ts</c>: constants, <c>createModel</c>, <c>defaultOptions</c>, and the test server.</summary>
internal static class MiniMaxVideoModelHarness
{
    public const string PromptText = "A white kitten chases a butterfly across a sunlit garden.";
    public const string BaseUrl = "https://api.example.com";
    public const string TaskId = "task-123";
    public const string CreateUrl = BaseUrl + "/v2/video_generation";
    public const string PollUrl = BaseUrl + "/v2/query/video_generation/" + TaskId;
    public const string ImageUrl = "https://cdn.example.com/first.png";
    public const string VideoUrl = "https://cdn.example.com/clip.mp4";

    public const string SucceededStatusResponse =
        "{\"task\":{\"id\":\"task-123\",\"model\":\"MiniMax-H3\",\"status\":\"succeeded\",\"content\":{\"url\":\"https://cdn.minimax.io/output/video-001.mp4\"},\"resolution\":\"2K\",\"duration\":5,\"ratio\":\"16:9\",\"usage\":{\"total_seconds\":5,\"input_seconds\":0,\"output_seconds\":5},\"task_type\":\"generation\",\"modality\":\"video\"}}";

    public static VideoModelFile ImageUrlFile { get; } =
        VideoModelFile.FromUrl(ImageUrl, "image/png");

    public static VideoModelFile VideoUrlFile { get; } =
        VideoModelFile.FromUrl(VideoUrl, "video/mp4");

    /// <summary>The server of <c>createTestServer</c>: the create call, the poll call, and the encoded poll call.</summary>
    public static ParityHandler Server(
        string createBody = "{\"task_id\":\"task-123\"}",
        string pollBody = SucceededStatusResponse
    )
    {
        return new ParityHandler(call =>
            call.Uri.AbsoluteUri switch
            {
                CreateUrl => ParityHandler.Json(createBody),
                PollUrl or BaseUrl + "/v2/query/video_generation/abc%2F..%2F..%2Finternal" =>
                    ParityHandler.Json(pollBody),
                _ => new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.NotFound),
            }
        );
    }

    /// <summary>Maps to <c>createModel</c>.</summary>
    public static Model CreateModel(
        ParityHandler handler,
        string modelId = "MiniMax-H3",
        Func<DateTime>? currentDate = null
    )
    {
        return new Model(
            modelId,
            new MiniMaxVideoModelConfig(
                "minimax.video",
                BaseUrl,
                () => new Dictionary<string, string> { ["Authorization"] = "Bearer test-key" },
                new HttpClient(handler),
                currentDate
            )
        );
    }

    /// <summary>Maps to <c>minimaxOptions</c>: the fast poll settings plus any provider options under test.</summary>
    public static JsonElement MiniMaxOptions(string? extra = null)
    {
        var options = new JsonObject { ["pollIntervalMs"] = 10, ["pollTimeoutMs"] = 5000 };
        if (extra != null)
        {
            foreach (var pair in JsonNode.Parse(extra)!.AsObject())
            {
                options[pair.Key] = pair.Value?.DeepClone();
            }
        }

        return OperationJson.Parse(new JsonObject { ["minimax"] = options }.ToJsonString());
    }

    /// <summary>Maps to <c>createModel({ modelId }).doGenerate(options)</c>.</summary>
    public static Task<VideoModelResult> Generate(
        ParityHandler handler,
        MiniMaxCall options,
        string modelId = "MiniMax-H3"
    )
    {
        return CreateModel(handler, modelId)
            .DoGenerateAsync(options.ToCall(), CancellationToken.None);
    }

    /// <summary>The request body of <c>defaultOptions</c> for a text-to-video call, with the fields under test replaced.</summary>
    public static string Body(
        string model = "MiniMax-H3",
        string resolution = "2K",
        int duration = 5,
        string? ratio = "16:9",
        string? extraContent = null,
        string? extra = null
    )
    {
        return "{\"model\":\""
            + model
            + "\",\"content\":[{\"type\":\"text\",\"text\":\""
            + PromptText
            + "\"}"
            + extraContent
            + "],\"resolution\":\""
            + resolution
            + "\",\"duration\":"
            + duration
            + (ratio != null ? ",\"ratio\":\"" + ratio + "\"" : string.Empty)
            + extra
            + "}";
    }

    /// <summary>One image, video, or audio part of the request content.</summary>
    public static string Part(string type, string url, string role)
    {
        return ",{\"type\":\""
            + type
            + "\",\""
            + type
            + "\":{\"url\":\""
            + url
            + "\"},\"role\":\""
            + role
            + "\"}";
    }

    public static VideoFrameFile Frame(VideoModelFile image, string frameType)
    {
        return new VideoFrameFile(image, frameType);
    }

    public static VideoModelFile ImageReference(int index)
    {
        return VideoModelFile.FromUrl($"https://cdn.example.com/img-{index}.png", "image/png");
    }

    public static VideoModelFile VideoReference(int index)
    {
        return VideoModelFile.FromUrl($"https://cdn.example.com/vid-{index}.mp4", "video/mp4");
    }

    public static JsonNode? RequestBody(ParityHandler handler, int index = 0)
    {
        return JsonNode.Parse(handler.Calls[index].Text);
    }

    public static JsonNode? Resolve(JsonElement? metadata, string field)
    {
        return JsonNode.Parse(metadata!.Value.GetRawText())!["minimax"]![field];
    }

    public static void AssertWarning(
        IReadOnlyList<OperationWarning> warnings,
        string feature,
        string details
    )
    {
        Assert.Contains(warnings, warning =>
            warning.Type == "unsupported"
            && warning.Feature == feature
            && warning.Details == details
        );
    }

    public static bool HasWarning(
        IReadOnlyList<OperationWarning> warnings,
        params string[] features
    )
    {
        return warnings.Any(warning =>
            warning.Type == "unsupported" && features.Contains(warning.Feature)
        );
    }
}

/// <summary>The call options of <c>defaultOptions</c>, with each field overridable.</summary>
internal sealed class MiniMaxCall
{
    public string? AspectRatio { get; set; }

    public string? Resolution { get; set; }

    public double? Duration { get; set; }

    public int? Fps { get; set; }

    public double? Seed { get; set; }

    public int N { get; set; } = 1;

    public bool? GenerateAudio { get; set; }

    public VideoModelFile? Image { get; set; }

    public IReadOnlyList<VideoFrameFile>? FrameImages { get; set; }

    public IReadOnlyList<VideoModelFile>? InputReferences { get; set; }

    public string? WebhookUrl { get; set; }

    public IReadOnlyDictionary<string, string>? Headers { get; set; }

    public JsonElement ProviderOptions { get; set; } = MiniMaxVideoModelHarness.MiniMaxOptions();

    public CancellationToken CancellationToken { get; set; }

    public VideoModelCall ToCall()
    {
        return new VideoModelCall(
            MiniMaxVideoModelHarness.PromptText,
            N,
            AspectRatio,
            Resolution,
            Duration,
            Fps,
            Seed,
            Image,
            FrameImages,
            InputReferences,
            GenerateAudio,
            ProviderOptions,
            Headers ?? new Dictionary<string, string>(),
            CancellationToken,
            WebhookUrl
        );
    }

    public VideoModelCall StatusCall(JsonElement operation)
    {
        return new VideoModelCall(
            null,
            0,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            OperationJson.Parse("{}"),
            Headers ?? new Dictionary<string, string>(),
            CancellationToken,
            operation: operation
        );
    }
}
