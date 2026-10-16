// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests;

/// <summary>Regression tests for bugs the C# audit found in provider helpers. Upstream has no matching test.</summary>
public sealed class ProviderBugRegressionTests
{
    [Fact]
    public async Task SendBytesAsync_sends_the_body_again_when_it_retries()
    {
        var handler = new ScriptedHandler();
        handler.Statuses.AddRange(new[] { HttpStatusCode.ServiceUnavailable, HttpStatusCode.OK });
        var http = new ProviderHttp(new HttpClient(handler), new RetryPolicy { MaxRetries = 1, InitialDelay = TimeSpan.FromMilliseconds(1), MaxDelay = TimeSpan.FromMilliseconds(1) });
        var content = new StringContent("{\"a\":1}", Encoding.UTF8, "application/json");

        await http.SendBytesAsync(HttpMethod.Post, new Uri("https://example.test/upload"), content, null, CancellationToken.None);

        Assert.Equal(2, handler.Calls);
        Assert.Equal("{\"a\":1}", handler.Body);
    }

    [Fact]
    public async Task SseParser_does_not_yield_a_pending_done_marker_at_the_end_of_the_stream()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("data: {\"a\":1}\n\ndata: [DONE]"));

        var seen = new List<string>();
        await foreach (var data in SseParser.ReadDataAsync(stream))
        {
            seen.Add(data);
        }

        Assert.Equal(new[] { "{\"a\":1}" }, seen);
    }

    [Fact]
    public async Task SseParser_strips_only_one_space_after_the_colon()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("data:   padded\n\n"));

        var seen = new List<string>();
        await foreach (var data in SseParser.ReadDataAsync(stream))
        {
            seen.Add(data);
        }

        Assert.Equal(new[] { "  padded" }, seen);
    }

    [Theory]
    [InlineData(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF })]
    [InlineData(new byte[] { 0x40, 0x00, 0x00, 0x00 })]
    public async Task Bedrock_event_stream_rejects_an_oversized_frame_length(byte[] prefix)
    {
        using var stream = new MemoryStream(prefix);

        await Assert.ThrowsAsync<AiSdkException>(async () =>
        {
            await foreach (var _ in AmazonBedrockEventStreamDecoder.Create<string>(stream, (_, _) => { }))
            {
            }
        });
    }

    [Fact]
    public async Task OpenAI_speech_retries_a_rate_limited_call()
    {
        var handler = new ScriptedHandler();
        handler.Statuses.AddRange(new[] { HttpStatusCode.TooManyRequests, HttpStatusCode.OK });
        var provider = OpenAIProvider.Create(new OpenAIOptions { ApiKey = "test-api-key" }, handler);

        await new OpenAISpeechModel(provider, "tts-1").GenerateAsync(new OpenAISpeechCall("hello"), CancellationToken.None);

        Assert.Equal(2, handler.Calls);
    }

    [Fact]
    public async Task OpenAI_speech_maps_a_network_failure_to_an_api_connection_exception()
    {
        var provider = OpenAIProvider.Create(new OpenAIOptions { ApiKey = "test-api-key" }, new ThrowingHandler());

        await Assert.ThrowsAsync<ApiConnectionException>(() =>
            new OpenAISpeechModel(provider, "tts-1").GenerateAsync(new OpenAISpeechCall("hello"), CancellationToken.None));
    }

    [Fact]
    public async Task OpenAI_image_edit_reports_invalid_json_as_a_json_parse_exception()
    {
        var handler = new RawHandler("not json");
        var provider = OpenAIProvider.Create(new OpenAIOptions { ApiKey = "test-api-key" }, handler);
        var call = new OpenAIImageCall("edit") { Files = new[] { new OpenAIImageFile(new byte[] { 1 }, "image/png", "a.png") } };

        await Assert.ThrowsAsync<JsonParseException>(() =>
            new OpenAIImageModel(provider, "gpt-image-1").GenerateAsync(call, CancellationToken.None));
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new HttpRequestException("connection refused");
        }
    }

    private sealed class RawHandler(string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body) });
        }
    }
}
