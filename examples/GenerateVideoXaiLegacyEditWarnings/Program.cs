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

Console.WriteLine("Step 1: generating a source video...");
var source = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
{
    Model = (IVideoCaller)xai.Video("grok-imagine-video"),
    Prompt = new VideoPrompt("A cat sitting on a windowsill."),
    Duration = 3,
    ProviderOptions = OperationJson.Parse("""{ "xai": { "pollTimeoutMs": 600000 } }"""),
});

var sourceUrl = source.ProviderMetadata.GetProperty("xai").GetProperty("videoUrl").GetString()
    ?? throw new InvalidOperationException("xAI provider metadata did not include a source videoUrl.");
Console.WriteLine("Source video URL: " + sourceUrl);

Console.WriteLine("\nStep 2: editing with unsupported params...");
#region generate-video-xai-legacy-edit-warnings
var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
{
    Model = (IVideoCaller)xai.Video("grok-imagine-video"),
    Prompt = new VideoPrompt("Add sunglasses to the cat"),
    Duration = 10,
    AspectRatio = "16:9",
    Resolution = "1280x720",
    ProviderOptions = OperationJson.Parse(
        new JsonObject
        {
            ["xai"] = new JsonObject { ["videoUrl"] = sourceUrl, ["pollTimeoutMs"] = 600000 },
        }.ToJsonString()),
});
#endregion

Console.WriteLine("\nWarnings:");
foreach (var warning in result.Warnings)
{
    Console.WriteLine("- " + warning.Type + " " + warning.Feature + ": " + warning.Details);
}
