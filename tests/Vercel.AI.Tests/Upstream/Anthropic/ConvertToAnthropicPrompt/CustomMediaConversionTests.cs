// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.ConvertToAnthropicPrompt;

/// <summary>Port of <c>convert-to-anthropic-prompt.test.ts</c> &gt; <c>custom media conversion</c>.</summary>
public sealed class CustomMediaConversionTests
{
    [Theory]
    [InlineData("part")]
    [InlineData("message")]
    [UpstreamTest(
        "packages/anthropic/src/convert-to-anthropic-prompt.test.ts::custom media conversion::converts the original file part and preserves %s cache control",
        Coverage = UpstreamCoverage.Partial,
        Note = "The hook receives the file part as SDK message JSON, so the test compares its content instead of its identity.")]
    public void Converts_the_original_file_part_and_preserves_cache_control(string cacheControlLocation)
    {
        var cacheOptions = "{\"anthropic\":{\"cacheControl\":{\"type\":\"ephemeral\"}}}";
        var fileOptions = cacheControlLocation == "part"
            ? "{\"custom\":{\"fps\":2},\"anthropic\":{\"cacheControl\":{\"type\":\"ephemeral\"}}}"
            : "{\"custom\":{\"fps\":2}}";
        var file = FileContentPart.FromProviderReference(
            "video/mp4",
            new Dictionary<string, string> { ["custom"] = "video-id" },
            null,
            Options(fileOptions));
        var received = new List<JsonElement>();

        var result = AnthropicPrompt.Convert(
            new ModelMessage[]
            {
                new UserModelMessage(new UserContentPart[] { new TextContentPart("Describe."), file })
                {
                    ProviderOptions = cacheControlLocation == "message" ? Options(cacheOptions) : null,
                },
            },
            sendReasoning: false,
            convertMediaPart: part =>
            {
                received.Add(part.Clone());
                return new JsonObject
                {
                    ["type"] = "video",
                    ["source"] = new JsonObject { ["type"] = "url", ["url"] = "https://example.com/video.mp4" },
                };
            });

        var original = Assert.Single(received);
        JsonAssert.Equal(original.GetProperty("data"), "{\"type\":\"reference\",\"reference\":{\"custom\":\"video-id\"}}");
        Assert.Equal("video/mp4", original.GetProperty("mediaType").GetString());
        JsonAssert.Equal(original.GetProperty("providerOptions").GetProperty("custom"), "{\"fps\":2}");
        JsonAssert.Equal(
            result.Messages,
            "[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Describe.\"},{\"type\":\"video\",\"source\":{\"type\":\"url\",\"url\":\"https://example.com/video.mp4\"},\"cache_control\":{\"type\":\"ephemeral\"}}]}]");
        Assert.Empty(result.Betas);
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/convert-to-anthropic-prompt.test.ts::custom media conversion::uses default file conversion when the hook returns undefined",
        Coverage = UpstreamCoverage.Covered)]
    public void Uses_default_file_conversion_when_the_hook_returns_undefined()
    {
        var result = AnthropicPrompt.Convert(
            new ModelMessage[]
            {
                new UserModelMessage(new UserContentPart[] { new FileContentPart("application/pdf", "https://example.com/doc.pdf", null, null) }),
            },
            sendReasoning: false,
            convertMediaPart: _ => null);

        JsonAssert.Equal(
            result.Messages[0]!["content"],
            "[{\"type\":\"document\",\"source\":{\"type\":\"url\",\"url\":\"https://example.com/doc.pdf\"}}]");
        Assert.Contains("pdfs-2024-09-25", result.Betas);
    }

    private static IReadOnlyDictionary<string, JsonElement> Options(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.EnumerateObject().ToDictionary(property => property.Name, property => property.Value.Clone());
    }
}
