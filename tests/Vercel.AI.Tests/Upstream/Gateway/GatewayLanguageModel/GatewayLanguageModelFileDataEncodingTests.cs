// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel.GatewayLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel;

/// <summary>Port of <c>gateway-language-model.test.ts</c> &gt; <c>GatewayLanguageModel &gt; file data encoding</c>.</summary>
public sealed class GatewayLanguageModelFileDataEncodingTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > file data encoding::";

    [Fact]
    [UpstreamTest(
        Prefix + "should base64-encode Uint8Array data in a reasoning-file part",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_base64_encode_Uint8Array_data_in_a_reasoning_file_part()
    {
        var server = PrepareJsonResponse();
        var bytes = new byte[] { 1, 2, 3, 4 };
        var expectedBase64 = Convert.ToBase64String(bytes);
        var prompt = new ModelMessage[]
        {
            new AssistantModelMessage(
                new AssistantContentPart[]
                {
                    new AssistantReasoningFilePart("image/png", null, bytes),
                }
            ),
        };

        await CreateTestModel(server)
            .DoGenerateAsync(
                new LanguageModelCallOptions { Prompt = prompt },
                CancellationToken.None
            );

        JsonAssert.Equal(
            server.RequestJson["prompt"]![0]!["content"]![0]!["data"],
            "{\"type\":\"data\",\"data\":\"" + expectedBase64 + "\"}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should base64-encode Uint8Array data in tool-result file content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_base64_encode_Uint8Array_data_in_tool_result_file_content()
    {
        var server = PrepareJsonResponse();
        var bytes = new byte[] { 5, 6, 7, 8 };
        var expectedBase64 = Convert.ToBase64String(bytes);
        var prompt = new ModelMessage[]
        {
            new ToolModelMessage(
                "call_1",
                "render",
                new UserContentPart[] { new FileContentPart("image/png", null, bytes, null) }
            ),
        };

        await CreateTestModel(server)
            .DoGenerateAsync(
                new LanguageModelCallOptions { Prompt = prompt },
                CancellationToken.None
            );

        JsonAssert.Equal(
            server.RequestJson["prompt"]![0]!["content"]![0]!["output"]!["value"]![0]!["data"],
            "{\"type\":\"data\",\"data\":\"" + expectedBase64 + "\"}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not modify reasoning-file data that is a URL",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_modify_reasoning_file_data_that_is_a_URL()
    {
        var server = PrepareJsonResponse();
        var prompt = new ModelMessage[]
        {
            new AssistantModelMessage(
                new AssistantContentPart[]
                {
                    new AssistantReasoningFilePart("image/png", "https://example.com/a.png", null),
                }
            ),
        };

        await CreateTestModel(server)
            .DoGenerateAsync(
                new LanguageModelCallOptions { Prompt = prompt },
                CancellationToken.None
            );

        JsonAssert.Equal(
            server.RequestJson["prompt"]![0]!["content"]![0]!["data"],
            "{\"type\":\"url\",\"url\":\"https://example.com/a.png\"}"
        );
    }
}
