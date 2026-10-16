// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using static Vercel.AI.Tests.Upstream.KlingAI.KlingAIVideoModel.KlingAIVideoModelSupport;

namespace Vercel.AI.Tests.Upstream.KlingAI.KlingAIVideoModel;

/// <summary>Port of <c>klingai-video-model.test.ts</c> &gt; <c>KlingAIVideoModel</c> &gt; <c>webhooks</c>.</summary>
public sealed class WebhooksTests
{
    private const string Prefix = "packages/klingai/src/klingai-video-model.test.ts::KlingAIVideoModel > webhooks";

    private static readonly string[] Endpoints = { "text2video", "image2video", "multi-image2video", "motion-control" };

    // name, webhookUrl, rawUrl, expected
    private static readonly (string Name, string? WebhookUrl, string? RawUrl, string? Expected)[] Cases =
    {
        ("explicit URL", "https://example.com/webhook", null, "https://example.com/webhook"),
        ("no callback", null, null, null),
        ("raw passthrough", null, "https://example.com/raw", "https://example.com/raw"),
        ("explicit URL overrides raw", "https://example.com/webhook", "https://example.com/raw", "https://example.com/webhook"),
    };

    public static IEnumerable<object?[]> SubmitCases()
    {
        foreach (var endpoint in Endpoints)
        {
            foreach (var (name, webhookUrl, rawUrl, expected) in Cases)
            {
                yield return new object?[] { endpoint, name, webhookUrl, rawUrl, expected };
            }
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "::should leave the generic webhook hook undefined", Coverage = UpstreamCoverage.Covered, Note = "handleWebhookOption undefined maps to CanWebhook == false.")]
    public void Should_leave_the_generic_webhook_hook_undefined()
    {
        var server = new Server();
        Assert.False(CreateModel(server.Handler).CanWebhook);
    }

    [Theory]
    [MemberData(nameof(SubmitCases))]
    [UpstreamTest(Prefix + " > $endpoint::should submit $name", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_submit(string endpoint, string name, string? webhookUrl, string? rawUrl, string? expected)
    {
        Assert.NotNull(name);
        var (modelId, options, inputReferences) = endpoint switch
        {
            "text2video" => ("kling-v2.6-t2v", StdOptions, null),
            "image2video" => ("kling-v2.6-i2v", StdOptions, null),
            "multi-image2video" => ("kling-v1.6-i2v", StdOptions, (IReadOnlyList<VideoModelFile>?)new[]
            {
                VideoModelFile.FromUrl("https://example.com/character-1.png"),
                VideoModelFile.FromUrl("https://example.com/character-2.png"),
            }),
            _ => ("kling-v2.6-motion-control", MotionControlOptions, null),
        };

        var providerOptions = JsonNode.Parse(options)!.AsObject();
        if (rawUrl != null)
        {
            providerOptions["klingai"]!.AsObject()["callback_url"] = rawUrl;
        }

        var image = endpoint == "image2video" ? StartFrame : null;
        var call = Start(image: image, inputReferences: inputReferences, providerOptions: providerOptions.ToJsonString(), webhookUrl: webhookUrl);
        var server = new Server();
        var result = await CreateModel(server.Handler, modelId).DoStartAsync(call, CancellationToken.None).ConfigureAwait(false);

        JsonAssert.Equal((System.Text.Json.JsonElement)result.Operation!, "{\"taskId\":\"task-abc-123\",\"endpointPath\":\"/v1/videos/" + endpoint + "\"}");
        var body = server.RequestBody();
        if (expected == null)
        {
            Assert.False(body.ContainsKey("callback_url"));
        }
        else
        {
            Assert.Equal(expected, body["callback_url"]!.GetValue<string>());
        }

        if (endpoint == "multi-image2video")
        {
            JsonAssert.Equal(body["image_list"], "[{\"image\":\"https://example.com/character-1.png\"},{\"image\":\"https://example.com/character-2.png\"}]");
        }
    }
}
