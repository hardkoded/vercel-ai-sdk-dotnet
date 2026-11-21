// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; options.output &gt; text output</c>.</summary>
public sealed class TextOutputTests
{
    private const string Id =
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.output > text output::";

    [Fact]
    [UpstreamTest(Id + "should forward text as output", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_forward_text_as_output()
    {
        var result = await Generate(new TestLanguageModel { OnGenerate = _ => HelloWorld() });

        Assert.Equal("Hello, world!", result.Output);
    }

    [Fact]
    [UpstreamTest(
        Id + "should set responseFormat to text and not change the prompt",
        Coverage = UpstreamCoverage.Partial,
        Note = "Asserts the response format, the tools and that the call is made once. The snapshot of every other call option is not ported."
    )]
    public async Task Should_set_responseFormat_to_text_and_not_change_the_prompt()
    {
        var model = new TestLanguageModel { OnGenerate = _ => HelloWorld() };

        await Generate(model);

        var call = Assert.Single(model.Calls);
        Assert.False(call.JsonResponse);
        Assert.Null(call.JsonSchema);
        Assert.Null(call.Tools);
    }

    private static Task<GenerateTextResult<string>> Generate(TestLanguageModel model)
    {
        return new AiClient(
            Vercel.AI.Gateway.GatewayProvider.Create(
                new Vercel.AI.Gateway.GatewayOptions { ApiKey = "test" }
            )
        ).GenerateTextAsync(
            new GenerateTextOptions
            {
                Model = model,
                Prompt = "prompt",
                Output = OutputSpec.Text(),
            }
        );
    }

    private static LanguageModelGenerateResult HelloWorld()
    {
        return new LanguageModelGenerateResult(
            new GeneratedContent[] { new GeneratedText("Hello, world!") },
            FinishReason.Stop,
            new LanguageModelUsage(3, 10, 13)
        );
    }
}
