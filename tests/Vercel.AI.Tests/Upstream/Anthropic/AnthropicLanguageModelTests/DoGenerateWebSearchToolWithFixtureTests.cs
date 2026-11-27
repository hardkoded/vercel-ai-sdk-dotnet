// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; web search tool &gt; with fixture</c>.</summary>
public sealed class DoGenerateWebSearchToolWithFixtureTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > web search tool > with fixture::";

    // Maps the beforeEach of the describe.
    private static async Task<(
        CaptureHandler Handler,
        LanguageModelGenerateResult Result
    )> GenerateAsync()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-web-search-tool.1");
        var options = AnthropicParity.Hello("What is the latest news?");
        options.Tools = new[]
        {
            LanguageModelTool.ProviderDefined(
                "anthropic.web_search_20250305",
                "web_search",
                AnthropicParity.Json(
                    "{\"maxUses\":1,\"userLocation\":{\"type\":\"approximate\",\"country\":\"US\"}}"
                )
            ),
        };

        var result = await AnthropicParity.Generate(
            LanguageModelTestSupport.ProviderWithIds(handler),
            "claude-3-haiku-20240307",
            options
        );
        return (handler, result);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send request body with include and tool",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_request_body_with_include_and_tool()
    {
        var (handler, _) = await GenerateAsync();

        LanguageModelTestSupport.AssertJson(
            """
            {
              "max_tokens": 4096,
              "messages": [
                {
                  "content": [{ "text": "What is the latest news?", "type": "text" }],
                  "role": "user"
                }
              ],
              "model": "claude-3-haiku-20240307",
              "tools": [
                {
                  "max_uses": 1,
                  "name": "web_search",
                  "type": "web_search_20250305",
                  "user_location": { "country": "US", "type": "approximate" }
                }
              ]
            }
            """,
            LanguageModelTestSupport.RequestBody(handler)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include web search tool call and result in content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_web_search_tool_call_and_result_in_content()
    {
        var (_, result) = await GenerateAsync();

        // Upstream shares one mockId counter between all tests of the file, so the snapshot ids count from id-11.
        // Here the counter belongs to the test, so the ids count from id-0.
        var expected = LanguageModelTestSupport.SnapshotFile("web-search-content").AsArray();
        var next = 0;
        foreach (var part in expected)
        {
            if (part!["type"]!.GetValue<string>() == "source")
            {
                part["id"] = "id-" + next++;
            }
        }

        Assert.True(
            JsonNode.DeepEquals(expected, LanguageModelTestSupport.Snapshot(result.Content))
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should expose search results as sources and keep citations in text metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_expose_search_results_as_sources_and_keep_citations_in_text_metadata()
    {
        var result = AnthropicResponse.Parse(
            File.ReadAllText(
                Path.Combine(
                    AppContext.BaseDirectory,
                    "Fixtures",
                    "anthropic-web-search-tool.1.json"
                )
            ),
            new AnthropicParseContext(false, "anthropic", false)
        );

        var sources = result.Content.OfType<GeneratedSource>().ToList();
        var textParts = result.Content.OfType<GeneratedText>().ToList();

        Assert.Equal(
            3,
            textParts.SelectMany(part => part.Citations ?? Array.Empty<Citation>()).Count()
        );
        Assert.Equal(10, sources.Count);
        Assert.Equal(10, sources.Select(source => source.Url).Distinct().Count());
        Assert.Equal(
            3,
            textParts
                .OfType<AnthropicText>()
                .Sum(part =>
                    part.ProviderMetadata!.Value.GetProperty("anthropic")
                        .GetProperty("citations")
                        .GetArrayLength()
                )
        );
    }
}
