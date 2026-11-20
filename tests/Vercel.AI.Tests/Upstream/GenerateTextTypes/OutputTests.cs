// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateTextTypes;

// C# has no test-d.ts. Each method compiles the typed property that the upstream type test checks, and reads it.
// The catalog lists only *.test.ts files, so these methods carry no UpstreamTest attribute.
// generate-text.test-d.ts > generateText types > output
public sealed class OutputTests
{
    // generate-text.test-d.ts > generateText types > output > should infer text output type
    [Fact]
    public async Task Should_infer_text_output_type()
    {
        var result = await Generate(OutputSpec.Text(), "Hello, world!");

        string? output = result.Output;

        Assert.Equal("Hello, world!", output);
    }

    // generate-text.test-d.ts > generateText types > output > should infer object output type
    [Fact]
    public async Task Should_infer_object_output_type()
    {
        var result = await Generate(
            OutputSpec.Object<Dictionary<string, string>>(
                """{"type":"object","properties":{"value":{"type":"string"}},"required":["value"]}"""
            ),
            """{"value":"a"}"""
        );

        Dictionary<string, string>? output = result.Output;

        Assert.Equal("a", output!["value"]);
    }

    // generate-text.test-d.ts > generateText types > output > should infer array output type
    [Fact]
    public async Task Should_infer_array_output_type()
    {
        var result = await Generate(
            OutputSpec.Array<string>("""{"type":"string"}""", minItems: 1, maxItems: 3),
            """{"elements":["a","b"]}"""
        );

        IReadOnlyList<string>? output = result.Output;

        Assert.Equal(new[] { "a", "b" }, output);
    }

    // generate-text.test-d.ts > generateText types > output > should infer choice output type
    [Fact]
    public async Task Should_infer_choice_output_type()
    {
        var result = await Generate(
            OutputSpec.Choice(new[] { "a", "b", "c" }),
            """{"result":"b"}"""
        );

        string? output = result.Output;

        Assert.Equal("b", output);
    }

    // generate-text.test-d.ts > generateText types > output > should infer json output type
    [Fact]
    public async Task Should_infer_json_output_type()
    {
        var result = await Generate(OutputSpec.Json(), """{"a":1}""");

        JsonElement output = result.Output;

        Assert.Equal(1, output.GetProperty("a").GetInt32());
    }

    private static Task<GenerateTextResult<TOutput>> Generate<TOutput>(
        OutputSpec<TOutput> output,
        string text
    )
    {
        return new AiClient(
            GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })
        ).GenerateTextAsync(
            new GenerateTextOptions
            {
                Model = new TestLanguageModel
                {
                    OnGenerate = _ => new LanguageModelGenerateResult(
                        new GeneratedContent[] { new GeneratedText(text) },
                        FinishReason.Stop,
                        new LanguageModelUsage(3, 10, 13)
                    ),
                },
                Prompt = "Hello, world!",
            },
            output
        );
    }
}
