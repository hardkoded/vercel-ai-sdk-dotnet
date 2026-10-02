// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenTelemetry;

namespace Vercel.AI.Tests;

public sealed class StringifyForTelemetryTests
{
    [Fact]
    [UpstreamTest(
        "packages/otel/src/stringify-for-telemetry.test.ts::stringifyForTelemetry::should stringify a prompt with text parts",
        Coverage = UpstreamCoverage.Covered)]
    public void Stringifies_a_prompt_with_text_parts()
    {
        var prompt = new[]
        {
            new TelemetryMessage("system") { Content = "You are a helpful assistant." },
            new TelemetryMessage("user")
            {
                Parts = new[]
                {
                    new TelemetryContentPart("text") { Text = "Hello!" },
                },
            },
        };

        Assert.Equal(
            "[{\"role\":\"system\",\"content\":\"You are a helpful assistant.\"},{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Hello!\"}]}]",
            TelemetryPrompt.Stringify(prompt));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/stringify-for-telemetry.test.ts::stringifyForTelemetry::should convert Uint8Array images to base64 strings",
        Coverage = UpstreamCoverage.Covered)]
    public void Converts_image_bytes_to_base64()
    {
        var prompt = new[]
        {
            new TelemetryMessage("user")
            {
                Parts = new[]
                {
                    new TelemetryContentPart("file")
                    {
                        Data = TelemetryFileData.FromBytes(new byte[] { 0x89, 0x50, 0x4e, 0x47, 0xff, 0xff }),
                        MediaType = "image/png",
                    },
                },
            },
        };

        Assert.Equal(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"file\",\"data\":\"iVBOR///\",\"mediaType\":\"image/png\"}]}]",
            TelemetryPrompt.Stringify(prompt));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/stringify-for-telemetry.test.ts::stringifyForTelemetry::should preserve the file name and provider options",
        Coverage = UpstreamCoverage.Covered)]
    public void Preserves_the_file_name_and_provider_options()
    {
        var prompt = new[]
        {
            new TelemetryMessage("user")
            {
                Parts = new[]
                {
                    new TelemetryContentPart("file")
                    {
                        Filename = "image.png",
                        Data = TelemetryFileData.FromBytes(new byte[] { 0x89, 0x50, 0x4e, 0x47, 0xff, 0xff }),
                        MediaType = "image/png",
                        ProviderOptions = JsonNode.Parse("{\"anthropic\":{\"key\":\"value\"}}"),
                    },
                },
            },
        };

        Assert.Equal(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"file\",\"filename\":\"image.png\",\"data\":\"iVBOR///\",\"mediaType\":\"image/png\",\"providerOptions\":{\"anthropic\":{\"key\":\"value\"}}}]}]",
            TelemetryPrompt.Stringify(prompt));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/stringify-for-telemetry.test.ts::stringifyForTelemetry::should keep URL images as is",
        Coverage = UpstreamCoverage.Covered)]
    public void Keeps_url_images()
    {
        var prompt = new[]
        {
            new TelemetryMessage("user")
            {
                Parts = new[]
                {
                    new TelemetryContentPart("text") { Text = "Check this image:" },
                    new TelemetryContentPart("file")
                    {
                        Data = TelemetryFileData.FromUrl(new Uri("https://example.com/image.jpg")),
                        MediaType = "image/jpeg",
                    },
                },
            },
        };

        Assert.Equal(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Check this image:\"},{\"type\":\"file\",\"data\":\"https://example.com/image.jpg\",\"mediaType\":\"image/jpeg\"}]}]",
            TelemetryPrompt.Stringify(prompt));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/stringify-for-telemetry.test.ts::stringifyForTelemetry::should handle a mixed prompt with various content types",
        Coverage = UpstreamCoverage.Covered)]
    public void Handles_a_mixed_prompt()
    {
        var prompt = new[]
        {
            new TelemetryMessage("system") { Content = "You are a helpful assistant." },
            new TelemetryMessage("user")
            {
                Parts = new[]
                {
                    new TelemetryContentPart("file")
                    {
                        Data = TelemetryFileData.FromBytes(new byte[] { 0x89, 0x50, 0x4e, 0x47, 0xff, 0xff }),
                        MediaType = "image/png",
                    },
                    new TelemetryContentPart("file")
                    {
                        Data = TelemetryFileData.FromUrl(new Uri("https://example.com/image.jpg")),
                        MediaType = "image/jpeg",
                    },
                },
            },
            new TelemetryMessage("assistant")
            {
                Parts = new[]
                {
                    new TelemetryContentPart("text") { Text = "I see the images!" },
                },
            },
        };

        Assert.Equal(
            "[{\"role\":\"system\",\"content\":\"You are a helpful assistant.\"},{\"role\":\"user\",\"content\":[{\"type\":\"file\",\"data\":\"iVBOR///\",\"mediaType\":\"image/png\"},{\"type\":\"file\",\"data\":\"https://example.com/image.jpg\",\"mediaType\":\"image/jpeg\"}]},{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"I see the images!\"}]}]",
            TelemetryPrompt.Stringify(prompt));
    }
}
