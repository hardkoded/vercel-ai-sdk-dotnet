// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Anthropic;
using Vercel.AI.Gateway;
using Vercel.AI.HuggingFace;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Xai;

namespace Vercel.AI.Tests;

/// <summary>Regression tests for disposing responses, requests, and forms, and for escaping multipart names.</summary>
public sealed class ResourceCleanupTests
{
    private const string HostileName = "a\"b\r\nX-Injected: 1.txt";

    [Fact]
    public async Task AnthropicFiles_escapes_quotes_and_line_breaks_in_the_file_name()
    {
        var handler = new CaptureHandler { ResponseBody = "{\"id\":\"file_1\"}" };
        var provider = AnthropicProvider.Create(new AnthropicOptions { ApiKey = "k", BaseUrl = "https://api.anthropic.com/v1" }, handler);

        await provider.Files().UploadAsync(HostileName, new byte[] { 1 }, "text/plain", null, CancellationToken.None);

        Assert.Contains("filename=\"a%22b%0D%0AX-Injected: 1.txt\"", handler.Body);
        Assert.DoesNotContain("\r\nX-Injected", handler.Body);
    }

    [Fact]
    public async Task AnthropicSkills_escape_quotes_and_line_breaks_in_the_file_path()
    {
        var handler = new CaptureHandler { ResponseBody = "{\"id\":\"skill_1\",\"latest_version\":\"v1\"}" };
        var provider = AnthropicProvider.Create(new AnthropicOptions { ApiKey = "k", BaseUrl = "https://api.anthropic.com/v1" }, handler);

        await provider.Skills().UploadSkillAsync(new[] { new AnthropicSkillFile(HostileName, new byte[] { 1 }) }, null, CancellationToken.None);

        Assert.Contains("filename=\"a%22b%0D%0AX-Injected: 1.txt\"", handler.Body);
        Assert.DoesNotContain("\r\nX-Injected", handler.Body);
    }

    [Fact]
    public async Task XaiFiles_escape_quotes_and_line_breaks_in_the_file_name()
    {
        var handler = new ParityHandler(_ => ParityHandler.Json("{\"id\":\"file-1\",\"object\":\"file\",\"bytes\":1,\"created_at\":1,\"filename\":\"x\"}"));
        var files = new XaiFiles(new HttpClient(handler, disposeHandler: false), "https://api.x.ai/v1", () => new Dictionary<string, string?> { ["Authorization"] = "Bearer k" });

        await files.UploadAsync(new XaiFileUpload { Bytes = new byte[] { 1 }, MediaType = "text/plain", FileName = HostileName }, CancellationToken.None);

        var body = Encoding.UTF8.GetString(handler.Calls[0].Body);
        Assert.Contains("filename=\"a%22b%0D%0AX-Injected: 1.txt\"", body);
        Assert.DoesNotContain("\r\nX-Injected", body);
    }

    [Fact]
    public async Task ReleasingAsyncEnumerable_releases_the_resource_when_disposed_before_the_first_read()
    {
        var resource = new Resource();
        var sequence = new ReleasingAsyncEnumerable<int>(Numbers(), resource);

        await using (sequence.GetAsyncEnumerator())
        {
        }

        Assert.True(resource.Disposed);
    }

