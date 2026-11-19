// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Azure;
using Vercel.AI.Operations;

// generate-image/azure/mai-image-2-7-transparent.ts
// Requires a Foundry resource with an MAI-Image-2.7 deployment.
var apiKey = Environment.GetEnvironmentVariable("AZURE_MAI_API_KEY")
    ?? Environment.GetEnvironmentVariable("AZURE_API_KEY");
var resourceName = Environment.GetEnvironmentVariable("AZURE_MAI_RESOURCE_NAME");
if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(resourceName))
{
    Console.WriteLine(
        "Set AZURE_MAI_RESOURCE_NAME and AZURE_MAI_API_KEY (or AZURE_API_KEY) to run this example."
    );
    return;
}

var azure = AzureOpenAIProvider.Create(
    new AzureOpenAIOptions { ResourceName = resourceName, ApiKey = apiKey }
);

#region generate-image-azure-mai-image-2-7-transparent
var result = await GenerateImage.GenerateImageAsync(
    new GenerateImageRequest
    {
        Model = azure.Image("MAI-Image-2.7"), // Use your own deployment
        Text = "A red apple sticker",
        // `api: "mai"` keeps custom deployment names on the MAI image API.
        ProviderOptions = JsonDocument
            .Parse("""{ "azure": { "api": "mai", "background": "transparent" } }""")
            .RootElement.Clone(),
    }
);
#endregion

Console.WriteLine("Provider metadata: " + result.ProviderMetadata);
Console.WriteLine("Warnings: " + JsonSerializer.Serialize(result.Warnings));

var path = Path.Combine(Path.GetTempPath(), "mai-image-2-7-transparent.png");
await File.WriteAllBytesAsync(path, result.Image.Data!);
Console.WriteLine("Saved " + result.Images.Count + " image(s); first at " + path);
