// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools;

public sealed class OmitFromRequestToolsTests
{
    private static LanguageModelTool[] Tools()
    {
        using var schema = JsonDocument.Parse("{\"type\":\"object\"}");
        using var options = JsonDocument.Parse("{\"openai\":{\"omitFromRequestTools\":true}}");
        return new[]
        {
            new LanguageModelTool("search", null, schema.RootElement.Clone()),
            new LanguageModelTool("list_issues", null, schema.RootElement.Clone()) { ProviderOptions = options.RootElement.Clone() },
        };
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > omitFromRequestTools::omits only the marked function from the top-level tools", Coverage = UpstreamCoverage.Covered)]
    public void Omits_only_the_marked_function_from_the_top_level_tools()
    {
        var tools = Tools();
        var result = OpenAIResponsesLanguageModel.PrepareResponsesTools(tools, ToolChoice.Auto);

        Assert.Equal(new[] { "search" }, result.Tools!.Select(tool => tool!["name"]!.GetValue<string>()));
        Assert.Equal("auto", result.ToolChoice!.GetValue<string>());
        Assert.Empty(result.ToolWarnings);
        Assert.Equal(2, tools.Length);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > omitFromRequestTools::supports having only history-loaded functions", Coverage = UpstreamCoverage.Covered)]
    public void Supports_having_only_history_loaded_functions()
    {
        var result = OpenAIResponsesLanguageModel.PrepareResponsesTools(new[] { Tools()[1] }, toolChoice: null);

        Assert.Empty(result.Tools!);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > omitFromRequestTools::rejects forcing a function absent from the top-level tools", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_forcing_a_function_absent_from_the_top_level_tools()
    {
        var error = Assert.Throws<UnsupportedFunctionalityException>(() =>
            OpenAIResponsesLanguageModel.PrepareResponsesTools(Tools(), ToolChoice.Tool("list_issues")));

        Assert.Contains("forcing tool \"list_issues\"", error.Message);
    }
}
