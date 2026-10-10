// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Moonshot;
using Vercel.AI.OpenAICompatible;

namespace Vercel.AI.Tests.MoonshotChatLanguageModelTests;

/// <summary>Port of <c>moonshotai-chat-language-model.test.ts</c> &gt; <c>doGenerate &gt; structured outputs</c>.</summary>
public sealed class DoGenerateStructuredOutputsTests
{
    [Theory]
    [InlineData("custom-model-id")]
    [InlineData("moonshot-v2")]
    [InlineData("kimi-next")]
    [UpstreamTest(
        "packages/moonshotai/src/moonshotai-chat-language-model.test.ts::doGenerate::should preserve JSON schema output for unknown model %s",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_JSON_schema_output_for_unknown_model(string modelId)
    {
        var capture = new UpstreamCapture();
        var provider = MoonshotProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture);
        var options = UpstreamChat.Prompt();
        options.JsonSchema = UpstreamCapture.Json("{\"type\":\"object\",\"properties\":{}}");
        options.JsonSchemaName = "response";

        var result = await ((OpenAICompatibleLanguageModel)provider.LanguageModel(modelId)).DoGenerateAsync(options, CancellationToken.None);

        var format = JsonNode.Parse(capture.Requests[0].Body)!["response_format"]!;
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("{\"type\":\"json_schema\",\"json_schema\":{\"name\":\"response\",\"schema\":{\"type\":\"object\",\"properties\":{}},\"strict\":true}}"),
            format));
        Assert.Empty(result.Warnings);
    }
}
