// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Perplexity;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class PerplexityChatLanguageModelTests
{
    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/convert-perplexity-usage.test.ts::convertPerplexityUsage::treats reasoning tokens as separate from completion tokens",
        Coverage = UpstreamCoverage.Partial,
        Note = "Output total is completion plus reasoning, and text tokens stay the completion count. Shared usage leaves noCache unset when cache counters are absent.")]
    public void Adds_reasoning_tokens_to_the_output_total()
    {
        using var document = JsonDocument.Parse("{\"prompt_tokens\":33,\"completion_tokens\":11395,\"reasoning_tokens\":193947}");
        var usage = PerplexityChatUsage.Convert(document.RootElement);

        Assert.Equal(33, usage.InputTokens);
        Assert.Null(usage.CacheReadTokens);
        Assert.Null(usage.CacheWriteTokens);
        Assert.Equal(205342, usage.OutputTokens);
        Assert.Equal(11395, usage.TextTokens);
        Assert.Equal(193947, usage.ReasoningTokens);
        Assert.Equal(33, usage.Raw!.Value.GetProperty("prompt_tokens").GetInt32());
        Assert.Equal(11395, usage.Raw.Value.GetProperty("completion_tokens").GetInt32());
        Assert.Equal(193947, usage.Raw.Value.GetProperty("reasoning_tokens").GetInt32());
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/perplexity-language-model.test.ts::doGenerate > text::should extract text content",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_text_and_citations()
    {
        using var fixture = Load("perplexity-text.json");
        var result = await Generate(fixture.RootElement.GetRawText());

        Assert.Equal(fixture.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString(), result.Text);
        AssertUrls(fixture.RootElement.GetProperty("citations"), result.Content.OfType<GeneratedSource>());
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal("stop", result.RawFinishReason);
        AssertUsage(result.Usage, 11, 392);
        Assert.Empty(result.Warnings);
        AssertNullMetadata(result.ProviderMetadata);
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/perplexity-language-model.test.ts::doGenerate > citations::should extract citation content",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_citation_sources()
    {
        using var fixture = Load("perplexity-citations.json");
        var result = await Generate(fixture.RootElement.GetRawText());

        Assert.Equal(fixture.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString(), result.Text);
        AssertUrls(fixture.RootElement.GetProperty("citations"), result.Content.OfType<GeneratedSource>());
        Assert.Equal(7, result.Content.OfType<GeneratedSource>().Count());
        AssertUsage(result.Usage, 10, 251);
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/perplexity-language-model.test.ts::doGenerate::should send correct request body",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_the_chat_request_body()
    {
        var handler = Handler(Fixture("perplexity-text.json"));
        await Model(handler).DoGenerateAsync(Prompt(), CancellationToken.None);
        var body = Body(handler);

        Assert.Equal("https://api.perplexity.ai/chat/completions", handler.Uri);
        Assert.Equal(2, body.EnumerateObject().Count());
        Assert.Equal("sonar", body.GetProperty("model").GetString());
        Assert.Equal("user", body.GetProperty("messages")[0].GetProperty("role").GetString());
        Assert.Equal("Hello", body.GetProperty("messages")[0].GetProperty("content").GetString());
        Assert.False(body.TryGetProperty("stream", out _));
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/perplexity-language-model.test.ts::doGenerate::should pass through perplexity provider options",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_perplexity_provider_options()
    {
        var handler = Handler(Fixture("perplexity-text.json"));
        var options = Prompt();
        options.ProviderOptions = new Dictionary<string, JsonElement>
        {
            ["perplexity"] = Json("{\"search_recency_filter\":\"month\",\"return_images\":true}"),
        };
        await Model(handler).DoGenerateAsync(options, CancellationToken.None);
        var body = Body(handler);

        Assert.Equal("month", body.GetProperty("search_recency_filter").GetString());
        Assert.True(body.GetProperty("return_images").GetBoolean());
        Assert.Equal("sonar", body.GetProperty("model").GetString());
        Assert.Equal("Hello", body.GetProperty("messages")[0].GetProperty("content").GetString());
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/perplexity-language-model.test.ts::doGenerate::should pass through unknown perplexity provider options",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_unknown_provider_options()
    {
        var handler = Handler(Minimal(""));
        var options = Prompt();
        options.ProviderOptions = new Dictionary<string, JsonElement>
        {
            ["perplexity"] = Json("{\"future_option\":{\"enabled\":true}}"),
        };
        await Model(handler).DoGenerateAsync(options, CancellationToken.None);
        var body = Body(handler);

        Assert.True(body.GetProperty("future_option").GetProperty("enabled").GetBoolean());
        Assert.Equal("sonar", body.GetProperty("model").GetString());
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/perplexity-language-model.test.ts::doGenerate::should reject invalid perplexity provider options",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_invalid_provider_options()
    {
        var handler = Handler(Minimal(""));
        var options = Prompt();
        options.ProviderOptions = new Dictionary<string, JsonElement>
        {
            ["perplexity"] = Json("{\"search_recency_filter\":\"decade\"}"),
        };

        await Assert.ThrowsAsync<AiSdkException>(() => Model(handler).DoGenerateAsync(options, CancellationToken.None));

        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/perplexity-language-model.test.ts::doGenerate::should pass headers",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_generate_headers()
    {
        var handler = Handler(Fixture("perplexity-text.json"));
        var provider = PerplexityProvider.Create(new OpenAICompatibleOptions
        {
            ApiKey = "test-api-key",
            Headers = { ["Custom-Provider-Header"] = "provider-header-value" },
        }, handler);
        var options = Prompt();
        options.Headers = new Dictionary<string, string?> { ["Custom-Request-Header"] = "request-header-value" };
        await provider.ChatLanguageModel("sonar").DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("Bearer test-api-key", handler.Headers["Authorization"]);
        Assert.Equal("provider-header-value", handler.Headers["Custom-Provider-Header"]);
        Assert.Equal("request-header-value", handler.Headers["Custom-Request-Header"]);
        Assert.Contains("application/json", handler.Headers["Content-Type"], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/perplexity-language-model.test.ts::doGenerate::should expose the raw response headers",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Exposes_generate_response_headers()
    {
        var handler = Handler(Fixture("perplexity-text.json"));
        handler.ResponseHeaders = new Dictionary<string, string> { ["test-header"] = "test-value" };
        var result = await Model(handler).DoGenerateAsync(Prompt(), CancellationToken.None);

        Assert.Equal("test-value", Header(result.ResponseHeaders, "test-header"));
        Assert.Contains("application/json", Header(result.ResponseHeaders, "content-type"), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/perplexity-language-model.test.ts::doGenerate::should extract usage",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_generate_usage()
    {
        var result = await Generate(Fixture("perplexity-text.json"));

        AssertUsage(result.Usage, 11, 392);
        Assert.Equal(392, result.Usage.Raw!.Value.GetProperty("completion_tokens").GetInt32());
        Assert.Equal(11, result.Usage.Raw.Value.GetProperty("prompt_tokens").GetInt32());
        Assert.Equal(403, result.Usage.Raw.Value.GetProperty("total_tokens").GetInt32());
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/perplexity-language-model.test.ts::doGenerate::should send additional response information",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Reads_response_metadata()
    {
        var result = await Generate(Fixture("perplexity-text.json"));

        Assert.Equal("aec30d94-c6a5-4d30-935e-97dbe8de9f85", result.ResponseId);
        Assert.Equal("sonar", result.ResponseModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1770768220), result.ResponseTimestamp);
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/perplexity-language-model.test.ts::doGenerate::should handle PDF files with base64 encoding",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_inline_pdf_file_urls()
    {
        var handler = Handler(Fixture("perplexity-text.json"));
        await Model(handler).DoGenerateAsync(PdfPrompt(inline: "mock-pdf-data", url: null), CancellationToken.None);
        var content = Body(handler).GetProperty("messages")[0].GetProperty("content");

        Assert.Equal("text", content[0].GetProperty("type").GetString());
        Assert.Equal("Analyze this PDF", content[0].GetProperty("text").GetString());
        Assert.Equal("file_url", content[1].GetProperty("type").GetString());
        Assert.Equal("mock-pdf-data", content[1].GetProperty("file_url").GetProperty("url").GetString());
        Assert.Equal("test.pdf", content[1].GetProperty("file_name").GetString());
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/perplexity-language-model.test.ts::doGenerate::should handle PDF files with URLs",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_remote_pdf_file_urls()
    {
        var handler = Handler(Fixture("perplexity-text.json"));
        await Model(handler).DoGenerateAsync(PdfPrompt(inline: null, url: "https://example.com/test.pdf"), CancellationToken.None);
        var file = Body(handler).GetProperty("messages")[0].GetProperty("content")[1];

        Assert.Equal("file_url", file.GetProperty("type").GetString());
        Assert.Equal("https://example.com/test.pdf", file.GetProperty("file_url").GetProperty("url").GetString());
        Assert.Equal("test.pdf", file.GetProperty("file_name").GetString());
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/perplexity-language-model.test.ts::doGenerate::should extract images",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_images_into_provider_metadata()
    {
        var result = await Generate(Minimal("", images: true));
        var perplexity = result.ProviderMetadata!.Value.GetProperty("perplexity");
        var image = perplexity.GetProperty("images")[0];

        Assert.Equal("https://example.com/image.jpg", image.GetProperty("imageUrl").GetString());
        Assert.Equal("https://example.com/image.jpg", image.GetProperty("originUrl").GetString());
        Assert.Equal(100, image.GetProperty("height").GetDouble());
        Assert.Equal(100, image.GetProperty("width").GetDouble());
        Assert.Equal(JsonValueKind.Null, perplexity.GetProperty("cost").ValueKind);
        Assert.Equal(JsonValueKind.Null, perplexity.GetProperty("usage").GetProperty("citationTokens").ValueKind);
        Assert.Equal(JsonValueKind.Null, perplexity.GetProperty("usage").GetProperty("numSearchQueries").ValueKind);
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/perplexity-language-model.test.ts::doGenerate::should extract extended usage",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_extended_usage_and_cost()
    {
        var result = await Generate(Extended(""));
        var perplexity = result.ProviderMetadata!.Value.GetProperty("perplexity");
        var cost = perplexity.GetProperty("cost");

        AssertUsage(result.Usage, 10, 20, 50);
        Assert.Equal(30, result.Usage.Raw!.Value.GetProperty("citation_tokens").GetInt32());
        Assert.Equal(2, result.Usage.Raw.Value.GetProperty("future_usage_field").GetProperty("units").GetInt32());
        Assert.Equal(0.1, cost.GetProperty("inputTokensCost").GetDouble(), 5);
        Assert.Equal(0.2, cost.GetProperty("outputTokensCost").GetDouble(), 5);
        Assert.Equal(0.4, cost.GetProperty("requestCost").GetDouble(), 5);
        Assert.Equal(2.1, cost.GetProperty("totalCost").GetDouble(), 5);
        Assert.Equal(30, perplexity.GetProperty("usage").GetProperty("citationTokens").GetInt32());
        Assert.Equal(40, perplexity.GetProperty("usage").GetProperty("numSearchQueries").GetInt32());
        Assert.Equal(JsonValueKind.Null, perplexity.GetProperty("images").ValueKind);
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/perplexity-language-model.test.ts::doGenerate::should reject invalid $name values",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_invalid_usage_values()
    {
        await Assert.ThrowsAsync<AiSdkException>(() => Generate(Minimal("", usageExtra: ",\"search_context_size\":\"extra-large\"")));
        await Assert.ThrowsAsync<AiSdkException>(() => Generate(Minimal("", usageExtra: ",\"cost\":{\"reasoning_tokens_cost\":\"unknown\"}")));
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/perplexity-language-model.test.ts::doGenerate > warnings::should warn about unsupported reasoning",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Warns_that_reasoning_is_unsupported()
    {
        var handler = Handler(Fixture("perplexity-text.json"));
        var options = Prompt();
        options.Reasoning = "medium";
        var result = await Model(handler).DoGenerateAsync(options, CancellationToken.None);

        Assert.Contains(result.Warnings, warning => warning.Type == "unsupported" && warning.Message == "reasoning");
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/perplexity-language-model.test.ts::doStream > text::should stream text",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_text_deltas_and_citations()
    {
        var parts = await Stream(Sse("perplexity-text.chunks.txt"));
        var lines = Chunks("perplexity-text.chunks.txt");

        Assert.DoesNotContain(parts, part => part is ErrorStreamPart);
        Assert.Equal(
            lines.Select(line => line.GetProperty("choices")[0].GetProperty("delta").GetProperty("content").GetString() ?? string.Empty).ToArray(),
            parts.OfType<TextDeltaStreamPart>().Select(part => part.Delta).ToArray());
        Assert.Equal("0", Assert.Single(parts.OfType<TextStartStreamPart>()).Id);
        AssertUrls(lines[0].GetProperty("citations"), parts.OfType<SourceStreamPart>().Select(ToSource));
        var metadata = Assert.Single(parts.OfType<ResponseMetadataStreamPart>());
        Assert.Equal("a3d55d44-63f9-4704-bb26-e17be1ddab3a", metadata.Id);
        Assert.Equal("sonar", metadata.ModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1770768233), metadata.Timestamp);
        var finish = Assert.Single(parts.OfType<FinishStreamPart>());
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal("stop", finish.RawFinishReason);
        AssertUsage(finish.Usage, 11, 434);
        AssertNullMetadata(finish.ProviderMetadata);
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/perplexity-language-model.test.ts::doStream > citations::should stream citations",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_citation_sources_once()
    {
        var parts = await Stream(Sse("perplexity-citations.chunks.txt"));
        var lines = Chunks("perplexity-citations.chunks.txt");

        Assert.Equal(
            lines.Select(line => line.GetProperty("choices")[0].GetProperty("delta").GetProperty("content").GetString() ?? string.Empty).ToArray(),
            parts.OfType<TextDeltaStreamPart>().Select(part => part.Delta).ToArray());
        Assert.Equal(7, parts.OfType<SourceStreamPart>().Count());
        AssertUrls(lines[0].GetProperty("citations"), parts.OfType<SourceStreamPart>().Select(ToSource));
        AssertUsage(Assert.Single(parts.OfType<FinishStreamPart>()).Usage, 10, 336);
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/perplexity-language-model.test.ts::doStream::should send correct streaming request body",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_the_streaming_request_body()
    {
        var handler = Handler(Sse("perplexity-text.chunks.txt"), "text/event-stream");
        await Read(Model(handler).DoStreamAsync(Prompt(), CancellationToken.None));
        var body = Body(handler);

        Assert.Equal("https://api.perplexity.ai/chat/completions", handler.Uri);
        Assert.True(body.GetProperty("stream").GetBoolean());
        Assert.Equal("sonar", body.GetProperty("model").GetString());
        Assert.Equal("Hello", body.GetProperty("messages")[0].GetProperty("content").GetString());
        Assert.False(body.TryGetProperty("stream_options", out _));
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/perplexity-language-model.test.ts::doStream::should pass headers",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_stream_headers()
    {
        var handler = Handler(Sse("perplexity-text.chunks.txt"), "text/event-stream");
        var provider = PerplexityProvider.Create(new OpenAICompatibleOptions
        {
            ApiKey = "test-api-key",
            Headers = { ["Custom-Provider-Header"] = "provider-header-value" },
        }, handler);
        var options = Prompt();
        options.Headers = new Dictionary<string, string?> { ["Custom-Request-Header"] = "request-header-value" };
        await Read(provider.ChatLanguageModel("sonar").DoStreamAsync(options, CancellationToken.None));

        Assert.Equal("Bearer test-api-key", handler.Headers["Authorization"]);
        Assert.Equal("provider-header-value", handler.Headers["Custom-Provider-Header"]);
        Assert.Equal("request-header-value", handler.Headers["Custom-Request-Header"]);
        Assert.Contains("application/json", handler.Headers["Content-Type"], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/perplexity-language-model.test.ts::doStream::should expose the raw response headers",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Exposes_stream_response_headers()
    {
        var handler = Handler(Sse("perplexity-text.chunks.txt"), "text/event-stream");
        handler.ResponseHeaders = new Dictionary<string, string> { ["test-header"] = "test-value" };
        var opened = await Model(handler).OpenStreamAsync(Prompt(), CancellationToken.None);
        await Read(opened.Parts);

        Assert.Equal("test-value", Header(opened.Headers, "test-header"));
        Assert.Contains("text/event-stream", Header(opened.Headers, "content-type"), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/perplexity-language-model.test.ts::doStream::should stream images",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_images_on_the_finish_metadata()
    {
        var sse = SseLines(
            "{\"id\":\"stream-id\",\"created\":1680003600,\"model\":\"sonar\",\"images\":[{\"image_url\":\"https://example.com/image.jpg\",\"origin_url\":\"https://example.com/image.jpg\",\"height\":100,\"width\":100}],\"choices\":[{\"delta\":{\"role\":\"assistant\",\"content\":\"Hello\"},\"finish_reason\":null}]}",
            "{\"id\":\"stream-id\",\"created\":1680003600,\"model\":\"sonar\",\"choices\":[{\"delta\":{\"role\":\"assistant\",\"content\":\"\"},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":10,\"completion_tokens\":20,\"total_tokens\":30}}");
        var finish = Assert.Single((await Stream(sse)).OfType<FinishStreamPart>());
        var image = finish.ProviderMetadata!.Value.GetProperty("perplexity").GetProperty("images")[0];

        Assert.Equal("https://example.com/image.jpg", image.GetProperty("imageUrl").GetString());
        Assert.Equal(100, image.GetProperty("height").GetDouble());
        Assert.Equal(100, image.GetProperty("width").GetDouble());
        Assert.Equal(JsonValueKind.Null, finish.ProviderMetadata.Value.GetProperty("perplexity").GetProperty("cost").ValueKind);
        Assert.Equal(JsonValueKind.Null, finish.ProviderMetadata.Value.GetProperty("perplexity").GetProperty("usage").GetProperty("citationTokens").ValueKind);
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/perplexity-language-model.test.ts::doStream::should stream extended usage",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_the_terminal_usage()
    {
        var first = "{\"id\":\"stream-id\",\"created\":1680003600,\"model\":\"sonar\",\"choices\":[{\"delta\":{\"role\":\"assistant\",\"content\":\"Hello\"},\"finish_reason\":null}],\"usage\":{\"prompt_tokens\":10,\"completion_tokens\":20,\"total_tokens\":30,\"first_chunk_only\":true}}";
        var terminal = "{\"prompt_tokens\":11,\"completion_tokens\":21,\"total_tokens\":32,\"search_context_size\":\"high\",\"citation_tokens\":30,\"num_search_queries\":40,\"reasoning_tokens\":50,\"cost\":{\"input_tokens_cost\":0.1,\"output_tokens_cost\":0.2,\"reasoning_tokens_cost\":0.3,\"request_cost\":0.4,\"citation_tokens_cost\":0.5,\"search_queries_cost\":0.6,\"total_cost\":2.1,\"future_cost_field\":{\"currency\":\"USD\"}},\"future_usage_field\":{\"units\":2}}";
        var second = "{\"id\":\"stream-id\",\"created\":1680003600,\"model\":\"sonar\",\"choices\":[{\"delta\":{\"role\":\"assistant\",\"content\":\"\"},\"finish_reason\":\"stop\"}],\"usage\":" + terminal + "}";
        var finish = Assert.Single((await Stream(SseLines(first, second))).OfType<FinishStreamPart>());
        var perplexity = finish.ProviderMetadata!.Value.GetProperty("perplexity");

        AssertUsage(finish.Usage, 11, 21, 50);
        Assert.Equal(11, finish.Usage.Raw!.Value.GetProperty("prompt_tokens").GetInt32());
        Assert.Equal(2, finish.Usage.Raw.Value.GetProperty("future_usage_field").GetProperty("units").GetInt32());
        Assert.Equal(0.1, perplexity.GetProperty("cost").GetProperty("inputTokensCost").GetDouble(), 5);
        Assert.Equal(2.1, perplexity.GetProperty("cost").GetProperty("totalCost").GetDouble(), 5);
        Assert.Equal(30, perplexity.GetProperty("usage").GetProperty("citationTokens").GetInt32());
        Assert.Equal(40, perplexity.GetProperty("usage").GetProperty("numSearchQueries").GetInt32());
        Assert.Equal(JsonValueKind.Null, perplexity.GetProperty("images").ValueKind);
    }

    [Fact]
    [UpstreamTest(
        "packages/perplexity/src/perplexity-language-model.test.ts::doStream::should stream raw chunks",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_raw_chunks_text_and_a_citation()
    {
        var first = "{\"id\":\"ppl-123\",\"object\":\"chat.completion.chunk\",\"created\":1234567890,\"model\":\"sonar\",\"choices\":[{\"index\":0,\"delta\":{\"role\":\"assistant\",\"content\":\"Hello\"},\"finish_reason\":null}],\"citations\":[\"https://example.com\"]}";
        var second = "{\"id\":\"ppl-456\",\"object\":\"chat.completion.chunk\",\"created\":1234567890,\"model\":\"sonar\",\"choices\":[{\"index\":0,\"delta\":{\"content\":\" world\"},\"finish_reason\":null}]}";
        var third = "{\"id\":\"ppl-789\",\"object\":\"chat.completion.chunk\",\"created\":1234567890,\"model\":\"sonar\",\"choices\":[{\"index\":0,\"delta\":{},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":10,\"completion_tokens\":5,\"total_tokens\":15,\"citation_tokens\":2,\"num_search_queries\":1}}";
        var options = Prompt();
        options.IncludeRawChunks = true;
        var parts = await Stream(SseLines(first, second, third), options);

        Assert.Equal(new[] { "stream-start", "raw", "response-metadata", "source", "text-start", "text-delta", "raw", "text-delta", "raw", "text-end", "finish" }, parts.Select(part => part.Type));
        Assert.Equal(new[] { first, second, third }, parts.OfType<RawStreamPart>().Select(part => part.RawJson));
        var metadata = Assert.Single(parts.OfType<ResponseMetadataStreamPart>());
        Assert.Equal("ppl-123", metadata.Id);
        Assert.Equal("sonar", metadata.ModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1234567890), metadata.Timestamp);
        Assert.Equal("https://example.com", Assert.Single(parts.OfType<SourceStreamPart>()).Url);
        Assert.Equal(new[] { "Hello", " world" }, parts.OfType<TextDeltaStreamPart>().Select(part => part.Delta));
        Assert.All(parts.OfType<TextDeltaStreamPart>(), part => Assert.Equal("0", part.Id));
        var finish = Assert.Single(parts.OfType<FinishStreamPart>());
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal("stop", finish.RawFinishReason);
        AssertUsage(finish.Usage, 10, 5);
        Assert.Equal(2, finish.ProviderMetadata!.Value.GetProperty("perplexity").GetProperty("usage").GetProperty("citationTokens").GetInt32());
        Assert.Equal(1, finish.ProviderMetadata.Value.GetProperty("perplexity").GetProperty("usage").GetProperty("numSearchQueries").GetInt32());
        Assert.Equal(JsonValueKind.Null, finish.ProviderMetadata.Value.GetProperty("perplexity").GetProperty("cost").ValueKind);
        Assert.Equal(JsonValueKind.Null, finish.ProviderMetadata.Value.GetProperty("perplexity").GetProperty("images").ValueKind);
    }

    private static async Task<LanguageModelGenerateResult> Generate(string json)
    {
        return await Model(Handler(json)).DoGenerateAsync(Prompt(), CancellationToken.None);
    }

    private static async Task<List<LanguageModelStreamPart>> Stream(string sse, LanguageModelCallOptions? options = null)
    {
        return await Read(Model(Handler(sse, "text/event-stream")).DoStreamAsync(options ?? Prompt(), CancellationToken.None));
    }

    private static PerplexityChatLanguageModel Model(PerplexityScriptedHandler handler)
    {
        return PerplexityProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-token" }, handler).ChatLanguageModel("sonar");
    }

    private static PerplexityScriptedHandler Handler(string body, string mediaType = "application/json")
    {
        return new PerplexityScriptedHandler
        {
            ResponseBody = body,
            MediaType = mediaType,
        };
    }

    private static LanguageModelCallOptions Prompt()
    {
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
        };
    }

    private static LanguageModelCallOptions PdfPrompt(string? inline, string? url)
    {
        UserContentPart file = inline != null
            ? new PerplexityFilePart("application/pdf", null, null, inline, "test.pdf")
            : new FileContentPart("application/pdf", url, null, "test.pdf");
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage(new UserContentPart[]
                {
                    new TextContentPart("Analyze this PDF"),
                    file,
                }),
            },
        };
    }

    private static string Fixture(string name)
    {
        return File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "perplexity-sonar", name));
    }

    private static JsonDocument Load(string name)
    {
        return JsonDocument.Parse(Fixture(name));
    }

    private static string Sse(string name)
    {
        return SseLines(File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "Fixtures", "perplexity-sonar", name)).Where(line => line.Trim().Length > 0).ToArray());
    }

    private static string SseLines(params string[] lines)
    {
        var builder = new StringBuilder();
        foreach (var line in lines)
        {
            builder.Append("data: ").Append(line).Append("\n\n");
        }

        builder.Append("data: [DONE]\n\n");
        return builder.ToString();
    }

    private static List<JsonElement> Chunks(string name)
    {
        return File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "Fixtures", "perplexity-sonar", name))
            .Where(line => line.Trim().Length > 0)
            .Select(line => JsonDocument.Parse(line).RootElement.Clone())
            .ToList();
    }

    private static string Minimal(string content, bool images = false, string usageExtra = "")
    {
        var imageJson = images
            ? ",\"images\":[{\"image_url\":\"https://example.com/image.jpg\",\"origin_url\":\"https://example.com/image.jpg\",\"height\":100,\"width\":100}]"
            : string.Empty;
        return "{\"id\":\"test-id\",\"created\":1680000000,\"model\":\"sonar\",\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"" + content + "\"},\"finish_reason\":\"stop\"}]" + imageJson + ",\"usage\":{\"prompt_tokens\":10,\"completion_tokens\":20,\"total_tokens\":30" + usageExtra + "}}";
    }

    private static string Extended(string content)
    {
        var usage = "{\"prompt_tokens\":10,\"completion_tokens\":20,\"total_tokens\":30,\"search_context_size\":\"medium\",\"citation_tokens\":30,\"num_search_queries\":40,\"reasoning_tokens\":50,\"cost\":{\"input_tokens_cost\":0.1,\"output_tokens_cost\":0.2,\"reasoning_tokens_cost\":0.3,\"request_cost\":0.4,\"citation_tokens_cost\":0.5,\"search_queries_cost\":0.6,\"total_cost\":2.1,\"future_cost_field\":{\"currency\":\"USD\"}},\"future_usage_field\":{\"units\":2}}";
        return "{\"id\":\"test-id\",\"created\":1680000000,\"model\":\"sonar\",\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"" + content + "\"},\"finish_reason\":\"stop\"}],\"usage\":" + usage + "}";
    }

    private static JsonElement Body(PerplexityScriptedHandler handler)
    {
        using var document = JsonDocument.Parse(handler.Body);
        return document.RootElement.Clone();
    }

    private static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private static async Task<List<LanguageModelStreamPart>> Read(IAsyncEnumerable<LanguageModelStreamPart> stream)
    {
        var parts = new List<LanguageModelStreamPart>();
        await foreach (var part in stream)
        {
            parts.Add(part);
        }

        return parts;
    }

    private static void AssertUsage(LanguageModelUsage usage, int prompt, int completion, int reasoning = 0)
    {
        Assert.Equal(prompt, usage.InputTokens);
        Assert.Null(usage.CacheReadTokens);
        Assert.Null(usage.CacheWriteTokens);
        Assert.Equal(completion + reasoning, usage.OutputTokens);
        Assert.Equal(completion, usage.TextTokens);
        Assert.Equal(reasoning, usage.ReasoningTokens);
    }

    private static void AssertNullMetadata(JsonElement? metadata)
    {
        var perplexity = metadata!.Value.GetProperty("perplexity");
        Assert.Equal(JsonValueKind.Null, perplexity.GetProperty("images").ValueKind);
        Assert.Equal(JsonValueKind.Null, perplexity.GetProperty("cost").ValueKind);
        Assert.Equal(JsonValueKind.Null, perplexity.GetProperty("usage").GetProperty("citationTokens").ValueKind);
        Assert.Equal(JsonValueKind.Null, perplexity.GetProperty("usage").GetProperty("numSearchQueries").ValueKind);
    }

    private static void AssertUrls(JsonElement citations, IEnumerable<GeneratedSource> sources)
    {
        var expected = new List<string>();
        foreach (var citation in citations.EnumerateArray())
        {
            expected.Add(citation.GetString()!);
        }

        Assert.Equal(expected, sources.Select(source => source.Url));
    }

    private static GeneratedSource ToSource(SourceStreamPart part)
    {
        return new GeneratedSource(part.Id, part.Url, part.Title);
    }

    private static string Header(IReadOnlyDictionary<string, string> headers, string name)
    {
        foreach (var pair in headers)
        {
            if (pair.Key.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return pair.Value;
            }
        }

        throw new InvalidOperationException("Missing header " + name);
    }
}
