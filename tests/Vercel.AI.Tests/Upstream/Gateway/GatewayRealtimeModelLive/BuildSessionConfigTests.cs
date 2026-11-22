// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayRealtimeModelLive.GatewayRealtimeModelLiveSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayRealtimeModelLive;

/// <summary>Port of <c>gateway-realtime-model-live.test.ts</c> &gt; <c>GatewayRealtimeModelLive</c> &gt; <c>buildSessionConfig</c>.</summary>
public sealed class BuildSessionConfigTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-realtime-model-live.test.ts::GatewayRealtimeModelLive > buildSessionConfig::";

    [Theory]
    [InlineData("tools")]
    [InlineData("turnDetection")]
    [InlineData("outputModalities")]
    [UpstreamTest(
        Prefix + "rejects the unsupported %s setting",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public void Rejects_the_unsupported_setting(string key)
    {
        var config = key switch
        {
            "tools" => new RealtimeModelV4SessionConfig { Tools = [] },
            "turnDetection" => new RealtimeModelV4SessionConfig
            {
                TurnDetection = new RealtimeModelV4TurnDetection { Type = "server-vad" },
            },
            _ => new RealtimeModelV4SessionConfig { OutputModalities = [] },
        };

        var thrown = Assert.Throws<UnsupportedFunctionalityException>(() =>
            CreateTestModel().BuildSessionConfig(config)
        );
        Assert.Contains("AI Gateway Live session setting: " + key, thrown.Message);
    }

    [Theory]
    [InlineData("{\"openai\":{\"client\":{\"dataChannel\":{}}}}", false)]
    [InlineData("{\"openai\":{\"delegation\":{\"type\":\"responses\"}}}", false)]
    [InlineData("{\"openai\":{\"delegation\":\"client\"}}", false)]
    [InlineData("{\"openai\":{\"voice\":null}}", true)]
    [InlineData("{\"openai\":{\"store\":null}}", true)]
    [InlineData("{\"openai\":null}", true)]
    [InlineData("{\"gateway\":\"tags\"}", true)]
    [InlineData("{\"google\":{}}", false)]
    [UpstreamTest(
        Prefix + "rejects the provider options %o",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public void Rejects_the_provider_options(string providerOptions, bool invalidArgument)
    {
        var config = WithProviderOptions(providerOptions);

        if (invalidArgument)
        {
            Assert.Throws<InvalidArgumentException>(() =>
                CreateTestModel().BuildSessionConfig(config)
            );
        }
        else
        {
            Assert.Throws<UnsupportedFunctionalityException>(() =>
                CreateTestModel().BuildSessionConfig(config)
            );
        }
    }

    public static TheoryData<string, string> MalformedOpenAIValues =>
        new()
        {
            { "voice", "\"marin\"" },
            { "voice", "{\"id\":\"\"}" },
            { "voice", "{\"id\":\"v\",\"name\":\"extra\"}" },
            { "store", "\"false\"" },
            { "input", "{}" },
            { "input", "[" + Message("user", "text") + "]" },
            { "input", "[" + Message("assistant", "input_text") + "]" },
            { "input", "[" + Message("system", "input_text") + "]" },
            { "input", "[" + Message("user", "input_text").TrimEnd('}') + ",\"extra\":true}]" },
            { "input", "[{\"type\":\"message\",\"role\":\"user\",\"content\":[]}]" },
            {
                "input",
                "[" + string.Join(",", Enumerable.Repeat(Message("user", "input_text"), 129)) + "]"
            },
        };

    [Theory]
    [MemberData(nameof(MalformedOpenAIValues))]
    [UpstreamTest(
        Prefix + "rejects the malformed openai.%s value %o",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public void Rejects_the_malformed_openai_value(string key, string value)
    {
        Assert.Throws<InvalidArgumentException>(() =>
            CreateTestModel()
                .BuildSessionConfig(
                    WithProviderOptions("{\"openai\":{\"" + key + "\":" + value + "}}")
                )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "rejects delegation objects with extra keys",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public void Rejects_delegation_objects_with_extra_keys()
    {
        Assert.Throws<UnsupportedFunctionalityException>(() =>
            CreateTestModel()
                .BuildSessionConfig(
                    WithProviderOptions(
                        "{\"openai\":{\"delegation\":{\"type\":\"client\",\"extra\":true}}}"
                    )
                )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "accepts 128 input messages and every supported role and part type",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public void Accepts_128_input_messages_and_every_supported_role_and_part_type()
    {
        var input = new List<string>
        {
            Message("developer", "input_text"),
            Message("user", "input_text"),
            Message("assistant", "text"),
            Message("assistant", "output_text"),
        };
        input.AddRange(Enumerable.Repeat(Message("user", "input_text"), 124));

        var exception = Record.Exception(() =>
            CreateTestModel()
                .BuildSessionConfig(
                    WithProviderOptions(
                        "{\"openai\":{\"input\":[" + string.Join(",", input) + "]}}"
                    )
                )
        );

        Assert.Null(exception);
    }

    [Theory]
    [InlineData("commentary")]
    [InlineData("instructions")]
    [InlineData("thinking")]
    [UpstreamTest(
        Prefix + "accepts the %s context channel",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public void Accepts_the_context_channel(string channel)
    {
        var result = CreateTestModel()
            .SerializeClientEvent(
                new RealtimeModelV4ClientEvent("context-append")
                {
                    Content = "Note",
                    DelegationId = null,
                    ProviderOptions = System
                        .Text.Json.Nodes.JsonNode.Parse(
                            "{\"openai\":{\"channel\":\"" + channel + "\"}}"
                        )!
                        .AsObject(),
                }
            );

        Assert.Equal(channel, (string?)result["providerOptions"]!["openai"]!["channel"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "accepts supported provider options and leaves gateway options open",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public void Accepts_supported_provider_options_and_leaves_gateway_options_open()
    {
        var config = WithProviderOptions(
            """
            {
              "gateway": { "tags": ["demo"], "futureOption": { "nested": true } },
              "openai": {
                "delegation": null,
                "input": [{ "type": "message", "role": "user", "content": [{ "type": "input_text", "text": "Hi" }] }],
                "store": false,
                "voice": { "id": "voice_1" }
              }
            }
            """
        );

        var result = CreateTestModel().BuildSessionConfig(config);

        Assert.True(
            System.Text.Json.Nodes.JsonNode.DeepEquals(
                config.ProviderOptions,
                result.ProviderOptions
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "ignores unset settings",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public void Ignores_unset_settings()
    {
        var result = CreateTestModel()
            .BuildSessionConfig(
                new RealtimeModelV4SessionConfig
                {
                    Instructions = "Hi",
                    TurnDetectionDisabled = true,
                }
            );

        Assert.Equal("Hi", result.Instructions);
        Assert.Null(result.Tools);
        Assert.Null(result.TurnDetection);
    }

    [Theory]
    [InlineData("audio/pcm", 48000)]
    [InlineData("audio/pcm", null)]
    [InlineData("audio/pcmu", 16000)]
    [InlineData("audio/opus", 24000)]
    [UpstreamTest(
        Prefix + "rejects the unsupported audio format %o",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public void Rejects_the_unsupported_audio_format(string type, int? rate)
    {
        Assert.Throws<UnsupportedFunctionalityException>(() =>
            CreateTestModel()
                .BuildSessionConfig(
                    new RealtimeModelV4SessionConfig
                    {
                        InputAudioFormatType = type,
                        InputAudioFormatRate = rate,
                    }
                )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "accepts the supported audio formats",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public void Accepts_the_supported_audio_formats()
    {
        foreach (
            var (type, rate) in new[]
            {
                ("audio/pcm", 16000),
                ("audio/pcm", 24000),
                ("audio/pcma", 8000),
                ("audio/pcmu", 8000),
            }
        )
        {
            var result = CreateTestModel()
                .BuildSessionConfig(
                    new RealtimeModelV4SessionConfig
                    {
                        InputAudioFormatType = type,
                        InputAudioFormatRate = rate,
                        OutputAudioFormatType = type,
                        OutputAudioFormatRate = rate,
                    }
                );

            Assert.Equal(type, result.InputAudioFormatType);
            Assert.Equal(rate, result.InputAudioFormatRate);
            Assert.Equal(type, result.OutputAudioFormatType);
            Assert.Equal(rate, result.OutputAudioFormatRate);
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "requires matching input and output formats",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public void Requires_matching_input_and_output_formats()
    {
        Assert.Throws<InvalidArgumentException>(() =>
            CreateTestModel()
                .BuildSessionConfig(
                    new RealtimeModelV4SessionConfig
                    {
                        InputAudioFormatType = "audio/pcm",
                        InputAudioFormatRate = 16000,
                        OutputAudioFormatType = "audio/pcm",
                        OutputAudioFormatRate = 24000,
                    }
                )
        );
    }
}
