// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;
using BedrockProvider = Vercel.AI.AmazonBedrock.AmazonBedrockProvider;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockProvider;

public sealed class ProviderMethodsTests
{
    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-provider.test.ts::AmazonBedrockProvider > provider methods::should pass chat model settings to the model",
        Coverage = UpstreamCoverage.Partial,
        Commit = "f35668be8bd5fffafb1461f34c5d4b1ca9a43beb"
    )]
    public async Task Should_pass_chat_model_settings_to_the_model()
    {
        // Upstream mocks the model constructor and asserts the `modelFamily` it receives. The port
        // cannot mock a constructor, so it checks that the declared family changes the request.
        var handler = new UpstreamRecordingHandler(
            "{\"output\":{\"message\":{\"role\":\"assistant\",\"content\":[{\"text\":\"ok\"}]}},\"stopReason\":\"end_turn\",\"usage\":{\"inputTokens\":1,\"outputTokens\":1,\"totalTokens\":2}}"
        );
        var provider = BedrockProvider.Create(
            new AmazonBedrockOptions
            {
                AccessKeyId = "test-access-key",
                SecretAccessKey = "test-secret",
            },
            handler
        );
        var model = provider.LanguageModel(
            "arn:aws:bedrock:us-east-1:123456789012:application-inference-profile/qibm5eutlkcy",
            new AmazonBedrockChatModelSettings
            {
                ModelFamily = AmazonBedrockChatModelFamily.Anthropic,
            }
        );
        using var schema = JsonDocument.Parse("{\"type\":\"object\"}");

        await model.DoGenerateAsync(
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
                Tools = [new LanguageModelTool("lookup", null, schema.RootElement.Clone())],
                ToolChoice = ToolChoice.Tool("lookup"),
            },
            CancellationToken.None
        );

        var body = JsonNode.Parse(handler.Body)!;
        Assert.Equal(
            "tool",
            body["additionalModelRequestFields"]!["tool_choice"]!["type"]!.GetValue<string>()
        );
    }
}
