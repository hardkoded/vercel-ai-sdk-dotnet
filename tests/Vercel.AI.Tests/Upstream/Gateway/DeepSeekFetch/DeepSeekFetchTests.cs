// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Gateway.DeepSeekFetch;

/// <summary>Port of <c>deepseek-fetch.test.ts</c> &gt; <c>deepseekFetch</c>.</summary>
public sealed class DeepSeekFetchTests
{
    private const string Prefix =
        "packages/gateway/src/tool/deepseek-fetch.test.ts::deepseekFetch::";
    private const string Commit = "2099d45";

    private static IReadOnlyList<string> ValidateOutput(string json)
    {
        using var document = JsonDocument.Parse(json);
        return ToolInputSchemaValidator.Validate(
            GatewayDeepSeekFetch.OutputSchema,
            document.RootElement
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "registers a provider-executed tool with an optional pinned URL",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Registers_a_provider_executed_tool_with_an_optional_pinned_URL()
    {
        var tool = GatewayDeepSeekFetch.Create(
            new GatewayDeepSeekFetchConfig { Url = "https://example.com" }
        );

        Assert.True(tool.IsProviderDefined);
        Assert.Equal("gateway.deepseek_fetch", tool.ProviderToolId);
        Assert.Null(tool.Execute);
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse("""{ "url": "https://example.com" }"""),
                JsonNode.Parse(tool.ProviderToolArgs.GetRawText())
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "describes the required fetch URL",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Describes_the_required_fetch_URL()
    {
        var schema = GatewayDeepSeekFetch.Create().InputSchema;

        Assert.Equal("url", schema.GetProperty("required")[0].GetString());
        Assert.Equal(
            "string",
            schema.GetProperty("properties").GetProperty("url").GetProperty("type").GetString()
        );
    }

    [Theory]
    [InlineData("file:///etc/passwd")]
    [InlineData("https://user:password@example.com")]
    [InlineData("not a URL")]
    [UpstreamTest(
        Prefix + "leaves URL validation to the gateway (%s)",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Leaves_URL_validation_to_the_gateway(string url)
    {
        using var document = JsonDocument.Parse(new JsonObject { ["url"] = url }.ToJsonString());

        Assert.Empty(
            ToolInputSchemaValidator.Validate(
                GatewayDeepSeekFetch.Create().InputSchema,
                document.RootElement
            )
        );
    }

    [Theory]
    [InlineData("""{ "title": "", "content": "# Markdown" }""")]
    [InlineData("""{ "content": "# Markdown" }""")]
    [InlineData("""{ "title": "Title", "content": "" }""")]
    [InlineData("""{ "error": "timeout", "message": "Timed out" }""")]
    [UpstreamTest(
        Prefix + "accepts response %j",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Accepts_response(string json)
    {
        Assert.Empty(ValidateOutput(json));
        var output = JsonSerializer.Deserialize<DeepSeekFetchOutput>(json)!;
        Assert.True((output.Response is null) != (output.Error is null));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "rejects a response without content",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Rejects_a_response_without_content()
    {
        const string json = """{ "title": "Title" }""";

        Assert.NotEmpty(ValidateOutput(json));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<DeepSeekFetchOutput>(json));
    }
}
