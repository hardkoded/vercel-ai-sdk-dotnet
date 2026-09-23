// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using Vercel.AI.Sdk.OpenAICompatible;
using Vercel.AI.Sdk.Provider;
using Vercel.AI.Sdk.ProviderUtils;

namespace Vercel.AI.Sdk.Tests;

public sealed class HttpTests
{
    [Fact]
    public async Task Retries_a_retryable_status_then_returns_the_body()
    {
        var handler = new ScriptedHandler();
        handler.Statuses.Add(HttpStatusCode.InternalServerError);
        handler.Statuses.Add(HttpStatusCode.OK);
        var http = new ProviderHttp(new HttpClient(handler), new RetryPolicy
        {
            MaxRetries = 2,
            InitialDelay = TimeSpan.FromMilliseconds(1),
            MaxDelay = TimeSpan.FromMilliseconds(5),
            Jitter = 0,
        });

        using var document = await http.SendJsonAsync(HttpMethod.Post, new Uri("https://example.test/v1"), "{}", null, CancellationToken.None);
        Assert.Equal(2, handler.Calls);
        Assert.Equal(JsonValueKind.Object, document.RootElement.ValueKind);
    }

    [Fact]
    public async Task Maps_401_to_authentication_exception()
    {
        var handler = new ScriptedHandler();
        handler.Statuses.Add(HttpStatusCode.Unauthorized);
        var http = new ProviderHttp(new HttpClient(handler), new RetryPolicy { MaxRetries = 0 });
        var error = await Assert.ThrowsAsync<AuthenticationException>(() =>
            http.SendJsonAsync(HttpMethod.Post, new Uri("https://example.test/v1"), "{}", null, CancellationToken.None));
        Assert.Equal(401, error.StatusCode);
        Assert.Contains("nope", error.Message);
    }

    [Theory]
    [InlineData(400, typeof(BadRequestException))]
    [InlineData(403, typeof(PermissionDeniedException))]
    [InlineData(404, typeof(NotFoundException))]
    [InlineData(422, typeof(UnprocessableEntityException))]
    [InlineData(429, typeof(RateLimitException))]
    [InlineData(500, typeof(InternalServerException))]
    public void MapStatus_uses_the_status_subclass(int status, Type type)
    {
        var error = ProviderHttp.MapStatus(status, "{\"error\":{\"message\":\"x\"}}");
        Assert.IsType(type, error);
        Assert.Equal("x", error.Message);
    }

    [Fact]
    public async Task Chat_completions_stream_reads_sse_deltas()
    {
        var handler = new SseHandler(
            "data: {\"choices\":[{\"delta\":{\"content\":\"Hi\"}}]}\n\n" +
            "data: {\"choices\":[{\"delta\":{},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":1,\"completion_tokens\":1,\"total_tokens\":2}}\n\n" +
            "data: [DONE]\n\n");
        var provider = OpenAICompatibleProvider.Create(
            new OpenAICompatibleOptions { ApiKey = "secret", ProviderName = "openai-compatible" },
            handler);
        var parts = new List<LanguageModelStreamPart>();
        await foreach (var part in provider.LanguageModel("m").DoStreamAsync(Prompt(), CancellationToken.None))
        {
            parts.Add(part);
        }

        Assert.Contains(parts, part => part is TextDeltaStreamPart delta && delta.Delta == "Hi");
        Assert.Contains("/chat/completions", handler.Uri);
    }

    private static LanguageModelCallOptions Prompt()
    {
        return new LanguageModelCallOptions { Prompt = new ModelMessage[] { new UserModelMessage("hi") } };
    }

    private sealed class SseHandler : HttpMessageHandler
    {
        private readonly string _body;

        public SseHandler(string body)
        {
            _body = body;
        }

        public string Uri { get; private set; } = string.Empty;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Uri = request.RequestUri?.AbsoluteUri ?? string.Empty;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_body, Encoding.UTF8, "text/event-stream"),
            });
        }
    }
}
