// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Anthropic;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Anthropic.AnthropicMessageThreads.ThreadTestSupport;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicMessageThreads;

/// <summary>Port of <c>anthropic-message-threads.test.ts</c> &gt; <c>message threads (%s)</c>.</summary>
public sealed class MessageThreadsTests
{
    private const string Prefix = "packages/anthropic/src/anthropic-message-threads.test.ts::message threads (%s)::";

    public static IEnumerable<object[]> EachMethod() => Each();

    public static IEnumerable<object[]> UnsupportedContextManagement()
    {
        foreach (var method in Methods)
        {
            yield return new object[] { method, "{\"compaction\":{\"type\":\"summarize\"}}" };
            yield return new object[] { method, "{\"contextManagement\":{\"edits\":[{\"type\":\"clear_tool_uses_20250919\"}]}}" };
            yield return new object[] { method, "{\"contextManagement\":{\"edits\":[{\"type\":\"clear_thinking_20251015\"}]}}" };
            yield return new object[] { method, "{\"contextManagement\":{\"edits\":[{\"type\":\"compact_20260112\"}]}}" };
        }
    }

    public static IEnumerable<object[]> InvalidThreadOptions()
    {
        foreach (var method in Methods)
        {
            yield return new object[] { method, "{\"type\":\"continue\"}" };
            yield return new object[] { method, "{\"type\":\"continue\",\"previousMessageId\":\"\"}" };
            yield return new object[] { method, "{\"type\":\"invalid\"}" };
        }
    }

    private static string OptionsJson(string thread, string? extra = null)
    {
        return "{\"thread\":" + thread + (extra == null ? string.Empty : "," + extra) + "}";
    }

    private const string Create = "{\"type\":\"create\"}";

    private static string Continue(string id)
    {
        return "{\"type\":\"continue\",\"previousMessageId\":\"" + id + "\"}";
    }

    [Theory]
    [MemberData(nameof(EachMethod))]
    [UpstreamTest(Prefix + "creates a thread with system and tools and merges beta headers", Coverage = UpstreamCoverage.Covered, Commit = "6b101ac0765a3cf796e94692c3e3c8a3e08ff7f4")]
    public async Task Creates_a_thread_with_system_and_tools_and_merges_beta_headers(string method)
    {
        var handler = new CaptureHandler();
        var options = BaseOptions(OptionsJson(Create));
        options.Headers = new Dictionary<string, string?> { ["anthropic-beta"] = "custom-beta" };

        var result = await Call(method, handler, options);

        using var body = Body(handler);
        JsonAssert.Equal(body.RootElement.GetProperty("thread"), "{\"type\":\"create\"}");
        JsonAssert.Equal(body.RootElement.GetProperty("system"), "[{\"type\":\"text\",\"text\":\"Be concise.\"}]");
        Assert.Equal("weather", body.RootElement.GetProperty("tools")[0].GetProperty("name").GetString());
        Assert.Contains("message-threads-2026-08-12", Betas(handler));
        Assert.Contains("custom-beta", Betas(handler));
        if (method == "stream")
        {
            Assert.Equal("msg_01QC4g3HwBThD4BaNtBckFDJ", result.ResponseId);
            Assert.Contains(nameof(FinishStreamPart), result.PartTypes);
        }
        else
        {
            Assert.Equal("msg_01VdEjxAP5ahtHKrrRdNBteQ", result.ResponseId);
        }
    }

    [Theory]
    [MemberData(nameof(EachMethod))]
    [UpstreamTest(Prefix + "continues with only supplied messages, omitting system and tools", Coverage = UpstreamCoverage.Covered, Commit = "6b101ac0765a3cf796e94692c3e3c8a3e08ff7f4")]
    public async Task Continues_with_only_supplied_messages_omitting_system_and_tools(string method)
    {
        var handler = new CaptureHandler();
        var options = BaseOptions(OptionsJson(Continue("msg_previous"), "\"cacheControl\":{\"type\":\"ephemeral\"}"));
        options.ToolChoice = ToolChoice.Auto;

        await Call(method, handler, options);

        using var body = Body(handler);
        JsonAssert.Equal(body.RootElement.GetProperty("thread"), "{\"type\":\"continue\",\"previous_message_id\":\"msg_previous\"}");
        Assert.False(body.RootElement.TryGetProperty("system", out _));
        Assert.False(body.RootElement.TryGetProperty("tools", out _));
        JsonAssert.Equal(body.RootElement.GetProperty("messages"), "[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Hello\"}]}]");
        JsonAssert.Equal(body.RootElement.GetProperty("tool_choice"), "{\"type\":\"auto\"}");
        JsonAssert.Equal(body.RootElement.GetProperty("cache_control"), "{\"type\":\"ephemeral\"}");
    }

