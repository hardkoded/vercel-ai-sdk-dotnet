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

var first = args.Length > 0 ? args[0] : "data/comic-cat.png";
if (!File.Exists(first))
{
    Console.WriteLine("Pass the path of an image file to run this example.");
    return;
}

var last = args.Length > 1 ? args[1] : "data/comic-dog.png";
if (!File.Exists(last))
{
    Console.WriteLine("Pass the path of an image file to run this example.");
    return;
}

var xai = XaiProvider.Create();

#region generate-video-xai-first-last-frame-audio
var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
{
    Model = (IVideoCaller)xai.Video("grok-imagine-video-1.5"),
    Prompt = new VideoPrompt(
        "The comic cat crosses the sunlit room and playfully transforms into the comic dog. "
        + "Add light footsteps and cheerful room ambience."),
    FrameImages =
    [
        new VideoFrameImage(File.ReadAllBytes(first), "first_frame"),
        new VideoFrameImage(File.ReadAllBytes(last), "last_frame"),
    ],
    GenerateAudio = true,
    Duration = 6,
    AspectRatio = "16:9",
    ProviderOptions = OperationJson.Parse("""{ "xai": { "pollTimeoutMs": 600000 } }"""),
});
#endregion

foreach (var warning in result.Warnings)
{
    Console.WriteLine("Warning: " + warning.Feature + ": " + warning.Details);
}

await File.WriteAllBytesAsync("video.mp4", result.Video.Data);
Console.WriteLine("Saved video.mp4");
