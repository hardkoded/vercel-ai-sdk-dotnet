// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using Vercel.AI.Operations;
using Vercel.AI.Tests.Upstream.Operations.Decide;
using static Vercel.AI.Tests.DecisionFixtures;
using static Vercel.AI.Tests.Upstream.Operations.Decide.DecideSupport;
using DecideOperation = Vercel.AI.Operations.Decide;
using OtelIntegration = Vercel.AI.OpenTelemetry.OpenTelemetry;
using OtelOptions = Vercel.AI.OpenTelemetry.OpenTelemetryOptions;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry integration with decide</c>.</summary>
[Collection("DownloadFetch")]
public sealed class OpenTelemetryIntegrationWithDecideTests
{
    private const string UpstreamCommit = "15bfd6b262f688cfb8281dbdb32c7dffe082337e";
    private const string Test =
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry integration with decide::";

    private static Dictionary<string, DecisionQuestion> RefundQuestions() =>
        new() { ["refund"] = Q("boolean", "Refund?") };

    private static TelemetryOptions Integration(
        MockTracer tracer,
        bool? recordInputs = null,
        bool experimentalDecision = false
    ) =>
        new()
        {
            RecordInputs = recordInputs,
            Integrations =
            [
                new OtelIntegration(
                    new OtelOptions
                    {
                        Tracer = tracer.Source,
                        ExperimentalDecision = experimentalDecision,
                    }
                ),
            ],
        };

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [UpstreamTest(
        Test + "serializes decision image bytes with recordInputs=%s",
        Commit = UpstreamCommit
    )]
    public async Task Serializes_decision_image_bytes(bool recordInputs)
    {
        using var tracer = new MockTracer();
        byte[] bytes = [0x89, 0x50, 0x4e, 0x47, 0xff, 0xff];

        await DecideOperation.DecideAsync(
            new DecideRequest
            {
                Model = new DecideModel
                {
                    Result = new DecisionModelResult(
                        new Dictionary<string, DecisionAnswer>
                        {
                            ["visible"] = new DecisionAnswer("boolean", probability: 0.9),
                        }
                    ),
                },
                State = S(
                    new List<object?>
                    {
                        new Dictionary<string, object?>
                        {
                            ["type"] = "file",
                            ["mediaType"] = "image/png",
                            ["data"] = bytes,
                        },
                    }
                ),
                Questions = new Dictionary<string, DecisionQuestion>
                {
                    ["visible"] = Q("boolean", "Is the product visible?"),
                },
                Telemetry = Integration(tracer, recordInputs, experimentalDecision: true),
            }
        );

        var spans = tracer.Spans;
        Assert.Equal(2, spans.Count);
        foreach (var span in spans)
        {
            span.InitAttributes.TryGetValue("ai.decision.state", out var state);
            Assert.Equal(
                recordInputs
                    ? "[{\"type\":\"file\",\"mediaType\":\"image/png\",\"data\":\"iVBOR///\"}]"
                    : null,
                state
            );
        }
    }

