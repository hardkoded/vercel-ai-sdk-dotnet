// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayRealtimeModel.GatewayRealtimeModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayRealtimeModel;

/// <summary>Port of <c>gateway-realtime-model.test.ts</c> &gt; <c>GatewayRealtimeModel</c>.</summary>
public sealed class GatewayRealtimeModelTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-realtime-model.test.ts::GatewayRealtimeModel::";

    [Fact]
    [UpstreamTest(
        Prefix + "rejects unsupported intents during serialization as well as connection setup",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public void Rejects_unsupported_intents_during_serialization_as_well_as_connection_setup()
    {
        var target = CreateTestModel();
        var intent = RealtimeModelV4SessionIntent.Transcription;

        var serialize = Assert.Throws<InvalidOperationException>(() =>
            target.SerializeClientEvent(new RealtimeModelV4ClientEvent("response-create"), intent)
        );
        Assert.Contains("does not support", serialize.Message, StringComparison.Ordinal);
        var build = Assert.Throws<InvalidOperationException>(() =>
            target.BuildSessionConfig(new RealtimeModelV4SessionConfig(), intent)
        );
        Assert.Contains("does not support", build.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "exposes the v4 specification, provider, and model id",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Exposes_the_v4_specification_provider_and_model_id()
    {
        var model = CreateTestModel();
        Assert.Equal("v4", model.SpecificationVersion);
        Assert.Equal("gateway.realtime", model.Provider);
        Assert.Equal("openai/gpt-realtime", model.ModelId);
    }
}
