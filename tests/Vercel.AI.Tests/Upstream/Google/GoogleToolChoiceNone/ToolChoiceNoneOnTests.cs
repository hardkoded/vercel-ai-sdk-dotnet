// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.GoogleToolChoiceNone;

/// <summary>Port of <c>google-tool-choice-none.test.ts</c> &gt; <c>toolChoice none on %s</c>.</summary>
public sealed class ToolChoiceNoneOnTests
{
    public static IEnumerable<object[]> Cases()
    {
        foreach (var modelId in GoogleToolChoiceNoneSupport.Models)
        {
            foreach (var provider in GoogleToolChoiceNoneSupport.Providers)
            {
                foreach (var mixed in new[] { false, true })
                {
                    foreach (var mode in GoogleToolChoiceNoneSupport.Modes)
                    {
                        foreach (var choice in new[] { "none", "auto" })
                        {
                            yield return new object[] { modelId, provider, mixed, mode, choice };
                        }
                    }
                }
            }
        }
    }

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest("packages/google/src/google-tool-choice-none.test.ts::toolChoice none on %s::$mode with $provider, mixed=$mixed and toolChoice=$toolChoice", Coverage = UpstreamCoverage.Covered, Commit = "32dc1d2d173f21bc1fda20f750400627fa5abb98")]
    public async Task Mode_with_provider_mixed_and_toolChoice(string modelId, string provider, bool mixed, string mode, string toolChoice)
    {
        var handler = GoogleToolChoiceNoneSupport.Handler(mode, new JsonArray { new JsonObject { ["text"] = "Hello." } });
        var model = GoogleToolChoiceNoneSupport.Model(provider, modelId, handler);
        var options = GoogleToolChoiceNoneSupport.Options(provider, mixed, toolChoice == "none" ? ToolChoice.None : ToolChoice.Auto);

        if (mode == "generate")
        {
            var result = await model.DoGenerateAsync(options, CancellationToken.None);
            Assert.Contains(result.Content, part => part is GeneratedText text && text.Text == "Hello.");
        }
        else
        {
            var parts = await GoogleUpstream.Collect(await model.DoStreamAsync(options, CancellationToken.None));
            Assert.Contains(parts, part => part is TextDeltaStreamPart delta && delta.Delta == "Hello.");
        }

        Assert.Equal(1, handler.Calls);
        var body = JsonNode.Parse(handler.Body)!.AsObject();
        if (toolChoice == "auto")
        {
            Assert.Contains(body["tools"]!.AsArray(), tool => JsonNode.DeepEquals(tool, JsonNode.Parse(GoogleToolChoiceNoneSupport.ExpectedJson(provider))));
        }
        else if (mixed)
        {
            var tools = body["tools"]!.AsArray();
            Assert.Single(tools);
            var declarations = tools[0]!["functionDeclarations"]!.AsArray();
            Assert.Single(declarations);
            Assert.Equal("lookup", declarations[0]!["name"]!.GetValue<string>());
            GoogleUpstream.JsonEqual(body["toolConfig"], "{\"functionCallingConfig\":{\"mode\":\"NONE\"}}");
        }
        else
        {
            Assert.False(body.ContainsKey("tools"));
            Assert.False(body.ContainsKey("toolConfig"));
        }
    }
}
