// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.OpenAICompatibleChatLanguageModelTests;

/// <summary>Port of <c>openai-compatible-chat-language-model.test.ts</c> &gt; <c>prepareRequest</c>.</summary>
public sealed class PrepareRequestTests
{
    private const string UpstreamCommit = "c9c6402cbdfce3f82b36b4073236043b1946dd06";
    private const string Prefix = "packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::prepareRequest::";

    private static LanguageModelCallOptions RichOptions()
    {
        using var schema = JsonDocument.Parse("{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}");
        using var toolSchema = JsonDocument.Parse("{\"type\":\"object\",\"properties\":{\"location\":{\"type\":\"string\"}},\"required\":[\"location\"]}");
        var options = UpstreamChat.Prompt();
        options.MaxOutputTokens = 100;
        options.Temperature = 0.5;
        options.TopK = 10;
        options.StopSequences = new[] { "\n\n" };
        options.Seed = 42;
        options.JsonResponse = true;
        options.JsonSchemaName = "answer";
        options.JsonSchema = schema.RootElement.Clone();
        options.Tools = new[] { new LanguageModelTool("weather", null, toolSchema.RootElement.Clone()) };
        options.ToolChoice = ToolChoice.Tool("weather");
        options.ProviderOptions = UpstreamChat.Bag("testProvider", "{\"user\":\"test-user\",\"reasoningEffort\":\"medium\",\"custom_passthrough\":\"value\"}");
        return options;
    }

    private static OpenAICompatibleLanguageModel Model(UpstreamCapture capture, Action<OpenAICompatibleOptions>? configure = null)
    {
        return UpstreamChat.Model(capture, "test-provider", "grok-3", configure);
    }

    [Fact]
    [UpstreamTest(Prefix + "should match the body and warnings of doGenerate", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Should_match_the_body_and_warnings_of_doGenerate()
    {
        var capture = new UpstreamCapture();
        var model = Model(capture, options => options.SupportsStructuredOutputs = true);

        var prepared = model.PrepareRequest(RichOptions());
        var result = await model.DoGenerateAsync(RichOptions(), CancellationToken.None);

        Assert.Equal(prepared.Body.ToJsonString(), capture.Requests[0].Body);
        Assert.True(JsonNode.DeepEquals(UpstreamChat.Body(capture), JsonNode.Parse(prepared.Body.ToJsonString())));
        Assert.Equal(result.Warnings.Select(w => (w.Type, w.Feature, w.Message)), prepared.Warnings.Select(w => (w.Type, w.Feature, w.Message)));
        Assert.Contains(prepared.Warnings, w => w.Type == "unsupported" && w.Feature == "topK");
        Assert.Equal("test-user", prepared.Body["user"]!.GetValue<string>());
        Assert.Equal("medium", prepared.Body["reasoning_effort"]!.GetValue<string>());
        Assert.Equal("value", prepared.Body["custom_passthrough"]!.GetValue<string>());
        Assert.Equal("json_schema", prepared.Body["response_format"]!["type"]!.GetValue<string>());
        Assert.Equal("function", prepared.Body["tool_choice"]!["type"]!.GetValue<string>());
        Assert.Equal("weather", prepared.Body["tool_choice"]!["function"]!["name"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Prefix + "should not make a network call", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Should_not_make_a_network_call()
    {
        var capture = new UpstreamCapture();
        var model = Model(capture);

        model.PrepareRequest(UpstreamChat.Prompt());

        Assert.Empty(capture.Requests);
    }

    [Fact]
    [UpstreamTest(Prefix + "should apply transformRequestBody and include its warnings", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Should_apply_transformRequestBody_and_include_its_warnings()
    {
        var capture = new UpstreamCapture();
        var calls = new List<JsonObject>();
        var model = Model(capture, options =>
        {
            options.IncludeUsage = true;
            options.TransformRequestBody = (body, warnings) =>
            {
                calls.Add((JsonObject)body.DeepClone());
                warnings.Add(new CallWarning("other", "transformed"));
                body["custom_field"] = "added-by-transform";
                return body;
            };
        });

        var prepared = model.PrepareRequest(UpstreamChat.Prompt());

        Assert.Equal("grok-3", prepared.Body["model"]!.GetValue<string>());
        Assert.Equal("added-by-transform", prepared.Body["custom_field"]!.GetValue<string>());
        var warning = Assert.Single(prepared.Warnings);
        Assert.Equal("other", warning.Type);
        Assert.Equal("transformed", warning.Message);
        var call = Assert.Single(calls);
        Assert.False(call.ContainsKey("stream"));
        Assert.False(call.ContainsKey("stream_options"));
    }

    [Fact]
    [UpstreamTest(Prefix + "should not include stream fields", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Should_not_include_stream_fields()
    {
        var model = Model(new UpstreamCapture(), options => options.IncludeUsage = true);

        var prepared = model.PrepareRequest(UpstreamChat.Prompt());

        Assert.False(prepared.Body.ContainsKey("stream"));
        Assert.False(prepared.Body.ContainsKey("stream_options"));
    }

    [Fact]
    [UpstreamTest(Prefix + "should apply mapReasoning", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Should_apply_mapReasoning()
    {
        var model = Model(new UpstreamCapture(), options => options.ResolveReasoningEffort = (_, _, _, warnings) =>
        {
            warnings.Add(new CallWarning("other", "mapped reasoning"));
            return "low";
        });
        var options = UpstreamChat.Prompt();
        options.Reasoning = "high";

        var prepared = model.PrepareRequest(options);

        Assert.Equal("low", prepared.Body["reasoning_effort"]!.GetValue<string>());
        Assert.Contains(prepared.Warnings, w => w.Type == "other" && w.Message == "mapped reasoning");
    }

    [Fact]
    [UpstreamTest(Prefix + "should warn about the deprecated raw provider options key like doGenerate", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Should_warn_about_the_deprecated_raw_provider_options_key_like_doGenerate()
    {
        var model = Model(new UpstreamCapture());
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag("test-provider", "{\"user\":\"test-user\"}");

        var prepared = model.PrepareRequest(options);
        var result = await model.DoGenerateAsync(options, CancellationToken.None);

        Assert.Contains(prepared.Warnings, w => w.Type == "deprecated" && w.Message == "providerOptions key 'test-provider'. Use 'testProvider' instead.");
        Assert.Equal(result.Warnings.Select(w => (w.Type, w.Feature, w.Message)), prepared.Warnings.Select(w => (w.Type, w.Feature, w.Message)));
    }
}
