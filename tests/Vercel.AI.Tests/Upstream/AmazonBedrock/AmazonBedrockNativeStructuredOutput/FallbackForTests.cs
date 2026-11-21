// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockNativeStructuredOutput.ConverseHarness;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockNativeStructuredOutput;

public sealed class FallbackForTests
{
    private const string File =
        "packages/amazon-bedrock/src/amazon-bedrock-native-structured-output.test.ts::";
    private const string FallbackPrefix =
        File + "Claude 5 structured outputs through Converse > fallback for %s::";

    public static TheoryData<string, string> FallbackSchemas =>
        new()
        {
            { "an open object", OpenObject },
            { "a recursive reference", RecursiveReference },
            { "too many optional properties", ManyOptionalProperties() },
        };

    [Theory]
    [MemberData(nameof(FallbackSchemas))]
    [UpstreamTest(
        FallbackPrefix + "preserves the JSON tool on models that accept forced tool use",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_the_JSON_tool_on_models_that_accept_forced_tool_use(
        string name,
        string schema
    )
    {
        Assert.NotEmpty(name);

        var body = await Send("anthropic.claude-opus-5", Call(schema));

        Assert.Null(body["additionalModelRequestFields"]?["output_config"]);
        Assert.Equal("{\"any\":{}}", body["toolConfig"]!["toolChoice"]!.ToJsonString());
        var spec = body["toolConfig"]!["tools"]![0]!["toolSpec"]!;
        Assert.Equal("json", spec["name"]!.GetValue<string>());
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(schema), spec["inputSchema"]!["json"]));
    }

    [Theory]
    [MemberData(nameof(FallbackSchemas))]
    [UpstreamTest(
        FallbackPrefix + "preserves JSON instructions on models that reject forced tool use",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_JSON_instructions_on_models_that_reject_forced_tool_use(
        string name,
        string schema
    )
    {
        Assert.NotEmpty(name);

        var body = await Send("anthropic.claude-sonnet-5-5", Call(schema));

        Assert.Null(body["additionalModelRequestFields"]?["output_config"]);
        Assert.Null(body["toolConfig"]);
        Assert.Contains(
            JsonNode.Parse(schema)!.ToJsonString(),
            body["system"]![0]!["text"]!.GetValue<string>()
        );
    }
}
