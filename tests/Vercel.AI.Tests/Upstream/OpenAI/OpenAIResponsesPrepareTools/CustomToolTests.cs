// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools.OpenAIResponsesPrepareToolsSupport;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools;

public sealed class CustomToolTests
{
    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > custom tool::should prepare custom tool with regex format", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_custom_tool_with_regex_format()
    {
        var result = Prepare(null, ProviderTool("openai.custom", "write_sql", "{\"description\":\"Write a SQL SELECT query.\",\"format\":{\"type\":\"grammar\",\"syntax\":\"regex\",\"definition\":\"SELECT .+\"}}"));

        AssertTools(result, "[{\"type\":\"custom\",\"name\":\"write_sql\",\"description\":\"Write a SQL SELECT query.\",\"format\":{\"type\":\"grammar\",\"syntax\":\"regex\",\"definition\":\"SELECT .+\"}}]");
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > custom tool::should prepare custom tool with lark format", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_custom_tool_with_lark_format()
    {
        var result = Prepare(null, ProviderTool("openai.custom", "generate_json", "{\"format\":{\"type\":\"grammar\",\"syntax\":\"lark\",\"definition\":\"start: \\\"{\\\"  \\\"}\\\"\"}}"));

        AssertTools(result, "[{\"type\":\"custom\",\"name\":\"generate_json\",\"format\":{\"type\":\"grammar\",\"syntax\":\"lark\",\"definition\":\"start: \\\"{\\\"  \\\"}\\\"\"}}]");
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > custom tool::should handle multiple tools including custom tool", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_multiple_tools_including_custom_tool()
    {
        var result = Prepare(null, FunctionTool(), ProviderTool("openai.custom", "write_sql", "{\"description\":\"Write SQL.\",\"format\":{\"type\":\"grammar\",\"syntax\":\"regex\",\"definition\":\"SELECT .+\"}}"));

        AssertTools(result, "[" + FunctionToolJson + ",{\"type\":\"custom\",\"name\":\"write_sql\",\"description\":\"Write SQL.\",\"format\":{\"type\":\"grammar\",\"syntax\":\"regex\",\"definition\":\"SELECT .+\"}}]");
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > custom tool::should resolve custom tool choice using tool name", Coverage = UpstreamCoverage.Covered)]
    public void Should_resolve_custom_tool_choice_using_tool_name()
    {
        var result = Prepare(ToolChoice.Tool("write_sql"), ProviderTool("openai.custom", "write_sql", "{}"));

        AssertTools(result, "[{\"type\":\"custom\",\"name\":\"write_sql\"}]");
        AssertToolChoice(result, "{\"type\":\"custom\",\"name\":\"write_sql\"}");
    }
}
