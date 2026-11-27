// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Cohere.CohereTestSupport;

namespace Vercel.AI.Tests.Upstream.Cohere.CohereChatLanguageModel;

/// <summary>Port of <c>cohere-chat-language-model.test.ts</c> &gt; <c>doGenerate</c> &gt; <c>text</c>.</summary>
public sealed class DoGenerateTextTests
{
    private const string Chat = "packages/cohere/src/cohere-chat-language-model.test.ts::";

    [Fact]
    [UpstreamTest(
        Chat + "doGenerate > text::should extract text response",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_extract_text_response()
    {
        var result = await Model(JsonFixture("cohere-text"))
            .DoGenerateAsync(TestPrompt(), CancellationToken.None);

        Assert.Equal(
            "The capital of France is Paris.",
            Assert.IsType<GeneratedText>(Assert.Single(result.Content)).Text
        );
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal("COMPLETE", result.RawFinishReason);
        Assert.Empty(result.Warnings);
        Assert.Equal("e7592632-1e3d-424f-b129-bd5f9f980f7b", result.ResponseId);
        Assert.Equal(507, result.Usage.InputTokens);
        Assert.Equal(10, result.Usage.OutputTokens);
        Assert.NotNull(result.Request?.Body);
        Assert.Equal(
            "command-r-plus",
            result.Request!.Body!.Value.GetProperty("model").GetString()
        );
        JsonAssert.Equal(JsonDocumentOf(result.RawResponse!), Fixture("cohere-text.json"));
        Assert.Equal("application/json", result.ResponseHeaders["Content-Type"]);
    }

    private static System.Text.Json.JsonElement JsonDocumentOf(string json) => Json(json);
}
