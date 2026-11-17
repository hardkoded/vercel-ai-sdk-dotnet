// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Live = Vercel.AI.OpenAI.OpenAIRealtimeModelLive;
using Realtime = Vercel.AI.OpenAI.OpenAIRealtimeModel;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIRealtimeFactory;

/// <summary>Port of <c>openai-realtime-factory.test.ts</c> &gt; <c>OpenAI realtime factory</c>.</summary>
public sealed class OpenAIRealtimeFactoryTests
{
    private const string Prefix =
        "packages/openai/src/realtime/openai-realtime-factory.test.ts::OpenAI realtime factory::";

    private static OpenAIProvider KeylessProvider(
        OpenAICapture capture,
        Action<OpenAIOptions>? configure = null
    )
    {
        // An environment variable that is never set stands for upstream's stubbed empty OPENAI_API_KEY.
        return OpenAIUpstream.Provider(capture, options =>
        {
            options.ApiKey = null;
            options.ApiKeyEnvironmentVariable = "OPENAI_API_KEY_UNSET_FOR_REALTIME_FACTORY_TESTS";
            configure?.Invoke(options);
        });
    }

    [Fact]
    [UpstreamTest(Prefix + "exposes one public factory", Coverage = UpstreamCoverage.Covered)]
    public void Exposes_one_public_factory()
    {
        var members = typeof(OpenAIProvider).GetMembers().Select(member => member.Name);

        Assert.DoesNotContain("ExperimentalLive", members);
        Assert.DoesNotContain("Live", members);
    }

