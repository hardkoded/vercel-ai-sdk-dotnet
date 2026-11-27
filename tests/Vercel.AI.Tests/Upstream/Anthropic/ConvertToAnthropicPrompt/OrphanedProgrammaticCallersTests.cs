// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using static Vercel.AI.Tests.ConvertToAnthropicPrompt.PromptRunner;

namespace Vercel.AI.Tests.ConvertToAnthropicPrompt;

/// <summary>Port of <c>convert-to-anthropic-prompt.test.ts</c> &gt; <c>orphaned programmatic callers</c>.</summary>
public sealed class OrphanedProgrammaticCallersTests
{
    private const string Prefix =
        "packages/anthropic/src/convert-to-anthropic-prompt.test.ts::orphaned programmatic callers::";

    [Theory]
    [InlineData("code_execution_20250825")]
    [InlineData("code_execution_20260120")]
    [UpstreamTest(
        Prefix + "should normalize historical %s callers without changing calls or results",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_normalize_historical_callers_without_changing_calls_or_results(
        string callerType
    )
    {
        var prompt =
            "[{\"role\":\"assistant\",\"content\":[{\"type\":\"tool-call\",\"toolCallId\":\"lookup-call\",\"toolName\":\"lookup\",\"input\":{\"ticker\":\"AAPL\"},\"providerOptions\":{\"anthropic\":{\"caller\":{\"type\":\""
            + callerType
            + "\",\"toolId\":\"pruned-source\"},\"cacheControl\":{\"type\":\"ephemeral\"}}}}]},"
            + "{\"role\":\"tool\",\"content\":[{\"type\":\"tool-result\",\"toolCallId\":\"lookup-call\",\"toolName\":\"lookup\",\"output\":{\"type\":\"json\",\"value\":{\"price\":185.42}}}]},"
            + "{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"The price is $185.42.\"}]},"
            + "{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Now look up MSFT.\"}]}]";

        var result = Run(prompt, sendReasoning: false);

        AssertJson(
            "[{\"role\":\"assistant\",\"content\":[{\"type\":\"tool_use\",\"id\":\"lookup-call\",\"name\":\"lookup\",\"input\":{\"ticker\":\"AAPL\"},\"cache_control\":{\"type\":\"ephemeral\"}}]},"
                + "{\"role\":\"user\",\"content\":[{\"type\":\"tool_result\",\"tool_use_id\":\"lookup-call\",\"content\":\"{\\\"price\\\":185.42}\"}]},"
                + "{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"The price is $185.42.\"}]},"
                + "{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Now look up MSFT.\"}]}]",
            result.Messages
        );
        AssertWarnings(
            result,
            "Omitted caller metadata for tool lookup-call because source code execution tool pruned-source is missing from the conversation history."
        );
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [UpstreamTest(
        Prefix + "should preserve callers in an active continuation (source present: %s)",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_preserve_callers_in_an_active_continuation(bool includeSource)
    {
        var source = includeSource
            ? "{\"type\":\"tool-call\",\"toolCallId\":\"source-call\",\"toolName\":\"code_execution\",\"providerExecuted\":true,\"input\":{\"type\":\"programmatic-tool-call\",\"code\":\"await lookup({})\"}},"
            : string.Empty;
        var prompt =
            "[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Look up AAPL.\"}]},"
            + "{\"role\":\"assistant\",\"content\":["
            + source
            + "{\"type\":\"tool-call\",\"toolCallId\":\"lookup-call\",\"toolName\":\"lookup\",\"input\":{},\"providerOptions\":{\"anthropic\":{\"caller\":{\"type\":\"code_execution_20260120\",\"toolId\":\"source-call\"}}}}]},"
            + "{\"role\":\"tool\",\"content\":[{\"type\":\"tool-result\",\"toolCallId\":\"lookup-call\",\"toolName\":\"lookup\",\"output\":{\"type\":\"text\",\"value\":\"185.42\"}}]}]";

        var result = Run(prompt, sendReasoning: false);

        var content = (JsonArray)result.Messages[1]!["content"]!;
        var last = content[content.Count - 1]!;
        Assert.Equal("tool_use", (string?)last["type"]);
        AssertJson(
            "{\"type\":\"code_execution_20260120\",\"tool_id\":\"source-call\"}",
            last["caller"]
        );
        Assert.Empty(result.Warnings);
    }

    [Theory]
    [InlineData("run_code", true, true)]
    [InlineData("code_execution", false, false)]
    [InlineData("web_search", true, false)]
    [InlineData("unsupported", true, false)]
    [UpstreamTest(
        Prefix + "should resolve historical sources from emitted code execution blocks: $toolName",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_resolve_historical_sources_from_emitted_code_execution_blocks(
        string toolName,
        bool providerExecuted,
        bool keepsCaller
    )
    {
        var prompt =
            "[{\"role\":\"assistant\",\"content\":[{\"type\":\"tool-call\",\"toolCallId\":\"source-call\",\"toolName\":\""
            + toolName
            + "\",\"providerExecuted\":"
            + (providerExecuted ? "true" : "false")
            + ",\"input\":{\"type\":\"programmatic-tool-call\",\"code\":\"await lookup({})\"},\"providerOptions\":{\"anthropic\":{\"caller\":{\"type\":\"direct\"}}}}]},"
            + "{\"role\":\"assistant\",\"content\":[{\"type\":\"tool-call\",\"toolCallId\":\"lookup-call\",\"toolName\":\"lookup\",\"input\":{},\"providerOptions\":{\"anthropic\":{\"caller\":{\"type\":\"code_execution_20260120\",\"toolId\":\"source-call\"}}}}]},"
            + "{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Continue.\"}]}]";

        var result = Run(
            prompt,
            sendReasoning: false,
            providerToolNames: new Dictionary<string, string> { ["run_code"] = "code_execution" }
        );

        var content = (JsonArray)result.Messages[0]!["content"]!;
        var lookup = content.Single(part => (string?)part!["id"] == "lookup-call")!;
        Assert.Equal("tool_use", (string?)lookup["type"]);
        if (keepsCaller)
        {
            AssertJson(
                "{\"type\":\"code_execution_20260120\",\"tool_id\":\"source-call\"}",
                lookup["caller"]
            );
            Assert.Empty(result.Warnings);
        }
        else
        {
            Assert.False(lookup.AsObject().ContainsKey("caller"));
            Assert.Contains(result.Warnings, warning =>
                warning.Type == "other"
                && warning.Message
                    == "Omitted caller metadata for tool lookup-call because source code execution tool source-call is missing from the conversation history."
            );
        }

        if (toolName != "unsupported")
        {
            var source = content.Single(part => (string?)part!["id"] == "source-call")!;
            AssertJson("{\"type\":\"direct\"}", source["caller"]);
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should normalize callers on historical server calls and results",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_normalize_callers_on_historical_server_calls_and_results()
    {
        const string caller =
            "\"providerOptions\":{\"anthropic\":{\"caller\":{\"type\":\"code_execution_20260120\",\"toolId\":\"pruned-source\"}}}";
        var prompt =
            "[{\"role\":\"assistant\",\"content\":["
            + "{\"type\":\"tool-call\",\"toolCallId\":\"search-call\",\"toolName\":\"web_search\",\"providerExecuted\":true,\"input\":{\"query\":\"AI SDK\"},"
            + caller
            + "},"
            + "{\"type\":\"tool-result\",\"toolCallId\":\"search-call\",\"toolName\":\"web_search\",\"output\":{\"type\":\"json\",\"value\":[]},"
            + caller
            + "}]},"
            + "{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Tell me more.\"}]}]";

        var result = Run(prompt, sendReasoning: false);

        AssertJson(
            "[{\"type\":\"server_tool_use\",\"id\":\"search-call\",\"name\":\"web_search\",\"input\":{\"query\":\"AI SDK\"}},"
                + "{\"type\":\"web_search_tool_result\",\"tool_use_id\":\"search-call\",\"content\":[]}]",
            result.Messages[0]!["content"]
        );
        AssertWarnings(
            result,
            "Omitted caller metadata for tool search-call because source code execution tool pruned-source is missing from the conversation history."
        );
    }
}
