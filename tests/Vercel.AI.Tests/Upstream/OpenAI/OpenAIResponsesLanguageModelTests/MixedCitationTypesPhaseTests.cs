// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>OpenAIResponsesLanguageModel &gt; mixed citation types &gt; phase</c>.</summary>
public sealed class MixedCitationTypesPhaseTests
{
    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > mixed citation types > phase::should include phase in provider metadata for streamed message items",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_phase_in_provider_metadata_for_streamed_message_items()
    {
        var chunks = string.Concat(
            File.ReadLines(Path.Combine(AppContext.BaseDirectory, "Fixtures", "openai-phase.1.chunks.txt"))
                .Select(chunk => "data: " + chunk + "\n\n"));
        var capture = new OpenAICapture { ServerSentEvents = chunks };

        var result = await OpenAIUpstream.Provider(capture)
            .ResponsesModel("gpt-5.3-codex")
            .DoStreamAsync(OpenAIUpstream.Hello(), CancellationToken.None);
        var parts = await OpenAIUpstream.Read(result.Stream);

        var textStartParts = parts.OfType<TextStartStreamPart>().ToList();
        Assert.Equal(2, textStartParts.Count);
        AssertMetadata(
            textStartParts[0].ProviderMetadata,
            "msg_0a63f40a2632b74300699f8819a5e08196ac270722d369af5a",
            "commentary");
        AssertMetadata(
            textStartParts[1].ProviderMetadata,
            "msg_0a63f40a2632b74300699f881bfbc88196aec38f30c3dd24b0",
            "final_answer");

        var textEndParts = parts.OfType<TextEndStreamPart>().ToList();
        Assert.Equal(2, textEndParts.Count);
        AssertMetadata(
            textEndParts[0].ProviderMetadata,
            "msg_0a63f40a2632b74300699f8819a5e08196ac270722d369af5a",
            "commentary");
        AssertMetadata(
            textEndParts[1].ProviderMetadata,
            "msg_0a63f40a2632b74300699f881bfbc88196aec38f30c3dd24b0",
            "final_answer");
    }

    private static void AssertMetadata(JsonElement? providerMetadata, string itemId, string phase)
    {
        var openai = providerMetadata!.Value.GetProperty("openai");
        Assert.Equal(itemId, openai.GetProperty("itemId").GetString());
        Assert.Equal(phase, openai.GetProperty("phase").GetString());
    }
}
