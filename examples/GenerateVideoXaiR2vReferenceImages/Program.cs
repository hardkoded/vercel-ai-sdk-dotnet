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

var cat = args.Length > 0 ? args[0] : "data/comic-cat.png";
if (!File.Exists(cat))
{
    Console.WriteLine("Pass the path of a image file to run this example.");
    return;
}

var dog = args.Length > 1 ? args[1] : "data/comic-dog.png";
if (!File.Exists(dog))
{
    Console.WriteLine("Pass the path of a image file to run this example.");
    return;
}

var xai = XaiProvider.Create();

#region generate-video-xai-r2v-reference-images
var result = await GenerateVideo.GenerateVideoAsync(
    new GenerateVideoRequest
    {
        Model = (IVideoCaller)xai.Video("grok-imagine-video-1.5"),
        Prompt = new VideoPrompt(
            "<IMAGE_1> and <IMAGE_2> are having a playful chase through a sunlit park. "
                + "Cinematic slow-motion, warm afternoon light."
        ),
        InputReferences =
        [
            new VideoReference(File.ReadAllBytes(cat)),
            new VideoReference(File.ReadAllBytes(dog)),
        ],
        Duration = 8,
        AspectRatio = "16:9",
        ProviderOptions = OperationJson.Parse("""{ "xai": { "pollTimeoutMs": 600000 } }"""),
    }
);
#endregion

await File.WriteAllBytesAsync("video.mp4", result.Video.Data);
Console.WriteLine("Saved video.mp4");
