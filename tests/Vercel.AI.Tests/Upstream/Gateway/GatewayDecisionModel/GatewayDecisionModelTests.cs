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

    public static TheoryData<string, List<object?>> StatePartsCases()
    {
        return new TheoryData<string, List<object?>>
        {
            { "a text part", new List<object?> { Text("Inspect.") } },
            { "a JSON object part", new List<object?> { Json(new Dictionary<string, object?> { ["product"] = "vase", ["history"] = new List<object?> { "new" } }) } },
            { "a JSON array part", new List<object?> { Json(new List<object?> { "vase", null }) } },
            { "empty parts", new List<object?>() },
            { "mixed parts", new List<object?> { Text("Inspect."), Json(new Dictionary<string, object?> { ["product"] = "vase" }), Json(new List<object?> { 1, null }) } },
            { "a JSON number", new List<object?> { Json(1) } },
            { "JSON null", new List<object?> { Json(null) } },
        };
    }

    [Theory]
    [MemberData(nameof(StatePartsCases))]
    [UpstreamTest(Prefix + "sends $name as ordered state parts", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_name_as_ordered_state_parts(string name, List<object?> state)
    {
        Assert.NotEmpty(name);
        var (model, capture) = Setup();
        await model.DoDecideAsync(Call(null, state: state));
        JsonAssert.Equal(RequestBody(capture), "{\"stateParts\":" + System.Text.Json.JsonSerializer.Serialize(state) + ",\"questions\":" + QuestionsJson + "}");
    }

    [Fact]
    [UpstreamTest(Prefix + "sends file bytes as base64 in ordered state parts", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_file_bytes_as_base64_in_ordered_state_parts()
    {
        var (model, capture) = Setup();
        var state = new List<object?>
        {
            Text("Inspect this package."),
            File("image/png", new byte[] { 1, 2, 3 }, "box.png"),
            Json(new Dictionary<string, object?> { ["product"] = "vase" }),
        };
        await model.DoDecideAsync(Call(null, state: state));
        JsonAssert.Equal(
            RequestBody(capture),
            """
            {
              "stateParts": [
                { "type": "text", "text": "Inspect this package." },
                { "type": "file", "mediaType": "image/png", "filename": "box.png", "data": { "type": "data", "data": "AQID" } },
                { "type": "json", "value": { "product": "vase" } }
              ],
              "questions":
            """ + QuestionsJson + "}");
    }

    [Fact]
    [UpstreamTest(Prefix + "sends base64 file strings unchanged", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_base64_file_strings_unchanged()
    {
        var (model, capture) = Setup();
        await model.DoDecideAsync(Call(null, state: new List<object?> { File("image/jpeg", "AQID", null) }));
        JsonAssert.Equal(
            RequestBody(capture),
            """
            {
              "stateParts": [{ "type": "file", "mediaType": "image/jpeg", "data": { "type": "data", "data": "AQID" } }],
              "questions":
            """ + QuestionsJson + "}");
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects URL file data before sending a Gateway request", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_URL_file_data_before_sending_a_Gateway_request()
    {
        var (model, capture) = Setup();
        var state = new List<object?>
        {
            new Dictionary<string, object?>
            {
                ["type"] = "file",
                ["mediaType"] = "image/png",
                ["data"] = new Dictionary<string, object?> { ["type"] = "url", ["url"] = "https://example.com/a.png" },
            },
        };
        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => model.DoDecideAsync(Call(null, state: state)));
        Assert.Equal("Gateway decision file input: url data", error.Functionality);
        Assert.Equal(0, capture.Calls);
    }

    private static Dictionary<string, object?> File(string mediaType, object data, string? filename)
    {
        var part = new Dictionary<string, object?>
        {
            ["type"] = "file",
            ["mediaType"] = mediaType,
            ["data"] = new Dictionary<string, object?> { ["type"] = "data", ["data"] = data },
        };
        if (filename != null)
        {
            part["filename"] = filename;
        }

        return part;
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
