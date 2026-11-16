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

var path = args.Length > 0 ? args[0] : "data/comic-cat.png";
if (!File.Exists(path))
{
    Console.WriteLine("Pass the path of a image file to run this example.");
    return;
}

var xai = XaiProvider.Create();

#region generate-video-xai-i2v-frame-images
var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
{
    Model = (IVideoCaller)xai.Video("grok-imagine-video-1.5"),
    Prompt = new VideoPrompt(
        "The comic cat stretches, yawns, and pads off across a sunlit room. "
        + "Cinematic, warm afternoon light."),
    FrameImages = [new VideoFrameImage(File.ReadAllBytes(path), "first_frame")],
    Duration = 8,
    AspectRatio = "16:9",
    ProviderOptions = OperationJson.Parse("""{ "xai": { "pollTimeoutMs": 600000 } }"""),
});
#endregion

await File.WriteAllBytesAsync("video.mp4", result.Video.Data);
Console.WriteLine("Saved video.mp4");
