// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.MoonshotAI.MoonshotAIChatLanguageModel.MoonshotLanguageModelSupportA;

namespace Vercel.AI.Tests.Upstream.MoonshotAI.MoonshotAIChatLanguageModel;

/// <summary>
/// Port of <c>moonshotai-chat-language-model.test.ts</c> &gt; <c>doGenerate</c> &gt; <c>text</c>.
/// </summary>
public sealed class DoGenerateTextTests
{
    private const string Prefix = MoonshotLanguageModelSupportA.Id + "text::";

    [Fact]
    [UpstreamTest(Prefix + "should send correct request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_correct_request_body()
    {
        var (model, handler) = Model("moonshot-v1-8k");

        await model.DoGenerateAsync(
            new LanguageModelCallOptions
            {
                Prompt =
                [
                    new SystemModelMessage("You are a helpful assistant."),
                    new UserModelMessage("Hello"),
                ],
                Temperature = 0.5,
                TopP = 0.3,
            },
            CancellationToken.None
        );

        Strict(
            Body(handler),
            "{\"messages\":[{\"content\":\"You are a helpful assistant.\",\"role\":\"system\"},{\"content\":\"Hello\",\"role\":\"user\"}],\"model\":\"moonshot-v1-8k\",\"temperature\":0.5,\"top_p\":0.3}"
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should send message names", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_message_names()
    {
        var (model, handler) = Model("kimi-k3");

        await model.DoGenerateAsync(
            Messages(
                new SystemModelMessage(
                    "You are a helpful assistant.",
                    Opts("{\"moonshotai\":{\"name\":\"guide\"}}")
                ),
                new UserModelMessage("Hello")
                {
                    ProviderOptions = Opts("{\"moonshotai\":{\"name\":\"alice\"}}"),
                },
                new AssistantModelMessage([new AssistantTextPart("Hello, Alice.")])
                {
                    ProviderOptions = Opts("{\"moonshotai\":{\"name\":\"assistant\"}}"),
                }
            ),
            CancellationToken.None
        );

        Match(
            Body(handler),
            "{\"messages\":[{\"role\":\"system\",\"content\":\"You are a helpful assistant.\",\"name\":\"guide\"},{\"role\":\"user\",\"content\":\"Hello\",\"name\":\"alice\"},{\"role\":\"assistant\",\"content\":\"Hello, Alice.\",\"name\":\"assistant\"}]}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send maxOutputTokens as max_completion_tokens",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_maxOutputTokens_as_max_completion_tokens()
    {
        var (model, handler) = Model("kimi-k3");
        var options = TestPrompt();
        options.MaxOutputTokens = 17;

        await model.DoGenerateAsync(options, CancellationToken.None);

        var body = Body(handler);
        Assert.Equal(17, (int)body["max_completion_tokens"]!);
        Assert.False(body.ContainsKey("max_tokens"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit max_completion_tokens when maxOutputTokens is undefined",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_max_completion_tokens_when_maxOutputTokens_is_undefined()
    {
        var (model, handler) = Model("kimi-k3");

        await model.DoGenerateAsync(TestPrompt(), CancellationToken.None);

        var body = Body(handler);
        Assert.False(body.ContainsKey("max_completion_tokens"));
        Assert.False(body.ContainsKey("max_tokens"));
    }

    [Theory]
    [InlineData("kimi-k2.5")]
    [InlineData("kimi-k2.6")]
    [InlineData("kimi-k2.7-code")]
    [InlineData("kimi-k2.7-code-highspeed")]
    [InlineData("kimi-k3")]
    [UpstreamTest(
        Prefix + "should omit fixed sampling options and warn for %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_fixed_sampling_options_and_warn_for(string modelId)
    {
        var (model, handler) = Model(modelId);
        var options = TestPrompt();
        options.Temperature = 0.2;
        options.TopP = 0.4;
        options.FrequencyPenalty = 0.5;
        options.PresencePenalty = 0.6;

        var result = await model.DoGenerateAsync(options, CancellationToken.None);

        Strict(
            Body(handler),
            "{\"model\":\""
                + modelId
                + "\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}]}"
        );
        AssertWarnings(
            result.Warnings,
            (
                "unsupported",
                "temperature",
                "temperature is fixed by model \"" + modelId + "\" and has been omitted."
            ),
            (
                "unsupported",
                "topP",
                "topP is fixed by model \"" + modelId + "\" and has been omitted."
            ),
            (
                "unsupported",
                "frequencyPenalty",
                "frequencyPenalty is fixed by model \"" + modelId + "\" and has been omitted."
            ),
            (
                "unsupported",
                "presencePenalty",
                "presencePenalty is fixed by model \"" + modelId + "\" and has been omitted."
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve sampling options for custom model IDs",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_sampling_options_for_custom_model_IDs()
    {
        var (model, handler) = Model("custom-model");
        var options = TestPrompt();
        options.Temperature = 0.2;
        options.TopP = 0.4;
        options.FrequencyPenalty = 0.5;
        options.PresencePenalty = 0.6;

        await model.DoGenerateAsync(options, CancellationToken.None);

        Match(
            Body(handler),
            "{\"model\":\"custom-model\",\"temperature\":0.2,\"top_p\":0.4,\"frequency_penalty\":0.5,\"presence_penalty\":0.6}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should extract text content, finish reason, and usage",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_extract_text_content_finish_reason_and_usage()
    {
        var (model, _) = Model("kimi-k3");

        var result = await model.DoGenerateAsync(TestPrompt(), CancellationToken.None);

        var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal("Hello, world!", text.Text);
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
    public async Task Should_tolerate_response_metadata_fields(bool isNull)
    {
        var body = JsonNode.Parse(Fixture("moonshotai-text"))!.AsObject();
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

        var (model, _) = Model("kimi-k3", new SequencedHandler().Json(body.ToJsonString()));

        var result = await model.DoGenerateAsync(TestPrompt(), CancellationToken.None);

        JsonAssert.Equal(result.ProviderMetadata!.Value, "{\"moonshotai\":{}}");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve extra top-level and nested fields in raw usage",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_extra_top_level_and_nested_fields_in_raw_usage()
    {
        var (model, _) = Model(
            "kimi-k3",
            new SequencedHandler().Json(
                "{\"id\":\"chatcmpl-raw-usage\",\"object\":\"chat.completion\",\"created\":1785880000,\"model\":\"kimi-k3\",\"choices\":[{\"index\":0,\"message\":{\"role\":\"assistant\",\"content\":\"Hello\"},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":100,\"completion_tokens\":50,\"total_tokens\":150,\"cached_tokens\":20,\"provider_usage_id\":\"usage-123\",\"prompt_tokens_details\":{\"cached_tokens\":20,\"cache_type\":\"ephemeral\"},\"completion_tokens_details\":{\"reasoning_tokens\":10,\"billable_tokens\":42}}}"
            )
        );

        var result = await model.DoGenerateAsync(TestPrompt(), CancellationToken.None);

        Assert.Equal(100, result.Usage.InputTokens);
        Assert.Equal(80, result.Usage.NoCacheInputTokens);
        Assert.Equal(20, result.Usage.CacheReadTokens);
        Assert.Equal(50, result.Usage.OutputTokens);
        Assert.Equal(40, result.Usage.TextTokens);
        Assert.Equal(10, result.Usage.ReasoningTokens);
        JsonAssert.Equal(
            result.Usage.Raw!.Value,
            "{\"prompt_tokens\":100,\"completion_tokens\":50,\"total_tokens\":150,\"cached_tokens\":20,\"provider_usage_id\":\"usage-123\",\"prompt_tokens_details\":{\"cached_tokens\":20,\"cache_type\":\"ephemeral\"},\"completion_tokens_details\":{\"reasoning_tokens\":10,\"billable_tokens\":42}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should continue validating known usage field types",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_continue_validating_known_usage_field_types()
    {
        var (model, _) = Model(
            "kimi-k3",
            new SequencedHandler().Json(
                "{\"id\":\"chatcmpl-invalid-usage\",\"created\":1785880000,\"model\":\"kimi-k3\",\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"Hello\"},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":\"100\",\"completion_tokens\":50,\"total_tokens\":150}}"
            )
        );

        await Assert.ThrowsAnyAsync<Exception>(() =>
            model.DoGenerateAsync(TestPrompt(), CancellationToken.None)
        );
    }
}
