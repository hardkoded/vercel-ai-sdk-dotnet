// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests.AzureResponsesToolSupport;

namespace Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests;

/// <summary>Port of <c>azure-openai-provider.test.ts</c> &gt; <c>responses</c> &gt; <c>doGenerate</c> &gt; <c>reasoning</c>.</summary>
public sealed class ResponsesDoGenerateReasoningTests
{
    private const string Calculator =
        "{\"type\":\"object\",\"properties\":{\"a\":{\"type\":\"number\"},\"b\":{\"type\":\"number\"},\"op\":{\"type\":\"string\"}},\"required\":[\"a\",\"b\"],\"additionalProperties\":false}";

    [Fact]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::responses > doGenerate > reasoning::should generate with reasoning encrypted content",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_generate_with_reasoning_encrypted_content()
    {
        var fixture = JsonNode.Parse(Fixture("azure-reasoning-encrypted-content.1.json"))!;
        var capture = new OpenAICapture { ResponseJson = fixture.ToJsonString() };
        capture.ResponseHeaders["content-type"] = "application/json";

        var options = Call(
            new[] { new LanguageModelTool("calculator", null, Json(Calculator)) },
            azureOptions: "{\"reasoningEffort\":\"high\",\"maxCompletionTokens\":32000,\"store\":false,\"include\":[\"reasoning.encrypted_content\"],\"reasoningSummary\":\"auto\",\"forceReasoning\":true}");
        var result = await Generate(capture, options);

        // Request body of the snapshot.
        var body = RequestBody(capture);
        Assert.Equal("test-deployment", body["model"]!.GetValue<string>());
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("[\"reasoning.encrypted_content\"]"), body["include"]));
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"effort\":\"high\",\"summary\":\"auto\"}"), body["reasoning"]));
        Assert.False(body["store"]!.GetValue<bool>());
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("[{\"role\":\"user\",\"content\":[{\"type\":\"input_text\",\"text\":\"Hello\"}]}]"),
            body["input"]));
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("[{\"type\":\"function\",\"name\":\"calculator\",\"parameters\":" + Calculator + ",\"strict\":false}]"),
            body["tools"]));

        // Content of the snapshot.
        var output = fixture["output"]!;
        Assert.Equal(2, result.Content.Count);
        var reasoning = Assert.IsType<GeneratedReasoning>(result.Content[0]);
        Assert.Equal(output[0]!["summary"]![0]!["text"]!.GetValue<string>(), reasoning.Text);
        JsonAssert.Equal(
            reasoning.ProviderMetadata!.Value,
            new JsonObject
            {
                ["azure"] = new JsonObject
                {
                    ["itemId"] = output[0]!["id"]!.GetValue<string>(),
                    ["reasoningEncryptedContent"] = output[0]!["encrypted_content"]!.GetValue<string>(),
                },
            }.ToJsonString());
        var text = Assert.IsType<Vercel.AI.OpenAI.OpenAIText>(result.Content[1]);
        Assert.Equal("(12 + 7) = 19\n19 × 3 = 57\n57 × 10 = 570\n\nFinal result: 570", text.Text);
        JsonAssert.Equal(
            text.ProviderMetadata,
            "{\"azure\":{\"itemId\":\"" + output[1]!["id"]!.GetValue<string>() + "\"}}");

        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Empty(result.Warnings);
        JsonAssert.Equal(
            result.ProviderMetadata!.Value,
            "{\"azure\":{\"responseId\":\"" + fixture["id"]!.GetValue<string>() + "\",\"serviceTier\":\"default\"}}");

        // Response of the snapshot.
        Assert.Equal(fixture["id"]!.GetValue<string>(), result.ResponseId);
        Assert.Equal("gpt-5-mini", result.ResponseModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1765587000), result.ResponseTimestamp);
        Assert.StartsWith("application/json", result.ResponseHeaders["content-type"]);

        // Usage of the snapshot.
        Assert.Equal(564, result.Usage.InputTokens);
        Assert.Equal(0, result.Usage.CacheReadTokens);
        Assert.Equal(101, result.Usage.OutputTokens);
        Assert.Equal(665, result.Usage.TotalTokens);
    }
}
