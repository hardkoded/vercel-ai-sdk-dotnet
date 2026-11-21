// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockNativeStructuredOutput.ConverseHarness;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockNativeStructuredOutput;

public sealed class Claude5StructuredOutputsThroughConverseTests
{
    private const string File =
        "packages/amazon-bedrock/src/amazon-bedrock-native-structured-output.test.ts::";
    private const string Prefix = File + "Claude 5 structured outputs through Converse::";

    [Theory]
    [InlineData("anthropic.claude-opus-5")]
    [InlineData("anthropic.claude-opus-5-5")]
    [InlineData("anthropic.claude-sonnet-5")]
    [InlineData("anthropic.claude-sonnet-5-5")]
    [InlineData("anthropic.claude-haiku-5-5")]
    [UpstreamTest(
        Prefix + "uses native output alongside user tools for %s in auto mode",
        Coverage = UpstreamCoverage.Covered,
        Commit = "2cd0a97424762b59d9979b1bf2f55060bf443fb2"
    )]
    public async Task Uses_native_output_alongside_user_tools_in_auto_mode(string modelId)
    {
        var options = Call(Schema);
        options.Tools = [new LanguageModelTool("lookup", null, Json(Schema))];

        var body = await Send(modelId, options);

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse("{\"type\":\"json_schema\",\"schema\":" + Schema + "}"),
                body["additionalModelRequestFields"]!["output_config"]!["format"]
            )
        );
        var tool = Assert.Single(body["toolConfig"]!["tools"]!.AsArray())!;
        Assert.Equal("lookup", tool["toolSpec"]!["name"]!.GetValue<string>());
    }
}
