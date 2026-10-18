// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;
using BedrockEmbeddingModel = Vercel.AI.AmazonBedrock.AmazonBedrockEmbeddingModel;
using BedrockProvider = Vercel.AI.AmazonBedrock.AmazonBedrockProvider;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockEmbeddingModel;

/// <summary>Port of <c>amazon-bedrock-embedding-model.test.ts</c> &gt; <c>doEmbed</c>.</summary>
public sealed class DoEmbedTests
{
    private const string Prefix = "packages/amazon-bedrock/src/amazon-bedrock-embedding-model.test.ts::doEmbed::";

    private const string CohereApplicationProfileArn = "arn:aws:bedrock:us-east-1:123456789012:application-inference-profile/qibm5eutlkcy";

    private const string TitanResponse = "{\"embedding\":[-0.09,0.05,-0.02,0.01,0.04],\"inputTextTokenCount\":8}";

    private const string CohereV3Response = "{\"embeddings\":[[-0.09,0.05,-0.02,0.01,0.04]]}";

    private const string CohereV4Response = "{\"embeddings\":{\"float\":[[-0.09,0.05,-0.02,0.01,0.04]]}}";

    private const string CohereV4MultipleResponse = "{\"embeddings\":{\"float\":[[-0.09,0.05,-0.02,0.01,0.04],[-0.08,0.06,-0.03,0.02,0.03]]}}";

    private static readonly string[] TestValues = { "sunny day at the beach", "rainy day in the city" };

    private static readonly float[] FirstEmbedding = { -0.09f, 0.05f, -0.02f, 0.01f, 0.04f };

    private static readonly float[] SecondEmbedding = { -0.08f, 0.06f, -0.03f, 0.02f, 0.03f };

