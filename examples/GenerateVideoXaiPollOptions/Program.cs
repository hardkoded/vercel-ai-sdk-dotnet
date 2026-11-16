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

var startTime = DateTime.UtcNow;

#region generate-video-xai-poll-options
var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
{
    Model = (IVideoCaller)xai.Video("grok-imagine-video-1.5"),
    Prompt = new VideoPrompt("A yorkie among dandelions at Crissy Field in San Francisco."),
    AspectRatio = "16:9",
    Duration = 5,
    ProviderOptions = OperationJson.Parse("""{ "xai": { "pollTimeoutMs": 600000 } }"""),
    Poll = true,
    PollIntervalMs = 1000,
    PollTimeoutMs = 60_000,
});
#endregion

Console.WriteLine("Video generation complete in " + (DateTime.UtcNow - startTime).TotalSeconds.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + "s");

await File.WriteAllBytesAsync("video.mp4", result.Video.Data);
Console.WriteLine("Saved video.mp4");
