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

#region generate-video-xai-r2v-reference-voice
var result = await GenerateVideo.GenerateVideoAsync(
    new GenerateVideoRequest
    {
        Model = (IVideoCaller)xai.Video("grok-imagine-video-1.5"),
        Prompt = new VideoPrompt(
            "The comic cat from <IMAGE_0> sits beside the comic dog from <IMAGE_1>. "
                + "The cat speaks with the voice from <AUDIO_0> and the dog talks to the camera with the voice from <AUDIO_1>. "
                + "Handheld vertical shot, warm afternoon light."
        ),
        AspectRatio = "9:16",
        Duration = 10,
        ProviderOptions = OperationJson.Parse(
            """
            {
              "xai": {
                "mode": "reference-to-video",
                "referenceImageUrls": ["https://raw.githubusercontent.com/vercel/ai/refs/heads/main/examples/ai-functions/data/comic-cat.png", "https://raw.githubusercontent.com/vercel/ai/refs/heads/main/examples/ai-functions/data/comic-dog.png"],
                "referenceVoiceIds": ["luna", "zagan"],
                "resolution": "720p",
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
