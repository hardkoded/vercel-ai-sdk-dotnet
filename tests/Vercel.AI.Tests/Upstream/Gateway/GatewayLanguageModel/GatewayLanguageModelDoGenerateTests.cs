// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel.GatewayLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel;

/// <summary>Port of <c>gateway-language-model.test.ts</c> &gt; <c>GatewayLanguageModel &gt; doGenerate</c>.</summary>
public sealed class GatewayLanguageModelDoGenerateTests
{
    private const string Prefix = "packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doGenerate::";

    [Fact]
    [UpstreamTest(Prefix + "should include o11y headers in the request", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_o11y_headers_in_the_request()
    {
        var server = new GatewayLanguageModelServer
        {
            ContentType = "application/json",
            ResponseBody = "{\"id\":\"test-id\",\"created\":1711115037,\"model\":\"test-model\",\"content\":[{\"type\":\"text\",\"text\":\"Hello, World!\"}],\"finish_reason\":\"stop\",\"usage\":{\"prompt_tokens\":4,\"completion_tokens\":30}}",
        };
        var o11yHeaders = new Dictionary<string, string>
        {
            ["ai-o11y-deployment-id"] = "test-deployment",
            ["ai-o11y-environment"] = "production",
            ["ai-o11y-region"] = "iad1",
        };

        await CreateTestModel(server, o11yHeaders).DoGenerateAsync(TestPrompt, CancellationToken.None);

        foreach (var header in o11yHeaders)
        {
            Assert.Equal(header.Value, server.RequestHeaders[header.Key]);
        }
    }
}
