// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Moonshot;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.MoonshotChatLanguageModelTests;

/// <summary>Port of <c>moonshotai-chat-language-model.test.ts</c> &gt; <c>doGenerate &gt; text</c> </summary>
public sealed class DoGenerateTextTests
{
    private const string Prefix =
        "packages/moonshotai/src/moonshotai-chat-language-model.test.ts::doGenerate > text::";

    [Theory]
    [InlineData("kimi-k2.5")]
    [InlineData("kimi-k2.6")]
    [InlineData("kimi-k2.7-code")]
    [InlineData("kimi-k2.7-code-highspeed")]
    [InlineData("kimi-k3")]
    [InlineData("kimi-k3.1-pro")]
    [InlineData("kimi-k3.1-pro-highspeed")]
    [UpstreamTest(
        Prefix + "should omit fixed sampling options and warn for %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_fixed_sampling_options_and_warn_for_model(string modelId)
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Temperature = 0.2;
        options.TopP = 0.4;
        options.FrequencyPenalty = 0.5;
        options.PresencePenalty = 0.6;

        var result = await Model(capture, modelId).DoGenerateAsync(options, CancellationToken.None);

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    $"{{\"model\":\"{modelId}\",\"messages\":[{{\"role\":\"user\",\"content\":\"Hello\"}}]}}"
                ),
                UpstreamChat.Body(capture)
            )
        );
        Assert.Collection(
            result.Warnings,
            w => AssertFixed(w, "temperature", modelId),
            w => AssertFixed(w, "topP", modelId),
            w => AssertFixed(w, "frequencyPenalty", modelId),
            w => AssertFixed(w, "presencePenalty", modelId)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve sampling options for custom model IDs",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_sampling_options_for_custom_model_IDs()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Temperature = 0.2;
        options.TopP = 0.4;
        options.FrequencyPenalty = 0.5;
        options.PresencePenalty = 0.6;

        await Model(capture, "custom-model").DoGenerateAsync(options, CancellationToken.None);

        var body = UpstreamChat.Body(capture);
        Assert.Equal("custom-model", body["model"]!.GetValue<string>());
        Assert.Equal(0.2, body["temperature"]!.GetValue<double>());
        Assert.Equal(0.4, body["top_p"]!.GetValue<double>());
        Assert.Equal(0.5, body["frequency_penalty"]!.GetValue<double>());
        Assert.Equal(0.6, body["presence_penalty"]!.GetValue<double>());
    }

    private const string TextFixture = """
        {"id":"chatcmpl-test-text","object":"chat.completion","created":1785880000,"model":"kimi-k3","choices":[{"index":0,"message":{"role":"assistant","content":"Hello, world!"},"finish_reason":"stop"}],"usage":{"prompt_tokens":9,"completion_tokens":5,"total_tokens":14}}
        """;

    [Fact]
    [UpstreamTest(Prefix + "should send correct request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_correct_request_body()
    {
        var capture = new UpstreamCapture { ResponseBody = TextFixture };
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new SystemModelMessage("You are a helpful assistant."),
                new UserModelMessage("Hello"),
            },
            Temperature = 0.5,
            TopP = 0.3,
        };

        await Model(capture, "moonshot-v1-8k").DoGenerateAsync(options, CancellationToken.None);

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    """
                    {"messages":[{"content":"You are a helpful assistant.","role":"system"},{"content":"Hello","role":"user"}],"model":"moonshot-v1-8k","temperature":0.5,"top_p":0.3}
                    """
                ),
                UpstreamChat.Body(capture)
            )
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should send message names", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_message_names()
    {
        var capture = new UpstreamCapture { ResponseBody = TextFixture };
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new SystemModelMessage(
                    "You are a helpful assistant.",
                    UpstreamChat.Bag("moonshotai", "{\"name\":\"guide\"}")
                ),
                new UserModelMessage("Hello")
                {
                    ProviderOptions = UpstreamChat.Bag("moonshotai", "{\"name\":\"alice\"}"),
                },
                new AssistantModelMessage("Hello, Alice.", null, null)
                {
                    ProviderOptions = UpstreamChat.Bag("moonshotai", "{\"name\":\"assistant\"}"),
                },
            },
        };

        await Model(capture, "kimi-k3").DoGenerateAsync(options, CancellationToken.None);

        var messages = UpstreamChat.Body(capture)["messages"]!.AsArray();
        Assert.Equal(3, messages.Count);
        Assert.Equal("system", messages[0]!["role"]!.GetValue<string>());
        Assert.Equal("You are a helpful assistant.", messages[0]!["content"]!.GetValue<string>());
        Assert.Equal("guide", messages[0]!["name"]!.GetValue<string>());
        Assert.Equal("user", messages[1]!["role"]!.GetValue<string>());
        Assert.Equal("Hello", messages[1]!["content"]!.GetValue<string>());
        Assert.Equal("alice", messages[1]!["name"]!.GetValue<string>());
        Assert.Equal("assistant", messages[2]!["role"]!.GetValue<string>());
        Assert.Equal("Hello, Alice.", messages[2]!["content"]!.GetValue<string>());
        Assert.Equal("assistant", messages[2]!["name"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send maxOutputTokens as max_completion_tokens",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_maxOutputTokens_as_max_completion_tokens()
    {
        var capture = new UpstreamCapture { ResponseBody = TextFixture };
        var options = UpstreamChat.Prompt();
        options.MaxOutputTokens = 17;

        await Model(capture, "kimi-k3").DoGenerateAsync(options, CancellationToken.None);

        var body = UpstreamChat.Body(capture);
        Assert.Equal(17, body["max_completion_tokens"]!.GetValue<int>());
        Assert.False(body.ContainsKey("max_tokens"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit max_completion_tokens when maxOutputTokens is undefined",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_max_completion_tokens_when_maxOutputTokens_is_undefined()
    {
        var capture = new UpstreamCapture { ResponseBody = TextFixture };

        await Model(capture, "kimi-k3")
            .DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);

        var body = UpstreamChat.Body(capture);
        Assert.False(body.ContainsKey("max_completion_tokens"));
        Assert.False(body.ContainsKey("max_tokens"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should extract text content, finish reason, and usage",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_extract_text_content_finish_reason_and_usage()
    {
        var capture = new UpstreamCapture { ResponseBody = TextFixture };

        var result = await Model(capture, "kimi-k3")
            .DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);

        var content = Assert.Single(result.Content);
        Assert.Equal("Hello, world!", Assert.IsType<GeneratedText>(content).Text);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal("stop", result.RawFinishReason);
        Assert.Equal(9, result.Usage.InputTokens);
        Assert.Equal(9, result.Usage.NoCacheInputTokens);
        Assert.Equal(0, result.Usage.CacheReadTokens);
        Assert.Equal(5, result.Usage.OutputTokens);
        Assert.Equal(5, result.Usage.TextTokens);
        Assert.Equal(0, result.Usage.ReasoningTokens);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [UpstreamTest(
        Prefix + "should tolerate %s response metadata fields",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_tolerate_null_and_undefined_response_metadata_fields(bool isNull)
    {
        var body = JsonNode.Parse(TextFixture)!.AsObject();
        var choice = body["choices"]![0]!.AsObject();
        var message = choice["message"]!.AsObject();
        if (isNull)
        {
            body["object"] = null;
            choice["index"] = null;
            message["role"] = null;
        }
        else
        {
            body.Remove("object");
            choice.Remove("index");
            message.Remove("role");
        }

        var capture = new UpstreamCapture { ResponseBody = body.ToJsonString() };

        var result = await Model(capture, "kimi-k3")
            .DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);

        Assert.True(result.ProviderMetadata.HasValue);
        var metadata = result.ProviderMetadata!.Value;
        Assert.Equal(JsonValueKind.Object, metadata.GetProperty("moonshotai").ValueKind);
        Assert.Empty(metadata.GetProperty("moonshotai").EnumerateObject());
        Assert.Single(metadata.EnumerateObject());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve extra top-level and nested fields in raw usage",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_extra_top_level_and_nested_fields_in_raw_usage()
    {
        const string usage = """
            {"prompt_tokens":100,"completion_tokens":50,"total_tokens":150,"cached_tokens":20,"provider_usage_id":"usage-123","prompt_tokens_details":{"cached_tokens":20,"cache_type":"ephemeral"},"completion_tokens_details":{"reasoning_tokens":10,"billable_tokens":42}}
            """;
        var capture = new UpstreamCapture
        {
            ResponseBody =
                "{\"id\":\"chatcmpl-raw-usage\",\"object\":\"chat.completion\",\"created\":1785880000,\"model\":\"kimi-k3\",\"choices\":[{\"index\":0,\"message\":{\"role\":\"assistant\",\"content\":\"Hello\"},\"finish_reason\":\"stop\"}],\"usage\":"
                + usage
                + "}",
        };

        var result = await Model(capture, "kimi-k3")
            .DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);

        Assert.Equal(100, result.Usage.InputTokens);
        Assert.Equal(80, result.Usage.NoCacheInputTokens);
        Assert.Equal(20, result.Usage.CacheReadTokens);
        Assert.Equal(50, result.Usage.OutputTokens);
        Assert.Equal(40, result.Usage.TextTokens);
        Assert.Equal(10, result.Usage.ReasoningTokens);
        Assert.True(result.Usage.Raw.HasValue);
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(usage),
                JsonNode.Parse(result.Usage.Raw!.Value.GetRawText())
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should continue validating known usage field types",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_continue_validating_known_usage_field_types()
    {
        var capture = new UpstreamCapture
        {
            ResponseBody = """
                {"id":"chatcmpl-invalid-usage","created":1785880000,"model":"kimi-k3","choices":[{"message":{"role":"assistant","content":"Hello"},"finish_reason":"stop"}],"usage":{"prompt_tokens":"100","completion_tokens":50,"total_tokens":150}}
                """,
        };

        await Assert.ThrowsAnyAsync<Exception>(() =>
            Model(capture, "kimi-k3").DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None)
        );
    }

    private static void AssertFixed(
        Vercel.AI.Provider.CallWarning warning,
        string feature,
        string modelId
    )
    {
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal(feature, warning.Feature);
        Assert.Equal(
            $"{feature} is fixed by model \"{modelId}\" and has been omitted.",
            warning.Message
        );
    }

    private static OpenAICompatibleLanguageModel Model(UpstreamCapture capture, string modelId) =>
        (OpenAICompatibleLanguageModel)
            MoonshotProvider
                .Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture)
                .LanguageModel(modelId);
}
