// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayToolError;

/// <summary>Port of <c>gateway-tool-error.test.ts</c> &gt; <c>Gateway tool errors</c>. Only the DeepSeek tools exist in the port.</summary>
public sealed class GatewayToolErrorTests
{
    private const string Json = """
        {
          "error": "gateway_rate_limit",
          "message": "Retry later",
          "statusCode": 429,
          "retryAfter": "60",
          "statusText": "Too Many Requests",
          "future_field": true
        }
        """;

    [Theory]
    [InlineData("deepseekFetch")]
    [InlineData("deepseekSearch")]
    [UpstreamTest(
        "packages/gateway/src/tool/gateway-tool-error.test.ts::Gateway tool errors::%s preserves structured errors and additive fields",
        Coverage = UpstreamCoverage.Partial,
        Note = "Only deepseekFetch and deepseekSearch are ported. parallelSearch is not ported; takoSearch has its own tests.",
        Commit = "2099d455e449b77778c9648a754e8c27b1a74829"
    )]
    public void Preserves_structured_errors_and_additive_fields(string name)
    {
        var schema =
            name == "deepseekFetch"
                ? GatewayDeepSeekFetch.OutputSchema
                : GatewayDeepSeekSearch.OutputSchema;
        using var document = JsonDocument.Parse(Json);
        Assert.Empty(ToolInputSchemaValidator.Validate(schema, document.RootElement));

        var error =
            name == "deepseekFetch"
                ? JsonSerializer.Deserialize<DeepSeekFetchOutput>(Json)!.Error
                : JsonSerializer.Deserialize<DeepSeekSearchOutput>(Json)!.Error;
        Assert.NotNull(error);
        Assert.Equal(429, error.StatusCode);
        Assert.True(error.AdditionalProperties!["future_field"].GetBoolean());
        var roundTrip = JsonSerializer.Serialize(error);
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(Json), JsonNode.Parse(roundTrip)));
    }
}
