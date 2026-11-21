// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockChatLanguageModel;

public sealed class ApplicationInferenceProfileStructuredOutputTests
{
    private const string ApplicationProfileArn =
        "arn:aws:bedrock:us-east-1:123456789012:application-inference-profile/custom-profile";

    [Theory]
    [InlineData("outputFormat")]
    [InlineData("auto")]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::application inference profile structured output::uses native structured output in %s mode when the Anthropic family is declared",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f35668be8bd5fffafb1461f34c5d4b1ca9a43beb"
    )]
    public async Task Uses_native_structured_output_in_mode_when_the_Anthropic_family_is_declared(
        string structuredOutputMode
    )
    {
        var handler = new UpstreamRecordingHandler(
            "{\"output\":{\"message\":{\"role\":\"assistant\",\"content\":[{\"text\":\"{\\\"name\\\":\\\"Test\\\"}\"}]}},\"stopReason\":\"end_turn\",\"usage\":{\"inputTokens\":1,\"outputTokens\":1,\"totalTokens\":2}}"
        );
        var provider = global::Vercel.AI.AmazonBedrock.AmazonBedrockProvider.Create(
            new AmazonBedrockOptions
            {
                AccessKeyId = "test-access-key",
                SecretAccessKey = "test-secret",
            },
            handler
        );
        var model = provider.LanguageModel(
            ApplicationProfileArn,
            new AmazonBedrockChatModelSettings
            {
                ModelFamily = AmazonBedrockChatModelFamily.Anthropic,
            }
        );
        using var schema = JsonDocument.Parse(
            "{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}},\"required\":[\"name\"]}"
        );

        await model.DoGenerateAsync(
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
                JsonSchema = schema.RootElement.Clone(),
                ProviderOptions = new Dictionary<string, JsonElement>
                {
                    ["amazonBedrock"] = JsonDocument
                        .Parse("{\"structuredOutputMode\":\"" + structuredOutputMode + "\"}")
                        .RootElement.Clone(),
                },
            },
            CancellationToken.None
        );

        var body = JsonNode.Parse(handler.Body)!;
        Assert.Null(body["toolConfig"]);
        var format = body["additionalModelRequestFields"]!["output_config"]!["format"]!;
        Assert.Equal("json_schema", format["type"]!.GetValue<string>());
        Assert.Equal("object", format["schema"]!["type"]!.GetValue<string>());
        Assert.Equal("name", format["schema"]!["required"]![0]!.GetValue<string>());
    }
}
