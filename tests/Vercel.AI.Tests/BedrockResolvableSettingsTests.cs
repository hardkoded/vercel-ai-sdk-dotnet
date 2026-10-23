// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.AmazonBedrock;

namespace Vercel.AI.Tests;

[Collection("BedrockEndpointEnvironment")]
public sealed class BedrockResolvableSettingsTests : BedrockEndpointEnvironment
{
    private const string AnthropicBody = "{\"content\":[],\"stop_reason\":\"end_turn\",\"usage\":{\"input_tokens\":1,\"output_tokens\":1}}";

    [Fact]
    public void Region_falls_back_to_the_environment_then_the_default()
    {
        Assert.Equal("us-east-1", new AmazonBedrockOptions().Region);
        Environment.SetEnvironmentVariable("AWS_REGION", "eu-west-1");
        Assert.Equal("eu-west-1", new AmazonBedrockOptions().Region);
        Assert.Equal("eu-west-1", new AmazonBedrockAnthropicOptions().Region);
        Assert.Equal("eu-west-1", new BedrockMantleOptions().Region);
        Assert.Equal("ap-south-1", new AmazonBedrockOptions { Region = "ap-south-1" }.Region);
    }

    [Fact]
    public async Task Converse_provider_uses_the_environment_region_in_the_endpoint()
    {
        Environment.SetEnvironmentVariable("AWS_REGION", "eu-west-1");
        var handler = new UpstreamRecordingHandler("{\"embedding\":[0.1],\"inputTextTokenCount\":1}");
        var provider = AmazonBedrockProvider.Create(new AmazonBedrockOptions { AccessKeyId = "k", SecretAccessKey = "s" }, handler);

        await provider.EmbeddingModel("amazon.titan-embed-text-v2:0").DoEmbedAsync(new[] { "a" }, null, CancellationToken.None).ConfigureAwait(false);

        Assert.StartsWith("https://bedrock-runtime.eu-west-1.amazonaws.com/", handler.Uri, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Converse_headers_provider_runs_on_every_call_for_chat_and_embedding()
    {
        var calls = 0;
        var chat = new UpstreamRecordingHandler("{\"output\":{\"message\":{\"role\":\"assistant\",\"content\":[{\"text\":\"hi\"}]}},\"stopReason\":\"end_turn\",\"usage\":{\"inputTokens\":1,\"outputTokens\":1,\"totalTokens\":2}}");
        var options = new AmazonBedrockOptions { AccessKeyId = "k", SecretAccessKey = "s" };
        options.Headers["Static"] = "static";
        options.Headers["Custom"] = "static";
        options.Headers["Dropped"] = "static";
        options.HeadersProvider = _ => Task.FromResult<IReadOnlyDictionary<string, string?>>(new Dictionary<string, string?> { ["Custom"] = "call-" + ++calls, ["Dropped"] = null });
        var model = AmazonBedrockProvider.Create(options, chat).LanguageModel("anthropic.claude-v2");
        var call = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
            Headers = new Dictionary<string, string?> { ["Layer"] = "call" },
        };

        await model.DoGenerateAsync(call, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(1, calls);
        Assert.Equal("call-1", chat.Headers["Custom"]);
        Assert.Equal("static", chat.Headers["Static"]);
        Assert.Equal("call", chat.Headers["Layer"]);
        Assert.False(chat.Headers.ContainsKey("Dropped"));
        Assert.Contains("ai-sdk-amazon-bedrock", chat.Headers["user-agent"], StringComparison.Ordinal);

        await model.DoGenerateAsync(call, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("call-2", chat.Headers["Custom"]);

        var embed = new UpstreamRecordingHandler("{\"embedding\":[0.1],\"inputTextTokenCount\":1}");
        await AmazonBedrockProvider.Create(options, embed).EmbeddingModel("amazon.titan-embed-text-v2:0").DoEmbedAsync(new[] { "a" }, null, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("call-3", embed.Headers["Custom"]);

        var rerank = new UpstreamRecordingHandler("{\"results\":[{\"index\":0,\"relevanceScore\":0.5}]}");
        await AmazonBedrockProvider.Create(options, rerank).RerankingModel("amazon.rerank-v1:0").DoRerankAsync("q", new[] { "a" }, 1, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("call-4", rerank.Headers["Custom"]);
        Assert.Equal(4, calls);
    }

    [Fact]
    public async Task Anthropic_headers_provider_runs_on_every_call_and_wins_over_static_headers()
    {
        var calls = 0;
        var handler = new UpstreamRecordingHandler(AnthropicBody);
        var options = new AmazonBedrockAnthropicOptions { AccessKeyId = "k", SecretAccessKey = "s" };
        options.Headers["Static"] = "static";
        options.Headers["Custom"] = "static";
        options.HeadersProvider = _ => Task.FromResult<IReadOnlyDictionary<string, string?>>(new Dictionary<string, string?> { ["Custom"] = "call-" + ++calls });
        var model = AmazonBedrockAnthropicProvider.Create(options, handler).LanguageModel("test-model-id");
        var call = new LanguageModelCallOptions { Prompt = new ModelMessage[] { new UserModelMessage("Hello") } };

        await model.DoGenerateAsync(call, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("call-1", handler.Headers["Custom"]);
        Assert.Equal("static", handler.Headers["Static"]);

        await model.DoGenerateAsync(call, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("call-2", handler.Headers["Custom"]);
    }

    [Fact]
    public async Task Mantle_headers_provider_runs_on_every_call_with_the_user_agent_suffix()
    {
        var calls = 0;
        var handler = new UpstreamRecordingHandler("{\"id\":\"x\",\"choices\":[{\"index\":0,\"message\":{\"role\":\"assistant\",\"content\":\"hi\"},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":1,\"completion_tokens\":1}}");
        var options = new BedrockMantleOptions { AccessKeyId = "k", SecretAccessKey = "s" };
        options.Headers["Static"] = "static";
        options.Headers["Dropped"] = "static";
        options.HeadersProvider = _ => Task.FromResult<IReadOnlyDictionary<string, string?>>(new Dictionary<string, string?> { ["Custom"] = "call-" + ++calls, ["Dropped"] = null });
        var model = BedrockMantleProvider.Create(options, handler).LanguageModel("test-model-id");
        var call = new LanguageModelCallOptions { Prompt = new ModelMessage[] { new UserModelMessage("Hello") } };

        await model.DoGenerateAsync(call, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("call-1", handler.Headers["Custom"]);
        Assert.Equal("static", handler.Headers["Static"]);
        Assert.False(handler.Headers.ContainsKey("Dropped"));
        Assert.Contains("amazon-bedrock", handler.Headers["User-Agent"], StringComparison.Ordinal);

        await model.DoGenerateAsync(call, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("call-2", handler.Headers["Custom"]);
    }

    [Fact]
    public async Task Mantle_call_headers_win_over_the_headers_provider_and_static_headers()
    {
        var handler = new UpstreamRecordingHandler("{\"id\":\"x\",\"choices\":[{\"index\":0,\"message\":{\"role\":\"assistant\",\"content\":\"hi\"},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":1,\"completion_tokens\":1}}");
        var options = new BedrockMantleOptions { AccessKeyId = "k", SecretAccessKey = "s" };
        options.Headers["Layer"] = "static";
        options.Headers["Other"] = "static";
        options.HeadersProvider = _ => Task.FromResult<IReadOnlyDictionary<string, string?>>(new Dictionary<string, string?> { ["Layer"] = "provider", ["Other"] = "provider" });
        var model = BedrockMantleProvider.Create(options, handler).LanguageModel("test-model-id");

        await model.DoGenerateAsync(
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
                Headers = new Dictionary<string, string?> { ["Layer"] = "call", ["User-Agent"] = "per-call-agent" },
            },
            CancellationToken.None).ConfigureAwait(false);

        Assert.Equal("call", handler.Headers["Layer"]);
        Assert.Equal("provider", handler.Headers["Other"]);
        Assert.Equal("per-call-agent", handler.Headers["User-Agent"]);
    }
}
