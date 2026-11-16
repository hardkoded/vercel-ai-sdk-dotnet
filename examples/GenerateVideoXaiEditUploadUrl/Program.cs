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

var uploadUrl = Environment.GetEnvironmentVariable("XAI_VIDEO_UPLOAD_URL");
var videoUrl = Environment.GetEnvironmentVariable("XAI_SOURCE_VIDEO_URL");
if (string.IsNullOrEmpty(uploadUrl) || string.IsNullOrEmpty(videoUrl))
{
    Console.WriteLine("Set XAI_VIDEO_UPLOAD_URL and XAI_SOURCE_VIDEO_URL to run this example.");
    return;
}

var xai = XaiProvider.Create();

try
{
    #region generate-video-xai-edit-upload-url
    var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
    {
        Model = (IVideoCaller)xai.Video("grok-imagine-video-1.5"),
        Prompt = new VideoPrompt("Make the scene look like a watercolor painting."),
        ProviderOptions = OperationJson.Parse(
            new JsonObject
            {
                ["xai"] = new JsonObject
                {
                    ["mode"] = "edit-video",
                    ["videoUrl"] = videoUrl,
                    ["output"] = new JsonObject { ["uploadUrl"] = uploadUrl },
                },
            }.ToJsonString()),
    });
    #endregion

    Console.WriteLine("Edited videos delivered by xAI: " + result.UploadedVideos);
}
catch (Exception)
{
    // Signed URLs are credentials. Do not log the request or the provider diagnostics.
    Console.WriteLine("Video delivery failed. Check the source, destination and URL expiry.");
}
