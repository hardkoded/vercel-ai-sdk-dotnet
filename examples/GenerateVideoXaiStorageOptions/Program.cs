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

#region generate-video-xai-storage-options
var result = await GenerateVideo.GenerateVideoAsync(
    new GenerateVideoRequest
    {
        Model = (IVideoCaller)xai.Video("grok-imagine-video-1.5"),
        Prompt = new VideoPrompt(
            "A tiny paper airplane glides through a bright modern library, cinematic tracking shot."
        ),
        Duration = 5,
        AspectRatio = "16:9",
        ProviderOptions = OperationJson.Parse(
            """
            {
              "xai": {
                "storageOptions": { "filename": "paper-airplane.mp4", "expiresAfter": 86400, "publicUrl": { "expiresAfter": 3600 } },
                "pollTimeoutMs": 600000
              }
            }
            """
        ),
    }
);
#endregion

foreach (var warning in result.Warnings)
{
    Console.WriteLine("Warning: " + warning.Feature + ": " + warning.Details);
}

Console.WriteLine("xAI metadata: " + result.ProviderMetadata.GetProperty("xai"));

await File.WriteAllBytesAsync("video.mp4", result.Video.Data);
Console.WriteLine("Saved video.mp4");
