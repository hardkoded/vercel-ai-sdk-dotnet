// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.GoogleToolChoiceNone;

/// <summary>Port of <c>google-tool-choice-none.test.ts</c> &gt; <c>toolChoice none with structured output</c>.</summary>
public sealed class ToolChoiceNoneWithStructuredOutputTests
{
    public static IEnumerable<object[]> Cases()
    {
        foreach (var provider in GoogleToolChoiceNoneSupport.Providers)
        {
            foreach (var modelId in GoogleToolChoiceNoneSupport.Models)
            {
                foreach (var mode in GoogleToolChoiceNoneSupport.Modes)
                {
                    yield return new object[] { provider, modelId, mode };
                }
            }
        }
    }

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest("packages/google/src/google-tool-choice-none.test.ts::toolChoice none with structured output::$mode with $provider on $modelId", Coverage = UpstreamCoverage.Covered, Commit = "32dc1d2d173f21bc1fda20f750400627fa5abb98")]
    public async Task Mode_with_provider_on_model(string provider, string modelId, string mode)
    {
        var usesResponseTool = modelId == "gemini-2.5-flash";
        var parts = usesResponseTool
            ? new JsonArray { new JsonObject { ["functionCall"] = new JsonObject { ["name"] = "json", ["args"] = new JsonObject { ["message"] = "Hello." } } } }
            : new JsonArray { new JsonObject { ["text"] = "{\"message\":\"Hello.\"}" } };
        var handler = GoogleToolChoiceNoneSupport.Handler(mode, parts);
        var model = GoogleToolChoiceNoneSupport.Model(provider, modelId, handler);
        var options = GoogleToolChoiceNoneSupport.Options(provider, true, ToolChoice.None);
        options.JsonSchema = GoogleUpstream.Element("{\"type\":\"object\",\"properties\":{\"message\":{\"type\":\"string\"}},\"required\":[\"message\"],\"additionalProperties\":false}");

        if (mode == "generate")
        {
            var result = await model.DoGenerateAsync(options, CancellationToken.None);
            Assert.Contains(result.Content, part => part is GeneratedText text && text.Text == "{\"message\":\"Hello.\"}");
        }
        else
        {
            var streamed = await GoogleUpstream.Collect(await model.DoStreamAsync(options, CancellationToken.None));
            Assert.Equal("{\"message\":\"Hello.\"}", string.Concat(streamed.OfType<TextDeltaStreamPart>().Select(part => part.Delta)));
        }

        Assert.Equal(1, handler.Calls);
        var body = JsonNode.Parse(handler.Body)!.AsObject();
        var tools = body["tools"]!.AsArray();
        Assert.Single(tools);
        var names = tools[0]!["functionDeclarations"]!.AsArray().Select(tool => tool!["name"]!.GetValue<string>()).ToArray();
        Assert.Equal(usesResponseTool ? new[] { "lookup", "json" } : new[] { "lookup" }, names);
        GoogleUpstream.JsonEqual(
            body["toolConfig"],
            usesResponseTool
                ? "{\"functionCallingConfig\":{\"mode\":\"ANY\",\"allowedFunctionNames\":[\"json\"]}}"
                : "{\"functionCallingConfig\":{\"mode\":\"NONE\"}}");
    }
}
