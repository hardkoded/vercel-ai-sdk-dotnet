// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools.OpenAIResponsesPrepareToolsSupport;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools;

public sealed class CodeInterpreterTests
{
    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > code interpreter::should prepare code interpreter tool with no container (auto mode)", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_code_interpreter_tool_with_no_container_auto_mode()
    {
        var result = Prepare(null, ProviderTool("openai.code_interpreter", "code_interpreter", "{}"));

        AssertTools(result, "[{\"type\":\"code_interpreter\",\"container\":{\"type\":\"auto\"}}]");
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > code interpreter::should prepare code interpreter tool with string container ID", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_code_interpreter_tool_with_string_container_ID()
    {
        var result = Prepare(null, ProviderTool("openai.code_interpreter", "code_interpreter", "{\"container\":\"container-123\"}"));

        AssertTools(result, "[{\"type\":\"code_interpreter\",\"container\":\"container-123\"}]");
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > code interpreter::should prepare code interpreter tool with file IDs container", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_code_interpreter_tool_with_file_IDs_container()
    {
        var result = Prepare(null, ProviderTool("openai.code_interpreter", "code_interpreter", "{\"container\":{\"fileIds\":[\"file-1\",\"file-2\",\"file-3\"]}}"));

        AssertTools(result, "[{\"type\":\"code_interpreter\",\"container\":{\"type\":\"auto\",\"file_ids\":[\"file-1\",\"file-2\",\"file-3\"]}}]");
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > code interpreter::should prepare code interpreter tool with empty file IDs array", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_code_interpreter_tool_with_empty_file_IDs_array()
    {
        var result = Prepare(null, ProviderTool("openai.code_interpreter", "code_interpreter", "{\"container\":{\"fileIds\":[]}}"));

        AssertTools(result, "[{\"type\":\"code_interpreter\",\"container\":{\"type\":\"auto\",\"file_ids\":[]}}]");
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > code interpreter::should prepare code interpreter tool with undefined file IDs", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_code_interpreter_tool_with_undefined_file_IDs()
    {
        var result = Prepare(null, ProviderTool("openai.code_interpreter", "code_interpreter", "{\"container\":{}}"));

        AssertTools(result, "[{\"type\":\"code_interpreter\",\"container\":{\"type\":\"auto\"}}]");
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > code interpreter::should handle tool choice selection with code interpreter", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_tool_choice_selection_with_code_interpreter()
    {
        var result = Prepare(ToolChoice.Tool("code_interpreter"), ProviderTool("openai.code_interpreter", "code_interpreter", "{}"));

        AssertTools(result, "[{\"type\":\"code_interpreter\",\"container\":{\"type\":\"auto\"}}]");
        AssertToolChoice(result, "{\"type\":\"code_interpreter\"}");
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > code interpreter::should handle multiple tools including code interpreter", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_multiple_tools_including_code_interpreter()
    {
        var result = Prepare(null, FunctionTool(), ProviderTool("openai.code_interpreter", "code_interpreter", "{\"container\":\"my-container\"}"));

        AssertTools(result, "[" + FunctionToolJson + ",{\"type\":\"code_interpreter\",\"container\":\"my-container\"}]");
    }
}
