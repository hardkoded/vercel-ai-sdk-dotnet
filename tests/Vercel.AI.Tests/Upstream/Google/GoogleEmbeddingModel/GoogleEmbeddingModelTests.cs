// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleEmbeddingModel;

/// <summary>Port of <c>google-embedding-model.test.ts</c> &gt; <c>GoogleEmbeddingModel</c>.</summary>
public sealed class GoogleEmbeddingModelTests
{
    private const string Prefix =
        "packages/google/src/google-embedding-model.test.ts::GoogleEmbeddingModel::";
    private const string BatchResponse =
        "{\"embeddings\":[{\"values\":[0.1,0.2,0.3,0.4,0.5]},{\"values\":[0.6,0.7,0.8,0.9,1]}]}";
    private const string SingleResponse = "{\"embedding\":{\"values\":[0.1,0.2,0.3,0.4,0.5]}}";

    private static readonly string[] TestValues =
    [
        "sunny day at the beach",
        "rainy day in the city",
    ];

    private static (IEmbeddingModel Model, RecordingHandler Handler) Create(
        string response,
        string modelId = "gemini-embedding-001",
        GoogleOptions? options = null
    )
    {
        var handler = new RecordingHandler { ResponseText = response };
        var provider = GoogleProvider.Create(
            options ?? new GoogleOptions { ApiKey = "test-api-key" },
            handler
        );
        return (provider.EmbeddingModel(modelId), handler);
    }

