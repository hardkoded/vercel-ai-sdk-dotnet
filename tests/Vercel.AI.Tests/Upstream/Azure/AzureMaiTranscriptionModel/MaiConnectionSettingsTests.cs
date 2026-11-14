// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Tests.Upstream.ProviderUtils;
using static Vercel.AI.Tests.Upstream.Azure.AzureMaiTestSupport;

namespace Vercel.AI.Tests.Upstream.Azure.AzureMaiTranscriptionModel;

/// <summary>Port of <c>azure-mai-transcription-model.test.ts</c> &gt; <c>MAI connection settings</c>.</summary>
[Collection("MockWebSocket")]
public sealed class MaiConnectionSettingsTests
{
    private const string TestPrefix = "packages/azure/src/azure-mai-transcription-model.test.ts::MAI connection settings::";

    [Fact]
    [UpstreamTest(TestPrefix + "uses maiBaseURL for the realtime endpoint", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Uses_maiBaseURL_for_the_realtime_endpoint()
    {
        var stream = await StartStream(options => options.MaiBaseUrl = "https://my-foundry.services.ai.azure.com/mai/v1/");

        Assert.Equal("wss://my-foundry.services.ai.azure.com/mai/v1/realtime?intent=transcription", stream.Socket.Url.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest(TestPrefix + "moves the key to the query string for native WebSocket", Coverage = UpstreamCoverage.Covered, Note = "The native WebSocket is a test seam on the options, because this port has no global WebSocket to stub.", Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Moves_the_key_to_the_query_string_for_native_WebSocket()
    {
        var stream = await StartStream(options => options.NativeWebSocket = MockWebSocket.Factory, customWebSocket: false);

        var query = System.Web.HttpUtility.ParseQueryString(stream.Socket.Url.Query);
        Assert.Equal("test-key", query["api-key"]);
        Assert.Equal("transcription", query["intent"]);
        Assert.False(stream.Socket.Headers!.ContainsKey("api-key"));
    }

    [Fact]
    [UpstreamTest(TestPrefix + "sends an Entra bearer token", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Sends_an_Entra_bearer_token()
    {
        var stream = await StartStream(options =>
        {
            options.ApiKey = null;
            options.TokenProvider = () => "entra-token";
        });

        Assert.Equal("Bearer entra-token", stream.Socket.Headers!["authorization"]);
        Assert.False(stream.Socket.Headers.ContainsKey("api-key"));
    }

    [Fact]
    [UpstreamTest(TestPrefix + "rejects Entra bearer auth without a custom WebSocket", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Rejects_Entra_bearer_auth_without_a_custom_WebSocket()
    {
        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() => StartStream(
            options =>
            {
                options.ApiKey = null;
                options.TokenProvider = () => "entra-token";
                options.NativeWebSocket = MockWebSocket.Factory;
            },
            customWebSocket: false));

        Assert.Equal("webSocket", error.Parameter);
        Assert.Empty(MockWebSocket.Instances);
    }

    [Fact]
    [UpstreamTest(TestPrefix + "routes custom deployment names with api=mai", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Routes_custom_deployment_names_with_api_mai()
    {
        var stream = await StartStream(modelId: "my-streaming-deployment", providerOptions: "{\"azure\":{\"api\":\"mai\"}}");
        var ws = stream.Socket;

        ws.Open();
        ws.Message(new JsonObject { ["type"] = "session.created" });
        await Wait.UntilAsync(() => ws.Sent.Count >= 1);

        var model = JsonNode.Parse((string)ws.Sent[0])!["session"]!["audio"]!["input"]!["transcription"]!["model"]!.GetValue<string>();
        Assert.Equal("my-streaming-deployment", model);
    }
}
