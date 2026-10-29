// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateObject;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; options.output &gt; object output</c>.</summary>
public sealed class ObjectOutputTests
{
    private const string Id = "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.output > object output::";
    private const string Schema = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false}";

    private static AiClient Client() => new(Vercel.AI.Gateway.GatewayProvider.Create(new Vercel.AI.Gateway.GatewayOptions { ApiKey = "test" }));

    private static LanguageModelGenerateResult Result(string text, FinishReason finishReason = FinishReason.Stop) =>
        new(
            new GeneratedContent[] { new GeneratedText(text) },
            finishReason,
            new LanguageModelUsage(3, 10, 13));

    [Fact]
    [UpstreamTest(Id + "should parse the output", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_parse_the_output()
    {
        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = new TestLanguageModel { OnGenerate = _ => Result("""{ "value": "test-value" }""") },
            Prompt = "prompt",
            Output = OutputSpec.Object(Schema),
        });

        Assert.Equal("test-value", result.Output!.Value.GetProperty("value").GetString());
    }

    [Fact]
    [UpstreamTest(Id + "should expose parse diagnostics when output is truncated", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_expose_parse_diagnostics_when_output_is_truncated()
    {
        const string truncatedText = "{\"value\":\"test";
        var error = await Assert.ThrowsAsync<NoObjectGeneratedException>(() => Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = new TestLanguageModel { OnGenerate = _ => Result(truncatedText, FinishReason.Length) },
            Prompt = "prompt",
            Output = OutputSpec.Object(Schema),
        }));

        Assert.True(NoObjectGeneratedException.IsInstance(error));
        Assert.Equal("No object generated: could not parse the response.", error.Message);
        Assert.IsType<Vercel.AI.ProviderUtils.JsonParseException>(error.Cause);
        Assert.Equal(truncatedText, error.Text);
        Assert.Equal(FinishReason.Length, error.FinishReason);
        Assert.Equal(10, error.Usage.OutputTokens);
    }

    [Fact]
    [UpstreamTest(
        Id + "should parse the output when the finish reason is missing",
        Coverage = UpstreamCoverage.Covered,
        Note = "FinishReason is a non-nullable enum in the port; Other is its representation of a missing or unknown reason.")]
    public async Task Should_parse_the_output_when_the_finish_reason_is_missing()
    {
        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = new TestLanguageModel { OnGenerate = _ => Result("""{ "value": "test-value" }""", FinishReason.Other) },
            Prompt = "prompt",
            Output = OutputSpec.Object(Schema),
        });

        Assert.Equal("test-value", result.Output!.Value.GetProperty("value").GetString());
    }

    [Fact]
    [UpstreamTest(Id + "should set responseFormat to json and send schema as part of the responseFormat", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_set_responseFormat_to_json_and_send_schema_as_part_of_the_responseFormat()
    {
        var model = new TestLanguageModel { OnGenerate = _ => Result("""{ "value": "test-value" }""") };

        await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "prompt",
            Output = OutputSpec.Object(Schema),
        });

        var call = Assert.Single(model.Calls);
        var prompt = Assert.IsType<UserModelMessage>(Assert.Single(call.Prompt));
        Assert.Equal("prompt", Assert.IsType<TextContentPart>(Assert.Single(prompt.Content)).Text);
        Assert.Equal(Schema, call.JsonSchema!.Value.GetRawText());
        Assert.Null(call.JsonSchemaName);
        Assert.Null(call.JsonSchemaDescription);
        Assert.Null(call.MaxOutputTokens);
        Assert.Null(call.FrequencyPenalty);
        Assert.Null(call.PresencePenalty);
        Assert.Null(call.ProviderOptions);
        Assert.Null(call.Reasoning);
        Assert.Null(call.Seed);
        Assert.Null(call.StopSequences);
        Assert.Null(call.Temperature);
        Assert.NotNull(call.ToolChoice);
        Assert.Null(call.Tools);
        Assert.Null(call.TopK);
        Assert.Null(call.TopP);
    }
}
