// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using Vercel.AI.Google;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Google.GoogleVertexAnthropicLabels;

/// <summary>Port of <c>google-vertex-anthropic-labels.test.ts</c>.</summary>
public sealed class GoogleVertexAnthropicLabelsTests
{
    private const string Response = "{\"type\":\"message\",\"id\":\"msg_1\",\"content\":[{\"type\":\"text\",\"text\":\"ok\"}],\"stop_reason\":\"end_turn\",\"usage\":{\"input_tokens\":1,\"output_tokens\":1}}";

    private const string Sse = "data: {\"type\":\"message_start\",\"message\":{\"id\":\"msg_1\",\"model\":\"claude\",\"usage\":{\"input_tokens\":1,\"output_tokens\":1}}}\n\n"
        + "data: {\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"end_turn\"},\"usage\":{\"output_tokens\":1}}\n\n"
        + "data: {\"type\":\"message_stop\"}\n\n";

    private const string DataPlatform = "eyJ0ZWFtIjoiZGF0YS1wbGF0Zm9ybSIsImZlYXR1cmUiOiJteS1hcHAifQ==";

    private sealed class Handler : HttpMessageHandler
    {
        private readonly bool _stream;

        public Handler(bool stream)
        {
            _stream = stream;
        }

        public List<Dictionary<string, string>> Headers { get; } = new();

        public List<string> Bodies { get; } = new();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var header in request.Headers)
            {
                headers[header.Key] = string.Join(",", header.Value);
            }

