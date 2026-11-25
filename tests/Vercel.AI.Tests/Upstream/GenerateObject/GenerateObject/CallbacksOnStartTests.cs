// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateObject;
using Vercel.AI.Operations;
using static Vercel.AI.Tests.Upstream.GenerateObject.GenerateObject.GenerateObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.GenerateObject;

/// <summary>Port of <c>generate-object.test.ts</c> &gt; <c>generateObject &gt; callbacks &gt; onStart</c>.</summary>
[Collection("LogWarnings")]
public sealed class CallbacksOnStartTests
{
    private const string Id =
        "packages/ai/src/generate-object/generate-object.test.ts::generateObject > callbacks > onStart::";

    [Fact]
    [UpstreamTest(
        Id + "should call onStart before the model call",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_onStart_before_the_model_call()
    {
        var events = new List<string>();
        var model = new GenerateObjectTestModel { OnGenerate = () => events.Add("doGenerate") };
        var options = ObjectOptions(model);
        options.OnStart = (_, _) =>
        {
            events.Add("onStart");
            return Task.CompletedTask;
        };

        await Client().GenerateObjectAsync(options);

        Assert.Equal(["onStart", "doGenerate"], events);
    }

    [Fact]
    [UpstreamTest(
        Id + "should send correct information with text prompt",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_correct_information_with_text_prompt()
    {
        GenerateObjectStartEvent? startEvent = null;
        var options = ObjectOptions(
            new GenerateObjectTestModel { Provider = "test-provider", ModelId = "test-model" }
        );
        options.SchemaName = "test-schema";
        options.SchemaDescription = "A test schema";
        options.Prompt = "test-prompt";
        options.Temperature = 0.5;
        options.MaxOutputTokens = 100;
        options.GenerateId = () => "test-call-id";
        options.OnStart = (e, _) =>
        {
            startEvent = e;
            return Task.CompletedTask;
        };

        await Client().GenerateObjectAsync(options);

        Assert.NotNull(startEvent);
        Assert.Equal("test-call-id", startEvent.CallId);
        Assert.Null(startEvent.FrequencyPenalty);
        var userAgent = Assert.Single(
            Assert.IsAssignableFrom<IReadOnlyDictionary<string, string?>>(startEvent.Headers)
        );
        Assert.Equal("user-agent", userAgent.Key);
        Assert.Equal(AiSdkVersion.UserAgent, userAgent.Value);
        Assert.Equal(100, startEvent.MaxOutputTokens);
        Assert.Equal(2, startEvent.MaxRetries);
        Assert.Null(startEvent.Messages);
        Assert.Equal("test-model", startEvent.ModelId);
        Assert.Equal("ai.generateObject", startEvent.OperationId);
        Assert.Equal("object", startEvent.Output);
        Assert.Null(startEvent.PresencePenalty);
        Assert.Equal("test-prompt", startEvent.Prompt);
        Assert.Equal("test-provider", startEvent.Provider);
        Assert.Null(startEvent.ProviderOptions);
        Assert.True(
            System.Text.Json.Nodes.JsonNode.DeepEquals(
                System.Text.Json.Nodes.JsonNode.Parse(ContentSchema),
                System.Text.Json.Nodes.JsonNode.Parse(startEvent.Schema!.Value.GetRawText())
            )
        );
        Assert.Equal("A test schema", startEvent.SchemaDescription);
        Assert.Equal("test-schema", startEvent.SchemaName);
        Assert.Null(startEvent.Seed);
        Assert.Null(startEvent.System);
        Assert.Equal(0.5, startEvent.Temperature);
        Assert.Null(startEvent.TopK);
        Assert.Null(startEvent.TopP);
    }

    [Fact]
    [UpstreamTest(Id + "should accept deprecated experimental_telemetry as an alias for telemetry")]
    public async Task Should_accept_deprecated_experimental_telemetry_as_an_alias_for_telemetry()
    {
        GenerateObjectStartEvent? startEvent = null;
        var options = ObjectOptions(new GenerateObjectTestModel());
        options.ExperimentalTelemetry = new TelemetryOptions
        {
            IsEnabled = true,
            FunctionId = "deprecated-fn",
        };
        options.OnStart = (e, _) =>
        {
            startEvent = e;
            return Task.CompletedTask;
        };

        await Client().GenerateObjectAsync(options);

        Assert.NotNull(startEvent);
        Assert.Null(typeof(GenerateObjectStartEvent).GetProperty("IsEnabled"));
        Assert.Null(typeof(GenerateObjectStartEvent).GetProperty("FunctionId"));
    }
}