    [Theory]
    [MemberData(nameof(EachMethod))]
    [UpstreamTest(Prefix + "sends tool results without resending the preceding assistant message", Coverage = UpstreamCoverage.Covered, Commit = "6b101ac0765a3cf796e94692c3e3c8a3e08ff7f4")]
    public async Task Sends_tool_results_without_resending_the_preceding_assistant_message(string method)
    {
        var handler = new CaptureHandler();
        var options = Options(
            new ModelMessage[] { new ToolModelMessage("tool_1", "weather", "Sunny", false, "text") },
            OptionsJson(Continue("msg_tools")));

        await Call(method, handler, options);

        using var body = Body(handler);
        JsonAssert.Equal(
            body.RootElement.GetProperty("messages"),
            "[{\"role\":\"user\",\"content\":[{\"type\":\"tool_result\",\"tool_use_id\":\"tool_1\",\"content\":\"Sunny\"}]}]");
    }

    private static ModelMessage Pdf()
    {
        return new UserModelMessage(new UserContentPart[] { new FileContentPart("application/pdf", null, new byte[] { 1 }, null) });
    }

    [Theory]
    [MemberData(nameof(EachMethod))]
    [UpstreamTest(Prefix + "keeps pinned PDF beta headers across a text-only continuation", Coverage = UpstreamCoverage.Covered, Commit = "6b101ac0765a3cf796e94692c3e3c8a3e08ff7f4")]
    public async Task Keeps_pinned_PDF_beta_headers_across_a_text_only_continuation(string method)
    {
        var handler = new CaptureHandler();
        const string pinned = "\"anthropicBeta\":[\"pdfs-2024-09-25\"]";
        var first = await Call(method, handler, Options(new[] { Pdf() }, OptionsJson(Create, pinned), tool: false));
        var firstBetas = string.Join(",", Betas(handler));
        await Call(method, handler, Options(new[] { Hello() }, OptionsJson(Continue("msg_pdf"), pinned), tool: false));
        var secondBetas = string.Join(",", Betas(handler));

        Assert.Equal(firstBetas, secondBetas);
        Assert.Empty(first.Warnings);
    }

    [Theory]
    [MemberData(nameof(EachMethod))]
    [UpstreamTest(Prefix + "warns when message content enables an unpinned beta", Coverage = UpstreamCoverage.Covered, Commit = "6b101ac0765a3cf796e94692c3e3c8a3e08ff7f4")]
    public async Task Warns_when_message_content_enables_an_unpinned_beta(string method)
    {
        var result = await Call(method, new CaptureHandler(), Options(new[] { Pdf() }, OptionsJson(Create), tool: false));

        Assert.Contains(result.Warnings, warning => warning.Message.Contains("pdfs-2024-09-25"));
    }

    [Theory]
    [MemberData(nameof(EachMethod))]
    [UpstreamTest(Prefix + "warns rather than silently dropping new leading system instructions", Coverage = UpstreamCoverage.Covered, Commit = "6b101ac0765a3cf796e94692c3e3c8a3e08ff7f4")]
    public async Task Warns_rather_than_silently_dropping_new_leading_system_instructions(string method)
    {
        var result = await Call(method, new CaptureHandler(), BaseOptions(OptionsJson(Continue("msg_previous"))));

        Assert.Contains(
            result.Warnings,
            warning => warning.Message.Contains("To change system instructions or tools, start a new thread"));
    }

