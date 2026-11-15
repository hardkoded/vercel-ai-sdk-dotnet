// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Tencent;
using Vercel.AI.Tests.MoreProviders;
using TencentProviderType = Vercel.AI.Tencent.TencentProvider;
using TencentVideoModelType = Vercel.AI.Tencent.TencentVideoModel;

namespace Vercel.AI.Tests.Upstream.Tencent.TencentVideoModel;

/// <summary>Fixtures shared by the <c>tencent-video-model.test.ts</c> port.</summary>
internal static class TencentVideoModelSupport
{
    internal const string Commit = "1c7a311d5384a6741daade550e4d3a7f9253d71e";
    internal const string UpstreamFile = "packages/tencent/src/tencent-video-model.test.ts";
    internal const string TokenHubBaseUrl = "https://tokenhub-intl.tencentcloudmaas.com/v1";
    internal const string VodUrl = "https://vod.intl.tencentcloudapi.com";
    internal const string TaskId = "task-123";
    internal const string Prompt = "A cat walking on a beach";

    internal static readonly DateTimeOffset CurrentDate = new(2026, 11, 8, 0, 0, 0, TimeSpan.Zero);

    internal const string SubmittedResponse = "{\"code\":0,\"message\":\"SUCCEED\",\"request_id\":\"req-1\",\"data\":{\"id\":\"task-123\",\"status\":\"submitted\"}}";

    internal const string SucceededStatusResponse = "{\"code\":0,\"message\":\"SUCCEED\",\"request_id\":\"req-2\",\"data\":[{\"id\":\"task-123\",\"status\":\"succeeded\",\"outputs\":[{\"type\":\"video\",\"id\":\"video-1\",\"url\":\"https://cdn.example.com/video.mp4\",\"duration\":\"5.041\"}]}],\"tokenhub_usage\":{\"total_tokens\":12345}}";

    /// <summary>The upstream <c>createTestServer</c>: URL to scripted response. Set <see cref="Respond"/> to override a URL.</summary>
    internal sealed class Server
    {
        private readonly Dictionary<string, (int Status, string Body)> _routes = new(StringComparer.Ordinal);

        internal Server()
        {
            foreach (var endpoint in new[] { "text-to-video", "image-to-video", "omni-video", "motion-control" })
            {
                _routes[TokenHubBaseUrl + "/wand/kling/" + endpoint] = (200, SubmittedResponse);
            }

            _routes[TokenHubBaseUrl + "/wand/kling/tasks/" + TaskId] = (200, SucceededStatusResponse);
            _routes[VodUrl] = (200, "{\"Response\":{\"TaskId\":\"task-123\",\"RequestId\":\"req-vod-1\"}}");
            Handler = new ParityHandler(call =>
            {
                var response = ParityHandler.Json(_routes[call.Uri.AbsoluteUri.TrimEnd('/')].Body);
                response.StatusCode = (System.Net.HttpStatusCode)_routes[call.Uri.AbsoluteUri.TrimEnd('/')].Status;
                return response;
            });
        }

        internal ParityHandler Handler { get; }

        internal List<ParityCall> Calls => Handler.Calls;

        /// <summary>Overrides the response of a URL.</summary>
        internal void Respond(string url, string body, int status = 200)
        {
            _routes[url] = (status, body);
        }

        internal JsonObject RequestBody(int index = 0)
        {
            return JsonNode.Parse(Calls[index].Text)!.AsObject();
        }
    }

    /// <summary>The upstream <c>createModel</c>: the TokenHub key is <c>test-key</c> and the VOD credentials are <c>AKIDEXAMPLE</c>.</summary>
    internal static TencentVideoModelType CreateModel(Server server, string modelId)
    {
        var options = new TencentOptions { ApiKey = "test-key", SecretId = "AKIDEXAMPLE", SecretKey = "EXAMPLESECRET", SubAppId = "1500000000" };
        options.Headers["x-vod-header"] = "vod";
        return new TencentVideoModelType(TencentProviderType.Create(options, server.Handler), modelId, () => CurrentDate);
    }

    /// <summary>The upstream <c>defaultOptions</c> with overrides.</summary>
    internal static VideoModelCall Start(
        string? prompt = Prompt,
        int n = 1,
        string? aspectRatio = null,
        string? resolution = null,
        double? duration = null,
        int? fps = null,
        double? seed = null,
        VideoModelFile? image = null,
        IReadOnlyList<VideoFrameFile>? frameImages = null,
        IReadOnlyList<VideoModelFile>? inputReferences = null,
        bool? generateAudio = null,
        string providerOptions = "{}",
        string? webhookUrl = null)
    {
        return new VideoModelCall(prompt, n, aspectRatio, resolution, duration, fps, seed, image, frameImages, inputReferences, generateAudio, OperationJson.Parse(providerOptions), new Dictionary<string, string>(), CancellationToken.None, webhookUrl);
    }

    internal static VideoModelCall Status()
    {
        return new VideoModelCall(null, 0, null, null, null, null, null, null, null, null, null, OperationJson.Parse("{}"), new Dictionary<string, string>(), CancellationToken.None, operation: OperationJson.Parse("{\"taskId\":\"task-123\"}"));
    }

    internal static IReadOnlyList<VideoFrameFile> Frames(params (VideoModelFile Image, string FrameType)[] frames)
    {
        return frames.Select(frame => new VideoFrameFile(frame.Image, frame.FrameType)).ToList();
    }

    internal static VideoModelFile Url(string url, string? mediaType = null) => VideoModelFile.FromUrl(url, mediaType);

    internal static void Body(Server server, string expected, int index = 0)
    {
        JsonAssert.Equal(server.RequestBody(index), expected);
    }

    internal static string[] Features(IEnumerable<OperationWarning> warnings) => warnings.Select(warning => warning.Feature ?? string.Empty).ToArray();
}
