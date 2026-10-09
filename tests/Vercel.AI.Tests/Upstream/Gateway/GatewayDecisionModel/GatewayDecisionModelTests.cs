// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayDecisionModel.GatewayDecisionModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayDecisionModel;

/// <summary>Port of <c>gateway-decision-model.test.ts</c> &gt; <c>GatewayDecisionModel</c>.</summary>
public sealed class GatewayDecisionModelTests
{
    private const string Prefix = "packages/gateway/src/gateway-decision-model.test.ts::GatewayDecisionModel::";

    [Fact]
    [UpstreamTest(Prefix + "should report all question types as supported", Coverage = UpstreamCoverage.Covered)]
    public void Should_report_all_question_types_as_supported()
    {
        Assert.Equal(new[] { "choice", "score", "boolean" }, Setup().Model.SupportedQuestionTypes);
    }

    public static TheoryData<string, object?, object?> LegacyStates()
    {
        return new TheoryData<string, object?, object?>
        {
            { "text", new List<object?> { Text("Inspect.") }, "Inspect." },
            { "object", new List<object?> { Json(new Dictionary<string, object?> { ["product"] = "vase", ["history"] = new List<object?> { "new" } }) }, new Dictionary<string, object?> { ["product"] = "vase", ["history"] = new List<object?> { "new" } } },
            { "JSON array", new List<object?> { Json(new List<object?> { "vase", null }) }, new List<object?> { "vase", null } },
            { "empty parts", new List<object?>(), new List<object?>() },
            { "mixed parts", new List<object?> { Text("Inspect."), Json(new Dictionary<string, object?> { ["product"] = "vase" }), Json(new List<object?> { 1, null }) }, new List<object?> { "Inspect.", new Dictionary<string, object?> { ["product"] = "vase" }, new List<object?> { 1, null } } },
            { "JSON number", new List<object?> { Json(1) }, new List<object?> { 1 } },
            { "JSON null", new List<object?> { Json(null) }, new List<object?> { null } },
        };
    }

    [Theory]
    [MemberData(nameof(LegacyStates))]
    [UpstreamTest(Prefix + "sends $name using the legacy Gateway state format", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_name_using_the_legacy_Gateway_state_format(string name, object? state, object? expected)
    {
        var (model, capture) = Setup();
        await model.DoDecideAsync(Call(state: state));
        var body = RequestBody(capture)!;
        Assert.Equal(System.Text.Json.JsonSerializer.Serialize(expected), body["state"]?.ToJsonString() ?? "null");
        Assert.Equal(2, body.AsObject().Count);
        Assert.NotNull(name);
    }

    [Theory]
    [InlineData("image/png")]
    [InlineData("application/pdf")]
    [UpstreamTest(Prefix + "rejects %s before sending a Gateway request", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_before_sending_a_Gateway_request(string mediaType)
    {
        var (model, capture) = Setup();
        var state = new List<object?>
        {
            new Dictionary<string, object?> { ["type"] = "file", ["mediaType"] = mediaType, ["data"] = new Dictionary<string, object?> { ["type"] = "data", ["data"] = new byte[] { 1, 2, 3 } } },
        };
        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => model.DoDecideAsync(Call(state: state)));
        Assert.Equal("Gateway decision file input", error.Functionality);
        Assert.Empty(capture.Body);
    }

    private static Dictionary<string, object?> Text(string text)
    {
        return new Dictionary<string, object?> { ["type"] = "text", ["text"] = text };
    }

    private static Dictionary<string, object?> Json(object? value)
    {
        return new Dictionary<string, object?> { ["type"] = "json", ["value"] = value };
    }
}
