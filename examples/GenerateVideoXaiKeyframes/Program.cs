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

#region generate-video-xai-keyframes
var result = await GenerateVideo.GenerateVideoAsync(
    new GenerateVideoRequest
    {
        Model = (IVideoCaller)xai.Video("grok-imagine-video-1.5"),
        Prompt = new VideoPrompt(
            "A continuous animated shot moves from a curious cat to a happy dog and ends on a wise owl, "
                + "all in the same sunlit room."
        ),
        Duration = 6,
        AspectRatio = "16:9",
        ProviderOptions = OperationJson.Parse(
            """
            {
              "xai": {
                "keyframes": [
                  { "imageUrl": "https://raw.githubusercontent.com/vercel/ai/refs/heads/main/examples/ai-functions/data/comic-cat.png", "timestampSeconds": 1.5 },
                  { "imageUrl": "https://raw.githubusercontent.com/vercel/ai/refs/heads/main/examples/ai-functions/data/comic-dog.png", "timestampSeconds": 3 },
                  { "imageUrl": "https://raw.githubusercontent.com/vercel/ai/refs/heads/main/examples/ai-functions/data/comic-owl.png", "timestampSeconds": 4.5 }
                ],
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

await File.WriteAllBytesAsync("video.mp4", result.Video.Data);
Console.WriteLine("Saved video.mp4");
