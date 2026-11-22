// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Json = global::Vercel.AI.ExtractJsonMiddleware;

namespace Vercel.AI.Tests.Upstream.Middleware.ExtractJsonMiddleware;

/// <summary>Port of <c>extract-json-middleware.test.ts</c> &gt; <c>extractJsonMiddleware &gt; wrapGenerate</c>.</summary>
public sealed class WrapGenerateTests
{
    private const string Id =
        "packages/ai/src/middleware/extract-json-middleware.test.ts::extractJsonMiddleware > wrapGenerate::";

    private const string ValueSchema =
        "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}}}";

    private static AiClient Client() =>
        new(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));

    private static ILanguageModel Wrap(
        LanguageModelGenerateResult result,
        Func<string, string>? transform = null
    ) => new TestLanguageModel { OnGenerate = _ => result }.WrapLanguageModel(new Json(transform));

    private static LanguageModelGenerateResult Result(
        IReadOnlyList<GeneratedContent> content,
        string? structuredOutputText = null
    ) =>
        new(
            content,
            FinishReason.Stop,
            new LanguageModelUsage(5, 10, 15),
            "stop",
            structuredOutputText: structuredOutputText
        );

    private static async Task<string> TextOf(string text, Func<string, string>? transform = null)
    {
        var result = await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = Wrap(
                        Result(new GeneratedContent[] { new GeneratedText(text) }),
                        transform
                    ),
                    Prompt = "Generate JSON",
                }
            );
        return result.Text;
    }

    public static TheoryData<string, string, bool> StructuredOutputCases =>
        new()
        {
            { "markdown fences", "```json\n{\"value\":\"final\"}\n```", false },
            { "a custom transform", "PREFIX{\"value\":\"final\"}SUFFIX", true },
        };

    [Theory]
    [MemberData(nameof(StructuredOutputCases))]
    [UpstreamTest(
        Id + "should apply $name to structured output text without losing other text",
        Coverage = UpstreamCoverage.Covered,
        Note = "OutputSpec.Object with a schema stands in for Output.json()."
    )]
    public async Task Should_apply_name_to_structured_output_text_without_losing_other_text(
        string name,
        string text,
        bool custom
    )
    {
        Assert.NotEmpty(name);
        Func<string, string>? transform = custom
            ? t => t.Replace("PREFIX", "").Replace("SUFFIX", "")
            : null;

        var result = await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = Wrap(
                        Result(
                            new GeneratedContent[]
                            {
                                new GeneratedText("{\"value\":\"draft\"}"),
                                new GeneratedText(text),
                            },
                            text
                        ),
                        transform
                    ),
                    Output = OutputSpec.Object(ValueSchema),
                    Prompt = "Generate JSON",
                }
            );

        Assert.Equal(
            "final",
            ((GenerateTextResult)result).Output!.Value.GetProperty("value").GetString()
        );
        Assert.Equal("{\"value\":\"draft\"}{\"value\":\"final\"}", result.Text);
        Assert.Collection(
            result.Content,
            part =>
                Assert.Equal(
                    "{\"value\":\"draft\"}",
                    Assert.IsAssignableFrom<GeneratedText>(part).Text
                ),
            part =>
                Assert.Equal(
                    "{\"value\":\"final\"}",
                    Assert.IsAssignableFrom<GeneratedText>(part).Text
                )
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should strip markdown json fence from text content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_strip_markdown_json_fence_from_text_content()
    {
        Assert.Equal("{\"value\": \"test\"}", await TextOf("```json\n{\"value\": \"test\"}\n```"));
    }

    [Fact]
    [UpstreamTest(
        Id + "should strip markdown fence without json tag",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_strip_markdown_fence_without_json_tag()
    {
        Assert.Equal("{\"value\": \"test\"}", await TextOf("```\n{\"value\": \"test\"}\n```"));
    }

    [Fact]
    [UpstreamTest(
        Id + "should leave text without fences unchanged",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_leave_text_without_fences_unchanged()
    {
        Assert.Equal("{\"value\": \"test\"}", await TextOf("{\"value\": \"test\"}"));
    }

    [Fact]
    [UpstreamTest(
        Id + "should use custom transform function when provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_custom_transform_function_when_provided()
    {
        Assert.Equal(
            "{\"value\": \"test\"}",
            await TextOf("PREFIX{\"value\": \"test\"}SUFFIX", text =>
                text.Replace("PREFIX", "").Replace("SUFFIX", "")
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should preserve non-text content parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_non_text_content_parts()
    {
        var result = await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = Wrap(
                        Result(
                            new GeneratedContent[]
                            {
                                new GeneratedText("```json\n{\"value\": \"test\"}\n```"),
                                new GeneratedToolCall("call-1", "testTool", "{\"foo\": \"bar\"}"),
                            }
                        )
                    ),
                    Prompt = "Generate JSON",
                }
            );

        Assert.Equal("{\"value\": \"test\"}", result.Text);
        Assert.Equal(3, result.Content.Count);
        Assert.Equal(
            "call-1",
            Assert.IsAssignableFrom<GeneratedToolCall>(result.Content[1]).ToolCallId
        );
    }
}
