// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Google.GoogleVertexAnthropicStructuredOutput.VertexAnthropicStructuredOutputSupport;

namespace Vercel.AI.Tests.Upstream.Google.GoogleVertexAnthropicStructuredOutput;

/// <summary>Port of <c>describe.each(['generate', 'stream'])('%s tools')</c> inside the native-output opt-in describe.</summary>
public sealed class RuntimeNativeOutputOptInMethodToolsTests
{
    private const string Prefix = "packages/google-vertex/src/anthropic/google-vertex-anthropic-structured-output.test.ts::%s native-output opt-in > %s tools::";

    [Theory]
    [InlineData("generate", false)]
    [InlineData("generate", true)]
    [InlineData("stream", false)]
    [InlineData("stream", true)]
    [UpstreamTest(Prefix + "preserves tool input schemas and arguments without enabling strict tools (native final output: %s)", Coverage = UpstreamCoverage.Covered, Commit = "4b8f00efff97de78e52667359aa33267a1036c46", Note = "Edge runs the same HttpClient.")]
    public async Task Preserves_tool_input_schemas_and_arguments_without_enabling_strict_tools(string method, bool withOutput)
    {
        var value = new JsonObject { ["input"] = Report }.ToJsonString();
        var handler = new Handler(method, false, value, "report");
        var model = Model("claude-sonnet-4-6", handler, true, authToken: true);
        var options = Options(withOutput ? ReportSchema : null);
        options.Tools = new[] { new LanguageModelTool("report", null, Parse(ReportSchema), strict: true) };

        var (content, _, parts) = await Call(model, method, options).ConfigureAwait(false);

        var tool = JsonNode.Parse("{\"name\":\"report\"}")!;
        tool["input_schema"] = JsonNode.Parse(ReportSchema);
        if (method == "stream")
        {
            tool["eager_input_streaming"] = true;
        }

        Assert.True(JsonNode.DeepEquals(new JsonArray(tool), handler.Body["tools"]));
        if (withOutput)
        {
            var expected = JsonNode.Parse("{\"type\":\"json_schema\"}")!;
            expected["schema"] = AnthropicJsonSchema.Sanitize(Parse(ReportSchema));
            Assert.True(JsonNode.DeepEquals(expected, handler.Body["output_config"]!["format"]));
        }
        else
        {
            Assert.Null(handler.Body["output_config"]);
        }

        Assert.False(handler.Headers.ContainsKey("anthropic-beta"));
        var call = method == "generate"
            ? content.OfType<GeneratedToolCall>().Single()
            : null;
        if (call != null)
        {
            Assert.Equal("report", call.ToolName);
            Assert.Equal(value, JsonNode.Parse(call.ArgumentsJson)!.ToJsonString());
        }
        else
        {
            var streamed = parts.OfType<ToolCallStreamPart>().Single();
            Assert.Equal("report", streamed.ToolName);
            Assert.Equal(value, JsonNode.Parse(streamed.ArgumentsJson)!.ToJsonString());
        }
    }
}
