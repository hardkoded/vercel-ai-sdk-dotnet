// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Xai;

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("XAI_API_KEY")))
{
    Console.WriteLine("Set XAI_API_KEY to run this example.");
    return;
}

var xai = XaiProvider.Create();

// xAI does not support webhooks natively. The SDK adds an unsupported-webhook warning and falls back to polling.
#region generate-video-xai-webhook-options
var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
{
    Model = (IVideoCaller)xai.Video("grok-imagine-video-1.5"),
    Prompt = new VideoPrompt("A yorkie among dandelions at Crissy Field in San Francisco."),
    AspectRatio = "16:9",
    Duration = 5,
    ProviderOptions = OperationJson.Parse("""{ "xai": { "pollTimeoutMs": 600000 } }"""),
    Webhook = () => throw new InvalidOperationException("Webhooks are not supported by xAI. This callback should not be invoked"),
});
#endregion

foreach (var warning in result.Warnings)
{
    Console.WriteLine("Warning: " + warning.Feature + ": " + warning.Details);
}

await File.WriteAllBytesAsync("video.mp4", result.Video.Data);
Console.WriteLine("Saved video.mp4");
