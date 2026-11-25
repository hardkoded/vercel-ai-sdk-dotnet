// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.AmazonBedrock;
using Vercel.AI.Anthropic;
using BedrockProvider = Vercel.AI.AmazonBedrock.AmazonBedrockProvider;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockProvider;

[Collection("BedrockEndpointEnvironment")]
public sealed class ProviderMethodsTests : BedrockEndpointEnvironment
{
    private const string File =
        "packages/amazon-bedrock/src/amazon-bedrock-provider.test.ts::AmazonBedrockProvider > provider methods::";

    private const string ProfileArn =
        "arn:aws:bedrock:us-east-1:123456789012:application-inference-profile/qibm5eutlkcy";

    [Fact]
    [UpstreamTest(File + "should create an embedding model", Coverage = UpstreamCoverage.Covered)]
    public void Should_create_an_embedding_model()
    {
        var provider = BedrockProvider.Create();
        var modelId = "amazon.titan-embed-text-v1";

        var model = provider.EmbeddingModel(modelId, null);

        Assert.Equal(modelId, model.ModelId);
        Assert.IsType<global::Vercel.AI.AmazonBedrock.AmazonBedrockEmbeddingModel>(model);
    }

    [Fact]
    [UpstreamTest(
        File + "should pass chat model settings to the model",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_pass_chat_model_settings_to_the_model()
    {
        var provider = BedrockProvider.Create();

        var model = provider.LanguageModel(
            ProfileArn,
            new AmazonBedrockChatModelSettings
            {
                ModelFamily = AmazonBedrockChatModelFamily.Anthropic,
            }
        );

        Assert.Equal(AmazonBedrockChatModelFamily.Anthropic, model.ModelFamily);
    }

    [Fact]
    [UpstreamTest(
        File + "should pass embedding model settings to the model",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_pass_embedding_model_settings_to_the_model()
    {
        var provider = BedrockProvider.Create();

        var model = provider.EmbeddingModel(
            ProfileArn,
            new AmazonBedrockEmbeddingModelSettings
            {
                ModelFamily = AmazonBedrockEmbeddingModelFamily.Cohere,
            }
        );

        // Cohere takes 96 values per call, the default family for this ARN takes one.
        Assert.Equal(96, model.MaxEmbeddingsPerCall);
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-provider.test.ts::AmazonBedrockProvider > provider methods::should expose anthropicTools",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_expose_anthropicTools()
    {
        var provider = BedrockProvider.Create();
        Assert.Same(AnthropicTools.Factories, provider.Tools);
    }
}