    [Fact]
    public async Task OpenAICompatible_chat_stream_releases_the_response_when_the_enumerator_is_disposed_unread()
    {
        var content = new TrackingContent("data: [DONE]\n\n");
        var provider = new OpenAICompatibleProvider(
            new OpenAICompatibleOptions { ApiKey = "k", ProviderName = "p", BaseUrl = "https://example.test/v1" },
            new HttpClient(new DelegateHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = content })));
        var model = new OpenAICompatibleLanguageModel(provider, "m");

        var result = await model.DoStreamAsync(Call(), CancellationToken.None);
        await using (result.GetAsyncEnumerator())
        {
        }

        Assert.True(content.Disposed);
    }

    [Fact]
    public async Task OpenAICompatible_completion_stream_releases_the_response_when_the_enumerator_is_disposed_unread()
    {
        var content = new TrackingContent("data: [DONE]\n\n");
        var provider = new OpenAICompatibleProvider(
            new OpenAICompatibleOptions { ApiKey = "k", ProviderName = "p", BaseUrl = "https://example.test/v1" },
            new HttpClient(new DelegateHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = content })));
        var model = new OpenAICompatibleCompletionLanguageModel(provider, "m");

        var result = await model.DoStreamAsync(Call(), CancellationToken.None);
        await using (result.GetAsyncEnumerator())
        {
        }

        Assert.True(content.Disposed);
    }

    [Fact]
    public async Task Bedrock_generate_disposes_the_response_when_the_provider_returns_an_error()
    {
        var content = new TrackingContent("{\"message\":\"nope\"}");
        var model = BedrockModel(new DelegateHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = content }));

        await Assert.ThrowsAnyAsync<Exception>(() => model.DoGenerateAsync(Call(), CancellationToken.None));

        Assert.True(content.Disposed);
    }

    [Fact]
    public async Task Bedrock_generate_disposes_the_response_when_the_body_is_not_json()
    {
        var content = new TrackingContent("not json");
        var model = BedrockModel(new DelegateHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = content }));

        await Assert.ThrowsAnyAsync<Exception>(() => model.DoGenerateAsync(Call(), CancellationToken.None));

        Assert.True(content.Disposed);
    }

    [Fact]
    public async Task Bedrock_stream_disposes_the_response_when_the_provider_returns_an_error()
    {
        var content = new TrackingContent("{\"message\":\"nope\"}");
        var model = BedrockModel(new DelegateHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = content }));

        await Assert.ThrowsAnyAsync<Exception>(() => model.DoStreamAsync(Call(), CancellationToken.None));

        Assert.True(content.Disposed);
    }

    [Fact]
    public async Task Bedrock_embedding_disposes_the_response_when_the_provider_returns_an_error()
    {
        var content = new TrackingContent("{\"message\":\"nope\"}");
        var provider = BedrockProvider(new DelegateHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = content }));
        var model = provider.EmbeddingModel("amazon.titan-embed-text-v2:0");

        await Assert.ThrowsAnyAsync<Exception>(() => model.DoEmbedAsync(new[] { "hi" }, null, CancellationToken.None));

        Assert.True(content.Disposed);
    }

    [Fact]
    public async Task OpenAICompatible_chat_stream_releases_the_response_when_the_result_is_disposed_unenumerated()
    {
        var content = new TrackingContent("data: [DONE]\n\n");
        var provider = new OpenAICompatibleProvider(
            new OpenAICompatibleOptions { ApiKey = "k", ProviderName = "p", BaseUrl = "https://example.test/v1" },
            new HttpClient(new DelegateHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = content })));

        var result = await new OpenAICompatibleLanguageModel(provider, "m").DoStreamAsync(Call(), CancellationToken.None);
        Assert.False(content.Disposed);
        await result.DisposeAsync();

        Assert.True(content.Disposed);
    }

    [Fact]
    public async Task OpenAICompatible_completion_stream_releases_the_response_when_the_result_is_disposed_unenumerated()
    {
        var content = new TrackingContent("data: [DONE]\n\n");
        var provider = new OpenAICompatibleProvider(
            new OpenAICompatibleOptions { ApiKey = "k", ProviderName = "p", BaseUrl = "https://example.test/v1" },
            new HttpClient(new DelegateHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = content })));

        var result = await new OpenAICompatibleCompletionLanguageModel(provider, "m").DoStreamAsync(Call(), CancellationToken.None);
        result.Dispose();

        Assert.True(content.Disposed);
    }

    [Fact]
    public async Task HuggingFace_responses_stream_releases_the_response_when_the_result_is_disposed_unenumerated()
    {
        var content = new TrackingContent("data: [DONE]\n\n");
        var provider = new HuggingFaceProvider(
            new HttpClient(new DelegateHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = content })),
            new OpenAICompatibleOptions { ApiKey = "k" });

        var result = await provider.ResponsesModel("m").DoStreamAsync(Call(), CancellationToken.None);
        await result.DisposeAsync();

        Assert.True(content.Disposed);
    }

    [Fact]
    public async Task Gateway_stream_releases_the_response_when_the_result_is_disposed_unenumerated()
    {
        var content = new TrackingContent("data: {}\n\n");
        var handler = new DelegateHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        var provider = GatewayProvider.Create(new GatewayOptions { ApiKey = "key", BaseUrl = "https://api.test.com" }, handler);

        var result = await provider.LanguageModel("m").DoStreamAsync(Call(), CancellationToken.None);
        await result.DisposeAsync();

        Assert.True(content.Disposed);
    }

    [Fact]
    public async Task Bedrock_converse_stream_releases_the_response_when_the_result_is_disposed_unenumerated()
    {
        var content = new TrackingContent(string.Empty);
        var model = BedrockModel(new DelegateHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = content }));

        var result = await model.DoStreamAsync(Call(), CancellationToken.None);
        await result.DisposeAsync();

        Assert.True(content.Disposed);
    }

    [Fact]
    public async Task LanguageModelStreamResult_disposes_its_resource_once()
    {
        var resource = new CountingResource();
        var result = new LanguageModelStreamResult(Parts(), resource: resource);

        await result.DisposeAsync();
        result.Dispose();

        Assert.Equal(1, resource.Count);
    }

    [Fact]
    public async Task LanguageModelStreamResult_dispose_after_full_enumeration_keeps_the_stream_complete()
    {
        var content = new TrackingContent("data: [DONE]\n\n");
        var provider = new OpenAICompatibleProvider(
            new OpenAICompatibleOptions { ApiKey = "k", ProviderName = "p", BaseUrl = "https://example.test/v1" },
            new HttpClient(new DelegateHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = content })));
        var result = await new OpenAICompatibleLanguageModel(provider, "m").DoStreamAsync(Call(), CancellationToken.None);

        await foreach (var _ in result)
        {
        }

        await result.DisposeAsync();
        await result.DisposeAsync();

        Assert.True(content.Disposed);
    }

    [Fact]
    public async Task StreamText_disposes_the_stream_result_when_the_stream_ends()
    {
        var resource = new CountingResource();
        var model = new ResourceModel(resource);

        var result = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })).StreamTextAsync(new StreamTextOptions { Model = model, Prompt = "hi" });
        await result.Text;

        Assert.Equal(1, resource.Count);
    }

    private sealed class ResourceModel(CountingResource resource) : ILanguageModel
    {
        public string SpecificationVersion => "V4";

        public string Provider => "test";

        public string ModelId => "m";

        public Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<LanguageModelStreamResult> DoStreamAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
        {
            return Task.FromResult(new LanguageModelStreamResult(Parts(), resource: resource));
        }
    }

    private static async IAsyncEnumerable<LanguageModelStreamPart> Parts()
    {
        await Task.CompletedTask;
        yield return new TextDeltaStreamPart("t", "ok");
        yield return new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(1, 1, 2), "stop");
    }

    private sealed class CountingResource : IDisposable
    {
        public int Count { get; private set; }

        public void Dispose()
        {
            Count++;
        }
    }

    private static AmazonBedrockLanguageModel BedrockModel(HttpMessageHandler handler)
    {
        return (AmazonBedrockLanguageModel)BedrockProvider(handler).LanguageModel("anthropic.claude-3-haiku");
    }

    private static AmazonBedrockProvider BedrockProvider(HttpMessageHandler handler)
    {
        return AmazonBedrockProvider.Create(new AmazonBedrockOptions
        {
            AccessKeyId = "AKIA",
            SecretAccessKey = "secret",
            UtcNow = () => new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
        }, handler);
    }

    private static LanguageModelCallOptions Call()
    {
        return new LanguageModelCallOptions { Prompt = new ModelMessage[] { new UserModelMessage("hi") } };
    }

    private static async IAsyncEnumerable<int> Numbers()
    {
        yield return 1;
        await Task.CompletedTask;
    }

    private sealed class Resource : IDisposable
    {
        public bool Disposed { get; private set; }

        public void Dispose()
        {
            Disposed = true;
        }
    }

    private sealed class TrackingContent(string body) : HttpContent
    {
        public bool Disposed { get; private set; }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            var bytes = Encoding.UTF8.GetBytes(body);
            return stream.WriteAsync(bytes, 0, bytes.Length);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
        }
    }

    private sealed class DelegateHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(respond(request));
        }
    }
}
