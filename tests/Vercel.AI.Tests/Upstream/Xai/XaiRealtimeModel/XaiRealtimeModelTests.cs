// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Xai;
using Model = global::Vercel.AI.Xai.XaiRealtimeModel;

namespace Vercel.AI.Tests.Upstream.Xai.XaiRealtimeModel;

/// <summary>Port of <c>xai-realtime-model.test.ts</c> &gt; <c>XaiRealtimeModel</c>.</summary>
public sealed class XaiRealtimeModelTests
{
    private const string Id = "packages/xai/src/realtime/xai-realtime-model.test.ts::XaiRealtimeModel::";
    private const string Commit = "892af4dab7f1351fd2ced70dcaba288746d8a782";

    [Theory]
    [InlineData("object")]
    [InlineData("promise")]
    [InlineData("callback")]
    [InlineData("async callback")]
    [UpstreamTest(Id + "resolves $name credentials for server connections", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Resolves_credentials_for_server_connections(string name)
    {
        IReadOnlyDictionary<string, string?> credentials = new Dictionary<string, string?> { ["authorization"] = "Bearer test-key", ["omitted"] = null };
        var promise = Task.FromResult(credentials);
        Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>> headers = name switch
        {
            "object" => _ => Task.FromResult(credentials),
            "promise" => _ => promise,
            "callback" => _ => Task.FromResult<IReadOnlyDictionary<string, string?>>(new Dictionary<string, string?> { ["authorization"] = "Bearer test-key", ["omitted"] = null }),
            _ => async _ =>
            {
                await Task.Yield();
                return new Dictionary<string, string?> { ["authorization"] = "Bearer test-key", ["omitted"] = null };
            },
        };

        var model = new Model("grok-voice-latest", new XaiRealtimeModelConfig("xai.realtime", "https://api.x.ai/v1", headers));

        var connection = await model.GetServerWebSocketConfigAsync(null, CancellationToken.None);
        var header = Assert.Single(connection.Headers);
        Assert.Equal("authorization", header.Key);
        Assert.Equal("Bearer test-key", header.Value);
    }

    [Fact]
    [UpstreamTest(Id + "rejects unsupported intents during serialization as well as connection setup", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Rejects_unsupported_intents_during_serialization_as_well_as_connection_setup()
    {
        var target = XaiRealtimeTestSupport.CreateModel();
        var intent = RealtimeModelV4SessionIntent.Transcription;

        var serialize = Assert.Throws<AiSdkException>(() => target.SerializeClientEvent(new RealtimeModelV4ClientEvent("response-create"), intent));
        Assert.Contains("does not support", serialize.Message, StringComparison.Ordinal);
        var build = Assert.Throws<AiSdkException>(() => target.BuildSessionConfig(new RealtimeModelV4SessionConfig(), intent));
        Assert.Contains("does not support", build.Message, StringComparison.Ordinal);
    }
}
