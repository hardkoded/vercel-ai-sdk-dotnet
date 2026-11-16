// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Xunit;

namespace Vercel.AI.Tests.SmoothStream;

public sealed class ProviderMetadataPreservationTests : SmoothStreamTestBase
{
    private static readonly System.Text.Json.JsonElement s_metadataA = Meta("sig-a");

    private static readonly System.Text.Json.JsonElement s_metadataB = Meta("sig-b");

    private Task SmoothParts(params TextStreamPart[] parts) => Run(parts, new SmoothStreamOptions { DelayInMs = null });

    [Fact]
    public async Task ShouldPreserveMetadataFromAnEmptyDeltaWhenTheBufferIsEmpty()
    {
        var metadata = Meta("sig_abc123");
        await Run(
            [
                new ReasoningStartPart("r1"),
                R("Let me think. ", "r1"),
                R(string.Empty, "r1", metadata),
                new ReasoningEndPart("r1"),
                new TextStartPart("t1"),
                D("Hello world", "t1"),
                new TextEndPart("t1"),
            ],
            new SmoothStreamOptions { DelayInMs = null });
        ExpectOutput(
            new ReasoningStartPart("r1"),
            R("Let ", "r1"),
            R("me ", "r1"),
            R("think. ", "r1"),
            R(string.Empty, "r1", metadata),
            new ReasoningEndPart("r1"),
            new TextStartPart("t1"),
            D("Hello ", "t1"),
            D("world", "t1"),
            new TextEndPart("t1"));
    }

    [Fact]
    public async Task ShouldPreserveProviderMetadataOnReasoningDeltaChunksSignatureForAnthropicThinking()
    {
        var metadata = Meta("sig_abc123");
        await Run([
            new ReasoningStartPart("1"),
            R("I am"),
            R(" thinking..."),
            R(string.Empty, "1", metadata), // signature as an empty delta with providerMetadata
            new ReasoningEndPart("1"),
            new TextStartPart("2"),
            D("Hello!", "2"),
            new TextEndPart("2"),
        ]);

        var lastReasoningDelta = Parts.OfType<ReasoningDeltaPart>().Last();
        Assert.NotNull(lastReasoningDelta.ProviderMetadata);
        Assert.Equal(metadata.GetRawText(), lastReasoningDelta.ProviderMetadata!.Value.GetRawText());
    }

    [Fact]
    public async Task ShouldPreserveAnEmptyMetadataDeltaAfterAnExactBoundary()
    {
        await SmoothParts(R("Done "), R(string.Empty, "1", s_metadataA), new ReasoningEndPart("1"));
        ExpectOutput(R("Done "), R(string.Empty, "1", s_metadataA), new ReasoningEndPart("1"));
    }

    [Fact]
    public async Task ShouldNotCarryMetadataToAMetadataFreeDeltaWithTheSameId()
    {
        await SmoothParts(R("Signed", "1", s_metadataA), R(" plain "));
        ExpectOutput(R("Signed", "1", s_metadataA), R(" plain "));
    }

    [Fact]
    public async Task ShouldKeepDifferentMetadataValuesWithTheirSourceDeltas()
    {
        await SmoothParts(R("First", "1", s_metadataA), R(" second ", "1", s_metadataB));
        ExpectOutput(R("First", "1", s_metadataA), R(" second ", "1", s_metadataB));
    }

    [Fact]
    public async Task ShouldNotCarryWordChunkMetadataToASubsequentDeltaType()
    {
        var metadata = Meta("sig_abc123");
        await SmoothParts(R("Signed ", "reasoning-1", metadata), D("Plain ", "text-1"));
        ExpectOutput(R("Signed ", "reasoning-1", metadata), D("Plain ", "text-1"));
    }

    [Fact]
    public async Task ShouldNotCarryLineChunkMetadataToASubsequentDeltaId()
    {
        var metadata = Meta("sig_abc123");
        await Run(
            [D("Signed line\n", "text-1", metadata), D("Plain line\n", "text-2")],
            new SmoothStreamOptions { Chunking = SmoothStreamChunking.Line, DelayInMs = null });
        ExpectOutput(D("Signed line\n", "text-1", metadata), D("Plain line\n", "text-2"));
    }

    [Fact]
    public async Task ShouldPreserveProviderMetadataOnReasoningStartForRedactedThinking()
    {
        var metadata = System.Text.Json.JsonSerializer.SerializeToElement(
            new Dictionary<string, object> { ["anthropic"] = new Dictionary<string, string> { ["redactedData"] = "redacted-thinking-data" } });
        await Run([new ReasoningStartPart("1", metadata), new ReasoningEndPart("1")]);

        // reasoning-start should pass through unchanged with providerMetadata
        var reasoningStart = Parts.OfType<ReasoningStartPart>().First();
        Assert.NotNull(reasoningStart.ProviderMetadata);
        Assert.Equal(metadata.GetRawText(), reasoningStart.ProviderMetadata!.Value.GetRawText());
    }

    [Theory]
    [InlineData(SmoothStreamChunking.Word, true, "First second final", new[] { "First ", "second ", "final" })]
    [InlineData(SmoothStreamChunking.Line, false, "First line\nSecond line\nfinal line", new[] { "First line\n", "Second line\n", "final line" })]
    public async Task ShouldPreserveProviderMetadataOnEveryChunkedPart(SmoothStreamChunking chunking, bool reasoning, string inputText, string[] expectedText)
    {
        var metadata = Meta("sig_abc123");
        TextStreamPart Delta(string text) => reasoning ? R(text, "1", metadata) : D(text, "1", metadata);
        TextStreamPart[] input = reasoning
            ? [new ReasoningStartPart("1"), Delta(inputText), new ReasoningEndPart("1")]
            : [new TextStartPart("1"), Delta(inputText), new TextEndPart("1")];

        await Run(input, new SmoothStreamOptions { Chunking = chunking, DelayInMs = null });

        Assert.Equal(
            expectedText.Select(Delta).Select(Describe).ToArray(),
            Parts.Where(p => p is TextDeltaPart or ReasoningDeltaPart).Select(Describe).ToArray());
    }
}
