// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.GenerateObject;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; options.output &gt; provider-selected structured output</c>.</summary>
public sealed class ProviderSelectedStructuredOutputTests
{
    private const string Id = "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.output > provider-selected structured output::";

    private const string ValueSchema = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}";

    private static AiClient Client() => new(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));

    private static LanguageModelGenerateResult Result(IReadOnlyList<GeneratedContent> content, string? structuredOutputText, FinishReason? finishReason = null) =>
        new(
            content,
            finishReason ?? FinishReason.Stop,
            new LanguageModelUsage(3, 10, 13),
            structuredOutputText: structuredOutputText);

    private static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    [Fact]
    [UpstreamTest(
        Id + "should parse the selected text while preserving content, messages, and callbacks",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_parse_the_selected_text_while_preserving_content_messages_and_callbacks()
    {
        var source = new GeneratedSource("source-1", "https://example.com", "Example");
        var draftMetadata = Json("""{"testProvider":{"itemId":"draft","phase":"commentary","annotations":[{"type":"url_citation","url":"https://example.com","title":"Example"}]}}""");
        var finalMetadata = Json("""{"testProvider":{"itemId":"final","phase":"final_answer"}}""");
        var content = new GeneratedContent[]
        {
            new GeneratedReasoning("Thinking."),
            new GeneratedText("{\"value\":\"draft\"}", new[] { new Citation(source, 0, 17) }, draftMetadata),
            source,
            new GeneratedText("{\"value\":", null, finalMetadata),
            new GeneratedText("\"final\"}", null, finalMetadata),
        };
        LanguageModelCallEndEvent? callEnd = null;
        StepResult? stepEnd = null;
        GenerateTextResult? end = null;

        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = new TestLanguageModel { OnGenerate = _ => Result(content, "{\"value\":\"final\"}") },
            Prompt = "prompt",
            Output = OutputSpec.Object(ValueSchema),
            OnLanguageModelCallEnd = (@event, _) =>
            {
                callEnd = @event;
                return Task.CompletedTask;
            },
            OnStepEnd = (step, _) =>
            {
                stepEnd = step;
                return Task.CompletedTask;
            },
            OnFinish = (finished, _) =>
            {
                end = finished;
                return Task.CompletedTask;
            },
        });

        const string expectedText = "{\"value\":\"draft\"}{\"value\":\"final\"}";
        Assert.Equal("final", result.Output!.Value.GetProperty("value").GetString());
        Assert.Equal(expectedText, result.Text);
        Assert.Equal(content, result.Content);
        Assert.Equal(new[] { source }, result.Sources);
        Assert.Equal(content, result.FinalStep.Content);
        Assert.Equal(13, result.Usage.TotalTokens);

        void AssertMessages(IReadOnlyList<ModelMessage> messages)
        {
            var message = Assert.IsType<AssistantModelMessage>(Assert.Single(messages));
            Assert.Collection(
                message.Parts,
                part => Assert.Equal("Thinking.", Assert.IsType<AssistantReasoningPart>(part).Text),
                part => AssertTextPart(part, "{\"value\":\"draft\"}", draftMetadata),
                part => AssertTextPart(part, "{\"value\":", finalMetadata),
                part => AssertTextPart(part, "\"final\"}", finalMetadata));
        }

        AssertMessages(result.Response.Messages);
        AssertMessages(result.ResponseMessages);
        Assert.Equal(content, callEnd!.Content);
        Assert.Equal(expectedText, stepEnd!.Text);
        Assert.Equal(content, stepEnd.Content);
        AssertMessages(stepEnd.Response.Messages);
        Assert.Equal(expectedText, end!.Text);
        Assert.Equal(content, end.Content);
        AssertMessages(end.ResponseMessages);
    }

    private static void AssertTextPart(AssistantContentPart part, string text, JsonElement metadata)
    {
        var textPart = Assert.IsType<AssistantTextPart>(part);
        Assert.Equal(text, textPart.Text);
        var options = Assert.IsAssignableFrom<IReadOnlyDictionary<string, JsonElement>>(textPart.ProviderOptions);
        var expected = metadata.GetProperty("testProvider");
        Assert.Equal(expected.GetRawText(), options["testProvider"].GetRawText());
    }

    [Fact]
    [UpstreamTest(Id + "should concatenate all text parts when no structured output text is supplied", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_concatenate_all_text_parts_when_no_structured_output_text_is_supplied()
    {
        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = new TestLanguageModel
            {
                OnGenerate = _ => Result(new GeneratedContent[] { new GeneratedText("{\"value\":"), new GeneratedText("\"final\"}") }, null),
            },
            Prompt = "prompt",
            Output = OutputSpec.Object(ValueSchema),
        });

        Assert.Equal("final", result.Output!.Value.GetProperty("value").GetString());
        Assert.Equal("{\"value\":\"final\"}", result.Text);
    }

    [Theory]
    [InlineData("")]
    [InlineData("{\"value\":")]
    [UpstreamTest(
        Id + "should reject invalid selected text %j without falling back to valid content",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_reject_invalid_selected_text_without_falling_back_to_valid_content(string structuredOutputText)
    {
        var error = await Assert.ThrowsAsync<NoObjectGeneratedException>(() => Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = new TestLanguageModel
            {
                OnGenerate = _ => Result(new GeneratedContent[] { new GeneratedText("{\"value\":\"draft\"}") }, structuredOutputText),
            },
            Prompt = "prompt",
            Output = OutputSpec.Object(ValueSchema),
        }));
        Assert.Equal("AI_NoObjectGeneratedError", error.ErrorName);
        Assert.Equal(structuredOutputText, error.Text);
        Assert.IsType<Vercel.AI.ProviderUtils.JsonParseException>(error.Cause);
    }

    [Theory]
    [InlineData("default")]
    [InlineData("text")]
    [UpstreamTest(
        Id + "should preserve all text for $name output",
        Coverage = UpstreamCoverage.Partial,
        Note = "The port has no typed text output, so it asserts the result text only.")]
    public async Task Should_preserve_all_text_for_name_output(string name)
    {
        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = new TestLanguageModel
            {
                OnGenerate = _ => Result(
                    new GeneratedContent[] { new GeneratedText("Commentary. "), new GeneratedText("Final answer.") },
                    "Final answer."),
            },
            Prompt = "prompt",
            Output = name == "text" ? OutputSpec.Text() : null,
        });

        Assert.Equal("Commentary. Final answer.", result.Text);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("{\"value\":\"final\"}")]
    [UpstreamTest(Id + "should not reuse an earlier step selection when the final selection is %j", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_reuse_an_earlier_step_selection_when_the_final_selection_is(string? structuredOutputText)
    {
        var model = new TestLanguageModel();
        model.OnGenerate = _ => model.Calls.Count == 1
            ? Result(
                new GeneratedContent[] { new GeneratedText("{\"value\":\"earlier\"}"), new GeneratedToolCall("call-1", "testTool", "{}") },
                "{\"value\":\"earlier\"}",
                FinishReason.ToolCalls)
            : Result(new GeneratedContent[] { new GeneratedText("{\"value\":\"final\"}") }, structuredOutputText);

        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "prompt",
            Output = OutputSpec.Object(ValueSchema),
            Tools = new[] { Tool.Function("testTool", null, "{\"type\":\"object\",\"properties\":{}}", (_, _) => Task.FromResult("\"tool result\"")) },
            StopWhen = StopWhen.IsStepCount(2),
        });

        Assert.Equal("final", result.Output!.Value.GetProperty("value").GetString());
        Assert.Equal("{\"value\":\"final\"}", result.Text);
        Assert.Equal(2, result.Steps.Count);
        Assert.Equal("{\"value\":\"earlier\"}", result.Steps[0].Text);
        Assert.Equal("\"tool result\"", result.ToolResults[0].OutputJson);
    }
}
