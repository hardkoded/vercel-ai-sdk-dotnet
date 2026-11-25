// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Moonshot;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.MoonshotChatLanguageModelTests;

/// <summary>Port of <c>moonshotai-chat-language-model.test.ts</c> &gt; <c>doGenerate &gt; prompt cache writes</c>.</summary>
public sealed class DoGeneratePromptCacheWritesTests
{
    private const string Prefix =
        "packages/moonshotai/src/moonshotai-chat-language-model.test.ts::doGenerate > prompt cache writes::";

    private const string CacheWriteUsage =
        """{"prompt_tokens":22459,"completion_tokens":44,"total_tokens":22503,"prompt_tokens_details":{"cached_tokens":1024,"cache_write_tokens":21248}}""";

    private static readonly Dictionary<string, string> CacheWriteHeaders = new()
    {
        ["msh-usage-cache-write-tokens-5m"] = "0",
        ["msh-usage-cache-write-tokens-1h"] = "21248",
    };

    [Fact]
    [UpstreamTest(
        Prefix + "should forward promptCacheOptions as prompt_cache_options",
        Coverage = UpstreamCoverage.Covered,
        Commit = "fce7a32cfdeed52b5ea0252cafd3d625b4e52746"
    )]
    public async Task Should_forward_promptCacheOptions_as_prompt_cache_options()
    {
        var capture = Capture();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag(
            "moonshotai",
            "{\"promptCacheOptions\":{\"ttl\":\"1h\"}}"
        );

        await Model(capture, "kimi-k3.1-pro").DoGenerateAsync(options, CancellationToken.None);

        var cache = Body(capture)["prompt_cache_options"]!.AsObject();
        Assert.Equal("implicit", cache["mode"]!.GetValue<string>());
        Assert.Equal("1h", cache["ttl"]!.GetValue<string>());
        Assert.Equal(2, cache.Count);
        Assert.False(Body(capture).ContainsKey("promptCacheOptions"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should default the mode when no ttl is set",
        Coverage = UpstreamCoverage.Covered,
        Commit = "fce7a32cfdeed52b5ea0252cafd3d625b4e52746"
    )]
    public async Task Should_default_the_mode_when_no_ttl_is_set()
    {
        var capture = Capture();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag("moonshotai", "{\"promptCacheOptions\":{}}");

        await Model(capture, "kimi-k3").DoGenerateAsync(options, CancellationToken.None);

        var cache = Body(capture)["prompt_cache_options"]!.AsObject();
        Assert.Equal("implicit", cache["mode"]!.GetValue<string>());
        Assert.Single(cache);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit prompt_cache_options when not set",
        Coverage = UpstreamCoverage.Covered,
        Commit = "fce7a32cfdeed52b5ea0252cafd3d625b4e52746"
    )]
    public async Task Should_omit_prompt_cache_options_when_not_set()
    {
        var capture = Capture();

        await Model(capture, "kimi-k3")
            .DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);

        Assert.False(Body(capture).ContainsKey("prompt_cache_options"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should report cache writes and the TTL split on generate",
        Coverage = UpstreamCoverage.Covered,
        Commit = "fce7a32cfdeed52b5ea0252cafd3d625b4e52746"
    )]
    public async Task Should_report_cache_writes_and_the_TTL_split_on_generate()
    {
        var capture = Capture();
        capture.ResponseBody = ResponseWithUsage();
        foreach (var pair in CacheWriteHeaders)
        {
            capture.ResponseHeaders[pair.Key] = pair.Value;
        }

        var result = await Model(capture, "kimi-k3.1-pro")
            .DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);

        Assert.Equal(22459, result.Usage.InputTokens);
        Assert.Equal(187, result.Usage.NoCacheInputTokens);
        Assert.Equal(1024, result.Usage.CacheReadTokens);
        Assert.Equal(21248, result.Usage.CacheWriteTokens);
        var split = result
            .ProviderMetadata!.Value.GetProperty("moonshotai")
            .GetProperty("cacheWriteTokens");
        Assert.Equal(0, split.GetProperty("ttl5m").GetInt32());
        Assert.Equal(21248, split.GetProperty("ttl1h").GetInt32());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should report cache writes and the TTL split on stream",
        Coverage = UpstreamCoverage.Covered,
        Commit = "fce7a32cfdeed52b5ea0252cafd3d625b4e52746"
    )]
    public async Task Should_report_cache_writes_and_the_TTL_split_on_stream()
    {
        var handler = new SseStreamHandler(
            new[]
            {
                "data: {\"id\":\"chatcmpl-cache\",\"created\":1785880003,\"model\":\"kimi-k3.1-pro\",\"choices\":[{\"index\":0,\"delta\":{\"content\":\"OK\"},\"finish_reason\":\"stop\"}],\"usage\":"
                    + CacheWriteUsage
                    + "}\n\n",
                "data: [DONE]\n\n",
            },
            CacheWriteHeaders
        );
        var model = (OpenAICompatibleLanguageModel)
            MoonshotProvider
                .Create(new OpenAICompatibleOptions { ApiKey = "secret" }, handler)
                .LanguageModel("kimi-k3.1-pro");

        var parts = await UpstreamChat.Read(
            await model.DoStreamAsync(UpstreamChat.Prompt(), CancellationToken.None)
        );

        var finish = Assert.Single(parts.OfType<FinishStreamPart>());
        Assert.Equal(21248, finish.Usage.CacheWriteTokens);
        Assert.Equal(187, finish.Usage.NoCacheInputTokens);
        var split = finish
            .ProviderMetadata!.Value.GetProperty("moonshotai")
            .GetProperty("cacheWriteTokens");
        Assert.Equal(0, split.GetProperty("ttl5m").GetInt32());
        Assert.Equal(21248, split.GetProperty("ttl1h").GetInt32());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit the TTL split without the usage headers",
        Coverage = UpstreamCoverage.Covered,
        Commit = "fce7a32cfdeed52b5ea0252cafd3d625b4e52746"
    )]
    public async Task Should_omit_the_TTL_split_without_the_usage_headers()
    {
        var capture = Capture();

        var result = await Model(capture, "kimi-k3")
            .DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);

        Assert.False(
            result
                .ProviderMetadata!.Value.GetProperty("moonshotai")
                .TryGetProperty("cacheWriteTokens", out _)
        );
    }

    private static string ResponseWithUsage() =>
        "{\"id\":\"chatcmpl-test\",\"object\":\"chat.completion\",\"created\":1785880000,\"model\":\"kimi-k3.1-pro\",\"choices\":[{\"index\":0,\"message\":{\"role\":\"assistant\",\"content\":\"OK\"},\"finish_reason\":\"stop\"}],\"usage\":"
        + CacheWriteUsage
        + "}";

    private static UpstreamCapture Capture() => new() { ResponseBody = ResponseWithUsage() };

    private static OpenAICompatibleLanguageModel Model(UpstreamCapture capture, string modelId) =>
        (OpenAICompatibleLanguageModel)
            MoonshotProvider
                .Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture)
                .LanguageModel(modelId);

    private static JsonObject Body(UpstreamCapture capture) =>
        (JsonObject)JsonNode.Parse(capture.Requests[0].Body)!;
}
