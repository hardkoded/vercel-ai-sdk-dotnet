// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Net.Http;
using System.Text.Json;
using Vercel.AI.Anthropic;
using Vercel.AI.AssemblyAI;
using Vercel.AI.Fal;
using Vercel.AI.GenerateText;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.RevAI;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Util;
using DownloadException = Vercel.AI.GenerateText.DownloadException;

namespace Vercel.AI.Tests;

/// <summary>URLs and ids that come from response text are validated, escaped, and read with a size limit.</summary>
public sealed class UntrustedUrlTests
{
    private const string Origin = "https://api.example.com";

    private static readonly Dictionary<string, string?> Credentials = new() { ["x-api-key"] = "secret" };

    [Fact]
    public async Task A_response_url_on_the_trusted_origin_gets_the_credentials()
    {
        var handler = new ParityHandler(_ => ParityHandler.Bytes(new byte[] { 1, 2 }, "image/png"));
        using var client = new HttpClient(handler);
        var result = await ProviderExchange.GetResponseUrlAsync(client, Origin + "/out.png", Origin, DownloadUrls.ValidateDownloadUrl, Credentials, CancellationToken.None);
        Assert.Equal(new byte[] { 1, 2 }, result.Bytes);
        Assert.Equal("secret", handler.Calls[0].Header("x-api-key"));
    }

    [Fact]
    public async Task A_response_url_on_another_origin_gets_no_credentials()
    {
        var handler = new ParityHandler(_ => ParityHandler.Bytes(new byte[] { 1 }, "image/png"));
        using var client = new HttpClient(handler);
        await ProviderExchange.GetResponseUrlAsync(client, "https://cdn.example.com/out.png", Origin, DownloadUrls.ValidateDownloadUrl, Credentials, CancellationToken.None);
        Assert.Null(handler.Calls[0].Header("x-api-key"));
    }

    [Theory]
    [InlineData("http://[::1]:8080/admin")]
    [InlineData("http://[fd00::1]/meta")]
    [InlineData("http://169.254.169.254/latest/meta-data/")]
    public async Task A_private_response_url_is_rejected_before_any_request(string url)
    {
        var handler = new ParityHandler();
        using var client = new HttpClient(handler);
        await Assert.ThrowsAsync<DownloadException>(() => ProviderExchange.GetResponseUrlAsync(client, url, Origin, DownloadUrls.ValidateDownloadUrl, Credentials, CancellationToken.None));
        Assert.Empty(handler.Calls);
    }

    [Fact]
    public async Task A_redirect_to_a_private_address_is_rejected()
    {
        var handler = new ParityHandler(call =>
        {
            var response = ParityHandler.Bytes(new byte[] { 1 }, "image/png");
            response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://169.254.169.254/latest/meta-data/");
            return response;
        });
        using var client = new HttpClient(handler);
        await Assert.ThrowsAsync<DownloadError>(() => ProviderExchange.GetResponseUrlAsync(client, "https://cdn.example.com/out.png", null, Download.ValidateDownloadUrl, null, CancellationToken.None));
    }

    private static HttpResponseMessage Redirect(string location)
    {
        var response = new HttpResponseMessage(HttpStatusCode.Found);
        response.Headers.Location = new Uri(location);
        return response;
    }

    [Fact]
    public async Task A_redirect_to_a_private_address_is_never_requested()
    {
        var handler = new ParityHandler(_ => Redirect("http://169.254.169.254/latest/meta-data/"));
        using var client = new HttpClient(handler);
        await Assert.ThrowsAsync<DownloadException>(() => ProviderExchange.GetResponseUrlAsync(client, "https://cdn.example.com/out.png", Origin, DownloadUrls.ValidateDownloadUrl, Credentials, CancellationToken.None));
        Assert.Single(handler.Calls);
    }

    [Fact]
    public async Task A_redirect_off_the_trusted_origin_is_followed_without_the_credentials()
    {
        var handler = new ParityHandler(call => call.Uri.Host == "api.example.com" ? Redirect("https://cdn.example.com/out.png") : ParityHandler.Bytes(new byte[] { 7 }, "image/png"));
        using var client = new HttpClient(handler);
        var result = await ProviderExchange.GetResponseUrlAsync(client, Origin + "/out.png", Origin, DownloadUrls.ValidateDownloadUrl, Credentials, CancellationToken.None);
        Assert.Equal(new byte[] { 7 }, result.Bytes);
        Assert.Equal("secret", handler.Calls[0].Header("x-api-key"));
        Assert.Null(handler.Calls[1].Header("x-api-key"));
    }

