// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class OpenAICompatibleChatUpstreamTests
{
    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/convert-openai-compatible-chat-usage.test.ts::convertOpenAICompatibleChatUsage::returns null usage when usage is missing", Coverage = UpstreamCoverage.Partial, Note = "Input and output stay unset. LanguageModelUsage computes a zero total when both counts are unset.")]
    public void Missing_usage_leaves_token_counts_unset()
    {
        var usage = OpenAICompatibleChat.ConvertUsage(null);
        Assert.Null(usage.Usage.InputTokens);
        Assert.Null(usage.Usage.OutputTokens);
        Assert.Null(usage.Usage.CacheWriteTokens);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/convert-openai-compatible-chat-usage.test.ts::convertOpenAICompatibleChatUsage::splits completion tokens into text and reasoning", Coverage = UpstreamCoverage.Covered)]
    public void Usage_splits_text_and_reasoning_tokens()
    {
        using var document = JsonDocument.Parse("{\"prompt_tokens\":11,\"completion_tokens\":7,\"completion_tokens_details\":{\"reasoning_tokens\":3}}");
        var usage = OpenAICompatibleChat.ConvertUsage(document.RootElement);
        Assert.Equal(11, usage.Usage.InputTokens);
        Assert.Equal(7, usage.Usage.OutputTokens);
        Assert.Equal(3, usage.Usage.ReasoningTokens);
        Assert.Equal(4, usage.Usage.TextTokens);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/convert-openai-compatible-chat-usage.test.ts::convertOpenAICompatibleChatUsage::clamps text tokens at 0 when reasoning exceeds completion", Coverage = UpstreamCoverage.Covered)]
    public void Usage_clamps_text_tokens_at_zero()
    {
        using var document = JsonDocument.Parse("{\"prompt_tokens\":1,\"completion_tokens\":2,\"completion_tokens_details\":{\"reasoning_tokens\":9}}");
        var usage = OpenAICompatibleChat.ConvertUsage(document.RootElement);
        Assert.Equal(0, usage.Usage.TextTokens);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/convert-to-openai-compatible-chat-messages.test.ts::user messages::should convert messages with only a text part to a string content", Coverage = UpstreamCoverage.Covered)]
    public async Task User_text_is_a_string()
    {
        var capture = new UpstreamCapture();
        await UpstreamChat.Model(capture).DoGenerateAsync(UpstreamChat.Prompt("Hello"), CancellationToken.None);
        Assert.Equal("Hello", UpstreamChat.Body(capture)["messages"]![0]!["content"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/convert-to-openai-compatible-chat-messages.test.ts::user messages::should convert messages with image parts", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_bytes_become_a_data_url()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Prompt = new ModelMessage[]
        {
            new UserModelMessage(new UserContentPart[]
            {
                new TextContentPart("see"),
                new FileContentPart("image/png", null, new byte[] { 1, 2, 3 }, null),
            }),
        };
        await UpstreamChat.Model(capture).DoGenerateAsync(options, CancellationToken.None);
        var part = UpstreamChat.Body(capture)["messages"]![0]!["content"]![1]!;
        Assert.Equal("image_url", part["type"]!.GetValue<string>());
        Assert.Equal("data:image/png;base64," + Convert.ToBase64String(new byte[] { 1, 2, 3 }), part["image_url"]!["url"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/convert-to-openai-compatible-chat-messages.test.ts::user messages::should handle URL-based images", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_urls_pass_through()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Prompt = new ModelMessage[] { new UserModelMessage(new UserContentPart[] { new FileContentPart("image/png", "https://example.test/cat.png", null, null) }) };
        await UpstreamChat.Model(capture).DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("https://example.test/cat.png", UpstreamChat.Body(capture)["messages"]![0]!["content"]![0]!["image_url"]!["url"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/convert-to-openai-compatible-chat-messages.test.ts::user messages::should convert messages with audio/wav parts", Coverage = UpstreamCoverage.Covered)]
    public async Task Wav_audio_uses_input_audio()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Prompt = new ModelMessage[] { new UserModelMessage(new UserContentPart[] { new FileContentPart("audio/wav", null, new byte[] { 9 }, null) }) };
        await UpstreamChat.Model(capture).DoGenerateAsync(options, CancellationToken.None);
        var audio = UpstreamChat.Body(capture)["messages"]![0]!["content"]![0]!["input_audio"]!;
        Assert.Equal("wav", audio["format"]!.GetValue<string>());
        Assert.Equal(Convert.ToBase64String(new byte[] { 9 }), audio["data"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/convert-to-openai-compatible-chat-messages.test.ts::user messages::should convert messages with audio/mpeg parts to mp3 format", Coverage = UpstreamCoverage.Covered)]
    public async Task Mpeg_audio_is_mp3()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Prompt = new ModelMessage[] { new UserModelMessage(new UserContentPart[] { new FileContentPart("audio/mpeg", null, new byte[] { 4 }, null) }) };
        await UpstreamChat.Model(capture).DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("mp3", UpstreamChat.Body(capture)["messages"]![0]!["content"]![0]!["input_audio"]!["format"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/convert-to-openai-compatible-chat-messages.test.ts::user messages::should throw error for audio parts with URLs", Coverage = UpstreamCoverage.Covered)]
    public async Task Audio_urls_are_rejected()
    {
        var options = UpstreamChat.Prompt();
        options.Prompt = new ModelMessage[] { new UserModelMessage(new UserContentPart[] { new FileContentPart("audio/wav", "https://example.test/a.wav", null, null) }) };
        var error = await Assert.ThrowsAsync<AiSdkException>(() => UpstreamChat.Model(new UpstreamCapture()).DoGenerateAsync(options, CancellationToken.None));
        Assert.Contains("audio file parts with URLs", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/convert-to-openai-compatible-chat-messages.test.ts::user messages::should throw error for unsupported audio format", Coverage = UpstreamCoverage.Covered)]
    public async Task Unsupported_audio_is_rejected()
    {
        var options = UpstreamChat.Prompt();
        options.Prompt = new ModelMessage[] { new UserModelMessage(new UserContentPart[] { new FileContentPart("audio/flac", null, new byte[] { 1 }, null) }) };
        var error = await Assert.ThrowsAsync<AiSdkException>(() => UpstreamChat.Model(new UpstreamCapture()).DoGenerateAsync(options, CancellationToken.None));
        Assert.Contains("audio/flac", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/convert-to-openai-compatible-chat-messages.test.ts::user messages::should convert messages with PDF parts", Coverage = UpstreamCoverage.Covered)]
    public async Task Pdf_bytes_use_the_default_filename()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Prompt = new ModelMessage[] { new UserModelMessage(new UserContentPart[] { new FileContentPart("application/pdf", null, new byte[] { 7 }, null) }) };
        await UpstreamChat.Model(capture).DoGenerateAsync(options, CancellationToken.None);
        var file = UpstreamChat.Body(capture)["messages"]![0]!["content"]![0]!["file"]!;
        Assert.Equal("document.pdf", file["filename"]!.GetValue<string>());
        Assert.StartsWith("data:application/pdf;base64,", file["file_data"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/convert-to-openai-compatible-chat-messages.test.ts::user messages::should throw error for PDF parts with URLs", Coverage = UpstreamCoverage.Covered)]
    public async Task Pdf_urls_are_rejected()
    {
        var options = UpstreamChat.Prompt();
        options.Prompt = new ModelMessage[] { new UserModelMessage(new UserContentPart[] { new FileContentPart("application/pdf", "https://example.test/a.pdf", null, null) }) };
        var error = await Assert.ThrowsAsync<AiSdkException>(() => UpstreamChat.Model(new UpstreamCapture()).DoGenerateAsync(options, CancellationToken.None));
        Assert.Contains("PDF file parts with URLs", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/convert-to-openai-compatible-chat-messages.test.ts::user messages::should convert messages with text/plain parts from Uint8Array", Coverage = UpstreamCoverage.Covered)]
    public async Task Text_file_bytes_are_decoded()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Prompt = new ModelMessage[] { new UserModelMessage(new UserContentPart[] { new FileContentPart("text/plain", null, Encoding.UTF8.GetBytes("notes"), "notes.txt") }) };
        await UpstreamChat.Model(capture).DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("notes", UpstreamChat.Body(capture)["messages"]![0]!["content"]![0]!["text"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/convert-to-openai-compatible-chat-messages.test.ts::user messages::should convert text file URL to string", Coverage = UpstreamCoverage.Covered)]
    public async Task Text_file_urls_are_sent_as_text()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Prompt = new ModelMessage[] { new UserModelMessage(new UserContentPart[] { new FileContentPart("text/plain", "https://example.test/notes.txt", null, null) }) };
        await UpstreamChat.Model(capture).DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("https://example.test/notes.txt", UpstreamChat.Body(capture)["messages"]![0]!["content"]![0]!["text"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/convert-to-openai-compatible-chat-messages.test.ts::user messages::should throw error for unsupported file types", Coverage = UpstreamCoverage.Covered)]
    public async Task Zip_files_are_rejected()
    {
        var options = UpstreamChat.Prompt();
        options.Prompt = new ModelMessage[] { new UserModelMessage(new UserContentPart[] { new FileContentPart("application/zip", null, new byte[] { 1 }, null) }) };
        var error = await Assert.ThrowsAsync<AiSdkException>(() => UpstreamChat.Model(new UpstreamCapture()).DoGenerateAsync(options, CancellationToken.None));
        Assert.Equal("'file part media type application/zip' functionality not supported.", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/convert-to-openai-compatible-chat-messages.test.ts::user messages::should throw error for file parts with provider references", Coverage = UpstreamCoverage.Covered)]
    public async Task Provider_file_references_are_rejected()
    {
        var options = UpstreamChat.Prompt();
        options.Prompt = new ModelMessage[] { new UserModelMessage(new UserContentPart[] { new FileContentPart("image/png", null, null, null) }) };
        var error = await Assert.ThrowsAsync<AiSdkException>(() => UpstreamChat.Model(new UpstreamCapture()).DoGenerateAsync(options, CancellationToken.None));
        Assert.Contains("provider references", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/convert-to-openai-compatible-chat-messages.test.ts::tool calls::should stringify arguments to tool calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Tool_call_arguments_stay_json_text()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Prompt = new ModelMessage[]
        {
            new AssistantModelMessage(null, new[] { new GeneratedToolCall("call_1", "weather", "{\"city\":\"Paris\"}") }, null),
        };
        await UpstreamChat.Model(capture).DoGenerateAsync(options, CancellationToken.None);
        var message = UpstreamChat.Body(capture)["messages"]![0]!;
        var call = message["tool_calls"]![0]!;
        Assert.Equal("{\"city\":\"Paris\"}", call["function"]!["arguments"]!.GetValue<string>());
        Assert.True(message.AsObject().ContainsKey("content"));
        Assert.Null(message["content"]);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/convert-to-openai-compatible-chat-messages.test.ts::tool calls::should send empty string content for assistant messages with no tool calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Assistant_without_tools_keeps_empty_content()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Prompt = new ModelMessage[] { new AssistantModelMessage(string.Empty, null, null) };
        await UpstreamChat.Model(capture).DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal(string.Empty, UpstreamChat.Body(capture)["messages"]![0]!["content"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/convert-to-openai-compatible-chat-messages.test.ts::tool calls::should handle text output type in tool results", Coverage = UpstreamCoverage.Covered)]
    public async Task Tool_results_send_output_json()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Prompt = new ModelMessage[] { new ToolModelMessage("call_1", "weather", "\"sunny\"", false) };
        await UpstreamChat.Model(capture).DoGenerateAsync(options, CancellationToken.None);
        var message = UpstreamChat.Body(capture)["messages"]![0]!;
        Assert.Equal("tool", message["role"]!.GetValue<string>());
        Assert.Equal("\"sunny\"", message["content"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/convert-to-openai-compatible-chat-messages.test.ts::Google Gemini thought signatures (OpenAI compatibility)::should serialize thought signature from a custom provider namespace", Coverage = UpstreamCoverage.Covered)]
    public async Task Thought_signature_uses_the_provider_metadata_key()
    {
        var capture = new UpstreamCapture();
        using var metadata = JsonDocument.Parse("{\"openai-compatible\":{\"thoughtSignature\":\"sig-1\"}}");
        var options = UpstreamChat.Prompt();
        options.Prompt = new ModelMessage[]
        {
            new AssistantModelMessage(null, new[] { new GeneratedToolCall("call_1", "weather", "{}", metadata.RootElement.Clone()) }, null),
        };
        await UpstreamChat.Model(capture).DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("sig-1", UpstreamChat.Body(capture)["messages"]![0]!["tool_calls"]![0]!["extra_content"]!["google"]!["thought_signature"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/convert-to-openai-compatible-chat-messages.test.ts::Google Gemini thought signatures (OpenAI compatibility)::should fall back to the google namespace", Coverage = UpstreamCoverage.Covered)]
    public async Task Thought_signature_falls_back_to_google()
    {
        var capture = new UpstreamCapture();
        using var metadata = JsonDocument.Parse("{\"google\":{\"thoughtSignature\":\"sig-g\"}}");
        var options = UpstreamChat.Prompt();
        options.Prompt = new ModelMessage[]
        {
            new AssistantModelMessage(null, new[] { new GeneratedToolCall("call_1", "weather", "{}", metadata.RootElement.Clone()) }, null),
        };
        await UpstreamChat.Model(capture).DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("sig-g", UpstreamChat.Body(capture)["messages"]![0]!["tool_calls"]![0]!["extra_content"]!["google"]!["thought_signature"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/convert-to-openai-compatible-chat-messages.test.ts::Google Gemini thought signatures (OpenAI compatibility)::should not include extra_content when no thought signature is present", Coverage = UpstreamCoverage.Covered)]
    public async Task Missing_thought_signature_omits_extra_content()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Prompt = new ModelMessage[] { new AssistantModelMessage(null, new[] { new GeneratedToolCall("call_1", "weather", "{}") }, null) };
        await UpstreamChat.Model(capture).DoGenerateAsync(options, CancellationToken.None);
        Assert.Null(UpstreamChat.Body(capture)["messages"]![0]!["tool_calls"]![0]!["extra_content"]);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate > text (fixture)::should extract text content", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_extracts_text()
    {
        var result = await UpstreamChat.Model(new UpstreamCapture()).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
        Assert.Equal("Hello", result.Text);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate::should reject a response without choices", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_rejects_a_response_without_choices()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"choices\":[]}" };
        var error = await Assert.ThrowsAsync<AiSdkException>(() => UpstreamChat.Model(capture).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None));
        Assert.Equal("Response did not contain any choices.", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate > tool call (fixture)::should extract tool call content", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_extracts_tool_calls()
    {
        var capture = new UpstreamCapture
        {
            ResponseBody = "{\"choices\":[{\"message\":{\"content\":null,\"tool_calls\":[{\"id\":\"call_1\",\"type\":\"function\",\"function\":{\"name\":\"weather\",\"arguments\":\"{\\\"city\\\":\\\"Paris\\\"}\"}}]},\"finish_reason\":\"tool_calls\"}],\"usage\":{\"prompt_tokens\":1,\"completion_tokens\":1,\"total_tokens\":2}}",
        };
        var result = await UpstreamChat.Model(capture).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
        var call = Assert.IsType<GeneratedToolCall>(result.Content[0]);
        Assert.Equal("call_1", call.ToolCallId);
        Assert.Equal("weather", call.ToolName);
        Assert.Equal("{\"city\":\"Paris\"}", call.ArgumentsJson);
        Assert.Equal(FinishReason.ToolCalls, result.FinishReason);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate::should generate distinct IDs for parallel tool calls with empty IDs", Coverage = UpstreamCoverage.Covered)]
    public async Task Empty_tool_call_ids_are_distinct()
    {
        var capture = new UpstreamCapture
        {
            ResponseBody = "{\"choices\":[{\"message\":{\"tool_calls\":[{\"id\":\"\",\"function\":{\"name\":\"a\",\"arguments\":\"{}\"}},{\"function\":{\"name\":\"b\",\"arguments\":\"{}\"}}]},\"finish_reason\":\"tool_calls\"}]}",
        };
        var result = await UpstreamChat.Model(capture).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
        var first = Assert.IsType<GeneratedToolCall>(result.Content[0]);
        var second = Assert.IsType<GeneratedToolCall>(result.Content[1]);
        Assert.StartsWith("call_", first.ToolCallId);
        Assert.StartsWith("call_", second.ToolCallId);
        Assert.NotEqual(first.ToolCallId, second.ToolCallId);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate::should extract usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_extracts_usage()
    {
        var result = await UpstreamChat.Model(new UpstreamCapture()).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
        Assert.Equal(10, result.Usage.InputTokens);
        Assert.Equal(5, result.Usage.OutputTokens);
        Assert.Equal(15, result.Usage.TotalTokens);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate::should send additional response information", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_reads_response_metadata()
    {
        var result = await UpstreamChat.Model(new UpstreamCapture()).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
        Assert.Equal("resp-1", result.ResponseId);
        Assert.Equal("m", result.ResponseModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1711363706), result.ResponseTimestamp);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate::should expose the raw response headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_exposes_response_headers()
    {
        var capture = new UpstreamCapture();
        capture.ResponseHeaders["x-request-id"] = "req-9";
        var model = UpstreamChat.Model(capture);
        var result = await model.DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
        Assert.Equal("req-9", result.ResponseHeaders["x-request-id"]);
        Assert.Equal("req-9", model.LastResponseHeaders["x-request-id"]);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate::should pass user setting to requests", Coverage = UpstreamCoverage.Covered)]
    public async Task User_provider_option_is_sent()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = Bag("openai-compatible", "{\"user\":\"alice\"}");
        await UpstreamChat.Model(capture).DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("alice", UpstreamChat.Body(capture)["user"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate::should extract reasoning from reasoning field when reasoning_content is not provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Reasoning_field_is_used_when_reasoning_content_is_absent()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"choices\":[{\"message\":{\"content\":\"Hi\",\"reasoning\":\"think\"},\"finish_reason\":\"stop\"}]}" };
        var result = await UpstreamChat.Model(capture).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
        Assert.Contains(result.Content, part => part is GeneratedReasoning reasoning && reasoning.Text == "think");
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate::should prefer reasoning_content over reasoning field when both are provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Reasoning_content_wins_over_reasoning()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"choices\":[{\"message\":{\"content\":\"Hi\",\"reasoning_content\":\"first\",\"reasoning\":\"second\"},\"finish_reason\":\"stop\"}]}" };
        var result = await UpstreamChat.Model(capture).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
        var reasoning = result.Content.OfType<GeneratedReasoning>().Single();
        Assert.Equal("first", reasoning.Text);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate::should normalize text and thinking content parts", Coverage = UpstreamCoverage.Covered)]
    public async Task Content_arrays_join_text_and_thinking()
    {
        var capture = new UpstreamCapture
        {
            ResponseBody = "{\"choices\":[{\"message\":{\"content\":[{\"type\":\"text\",\"text\":\"Hi\"},{\"type\":\"thinking\",\"thinking\":[{\"type\":\"text\",\"text\":\"a\"},{\"type\":\"text\",\"text\":\"b\"}]},{\"type\":\"text\",\"text\":\"\"}]},\"finish_reason\":\"stop\"}]}",
        };
        var result = await UpstreamChat.Model(capture).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
        Assert.Equal("Hi", result.Text);
        Assert.Equal("ab", result.Content.OfType<GeneratedReasoning>().Single().Text);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate::should ignore unknown content parts", Coverage = UpstreamCoverage.Covered)]
    public async Task Unknown_content_parts_are_ignored()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"choices\":[{\"message\":{\"content\":[{\"type\":\"mystery\"},{\"type\":\"text\",\"text\":\"Hi\"}]},\"finish_reason\":\"stop\"}]}" };
        var result = await UpstreamChat.Model(capture).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
        Assert.Equal("Hi", result.Text);
        Assert.Single(result.Content);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate::should support partial usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Partial_usage_defaults_missing_counts_to_zero()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"choices\":[{\"message\":{\"content\":\"Hi\"},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":4}}" };
        var result = await UpstreamChat.Model(capture).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
        Assert.Equal(4, result.Usage.InputTokens);
        Assert.Equal(0, result.Usage.OutputTokens);
        Assert.Equal(0, result.Usage.ReasoningTokens);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate::should support unknown finish reason", Coverage = UpstreamCoverage.Covered)]
    public async Task Unknown_finish_reason_is_other()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"choices\":[{\"message\":{\"content\":\"Hi\"},\"finish_reason\":\"custom\"}]}" };
        var result = await UpstreamChat.Model(capture).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
        Assert.Equal(FinishReason.Other, result.FinishReason);
        Assert.Equal("custom", result.RawFinishReason);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate::should pass the model and the messages", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_sends_model_and_messages_without_stream()
    {
        var capture = new UpstreamCapture();
        await UpstreamChat.Model(capture, modelId: "chat-1").DoGenerateAsync(UpstreamChat.Prompt("Hi"), CancellationToken.None);
        var body = UpstreamChat.Body(capture);
        Assert.Equal("chat-1", body["model"]!.GetValue<string>());
        Assert.Equal("Hi", body["messages"]![0]!["content"]!.GetValue<string>());
        Assert.Null(body["stream"]);
        Assert.EndsWith("/chat/completions", capture.Requests[0].Uri!.AbsolutePath);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate::should pass settings", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_sends_sampling_settings()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.MaxOutputTokens = 16;
        options.Temperature = 0.2;
        options.TopP = 0.5;
        options.FrequencyPenalty = 0.1;
        options.PresencePenalty = 0.3;
        options.Seed = 7;
        options.StopSequences = new[] { "END" };
        options.TopK = 3;
        options.Reasoning = "low";
        await UpstreamChat.Model(capture).DoGenerateAsync(options, CancellationToken.None);
        var body = UpstreamChat.Body(capture);
        Assert.Equal(16, body["max_tokens"]!.GetValue<int>());
        Assert.Equal(0.2, body["temperature"]!.GetValue<double>());
        Assert.Equal(0.5, body["top_p"]!.GetValue<double>());
        Assert.Equal(0.1, body["frequency_penalty"]!.GetValue<double>());
        Assert.Equal(0.3, body["presence_penalty"]!.GetValue<double>());
        Assert.Equal(7, body["seed"]!.GetValue<int>());
        Assert.Equal("END", body["stop"]![0]!.GetValue<string>());
        Assert.Equal("low", body["reasoning_effort"]!.GetValue<string>());
        Assert.Null(body["top_k"]);
        Assert.Contains((await Result(capture, options)).Warnings, warning => warning.Type == "unsupported" && warning.Message == "topK");
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate::should pass settings with deprecated openai-compatible key and emit warning", Coverage = UpstreamCoverage.Covered)]
    public async Task Deprecated_provider_key_warns_and_still_applies()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = Bag("openai-compatible", "{\"user\":\"ada\",\"textVerbosity\":\"low\"}");
        var result = await UpstreamChat.Model(capture).DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("ada", UpstreamChat.Body(capture)["user"]!.GetValue<string>());
        Assert.Equal("low", UpstreamChat.Body(capture)["verbosity"]!.GetValue<string>());
        Assert.Contains(result.Warnings, warning => warning.Type == "deprecated" && warning.Message.Contains("openaiCompatible"));
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate::should include provider-specific options", Coverage = UpstreamCoverage.Covered)]
    public async Task Unknown_provider_options_are_copied_into_the_body()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = Bag("openai-compatible", "{\"service_tier\":\"priority\"}");
        await UpstreamChat.Model(capture).DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("priority", UpstreamChat.Body(capture)["service_tier"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate::should not include provider-specific options for different provider", Coverage = UpstreamCoverage.Covered)]
    public async Task Other_provider_options_are_ignored()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = Bag("other", "{\"service_tier\":\"priority\"}");
        await UpstreamChat.Model(capture).DoGenerateAsync(options, CancellationToken.None);
        Assert.Null(UpstreamChat.Body(capture)["service_tier"]);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate > camelCase provider options::should accept camelCase provider options key for hyphenated provider name", Coverage = UpstreamCoverage.Covered)]
    public async Task Camel_case_provider_options_are_accepted()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = Bag("myProvider", "{\"user\":\"ada\"}");
        await UpstreamChat.Model(capture, "my-provider").DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("ada", UpstreamChat.Body(capture)["user"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate > camelCase provider options::should prefer camelCase options over raw-name options", Coverage = UpstreamCoverage.Covered)]
    public async Task Camel_case_provider_options_win()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = new Dictionary<string, JsonElement>
        {
            ["my-provider"] = Json("{\"user\":\"raw\"}"),
            ["myProvider"] = Json("{\"user\":\"camel\"}"),
        };
        await UpstreamChat.Model(capture, "my-provider").DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("camel", UpstreamChat.Body(capture)["user"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate > camelCase provider options::should emit deprecated warning when raw provider options key is used", Coverage = UpstreamCoverage.Covered)]
    public async Task Raw_hyphenated_provider_key_warns()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = Bag("my-provider", "{\"user\":\"ada\"}");
        var result = await UpstreamChat.Model(capture, "my-provider").DoGenerateAsync(options, CancellationToken.None);
        Assert.Contains(result.Warnings, warning => warning.Type == "deprecated" && warning.Message.Contains("myProvider"));
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate > camelCase provider options::should not emit deprecated warning when camelCase provider options key is used", Coverage = UpstreamCoverage.Covered)]
    public async Task Camel_case_provider_key_does_not_warn()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = Bag("myProvider", "{\"user\":\"ada\"}");
        var result = await UpstreamChat.Model(capture, "my-provider").DoGenerateAsync(options, CancellationToken.None);
        Assert.DoesNotContain(result.Warnings, warning => warning.Type == "deprecated");
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate::should pass tools and toolChoice", Coverage = UpstreamCoverage.Covered)]
    public async Task Tools_and_tool_choice_are_sent_when_present()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        using var schema = JsonDocument.Parse("{\"type\":\"object\"}");
        options.Tools = new[] { new LanguageModelTool("weather", "Look up weather", schema.RootElement.Clone(), true) };
        options.ToolChoice = ToolChoice.Tool("weather");
        await UpstreamChat.Model(capture).DoGenerateAsync(options, CancellationToken.None);
        var body = UpstreamChat.Body(capture);
        Assert.Equal("weather", body["tools"]![0]!["function"]!["name"]!.GetValue<string>());
        Assert.True(body["tools"]![0]!["function"]!["strict"]!.GetValue<bool>());
        Assert.Equal("weather", body["tool_choice"]!["function"]!["name"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Call_headers_override_provider_headers()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.Headers = new Dictionary<string, string?> { ["X-Custom"] = "yes" };
        await UpstreamChat.Model(capture, configure: options => options.Headers["X-Provider"] = "p").DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("yes", capture.Requests[0].Headers["X-Custom"]);
        Assert.Equal("p", capture.Requests[0].Headers["X-Provider"]);
        Assert.Equal("Bearer secret", capture.Requests[0].Headers["Authorization"]);
        Assert.Contains("ai-sdk/openai-compatible/" + AiSdkVersion.Version, capture.Requests[0].Headers["User-Agent"]);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate > Google Gemini thought signatures (OpenAI compatibility)::should parse thought signature from extra_content and include in providerMetadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Response_thought_signature_is_provider_metadata()
    {
        var capture = new UpstreamCapture
        {
            ResponseBody = "{\"choices\":[{\"message\":{\"tool_calls\":[{\"id\":\"call_1\",\"function\":{\"name\":\"weather\",\"arguments\":\"{}\"},\"extra_content\":{\"google\":{\"thought_signature\":\"sig\"}}}]},\"finish_reason\":\"tool_calls\"}]}",
        };
        var result = await UpstreamChat.Model(capture).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
        var call = Assert.IsType<GeneratedToolCall>(result.Content[0]);
        Assert.True(call.ProviderMetadata.HasValue);
        Assert.Equal("sig", call.ProviderMetadata!.Value.GetProperty("openai-compatible").GetProperty("thoughtSignature").GetString());
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate > response format::should forward json response format as \"json_object\" and omit schema when structuredOutputs are disabled", Coverage = UpstreamCoverage.Covered)]
    public async Task Json_mode_without_structured_outputs_uses_json_object()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.JsonSchema = Json("{\"type\":\"object\"}");
        options.ProviderOptions = Bag("openai-compatible", "{\"responseFormat\":\"json\"}");
        var result = await UpstreamChat.Model(capture).DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("json_object", UpstreamChat.Body(capture)["response_format"]!["type"]!.GetValue<string>());
        Assert.Contains(result.Warnings, warning => warning.Message.Contains("structuredOutputs"));
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doGenerate > response format::should use json_schema & strict with responseFormat json when structuredOutputs are enabled", Coverage = UpstreamCoverage.Covered)]
    public async Task Structured_outputs_send_json_schema()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.JsonSchema = Json("{\"type\":\"object\"}");
        options.JsonSchemaName = "answer";
        options.ProviderOptions = Bag("openai-compatible", "{\"responseFormat\":\"json\",\"responseFormatDescription\":\"A name\"}");
        var model = UpstreamChat.Model(capture, configure: settings => settings.SupportsStructuredOutputs = true);
        await model.DoGenerateAsync(options, CancellationToken.None);
        var schema = UpstreamChat.Body(capture)["response_format"]!["json_schema"]!;
        Assert.Equal("json_schema", UpstreamChat.Body(capture)["response_format"]!["type"]!.GetValue<string>());
        Assert.True(schema["strict"]!.GetValue<bool>());
        Assert.Equal("answer", schema["name"]!.GetValue<string>());
        Assert.Equal("A name", schema["description"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doStream::should respect the includeUsage option", Coverage = UpstreamCoverage.Covered)]
    public async Task Include_usage_sends_stream_options()
    {
        var capture = StreamCapture();
        var model = UpstreamChat.Model(capture, configure: settings => settings.IncludeUsage = true);
        await UpstreamChat.Read(model.DoStreamAsync(UpstreamChat.Prompt(), CancellationToken.None));
        Assert.True(UpstreamChat.Body(capture)["stream"]!.GetValue<bool>());
        Assert.True(UpstreamChat.Body(capture)["stream_options"]!["include_usage"]!.GetValue<bool>());
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doStream::should stream reasoning content before text deltas", Coverage = UpstreamCoverage.Covered)]
    public async Task Reasoning_is_streamed_before_text()
    {
        var capture = StreamCapture(UpstreamChat.Sse(
            "{\"choices\":[{\"delta\":{\"reasoning_content\":\"Think\"}}]}",
            "{\"choices\":[{\"delta\":{\"content\":\"Hi\"},\"finish_reason\":\"stop\"}]}"));
        var parts = await UpstreamChat.Read(UpstreamChat.Model(capture).DoStreamAsync(UpstreamChat.Prompt(), CancellationToken.None));
        Assert.IsType<ReasoningStartStreamPart>(parts[1]);
        Assert.Equal("Think", Assert.IsType<ReasoningDeltaStreamPart>(parts[2]).Delta);
        Assert.IsType<ReasoningEndStreamPart>(parts[3]);
        Assert.Equal("Hi", Assert.IsType<TextDeltaStreamPart>(parts[5]).Delta);
        Assert.Equal("txt-0", Assert.IsType<TextDeltaStreamPart>(parts[5]).Id);
        Assert.IsType<FinishStreamPart>(parts[parts.Count - 1]);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doStream::should keep reasoning active when deltas include empty tool calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Empty_tool_calls_do_not_end_reasoning()
    {
        var capture = StreamCapture(UpstreamChat.Sse(
            "{\"choices\":[{\"delta\":{\"reasoning_content\":\"Think\",\"tool_calls\":[]}}]}",
            "{\"choices\":[{\"delta\":{\"reasoning_content\":\" more\"},\"finish_reason\":\"stop\"}]}"));
        var parts = await UpstreamChat.Read(UpstreamChat.Model(capture).DoStreamAsync(UpstreamChat.Prompt(), CancellationToken.None));
        var reasoning = parts.OfType<ReasoningDeltaStreamPart>().Select(part => part.Delta).ToArray();
        Assert.Equal(new[] { "Think", " more" }, reasoning);
        Assert.Single(parts.OfType<ReasoningEndStreamPart>());
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doStream::should handle unparsable stream parts", Coverage = UpstreamCoverage.Covered)]
    public async Task Unparsable_stream_chunks_finish_with_error()
    {
        var capture = StreamCapture(UpstreamChat.Sse("not-json", "{\"choices\":[{\"delta\":{},\"finish_reason\":\"stop\"}]}"));
        var parts = await UpstreamChat.Read(UpstreamChat.Model(capture).DoStreamAsync(UpstreamChat.Prompt(), CancellationToken.None));
        Assert.Contains(parts, part => part is ErrorStreamPart);
        Assert.Equal(FinishReason.Stop, Assert.IsType<FinishStreamPart>(parts[parts.Count - 1]).FinishReason);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doStream::should preserve structured error stream parts", Coverage = UpstreamCoverage.Covered)]
    public async Task Error_chunks_finish_as_error_when_no_later_finish_reason_arrives()
    {
        var capture = StreamCapture(UpstreamChat.Sse("{\"error\":{\"message\":\"quota\"}}"));
        var parts = await UpstreamChat.Read(UpstreamChat.Model(capture).DoStreamAsync(UpstreamChat.Prompt(), CancellationToken.None));
        Assert.Equal("quota", Assert.IsType<ErrorStreamPart>(parts[1]).Message);
        Assert.Equal(FinishReason.Error, Assert.IsType<FinishStreamPart>(parts[parts.Count - 1]).FinishReason);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doStream::should report an error when $scenario without a finish reason", Coverage = UpstreamCoverage.Covered)]
    public async Task A_stream_without_a_finish_reason_is_an_error()
    {
        var capture = StreamCapture(UpstreamChat.Sse("{\"choices\":[{\"delta\":{\"content\":\"Hi\"}}]}"));
        var parts = await UpstreamChat.Read(UpstreamChat.Model(capture).DoStreamAsync(UpstreamChat.Prompt(), CancellationToken.None));
        Assert.Contains(parts, part => part is ErrorStreamPart error && error.Message == "Response stream ended without a finish reason.");
        Assert.Equal(FinishReason.Error, Assert.IsType<FinishStreamPart>(parts[parts.Count - 1]).FinishReason);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doStream::should error when streamed tool call never receives a function.name", Coverage = UpstreamCoverage.Covered)]
    public async Task A_tool_call_without_a_name_fails()
    {
        var capture = StreamCapture(UpstreamChat.Sse(
            "{\"choices\":[{\"delta\":{\"tool_calls\":[{\"index\":0,\"id\":\"call_1\",\"function\":{\"arguments\":\"{}\"}}]},\"finish_reason\":\"tool_calls\"}]}"));
        var error = await Assert.ThrowsAsync<AiSdkException>(() => UpstreamChat.Read(UpstreamChat.Model(capture).DoStreamAsync(UpstreamChat.Prompt(), CancellationToken.None)));
        Assert.Equal("Expected 'function.name' to be a string.", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::doStream::should stream tool deltas when function.name arrives in a later chunk", Coverage = UpstreamCoverage.Partial, Note = "The tracker emits one completed tool call. It does not emit tool-input start and delta parts.")]
    public async Task A_late_tool_name_is_assembled()
    {
        var capture = StreamCapture(UpstreamChat.Sse(
            "{\"choices\":[{\"delta\":{\"tool_calls\":[{\"index\":0,\"id\":\"call_1\",\"function\":{\"arguments\":\"{\\\"a\\\":\"}}]}}]}",
            "{\"choices\":[{\"delta\":{\"tool_calls\":[{\"index\":0,\"function\":{\"name\":\"weather\",\"arguments\":\"1}\"}}]},\"finish_reason\":\"tool_calls\"}]}"));
        var parts = await UpstreamChat.Read(UpstreamChat.Model(capture).DoStreamAsync(UpstreamChat.Prompt(), CancellationToken.None));
        var call = parts.OfType<ToolCallStreamPart>().Single();
        Assert.Equal("call_1", call.ToolCallId);
        Assert.Equal("weather", call.ToolName);
        Assert.Equal("{\"a\":1}", call.ArgumentsJson);
    }

    private static async Task<LanguageModelGenerateResult> Result(UpstreamCapture capture, LanguageModelCallOptions options)
    {
        capture.Requests.Clear();
        return await UpstreamChat.Model(capture).DoGenerateAsync(options, CancellationToken.None);
    }

    private static UpstreamCapture StreamCapture(string? body = null)
    {
        return new UpstreamCapture
        {
            MediaType = "text/event-stream",
            ResponseBody = body ?? UpstreamChat.Sse("{\"choices\":[{\"delta\":{\"content\":\"Hi\"},\"finish_reason\":\"stop\"}]}"),
        };
    }

    private static IReadOnlyDictionary<string, JsonElement> Bag(string key, string json)
    {
        return new Dictionary<string, JsonElement> { [key] = Json(json) };
    }

    private static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
