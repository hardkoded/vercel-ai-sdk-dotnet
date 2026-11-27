// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.AnthropicLanguageModelTests.AnthropicLanguageModelTestSupport;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; code execution file uploads</c>.</summary>
public sealed class DoGenerateCodeExecutionFileUploadsTests
{
    private static JsonNode? Find(JsonArray content, string type, string toolCallId)
    {
        return content.FirstOrDefault(part =>
            (string?)part!["type"] == type && (string?)part["toolCallId"] == toolCallId
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > code execution file uploads::should send container upload content with code execution tool and parse results",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_container_upload_content_with_code_execution_tool_and_parse_results()
    {
        var handler = JsonFixture("anthropic-code-execution-file-upload.1");
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage(
                    new UserContentPart[]
                    {
                        new TextContentPart("Analyze this CSV data."),
                        FileContentPart.FromProviderReference(
                            "text/csv",
                            new Dictionary<string, string> { ["anthropic"] = "file-csv-12345" },
                            null,
                            new Dictionary<string, JsonElement>
                            {
                                ["anthropic"] = AnthropicParity.Json(
                                    """{"containerUpload":true}"""
                                ),
                            }
                        ),
                    }
                ),
            },
            Tools = [ProviderTool("anthropic.code_execution_20250825", "code_execution")],
        };

        var result = await DoGenerate(handler, options);

        AssertJson(
            """
            {
              "max_tokens": 4096,
              "messages": [
                {
                  "content": [
                    {"text":"Analyze this CSV data.","type":"text"},
                    {"file_id":"file-csv-12345","type":"container_upload"}
                  ],
                  "role": "user"
                }
              ],
              "model": "claude-3-haiku-20240307",
              "tools": [{"name":"code_execution","type":"code_execution_20250825"}]
            }
            """,
            RequestBody(handler)
        );
        AssertRequestHeaders(
            handler,
            ("anthropic-beta", "files-api-2025-04-14,code-execution-2025-08-25"),
            ("anthropic-version", "2023-06-01"),
            ("content-type", "application/json"),
            ("x-api-key", "test-api-key")
        );

        var content = ContentJson(result.Content).AsArray();
        var viewCall = Find(content, "tool-call", "srvtoolu_01Bxt8MXPF5wYdD7KH2HLPzR");
        AssertJsonContains(
            JsonNode.Parse(
                """{"type":"tool-call","toolCallId":"srvtoolu_01Bxt8MXPF5wYdD7KH2HLPzR","toolName":"code_execution","input":"{\"type\":\"text_editor_code_execution\",\"command\":\"view\",\"path\":\"$INPUT_DIR/sample.csv\"}","providerExecuted":true}"""
            ),
            viewCall
        );
        var viewResult = Find(content, "tool-result", "srvtoolu_01Bxt8MXPF5wYdD7KH2HLPzR");
        AssertJsonContains(
            JsonNode.Parse(
                """{"type":"tool-result","toolName":"code_execution","result":{"type":"text_editor_code_execution_view_result"}}"""
            ),
            viewResult
        );
        Assert.Contains(
            "month,revenue,expenses,profit",
            (string)viewResult!["result"]!["content"]!
        );
        var bashCall = Find(content, "tool-call", "srvtoolu_0142vFu9QyCKuFZ2X14zqBLR");
        AssertJsonContains(
            JsonNode.Parse(
                """{"type":"tool-call","toolName":"code_execution","providerExecuted":true}"""
            ),
            bashCall
        );
        var bashResult = Find(content, "tool-result", "srvtoolu_0142vFu9QyCKuFZ2X14zqBLR");
        AssertJsonContains(
            JsonNode.Parse(
                """{"type":"tool-result","toolName":"code_execution","result":{"type":"bash_code_execution_result"}}"""
            ),
            bashResult
        );
        Assert.Contains("Total Profit: $35400", (string)bashResult!["result"]!["stdout"]!);

        AssertJsonContains(
            JsonNode.Parse("""{"id":"container_011CbJPykM9av9Vj1cQkVu87"}"""),
            JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())!["anthropic"]!["container"]
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > code execution file uploads::should send container id for a follow-up code execution turn",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_container_id_for_a_follow_up_code_execution_turn()
    {
        var handler = JsonFixture("anthropic-text");

        await DoGenerate(
            handler,
            Options(
                """{"container":{"id":"container_12345"}}""",
                [ProviderTool("anthropic.code_execution_20250825", "code_execution")],
                "Use the uploaded CSV from the existing container."
            )
        );

        var expected = HelloBody(
            """{"container":"container_12345","tools":[{"name":"code_execution","type":"code_execution_20250825"}]}"""
        );
        expected["messages"] = JsonNode.Parse(
            """[{"content":[{"text":"Use the uploaded CSV from the existing container.","type":"text"}],"role":"user"}]"""
        );
        AssertJson(expected, RequestBody(handler));
    }
}
