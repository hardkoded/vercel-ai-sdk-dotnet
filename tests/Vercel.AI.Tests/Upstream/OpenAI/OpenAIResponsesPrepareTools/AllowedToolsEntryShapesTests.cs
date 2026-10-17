// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools.OpenAIResponsesPrepareToolsSupport;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools;

public sealed class AllowedToolsEntryShapesTests
{
    [Fact]
    [UpstreamTest(Suite + " > allowedTools provider option > entry shapes::should emit the correct allowed_tools entry for %s", Coverage = UpstreamCoverage.Covered)]
    public void Should_emit_the_correct_allowed_tools_entry_for_each_tool()
    {
        var cases = new (string Label, LanguageModelTool Tool, string AllowedName, string ExpectedEntry)[]
        {
            ("web search", ProviderTool("openai.web_search", "search", "{}"), "search", """{"type":"web_search"}"""),
            ("file search", ProviderTool("openai.file_search", "file_search", """{"vectorStoreIds":["vs-1"]}"""), "file_search", """{"type":"file_search"}"""),
            ("web search preview", ProviderTool("openai.web_search_preview", "web_search_preview", "{}"), "web_search_preview", """{"type":"web_search_preview"}"""),
            ("image generation", ProviderTool("openai.image_generation", "image_generation", "{}"), "image_generation", """{"type":"image_generation"}"""),
            ("code interpreter", ProviderTool("openai.code_interpreter", "code_interpreter", "{}"), "code_interpreter", """{"type":"code_interpreter"}"""),
            ("apply patch", ProviderTool("openai.apply_patch", "apply_patch", "{}"), "apply_patch", """{"type":"apply_patch"}"""),
            ("shell", ProviderTool("openai.shell", "shell", "{}"), "shell", """{"type":"shell"}"""),
            ("computer", ProviderTool("openai.computer", "computer", "{}"), "computer", """{"type":"computer"}"""),
            ("mcp (carries its server label)", ProviderTool("openai.mcp", "deepwiki", """{"serverLabel":"deepwiki","serverUrl":"https://mcp.deepwiki.com/mcp"}"""), "deepwiki", """{"type":"mcp","server_label":"deepwiki"}"""),
            ("custom (carries its name)", ProviderTool("openai.custom", "write_sql", """{"description":"Write a SQL SELECT query."}"""), "write_sql", """{"type":"custom","name":"write_sql"}"""),
        };

        foreach (var (label, tool, allowedName, expectedEntry) in cases)
        {
            var result = Prepare(Allowed(null, new[] { allowedName }), null, tool);

            AssertToolChoice(result, $$$"""{"type":"allowed_tools","mode":"auto","tools":[{{{expectedEntry}}}]}""");
            Assert.True(result.ToolWarnings.Count == 0, label);
        }
    }
}
