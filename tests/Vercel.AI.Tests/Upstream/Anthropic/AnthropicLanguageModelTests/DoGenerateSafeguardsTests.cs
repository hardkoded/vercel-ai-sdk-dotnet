// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; safeguards</c>.</summary>
public sealed class DoGenerateSafeguardsTests
{
    private const string Prefix = "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > safeguards::";

    [Fact]
    [UpstreamTest(Prefix + "should send safeguards in request body and add the beta", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_safeguards_in_request_body_and_add_the_beta()
    {
        var handler = AnthropicParity.Ok();
        var options = AnthropicParity.WithProvider(
            "{\"safeguards\":[{\"type\":\"dangerous_tool_use\",\"classifierContext\":{\"v\":1,\"permission_mode\":\"auto\"}}]}");

        await AnthropicParity.Generate(AnthropicParity.Client(handler), "claude-3-haiku-20240307", options);

        using var body = JsonDocument.Parse(handler.Body);
        JsonAssert.Equal(
            body.RootElement.GetProperty("safeguards"),
            "[{\"type\":\"dangerous_tool_use\",\"classifier_context\":{\"v\":1,\"permission_mode\":\"auto\"}}]");
        Assert.Contains("dangerous-tool-use-2026-09-03", handler.Headers["anthropic-beta"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not send safeguards or the beta when the option is absent", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_send_safeguards_or_the_beta_when_the_option_is_absent()
    {
        var handler = AnthropicParity.Ok();

        await AnthropicParity.Generate(AnthropicParity.Client(handler), "claude-3-haiku-20240307");

        using var body = JsonDocument.Parse(handler.Body);
        Assert.False(body.RootElement.TryGetProperty("safeguards", out _));
        Assert.DoesNotContain(
            "dangerous-tool-use",
            handler.Headers.TryGetValue("anthropic-beta", out var beta) ? beta : string.Empty);
    }

    [Fact]
    [UpstreamTest(Prefix + "should expose safeguard_results as provider metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_expose_safeguard_results_as_provider_metadata()
    {
        var handler = AnthropicParity.Ok(
            "{\"id\":\"msg_123\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[{\"type\":\"tool_use\",\"id\":\"toolu_01\",\"name\":\"Bash\",\"input\":{\"command\":\"echo hello\"}}],\"model\":\"claude-3-haiku-20240307\",\"stop_reason\":\"tool_use\",\"stop_sequence\":null,\"usage\":{\"input_tokens\":100,\"output_tokens\":50},\"safeguard_results\":[{\"type\":\"dangerous_tool_use\",\"status\":{\"type\":\"available\",\"tool_uses\":{\"toolu_01\":{\"type\":\"evaluated\",\"outcome\":\"flagged\",\"explanation\":\"[Data Exfiltration]\"}}}}]}");

        var result = await AnthropicParity.Generate(AnthropicParity.Client(handler), "claude-3-haiku-20240307");

        JsonAssert.Equal(
            result.ProviderMetadata!.Value.GetProperty("anthropic").GetProperty("safeguardResults"),
            "[{\"type\":\"dangerous_tool_use\",\"status\":{\"type\":\"available\",\"tool_uses\":{\"toolu_01\":{\"type\":\"evaluated\",\"outcome\":\"flagged\",\"explanation\":\"[Data Exfiltration]\"}}}}]");
    }

    [Fact]
    [UpstreamTest(Prefix + "should omit safeguardResults when the response has none", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_omit_safeguard_results_when_the_response_has_none()
    {
        var result = await AnthropicParity.Generate(
            AnthropicParity.Client(AnthropicParity.Ok()),
            "claude-3-haiku-20240307");

        Assert.False(result.ProviderMetadata!.Value.GetProperty("anthropic").TryGetProperty("safeguardResults", out _));
    }
}