    [Theory]
    [InlineData("gpt-live-1", false, null, true)]
    [InlineData("gpt-live-1", true, null, true)]
    [InlineData("gpt-realtime", false, null, false)]
    [InlineData("unknown", false, null, false)]
    [InlineData("gpt-live", false, null, false)]
    [InlineData("gpt-live-1-preview", false, null, false)]
    [InlineData("prefix-gpt-live-1", false, null, false)]
    [InlineData("GPT-LIVE-1", false, null, false)]
    [InlineData("not-yet-released", true, "live", true)]
    [InlineData("gpt-realtime", true, "live", true)]
    [InlineData("gpt-live-1", true, "realtime", false)]
    [UpstreamTest(Prefix + "routes %s with %j", Coverage = UpstreamCoverage.Covered)]
    public void Routes_model_with_options(string modelId, bool hasOptions, string? api, bool live)
    {
        var provider = OpenAIUpstream.Provider(new OpenAICapture());

        var model = provider.RealtimeModel(
            modelId,
            hasOptions ? new OpenAIRealtimeOptions { Api = api } : null
        );

        if (live)
        {
            Assert.IsType<Live>(model);
            Assert.Equal("v4", ((Live)model).SpecificationVersion);
        }
        else
        {
            Assert.IsType<Realtime>(model);
            Assert.Equal("v4", ((Realtime)model).SpecificationVersion);
        }

        Assert.Equal(modelId, model.ModelId);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("")]
    [UpstreamTest(
        Prefix + "rejects an invalid selector %j in construction and token minting",
        Coverage = UpstreamCoverage.Partial,
        Note = "Only the string selectors can be expressed: upstream types api as 'live' | 'realtime' and passes false, 1 and {} through a type cast; OpenAIRealtimeOptions.Api is a string?, which has no equivalent for non-string values, and null means unset."
    )]
    public async Task Partial_Rejects_an_invalid_selector_in_construction_and_token_minting(
        string api
    )
    {
        var capture = new OpenAICapture();
        var provider = OpenAIUpstream.Provider(capture);
        var options = new OpenAIRealtimeOptions { Api = api };

        Assert.Throws<InvalidArgumentException>(() =>
            provider.RealtimeModel("gpt-live-1", options)
        );
        var exception = Assert.Throws<InvalidArgumentException>(() =>
            provider.RealtimeModel("unknown", options)
        );
        Assert.Contains(
            "OpenAI realtime api must be \"live\" or \"realtime\".",
            exception.Message,
            StringComparison.Ordinal
        );
        await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            provider.ExperimentalRealtimeGetTokenAsync("gpt-live-1", options)
        );
        Assert.Equal(0, capture.Calls);
    }

    [Theory]
    [InlineData("live")]
    [InlineData("realtime")]
    [UpstreamTest(
        Prefix + "constructs %s codecs without an API key or network request",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Constructs_codecs_without_an_API_key_or_network_request(string api)
    {
        var capture = new OpenAICapture();
        var model = KeylessProvider(capture)
            .RealtimeModel("not-yet-released", new OpenAIRealtimeOptions { Api = api });
        var unknown = OpenAIUpstream.Json("{\"type\":\"unknown\"}");
        var append = new RealtimeModelV4ClientEvent("input-audio-append") { Audio = "AA==" };

        JsonObject session;
        object parsed;
        JsonNode? serialized;
        if (model is Live live)
        {
            session = live.BuildSessionConfig(new RealtimeModelV4SessionConfig());
            parsed = live.ParseServerEvent(unknown);
            serialized = live.SerializeClientEvent(append);
        }
        else
        {
            var realtime = (Realtime)model;
            session = realtime.BuildSessionConfig(new RealtimeModelV4SessionConfig());
            parsed = realtime.ParseServerEvent(unknown);
            serialized = realtime.SerializeClientEvent(append);
        }

        Assert.Equal("not-yet-released", session["model"]!.GetValue<string>());
        Assert.NotNull(parsed);
        Assert.NotNull(serialized);
        Assert.Equal(0, capture.Calls);
    }

    [Theory]
    [InlineData("live")]
    [InlineData("realtime")]
    [UpstreamTest(
        Prefix
            + "forwards the shared constructor configuration to %s without serializing the selector",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Forwards_the_shared_constructor_configuration_without_serializing_the_selector(
        string api
    )
    {
        var provider = OpenAIUpstream.Provider(new OpenAICapture(), options =>
        {
            options.ApiKey = "test-key";
            options.BaseUrl = "https://example.com/proxy/v1/";
            options.Headers["X-Custom"] = "value";
            options.ProviderName = "custom";
        });

        var model = provider.RealtimeModel(
            "not-yet-released",
            new OpenAIRealtimeOptions { Api = api }
        );

        Assert.Equal("custom." + api, model.Provider);
        if (model is Live live)
        {
            Assert.Equal("https://example.com/proxy/v1", live.Config.Options.BaseUrl);
            Assert.Same(provider.Http, live.Config.Http);
            var connection = await live.GetServerWebSocketConfigAsync(null, CancellationToken.None);
            Assert.Equal("wss://example.com/proxy/v1/live/sessions", connection.Url);
            Assert.Equal("Bearer test-key", connection.Headers["authorization"]);
            Assert.Equal("value", connection.Headers["x-custom"]);
            Assert.Null(live.BuildSessionConfig(new RealtimeModelV4SessionConfig())["api"]);
            var start = live.SerializeClientEvent(
                new RealtimeModelV4ClientEvent("session-start")
                {
                    Config = new RealtimeModelV4SessionConfig(),
                }
            )!;
            Assert.Equal("not-yet-released", start["session"]!["model"]!.GetValue<string>());
            Assert.Null(start["session"]!["api"]);
        }
        else
        {
            var realtime = (Realtime)model;
            Assert.Equal("https://example.com/proxy/v1", realtime.Config.BaseUrl);
            Assert.Same(provider.Http, realtime.Config.Http);
            var connection = await realtime.GetServerWebSocketConfigAsync(
                null,
                CancellationToken.None
            );
            Assert.StartsWith(
                "wss://example.com/proxy/v1/realtime",
                connection.Url,
                StringComparison.Ordinal
            );
            Assert.Equal("Bearer test-key", connection.Headers["authorization"]);
            Assert.Equal("value", connection.Headers["x-custom"]);
            Assert.Null(realtime.BuildSessionConfig(new RealtimeModelV4SessionConfig())["api"]);
        }
    }
}