            lock (Headers)
            {
                Headers.Add(headers);
                Bodies.Add(request.Content == null ? string.Empty : request.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            }

            await Task.Yield();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_stream ? Sse : Response, Encoding.UTF8, _stream ? "text/event-stream" : "application/json"),
            };
        }
    }

    private static (ILanguageModel Model, Handler Handler) Create(string mode, Action<GoogleVertexAnthropicOptions>? configure = null)
    {
        var handler = new Handler(mode == "stream");
        var options = new GoogleVertexAnthropicOptions
        {
            Project = "test-project",
            Location = "us-east5",
            GenerateAuthToken = _ => Task.FromResult("test-token"),
        };
        options.Headers["x-custom-header"] = "preserved";
        configure?.Invoke(options);
        var provider = GoogleVertexAnthropicProvider.Create(options, handler);
        return (provider.LanguageModel("claude-sonnet-4-6@default"), handler);
    }

    private static async Task Run(ILanguageModel model, string mode, IReadOnlyDictionary<string, JsonElement>? providerOptions, IReadOnlyDictionary<string, string?>? headers = null)
    {
        var options = GoogleUpstream.Hello();
        options.ProviderOptions = providerOptions;
        options.Headers = headers;
        if (mode == "stream")
        {
            var result = await model.DoStreamAsync(options, CancellationToken.None).ConfigureAwait(false);
            await foreach (var part in result.Stream.ConfigureAwait(false))
            {
                Assert.IsNotType<ErrorStreamPart>(part);
            }
        }
        else
        {
            await model.DoGenerateAsync(options, CancellationToken.None).ConfigureAwait(false);
        }
    }

    private static Dictionary<string, JsonElement> Vertex(string json)
    {
        return GoogleUpstream.ProviderOptions("{\"vertex\":" + json + "}");
    }

    [Theory]
    [InlineData("generate")]
    [InlineData("stream")]
    [UpstreamTest("packages/google-vertex/src/anthropic/google-vertex-anthropic-labels.test.ts::Node Vertex Anthropic labels::generate::sends labels in the header and preserves authentication and other headers", Coverage = UpstreamCoverage.Covered, Note = "Edge runs the same HttpClient.")]
    [UpstreamTest("packages/google-vertex/src/anthropic/google-vertex-anthropic-labels.test.ts::Node Vertex Anthropic labels::stream::sends labels in the header and preserves authentication and other headers", Coverage = UpstreamCoverage.Covered, Note = "Edge runs the same HttpClient.")]
    public async Task Sends_labels_in_the_header_and_preserves_authentication_and_other_headers(string mode)
    {
        var (model, handler) = Create(mode);
        await Run(model, mode, Vertex("{\"labels\":{\"team\":\"data-platform\",\"feature\":\"my-app\"}}"), new Dictionary<string, string?> { ["anthropic-beta"] = "test-beta", ["x-request-header"] = "preserved" }).ConfigureAwait(false);

        var headers = Assert.Single(handler.Headers);
        Assert.Equal(DataPlatform, headers["x-vertex-ai-labels"]);
        Assert.Equal("Bearer test-token", headers["authorization"]);
        Assert.Equal("preserved", headers["x-custom-header"]);
        Assert.Equal("test-beta", headers["anthropic-beta"]);
        Assert.Equal("preserved", headers["x-request-header"]);
        Assert.False(System.Text.Json.Nodes.JsonNode.Parse(Assert.Single(handler.Bodies))!.AsObject().ContainsKey("labels"));
    }

    [Theory]
    [InlineData("generate","")]
    [InlineData("generate","{}")]
    [InlineData("stream","")]
    [InlineData("stream","{}")]
    [UpstreamTest("packages/google-vertex/src/anthropic/google-vertex-anthropic-labels.test.ts::Node Vertex Anthropic labels::generate::omits the header when labels are absent", Coverage = UpstreamCoverage.Covered, Note = "Edge runs the same HttpClient.")]
    [UpstreamTest("packages/google-vertex/src/anthropic/google-vertex-anthropic-labels.test.ts::Node Vertex Anthropic labels::stream::omits the header when labels are absent", Coverage = UpstreamCoverage.Covered, Note = "Edge runs the same HttpClient.")]
    public async Task Omits_the_header_when_labels_are_absent(string mode, string providerOptions)
    {
        var (model, handler) = Create(mode);
        await Run(model, mode, providerOptions.Length == 0 ? null : Vertex(providerOptions)).ConfigureAwait(false);

        Assert.False(Assert.Single(handler.Headers).ContainsKey("x-vertex-ai-labels"));
    }

    [Theory]
    [InlineData("generate")]
    [InlineData("stream")]
    [UpstreamTest("packages/google-vertex/src/anthropic/google-vertex-anthropic-labels.test.ts::Node Vertex Anthropic labels::generate::encodes an empty labels map", Coverage = UpstreamCoverage.Covered, Note = "Edge runs the same HttpClient.")]
    [UpstreamTest("packages/google-vertex/src/anthropic/google-vertex-anthropic-labels.test.ts::Node Vertex Anthropic labels::stream::encodes an empty labels map", Coverage = UpstreamCoverage.Covered, Note = "Edge runs the same HttpClient.")]
    public async Task Encodes_an_empty_labels_map(string mode)
    {
        var (model, handler) = Create(mode);
        await Run(model, mode, Vertex("{\"labels\":{}}")).ConfigureAwait(false);

        Assert.Equal("e30=", Assert.Single(handler.Headers)["x-vertex-ai-labels"]);
    }

    [Theory]
    [InlineData("generate")]
    [InlineData("stream")]
    [UpstreamTest("packages/google-vertex/src/anthropic/google-vertex-anthropic-labels.test.ts::Node Vertex Anthropic labels::generate::encodes labels as UTF-8", Coverage = UpstreamCoverage.Covered, Note = "Edge runs the same HttpClient.")]
    [UpstreamTest("packages/google-vertex/src/anthropic/google-vertex-anthropic-labels.test.ts::Node Vertex Anthropic labels::stream::encodes labels as UTF-8", Coverage = UpstreamCoverage.Covered, Note = "Edge runs the same HttpClient.")]
    public async Task Encodes_labels_as_UTF8(string mode)
    {
        var (model, handler) = Create(mode);
        await Run(model, mode, Vertex("{\"labels\":{\"team\":\"équipe\"}}")).ConfigureAwait(false);

        Assert.Equal("eyJ0ZWFtIjoiw6lxdWlwZSJ9", Assert.Single(handler.Headers)["x-vertex-ai-labels"]);
    }

    [Theory]
    [InlineData("generate","x-vertex-ai-labels")]
    [InlineData("generate","X-Vertex-AI-Labels")]
    [InlineData("stream","x-vertex-ai-labels")]
    [InlineData("stream","X-Vertex-AI-Labels")]
    [UpstreamTest("packages/google-vertex/src/anthropic/google-vertex-anthropic-labels.test.ts::Node Vertex Anthropic labels::generate::preserves an explicit request header", Coverage = UpstreamCoverage.Covered, Note = "Edge runs the same HttpClient.")]
    [UpstreamTest("packages/google-vertex/src/anthropic/google-vertex-anthropic-labels.test.ts::Node Vertex Anthropic labels::stream::preserves an explicit request header", Coverage = UpstreamCoverage.Covered, Note = "Edge runs the same HttpClient.")]
    public async Task Preserves_an_explicit_request_header(string mode, string headerName)
    {
        var (model, handler) = Create(mode);
        await Run(model, mode, Vertex("{\"labels\":{\"team\":\"data-platform\"}}"), new Dictionary<string, string?> { [headerName] = "explicit-labels" }).ConfigureAwait(false);

        Assert.Equal("explicit-labels", Assert.Single(handler.Headers)["x-vertex-ai-labels"]);
    }

    [Theory]
    [InlineData("generate")]
    [InlineData("stream")]
    [UpstreamTest("packages/google-vertex/src/anthropic/google-vertex-anthropic-labels.test.ts::Node Vertex Anthropic labels::generate::preserves an explicit provider labels header", Coverage = UpstreamCoverage.Covered, Note = "Edge runs the same HttpClient.")]
    [UpstreamTest("packages/google-vertex/src/anthropic/google-vertex-anthropic-labels.test.ts::Node Vertex Anthropic labels::stream::preserves an explicit provider labels header", Coverage = UpstreamCoverage.Covered, Note = "Edge runs the same HttpClient.")]
    public async Task Preserves_an_explicit_provider_labels_header(string mode)
    {
        var (model, handler) = Create(mode, options => options.Headers["X-Vertex-AI-Labels"] = "provider-labels");
        await Run(model, mode, Vertex("{\"labels\":{\"team\":\"data-platform\"}}")).ConfigureAwait(false);

        Assert.Equal("provider-labels", Assert.Single(handler.Headers)["x-vertex-ai-labels"]);
    }

    [Theory]
    [InlineData("generate","null")]
    [InlineData("generate","\"invalid\"")]
    [InlineData("generate","{\"team\":123}")]
    [InlineData("generate","[\"team\"]")]
    [InlineData("stream","null")]
    [InlineData("stream","\"invalid\"")]
    [InlineData("stream","{\"team\":123}")]
    [InlineData("stream","[\"team\"]")]
    [UpstreamTest("packages/google-vertex/src/anthropic/google-vertex-anthropic-labels.test.ts::Node Vertex Anthropic labels::generate::rejects invalid labels before sending a request", Coverage = UpstreamCoverage.Covered, Note = "Edge runs the same HttpClient.")]
    [UpstreamTest("packages/google-vertex/src/anthropic/google-vertex-anthropic-labels.test.ts::Node Vertex Anthropic labels::stream::rejects invalid labels before sending a request", Coverage = UpstreamCoverage.Covered, Note = "Edge runs the same HttpClient.")]
    public async Task Rejects_invalid_labels_before_sending_a_request(string mode, string labels)
    {
        var (model, handler) = Create(mode);
        await Assert.ThrowsAsync<InvalidArgumentError>(() => Run(model, mode, Vertex("{\"labels\":" + labels + "}"))).ConfigureAwait(false);

        Assert.Empty(handler.Headers);
    }

    [Theory]
    [InlineData("generate")]
    [InlineData("stream")]
    [UpstreamTest("packages/google-vertex/src/anthropic/google-vertex-anthropic-labels.test.ts::Node Vertex Anthropic labels::generate::keeps labels isolated across concurrent and subsequent calls", Coverage = UpstreamCoverage.Covered, Note = "Edge runs the same HttpClient.")]
    [UpstreamTest("packages/google-vertex/src/anthropic/google-vertex-anthropic-labels.test.ts::Node Vertex Anthropic labels::stream::keeps labels isolated across concurrent and subsequent calls", Coverage = UpstreamCoverage.Covered, Note = "Edge runs the same HttpClient.")]
    public async Task Keeps_labels_isolated_across_concurrent_and_subsequent_calls(string mode)
    {
        var (model, handler) = Create(mode);
        await Task.WhenAll(
            Run(model, mode, Vertex("{\"labels\":{\"team\":\"data-platform\",\"feature\":\"my-app\"}}")),
            Run(model, mode, Vertex("{\"labels\":{\"team\":\"other\"}}"))).ConfigureAwait(false);
        await Run(model, mode, null).ConfigureAwait(false);

        Assert.Equal(3, handler.Headers.Count);
        var sent = handler.Headers.Take(2).Select(h => h["x-vertex-ai-labels"]).OrderBy(v => v, StringComparer.Ordinal).ToArray();
        Assert.Equal(new[] { DataPlatform, "eyJ0ZWFtIjoib3RoZXIifQ==" }.OrderBy(v => v, StringComparer.Ordinal).ToArray(), sent);
        Assert.False(handler.Headers[2].ContainsKey("x-vertex-ai-labels"));
    }
}
