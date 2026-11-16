// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Fireworks;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.FireworksChatLanguageModelTests;

/// <summary>Port of <c>fireworks-chat-language-model.test.ts</c> &gt; <c>Fireworks chat reasoning</c>.</summary>
public sealed class FireworksChatReasoningTests
{
    private const string GptOss = "accounts/fireworks/models/gpt-oss-120b";
    private const string Prefix =
        "packages/fireworks/src/fireworks-chat-language-model.test.ts::Fireworks chat reasoning::";

    [Theory]
    [InlineData("accounts/fireworks/models/gpt-oss-120b", "none", "minimal")]
    [InlineData("accounts/fireworks/models/glm-5p3", "none", "minimal")]
    [InlineData("accounts/fireworks/models/nemotron-3-ultra-nvfp4", "minimal", "low")]
    [UpstreamTest(
        Prefix
            + "should map unsupported reasoning for %s from \"%s\" to \"%s\" with a warning in doGenerate",
        Coverage = UpstreamCoverage.Covered,
        Commit = "c9c6402cbdfce3f82b36b4073236043b1946dd06"
    )]
    public async Task Should_map_unsupported_reasoning_with_a_warning_in_doGenerate(
        string modelId,
        string reasoning,
        string effort
    )
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Reasoning = reasoning;

        var result = await Model(capture, modelId).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal(effort, UpstreamChat.Body(capture)["reasoning_effort"]!.GetValue<string>());
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("compatibility", warning.Type);
        Assert.Equal("reasoning", warning.Feature);
        Assert.Equal(
            "reasoning \""
                + reasoning
                + "\" is not directly supported by this model. mapped to effort \""
                + effort
                + "\".",
            warning.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return the compatibility warning in stream-start",
        Coverage = UpstreamCoverage.Covered,
        Commit = "c9c6402cbdfce3f82b36b4073236043b1946dd06"
    )]
    public async Task Should_return_the_compatibility_warning_in_stream_start()
    {
        var capture = new UpstreamCapture
        {
            ResponseBody = UpstreamChat.Sse(
                "{\"choices\":[{\"index\":0,\"delta\":{\"content\":\"Hello\"},\"finish_reason\":\"stop\"}]}"
            ),
            MediaType = "text/event-stream",
        };
        var options = UpstreamChat.Prompt();
        options.Reasoning = "none";
        options.TopK = 1;

        var parts = await UpstreamChat.Read(
            (await Model(capture, GptOss).DoStreamAsync(options, CancellationToken.None))
        );

        var body = UpstreamChat.Body(capture);
        Assert.Equal("minimal", body["reasoning_effort"]!.GetValue<string>());
        Assert.True(body["stream"]!.GetValue<bool>());
        var start = Assert.IsType<StreamStartStreamPart>(parts[0]);
        Assert.Collection(
            start.Warnings,
            warning =>
            {
                Assert.Equal("unsupported", warning.Type);
                Assert.Equal("topK", warning.Feature);
                Assert.Equal("topK", warning.Message);
            },
            warning =>
            {
                Assert.Equal("compatibility", warning.Type);
                Assert.Equal("reasoning", warning.Feature);
                Assert.Equal(
                    "reasoning \"none\" is not directly supported by this model. mapped to effort \"minimal\".",
                    warning.Message
                );
            }
        );
    }

    [Theory]
    [InlineData("none")]
    [InlineData("minimal")]
    [InlineData("low")]
    [InlineData("medium")]
    [InlineData("high")]
    [InlineData("xhigh")]
    [InlineData("max")]
    [UpstreamTest(
        Prefix + "should send \"%s\" directly without warnings",
        Coverage = UpstreamCoverage.Covered,
        Commit = "c9c6402cbdfce3f82b36b4073236043b1946dd06"
    )]
    public async Task Should_send_reasoning_directly_without_warnings(string reasoning)
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Reasoning = reasoning;

        var result = await Model(capture, "test-model")
            .DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal(reasoning, UpstreamChat.Body(capture)["reasoning_effort"]!.GetValue<string>());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should honor provider reasoning effort without warning about ignored portable max",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_honor_provider_reasoning_effort_without_warning_about_ignored_portable_max()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Reasoning = "max";
        options.ProviderOptions = UpstreamChat.Bag("fireworks", "{\"reasoningEffort\":\"medium\"}");

        var result = await Model(capture, "test-model")
            .DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("medium", UpstreamChat.Body(capture)["reasoning_effort"]!.GetValue<string>());
        Assert.Empty(result.Warnings);
    }

    [Theory]
    [InlineData("accounts/fireworks/models/kimi-k3", "max")]
    [InlineData("accounts/fireworks/models/nemotron-3-ultra-nvfp4", "minimal")]
    [UpstreamTest(
        Prefix + "should send provider reasoning effort for %s unchanged as \"%s\"",
        Coverage = UpstreamCoverage.Covered,
        Commit = "c9c6402cbdfce3f82b36b4073236043b1946dd06"
    )]
    public async Task Should_send_provider_reasoning_effort_unchanged(
        string modelId,
        string reasoningEffort
    )
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag(
            "fireworks",
            "{\"reasoningEffort\":\"" + reasoningEffort + "\"}"
        );

        var result = await Model(capture, modelId).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal(
            reasoningEffort,
            UpstreamChat.Body(capture)["reasoning_effort"]!.GetValue<string>()
        );
        Assert.Empty(result.Warnings);
    }

    private static OpenAICompatibleLanguageModel Model(UpstreamCapture capture, string modelId)
    {
        return (OpenAICompatibleLanguageModel)
            FireworksProvider
                .Create(new OpenAICompatibleOptions { ApiKey = "test-key" }, capture)
                .LanguageModel(modelId);
    }
}
