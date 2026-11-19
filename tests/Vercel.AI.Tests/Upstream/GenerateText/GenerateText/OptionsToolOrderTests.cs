// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText</c> &gt; <c>options.toolOrder</c>.</summary>
public sealed class OptionsToolOrderTests
{
    private const string Schema =
        "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}";

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.toolOrder::should order available tools before sending them to the model",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_order_available_tools_before_sending_them_to_the_model()
    {
        var model = new TestLanguageModel();

        await new AiClient(
            GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })
        ).GenerateTextAsync(
            new GenerateTextOptions
            {
                Model = model,
                Tools =
                [
                    Tool.Function("zebra", null, Schema, (_, _) => Task.FromResult("\"zebra\"")),
                    Tool.Function("alpha", null, Schema, (_, _) => Task.FromResult("\"alpha\"")),
                    Tool.Function("middle", null, Schema, (_, _) => Task.FromResult("\"middle\"")),
                ],
                Prompt = "test-input",
                ToolOrder = ["middle"],
            }
        );

        Assert.Equal(
            new[] { "middle", "alpha", "zebra" },
            model.Calls[0].Tools!.Select(tool => tool.Name)
        );
    }
}
