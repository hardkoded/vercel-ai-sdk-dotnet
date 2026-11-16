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

#region generate-video-xai-basic
var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
{
    Model = (IVideoCaller)xai.Video("grok-imagine-video-1.5"),
    Prompt = new VideoPrompt("A yorkie among dandelions at Crissy Field in San Francisco."),
    AspectRatio = "16:9",
    Duration = 5,
    ProviderOptions = OperationJson.Parse("""
        { "xai": { "user": "example-user-123", "pollTimeoutMs": 600000 } }
        """),
});
#endregion

await File.WriteAllBytesAsync("video.mp4", result.Video.Data);
Console.WriteLine("Saved video.mp4");
