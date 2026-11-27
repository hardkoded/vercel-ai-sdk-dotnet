// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;
using static Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests.AzureResponsesToolSupport;

namespace Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests;

/// <summary>Port of <c>azure-openai-provider.test.ts</c> &gt; <c>responses > doGenerate > code interpreter tool</c>.</summary>
public sealed class ResponsesDoGenerateCodeInterpreterToolTests
{
    private const string Id =
        "packages/azure/src/azure-openai-provider.test.ts::responses > doGenerate > code interpreter tool::";

    private static async Task<(OpenAICapture Capture, LanguageModelGenerateResult Result)> Run()
    {
        var capture = JsonFixture("azure-code-interpreter-tool.1");
        var result = await Generate(
            capture,
            Call(
                ProviderTool("openai.code_interpreter", "code_interpreter", "{}"),
                azureOptions: null
            )
        );
        return (capture, result);
    }

    [Fact]
    [UpstreamTest(
        Id + "should send request body with include and tool",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_request_body_with_include_and_tool()
    {
        var (capture, _) = await Run();

        var body = RequestBody(capture);
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    "{\"include\":[\"code_interpreter_call.outputs\"],\"input\":[{\"content\":[{\"text\":\"Hello\",\"type\":\"input_text\"}],\"role\":\"user\"}],\"model\":\"test-deployment\",\"tools\":[{\"container\":{\"type\":\"auto\"},\"type\":\"code_interpreter\"}]}"
                ),
                body
            ),
            body.ToJsonString()
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should include code interpreter tool call and result in content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_code_interpreter_tool_call_and_result_in_content()
    {
        var (_, result) = await Run();

        OpenAIResponsesSupport.AssertContentSnapshot(
            result.Content,
            "azure-code-interpreter-tool.1",
            includeCitations: true,
            SequentialIds()
        );
    }
}
