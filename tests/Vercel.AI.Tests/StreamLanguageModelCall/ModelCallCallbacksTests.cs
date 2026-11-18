// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using static Vercel.AI.Tests.StreamLanguageModelCall.StreamLanguageModelCallFixture;

namespace Vercel.AI.Tests.StreamLanguageModelCall;

/// <summary>Port of <c>stream-language-model-call.test.ts</c> &gt; <c>streamLanguageModelCall &gt; model-call callbacks</c>.</summary>
public sealed class ModelCallCallbacksTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/stream-language-model-call.test.ts::streamLanguageModelCall > model-call callbacks::";

    [Fact]
    [UpstreamTest(
        Prefix + "should call onLanguageModelCallStart before doStream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_onLanguageModelCallStart_before_doStream()
    {
        var callOrder = new List<string>();
        LanguageModelCallStartEvent? startEvent = null;
        var model = Model(Finish());
        var tool = TestTool();

        var result = await LanguageModelCallStreaming.StreamLanguageModelCall(
            new StreamLanguageModelCallOptions
            {
                Model = new RecordingModel(model, callOrder),
                Prompt = "test prompt",
                MaxOutputTokens = 128,
                Temperature = 0.7,
                TopP = 0.9,
                TopK = 40,
                PresencePenalty = 0.2,
                FrequencyPenalty = 0.1,
                StopSequences = ["stop"],
                Seed = 123,
                Reasoning = "high",
                Tools = [tool],
                CallId = "test-telemetry-call-id",
                NowMs = () => 0,
                OnLanguageModelCallStart = (@event, _) =>
                {
                    callOrder.Add("onLanguageModelCallStart");
                    startEvent = @event;
                    return Task.CompletedTask;
                },
            }
        );
        await Collect(result);

        Assert.Equal(["onLanguageModelCallStart", "doStream"], callOrder);
        Assert.NotNull(startEvent);
        Assert.Equal("test-telemetry-call-id", startEvent.CallId);
        Assert.Equal("test", startEvent.Provider);
        Assert.Equal("test", startEvent.ModelId);
        Assert.Null(startEvent.Instructions);
        var message = Assert.IsType<UserModelMessage>(Assert.Single(startEvent.Messages));
        Assert.Equal(
            "test prompt",
            Assert.IsType<TextContentPart>(Assert.Single(message.Content)).Text
        );
        Assert.Equal(128, startEvent.CallOptions.MaxOutputTokens);
        Assert.Equal(0.7, startEvent.CallOptions.Temperature);
        Assert.Equal(0.9, startEvent.CallOptions.TopP);
        Assert.Equal(40, startEvent.CallOptions.TopK);
        Assert.Equal(0.2, startEvent.CallOptions.PresencePenalty);
        Assert.Equal(0.1, startEvent.CallOptions.FrequencyPenalty);
        Assert.Equal(["stop"], startEvent.CallOptions.StopSequences);
        Assert.Equal(123, startEvent.CallOptions.Seed);
        Assert.Equal("high", startEvent.CallOptions.Reasoning);
        var startTool = Assert.Single(startEvent.Tools!);
        Assert.Equal("testTool", startTool.Name);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should generate a consistent callId when one is not provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_generate_a_consistent_callId_when_one_is_not_provided()
    {
        LanguageModelCallStartEvent? startEvent = null;
        LanguageModelCallEndEvent? endEvent = null;

        var result = await LanguageModelCallStreaming.StreamLanguageModelCall(
            new StreamLanguageModelCallOptions
            {
                Model = Model(Finish()),
                Prompt = "test prompt",
                NowMs = () => 0,
                GenerateId = () => "aitxt-generated-response-id",
                GenerateCallId = () => "call-generated-call-id",
                OnLanguageModelCallStart = (@event, _) =>
                {
                    startEvent = @event;
                    return Task.CompletedTask;
                },
                OnLanguageModelCallEnd = (@event, _) =>
                {
                    endEvent = @event;
                    return Task.CompletedTask;
                },
            }
        );
        await Collect(result);

        Assert.NotNull(startEvent);
        Assert.NotNull(endEvent);
        Assert.Equal("call-generated-call-id", endEvent.CallId);
        Assert.Empty(endEvent.Content);
        Assert.Equal(FinishReason.Stop, endEvent.FinishReason);
        Assert.Equal("test", endEvent.ModelId);
        Assert.Equal(0, endEvent.Performance!.EffectiveOutputTokensPerSecond);
        Assert.Equal(0, endEvent.Performance.EffectiveTotalTokensPerSecond);
        Assert.Null(endEvent.Performance.InputTokensPerSecond);
        Assert.Null(endEvent.Performance.OutputTokensPerSecond);
        Assert.Equal(0, endEvent.Performance.ResponseTimeMs);
        Assert.Null(endEvent.Performance.TimeBetweenOutputChunksMs);
        Assert.Null(endEvent.Performance.TimeToFirstOutputMs);
        Assert.Equal("test", endEvent.Provider);
        Assert.Equal("aitxt-generated-response-id", endEvent.ResponseId);
        Assert.Equal(3, endEvent.Usage.InputTokens);
        Assert.Equal(10, endEvent.Usage.OutputTokens);
        Assert.Equal(13, endEvent.Usage.TotalTokens);
        Assert.Equal("call-generated-call-id", startEvent.CallId);
        Assert.Null(startEvent.Instructions);
        Assert.Equal("test", startEvent.ModelId);
        Assert.Equal("test", startEvent.Provider);
        Assert.Null(startEvent.Tools);
        Assert.Equal(startEvent.CallId, endEvent.CallId);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should call onLanguageModelCallEnd with parsed model-call content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_onLanguageModelCallEnd_with_parsed_model_call_content()
    {
        var endEvents = new List<LanguageModelCallEndEvent>();

        var result = await LanguageModelCallStreaming.StreamLanguageModelCall(
            new StreamLanguageModelCallOptions
            {
                Model = Model(
                    new ResponseMetadataStreamPart(
                        "response-1",
                        "response-model",
                        DateTimeOffset.Parse("2025-01-01T00:00:00.000Z")
                    ),
                    new TextStartStreamPart("text-1"),
                    new TextDeltaStreamPart("text-1", "Hello "),
                    new TextDeltaStreamPart("text-1", "world"),
                    new TextEndStreamPart("text-1"),
                    new ToolCallStreamPart("call-1", "testTool", "{ \"value\": \"hello\" }"),
                    Finish(FinishReason.ToolCalls, null)
                ),
                Tools = [TestTool()],
                Prompt = "test prompt",
                CallId = "test-telemetry-call-id",
                NowMs = () => 0,
                OnLanguageModelCallEnd = (@event, _) =>
                {
                    endEvents.Add(@event);
                    return Task.CompletedTask;
                },
            }
        );
        await Collect(result);

        var end = Assert.Single(endEvents);
        Assert.Equal("test-telemetry-call-id", end.CallId);
        Assert.Equal(2, end.Content.Count);
        Assert.Equal("Hello world", Assert.IsType<GeneratedText>(end.Content[0]).Text);
        var call = Assert.IsType<GeneratedToolCall>(end.Content[1]);
        Assert.Equal("call-1", call.ToolCallId);
        Assert.Equal("testTool", call.ToolName);
        JsonAssertEqual("{\"value\":\"hello\"}", call.ArgumentsJson);
        Assert.False(call.ProviderExecuted);
        Assert.Equal(FinishReason.ToolCalls, end.FinishReason);
        Assert.Equal("response-model", end.ModelId);
        Assert.Equal(0, end.Performance!.EffectiveOutputTokensPerSecond);
        Assert.Equal(0, end.Performance.EffectiveTotalTokensPerSecond);
        Assert.Equal(0, end.Performance.InputTokensPerSecond);
        Assert.Equal(0, end.Performance.OutputTokensPerSecond);
        Assert.Equal(0, end.Performance.ResponseTimeMs);
        var gaps = end.Performance.TimeBetweenOutputChunksMs!;
        Assert.Equal(
            [0, 0, 0, 0, 0, 0],
            [gaps.Avg, gaps.Max, gaps.Median, gaps.Min, gaps.P10, gaps.P90]
        );
        Assert.Equal(0, end.Performance.TimeToFirstOutputMs);
        Assert.Equal("test", end.Provider);
        Assert.Equal("response-1", end.ResponseId);
        Assert.Equal(3, end.Usage.InputTokens);
        Assert.Equal(10, end.Usage.OutputTokens);
        Assert.Equal(13, end.Usage.TotalTokens);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should generate a responseId when the provider does not send one",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_generate_a_responseId_when_the_provider_does_not_send_one()
    {
        LanguageModelCallEndEvent? endEvent = null;

        var result = await LanguageModelCallStreaming.StreamLanguageModelCall(
            new StreamLanguageModelCallOptions
            {
                Model = Model(Finish()),
                Prompt = "test prompt",
                CallId = "test-telemetry-call-id",
                NowMs = () => 0,
                GenerateId = () => "aitxt-generated-response-id",
                OnLanguageModelCallEnd = (@event, _) =>
                {
                    endEvent = @event;
                    return Task.CompletedTask;
                },
            }
        );
        await Collect(result);

        Assert.NotNull(endEvent);
        Assert.Equal("test-telemetry-call-id", endEvent.CallId);
        Assert.Empty(endEvent.Content);
        Assert.Equal(FinishReason.Stop, endEvent.FinishReason);
        Assert.Equal("test", endEvent.ModelId);
        Assert.Equal(0, endEvent.Performance!.EffectiveOutputTokensPerSecond);
        Assert.Equal(0, endEvent.Performance.EffectiveTotalTokensPerSecond);
        Assert.Null(endEvent.Performance.InputTokensPerSecond);
        Assert.Null(endEvent.Performance.OutputTokensPerSecond);
        Assert.Equal(0, endEvent.Performance.ResponseTimeMs);
        Assert.Null(endEvent.Performance.TimeBetweenOutputChunksMs);
        Assert.Null(endEvent.Performance.TimeToFirstOutputMs);
        Assert.Equal("test", endEvent.Provider);
        Assert.Equal("aitxt-generated-response-id", endEvent.ResponseId);
        Assert.Equal(3, endEvent.Usage.InputTokens);
        Assert.Equal(10, endEvent.Usage.OutputTokens);
        Assert.Equal(13, endEvent.Usage.TotalTokens);
    }

    private static void JsonAssertEqual(string expected, string actual) =>
        Assert.Equal(
            System.Text.Json.Nodes.JsonNode.Parse(expected)!.ToJsonString(),
            System.Text.Json.Nodes.JsonNode.Parse(actual)!.ToJsonString()
        );

    // Records when doStream runs, so the test can check the order of the callbacks.
    private sealed class RecordingModel(TestLanguageModel inner, List<string> callOrder)
        : ILanguageModel
    {
        public string SpecificationVersion => inner.SpecificationVersion;

        public string Provider => inner.Provider;

        public string ModelId => inner.ModelId;

        public IReadOnlyDictionary<string, string> SupportedUrls => inner.SupportedUrls;

        public Task<LanguageModelGenerateResult> DoGenerateAsync(
            LanguageModelCallOptions options,
            CancellationToken cancellationToken
        ) => inner.DoGenerateAsync(options, cancellationToken);

        public Task<LanguageModelStreamResult> DoStreamAsync(
            LanguageModelCallOptions options,
            CancellationToken cancellationToken
        )
        {
            callOrder.Add("doStream");
            return inner.DoStreamAsync(options, cancellationToken);
        }
    }
}
