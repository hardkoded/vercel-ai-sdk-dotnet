// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>OpenAIResponsesLanguageModel &gt; doGenerate &gt; local shell tool</c>.</summary>
public sealed class DoGenerateLocalShellToolTests
{
    private const string Prefix = "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > local shell tool::";

    [Fact]
    [UpstreamTest(Prefix + "should send request body with include and tool", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_request_body_with_include_and_tool()
    {
        var (_, capture) = await Run();

        var expected = JsonNode.Parse("""{"input":[{"content":[{"text":"Hello","type":"input_text"}],"role":"user"}],"model":"gpt-5-codex","tools":[{"type":"local_shell"}]}""");
        var body = JsonNode.Parse(capture.Body)!;
        Assert.True(JsonNode.DeepEquals(expected, body), body.ToJsonString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should include generate image tool call and result in content", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_generate_image_tool_call_and_result_in_content()
    {
        var (result, _) = await Run();

        OpenAIResponsesSupport.AssertContentSnapshot(result.Content, "openai-local-shell-tool.1");
    }

    private static async Task<(LanguageModelGenerateResult Result, OpenAICapture Capture)> Run()
    {
        var capture = new OpenAICapture { ResponseJson = OpenAIResponsesSupport.Fixture("openai-local-shell-tool.1.json") };
        var options = OpenAIUpstream.Hello();
        options.Tools = new[] { LanguageModelTool.ProviderDefined("openai.local_shell", "shell", OpenAIUpstream.Json("{}")) };

        var result = await OpenAIUpstream.Provider(capture).ResponsesModel("gpt-5-codex").DoGenerateAsync(options, CancellationToken.None);
        return (result, capture);
    }
}