    [Theory]
    [MemberData(nameof(EachMethod))]
    [UpstreamTest(Prefix + "keeps implicit code execution dynamic for web tools on continuation", Coverage = UpstreamCoverage.Covered, Commit = "6b101ac0765a3cf796e94692c3e3c8a3e08ff7f4")]
    public async Task Keeps_implicit_code_execution_dynamic_for_web_tools_on_continuation(string method)
    {
        var handler = new CaptureHandler();
        var options = new LanguageModelCallOptions
        {
            Prompt = new[] { Hello() },
            Tools = new[] { LanguageModelTool.ProviderDefined("anthropic.web_search_20260209", "research", Json("{}")) },
            ProviderOptions = Opts(OptionsJson(Continue("msg_web"))),
        };

        var result = await Call(method, handler, options, null, "anthropic-programmatic-tool-calling.1");

        if (method == "stream")
        {
            Assert.Contains(result.StreamToolCalls, call => call.ToolName == "code_execution" && call.Dynamic);
        }
        else
        {
            Assert.Contains(result.Content.OfType<GeneratedToolCall>(), call => call.ToolName == "code_execution" && call.Dynamic);
        }

        using var body = Body(handler);
        Assert.False(body.RootElement.TryGetProperty("tools", out _));
    }

    [Theory]
    [MemberData(nameof(EachMethod))]
    [UpstreamTest(Prefix + "supports a custom provider-options namespace", Coverage = UpstreamCoverage.Covered, Commit = "6b101ac0765a3cf796e94692c3e3c8a3e08ff7f4")]
    public async Task Supports_a_custom_provider_options_namespace(string method)
    {
        var handler = new CaptureHandler();
        var model = AnthropicProvider.Create(new AnthropicOptions { ApiKey = "test-key", Name = "custom.messages" }, handler)
            .LanguageModel("claude-sonnet-4-6");
        var options = new LanguageModelCallOptions
        {
            Prompt = new[] { Hello() },
            ProviderOptions = Opts(OptionsJson(Continue("msg_custom")), "custom"),
        };

        await Call(method, handler, options, model);

        using var body = Body(handler);
        JsonAssert.Equal(body.RootElement.GetProperty("thread"), "{\"type\":\"continue\",\"previous_message_id\":\"msg_custom\"}");
    }

    [Theory]
    [MemberData(nameof(EachMethod))]
    [UpstreamTest(Prefix + "retains JSON-tool choice while omitting its synthetic definition", Coverage = UpstreamCoverage.Covered, Commit = "6b101ac0765a3cf796e94692c3e3c8a3e08ff7f4")]
    public async Task Retains_JSON_tool_choice_while_omitting_its_synthetic_definition(string method)
    {
        var handler = new CaptureHandler();
        var options = new LanguageModelCallOptions
        {
            Prompt = new[] { Hello() },
            JsonResponse = true,
            JsonSchema = Json("{\"type\":\"object\",\"properties\":{}}"),
            ProviderOptions = Opts(OptionsJson(Continue("msg_json"), "\"structuredOutputMode\":\"jsonTool\"")),
        };

        await Call(method, handler, options);

        using var body = Body(handler);
        Assert.False(body.RootElement.TryGetProperty("tools", out _));
        Assert.Equal("any", body.RootElement.GetProperty("tool_choice").GetProperty("type").GetString());
    }

