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

    [Fact]
    [UpstreamTest(
        Test + "creates decision spans through the %s API",
        Coverage = UpstreamCoverage.Partial,
        Commit = UpstreamCommit,
        Note = "Only the current API is checked: the port's decide models do not report a response, so the model-call span has no gen_ai.response.id or gen_ai.response.model, and the deprecated experimental_evaluate path is not run."
    )]
    public async Task Creates_decision_spans_through_the_current_API()
    {
        using var tracer = new MockTracer();

        await DecideOperation.DecideAsync(
            new DecideRequest
            {
                Model = new DecideModel
                {
                    Result = new DecisionModelResult(
                        new Dictionary<string, DecisionAnswer>
                        {
                            ["refund"] = new DecisionAnswer("boolean", probability: 0.9),
                        },
                        usage: new OperationUsage(12, 2)
                    ),
                },
                State = S(new Dictionary<string, object?> { ["message"] = "Please refund me" }),
                Questions = RefundQuestions(),
                Telemetry = Integration(tracer, experimentalDecision: true),
            }
        );

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
                ["gen_ai.usage.input_tokens"] = 12,
                ["gen_ai.usage.output_tokens"] = 2,
            },
            spans[1].RuntimeAttributes
        );
    }

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