    [Fact]
    [UpstreamTest(Prefix + "should expose model-specific max embeddings per call", Coverage = UpstreamCoverage.Covered)]
    public void Should_expose_model_specific_max_embeddings_per_call()
    {
        var provider = Provider(new UpstreamRecordingHandler(TitanResponse));

        Assert.Equal(1, Model(provider, "amazon.titan-embed-text-v2:0").MaxEmbeddingsPerCall);
        Assert.Equal(96, Model(provider, "cohere.embed-english-v3").MaxEmbeddingsPerCall);
        Assert.Equal(96, Model(provider, "us.cohere.embed-v4:0").MaxEmbeddingsPerCall);
        Assert.Equal(1, Model(provider, "amazon.nova-2-multimodal-embeddings-v1:0").MaxEmbeddingsPerCall);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle single input value and return embeddings", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_single_input_value_and_return_embeddings()
    {
        var handler = new UpstreamRecordingHandler(TitanResponse);

        var result = await Model(Provider(handler), "amazon.titan-embed-text-v2:0").DoEmbedAsync(new[] { TestValues[0] }, null, CancellationToken.None);

        Assert.Single(result.Embeddings);
        Assert.Equal(FirstEmbedding, result.Embeddings[0]);
        JsonAssert.Equal(JsonNode.Parse(handler.Body), "{\"inputText\":\"sunny day at the beach\"}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle single input value and extract usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_single_input_value_and_extract_usage()
    {
        var result = await Model(Provider(new UpstreamRecordingHandler(TitanResponse)), "amazon.titan-embed-text-v2:0").DoEmbedAsync(new[] { TestValues[0] }, null, CancellationToken.None);

        Assert.Equal(8, result.Tokens);
    }

    [Fact]
    [UpstreamTest(Prefix + "should support Cohere embedding models", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_support_Cohere_embedding_models()
    {
        var handler = Handler(CohereV3Response, "6");

        var result = await Model(Provider(handler), "cohere.embed-english-v3").DoEmbedAsync(new[] { TestValues[0] }, null, CancellationToken.None);

        Assert.Single(result.Embeddings);
        Assert.Equal(FirstEmbedding, result.Embeddings[0]);
        Assert.Equal(6, result.Tokens);
        JsonAssert.Equal(JsonNode.Parse(handler.Body), "{\"input_type\":\"search_query\",\"texts\":[\"sunny day at the beach\"]}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should support Cohere v4 embedding models", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_support_Cohere_v4_embedding_models()
    {
        var handler = Handler(CohereV4Response, "6");

        var result = await Model(Provider(handler), "cohere.embed-v4:0").DoEmbedAsync(new[] { TestValues[0] }, null, CancellationToken.None);

        Assert.Single(result.Embeddings);
        Assert.Equal(FirstEmbedding, result.Embeddings[0]);
        Assert.Equal(6, result.Tokens);
        JsonAssert.Equal(JsonNode.Parse(handler.Body), "{\"input_type\":\"search_query\",\"texts\":[\"sunny day at the beach\"]}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should send multiple values for Cohere embedding models", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_multiple_values_for_Cohere_embedding_models()
    {
        var handler = Handler(CohereV4MultipleResponse, "12");

        var result = await Model(Provider(handler), "cohere.embed-v4:0").DoEmbedAsync(TestValues, null, CancellationToken.None);

        Assert.Equal(2, result.Embeddings.Count);
        Assert.Equal(FirstEmbedding, result.Embeddings[0]);
        Assert.Equal(SecondEmbedding, result.Embeddings[1]);
        Assert.Equal(12, result.Tokens);
        JsonAssert.Equal(JsonNode.Parse(handler.Body), "{\"input_type\":\"search_query\",\"texts\":[\"sunny day at the beach\",\"rainy day in the city\"]}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should return NaN usage for Cohere models when the token count header is absent", Coverage = UpstreamCoverage.Covered, Note = "EmbeddingResult.Tokens is int?, so the missing count is null. Embed and EmbedMany turn it into NaN.")]
    public async Task Should_return_NaN_usage_for_Cohere_models_when_the_token_count_header_is_absent()
    {
        var result = await Model(Provider(new UpstreamRecordingHandler(CohereV4Response)), "cohere.embed-v4:0").DoEmbedAsync(new[] { TestValues[0] }, null, CancellationToken.None);

        Assert.Null(result.Tokens);
    }

    [Fact]
    [UpstreamTest(Prefix + "should support Cohere models behind cross-region inference profile ids", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_support_Cohere_models_behind_cross_region_inference_profile_ids()
    {
        var handler = Handler(CohereV4Response, "6");

        var result = await Model(Provider(handler), "us.cohere.embed-v4:0").DoEmbedAsync(new[] { TestValues[0] }, null, CancellationToken.None);

        Assert.Single(result.Embeddings);
        Assert.Equal(6, result.Tokens);
        Assert.Equal("https://bedrock-runtime.us-east-1.amazonaws.com/model/us.cohere.embed-v4%3A0/invoke", handler.Uri);
        JsonAssert.Equal(JsonNode.Parse(handler.Body), "{\"input_type\":\"search_query\",\"texts\":[\"sunny day at the beach\"]}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should support Cohere models behind application inference profile ARNs", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_support_Cohere_models_behind_application_inference_profile_ARNs()
    {
        var handler = Handler(CohereV4MultipleResponse, "12");
        var model = Provider(handler).EmbeddingModel(CohereApplicationProfileArn, new AmazonBedrockEmbeddingModelSettings { ModelFamily = AmazonBedrockEmbeddingModelFamily.Cohere });

        var result = await model.DoEmbedAsync(new[] { TestValues[0] }, null, CancellationToken.None);

        Assert.Equal(2, result.Embeddings.Count);
        Assert.Equal(FirstEmbedding, result.Embeddings[0]);
        Assert.Equal(SecondEmbedding, result.Embeddings[1]);
        Assert.Equal(12, result.Tokens);
        Assert.Equal("https://bedrock-runtime.us-east-1.amazonaws.com/model/" + Uri.EscapeDataString(CohereApplicationProfileArn) + "/invoke", handler.Uri);
        JsonAssert.Equal(JsonNode.Parse(handler.Body), "{\"input_type\":\"search_query\",\"texts\":[\"sunny day at the beach\"]}");
        Assert.Equal(96, model.MaxEmbeddingsPerCall);
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass outputDimension for Cohere v4 embedding models", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_outputDimension_for_Cohere_v4_embedding_models()
    {
        var handler = Handler(CohereV4Response, "6");

        var result = await Model(Provider(handler), "cohere.embed-v4:0").DoEmbedAsync(new[] { TestValues[0] }, EmbeddingDimensionCases.Options("bedrock", "outputDimension", 256), CancellationToken.None);

        Assert.Single(result.Embeddings);
        Assert.Equal(FirstEmbedding, result.Embeddings[0]);
        JsonAssert.Equal(JsonNode.Parse(handler.Body), "{\"input_type\":\"search_query\",\"texts\":[\"sunny day at the beach\"],\"output_dimension\":256}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should properly combine headers from all sources", Coverage = UpstreamCoverage.Covered, Note = "The port signs the request itself, so the SigV4 x-amz-date and Authorization headers stand in for the signed headers that the test fetch injects.")]
    public async Task Should_properly_combine_headers_from_all_sources()
    {
        var handler = new UpstreamRecordingHandler(TitanResponse);
        var provider = Provider(handler);
        provider.Options.Headers["model-header"] = "model-value";
        provider.Options.Headers["shared-header"] = "model-shared";
        var model = Model(provider, "amazon.titan-embed-text-v2:0");
        var headers = new Dictionary<string, string?>
        {
            ["options-header"] = "options-value",
            ["shared-header"] = "options-shared",
        };

        await model.DoEmbedAsync(new EmbeddingCallOptions(new[] { TestValues[0] }) { Headers = headers }, CancellationToken.None);

        Assert.Equal("options-value", handler.Headers["options-header"]);
        Assert.Equal("model-value", handler.Headers["model-header"]);
        Assert.True(handler.Headers.ContainsKey("x-amz-date"));
        Assert.StartsWith("AWS4-HMAC-SHA256 ", handler.Headers["Authorization"]);
        Assert.Equal("options-shared", handler.Headers["shared-header"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should work with partial headers", Coverage = UpstreamCoverage.Covered, Note = "The port signs the request itself, so the SigV4 x-amz-date and Authorization headers stand in for the signed headers that the test fetch injects.")]
    public async Task Should_work_with_partial_headers()
    {
        var handler = new UpstreamRecordingHandler(TitanResponse);
        var provider = Provider(handler);
        provider.Options.Headers["model-header"] = "model-value";

        await Model(provider, "amazon.titan-embed-text-v2:0").DoEmbedAsync(new[] { TestValues[0] }, null, CancellationToken.None);

        Assert.Equal("model-value", handler.Headers["model-header"]);
        Assert.True(handler.Headers.ContainsKey("x-amz-date"));
        Assert.StartsWith("AWS4-HMAC-SHA256 ", handler.Headers["Authorization"]);
    }

    private static UpstreamRecordingHandler Handler(string body, string tokenCount)
    {
        var handler = new UpstreamRecordingHandler(body);
        handler.ResponseHeaders["x-amzn-bedrock-input-token-count"] = tokenCount;
        return handler;
    }

    private static BedrockProvider Provider(UpstreamRecordingHandler handler)
    {
        return BedrockProvider.Create(new AmazonBedrockOptions { AccessKeyId = "AKIA", SecretAccessKey = "secret" }, handler);
    }

    private static BedrockEmbeddingModel Model(BedrockProvider provider, string modelId)
    {
        return (BedrockEmbeddingModel)provider.EmbeddingModel(modelId);
    }
}
