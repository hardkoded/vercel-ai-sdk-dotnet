// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using RealtimeModel = Vercel.AI.OpenAI.OpenAIRealtimeModel;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIRealtimeModel;

/// <summary>Port of <c>openai-realtime-model.test.ts</c> &gt; <c>OpenAIRealtimeModel</c> &gt; <c>doCreateClientSecret</c>.</summary>
public sealed class DoCreateClientSecretTests
{
    private const string Prefix =
        "packages/openai/src/realtime/openai-realtime-model.test.ts::OpenAIRealtimeModel > doCreateClientSecret::";

    [Fact]
    [UpstreamTest(
        Prefix + "omits expires_after when no ttl is requested",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Omits_expires_after_when_no_ttl_is_requested()
    {
        var capture = new OpenAICapture
        {
            ResponseJson = "{\"value\":\"ek_test\",\"expires_at\":1700000000}",
        };
        var secret = await new RealtimeModel(
            OpenAIUpstream.Provider(capture),
            "gpt-4o-realtime-preview"
        ).CreateClientSecretAsync(null, CancellationToken.None);
        OpenAIUpstream.Equal(
            JsonNode.Parse(capture.Body),
            "{\"session\":{\"type\":\"realtime\",\"model\":\"gpt-4o-realtime-preview\"}}"
        );
        Assert.Equal("ek_test", secret.Token);
        Assert.Equal("wss://api.openai.com/v1/realtime?model=gpt-4o-realtime-preview", secret.Url);
        Assert.DoesNotContain("expires_after", capture.Body, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "includes the required anchor with expires_after",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Includes_the_required_anchor_with_expires_after()
    {
        var capture = new OpenAICapture
        {
            ResponseJson = "{\"value\":\"ek_test\",\"expires_at\":1700000060}",
        };
        await new RealtimeModel(
            OpenAIUpstream.Provider(capture),
            "gpt-4o-realtime-preview"
        ).CreateClientSecretAsync(60, CancellationToken.None);
        // The client secrets endpoint 400s without `anchor`.
        OpenAIUpstream.Equal(
            JsonNode.Parse(capture.Body)!["expires_after"],
            "{\"anchor\":\"created_at\",\"seconds\":60}"
        );
    }
}
