// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Mistral;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Mistral.MistralRealtimeModel.MistralRealtimeModelTestSupport;

namespace Vercel.AI.Tests.Upstream.Mistral.MistralRealtimeModel;

/// <summary>Port of <c>mistral-realtime-model.test.ts</c> &gt; <c>Mistral realtime configuration</c>.</summary>
public sealed class MistralRealtimeConfigurationTests
{
    private const string Id =
        "packages/mistral/src/realtime/mistral-realtime-model.test.ts::Mistral realtime configuration::";

    private static RealtimeModelV4SessionConfig Config(string providerOptionsJson) =>
        new() { ProviderOptions = (JsonObject)JsonNode.Parse(providerOptionsJson)! };

    public static TheoryData<string, RealtimeModelV4SessionConfig> UnsupportedNormalizedSettings =>
        new()
        {
            {
                "inputAudioFormat",
                new RealtimeModelV4SessionConfig
                {
                    InputAudioFormatType = "audio/pcm",
                    InputAudioFormatRate = 16000,
                }
            },
            {
                "outputAudioFormat",
                new RealtimeModelV4SessionConfig
                {
                    OutputAudioFormatType = "audio/pcm",
                    OutputAudioFormatRate = 24000,
                }
            },
            {
                "inputAudioTranscription.prompt",
                new RealtimeModelV4SessionConfig
                {
                    InputAudioTranscription = new RealtimeModelV4AudioTranscription
                    {
                        Prompt = "Vocabulary",
                    },
                }
            },
            {
                "outputAudioTranscription.model",
                new RealtimeModelV4SessionConfig
                {
                    OutputAudioTranscription = new RealtimeModelV4AudioTranscription
                    {
                        Model = "other",
                    },
                }
            },
            {
                "outputModalities text",
                new RealtimeModelV4SessionConfig { OutputModalities = ["text"] }
            },
            {
                "turnDetection null",
                new RealtimeModelV4SessionConfig { TurnDetectionDisabled = true }
            },
            {
                "turnDetection server-vad",
                new RealtimeModelV4SessionConfig
                {
                    TurnDetection = new RealtimeModelV4TurnDetection { Type = "server-vad" },
                }
            },
        };

    [Theory]
    [MemberData(nameof(UnsupportedNormalizedSettings))]
    [UpstreamTest(
        Id + "rejects unsupported normalized settings: %j",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Rejects_unsupported_normalized_settings(
        string name,
        RealtimeModelV4SessionConfig config
    )
    {
        Assert.NotNull(name);
        Assert.Throws<UnsupportedFunctionalityException>(() =>
            MistralRealtimeModelOptions.BuildMistralRealtimeSessionConfig(config)
        );
    }

    [Theory]
    [InlineData("{\"mistral\":{\"stt\":{\"targetStreamingDelayMs\":79}}}")]
    [InlineData("{\"mistral\":{\"stt\":{\"turnDetection\":{\"endThreshold\":2}}}}")]
    [InlineData("{\"mistral\":{\"tts\":{\"expressiveness\":2}}}")]
    [InlineData("{\"mistral\":{\"agentVersion\":2}}")]
    [UpstreamTest(
        Id + "rejects invalid or unavailable provider settings: %j",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Rejects_invalid_or_unavailable_provider_settings(string providerOptions)
    {
        Assert.ThrowsAny<AiSdkException>(() =>
            MistralRealtimeModelOptions.BuildMistralRealtimeSessionConfig(Config(providerOptions))
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "supports instructions, tools and turn settings in live updates",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Supports_instructions_tools_and_turn_settings_in_live_updates()
    {
        var config = Config("{\"mistral\":{\"turn\":{\"allowInterruptions\":true}}}");
        config.Instructions = "Updated instructions";
        config.Tools = [];

        var result = MistralRealtimeModelOptions.BuildMistralRealtimeSessionConfig(
            config,
            update: true
        );

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    "{\"instructions\":\"Updated instructions\",\"tools\":[],\"turn\":{\"allow_interruptions\":true}}"
                ),
                result
            )
        );
    }

    public static TheoryData<string, RealtimeModelV4SessionConfig> NotUpdatableLive =>
        new()
        {
            {
                "voice",
                new RealtimeModelV4SessionConfig { Voice = "voice_447" }
            },
            { "stt", Config("{\"mistral\":{\"stt\":{\"language\":\"en\"}}}") },
            { "agentId", Config("{\"mistral\":{\"agentId\":\"saved-agent\"}}") },
        };

    [Theory]
    [MemberData(nameof(NotUpdatableLive))]
    [UpstreamTest(
        Id + "rejects settings that cannot be updated live: %j",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Rejects_settings_that_cannot_be_updated_live(
        string name,
        RealtimeModelV4SessionConfig config
    )
    {
        Assert.NotNull(name);
        Assert.Throws<UnsupportedFunctionalityException>(() =>
            MistralRealtimeModelOptions.BuildMistralRealtimeSessionConfig(config, update: true)
        );
    }
}
