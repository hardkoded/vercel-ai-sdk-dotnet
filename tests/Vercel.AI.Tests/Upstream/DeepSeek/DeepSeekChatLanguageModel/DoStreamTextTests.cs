// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.DeepSeek;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel.DeepSeekChatLanguageModelStreamSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel;

/// <summary>Port of <c>deepseek-chat-language-model.test.ts</c> &gt; <c>DeepSeekChatLanguageModel &gt; doStream &gt; text</c>.</summary>
public sealed class DoStreamTextTests
{
    private const string Prefix = TestFile + " > doStream > text::";

    [Fact]
    [UpstreamTest(
        Prefix + "should send model id, settings, and input",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_model_id_settings_and_input()
    {
        var handler = ChunksFixture("deepseek-text");

        await Chat(handler)
            .DoStreamAsync(
                new LanguageModelCallOptions
                {
                    Prompt = new ModelMessage[]
                    {
                        new SystemModelMessage("You are a helpful assistant."),
                        new UserModelMessage(
                            new UserContentPart[] { new TextContentPart("Hello") }
                        ),
                    },
                    Temperature = 0.5,
                    TopP = 0.3,
                },
                CancellationToken.None
            );

        AssertJson(
            RequestBody(handler),
            """
            {
              "messages": [
                { "content": "You are a helpful assistant.", "role": "system" },
                { "content": "Hello", "role": "user" }
              ],
              "model": "deepseek-chat",
              "stream": true,
              "stream_options": { "include_usage": true },
              "temperature": 0.5,
              "top_p": 0.3
            }
            """
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should omit deprecated and ineffective sampling options in default V4 thinking mode",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_deprecated_and_ineffective_sampling_options_in_default_V4_thinking_mode()
    {
        var handler = ChunksFixture("deepseek-text");
        var options = TestPrompt();
        options.Temperature = 0.2;
        options.TopP = 0.4;
        options.FrequencyPenalty = 0.5;
        options.PresencePenalty = 0.6;

        var parts = await Stream(Chat(handler, "deepseek-v4-flash"), options);

        AssertJson(
            RequestBody(handler),
            """
            {
              "model": "deepseek-v4-flash",
              "messages": [{ "role": "user", "content": "Hello" }],
              "stream": true,
              "stream_options": { "include_usage": true }
            }
            """
        );
        AssertJson(
            StreamStartJson(parts),
            """
            {
              "type": "stream-start",
              "warnings": [
                {
                  "type": "deprecated",
                  "setting": "frequencyPenalty",
                  "message": "frequencyPenalty is deprecated by DeepSeek and has been omitted. Remove frequencyPenalty from the request."
                },
                {
                  "type": "deprecated",
                  "setting": "presencePenalty",
                  "message": "presencePenalty is deprecated by DeepSeek and has been omitted. Remove presencePenalty from the request."
                },
                {
                  "type": "unsupported",
                  "feature": "temperature",
                  "details": "temperature has no effect when DeepSeek thinking is enabled. Set providerOptions.deepseek.thinking.type to 'disabled' to use temperature."
                },
                {
                  "type": "unsupported",
                  "feature": "topP",
                  "details": "topP has no effect when DeepSeek thinking is enabled. Set providerOptions.deepseek.thinking.type to 'disabled' to use topP."
                }
              ]
            }
            """
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve supported sampling options when V4 thinking is disabled",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_supported_sampling_options_when_V4_thinking_is_disabled()
    {
        var handler = ChunksFixture("deepseek-text");
        var options = TestPrompt();
        options.Temperature = 0.2;
        options.TopP = 0.4;
        options.FrequencyPenalty = 0.5;
        options.PresencePenalty = 0.6;
        options.ProviderOptions = new DeepSeekLanguageModelChatOptions
        {
            Thinking = new DeepSeekThinkingOptions { Type = "disabled" },
        }.ToProviderOptions();

        var parts = await Stream(Chat(handler, "deepseek-v4-flash"), options);

        AssertJson(
            RequestBody(handler),
            """
            {
              "model": "deepseek-v4-flash",
              "messages": [{ "role": "user", "content": "Hello" }],
              "temperature": 0.2,
              "top_p": 0.4,
              "thinking": { "type": "disabled" },
              "stream": true,
              "stream_options": { "include_usage": true }
            }
            """
        );
        AssertJson(
            StreamStartJson(parts),
            """
            {
              "type": "stream-start",
              "warnings": [
                {
                  "type": "deprecated",
                  "setting": "frequencyPenalty",
                  "message": "frequencyPenalty is deprecated by DeepSeek and has been omitted. Remove frequencyPenalty from the request."
                },
                {
                  "type": "deprecated",
                  "setting": "presencePenalty",
                  "message": "presencePenalty is deprecated by DeepSeek and has been omitted. Remove presencePenalty from the request."
                }
              ]
            }
            """
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should send message names", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_message_names()
    {
        var handler = ChunksFixture("deepseek-text");

        await Chat(handler)
            .DoStreamAsync(
                new LanguageModelCallOptions
                {
                    Prompt = new ModelMessage[]
                    {
                        new SystemModelMessage(
                            "You are a helpful assistant.",
                            new DeepSeekMessageProviderOptions
                            {
                                Name = "guide",
                            }.ToProviderOptions()
                        ),
                        new UserModelMessage(new UserContentPart[] { new TextContentPart("Hello") })
                        {
                            ProviderOptions = new DeepSeekMessageProviderOptions
                            {
                                Name = "alice",
                            }.ToProviderOptions(),
                        },
                        new AssistantModelMessage(
                            new AssistantContentPart[] { new AssistantTextPart("Hello, Alice.") }
                        )
                        {
                            ProviderOptions = new DeepSeekMessageProviderOptions
                            {
                                Name = "assistant",
                            }.ToProviderOptions(),
                        },
                    },
                },
                CancellationToken.None
            );

        AssertJson(
            RequestBody(handler),
            """
            {
              "messages": [
                { "content": "You are a helpful assistant.", "name": "guide", "role": "system" },
                { "content": "Hello", "name": "alice", "role": "user" },
                { "content": "Hello, Alice.", "name": "assistant", "role": "assistant" }
              ],
              "model": "deepseek-chat",
              "stream": true,
              "stream_options": { "include_usage": true }
            }
            """
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass providerOptions userId as user_id",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_providerOptions_userId_as_user_id()
    {
        var handler = ChunksFixture("deepseek-text");
        var options = TestPrompt();
        options.ProviderOptions = new DeepSeekLanguageModelChatOptions
        {
            UserId = "tenant_123-user",
        }.ToProviderOptions();

        await Chat(handler).DoStreamAsync(options, CancellationToken.None);

        var body = RequestBody(handler);
        Assert.True((bool)body["stream"]!);
        Assert.Equal("tenant_123-user", (string)body["user_id"]!);
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream text", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_text()
    {
        var handler = ChunksFixture("deepseek-text");

        var parts = await Stream(Chat(handler), TestPrompt());

        AssertStreamSnapshot(parts, "deepseek-chat-text.stream.snap.json");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include the repeated system fingerprint in provider metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_the_repeated_system_fingerprint_in_provider_metadata()
    {
        var handler = ChunksFixture("deepseek-text");

        var parts = await Stream(Chat(handler), TestPrompt());

        var metadata = Finish(parts).ProviderMetadata!.Value;
        Assert.Single(metadata.EnumerateObject());
        Assert.Equal(
            "fp_eaab8d114b_prod0820_fp8_kvcache",
            metadata.GetProperty("deepseek").GetProperty("systemFingerprint").GetString()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should keep the latest non-null system fingerprint",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_keep_the_latest_non_null_system_fingerprint()
    {
        var handler = Chunks(
            "data: {\"system_fingerprint\":\"fp_initial\",\"choices\":[{\"delta\":{\"content\":\"OK\"},\"finish_reason\":null}],\"usage\":null}\n\n",
            "data: {\"system_fingerprint\":null,\"choices\":[{\"delta\":{},\"finish_reason\":null}],\"usage\":null}\n\n",
            "data: {\"system_fingerprint\":\"fp_latest\",\"choices\":[{\"delta\":{},\"finish_reason\":\"stop\"}],\"usage\":null}\n\n",
            "data: [DONE]\n\n"
        );

        var parts = await Stream(Chat(handler), TestPrompt());

        var metadata = Finish(parts).ProviderMetadata!.Value;
        Assert.Single(metadata.EnumerateObject());
        Assert.Equal(
            "fp_latest",
            metadata.GetProperty("deepseek").GetProperty("systemFingerprint").GetString()
        );
    }

    [Theory]
    [InlineData("null")]
    [InlineData("undefined")]
    [UpstreamTest(
        Prefix + "should tolerate a %s system fingerprint",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_tolerate_a_system_fingerprint(string systemFingerprint)
    {
        var chunk =
            systemFingerprint == "undefined"
                ? "{\"choices\":[{\"delta\":{\"content\":\"OK\"},\"finish_reason\":\"stop\"}],\"usage\":null}"
                : "{\"system_fingerprint\":null,\"choices\":[{\"delta\":{\"content\":\"OK\"},\"finish_reason\":\"stop\"}],\"usage\":null}";
        var handler = Chunks("data: " + chunk + "\n\n", "data: [DONE]\n\n");

        var parts = await Stream(Chat(handler), TestPrompt());

        var finishPart = Finish(parts);
        var deepseek = finishPart.ProviderMetadata!.Value.GetProperty("deepseek");
        Assert.False(deepseek.TryGetProperty("systemFingerprint", out _));
    }
}
