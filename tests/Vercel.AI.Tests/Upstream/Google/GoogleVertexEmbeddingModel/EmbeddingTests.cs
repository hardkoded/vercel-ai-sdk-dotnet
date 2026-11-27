// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.Google.GoogleVertexEmbeddingModel.GoogleVertexEmbeddingModelFixture;

namespace Vercel.AI.Tests.Upstream.Google.GoogleVertexEmbeddingModel;

/// <summary>Port of <c>google-vertex-embedding-model.test.ts</c> &gt; <c>GoogleVertexEmbeddingModel</c> &gt; <c>embedding</c>.</summary>
public sealed class EmbeddingTests
{
    private const string Prefix =
        "packages/google-vertex/src/google-vertex-embedding-model.test.ts::GoogleVertexEmbeddingModel > embedding::";

    private const string ExpectedPredictBody =
        "{\"instances\":[{\"content\":\"test text one\",\"task_type\":\"SEMANTIC_SIMILARITY\",\"title\":\"test title\"},{\"content\":\"test text two\",\"task_type\":\"SEMANTIC_SIMILARITY\",\"title\":\"test title\"}],\"parameters\":{\"autoTruncate\":false,\"outputDimensionality\":768}}";

    [Fact]
    [UpstreamTest(Prefix + "should extract embeddings", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_embeddings()
    {
        var result = await Model(Server())
            .DoEmbedAsync(
                Call(TestValues, Options("google", MockProviderOptions)),
                CancellationToken.None
            );

        AssertEmbeddings(result.Embeddings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return full result snapshot",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_return_full_result_snapshot()
    {
        var result = await Model(Server())
            .DoEmbedAsync(
                Call(TestValues, Options("google", MockProviderOptions)),
                CancellationToken.None
            );

        AssertEmbeddings(result.Embeddings);
        Assert.Equal(11, result.Tokens);
        Assert.Empty(result.Warnings);
        GoogleUpstream.JsonEqual(result.Response!.Body!.Value, Fixture);
        Assert.StartsWith("application/json", result.Response.Headers!["content-type"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should extract usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_usage()
    {
        var result = await Model(Server())
            .DoEmbedAsync(
                Call(TestValues, Options("google", MockProviderOptions)),
                CancellationToken.None
            );

        Assert.Equal(11, result.Tokens);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass the model parameters correctly",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_the_model_parameters_correctly()
    {
        var handler = Server();
        await Model(handler)
            .DoEmbedAsync(
                Call(TestValues, Options("google", MockProviderOptions)),
                CancellationToken.None
            );

        GoogleUpstream.JsonEqual(Body(handler), ExpectedPredictBody);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should accept googleVertex as provider options key",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_accept_googleVertex_as_provider_options_key()
    {
        var handler = Server();
        await Model(handler)
            .DoEmbedAsync(
                Call(TestValues, Options("googleVertex", MockProviderOptions)),
                CancellationToken.None
            );

        GoogleUpstream.JsonEqual(Body(handler), ExpectedPredictBody);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass the taskType setting in instances",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_the_taskType_setting_in_instances()
    {
        var handler = Server();
        await Model(handler)
            .DoEmbedAsync(
                Call(TestValues, Options("google", "{\"taskType\":\"SEMANTIC_SIMILARITY\"}")),
                CancellationToken.None
            );

        GoogleUpstream.JsonEqual(
            Body(handler),
            "{\"instances\":[{\"content\":\"test text one\",\"task_type\":\"SEMANTIC_SIMILARITY\"},{\"content\":\"test text two\",\"task_type\":\"SEMANTIC_SIMILARITY\"}],\"parameters\":{}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass the title setting in instances",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_the_title_setting_in_instances()
    {
        var handler = Server();
        await Model(handler)
            .DoEmbedAsync(
                Call(TestValues, Options("google", "{\"title\":\"test title\"}")),
                CancellationToken.None
            );

        GoogleUpstream.JsonEqual(
            Body(handler),
            "{\"instances\":[{\"content\":\"test text one\",\"title\":\"test title\"},{\"content\":\"test text two\",\"title\":\"test title\"}],\"parameters\":{}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should use embedContent for gemini-embedding-2",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_embedContent_for_gemini_embedding_2()
    {
        var handler = Server(
            "{\"embedding\":{\"values\":[0.1,0.2,0.3]},\"usageMetadata\":{\"promptTokenCount\":4}}"
        );

        var result = await Model(handler, "gemini-embedding-2")
            .DoEmbedAsync(
                Call(["test text one"], Options("google", MockProviderOptions)),
                CancellationToken.None
            );

        Assert.Equal(Gemini2Url, handler.Uris[0]);
        GoogleUpstream.JsonEqual(
            Body(handler),
            "{\"content\":{\"parts\":[{\"text\":\"test text one\"}]},\"embedContentConfig\":{\"autoTruncate\":false,\"outputDimensionality\":768,\"taskType\":\"SEMANTIC_SIMILARITY\",\"title\":\"test title\"}}"
        );
        Assert.Equal([0.1, 0.2, 0.3], Assert.Single(result.Embeddings));
        Assert.Equal(4, result.Tokens);
    }
}
