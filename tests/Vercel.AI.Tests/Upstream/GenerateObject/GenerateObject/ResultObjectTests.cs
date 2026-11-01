// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateObject;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.GenerateObject.GenerateObject;

/// <summary>Port of <c>generate-object.test.ts</c> &gt; <c>generateObject &gt; output = "object" &gt; result.object</c>.</summary>
[Collection("LogWarnings")]
public sealed class ResultObjectTests
{
    private const string Id = "packages/ai/src/generate-object/generate-object.test.ts::generateObject > output = \"object\" > result.object::";
    private const string Schema = "{\"type\":\"object\",\"properties\":{\"content\":{\"type\":\"string\"}},\"required\":[\"content\"],\"additionalProperties\":false}";

    private static AiClient Client() => new(Vercel.AI.Gateway.GatewayProvider.Create(new Vercel.AI.Gateway.GatewayOptions { ApiKey = "test" }));

    private sealed class ContentResult
    {
        public string Content { get; set; } = string.Empty;
    }

    [Fact]
    [UpstreamTest(Id + "should parse structured output text while preserving raw callback text and response metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_parse_structured_output_text_while_preserving_raw_callback_text_and_response_metadata()
    {
        const string draft = "{\"content\":\"draft\"}";
        const string finalAnswer = "{\"content\":\"final\"}";
        const string responseBody = "{\"output\":[\"{\\\"content\\\":\\\"draft\\\"}\",\"{\\\"content\\\":\\\"final\\\"}\"]}";
        GenerateObjectStepEndEvent? stepEnd = null;

        var result = await Client().GenerateObjectAsync(new GenerateObjectOptions<ContentResult>
        {
            Model = new TestLanguageModel
            {
                OnGenerate = _ => new LanguageModelGenerateResult(
                    new GeneratedContent[]
                    {
                        new GeneratedReasoning("Checking both answers."),
                        new GeneratedText(draft),
                        new GeneratedText(finalAnswer),
                    },
                    FinishReason.Stop,
                    new LanguageModelUsage(10, 20, 30),
                    rawResponse: responseBody,
                    responseId: "id-1",
                    responseModelId: "m-1",
                    responseTimestamp: DateTimeOffset.FromUnixTimeMilliseconds(123),
                    structuredOutputText: finalAnswer),
            },
            Schema = Schema,
            Prompt = "prompt",
            SerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
            OnStepEnd = (@event, _) =>
            {
                stepEnd = @event;
                return Task.CompletedTask;
            },
        });

        Assert.Equal("final", result.Object.Content);
        Assert.Equal("Checking both answers.", result.Reasoning);
        Assert.Equal(responseBody, result.Response.Body!.Value.GetRawText());
        Assert.Equal(30, result.Usage.TotalTokens);
        Assert.Equal(draft + finalAnswer, stepEnd!.ObjectText);
        Assert.Equal("Checking both answers.", stepEnd.Reasoning);
        Assert.Same(result.Response, stepEnd.Response);
        Assert.Equal(result.Usage.InputTokens, stepEnd.Usage.InputTokens);
        Assert.Equal(result.Usage.OutputTokens, stepEnd.Usage.OutputTokens);
        Assert.Equal(result.Usage.TotalTokens, stepEnd.Usage.TotalTokens);
    }

    [Fact]
    [UpstreamTest(Id + "should generate object", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_generate_object()
    {
        var warnings = new List<LogWarningsOptions>();
        var previous = LogWarnings.Logger;
        LogWarnings.Logger = new Action<LogWarningsOptions>(warnings.Add);
        try
        {
            var model = new TestLanguageModel
            {
                OnGenerate = _ => new LanguageModelGenerateResult(
                    new GeneratedContent[] { new GeneratedText("{ \"content\": \"Hello, world!\" }") },
                    FinishReason.Stop,
                    new LanguageModelUsage(10, 20, 30)),
            };
            var result = await Client().GenerateObjectAsync(new GenerateObjectOptions<ContentResult>
            {
                Model = model,
                Schema = Schema,
                Prompt = "prompt",
                SerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
            });

            Assert.Equal("Hello, world!", result.Object.Content);
            var prompt = Assert.IsType<UserModelMessage>(Assert.Single(model.Calls[0].Prompt));
            Assert.Equal("prompt", Assert.IsType<TextContentPart>(Assert.Single(prompt.Content)).Text);
            Assert.Equal(Schema, model.Calls[0].JsonSchema!.Value.GetRawText());
            var deprecated = Assert.IsType<DeprecatedWarning>(Assert.Single(Assert.Single(warnings).Warnings));
            Assert.Equal("generateObject", deprecated.Setting);
            Assert.Equal("Use generateText with an output setting instead.", deprecated.Message);
        }
        finally
        {
            LogWarnings.Logger = previous;
        }
    }

    [Fact]
    [UpstreamTest(Id + "should use name and description", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_name_and_description()
    {
        var model = new TestLanguageModel
        {
            OnGenerate = call =>
            {
                Assert.Equal("test-name", call.JsonSchemaName);
                Assert.Equal("test description", call.JsonSchemaDescription);
                Assert.Equal(Schema, call.JsonSchema!.Value.GetRawText());
                var prompt = Assert.IsType<UserModelMessage>(Assert.Single(call.Prompt));
                Assert.Equal("prompt", Assert.IsType<TextContentPart>(Assert.Single(prompt.Content)).Text);
                return new LanguageModelGenerateResult(
                    new GeneratedContent[] { new GeneratedText("{ \"content\": \"Hello, world!\" }") },
                    FinishReason.Stop,
                    new LanguageModelUsage(10, 20, 30));
            },
        };

        var result = await Client().GenerateObjectAsync(new GenerateObjectOptions<ContentResult>
        {
            Model = model,
            Schema = Schema,
            SchemaName = "test-name",
            SchemaDescription = "test description",
            Prompt = "prompt",
            SerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
        });

        Assert.Equal("Hello, world!", result.Object.Content);
    }
}
