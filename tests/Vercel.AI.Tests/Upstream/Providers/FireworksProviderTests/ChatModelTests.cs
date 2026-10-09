// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.FireworksProviderTests;

/// <summary>Port of <c>fireworks-provider.test.ts</c> &gt; <c>FireworksProvider &gt; chatModel</c>.</summary>
public sealed class ChatModelTests
{
    [Fact]
    [UpstreamTest("packages/fireworks/src/fireworks-provider.test.ts::FireworksProvider > chatModel::should map max reasoning effort to high", Coverage = UpstreamCoverage.Covered)]
    public void Should_map_max_reasoning_effort_to_high()
    {
        var body = new JsonObject
        {
            ["model"] = "test-model",
            ["messages"] = new JsonArray(),
            ["reasoning_effort"] = "max",
        };

        var provider = Vercel.AI.Fireworks.FireworksProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" });

        var transformed = provider.Options.TransformRequestBody!(body, new List<CallWarning>());

        Assert.Equal("{\"model\":\"test-model\",\"messages\":[],\"reasoning_effort\":\"high\"}", transformed.ToJsonString());
    }
}
