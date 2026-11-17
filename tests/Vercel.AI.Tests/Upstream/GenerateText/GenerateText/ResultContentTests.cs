// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using static Vercel.AI.Tests.Upstream.GenerateText.ResultToolCallsTests;

namespace Vercel.AI.Tests.Upstream.GenerateText.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; result.content</c>.</summary>
public sealed class ResultContentTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > result.content::";

    [Fact]
    [UpstreamTest(
        Prefix + "should include reasoning-file in content and exclude from files",
        Coverage = UpstreamCoverage.Covered,
        Commit = "2ebc78b65435926718879f85a1a85687b053c542"
    )]
    public async Task Should_include_reasoning_file_in_content_and_exclude_from_files()
    {
        using var metadata = JsonDocument.Parse("""{"google":{"thoughtSignature":"sig123"}}""");
        var result = await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = new TestLanguageModel
                    {
                        OnGenerate = _ => new LanguageModelGenerateResult(
                            new GeneratedContent[]
                            {
                                new GeneratedText("Here is a thought image:"),
                                new GeneratedReasoningFile(
                                    new byte[] { 10, 20, 30 },
                                    "image/png",
                                    metadata.RootElement.Clone()
                                ),
                                new GeneratedFile(new byte[] { 40, 50, 60 }, "image/jpeg"),
                            },
                            FinishReason.Stop,
                            new LanguageModelUsage(3, 10, 13),
                            "stop"
                        ),
                    },
                    Prompt = "prompt",
                }
            );

        Assert.Equal(3, result.Content.Count);
        Assert.Equal(
            "Here is a thought image:",
            Assert.IsType<GeneratedText>(result.Content[0]).Text
        );
        var reasoningFile = Assert.IsType<GeneratedReasoningFile>(result.Content[1]);
        Assert.Equal(new byte[] { 10, 20, 30 }, reasoningFile.Data);
        Assert.Equal("image/png", reasoningFile.MediaType);
        Assert.Equal(
            "sig123",
            reasoningFile
                .ProviderMetadata!.Value.GetProperty("google")
                .GetProperty("thoughtSignature")
                .GetString()
        );
        var file = Assert.IsType<GeneratedFile>(result.Content[2]);
        Assert.Equal(new byte[] { 40, 50, 60 }, file.Data);
        Assert.Equal("image/jpeg", file.MediaType);

        var onlyFile = Assert.Single(result.Files);
        Assert.Equal(new byte[] { 40, 50, 60 }, onlyFile.Data);
        Assert.Equal("image/jpeg", onlyFile.MediaType);
    }
}