    [Theory]
    [MemberData(nameof(EachMethod))]
    [UpstreamTest(Prefix + "counts top-level cache control toward the four-breakpoint limit", Coverage = UpstreamCoverage.Covered, Commit = "6b101ac0765a3cf796e94692c3e3c8a3e08ff7f4")]
    public async Task Counts_top_level_cache_control_toward_the_four_breakpoint_limit(string method)
    {
        var handler = new CaptureHandler();
        var cache = Opts("{\"cacheControl\":{\"type\":\"ephemeral\"}}");
        var parts = Enumerable.Range(0, 4)
            .Select(index => (UserContentPart)new TextContentPart("Block " + index) { ProviderOptions = cache })
            .ToArray();
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage(parts) },
            ProviderOptions = Opts(OptionsJson(Create, "\"cacheControl\":{\"type\":\"ephemeral\"}")),
        };

        var result = await Call(method, handler, options);

        using var body = Body(handler);
        var cached = body.RootElement.GetProperty("messages")[0].GetProperty("content").EnumerateArray()
            .Count(part => part.TryGetProperty("cache_control", out var value) && value.ValueKind != JsonValueKind.Null);
        Assert.Equal(3, cached);
        Assert.Contains(result.Warnings, warning => warning.Feature == "cacheControl breakpoint limit");
    }

    [Theory]
    [MemberData(nameof(EachMethod))]
    [UpstreamTest(Prefix + "allows empty continuation messages for pause_turn", Coverage = UpstreamCoverage.Covered, Commit = "6b101ac0765a3cf796e94692c3e3c8a3e08ff7f4")]
    public async Task Allows_empty_continuation_messages_for_pause_turn(string method)
    {
        var handler = new CaptureHandler();
        var options = new LanguageModelCallOptions
        {
            Prompt = Array.Empty<ModelMessage>(),
            ProviderOptions = Opts(OptionsJson(Continue("msg_paused"))),
        };

        await Call(method, handler, options);

        using var body = Body(handler);
        JsonAssert.Equal(body.RootElement.GetProperty("messages"), "[]");
    }

    [Theory]
    [MemberData(nameof(EachMethod))]
    [UpstreamTest(Prefix + "leaves requests unchanged when threads are not enabled", Coverage = UpstreamCoverage.Covered, Commit = "6b101ac0765a3cf796e94692c3e3c8a3e08ff7f4")]
    public async Task Leaves_requests_unchanged_when_threads_are_not_enabled(string method)
    {
        var handler = new CaptureHandler();

        await Call(method, handler, BaseOptions());

        using var body = Body(handler);
        Assert.False(body.RootElement.TryGetProperty("thread", out _));
        Assert.DoesNotContain(Betas(handler), beta => beta.Contains("message-threads"));
    }

    [Theory]
    [MemberData(nameof(UnsupportedContextManagement))]
    [UpstreamTest(Prefix + "rejects unsupported context management: %j", Coverage = UpstreamCoverage.Covered, Commit = "6b101ac0765a3cf796e94692c3e3c8a3e08ff7f4")]
    public async Task Rejects_unsupported_context_management(string method, string incompatible)
    {
        var handler = new CaptureHandler();
        var providerOptions = incompatible.Substring(0, incompatible.Length - 1) + ",\"thread\":" + Create + "}";

        var error = await Assert.ThrowsAnyAsync<Exception>(() => Call(method, handler, BaseOptions(providerOptions)));

        Assert.Contains("message threads do not support", error.Message);
        Assert.Equal(0, handler.Calls);
    }

    [Theory]
    [MemberData(nameof(EachMethod))]
    [UpstreamTest(Prefix + "allows keeping all thinking blocks", Coverage = UpstreamCoverage.Covered, Commit = "6b101ac0765a3cf796e94692c3e3c8a3e08ff7f4")]
    public async Task Allows_keeping_all_thinking_blocks(string method)
    {
        var handler = new CaptureHandler();
        var options = BaseOptions(OptionsJson(Create, "\"contextManagement\":{\"edits\":[{\"type\":\"clear_thinking_20251015\",\"keep\":\"all\"}]}"));

        await Call(method, handler, options);

        using var body = Body(handler);
        JsonAssert.Equal(
            body.RootElement.GetProperty("context_management"),
            "{\"edits\":[{\"type\":\"clear_thinking_20251015\",\"keep\":\"all\"}]}");
    }

    [Theory]
    [MemberData(nameof(InvalidThreadOptions))]
    [UpstreamTest(Prefix + "rejects invalid thread options: %j", Coverage = UpstreamCoverage.Covered, Commit = "6b101ac0765a3cf796e94692c3e3c8a3e08ff7f4")]
    public async Task Rejects_invalid_thread_options(string method, string thread)
    {
        var handler = new CaptureHandler();

        await Assert.ThrowsAnyAsync<Exception>(() => Call(method, handler, BaseOptions(OptionsJson(thread))));

        Assert.Equal(0, handler.Calls);
    }
}
