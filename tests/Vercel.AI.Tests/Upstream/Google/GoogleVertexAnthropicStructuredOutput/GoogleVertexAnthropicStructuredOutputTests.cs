// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Google.GoogleVertexAnthropicStructuredOutput.VertexAnthropicStructuredOutputSupport;

namespace Vercel.AI.Tests.Upstream.Google.GoogleVertexAnthropicStructuredOutput;

/// <summary>The tests of <c>google-vertex-anthropic-structured-output.test.ts</c> outside any describe.</summary>
public sealed class GoogleVertexAnthropicStructuredOutputTests
{
    private const string Prefix =
        "packages/google-vertex/src/anthropic/google-vertex-anthropic-structured-output.test.ts::";

    [Theory]
    [InlineData("generate")]
    [InlineData("stream")]
    [UpstreamTest(
        Prefix + "keeps open-object schemas on the JSON tool despite the opt-in: %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4b8f00efff97de78e52667359aa33267a1036c46"
    )]
    public async Task Keeps_open_object_schemas_on_the_JSON_tool_despite_the_opt_in(string method)
    {
        const string schema =
            "{\"type\":\"object\",\"additionalProperties\":{\"type\":\"string\"}}";
        var handler = new Handler(method, false, "{\"answer\":\"42\"}");
        var model = Model("claude-sonnet-4-6", handler, true);

        await Call(model, method, Options(schema)).ConfigureAwait(false);

        Assert.Null(handler.Body["output_config"]);
        Assert.True(
            System.Text.Json.Nodes.JsonNode.DeepEquals(
                System.Text.Json.Nodes.JsonNode.Parse(schema),
                handler.Body["tools"]![0]!["input_schema"]
            )
        );
    }

    [Theory]
    [InlineData("generate")]
    [InlineData("stream")]
    [UpstreamTest(
        Prefix + "keeps Vertex Claude 4.1 on tool emulation with the opt-in: %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4b8f00efff97de78e52667359aa33267a1036c46"
    )]
    public async Task Keeps_Vertex_Claude_4_1_on_tool_emulation_with_the_opt_in(string method)
    {
        var handler = new Handler(method, false, Report);
        var model = Model("claude-opus-4-1@20250805", handler, true);

        await Call(model, method, Options(ReportSchema)).ConfigureAwait(false);

        Assert.Null(handler.Body["output_config"]);
        Assert.Equal("json", handler.Body["tools"]![0]!["name"]!.GetValue<string>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [UpstreamTest(
        Prefix + "only warns about forced-tool fallback when native output is disabled (%s)",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4b8f00efff97de78e52667359aa33267a1036c46"
    )]
    public async Task Only_warns_about_forced_tool_fallback_when_native_output_is_disabled(
        bool supportsNativeStructuredOutput
    )
    {
        var handler = new Handler("generate", true, Report);
        var model = Model("claude-opus-5-5", handler, supportsNativeStructuredOutput);

        var (_, warnings, _) = await Call(model, "generate", Options(ReportSchema))
            .ConfigureAwait(false);

        Assert.Equal(
            "json_schema",
            handler.Body["output_config"]!["format"]!["type"]!.GetValue<string>()
        );
        Assert.Equal(
            supportsNativeStructuredOutput ? 0 : 1,
            warnings.Count(warning =>
                warning.Type == "unsupported"
                && warning.Feature == "providerOptions.anthropic.structuredOutputMode"
            )
        );
    }
}
