// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>OpenAIResponsesLanguageModel &gt; doStream &gt; mcp tool</c>.</summary>
public sealed class DoStreamMcpToolTests
{
    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doStream > mcp tool::should send a tunnel target without legacy target fields", Coverage = UpstreamCoverage.Covered, Commit = "7a905226b619e14e705722448fe0e8629de13739")]
    public async Task Should_send_a_tunnel_target_without_legacy_target_fields()
    {
        var events = OpenAIResponsesSupport.Fixture("openai-mcp-tool.1.chunks.txt")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => JsonNode.Parse(line)!);
        var capture = new OpenAICapture { ServerSentEvents = OpenAIResponsesSupport.Sse(events) };
        var options = OpenAIUpstream.Hello();
        options.Tools = new[]
        {
            LanguageModelTool.ProviderDefined(
                "openai.mcp",
                "MCP",
                OpenAIUpstream.Json("""{"serverLabel":"private","tunnelId":"tunnel_0123456789abcdef0123456789abcdef","allowedTools":{"readOnly":true},"requireApproval":"always"}""")),
        };

        await OpenAIUpstream.Read(await OpenAIUpstream.Provider(capture).ResponsesModel("gpt-5-mini").DoStreamAsync(options, CancellationToken.None));

        var expected = JsonNode.Parse("""[{"type":"mcp","server_label":"private","tunnel_id":"tunnel_0123456789abcdef0123456789abcdef","allowed_tools":{"read_only":true},"require_approval":"always"}]""");
        var body = JsonNode.Parse(capture.Body)!;
        Assert.True(JsonNode.DeepEquals(expected, body["tools"]), body["tools"]!.ToJsonString());
    }
}
