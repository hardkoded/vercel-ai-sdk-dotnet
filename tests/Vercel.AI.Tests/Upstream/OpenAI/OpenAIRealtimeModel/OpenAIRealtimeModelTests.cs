// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using RealtimeModel = Vercel.AI.OpenAI.OpenAIRealtimeModel;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIRealtimeModel;

/// <summary>Port of <c>openai-realtime-model.test.ts</c> &gt; <c>OpenAIRealtimeModel</c>.</summary>
public sealed class OpenAIRealtimeModelTests
{
    [Fact]
    [UpstreamTest(
        "packages/openai/src/realtime/openai-realtime-model.test.ts::OpenAIRealtimeModel::resolves $name credentials for server connections",
        Coverage = UpstreamCoverage.Partial,
        Commit = "892af4dab7f1351fd2ced70dcaba288746d8a782",
        Note = "The .NET provider has no Resolvable headers (promise, callback, async callback). Checks that the provider credentials resolve for a server connection and that a header without a value is dropped.")]
    public async Task Resolves_provider_credentials_for_server_connections()
    {
        var provider = OpenAIUpstream.Provider(new OpenAICapture(), options => options.Headers["omitted"] = null!);
        var model = new RealtimeModel(provider, "gpt-realtime");

        var headers = (await model.GetServerWebSocketConfigAsync(null, CancellationToken.None)).Headers;

        Assert.Equal("Bearer test-api-key", headers["authorization"]);
        Assert.DoesNotContain("omitted", headers.Keys);
    }
}
