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

// Create a signed HTTPS PUT URL in your storage service. Sign it for Content-Type: video/mp4
// and allow enough time for generation and upload.
var uploadUrl = Environment.GetEnvironmentVariable("XAI_VIDEO_UPLOAD_URL");
if (string.IsNullOrEmpty(uploadUrl))
{
    Console.WriteLine("Set XAI_VIDEO_UPLOAD_URL to a signed storage PUT URL to run this example.");
    return;
}

var xai = XaiProvider.Create();

try
{
    #region generate-video-xai-upload-url
    var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
    {
        Model = (IVideoCaller)xai.Video("grok-imagine-video-1.5"),
        Prompt = new VideoPrompt("A blue ball rolls across a plain white surface."),
        Duration = 5,
        ProviderOptions = OperationJson.Parse(
            new JsonObject { ["xai"] = new JsonObject { ["output"] = new JsonObject { ["uploadUrl"] = uploadUrl } } }.ToJsonString()),
    });
    #endregion

    // Upload-only results have no local video bytes. Do not read result.Video.
    Console.WriteLine("Videos delivered by xAI: " + result.UploadedVideos);
}
catch (Exception)
{
    // Signed URLs are credentials. Do not log the request or the provider diagnostics.
    Console.WriteLine("Video delivery failed. Check the destination and URL expiry.");
}
