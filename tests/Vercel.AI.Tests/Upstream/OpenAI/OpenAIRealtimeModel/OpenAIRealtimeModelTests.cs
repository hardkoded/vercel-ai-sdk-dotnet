// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAI;
using RealtimeModel = Vercel.AI.OpenAI.OpenAIRealtimeModel;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIRealtimeModel;

/// <summary>Port of <c>openai-realtime-model.test.ts</c> &gt; <c>OpenAIRealtimeModel</c>.</summary>
public sealed class OpenAIRealtimeModelTests
{
    [Theory]
    [InlineData("object")]
    [InlineData("promise")]
    [InlineData("callback")]
    [InlineData("async callback")]
    [UpstreamTest(
        "packages/openai/src/realtime/openai-realtime-model.test.ts::OpenAIRealtimeModel::resolves $name credentials for server connections",
        Coverage = UpstreamCoverage.Covered,
        Commit = "892af4dab7f1351fd2ced70dcaba288746d8a782"
    )]
    public async Task Resolves_credentials_for_server_connections(string name)
    {
        IReadOnlyDictionary<string, string?> credentials = new Dictionary<string, string?>
        {
            ["authorization"] = "Bearer test-key",
            ["omitted"] = null,
        };
        var promise = Task.FromResult(credentials);
        Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>> headers = name switch
        {
            "object" => _ => Task.FromResult(credentials),
            "promise" => _ => promise,
            "callback" => _ =>
                Task.FromResult<IReadOnlyDictionary<string, string?>>(
                    new Dictionary<string, string?>
                    {
                        ["authorization"] = "Bearer test-key",
                        ["omitted"] = null,
                    }
                ),
            _ => async _ =>
            {
                await Task.Yield();
                return new Dictionary<string, string?>
                {
                    ["authorization"] = "Bearer test-key",
                    ["omitted"] = null,
                };
            },
        };

        var model = new RealtimeModel(
            "gpt-realtime",
            new OpenAIRealtimeModelConfig("openai.realtime", "https://api.openai.com/v1", headers)
        );

        var connection = await model.GetServerWebSocketConfigAsync(null, CancellationToken.None);
        var header = Assert.Single(connection.Headers);
        Assert.Equal("authorization", header.Key);
        Assert.Equal("Bearer test-key", header.Value);
    }
}
