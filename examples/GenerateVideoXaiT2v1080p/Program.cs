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

#region generate-video-xai-t2v-1080p
var result = await GenerateVideo.GenerateVideoAsync(
    new GenerateVideoRequest
    {
        Model = (IVideoCaller)xai.Video("grok-imagine-video-1.5"),
        Prompt = new VideoPrompt(
            "A glowing crystal-powered rocket launching from the red dunes of Mars."
        ),
        AspectRatio = "16:9",
        Duration = 8,
        ProviderOptions = OperationJson.Parse(
            """
            { "xai": { "resolution": "1080p", "pollTimeoutMs": 600000 } }
            """
        ),
    }
);
#endregion

await File.WriteAllBytesAsync("video.mp4", result.Video.Data);
Console.WriteLine("Saved video.mp4");