    [Fact]
    public async Task Redirects_are_capped()
    {
        var handler = new ParityHandler(_ => Redirect("https://cdn.example.com/loop"));
        using var client = new HttpClient(handler);
        await Assert.ThrowsAsync<DownloadError>(() => ProviderExchange.GetResponseUrlAsync(client, "https://cdn.example.com/loop", null, DownloadUrls.ValidateDownloadUrl, null, CancellationToken.None));
        Assert.Equal(11, handler.Calls.Count);
    }

    [Fact]
    public async Task A_body_that_declares_more_than_the_size_limit_is_rejected_without_reading_it()
    {
        var handler = new ParityHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new DeclaredLengthContent(Download.DefaultMaxBytes + 1) });
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<DownloadError>(() => ProviderExchange.GetResponseUrlAsync(client, "https://cdn.example.com/huge.bin", null, DownloadUrls.ValidateDownloadUrl, null, CancellationToken.None));
        Assert.Contains("exceeded maximum size", error.Message);
    }

    [Fact]
    public async Task A_failed_status_maps_to_a_provider_error()
    {
        var handler = new ParityHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("{\"error\":{\"message\":\"missing\"}}") });
        using var client = new HttpClient(handler);
        await Assert.ThrowsAsync<NotFoundException>(() => ProviderExchange.GetResponseUrlAsync(client, Origin + "/gone", Origin, DownloadUrls.ValidateDownloadUrl, null, CancellationToken.None));
    }

    [Fact]
    public async Task Fal_does_not_download_a_generated_file_from_a_private_ipv6_address()
    {
        var handler = new ParityHandler(_ => ParityHandler.Json("{\"images\":[{\"url\":\"http://[::1]:8080/image.png\"}]}"));
        var model = new FalImageModel(FalProvider.Create(new OpenAICompatibleOptions { ApiKey = "key", BaseUrl = Origin }, handler), "fal-ai/flux/dev");
        await Assert.ThrowsAsync<DownloadException>(() => model.DoGenerateAsync(new ImageModelCall("a cat", null, null, 1, null, null, null, JsonSerializer.SerializeToElement(new Dictionary<string, string>()), new Dictionary<string, string>(), CancellationToken.None), CancellationToken.None));
        Assert.Single(handler.Calls);
    }

    [Fact]
    public async Task Data_urls_obey_the_size_limit()
    {
        var error = await Assert.ThrowsAsync<DownloadError>(() => Download.GetAsync(new Uri("data:text/plain;base64,AAAAAAAA"), 3));
        Assert.Contains("exceeded maximum size of 3 bytes", error.Message);
        Assert.Equal(6, (await Download.GetAsync(new Uri("data:text/plain;base64,AAAAAAAA"), 6)).Data.Length);
    }

    [Fact]
    public async Task The_fetch_gets_the_size_limit_so_it_can_stop_reading()
    {
        long? seen = null;
        await Download.GetAsync(new Uri("https://example.com/a"), 5, default, (url, request) =>
        {
            seen = request.MaxBytes;
            return Task.FromResult(DownloadResponse.Bytes(new byte[] { 1 }, "image/png"));
        });
        Assert.Equal(5, seen);
    }

    [Fact]
    public async Task An_anthropic_results_url_on_the_provider_origin_gets_the_api_key()
    {
        var handler = BatchHandler("https://api.anthropic.com/v1/messages/batches/msgbatch_1/results");
        await Results(handler);
        Assert.Equal("test-api-key", handler.Calls[1].Header("x-api-key"));
    }

    [Fact]
    public async Task An_anthropic_results_url_on_another_origin_gets_no_api_key()
    {
        var handler = BatchHandler("https://results.example.com/msgbatch_1");
        await Results(handler);
        Assert.Equal("https://results.example.com/msgbatch_1", handler.Calls[1].Uri.AbsoluteUri);
        Assert.Null(handler.Calls[1].Header("x-api-key"));
        Assert.Null(handler.Calls[1].Header("authorization"));
    }

    [Theory]
    [InlineData("http://[::1]:8080/results")]
    [InlineData("http://169.254.169.254/results")]
    [InlineData("file:///etc/passwd")]
    public async Task An_anthropic_results_url_that_is_not_safe_is_rejected_before_any_request(string resultsUrl)
    {
        var handler = BatchHandler(resultsUrl);
        await Assert.ThrowsAsync<DownloadException>(() => Results(handler));
        Assert.Single(handler.Calls);
    }

    [Fact]
    public async Task The_fal_transcription_request_id_is_escaped_in_the_poll_url()
    {
        var handler = new ParityHandler(call => call.Method == HttpMethod.Post
            ? ParityHandler.Json("{\"request_id\":\"a/b?c\"}")
            : ParityHandler.Json("{\"text\":\"hi\",\"chunks\":[]}"));
        var model = (FalTranscriptionModel)FalProvider.Create(new OpenAICompatibleOptions { ApiKey = "key" }, handler).TranscriptionModel("wizper");
        await model.DoGenerateAsync(new TranscriptionModelCall(new byte[] { 1 }, "audio/wav", JsonSerializer.SerializeToElement(new Dictionary<string, string>()), new Dictionary<string, string>(), CancellationToken.None), CancellationToken.None);
        Assert.Equal("https://queue.fal.run/fal-ai/wizper/requests/a%2Fb%3Fc", handler.Calls[1].Uri.AbsoluteUri);
    }

    [Fact]
    public async Task The_assemblyai_transcript_id_is_escaped_in_the_poll_url()
    {
        var handler = new ParityHandler(call =>
        {
            if (call.Uri.AbsolutePath == "/v2/upload")
            {
                return ParityHandler.Json("{\"upload_url\":\"https://storage.assemblyai.com/x\"}");
            }

            return call.Method == HttpMethod.Post
                ? ParityHandler.Json("{\"id\":\"a/b?c\",\"status\":\"queued\"}")
                : ParityHandler.Json("{\"id\":\"a/b?c\",\"status\":\"completed\",\"text\":\"hi\"}");
        });
        var provider = AssemblyAIProvider.Create(new AssemblyAIOptions { ApiKey = "key", PollDelay = _ => Task.CompletedTask }, handler);
        await provider.Transcription("best").TranscribeAsync(new AudioInput(new byte[] { 1 }, "audio/wav", "a.wav"), null, CancellationToken.None);
        Assert.Equal("https://api.assemblyai.com/v2/transcript/a%2Fb%3Fc", handler.Calls[2].Uri.AbsoluteUri);
    }

    [Fact]
    public async Task The_revai_job_id_is_escaped_in_the_poll_and_transcript_urls()
    {
        var handler = new ParityHandler(call =>
        {
            if (call.Uri.AbsolutePath.EndsWith("/transcript", StringComparison.Ordinal))
            {
                return ParityHandler.Json("{\"monologues\":[]}");
            }

            return ParityHandler.Json(call.Method == HttpMethod.Get
                ? "{\"id\":\"a/b?c\",\"status\":\"transcribed\"}"
                : "{\"id\":\"a/b?c\",\"status\":\"in_progress\"}");
        });
        var provider = RevAIProvider.Create(new RevAIOptions { ApiKey = "key", PollDelay = _ => Task.CompletedTask }, handler);
        await ((RevAIProvider.RevAITranscriptionModel)provider.TranscriptionModel("machine")).TranscribeAsync(new AudioInput(new byte[] { 1 }, "audio/wav", "a.wav"), null, CancellationToken.None);
        Assert.Contains(handler.Calls, call => call.Uri.AbsoluteUri.EndsWith("/speechtotext/v1/jobs/a%2Fb%3Fc", StringComparison.Ordinal));
        Assert.Contains(handler.Calls, call => call.Uri.AbsoluteUri.EndsWith("/speechtotext/v1/jobs/a%2Fb%3Fc/transcript", StringComparison.Ordinal));
    }

    private static ParityHandler BatchHandler(string resultsUrl)
    {
        const string line = "{\"custom_id\":\"france\",\"result\":{\"type\":\"canceled\"}}\n";
        return new ParityHandler(call => call.Uri.AbsolutePath.EndsWith("/msgbatch_1", StringComparison.Ordinal)
            ? ParityHandler.Json("{\"id\":\"msgbatch_1\",\"type\":\"message_batch\",\"processing_status\":\"ended\",\"request_counts\":{\"processing\":0,\"succeeded\":0,\"errored\":0,\"canceled\":1,\"expired\":0},\"created_at\":\"2026-01-01T00:00:00Z\",\"expires_at\":\"2026-01-02T00:00:00Z\",\"ended_at\":\"2026-01-01T01:00:00Z\",\"archived_at\":null,\"cancel_initiated_at\":null,\"results_url\":\"" + resultsUrl + "\"}")
            : ParityHandler.Bytes(System.Text.Encoding.UTF8.GetBytes(line), "application/x-jsonl"));
    }

    private static async Task Results(ParityHandler handler)
    {
        var provider = AnthropicProvider.Create(new AnthropicOptions { ApiKey = "test-api-key" }, handler);
        var stream = await provider.ExperimentalBatch()!.DoGetResultsAsync(new BatchOperationCall("msgbatch_1", null, new Dictionary<string, string>(), CancellationToken.None), CancellationToken.None);
        await stream.ToListAsync();
    }

    private sealed class DeclaredLengthContent : HttpContent
    {
        private readonly long _length;

        public DeclaredLengthContent(long length)
        {
            _length = length;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            return Task.CompletedTask;
        }

        protected override bool TryComputeLength(out long length)
        {
            length = _length;
            return true;
        }
    }
}
