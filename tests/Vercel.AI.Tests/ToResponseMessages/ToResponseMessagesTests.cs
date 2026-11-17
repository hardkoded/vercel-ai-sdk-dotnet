// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Responses = Vercel.AI.GenerateText.ToResponseMessages;

namespace Vercel.AI.Tests.ToResponseMessages;

/// <summary>Port of <c>to-response-messages.test.ts</c> &gt; <c>toResponseMessages</c>.</summary>
public sealed class ToResponseMessagesTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/to-response-messages.test.ts::toResponseMessages::";

    [Fact]
    [UpstreamTest(
        Prefix + "should include reasoning-file parts in the assistant message",
        Coverage = UpstreamCoverage.Partial,
        Note = "Covers the assistant message only. The tool message half of toResponseMessages is not extracted yet."
    )]
    public void Should_include_reasoning_file_parts_in_the_assistant_message()
    {
        var pngFile = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
        using var metadata = JsonDocument.Parse("{\"testProvider\":{\"signature\":\"sig\"}}");

        var result = Responses.ToAssistantContent(
            new GeneratedContent[]
            {
                new GeneratedReasoningFile(pngFile, "image/png", metadata.RootElement.Clone()),
                new GeneratedText("Here is my analysis"),
            }
        );

        Assert.Equal(2, result.Count);
        var reasoningFile = Assert.IsType<AssistantReasoningFilePart>(result[0]);
        Assert.Equal("image/png", reasoningFile.MediaType);
        Assert.Equal("iVBORw0KGgo=", Convert.ToBase64String(reasoningFile.Data!));
        Assert.Equal(
            "{\"signature\":\"sig\"}",
            reasoningFile.ProviderOptions!["testProvider"].GetRawText()
        );
        var text = Assert.IsType<AssistantTextPart>(result[1]);
        Assert.Equal("Here is my analysis", text.Text);
        Assert.Null(text.ProviderOptions);
    }
}