    private static IReadOnlyDictionary<string, JsonElement> Google(string json)
    {
        return GoogleUpstream.ProviderOptions("{\"google\":" + json + "}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should expose the raw response", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_expose_the_raw_response()
    {
        var (model, handler) = Create(BatchResponse);
        handler.ResponseHeaders["test-header"] = "test-value";

        var result = await model.DoEmbedAsync(TestValues, null, CancellationToken.None);

        Assert.Equal("test-value", result.Response!.Headers["test-header"]);
        Assert.StartsWith("application/json", result.Response.Headers["content-type"]);
        GoogleUpstream.JsonEqual(JsonNode.Parse(result.Response.Body!), BatchResponse);
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_headers()
    {
        var providerOptions = new GoogleOptions { ApiKey = "test-api-key" };
        providerOptions.Headers["Custom-Provider-Header"] = "provider-header-value";
        var (model, handler) = Create(BatchResponse, options: providerOptions);

        await model.DoEmbedAsync(
            TestValues,
            null,
            CancellationToken.None,
            headers: new Dictionary<string, string?>
            {
                ["Custom-Request-Header"] = "request-header-value",
            }
        );

        Assert.Equal("provider-header-value", handler.RequestHeaders["custom-provider-header"]);
        Assert.Equal("request-header-value", handler.RequestHeaders["custom-request-header"]);
        Assert.Equal("test-api-key", handler.RequestHeaders["x-goog-api-key"]);
        Assert.StartsWith("application/json", handler.RequestHeaders["content-type"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should merge multimodal content for single embedding",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_merge_multimodal_content_for_single_embedding()
    {
        var (model, handler) = Create(SingleResponse);

        await model.DoEmbedAsync(
            [TestValues[0]],
            Google(
                "{\"content\":[[{\"inlineData\":{\"mimeType\":\"image/png\",\"data\":\"abc123\"}}]]}"
            ),
            CancellationToken.None
        );

        GoogleUpstream.JsonEqual(
            JsonNode.Parse(handler.Body),
            "{\"model\":\"models/gemini-embedding-001\",\"content\":{\"parts\":[{\"text\":\"sunny day at the beach\"},{\"inlineData\":{\"data\":\"abc123\",\"mimeType\":\"image/png\"}}]}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should merge per-value multimodal content for batch embedding",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_merge_per_value_multimodal_content_for_batch_embedding()
    {
        var (model, handler) = Create(BatchResponse);

        await model.DoEmbedAsync(
            TestValues,
            Google(
                "{\"content\":[[{\"inlineData\":{\"mimeType\":\"image/png\",\"data\":\"img1\"}}],[{\"inlineData\":{\"mimeType\":\"image/jpeg\",\"data\":\"img2\"}}]]}"
            ),
            CancellationToken.None
        );

        GoogleUpstream.JsonEqual(
            JsonNode.Parse(handler.Body),
            "{\"requests\":[{\"content\":{\"parts\":[{\"text\":\"sunny day at the beach\"},{\"inlineData\":{\"data\":\"img1\",\"mimeType\":\"image/png\"}}],\"role\":\"user\"},\"model\":\"models/gemini-embedding-001\"},{\"content\":{\"parts\":[{\"text\":\"rainy day in the city\"},{\"inlineData\":{\"data\":\"img2\",\"mimeType\":\"image/jpeg\"}}],\"role\":\"user\"},\"model\":\"models/gemini-embedding-001\"}]}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle null entries as text-only in batch embedding",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_null_entries_as_text_only_in_batch_embedding()
    {
        var (model, handler) = Create(BatchResponse);

        await model.DoEmbedAsync(
            TestValues,
            Google(
                "{\"content\":[[{\"inlineData\":{\"mimeType\":\"image/png\",\"data\":\"img1\"}}],null]}"
            ),
            CancellationToken.None
        );

        GoogleUpstream.JsonEqual(
            JsonNode.Parse(handler.Body),
            "{\"requests\":[{\"content\":{\"parts\":[{\"text\":\"sunny day at the beach\"},{\"inlineData\":{\"data\":\"img1\",\"mimeType\":\"image/png\"}}],\"role\":\"user\"},\"model\":\"models/gemini-embedding-001\"},{\"content\":{\"parts\":[{\"text\":\"rainy day in the city\"}],\"role\":\"user\"},\"model\":\"models/gemini-embedding-001\"}]}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should merge fileData content for single embedding",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_merge_fileData_content_for_single_embedding()
    {
        var (model, handler) = Create(SingleResponse, "gemini-embedding-2-preview");

        await model.DoEmbedAsync(
            [TestValues[0]],
            Google(
                "{\"content\":[[{\"fileData\":{\"fileUri\":\"gs://bucket/video.mp4\",\"mimeType\":\"video/mp4\"}}]]}"
            ),
            CancellationToken.None
        );

        GoogleUpstream.JsonEqual(
            JsonNode.Parse(handler.Body),
            "{\"content\":{\"parts\":[{\"text\":\"sunny day at the beach\"},{\"fileData\":{\"fileUri\":\"gs://bucket/video.mp4\",\"mimeType\":\"video/mp4\"}}]},\"model\":\"models/gemini-embedding-2-preview\"}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should merge fileData content for batch embedding",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_merge_fileData_content_for_batch_embedding()
    {
        var (model, handler) = Create(BatchResponse, "gemini-embedding-2-preview");

        await model.DoEmbedAsync(
            TestValues,
            Google(
                "{\"content\":[[{\"fileData\":{\"fileUri\":\"gs://bucket/video.mp4\",\"mimeType\":\"video/mp4\"}}],null]}"
            ),
            CancellationToken.None
        );

        GoogleUpstream.JsonEqual(
            JsonNode.Parse(handler.Body),
            "{\"requests\":[{\"content\":{\"parts\":[{\"text\":\"sunny day at the beach\"},{\"fileData\":{\"fileUri\":\"gs://bucket/video.mp4\",\"mimeType\":\"video/mp4\"}}],\"role\":\"user\"},\"model\":\"models/gemini-embedding-2-preview\"},{\"content\":{\"parts\":[{\"text\":\"rainy day in the city\"}],\"role\":\"user\"},\"model\":\"models/gemini-embedding-2-preview\"}]}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw error when content length does not match values length",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_throw_error_when_content_length_does_not_match_values_length()
    {
        var (model, _) = Create(BatchResponse);

        var error = await Assert.ThrowsAsync<AiSdkException>(() =>
            model.DoEmbedAsync(
                TestValues,
                Google(
                    "{\"content\":[[{\"inlineData\":{\"mimeType\":\"image/png\",\"data\":\"img1\"}}]]}"
                ),
                CancellationToken.None
            )
        );

        Assert.Equal(
            "The number of multimodal content entries (1) must match the number of values (2).",
            error.Message
        );
    }
}
