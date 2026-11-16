// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Cerebras;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests;

public sealed class CerebrasServiceTiersUpstreamTests
{
    private const string Prefix =
        "packages/cerebras/src/cerebras-chat-language-model.test.ts::service tiers > ";

    private const string ResponseBody =
        "{\"id\":\"chatcmpl-test\",\"model\":\"gpt-oss-120b\",\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"blue\"},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":1,\"completion_tokens\":1,\"total_tokens\":2}}";

    [Fact]
    [UpstreamTest(
        Prefix + "doGenerate::sends service_tier in the body and the queue_threshold header",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4a4e854fa66633dd7ce3f709d1d7aa7ecc693047"
    )]
    public async Task Sends_service_tier_in_the_body_and_the_queue_threshold_header()
    {
        var capture = Capture(ResponseBody);
        var options = Options("{\"serviceTier\":\"flex\",\"queueThreshold\":200}");
        await Chat(capture).DoGenerateAsync(options, CancellationToken.None);

        var body = UpstreamChat.Body(capture);
        Assert.Equal("flex", body["service_tier"]!.GetValue<string>());
        // camelCase passthrough keys must not leak into the body:
        Assert.Null(body["serviceTier"]);
        Assert.Null(body["queueThreshold"]);
        Assert.Equal("200", capture.Requests[0].Headers["queue_threshold"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "doGenerate::omits the queue_threshold header when queueThreshold is not set",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4a4e854fa66633dd7ce3f709d1d7aa7ecc693047"
    )]
    public async Task Omits_the_queue_threshold_header_when_queue_threshold_is_not_set()
    {
        var capture = Capture(ResponseBody);
        await Chat(capture)
            .DoGenerateAsync(Options("{\"serviceTier\":\"auto\"}"), CancellationToken.None);

        Assert.False(capture.Requests[0].Headers.ContainsKey("queue_threshold"));
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate::gives queueThreshold precedence over header casing without mutating callers",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4a4e854fa66633dd7ce3f709d1d7aa7ecc693047"
    )]
    public async Task Gives_queue_threshold_precedence_over_header_casing_without_mutating_callers()
    {
        var capture = Capture(ResponseBody);
        var providerOptions = new OpenAICompatibleOptions { ApiKey = "secret" };
        providerOptions.Headers["queue_threshold"] = "777";
        var model = (OpenAICompatibleLanguageModel)
            CerebrasProvider.Create(providerOptions, capture).LanguageModel("gpt-oss-120b");
        var headers = new Dictionary<string, string?>
        {
            ["queue_threshold"] = "999",
            ["QUEUE_THRESHOLD"] = "888",
            ["X-Request-ID"] = "request-123",
        };
        var options = Options("{\"queueThreshold\":200}");
        options.Headers = headers;

        await model.DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("200", capture.Requests[0].Headers["queue_threshold"]);
        Assert.Equal("request-123", capture.Requests[0].Headers["x-request-id"]);
        Assert.Equal(3, headers.Count);
        Assert.Equal("999", headers["queue_threshold"]);
        Assert.Equal("888", headers["QUEUE_THRESHOLD"]);
        Assert.Equal("request-123", headers["X-Request-ID"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "doGenerate::exposes the effective service tier from service_tier",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4a4e854fa66633dd7ce3f709d1d7aa7ecc693047"
    )]
    public async Task Exposes_the_effective_service_tier_from_service_tier()
    {
        var capture = Capture(WithTier("\"flex\""));
        var result = await Chat(capture)
            .DoGenerateAsync(Options("{\"serviceTier\":\"auto\"}"), CancellationToken.None);

        Assert.Equal(
            "flex",
            result
                .ProviderMetadata!.Value.GetProperty("cerebras")
                .GetProperty("serviceTier")
                .GetString()
        );
    }

    [Theory]
    [InlineData(null)]
    [InlineData("null")]
    [UpstreamTest(
        Prefix + "doGenerate::does not fabricate missing serviceTier metadata (%s)",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4a4e854fa66633dd7ce3f709d1d7aa7ecc693047"
    )]
    public async Task Does_not_fabricate_missing_service_tier_metadata(string? serviceTier)
    {
        var capture = Capture(serviceTier is null ? ResponseBody : WithTier(serviceTier));
        var result = await Chat(capture)
            .DoGenerateAsync(Options("{\"serviceTier\":\"auto\"}"), CancellationToken.None);

        Assert.False(
            result
                .ProviderMetadata!.Value.GetProperty("cerebras")
                .TryGetProperty("serviceTier", out _)
        );
    }

    [Theory]
    [InlineData(null)]
    [InlineData("null")]
    [UpstreamTest(
        Prefix + "doStream::does not fabricate missing streamed tier metadata (%s)",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4a4e854fa66633dd7ce3f709d1d7aa7ecc693047"
    )]
    public async Task Does_not_fabricate_missing_streamed_tier_metadata(string? serviceTier)
    {
        var capture = StreamCapture(null, serviceTier);
        var result = await Chat(capture)
            .DoStreamAsync(Options("{\"serviceTier\":\"auto\"}"), CancellationToken.None);
        var parts = await UpstreamChat.Read(result.Stream);

        var finish = Assert.Single(parts.OfType<FinishStreamPart>());
        Assert.False(
            finish
                .ProviderMetadata!.Value.GetProperty("cerebras")
                .TryGetProperty("serviceTier", out _)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "doStream::uses the final returned tier while preserving prediction metadata",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4a4e854fa66633dd7ce3f709d1d7aa7ecc693047"
    )]
    public async Task Uses_the_final_returned_tier_while_preserving_prediction_metadata()
    {
        var capture = StreamCapture(
            "auto",
            "\"flex\"",
            "{\"prompt_tokens\":1,\"completion_tokens\":1,\"total_tokens\":2,\"completion_tokens_details\":{\"accepted_prediction_tokens\":1,\"rejected_prediction_tokens\":0}}"
        );
        var result = await Chat(capture)
            .DoStreamAsync(UpstreamChat.Prompt(), CancellationToken.None);
        var parts = await UpstreamChat.Read(result.Stream);

        var finish = Assert.Single(parts.OfType<FinishStreamPart>());
        JsonAssert.Equal(
            finish.ProviderMetadata!.Value,
            "{\"cerebras\":{\"serviceTier\":\"flex\",\"acceptedPredictionTokens\":1,\"rejectedPredictionTokens\":0}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "doStream::sends the queue_threshold header and exposes the effective tier",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4a4e854fa66633dd7ce3f709d1d7aa7ecc693047"
    )]
    public async Task Sends_the_queue_threshold_header_and_exposes_the_effective_tier()
    {
        var capture = StreamCapture(null, "\"flex\"");
        var result = await Chat(capture)
            .DoStreamAsync(
                Options("{\"serviceTier\":\"flex\",\"queueThreshold\":150}"),
                CancellationToken.None
            );
        var parts = await UpstreamChat.Read(result.Stream);

        Assert.Equal("150", capture.Requests[0].Headers["queue_threshold"]);
        var finish = Assert.Single(parts.OfType<FinishStreamPart>());
        Assert.Equal(
            "flex",
            finish
                .ProviderMetadata!.Value.GetProperty("cerebras")
                .GetProperty("serviceTier")
                .GetString()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "option validation::rejects a queueThreshold below the valid range",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4a4e854fa66633dd7ce3f709d1d7aa7ecc693047"
    )]
    public async Task Rejects_a_queue_threshold_below_the_valid_range()
    {
        await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            Chat(Capture(ResponseBody))
                .DoGenerateAsync(Options("{\"queueThreshold\":5}"), CancellationToken.None)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "option validation::rejects a queueThreshold above the valid range",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4a4e854fa66633dd7ce3f709d1d7aa7ecc693047"
    )]
    public async Task Rejects_a_queue_threshold_above_the_valid_range()
    {
        await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            Chat(Capture(ResponseBody))
                .DoGenerateAsync(Options("{\"queueThreshold\":25000}"), CancellationToken.None)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "option validation::rejects a non-integer queueThreshold",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4a4e854fa66633dd7ce3f709d1d7aa7ecc693047"
    )]
    public async Task Rejects_a_non_integer_queue_threshold()
    {
        await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            Chat(Capture(ResponseBody))
                .DoGenerateAsync(Options("{\"queueThreshold\":100.5}"), CancellationToken.None)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "option validation::accepts the inclusive queueThreshold boundaries",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4a4e854fa66633dd7ce3f709d1d7aa7ecc693047"
    )]
    public async Task Accepts_the_inclusive_queue_threshold_boundaries()
    {
        Assert.NotNull(
            await Chat(Capture(ResponseBody))
                .DoGenerateAsync(Options("{\"queueThreshold\":50}"), CancellationToken.None)
        );
        Assert.NotNull(
            await Chat(Capture(ResponseBody))
                .DoGenerateAsync(Options("{\"queueThreshold\":20000}"), CancellationToken.None)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "option validation::preserves the existing runtime option pass-through",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4a4e854fa66633dd7ce3f709d1d7aa7ecc693047"
    )]
    public async Task Preserves_the_existing_runtime_option_pass_through()
    {
        var capture = Capture(ResponseBody);
        var longKey = new string('x', 1025);
        await Chat(capture)
            .DoGenerateAsync(
                Options(
                    "{\"queueThreshold\":200,\"serviceTier\":\"future-tier\",\"reasoningEffort\":null,\"topLogprobs\":21,\"promptCacheKey\":\""
                        + longKey
                        + "\"}"
                ),
                CancellationToken.None
            );

        var body = UpstreamChat.Body(capture);
        Assert.Equal("future-tier", body["service_tier"]!.GetValue<string>());
        Assert.True(body.ContainsKey("reasoning_effort"));
        Assert.Null(body["reasoning_effort"]);
        Assert.Equal(21, body["top_logprobs"]!.GetValue<int>());
        Assert.Equal(longKey, body["prompt_cache_key"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "option validation::rejects invalid options on the streaming path too",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4a4e854fa66633dd7ce3f709d1d7aa7ecc693047"
    )]
    public async Task Rejects_invalid_options_on_the_streaming_path_too()
    {
        await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            Chat(StreamCapture(null, "\"flex\""))
                .DoStreamAsync(Options("{\"queueThreshold\":5}"), CancellationToken.None)
        );
    }

    private const string ProviderPrefix =
        "packages/cerebras/src/cerebras-provider.test.ts::CerebrasProvider > service tier options::";

    [Fact]
    [UpstreamTest(
        ProviderPrefix + "maps serviceTier to service_tier in the request body",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4a4e854fa66633dd7ce3f709d1d7aa7ecc693047"
    )]
    public void Maps_service_tier_to_service_tier_in_the_request_body()
    {
        var body = TransformBody(
            "{\"model\":\"model-id\",\"messages\":[{\"role\":\"user\",\"content\":\"hi\"}],\"serviceTier\":\"flex\"}"
        );

        Assert.Equal("flex", body["service_tier"]!.GetValue<string>());
        Assert.False(body.ContainsKey("serviceTier"));
    }

    [Fact]
    [UpstreamTest(
        ProviderPrefix + "strips queueThreshold from the request body",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4a4e854fa66633dd7ce3f709d1d7aa7ecc693047"
    )]
    public void Strips_queue_threshold_from_the_request_body()
    {
        var body = TransformBody(
            "{\"model\":\"model-id\",\"messages\":[{\"role\":\"user\",\"content\":\"hi\"}],\"serviceTier\":\"auto\",\"queueThreshold\":200}"
        );

        Assert.Equal("auto", body["service_tier"]!.GetValue<string>());
        Assert.False(body.ContainsKey("queueThreshold"));
        Assert.False(body.ContainsKey("queue_threshold"));
    }

    [Fact]
    [UpstreamTest(
        ProviderPrefix + "does not add service_tier when serviceTier is absent",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4a4e854fa66633dd7ce3f709d1d7aa7ecc693047"
    )]
    public void Does_not_add_service_tier_when_service_tier_is_absent()
    {
        var body = TransformBody(
            "{\"model\":\"model-id\",\"messages\":[{\"role\":\"user\",\"content\":\"hi\"}]}"
        );

        Assert.False(body.ContainsKey("service_tier"));
    }

    [Fact]
    [UpstreamTest(
        ProviderPrefix + "wires a metadata extractor that surfaces the effective tier",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4a4e854fa66633dd7ce3f709d1d7aa7ecc693047"
    )]
    public void Wires_a_metadata_extractor_that_surfaces_the_effective_tier()
    {
        var extractor = CerebrasProvider
            .Create(new OpenAICompatibleOptions { ApiKey = "secret" })
            .Options.MetadataExtractor;

        Assert.NotNull(extractor);
        Assert.Equal(
            "{\"cerebras\":{\"serviceTier\":\"flex\"}}",
            extractor!
                .ExtractMetadata(UpstreamChat.Json("{\"service_tier\":\"flex\"}"))!
                .ToJsonString()
        );
        Assert.Null(extractor.ExtractMetadata(UpstreamChat.Json("{}")));
    }

    [Fact]
    [UpstreamTest(
        ProviderPrefix + "builds streamed metadata only when the tier is returned",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4a4e854fa66633dd7ce3f709d1d7aa7ecc693047"
    )]
    public void Builds_streamed_metadata_only_when_the_tier_is_returned()
    {
        var extractor = CerebrasProvider
            .Create(new OpenAICompatibleOptions { ApiKey = "secret" })
            .Options.MetadataExtractor!;

        var withTier = extractor.CreateStreamExtractor();
        withTier.ProcessChunk(
            UpstreamChat.Json("{\"choices\":[{\"delta\":{\"content\":\"blue\"}}]}")
        );
        withTier.ProcessChunk(UpstreamChat.Json("{\"service_tier\":\"flex\"}"));
        Assert.Equal(
            "{\"cerebras\":{\"serviceTier\":\"flex\"}}",
            withTier.BuildMetadata()!.ToJsonString()
        );

        var withoutTier = extractor.CreateStreamExtractor();
        withoutTier.ProcessChunk(
            UpstreamChat.Json("{\"choices\":[{\"delta\":{\"content\":\"blue\"}}]}")
        );
        Assert.Null(withoutTier.BuildMetadata());
    }

    private static JsonObject TransformBody(string json)
    {
        var options = CerebrasProvider
            .Create(new OpenAICompatibleOptions { ApiKey = "secret" })
            .Options;
        return options.TransformRequestBody!(
            (JsonObject)JsonNode.Parse(json)!,
            new List<CallWarning>()
        );
    }

    private static string WithTier(string tierJson)
    {
        return ResponseBody.Substring(0, ResponseBody.Length - 1)
            + ",\"service_tier\":"
            + tierJson
            + "}";
    }

    private static UpstreamCapture Capture(string body)
    {
        return new UpstreamCapture { ResponseBody = body };
    }

    // Upstream SERVICE_TIER_STREAM_CHUNKS with the tier of each chunk overridden: the first chunk gets
    // firstTier and the last gets lastTier. A null tier writes JSON null; the test data uses the string "null".
    private static UpstreamCapture StreamCapture(
        string? firstTier,
        string? lastTier,
        string? usage = null
    )
    {
        static string Tier(string? tier) =>
            tier is null
                ? string.Empty
                : ",\"service_tier\":"
                    + (tier.StartsWith('"') || tier == "null" ? tier : "\"" + tier + "\"");
        var first =
            "{\"id\":\"chatcmpl-test\",\"model\":\"gpt-oss-120b\",\"choices\":[{\"delta\":{\"role\":\"assistant\",\"content\":\"blue\"}}]"
            + Tier(firstTier)
            + "}";
        var last =
            "{\"id\":\"chatcmpl-test\",\"model\":\"gpt-oss-120b\",\"choices\":[{\"delta\":{},\"finish_reason\":\"stop\"}],\"usage\":"
            + (usage ?? "{\"prompt_tokens\":1,\"completion_tokens\":1,\"total_tokens\":2}")
            + Tier(lastTier)
            + "}";
        return new UpstreamCapture
        {
            MediaType = "text/event-stream",
            ResponseBody = UpstreamChat.Sse(first, last),
        };
    }

    private static LanguageModelCallOptions Options(string cerebrasOptions)
    {
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag("cerebras", cerebrasOptions);
        return options;
    }

    private static OpenAICompatibleLanguageModel Chat(UpstreamCapture capture)
    {
        return (OpenAICompatibleLanguageModel)
            CerebrasProvider
                .Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture)
                .LanguageModel("gpt-oss-120b");
    }
}
