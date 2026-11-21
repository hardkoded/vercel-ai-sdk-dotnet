// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Operations;

// generate-image/gateway/streaming-image-decoding.ts
// Requires AI_GATEWAY_API_KEY (or a Vercel OIDC token).
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AI_GATEWAY_API_KEY")))
{
    Console.WriteLine("Set AI_GATEWAY_API_KEY to run this example.");
    return;
}

var gateway = GatewayProvider.Create();

#region generate-image-gateway-streaming-image-decoding
var result = await GenerateImage.GenerateImageAsync(
    new GenerateImageRequest
    {
        Model = (IImageCaller)gateway.ImageModel("bfl/flux-kontext-pro"),
        Text = "A mountain lake at sunrise",
        ProviderOptions = JsonDocument
            .Parse("""{ "gateway": { "experimental_streamingImageDecoding": true } }""")
            .RootElement.Clone(),
    }
);
#endregion

Console.WriteLine("Media type: " + result.Image.MediaType);
Console.WriteLine("Image bytes: " + result.Image.Data!.Length);
