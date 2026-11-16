// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleRealtimeModel;

/// <summary>Port of <c>google-realtime-model.test.ts</c> &gt; <c>GoogleRealtimeModel</c>.</summary>
public sealed class GoogleRealtimeModelTests
{
    private const string Id = "packages/google/src/realtime/google-realtime-model.test.ts::GoogleRealtimeModel::";

    [Theory]
    [InlineData("object")]
    [InlineData("promise")]
    [InlineData("callback")]
    [InlineData("async callback")]
    [UpstreamTest(Id + "resolves $name credentials for server connections", Coverage = UpstreamCoverage.Covered, Commit = "892af4dab7f1351fd2ced70dcaba288746d8a782")]
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
            }
            ,
        };

        var model = GoogleRealtimeTestSupport.CreateModel("gemini-3.8-live", headers);

        var connection = await model.GetServerWebSocketConfigAsync(null, CancellationToken.None);
        var header = Assert.Single(connection.Headers);
        Assert.Equal("authorization", header.Key);
        Assert.Equal("Bearer test-key", header.Value);
    }

    [Fact]
    [UpstreamTest(Id + "rejects unsupported intents during serialization as well as connection setup", Coverage = UpstreamCoverage.Covered, Commit = "892af4dab7f1351fd2ced70dcaba288746d8a782")]
    public void Rejects_unsupported_intents_during_serialization_as_well_as_connection_setup()
    {
        var target = GoogleRealtimeTestSupport.CreateModel("gemini-3.8-live", GoogleRealtimeTestSupport.ApiKeyHeaders());
        var intent = RealtimeModelV4SessionIntent.Transcription;

        var serialize = Assert.Throws<AiSdkException>(() => target.SerializeClientEvent(new RealtimeModelV4ClientEvent("response-create"), intent));
        Assert.Contains("does not support", serialize.Message, StringComparison.Ordinal);
        var build = Assert.Throws<AiSdkException>(() => target.BuildSessionConfig(new RealtimeModelV4SessionConfig(), intent));
        Assert.Contains("does not support", build.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Id + "isolates usage and turn state between server connections", Coverage = UpstreamCoverage.Covered, Commit = "892af4dab7f1351fd2ced70dcaba288746d8a782")]
    public void Isolates_usage_and_turn_state_between_server_connections()
    {
        var model = GoogleRealtimeTestSupport.CreateModel("gemini-3.8-live", GoogleRealtimeTestSupport.ApiKeyHeaders());
        var first = model.CreateServerEventParser();
        var second = model.CreateServerEventParser();
        var firstUsage = Assert.Single(first(GoogleUpstream.Element("""{"usageMetadata":{"promptTokensDetails":[{"modality":"TEXT","tokenCount":12}]}}""")));
        var secondDone = second(GoogleUpstream.Element("""{"serverContent":{"turnComplete":true}}"""));
        Assert.Contains(secondDone, e => e.Type == "response-done" && e.ResponseId == "google-resp-0");
        Assert.DoesNotContain(secondDone, e => e.Usage != null);
        Assert.Equal("custom", firstUsage.Type);
        Assert.Equal("usageMetadata", firstUsage.RawType);
        Assert.Equal(12, firstUsage.Usage!.InputTextTokens);
        var firstDone = first(GoogleUpstream.Element("""{"serverContent":{"turnComplete":true}}"""));
        Assert.DoesNotContain(firstDone, e => e.Usage != null);
        first(GoogleUpstream.Element("""{"serverContent":{"modelTurn":{"parts":[{"text":"next"}]},"turnComplete":true}}"""));
        first(GoogleUpstream.Element("""{"serverContent":{"modelTurn":{"parts":[{"text":"third"}]}}}"""));
        var secondTurn = second(GoogleUpstream.Element("""{"serverContent":{"modelTurn":{"parts":[{"text":"independent"}]}}}"""));
        Assert.Contains(secondTurn, e => e.Type == "text-delta" && e.ResponseId == "google-resp-1");
    }
}
