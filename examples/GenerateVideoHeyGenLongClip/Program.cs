// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.HeyGen;
using Vercel.AI.Operations;

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HEYGEN_API_KEY")))
{
    Console.WriteLine("Set HEYGEN_API_KEY to run this example.");
    return;
}

#region generate-video-heygen-long-clip
var result = await GenerateVideo.GenerateVideoAsync(
    new GenerateVideoRequest
    {
        Model = (IVideoCaller)HeyGenProvider.Create().Video("heygen-video-1"),
        Prompt = new VideoPrompt(
            "A potter slowly shapes a clay bowl on a wheel in a sunlit studio. One continuous shot with soft workshop sounds."
        ),
        Duration = 30,
        AspectRatio = "16:9",
        ProviderOptions = OperationJson.Parse(
            """
            { "heygen": { "resolution": "768p", "promptEnhancement": "quality" } }
            """
        ),
    }
);
#endregion

Console.WriteLine($"HeyGen metadata: {result.ProviderMetadata}");
await File.WriteAllBytesAsync("video.mp4", result.Video.Data);
Console.WriteLine("Saved video.mp4");
