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
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_generate_with_reasoning_encrypted_content()
    {
        var fixture = JsonNode.Parse(Fixture("azure-reasoning-encrypted-content.1.json"))!;
        var capture = new OpenAICapture { ResponseJson = fixture.ToJsonString() };
        capture.ResponseHeaders["content-type"] = "application/json";

        var options = Call(
            new[] { new LanguageModelTool("calculator", null, Json(Calculator)) },
            azureOptions: "{\"reasoningEffort\":\"high\",\"maxCompletionTokens\":32000,\"store\":false,\"include\":[\"reasoning.encrypted_content\"],\"reasoningSummary\":\"auto\",\"forceReasoning\":true}"
        );
        var result = await Generate(capture, options);

        // Request body of the snapshot (undefined fields are omitted).
        var expectedBody = JsonNode.Parse(
            "{\"model\":\"test-deployment\",\"input\":[{\"role\":\"user\",\"content\":[{\"type\":\"input_text\",\"text\":\"Hello\"}]}],"
                + "\"include\":[\"reasoning.encrypted_content\"],\"reasoning\":{\"effort\":\"high\",\"summary\":\"auto\"},\"store\":false,"
                + "\"tools\":[{\"type\":\"function\",\"name\":\"calculator\",\"parameters\":"
                + Calculator
                + ",\"strict\":false}]}"
        )!;
        var body = RequestBody(capture);
        Assert.True(JsonNode.DeepEquals(expectedBody, body), body.ToJsonString());

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
                    ["reasoningEncryptedContent"] = output[0]![
                        "encrypted_content"
                    ]!.GetValue<string>(),
                },
            }.ToJsonString()
        );
        var text = Assert.IsType<Vercel.AI.OpenAI.OpenAIText>(result.Content[1]);
        Assert.Equal("(12 + 7) = 19\n19 × 3 = 57\n57 × 10 = 570\n\nFinal result: 570", text.Text);
        JsonAssert.Equal(
            text.ProviderMetadata,
            "{\"azure\":{\"itemId\":\"" + output[1]!["id"]!.GetValue<string>() + "\"}}"
        );

        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Empty(result.Warnings);
        JsonAssert.Equal(
            result.ProviderMetadata!.Value,
            "{\"azure\":{\"responseId\":\""
                + fixture["id"]!.GetValue<string>()
                + "\",\"serviceTier\":\"default\"}}"
        );

        // Response of the snapshot.
        Assert.Equal(fixture["id"]!.GetValue<string>(), result.ResponseId);
        Assert.Equal("gpt-5-mini", result.ResponseModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1765587000), result.ResponseTimestamp);
        Assert.StartsWith("application/json", result.ResponseHeaders["content-type"]);

        // Response body of the snapshot.
        Assert.True(JsonNode.DeepEquals(fixture, JsonNode.Parse(result.RawResponse!)));

        // Usage of the snapshot.
        Assert.Equal(564, result.Usage.NoCacheInputTokens);
        Assert.Equal(564, result.Usage.InputTokens);
        Assert.Equal(0, result.Usage.CacheReadTokens);
        Assert.Null(result.Usage.CacheWriteTokens);
        Assert.Equal(64, result.Usage.ReasoningTokens);
        Assert.Equal(101, result.Usage.OutputTokens);
        Assert.Equal(37, result.Usage.TextTokens);
        Assert.Equal(665, result.Usage.TotalTokens);
        Assert.True(
            JsonNode.DeepEquals(
                fixture["usage"],
                JsonNode.Parse(result.Usage.Raw!.Value.GetRawText())
            )
        );
    }
}
