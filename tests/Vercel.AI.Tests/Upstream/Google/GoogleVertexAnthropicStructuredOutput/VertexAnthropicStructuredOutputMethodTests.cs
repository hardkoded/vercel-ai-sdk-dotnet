// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Google.GoogleVertexAnthropicStructuredOutput.VertexAnthropicStructuredOutputSupport;

namespace Vercel.AI.Tests.Upstream.Google.GoogleVertexAnthropicStructuredOutput;

/// <summary>Port of <c>describe.each(['generate', 'stream'])('Vertex Anthropic structured output: %s')</c>.</summary>
public sealed class VertexAnthropicStructuredOutputMethodTests
{
    private const string Prefix =
        "packages/google-vertex/src/anthropic/google-vertex-anthropic-structured-output.test.ts::Vertex Anthropic structured output: %s::";

    public static IEnumerable<object[]> Cases()
    {
        foreach (var method in new[] { "generate", "stream" })
        {
            foreach (var provider in new[] { "vertex", "vertex-opt-in", "anthropic" })
            {
                foreach (var mode in new[] { "auto", "jsonTool", "outputFormat" })
                {
                    yield return new object[] { method, provider, mode };
                }
            }
        }
    }

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest(
        Prefix + "$provider $mode preserves the schema and returned arguments",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4b8f00efff97de78e52667359aa33267a1036c46",
        Note = "The 'anthropic' provider runs AnthropicProvider with a stub handler."
    )]
    public async Task Preserves_the_schema_and_returned_arguments(
        string method,
        string provider,
        string mode
    )
    {
        var native = mode == "outputFormat" || (provider != "vertex" && mode == "auto");
        var handler = new Handler(method, native, Report);
        var model =
            provider == "anthropic"
                ? AnthropicProvider
                    .Create(new AnthropicOptions { ApiKey = "test-key" }, handler)
                    .LanguageModel("claude-opus-5")
                : Model("claude-opus-5", handler, provider == "vertex-opt-in");

        var (content, _, parts) = await Call(model, method, Options(ReportSchema, mode))
            .ConfigureAwait(false);

        if (method == "generate")
        {
            Assert.Equal(Report, Assert.IsType<GeneratedText>(Assert.Single(content)).Text);
        }
        else
        {
            Assert.Equal(native ? Report : JsonNode.Parse(Report)!.ToJsonString(), JoinText(parts));
            Assert.Equal(FinishReason.Stop, parts.OfType<FinishStreamPart>().Single().FinishReason);
        }

        var request = handler.Body;
        if (native)
        {
            var expected = JsonNode.Parse("{\"type\":\"json_schema\"}")!;
            expected["schema"] = AnthropicJsonSchema.Sanitize(Parse(ReportSchema));
            Assert.True(JsonNode.DeepEquals(expected, request["output_config"]!["format"]));
            Assert.Null(request["tools"]);
            Assert.False(handler.Headers.ContainsKey("anthropic-beta"));
        }
        else
        {
            Assert.Null(request["output_config"]);
            var tool = JsonNode.Parse(
                "{\"name\":\"json\",\"description\":\"Respond with a JSON object.\"}"
            )!;
            tool["input_schema"] = JsonNode.Parse(ReportSchema);
            if (method == "stream")
            {
                tool["eager_input_streaming"] = true;
            }

            Assert.True(JsonNode.DeepEquals(new JsonArray(tool), request["tools"]));
            Assert.True(
                JsonNode.DeepEquals(
                    JsonNode.Parse("{\"type\":\"any\",\"disable_parallel_tool_use\":true}"),
                    request["tool_choice"]
                )
            );
        }

        if (provider != "anthropic")
        {
            Assert.Equal("vertex-2023-10-16", request["anthropic_version"]!.GetValue<string>());
            Assert.Null(request["model"]);
        }
    }
}
