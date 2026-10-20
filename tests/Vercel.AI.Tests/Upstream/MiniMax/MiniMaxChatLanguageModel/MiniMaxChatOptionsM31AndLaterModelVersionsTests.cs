// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.MiniMaxChatLanguageModel;

/// <summary>Port of <c>minimax-chat-language-model.test.ts</c> &gt; <c>MiniMax chat options (%s)</c> &gt; <c>M3.1 and later model versions</c>.</summary>
public sealed class MiniMaxChatOptionsM31AndLaterModelVersionsTests
{
    public static IEnumerable<object?[]> CapabilityModels()
    {
        return MiniMaxChatOptionsCall.Modes(
            new object?[] { "minimax-m3.1" },
            new object?[] { "MiniMax-M3.1-Flash-Preview" },
            new object?[] { "minimax-m3.1.1" },
            new object?[] { "minimax-m3.2" },
            new object?[] { "minimax-m3.10-flash" },
            new object?[] { "minimax-m3.123-flash" },
            new object?[] { "minimax-m4" },
            new object?[] { "MiniMax-M4.0-Highspeed" },
            new object?[] { "minimax-m10" });
    }

    public static IEnumerable<object?[]> NoneModels()
    {
        return MiniMaxChatOptionsCall.Modes(new object?[] { "minimax-m3.2" }, new object?[] { "minimax-m4" });
    }

    public static IEnumerable<object?[]> OlderModels()
    {
        return MiniMaxChatOptionsCall.Modes(
            new object?[] { "minimax-m3" },
            new object?[] { "minimax-m3.0-flash" },
            new object?[] { "minimax-m2.99" },
            new object?[] { "custom-model" },
            new object?[] { "other-minimax-m4" },
            new object?[] { "minimax-m3.1oops" },
            new object?[] { "minimax-m4oops" });
    }

    [Theory]
    [MemberData(nameof(CapabilityModels))]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax chat options (%s) > M3.1 and later model versions::applies reasoning, sampling, media, and caching capabilities to %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Applies_reasoning_sampling_media_and_caching_capabilities(string mode, string modelId)
    {
        var options = new LanguageModelCallOptions
        {
            Reasoning = "high",
            Temperature = 2,
            TopP = 0.9,
            ProviderOptions = MiniMaxParity.Options("minimax", "{\"thinking\":{\"type\":\"adaptive\"}}"),
            Prompt = new ModelMessage[]
            {
                new UserModelMessage(new UserContentPart[]
                {
                    MiniMaxChatOptionsCall.Text("Reference.", "{\"minimax\":{\"cacheControl\":{\"type\":\"ephemeral\"}}}"),
                    MiniMaxParity.FileUrl("image/png", "https://example.com/image.png", "{\"detail\":\"high\"}"),
                    MiniMaxParity.FileReference("video/mp4", "minimax", "123", "{\"fps\":1}"),
                }),
            },
        };

        var (body, warnings, _) = await MiniMaxChatOptionsCall.Call(mode, options, modelId);

        MiniMaxParity.AssertMatch(body, "{\"model\":\"" + modelId + "\",\"temperature\":2,\"top_p\":0.9,\"thinking\":{\"type\":\"adaptive\"},\"output_config\":{\"effort\":\"high\"}}");
        JsonAssert.Equal(
            body["messages"]![0]!["content"],
            "[{\"type\":\"text\",\"text\":\"Reference.\",\"cache_control\":{\"type\":\"ephemeral\"}},{\"type\":\"image\",\"source\":{\"type\":\"url\",\"url\":\"https://example.com/image.png\",\"detail\":\"high\"}},{\"type\":\"video\",\"source\":{\"type\":\"url\",\"url\":\"mm_file://123\",\"fps\":1}}]");
        var expectedUrls = new Dictionary<string, string> { ["image/*"] = "^https?://", ["video/*"] = "^https?://" };
        Assert.Equal(expectedUrls, MiniMaxParity.Provider(new MiniMaxServer()).Chat(modelId).SupportedUrls);
        Assert.Empty(warnings);
    }

    [Theory]
    [MemberData(nameof(NoneModels))]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax chat options (%s) > M3.1 and later model versions::maps reasoning none to low effort for %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_reasoning_none_to_low_effort(string mode, string modelId)
    {
        var (body, warnings, _) = await MiniMaxChatOptionsCall.Call(mode, new LanguageModelCallOptions { Reasoning = "none" }, modelId);

        JsonAssert.Equal(body["output_config"], "{\"effort\":\"low\"}");
        Assert.Null(body["thinking"]);
        var warning = Assert.Single(warnings);
        Assert.Equal("compatibility", warning.Type);
        Assert.Equal("reasoning", warning.Feature);
    }

    [Theory]
    [MemberData(nameof(OlderModels))]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax chat options (%s) > M3.1 and later model versions::does not opt older or unrecognized model IDs into the new capabilities: %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_opt_older_or_unrecognized_model_IDs_into_the_new_capabilities(string mode, string modelId)
    {
        var options = MiniMaxChatOptionsCall.WithText("Reference.", "{\"minimax\":{\"cacheControl\":{\"type\":\"ephemeral\"}}}");
        options.Temperature = 1.5;

        var (body, warnings, _) = await MiniMaxChatOptionsCall.Call(mode, options, modelId);

        Assert.Equal(1, body["temperature"]!.GetValue<double>());
        Assert.False(body["messages"]![0]!["content"]![0]!.AsObject().ContainsKey("cache_control"));
        Assert.Empty(MiniMaxParity.Provider(new MiniMaxServer()).Chat(modelId).SupportedUrls);
        var warning = Assert.Single(warnings);
        Assert.Equal("temperature", warning.Feature);
    }
}
