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

// Reference-to-video (R2V): provide reference images to guide the style and content of the video.
// The model uses the visual elements of those images without using them as the first frame (unlike image-to-video).
// Each reference image can be a public HTTPS URL or a base64 data URI.
#region generate-video-xai-reference-images
var result = await GenerateVideo.GenerateVideoAsync(
    new GenerateVideoRequest
    {
        Model = (IVideoCaller)xai.Video("grok-imagine-video-1.5"),
        Prompt = new VideoPrompt(
            "The comic cat from <IMAGE_1> and the comic dog from <IMAGE_2> are having a playful chase through a sunlit park. "
                + "Cinematic slow-motion, warm afternoon light."
        ),
        Duration = 8,
        AspectRatio = "16:9",
        ProviderOptions = OperationJson.Parse(
            """
            {
              "xai": {
                "mode": "reference-to-video",
                "referenceImageUrls": ["https://raw.githubusercontent.com/vercel/ai/refs/heads/main/examples/ai-functions/data/comic-cat.png", "https://raw.githubusercontent.com/vercel/ai/refs/heads/main/examples/ai-functions/data/comic-dog.png"],
                "pollTimeoutMs": 600000
              }
            }
            """
        ),
    }
);
#endregion

await File.WriteAllBytesAsync("video.mp4", result.Video.Data);
Console.WriteLine("Saved video.mp4");
