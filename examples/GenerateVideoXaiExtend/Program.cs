// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Xai;

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("XAI_API_KEY")))
{
    Console.WriteLine("Set XAI_API_KEY to run this example.");
    return;
}

var xai = XaiProvider.Create();

// Video extension: continue an existing video from its last frame.
// Duration controls the length of the extension only, not the total.
// AspectRatio and Resolution are not supported in extension mode: the output inherits them from the input video.

// Step 1: Generate a short source video.
var source = await GenerateVideo.GenerateVideoAsync(
    new GenerateVideoRequest
    {
        Model = (IVideoCaller)xai.Video("grok-imagine-video-1.5"),
        Prompt = new VideoPrompt("A cat sitting on a sunlit windowsill, tail gently swishing."),
        Duration = 5,
        AspectRatio = "16:9",
        ProviderOptions = OperationJson.Parse("""{ "xai": { "pollTimeoutMs": 600000 } }"""),
    }
);

var sourceUrl = source.ProviderMetadata.GetProperty("xai").GetProperty("videoUrl").GetString()
    ?? throw new InvalidOperationException(
        "xAI provider metadata did not include a source videoUrl."
    );
Console.WriteLine("Source video URL: " + sourceUrl);

// Step 2: Extend the video with a new scene.
#region generate-video-xai-extend
var result = await GenerateVideo.GenerateVideoAsync(
    new GenerateVideoRequest
    {
        Model = (IVideoCaller)xai.Video("grok-imagine-video-1.5"),
        Prompt = new VideoPrompt(
            "The cat slowly turns its head, notices a butterfly, and leaps off the windowsill."
        ),
        Duration = 6,
        ProviderOptions = OperationJson.Parse(
            new JsonObject
            {
                ["xai"] = new JsonObject
                {
                    ["mode"] = "extend-video",
                    ["videoUrl"] = sourceUrl,
                    ["pollTimeoutMs"] = 600000,
                },
            }.ToJsonString()
        ),
    }
);
#endregion

Console.WriteLine("Extended video (original 5s + 6s extension = 11s total).");
Console.WriteLine("Provider metadata: " + result.ProviderMetadata);

await File.WriteAllBytesAsync("video.mp4", result.Video.Data);
Console.WriteLine("Saved video.mp4");
