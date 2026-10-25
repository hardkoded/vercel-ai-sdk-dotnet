// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Reasoning = global::Vercel.AI.ExtractReasoningMiddleware;

namespace Vercel.AI.Tests.Upstream.Middleware.ExtractReasoningMiddleware;

/// <summary>Port of <c>extract-reasoning-middleware.test.ts</c> &gt; <c>extractReasoningMiddleware &gt; wrapGenerate</c>.</summary>
public sealed class WrapGenerateTests
{
    private const string Id = "packages/ai/src/middleware/extract-reasoning-middleware.test.ts::extractReasoningMiddleware > wrapGenerate::";

    private const string ValueSchema = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}}}";

    private static readonly (string Opening, string Closing) Channel = ("<|channel>thought\n", "<channel|>");

    private static AiClient Client() => new(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));

    private static LanguageModelGenerateResult Result(IReadOnlyList<GeneratedContent> content, string? structuredOutputText = null) =>
        new(content, FinishReason.Stop, new LanguageModelUsage(5, 10, 15), "stop", structuredOutputText: structuredOutputText);

    private static async Task<IReadOnlyList<GeneratedContent>> ContentOf(string text, Reasoning middleware)
    {
        var model = new TestLanguageModel { OnGenerate = _ => Result(new GeneratedContent[] { new GeneratedText(text) }) }.WrapLanguageModel(middleware);
        var result = await Client().GenerateTextAsync(new GenerateTextOptions { Model = model, Prompt = "Hello, how can I help?" });
        return result.Content;
    }

    private static void AssertReasoningAndText(IReadOnlyList<GeneratedContent> content, string reasoning, string text)
    {
        Assert.Collection(
            content,
            part => Assert.Equal(reasoning, Assert.IsType<GeneratedReasoning>(part).Text),
            part => Assert.Equal(text, Assert.IsAssignableFrom<GeneratedText>(part).Text));
    }

    public static TheoryData<string> StructuredCases => new() { "reasoning tags", "an omitted custom opening delimiter", "multiple reasoning blocks with a custom separator" };

    [Theory]
    [MemberData(nameof(StructuredCases))]
    [UpstreamTest(Id + "should extract $name from structured output text without losing other content", Coverage = UpstreamCoverage.Partial,
        Note = "OutputSpec.Object with a schema stands in for Output.json().")]
    public async Task Should_extract_name_from_structured_output_text_without_losing_other_content(string name)
    {
        var (middleware, input, text, reasoning) = name switch
        {
            "reasoning tags" => (new Reasoning("think"), "<think>Checking</think>{\"value\":\"final\"}", "{\"value\":\"final\"}", "Checking"),
            "an omitted custom opening delimiter" => (new Reasoning(Channel, startWithReasoning: true), "Checking<channel|>{\"value\":\"final\"}", "{\"value\":\"final\"}", "Checking"),
            _ => (new Reasoning("think", " "), "{<think>First</think>\"value\":\"final\"<think>Second</think>}", "{ \"value\":\"final\" }", "First Second"),
        };
        var model = new TestLanguageModel
        {
            OnGenerate = _ => Result(new GeneratedContent[] { new GeneratedText("{\"value\":\"draft\"}"), new GeneratedText(input) }, input),
        }.WrapLanguageModel(middleware);

        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Output = OutputSpec.Object(ValueSchema),
            Prompt = "Generate JSON",
        });

        Assert.Equal("final", result.Output!.Value.GetProperty("value").GetString());
        Assert.Equal("{\"value\":\"draft\"}" + text, result.Text);
        Assert.Equal(reasoning, result.ReasoningText);
        Assert.Collection(
            result.Content,
            part => Assert.Equal("{\"value\":\"draft\"}", Assert.IsAssignableFrom<GeneratedText>(part).Text),
            part => Assert.Equal(reasoning, Assert.IsType<GeneratedReasoning>(part).Text),
            part => Assert.Equal(text, Assert.IsAssignableFrom<GeneratedText>(part).Text));
    }

    [Fact]
    [UpstreamTest(Id + "should extract reasoning from <think> tags", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_reasoning_from_think_tags()
    {
        var content = await ContentOf("<think>analyzing the request</think>Here is the response", new Reasoning("think"));

        AssertReasoningAndText(content, "analyzing the request", "Here is the response");
    }

    [Fact]
    [UpstreamTest(Id + "should extract reasoning from <think> tags when there is no text", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_reasoning_from_think_tags_when_there_is_no_text()
    {
        var content = await ContentOf("<think>analyzing the request\n</think>", new Reasoning("think"));

        AssertReasoningAndText(content, "analyzing the request\n", string.Empty);
    }

    [Fact]
    [UpstreamTest(Id + "should extract reasoning from multiple <think> tags", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_reasoning_from_multiple_think_tags()
    {
        var content = await ContentOf(
            "<think>analyzing the request</think>Here is the response<think>thinking about the response</think>more",
            new Reasoning("think"));

        AssertReasoningAndText(content, "analyzing the request\nthinking about the response", "Here is the response\nmore");
    }

    [Fact]
    [UpstreamTest(Id + "should prepend <think> tag IFF startWithReasoning is true", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_prepend_think_tag_IFF_startWithReasoning_is_true()
    {
        const string text = "analyzing the request</think>Here is the response";

        var withReasoning = await ContentOf(text, new Reasoning("think", startWithReasoning: true));
        var without = await ContentOf(text, new Reasoning("think"));

        AssertReasoningAndText(withReasoning, "analyzing the request", "Here is the response");
        Assert.Equal(text, Assert.IsAssignableFrom<GeneratedText>(Assert.Single(without)).Text);
    }

    [Fact]
    [UpstreamTest(
        Id + "should preserve reasoning property even when rest contains other properties",
        Coverage = UpstreamCoverage.Partial,
        Note = "The port's generate result has no free-form reasoning property, so the extra property is not set.")]
    public async Task Should_preserve_reasoning_property_even_when_rest_contains_other_properties()
    {
        var content = await ContentOf("<think>analyzing the request</think>Here is the response", new Reasoning("think"));

        AssertReasoningAndText(content, "analyzing the request", "Here is the response");
    }
}
