// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Xai;
using System.Text.Json.Nodes;

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("XAI_API_KEY")))
{
    Console.WriteLine("Set XAI_API_KEY to run this example.");
    return;
}

var xai = XaiProvider.Create();

#region generate-video-xai-edit-concurrent
var model = (IVideoCaller)xai.Video("grok-imagine-video-1.5");

// Step 1: Apply the first edit.
var step1 = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
{
    Model = model,
    Prompt = new VideoPrompt("Make the cat look like a princess with a small tiara"),
    ProviderOptions = OperationJson.Parse("""
        { "xai": { "mode": "edit-video", "videoUrl": "https://raw.githubusercontent.com/vercel/ai/refs/heads/main/examples/ai-functions/data/prudence.mp4", "pollTimeoutMs": 600000 } }
        """),
});

// Use the xAI-hosted URL from step 1 as input for the next two edits.
var step1VideoUrl = step1.ProviderMetadata.GetProperty("xai").GetProperty("videoUrl").GetString()
    ?? throw new InvalidOperationException("xAI provider metadata did not include a step-1 videoUrl.");

// Step 2: Apply two more edits concurrently, building on step 1.
string[] edits =
[
    "Add a sparkly pink collar with a heart-shaped pendant",
    "Surround the cat with floating butterflies and flower petals",
];

var step2 = await Task.WhenAll(edits.Select(prompt => GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
{
    Model = model,
    Prompt = new VideoPrompt(prompt),
    ProviderOptions = OperationJson.Parse(
        new JsonObject
        {
            ["xai"] = new JsonObject { ["mode"] = "edit-video", ["videoUrl"] = step1VideoUrl, ["pollTimeoutMs"] = 600000 },
        }.ToJsonString()),
})));
#endregion

for (var i = 0; i < step2.Length; i++)
{
    var path = "edit-" + i + ".mp4";
    await File.WriteAllBytesAsync(path, step2[i].Video.Data);
    Console.WriteLine("Saved " + path + " for: " + edits[i]);
}
