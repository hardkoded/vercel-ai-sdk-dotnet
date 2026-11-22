// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateObject;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateObject.GenerateObject;

/// <summary>Port of <c>generate-object.test.ts</c> &gt; <c>generateObject &gt; output = "object" &gt; options.repairText</c>.</summary>
public sealed class OptionsRepairTextTests
{
    private const string Id =
        "packages/ai/src/generate-object/generate-object.test.ts::generateObject > output = \"object\" > options.repairText::";
    private const string Schema =
        "{\"type\":\"object\",\"properties\":{\"content\":{\"type\":\"string\"}},\"required\":[\"content\"],\"additionalProperties\":false}";

    private static AiClient Client() =>
        new(
            Vercel.AI.Gateway.GatewayProvider.Create(
                new Vercel.AI.Gateway.GatewayOptions { ApiKey = "test" }
            )
        );

    private static GenerateObjectOptions<ContentResult> Options(string text) =>
        new()
        {
            Model = new TestLanguageModel
            {
                OnGenerate = _ => new LanguageModelGenerateResult(
                    new GeneratedContent[] { new GeneratedText(text) },
                    FinishReason.Stop,
                    new LanguageModelUsage(10, 20, 30)
                ),
            },
            Schema = Schema,
            Prompt = "prompt",
            SerializerOptions = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            },
        };

    private sealed class ContentResult
    {
        public string Content { get; set; } = string.Empty;
    }

    [Fact]
    [UpstreamTest(
        Id + "should be able to repair a JSONParseError",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_be_able_to_repair_a_JSONParseError()
    {
        var options = Options("{ \"content\": \"provider metadata test\" ");
        options.RepairText = (context, _) =>
        {
            Assert.IsType<JsonParseException>(context.Error);
            Assert.Equal("{ \"content\": \"provider metadata test\" ", context.Text);
            return Task.FromResult<string?>(context.Text + "}");
        };

        var result = await Client().GenerateObjectAsync(options);

        Assert.Equal("provider metadata test", result.Object.Content);
    }

    [Fact]
    [UpstreamTest(
        Id + "should be able to repair a TypeValidationError",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_be_able_to_repair_a_TypeValidationError()
    {
        var options = Options("{ \"content-a\": \"provider metadata test\" }");
        options.RepairText = (context, _) =>
        {
            Assert.IsType<TypeValidationException>(context.Error);
            Assert.Equal("{ \"content-a\": \"provider metadata test\" }", context.Text);
            return Task.FromResult<string?>("{ \"content\": \"provider metadata test\" }");
        };

        var result = await Client().GenerateObjectAsync(options);

        Assert.Equal("provider metadata test", result.Object.Content);
    }

    [Fact]
    [UpstreamTest(
        Id + "should be able to handle repair that returns null",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_be_able_to_handle_repair_that_returns_null()
    {
        var options = Options("{ \"content-a\": \"provider metadata test\" }");
        options.RepairText = (context, _) =>
        {
            Assert.IsType<TypeValidationException>(context.Error);
            Assert.Equal("{ \"content-a\": \"provider metadata test\" }", context.Text);
            return Task.FromResult<string?>(null);
        };

        var error = await Assert.ThrowsAsync<NoObjectGeneratedException>(() =>
            Client().GenerateObjectAsync(options)
        );

        Assert.Contains("No object generated: response did not match schema.", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Id + "should support the deprecated experimental_repairText option",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_support_the_deprecated_experimental_repairText_option()
    {
        var options = Options("{ \"content\": \"repaired\"");
        options.ExperimentalRepairText = (context, _) =>
            Task.FromResult<string?>(context.Text + " }");

        var result = await Client().GenerateObjectAsync(options);

        Assert.Equal("repaired", result.Object.Content);
    }

    [Fact]
    [UpstreamTest(
        Id + "should prefer repairText over experimental_repairText",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_prefer_repairText_over_experimental_repairText()
    {
        var options = Options("{ \"content\": \"repaired\"");
        options.RepairText = (context, _) => Task.FromResult<string?>(context.Text + " }");
        options.ExperimentalRepairText = (_, _) =>
            throw new InvalidOperationException("deprecated alias should not be called");

        var result = await Client().GenerateObjectAsync(options);

        Assert.Equal("repaired", result.Object.Content);
    }
}
