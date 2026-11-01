// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateObject;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateObject.GenerateObject;

/// <summary>Port of <c>generate-object.test.ts</c> &gt; <c>generateObject &gt; output = "object" &gt; error handling</c>.</summary>
public sealed class ErrorHandlingTests
{
    private const string Id = "packages/ai/src/generate-object/generate-object.test.ts::generateObject > output = \"object\" > error handling::";
    private const string Schema = "{\"type\":\"object\",\"properties\":{\"content\":{\"type\":\"string\"}},\"required\":[\"content\"],\"additionalProperties\":false}";

    private static AiClient Client() => new(Vercel.AI.Gateway.GatewayProvider.Create(new Vercel.AI.Gateway.GatewayOptions { ApiKey = "test" }));

    private static LanguageModelGenerateResult Result(params GeneratedContent[] content) =>
        new(
            content,
            FinishReason.Stop,
            new LanguageModelUsage(10, 20, 30),
            responseId: "id-1",
            responseModelId: "m-1",
            responseTimestamp: DateTimeOffset.FromUnixTimeMilliseconds(123));

    private static GenerateObjectOptions<ContentResult> Options(LanguageModelGenerateResult result) =>
        new()
        {
            Model = new TestLanguageModel { OnGenerate = _ => result },
            Schema = Schema,
            Prompt = "prompt",
            SerializerOptions = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true },
        };

    private static void Verify(NoObjectGeneratedException error, string message)
    {
        Assert.True(NoObjectGeneratedException.IsInstance(error));
        Assert.Equal(message, error.Message);
        Assert.Equal("id-1", error.Response.Id);
        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(123), error.Response.Timestamp);
        Assert.Equal("m-1", error.Response.ModelId);
        Assert.Equal(10, error.Usage.InputTokens);
        Assert.Equal(20, error.Usage.OutputTokens);
        Assert.Equal(30, error.Usage.TotalTokens);
        Assert.Equal(FinishReason.Stop, error.FinishReason);
    }

    [Fact]
    [UpstreamTest(Id + "should throw NoObjectGeneratedError when schema validation fails", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_throw_NoObjectGeneratedError_when_schema_validation_fails()
    {
        var error = await Assert.ThrowsAsync<NoObjectGeneratedException>(() =>
            Client().GenerateObjectAsync(Options(Result(new GeneratedText("""{ "content": 123 }""")))));

        Verify(error, "No object generated: response did not match schema.");
    }

    [Fact]
    [UpstreamTest(Id + "should throw NoObjectGeneratedError when parsing fails", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_throw_NoObjectGeneratedError_when_parsing_fails()
    {
        var error = await Assert.ThrowsAsync<NoObjectGeneratedException>(() =>
            Client().GenerateObjectAsync(Options(Result(new GeneratedText("{ broken json")))));

        Verify(error, "No object generated: could not parse the response.");
    }

    [Fact]
    [UpstreamTest(Id + "should throw NoObjectGeneratedError when parsing fails with repairResponse", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_throw_NoObjectGeneratedError_when_parsing_fails_with_repairResponse()
    {
        var options = Options(Result(new GeneratedText("{ broken json")));
        options.RepairText = (context, _) => Task.FromResult<string?>(context.Text + "{");

        var error = await Assert.ThrowsAsync<NoObjectGeneratedException>(() => Client().GenerateObjectAsync(options));

        Verify(error, "No object generated: could not parse the response.");
    }

    [Fact]
    [UpstreamTest(Id + "should throw NoObjectGeneratedError when no text is available", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_throw_NoObjectGeneratedError_when_no_text_is_available()
    {
        var error = await Assert.ThrowsAsync<NoObjectGeneratedException>(() =>
            Client().GenerateObjectAsync(Options(Result())));

        Verify(error, "No object generated: the model did not return a response.");
    }

    private sealed class ContentResult
    {
        public string Content { get; set; } = string.Empty;
    }
}
