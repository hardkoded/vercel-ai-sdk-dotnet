// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class OpenAICompatibleCompletionUpstreamTests
{
    [Fact]
    [UpstreamTest("packages/openai-compatible/src/completion/openai-compatible-completion-language-model.test.ts::config::should extract base name from provider string", Coverage = UpstreamCoverage.Covered)]
    public void Provider_options_name_is_the_segment_before_the_dot()
    {
        var model = new OpenAICompatibleCompletionLanguageModel(Provider(new UpstreamCapture()), "m", "my-provider.completion");
        Assert.Equal("my-provider", model.ProviderOptionsName);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/completion/openai-compatible-completion-language-model.test.ts::config::should handle provider without dot notation", Coverage = UpstreamCoverage.Covered)]
    public void A_provider_id_without_a_dot_is_its_own_options_name()
    {
        var model = new OpenAICompatibleCompletionLanguageModel(Provider(new UpstreamCapture()), "m", "completion");
        Assert.Equal("completion", model.ProviderOptionsName);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/completion/openai-compatible-completion-language-model.test.ts::config::should return empty for empty provider", Coverage = UpstreamCoverage.Covered)]
    public void An_empty_provider_id_has_an_empty_options_name()
    {
        var model = new OpenAICompatibleCompletionLanguageModel(Provider(new UpstreamCapture()), "m", string.Empty);
        Assert.Equal(string.Empty, model.ProviderOptionsName);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/completion/openai-compatible-completion-language-model.test.ts::doGenerate::should extract text response", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_extracts_completion_text()
    {
        var result = await Generate("{\"choices\":[{\"text\":\"Hello, World!\",\"finish_reason\":\"stop\"}]}");
        Assert.Equal("Hello, World!", result.Text);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/completion/openai-compatible-completion-language-model.test.ts::doGenerate::should extract usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_extracts_completion_usage()
    {
        var result = await Generate("{\"choices\":[{\"text\":\"Hi\",\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":20,\"completion_tokens\":5,\"total_tokens\":25}}");
        Assert.Equal(20, result.Usage.InputTokens);
        Assert.Equal(20, result.Usage.NoCacheInputTokens);
        Assert.Null(result.Usage.CacheReadTokens);
        Assert.Null(result.Usage.CacheWriteTokens);
        Assert.Equal(5, result.Usage.OutputTokens);
        Assert.Equal(5, result.Usage.TextTokens);
        Assert.Null(result.Usage.ReasoningTokens);
        Assert.Equal(25, result.Usage.TotalTokens);
        Assert.Equal("{\"prompt_tokens\":20,\"completion_tokens\":5,\"total_tokens\":25}", result.Usage.Raw!.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/completion/openai-compatible-completion-language-model.test.ts::doGenerate::should send request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_sends_the_completion_prompt_and_stop()
    {
        var capture = new UpstreamCapture();
        await Model(capture, "gpt-3.5-turbo-instruct").DoGenerateAsync(UpstreamChat.Prompt("Hello"), CancellationToken.None);
        var body = JsonNode.Parse(capture.Requests[0].Body)!;
        Assert.Equal("gpt-3.5-turbo-instruct", body["model"]!.GetValue<string>());
        Assert.Equal("user:\nHello\n\nassistant:\n", body["prompt"]!.GetValue<string>());
        Assert.Equal("\nuser:", body["stop"]![0]!.GetValue<string>());
        Assert.Null(body["stream"]);
        Assert.EndsWith("/completions", capture.Requests[0].Uri!.AbsolutePath);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/completion/openai-compatible-completion-language-model.test.ts::doGenerate::should send additional response information", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_exposes_response_id_model_and_timestamp()
    {
        var result = await Generate("{\"id\":\"test-id\",\"created\":123,\"model\":\"test-model\",\"choices\":[{\"text\":\"\",\"finish_reason\":\"stop\"}]}");
        Assert.Equal("test-id", result.ResponseId);
        Assert.Equal("test-model", result.ResponseModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(123), result.ResponseTimestamp);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/completion/openai-compatible-completion-language-model.test.ts::doGenerate::should extract finish reason", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_maps_stop()
    {
        var result = await Generate("{\"choices\":[{\"text\":\"Hi\",\"finish_reason\":\"stop\"}]}");
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal("stop", result.RawFinishReason);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/completion/openai-compatible-completion-language-model.test.ts::doGenerate::should support unknown finish reason", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_maps_an_unknown_finish_reason_to_other()
    {
        var result = await Generate("{\"choices\":[{\"text\":\"Hi\",\"finish_reason\":\"eos\"}]}");
        Assert.Equal(FinishReason.Other, result.FinishReason);
        Assert.Equal("eos", result.RawFinishReason);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/completion/openai-compatible-completion-language-model.test.ts::doGenerate::should expose the raw response headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_exposes_response_headers()
    {
        var capture = new UpstreamCapture();
        capture.ResponseHeaders["test-header"] = "test-value";
        var model = Model(capture);
        var result = await model.DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
        Assert.Equal("test-value", result.ResponseHeaders["test-header"]);
        Assert.Equal("test-value", model.LastResponseHeaders["test-header"]);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/completion/openai-compatible-completion-language-model.test.ts::doGenerate::should pass the model and the prompt", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_sends_the_model_id()
    {
        var capture = new UpstreamCapture();
        await Model(capture, "gpt-3.5-turbo-instruct").DoGenerateAsync(UpstreamChat.Prompt("Hello"), CancellationToken.None);
        Assert.Equal("gpt-3.5-turbo-instruct", JsonNode.Parse(capture.Requests[0].Body)!["model"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/completion/openai-compatible-completion-language-model.test.ts::doGenerate::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_passes_call_headers()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Headers = new Dictionary<string, string?> { ["Custom-Request-Header"] = "request-header-value" };
        var provider = OpenAICompatibleProvider.Create(Settings(), capture);
        provider.Options.Headers["Custom-Provider-Header"] = "provider-header-value";
        await provider.CompletionModel("m").DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("request-header-value", capture.Requests[0].Headers["Custom-Request-Header"]);
        Assert.Equal("provider-header-value", capture.Requests[0].Headers["Custom-Provider-Header"]);
        Assert.Equal("Bearer secret", capture.Requests[0].Headers["Authorization"]);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/completion/openai-compatible-completion-language-model.test.ts::doGenerate::should include provider-specific options", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_copies_provider_options()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag("openai-compatible", "{\"suffix\":\"END\",\"user\":\"ada\"}");
        await Model(capture).DoGenerateAsync(options, CancellationToken.None);
        var body = JsonNode.Parse(capture.Requests[0].Body)!;
        Assert.Equal("END", body["suffix"]!.GetValue<string>());
        Assert.Equal("ada", body["user"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/completion/openai-compatible-completion-language-model.test.ts::doGenerate::should not include provider-specific options for different provider", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_ignores_another_providers_options()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag("other", "{\"suffix\":\"END\"}");
        await Model(capture).DoGenerateAsync(options, CancellationToken.None);
        Assert.Null(JsonNode.Parse(capture.Requests[0].Body)!["suffix"]);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/completion/openai-compatible-completion-language-model.test.ts::doGenerate > camelCase provider options::should accept camelCase provider options key for hyphenated provider name", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_accepts_camel_case_provider_options()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag("myProvider", "{\"user\":\"ada\"}");
        await Named(capture, "my-provider").DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("ada", JsonNode.Parse(capture.Requests[0].Body)!["user"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/completion/openai-compatible-completion-language-model.test.ts::doGenerate > camelCase provider options::should prefer camelCase options over raw-name options", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_prefers_camel_case_provider_options()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = new Dictionary<string, JsonElement>
        {
            ["my-provider"] = UpstreamChat.Json("{\"user\":\"raw\"}"),
            ["myProvider"] = UpstreamChat.Json("{\"user\":\"camel\"}"),
        };
        await Named(capture, "my-provider").DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("camel", JsonNode.Parse(capture.Requests[0].Body)!["user"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/completion/openai-compatible-completion-language-model.test.ts::doGenerate > camelCase provider options::should emit deprecated warning when raw provider options key is used", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_warns_on_a_raw_provider_key()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag("my-provider", "{\"user\":\"ada\"}");
        var result = await Named(capture, "my-provider").DoGenerateAsync(options, CancellationToken.None);
        Assert.Contains(result.Warnings, warning => warning.Type == "deprecated" && warning.Message.Contains("myProvider"));
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/completion/openai-compatible-completion-language-model.test.ts::doGenerate > camelCase provider options::should not emit deprecated warning when camelCase provider options key is used", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_does_not_warn_for_a_camel_case_key()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag("myProvider", "{\"user\":\"ada\"}");
        var result = await Named(capture, "my-provider").DoGenerateAsync(options, CancellationToken.None);
        Assert.DoesNotContain(result.Warnings, warning => warning.Type == "deprecated");
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/completion/openai-compatible-completion-language-model.test.ts::doStream::should stream text deltas", Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_emits_text_deltas_with_id_zero()
    {
        var parts = await Read(UpstreamChat.Sse("{\"id\":\"c1\",\"model\":\"m\",\"created\":1711363706,\"choices\":[{\"text\":\"Hi\",\"finish_reason\":\"stop\"}]}"));
        var delta = parts.OfType<TextDeltaStreamPart>().Single();
        Assert.Equal("0", delta.Id);
        Assert.Equal("Hi", delta.Delta);
        Assert.Equal("0", parts.OfType<TextStartStreamPart>().Single().Id);
        Assert.Equal(FinishReason.Stop, parts.OfType<FinishStreamPart>().Single().FinishReason);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/completion/openai-compatible-completion-language-model.test.ts::doStream::should handle error stream parts", Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_surfaces_an_error_chunk()
    {
        var parts = await Read(UpstreamChat.Sse("{\"error\":{\"message\":\"quota\"}}"));
        Assert.Equal("quota", parts.OfType<ErrorStreamPart>().Single().Message);
        Assert.Equal(FinishReason.Error, parts.OfType<FinishStreamPart>().Single().FinishReason);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/completion/openai-compatible-completion-language-model.test.ts::doStream::should handle unparsable stream parts", Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_reports_an_unparsable_chunk()
    {
        var parts = await Read(UpstreamChat.Sse("not-json", "{\"choices\":[{\"text\":\"\",\"finish_reason\":\"stop\"}]}"));
        Assert.Contains(parts.OfType<ErrorStreamPart>(), part => part.Message == "The provider stream chunk could not be parsed.");
        Assert.Equal(FinishReason.Stop, parts.OfType<FinishStreamPart>().Single().FinishReason);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/completion/openai-compatible-completion-language-model.test.ts::doStream::should send request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_sends_stream_true_and_the_prompt()
    {
        var capture = StreamCapture();
        await UpstreamChat.Read(Model(capture).DoStreamAsync(UpstreamChat.Prompt("Hello"), CancellationToken.None));
        var body = JsonNode.Parse(capture.Requests[0].Body)!;
        Assert.True(body["stream"]!.GetValue<bool>());
        Assert.Equal("user:\nHello\n\nassistant:\n", body["prompt"]!.GetValue<string>());
        Assert.Null(body["stream_options"]);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/completion/openai-compatible-completion-language-model.test.ts::doStream::should expose the raw response headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_exposes_response_headers()
    {
        var capture = StreamCapture();
        capture.ResponseHeaders["test-header"] = "test-value";
        var model = Model(capture);
        await UpstreamChat.Read(model.DoStreamAsync(UpstreamChat.Prompt(), CancellationToken.None));
        Assert.Equal("test-value", model.LastResponseHeaders["test-header"]);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/completion/openai-compatible-completion-language-model.test.ts::doStream::should pass the model and the prompt", Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_sends_the_model_id()
    {
        var capture = StreamCapture();
        await UpstreamChat.Read(Model(capture, "gpt-3.5-turbo-instruct").DoStreamAsync(UpstreamChat.Prompt("Hello"), CancellationToken.None));
        Assert.Equal("gpt-3.5-turbo-instruct", JsonNode.Parse(capture.Requests[0].Body)!["model"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/completion/openai-compatible-completion-language-model.test.ts::doStream::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_passes_call_headers()
    {
        var capture = StreamCapture();
        var options = UpstreamChat.Prompt();
        options.Headers = new Dictionary<string, string?> { ["X-Custom"] = "yes" };
        await UpstreamChat.Read(Model(capture).DoStreamAsync(options, CancellationToken.None));
        Assert.Equal("yes", capture.Requests[0].Headers["X-Custom"]);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/completion/openai-compatible-completion-language-model.test.ts::doStream::should include provider-specific options", Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_copies_provider_options()
    {
        var capture = StreamCapture();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag("openai-compatible", "{\"suffix\":\"END\"}");
        await UpstreamChat.Read(Model(capture).DoStreamAsync(options, CancellationToken.None));
        Assert.Equal("END", JsonNode.Parse(capture.Requests[0].Body)!["suffix"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/completion/openai-compatible-completion-language-model.test.ts::doStream::should not include provider-specific options for different provider", Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_ignores_another_providers_options()
    {
        var capture = StreamCapture();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag("other", "{\"suffix\":\"END\"}");
        await UpstreamChat.Read(Model(capture).DoStreamAsync(options, CancellationToken.None));
        Assert.Null(JsonNode.Parse(capture.Requests[0].Body)!["suffix"]);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/completion/openai-compatible-completion-language-model.test.ts::doStream > camelCase provider options::should accept camelCase provider options key for hyphenated provider name", Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_accepts_camel_case_provider_options()
    {
        var capture = StreamCapture();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag("myProvider", "{\"user\":\"ada\"}");
        await UpstreamChat.Read(Named(capture, "my-provider").DoStreamAsync(options, CancellationToken.None));
        Assert.Equal("ada", JsonNode.Parse(capture.Requests[0].Body)!["user"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/completion/openai-compatible-completion-language-model.test.ts::doStream > camelCase provider options::should prefer camelCase options over raw-name options", Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_prefers_camel_case_provider_options()
    {
        var capture = StreamCapture();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = new Dictionary<string, JsonElement>
        {
            ["my-provider"] = UpstreamChat.Json("{\"user\":\"raw\"}"),
            ["myProvider"] = UpstreamChat.Json("{\"user\":\"camel\"}"),
        };
        await UpstreamChat.Read(Named(capture, "my-provider").DoStreamAsync(options, CancellationToken.None));
        Assert.Equal("camel", JsonNode.Parse(capture.Requests[0].Body)!["user"]!.GetValue<string>());
    }

    private static async Task<LanguageModelGenerateResult> Generate(string body)
    {
        var capture = new UpstreamCapture { ResponseBody = body };
        return await Model(capture).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
    }

    private static async Task<List<LanguageModelStreamPart>> Read(string body)
    {
        return await UpstreamChat.Read(Model(StreamCapture(body)).DoStreamAsync(UpstreamChat.Prompt(), CancellationToken.None));
    }

    private static OpenAICompatibleCompletionLanguageModel Model(UpstreamCapture capture, string modelId = "m")
    {
        return Provider(capture).CompletionModel(modelId);
    }

    private static OpenAICompatibleCompletionLanguageModel Named(UpstreamCapture capture, string name)
    {
        var options = Settings();
        options.ProviderName = name;
        return OpenAICompatibleProvider.Create(options, capture).CompletionModel("m");
    }

    private static OpenAICompatibleProvider Provider(UpstreamCapture capture)
    {
        return OpenAICompatibleProvider.Create(Settings(), capture);
    }

    private static OpenAICompatibleOptions Settings()
    {
        return new OpenAICompatibleOptions
        {
            ProviderName = "openai-compatible",
            BaseUrl = "https://example.test/v1",
            ApiKey = "secret",
        };
    }

    private static UpstreamCapture StreamCapture(string? body = null)
    {
        return new UpstreamCapture
        {
            MediaType = "text/event-stream",
            ResponseBody = body ?? UpstreamChat.Sse("{\"choices\":[{\"text\":\"Hi\",\"finish_reason\":\"stop\"}]}"),
        };
    }
}
