// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.StreamTextTypes;

// C# has no test-d.ts. Each method compiles the typed property that the upstream type test checks, and reads it.
// The catalog lists only *.test.ts files, so these methods carry no UpstreamTest attribute.
// stream-text.test-d.ts > streamText types > result shape
public sealed class ResultShapeTests
{
    // stream-text.test-d.ts > streamText types > result shape > should infer structured output for reusable options
    [Fact]
    public async Task Should_infer_structured_output_for_reusable_options()
    {
        var options = new StreamTextOptions
        {
            Model = new TestLanguageModel
            {
                StreamParts = new LanguageModelStreamPart[]
                {
                    new TextStartStreamPart("1"),
                    new TextDeltaStreamPart("1", """{"value":"a"}"""),
                    new TextEndStreamPart("1"),
                    new FinishStreamPart(
                        FinishReason.Stop,
                        new LanguageModelUsage(3, 10, 13),
                        "stop"
                    ),
                },
            },
            Prompt = "Hello",
        };

        var result = new AiClient(
            GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })
        ).StreamTextAsync(
            options,
            OutputSpec.Object<Dictionary<string, string>>(
                """{"type":"object","properties":{"value":{"type":"string"}},"required":["value"]}"""
            )
        );

        Task<Dictionary<string, string>?> output = result.Output;

        Assert.Equal("a", (await output)!["value"]);
        Assert.Null(options.Output);
    }
}