    [Theory]
    [InlineData("current")]
    [InlineData("deprecated")]
    [UpstreamTest(Test + "creates decision spans through the %s API", Commit = UpstreamCommit)]
    public async Task Creates_decision_spans_through_the_API(string api)
    {
        using var tracer = new MockTracer();
        var model = new DecideModel
        {
            Result = new DecisionModelResult(
                new Dictionary<string, DecisionAnswer>
                {
                    ["refund"] = new DecisionAnswer("boolean", probability: 0.9),
                },
                usage: new OperationUsage(12, 2),
                response: new ProviderResponse(
                    id: "test-response-id",
                    modelId: "mock-model-resolved"
                )
            ),
        };
        var state = S(new Dictionary<string, object?> { ["message"] = "Please refund me" });
        var telemetry = Integration(tracer, experimentalDecision: true);

        if (api == "current")
        {
            await DecideOperation.DecideAsync(
                new DecideRequest
                {
                    Model = model,
                    State = state,
                    Questions = RefundQuestions(),
                    Telemetry = telemetry,
                }
            );
        }
        else
        {
#pragma warning disable CS0618, CS0672
            await Evaluate.EvaluateAsync(
                new EvaluateRequest
                {
                    Model = model,
                    State = state,
                    Questions = RefundQuestions()
                        .ToDictionary(
                            pair => pair.Key,
                            pair => new EvaluationQuestion(pair.Value.Type, pair.Value.Instructions)
                        ),
                    Telemetry = telemetry,
                }
            );
#pragma warning restore CS0618, CS0672
        }

        var spans = tracer.Spans;
        Assert.Equal(2, spans.Count);
        Assert.All(spans, span => Assert.True(span.Ended));
        Assert.All(spans, span => Assert.Equal("decide mock-model-id", span.Name));
        Assert.Equal(
            new Dictionary<string, object?>
            {
                ["ai.decision.questions"] =
                    "{\"refund\":{\"type\":\"boolean\",\"instructions\":\"Refund?\"}}",
                ["ai.decision.state"] = "{\"message\":\"Please refund me\"}",
                ["gen_ai.operation.name"] = "decide",
                ["gen_ai.provider.name"] = "mock-provider",
                ["gen_ai.request.model"] = "mock-model-id",
            },
            spans[0].InitAttributes
        );
        Assert.Equal(
            new Dictionary<string, object?>
            {
                ["ai.decision.answers"] = "{\"refund\":{\"type\":\"boolean\",\"probability\":0.9}}",
            },
            spans[0].RuntimeAttributes
        );
        Assert.Equal(
            "[{\"type\":\"json\",\"value\":{\"message\":\"Please refund me\"}}]",
            spans[1].InitAttributes["ai.decision.state"]
        );
        Assert.Equal(
            new Dictionary<string, object?>
            {
                ["ai.decision.answers"] = "{\"refund\":{\"type\":\"boolean\",\"probability\":0.9}}",
                ["gen_ai.response.id"] = "test-response-id",
                ["gen_ai.response.model"] = "mock-model-resolved",
                ["gen_ai.usage.input_tokens"] = 12,
                ["gen_ai.usage.output_tokens"] = 2,
            },
            spans[1].RuntimeAttributes
        );
    }

    [Theory]
    [InlineData("deprecated")]
    [InlineData("current")]
    [UpstreamTest(
        Test + "honors %s subclass hooks with and without super calls",
        Commit = UpstreamCommit
    )]
    public async Task Honors_subclass_hooks_with_and_without_super_calls(string hooks)
    {
        foreach (var callSuper in new[] { false, true })
        {
            var calls = new List<(string, string)>();
            using var tracer = new MockTracer();
            var options = new OtelOptions { Tracer = tracer.Source };
            OtelIntegration integration =
                hooks == "deprecated"
                    ? new DeprecatedHooksIntegration(options, calls, callSuper)
                    : new CurrentHooksIntegration(options, calls, callSuper);

            await DecideOperation.DecideAsync(
                new DecideRequest
                {
                    Model = new DecideModel
                    {
                        Result = new DecisionModelResult(
                            new Dictionary<string, DecisionAnswer>
                            {
                                ["refund"] = new DecisionAnswer("boolean", probability: 0.9),
                            }
                        ),
                    },
                    State = "Please refund me",
                    Questions = RefundQuestions(),
                    Telemetry = new TelemetryOptions { Integrations = [integration] },
                }
            );

            Assert.Equal(
                new[]
                {
                    ("start", "ai.decide"),
                    ("model-start", "ai.decide.doDecide"),
                    ("model-end", "ai.decide.doDecide"),
                    ("end", "ai.decide"),
                },
                calls
            );
            Assert.Equal(callSuper ? 2 : 0, tracer.Spans.Count);
            Assert.All(tracer.Spans, span => Assert.True(span.Ended));
        }
    }

#pragma warning disable CS0618, CS0672
    private sealed class DeprecatedHooksIntegration(
        OtelOptions options,
        List<(string, string)> calls,
        bool callSuper
    ) : OtelIntegration(options)
    {
        public override Func<EvaluateEvent, CancellationToken, Task>? OnEvaluateStart =>
            Hook("start", base.OnEvaluateStart);

        public override Func<
            EvaluateModelEvent,
            CancellationToken,
            Task
        >? OnEvaluationModelCallStart => Hook("model-start", base.OnEvaluationModelCallStart);

        public override Func<
            EvaluateModelEvent,
            CancellationToken,
            Task
        >? OnEvaluationModelCallEnd => Hook("model-end", base.OnEvaluationModelCallEnd);

        public override Func<EvaluateEvent, CancellationToken, Task>? OnEvaluateEnd =>
            Hook("end", base.OnEvaluateEnd);

        private Func<TEvent, CancellationToken, Task> Hook<TEvent>(
            string name,
            Func<TEvent, CancellationToken, Task>? inner
        )
            where TEvent : class
        {
            return (e, ct) =>
            {
                calls.Add((name, OperationIdOf(e)));
                return callSuper ? inner!(e, ct) : Task.CompletedTask;
            };
        }
    }

