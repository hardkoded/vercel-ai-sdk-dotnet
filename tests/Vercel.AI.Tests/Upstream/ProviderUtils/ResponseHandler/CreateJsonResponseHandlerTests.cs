// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.ResponseHandler;

public sealed class CreateJsonResponseHandlerTests
{
    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/response-handler.test.ts::createJsonResponseHandler::should return both parsed value and rawValue",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Json_response_keeps_the_raw_value_and_projects_known_properties()
    {
        var schema = JsonSchemas.Object(
            new[]
            {
                Pair("name", JsonSchemas.String()),
                Pair("age", JsonSchemas.Number()),
            },
            new[] { "name", "age" },
            additionalPropertiesFlag: false);
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("{\"name\":\"John\",\"age\":30,\"extraField\":\"ignored\"}", Encoding.UTF8, "application/json"),
        };
        var body = await JsonStreams.ReadJsonAsync(response, schema);
        JsonAssert.Equal(body.Value, "{\"name\":\"John\",\"age\":30}");
        JsonAssert.Equal(body.RawValue, "{\"name\":\"John\",\"age\":30,\"extraField\":\"ignored\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/response-handler.test.ts::createJsonResponseHandler::should reject oversized responses before reading the body",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Json_response_rejects_oversized_responses_before_reading_the_body()
    {
        var content = new OversizedContent("{}");
        using var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = content };
        var schema = JsonSchemas.Object(Array.Empty<KeyValuePair<string, JsonNode>>(), Array.Empty<string>());
        var error = await Assert.ThrowsAsync<DownloadError>(() => JsonStreams.ReadJsonAsync(response, schema));
        Assert.Contains("exceeded maximum size", error.Message);
        Assert.False(content.BodyRead);
        Assert.True(content.Disposed);
    }

    private static KeyValuePair<string, JsonNode> Pair(string name, JsonNode schema)
    {
        return new KeyValuePair<string, JsonNode>(name, schema);
    }

    private sealed class OversizedContent : HttpContent
    {
        private readonly byte[] _bytes;

        public OversizedContent(string body)
        {
            _bytes = Encoding.UTF8.GetBytes(body);
            Headers.ContentLength = Download.DefaultMaxBytes + 1;
        }

        public bool BodyRead { get; private set; }

        public bool Disposed { get; private set; }

        protected override Task SerializeToStreamAsync(Stream stream, System.Net.TransportContext? context)
        {
            BodyRead = true;
            return stream.WriteAsync(_bytes, 0, _bytes.Length);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = Download.DefaultMaxBytes + 1;
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
        }
    }
}