    private sealed class CurrentHooksIntegration(
        OtelOptions options,
        List<(string, string)> calls,
        bool callSuper
    ) : OtelIntegration(options)
    {
        public override Func<DecideEvent, CancellationToken, Task>? OnDecideStart =>
            Hook("start", base.OnDecideStart);

        public override Func<
            DecisionModelEvent,
            CancellationToken,
            Task
        >? OnDecisionModelCallStart => Hook("model-start", base.OnDecisionModelCallStart);

        public override Func<DecisionModelEvent, CancellationToken, Task>? OnDecisionModelCallEnd =>
            Hook("model-end", base.OnDecisionModelCallEnd);

        public override Func<DecideEvent, CancellationToken, Task>? OnDecideEnd =>
            Hook("end", base.OnDecideEnd);

        private Func<TEvent, CancellationToken, Task> Hook<TEvent>(
            string name,
            Func<TEvent, CancellationToken, Task>? inner
        )
            where TEvent : class
        {
            return (e, ct) =>
            {
                calls.Add((name, OperationIdOf(e)));
                return callSuper ? inner!(e, ct) : Task.CompletedTask;
            };
        }
    }
#pragma warning restore CS0618, CS0672

    private static string OperationIdOf(object e) =>
        e switch
        {
            DecideEvent decide => decide.OperationId,
            DecisionModelEvent model => model.OperationId,
            _ => throw new InvalidOperationException(),
        };

    [Fact]
    [UpstreamTest(
        Test + "ends both spans with error status when decision fails",
        Commit = UpstreamCommit
    )]
    public async Task Ends_both_spans_with_error_status_when_decision_fails()
    {
        using var tracer = new MockTracer();
        var failure = new InvalidOperationException("decision failed");

        var error = await Assert.ThrowsAnyAsync<Exception>(() =>
            DecideOperation.DecideAsync(
                new DecideRequest
                {
                    Model = new DecideModel { FailWith = failure },
                    State = "Please refund me",
                    Questions = RefundQuestions(),
                    MaxRetries = 0,
                    Telemetry = Integration(tracer),
                }
            )
        );

        Assert.Same(failure, error);
        Assert.Equal(2, tracer.Spans.Count);
        foreach (var span in tracer.Spans)
        {
            Assert.True(span.Ended);
            Assert.Equal(ActivityStatusCode.Error, span.Activity.Status);
            Assert.Equal("decision failed", span.Activity.StatusDescription);
            Assert.Equal(1, span.ExceptionCount);
        }
    }
}
