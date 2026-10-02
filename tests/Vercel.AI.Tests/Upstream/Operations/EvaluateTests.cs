// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;

namespace Vercel.AI.Tests;

/// <summary>Upstream parity for <c>evaluate</c>.</summary>
public sealed class EvaluateTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/evaluate/evaluate.test.ts::evaluates mixed questions in one call and preserves distributions and metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Evaluates_mixed_questions_and_preserves_distributions()
    {
        var warning = OperationWarning.Other("Provider note");
        var timestamp = new DateTime(2026, 9, 16, 12, 0, 0, DateTimeKind.Utc);
        var metadata = OperationJson.Parse("{\"test\":{\"confidence\":0.8}}");
        var logged = new List<WarningLogContext>();
        WarningLog.Logger = new Action<WarningLogContext>(logged.Add);
        try
        {
            var model = new EvalModel();
            using var source = new CancellationTokenSource();
            var state = new Dictionary<string, object?> { ["message"] = "refund", ["history"] = new List<object?> { "hello" } };
            var questions = SampleQuestions();
            model.Result = new EvaluationModelResult(
                SampleAnswers(),
                new[] { warning },
                new OperationUsage(30, 4),
                providerMetadata: metadata,
                response: new ProviderResponse(new Dictionary<string, string> { ["x-request-id"] = "request" }, OperationJson.Parse("{\"raw\":true}"), "response", timestamp, "actual-model"));
            var result = await Evaluate.EvaluateAsync(new EvaluateRequest
            {
                Model = model,
                State = state,
                Questions = questions,
                CancellationToken = source.Token,
                Headers = new Dictionary<string, string> { ["custom"] = "value" },
                ProviderOptions = OperationJson.Parse("{\"test\":{\"option\":true}}"),
            });
            Assert.Single(model.Calls);
            Assert.Same(state, model.Calls[0].State);
            Assert.Equal("value", model.Calls[0].Headers["custom"]);
            Assert.Contains("ai/", model.Calls[0].Headers["user-agent"]);
            Assert.Equal((double?)0.92, result.Answers["refund"].Probability);
            Assert.Equal((double?)1.6, result.Answers["severity"].Score);
            Assert.Equal(0.6, result.Answers["severity"].Probabilities!["2"]);
            Assert.Equal((int?)30, result.Usage.InputTokens);
            Assert.Equal((int?)4, result.Usage.OutputTokens);
            Assert.Equal((int?)34, result.Usage.TotalTokens);
            Assert.Equal(0.8, result.ProviderMetadata!.Value.GetProperty("test").GetProperty("confidence").GetDouble());
            Assert.Equal("response", result.Response.Id);
            Assert.Equal((DateTime?)timestamp, result.Response.Timestamp);
            Assert.Equal("actual-model", result.Response.ModelId);
            Assert.Equal("Provider note", result.Warnings[0].Message);
            Assert.Equal("mock-provider", logged[0].Provider);
        }
        finally
        {
            WarningLog.Logger = null;
        }
    }

    [Fact]
    [UpstreamTest("packages/ai/src/evaluate/evaluate.test.ts::allows choice and score without distributions, with unknown usage left unknown", Coverage = UpstreamCoverage.Covered)]
    public async Task Allows_answers_without_distributions_and_leaves_usage_unknown()
    {
        var now = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var answers = new Dictionary<string, EvaluationAnswer>
        {
            ["topic"] = new EvaluationAnswer("choice", choice: "support"),
            ["severity"] = new EvaluationAnswer("score", score: 1.4),
            ["refund"] = new EvaluationAnswer("boolean", probability: 0.92),
        };
        var result = await Evaluate.EvaluateAsync(new EvaluateRequest
        {
            Model = new EvalModel { Result = new EvaluationModelResult(answers) },
            State = string.Empty,
            Questions = SampleQuestions(),
            Now = () => now,
        });
        Assert.Null(result.Answers["topic"].Probabilities);
        Assert.Null(result.Answers["severity"].Probabilities);
        Assert.Null(result.Usage.InputTokens);
        Assert.Null(result.Usage.OutputTokens);
        Assert.Null(result.Usage.TotalTokens);
        Assert.Equal((DateTime?)now, result.Response.Timestamp);
        Assert.Equal("mock-model-id", result.Response.ModelId);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/evaluate/evaluate.test.ts::rejects unsupported types before evaluating any question", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_unsupported_question_types_before_the_model()
    {
        var model = new EvalModel { Supported = new[] { "choice", "score" } };
        var error = await Assert.ThrowsAsync<EvaluationUnsupportedQuestionTypeException>(() => Evaluate.EvaluateAsync(new EvaluateRequest { Model = model, State = "refund", Questions = SampleQuestions() }));
        Assert.Equal("refund", error.QuestionId);
        Assert.Equal("boolean", error.QuestionType);
        Assert.Equal("mock-provider", error.Provider);
        Assert.Equal("mock-model-id", error.ModelId);
        Assert.Empty(model.Calls);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/evaluate/evaluate.test.ts::rejects an unsupported model version", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_an_unsupported_model_version()
    {
        var model = new EvalModel { SpecificationVersion = "v99" };
        var error = await Assert.ThrowsAsync<UnsupportedModelVersionException>(() => Evaluate.EvaluateAsync(new EvaluateRequest { Model = model, State = "text", Questions = SampleQuestions() }));
        Assert.Contains("v2", error.Message);
        Assert.Empty(model.Calls);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/evaluate/evaluate.test.ts::input validation::rejects non-JSON state %s before I/O", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_non_json_state_before_io()
    {
        var cyclic = new Dictionary<string, object?>();
        cyclic["self"] = cyclic;
        object?[] states =
        {
            null,
            true,
            42,
            EvaluationValues.Date(),
            new Dictionary<string, object?> { ["value"] = EvaluationValues.Undefined },
            new Dictionary<string, object?> { ["value"] = EvaluationValues.NotANumber },
            new Dictionary<string, object?> { ["fn"] = EvaluationValues.Function },
            cyclic,
            new List<object?> { EvaluationValues.Undefined },
            EvaluationValues.SparseArray,
        };
        foreach (var state in states)
        {
            var model = new EvalModel();
            await Assert.ThrowsAsync<InvalidArgumentException>(() => Evaluate.EvaluateAsync(new EvaluateRequest { Model = model, State = state, Questions = SampleQuestions() }));
            Assert.Empty(model.Calls);
        }
    }

    [Fact]
    [UpstreamTest("packages/ai/src/evaluate/evaluate.test.ts::input validation::rejects invalid questions %s before I/O", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_invalid_questions_before_io()
    {
        var cases = new IReadOnlyDictionary<string, EvaluationQuestion>?[]
        {
            new Dictionary<string, EvaluationQuestion>(),
            null,
            new Dictionary<string, EvaluationQuestion> { ["test"] = new EvaluationQuestion("other", "test") },
            new Dictionary<string, EvaluationQuestion> { ["test"] = new EvaluationQuestion("boolean", null) },
            new Dictionary<string, EvaluationQuestion> { ["test"] = new EvaluationQuestion("boolean", "test", new Dictionary<string, object?> { ["yes"] = "yes" }) },
            new Dictionary<string, EvaluationQuestion> { ["test"] = new EvaluationQuestion("choice", "test", new Dictionary<string, object?>()) },
            new Dictionary<string, EvaluationQuestion> { ["test"] = new EvaluationQuestion("choice", "test", new Dictionary<string, object?> { ["a"] = 42 }) },
            new Dictionary<string, EvaluationQuestion> { ["test"] = new EvaluationQuestion("score", "test", new List<object?> { "Only one" }) },
            new Dictionary<string, EvaluationQuestion> { ["test"] = new EvaluationQuestion("score", "test", new List<object?> { "Low", EvaluationValues.Undefined }) },
        };
        foreach (var questions in cases)
        {
            var model = new EvalModel();
            await Assert.ThrowsAsync<InvalidArgumentException>(() => Evaluate.EvaluateAsync(new EvaluateRequest { Model = model, State = "text", Questions = questions }));
            Assert.Empty(model.Calls);
        }
    }

    [Fact]
    [UpstreamTest("packages/ai/src/evaluate/evaluate.test.ts::input validation::allows repeated JSON references without treating them as cycles", Coverage = UpstreamCoverage.Covered)]
    public async Task Allows_repeated_json_references()
    {
        var shared = new Dictionary<string, object?> { ["text"] = "hello" };
        var result = await Evaluate.EvaluateAsync(new EvaluateRequest
        {
            Model = new EvalModel(),
            State = new List<object?> { shared, shared },
            Questions = SampleQuestions(),
        });
        Assert.Equal(3, result.Answers.Count);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/evaluate/evaluate.test.ts::response validation::accepts native rounded scores without rewriting probabilities or scores", Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_native_rounded_scores_without_rewriting()
    {
        var answers = SampleAnswers();
        answers["severity"] = new EvaluationAnswer("score", score: 0.97, probabilities: new Dictionary<string, double> { ["0"] = 0.13, ["1"] = 0.76, ["2"] = 0.11 });
        var rounding = new EvaluationRounding(2, 2);
        var result = await Evaluate.EvaluateAsync(new EvaluateRequest
        {
            Model = new EvalModel { Result = new EvaluationModelResult(answers, rounding: rounding) },
            State = "text",
            Questions = SampleQuestions(),
        });
        Assert.Equal((double?)0.97, result.Answers["severity"].Score);
        Assert.Equal(0.13, result.Answers["severity"].Probabilities!["0"]);
        Assert.Equal((int?)2, result.Rounding!.ScoreDecimals);
        await Assert.ThrowsAsync<InvalidResponseDataException>(() => Evaluate.EvaluateAsync(new EvaluateRequest
        {
            Model = new EvalModel { Result = new EvaluationModelResult(answers) },
            State = "text",
            Questions = SampleQuestions(),
        }));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/evaluate/evaluate.test.ts::response validation::accepts a rounded distribution sum but rejects errors beyond declared precision", Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_a_rounded_distribution_sum_within_precision()
    {
        var answers = SampleAnswers();
        answers["severity"] = new EvaluationAnswer("score", score: 1, probabilities: new Dictionary<string, double> { ["0"] = 0.33, ["1"] = 0.33, ["2"] = 0.33 });
        var rounding = new EvaluationRounding(2, 2);
        var result = await Evaluate.EvaluateAsync(new EvaluateRequest
        {
            Model = new EvalModel { Result = new EvaluationModelResult(answers, rounding: rounding) },
            State = "text",
            Questions = SampleQuestions(),
        });
        Assert.Equal((double?)1, result.Answers["severity"].Score);
        var drifted = SampleAnswers();
        drifted["severity"] = new EvaluationAnswer("score", score: 1.5, probabilities: answers["severity"].Probabilities);
        await Assert.ThrowsAsync<InvalidResponseDataException>(() => Evaluate.EvaluateAsync(new EvaluateRequest
        {
            Model = new EvalModel { Result = new EvaluationModelResult(drifted, rounding: rounding) },
            State = "text",
            Questions = SampleQuestions(),
        }));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/evaluate/evaluate.test.ts::response validation::rejects invalid rounding precision %s", Coverage = UpstreamCoverage.Partial, Note = "NaN and 1.5 are JavaScript numbers; EvaluationRounding stores integers, so -1 and 16 are the representable cases.")]
    public async Task Rejects_invalid_rounding_precision()
    {
        foreach (var decimals in new int?[] { -1, 16 })
        {
            await Assert.ThrowsAsync<InvalidResponseDataException>(() => Evaluate.EvaluateAsync(new EvaluateRequest
            {
                Model = new EvalModel { Result = new EvaluationModelResult(SampleAnswers(), rounding: new EvaluationRounding(decimals)) },
                State = "text",
                Questions = SampleQuestions(),
            }));
        }
    }

    [Fact]
    [UpstreamTest("packages/ai/src/evaluate/evaluate.test.ts::response validation::rejects malformed answers %s without retrying", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_malformed_answers_without_retrying()
    {
        var cases = new IReadOnlyDictionary<string, EvaluationAnswer>?[]
        {
            null,
            new Dictionary<string, EvaluationAnswer>(),
            With("extra", new EvaluationAnswer("boolean", probability: 0.92)),
            With("topic", new EvaluationAnswer("score", score: 1)),
            With("topic", new EvaluationAnswer("choice", choice: "other")),
            With("topic", new EvaluationAnswer("choice", choice: "toString")),
            With("topic", new EvaluationAnswer("choice", choice: "billing", probabilities: new Dictionary<string, double> { ["billing"] = 1 })),
            With("topic", new EvaluationAnswer("choice", choice: "billing", probabilities: new Dictionary<string, double> { ["billing"] = 0.1, ["support"] = 0.9 })),
            With("severity", new EvaluationAnswer("score", score: 3)),
            With("severity", new EvaluationAnswer("score", score: double.NaN)),
            With("severity", new EvaluationAnswer("score", score: 1, probabilities: new Dictionary<string, double> { ["0"] = 0, ["1"] = 0, ["2"] = 1 })),
            With("severity", new EvaluationAnswer("score", score: 1, probabilities: new Dictionary<string, double> { ["0"] = 0.2, ["1"] = 0.2, ["2"] = 0.2 })),
            With("severity", new EvaluationAnswer("score", score: 1, probabilities: new Dictionary<string, double> { ["0"] = -0.1, ["1"] = 1.1, ["2"] = 0 })),
            With("severity", new EvaluationAnswer("score", score: 1, probabilities: new Dictionary<string, double> { ["0"] = double.NaN, ["1"] = 1, ["2"] = 0 })),
            With("refund", new EvaluationAnswer("boolean")),
            With("refund", new EvaluationAnswer("boolean", probability: null)),
            With("refund", new EvaluationAnswer("boolean", probability: double.PositiveInfinity)),
            With("refund", new EvaluationAnswer("boolean", probability: 1.1)),
        };
        foreach (var answers in cases)
        {
            var model = new EvalModel { Result = new EvaluationModelResult(answers ?? new Dictionary<string, EvaluationAnswer>()) };
            if (answers == null)
            {
                model.Result = new EvaluationModelResult(new Dictionary<string, EvaluationAnswer>());
                model.ReturnNullAnswers = true;
            }

            await Assert.ThrowsAsync<InvalidResponseDataException>(() => Evaluate.EvaluateAsync(new EvaluateRequest { Model = model, State = "text", Questions = SampleQuestions(), MaxRetries = 2 }));
            Assert.Single(model.Calls);
        }
    }

    [Fact]
    [UpstreamTest("packages/ai/src/evaluate/evaluate.test.ts::response validation::accepts rounding within tolerance without modifying the distribution", Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_rounding_within_tolerance()
    {
        var probabilities = new Dictionary<string, double> { ["0"] = 0.3333333, ["1"] = 0.3333333, ["2"] = 0.3333333 };
        var answers = SampleAnswers();
        answers["severity"] = new EvaluationAnswer("score", score: 1, probabilities: probabilities);
        var result = await Evaluate.EvaluateAsync(new EvaluateRequest
        {
            Model = new EvalModel { Result = new EvaluationModelResult(answers) },
            State = "text",
            Questions = SampleQuestions(),
        });
        Assert.Equal(0.3333333, result.Answers["severity"].Probabilities!["0"]);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/evaluate/evaluate.test.ts::response validation::handles question and option keys that match object prototype properties", Coverage = UpstreamCoverage.Covered)]
    public async Task Handles_prototype_property_keys()
    {
        var questions = new Dictionary<string, EvaluationQuestion>
        {
            ["__proto__"] = new EvaluationQuestion("choice", "Which?", new Dictionary<string, object?> { ["__proto__"] = null, ["constructor"] = null }),
        };
        var answers = new Dictionary<string, EvaluationAnswer>
        {
            ["__proto__"] = new EvaluationAnswer("choice", choice: "__proto__"),
        };
        var result = await Evaluate.EvaluateAsync(new EvaluateRequest
        {
            Model = new EvalModel { Result = new EvaluationModelResult(answers) },
            State = "text",
            Questions = questions,
        });
        Assert.Equal(new[] { "__proto__" }, result.Answers.Keys.ToArray());
    }

    [Fact]
    [UpstreamTest("packages/ai/src/evaluate/evaluate.test.ts::retries transient errors with the configured retry limit", Coverage = UpstreamCoverage.Covered)]
    public async Task Retries_transient_errors_with_the_configured_limit()
    {
        var starts = 0;
        var ends = 0;
        var model = new EvalModel { Failures = 1 };
        await Evaluate.EvaluateAsync(new EvaluateRequest
        {
            Model = model,
            State = "text",
            Questions = SampleQuestions(),
            MaxRetries = 1,
            Telemetry = new EvaluateTelemetry
            {
                OnModelStart = _ => { starts++; return Task.CompletedTask; },
                OnModelEnd = _ => { ends++; return Task.CompletedTask; },
            },
        });
        Assert.Equal(2, model.Calls.Count);
        Assert.Equal(1, starts);
        Assert.Equal(1, ends);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/evaluate/evaluate.test.ts::honors maxRetries: 0", Coverage = UpstreamCoverage.Covered)]
    public async Task Honors_a_retry_limit_of_zero()
    {
        var model = new EvalModel { Failures = 5, FailureStatus = 529 };
        var error = await Assert.ThrowsAsync<RetryableCallException>(() => Evaluate.EvaluateAsync(new EvaluateRequest { Model = model, State = "text", Questions = SampleQuestions(), MaxRetries = 0 }));
        Assert.Equal("Overloaded", error.Message);
        Assert.Single(model.Calls);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/evaluate/evaluate.test.ts::does not call the provider when already aborted", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_call_the_provider_when_already_aborted()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        var reason = new InvalidOperationException("Cancelled");
        var model = new EvalModel();
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => Evaluate.EvaluateAsync(new EvaluateRequest
        {
            Model = model,
            State = "text",
            Questions = SampleQuestions(),
            CancellationToken = source.Token,
            AbortReason = reason,
        }));
        Assert.Same(reason, error);
        Assert.Empty(model.Calls);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/evaluate/evaluate.test.ts::does not return a result after cancellation during a call", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_return_a_result_after_cancellation_during_a_call()
    {
        using var source = new CancellationTokenSource();
        var reason = new InvalidOperationException("Cancelled");
        var model = new EvalModel { AbortDuringCall = (source, reason) };
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => Evaluate.EvaluateAsync(new EvaluateRequest
        {
            Model = model,
            State = "text",
            Questions = SampleQuestions(),
            CancellationToken = source.Token,
            AbortReason = reason,
        }));
        Assert.Same(reason, error);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/evaluate/evaluate.test.ts::telemetry::emits operation and model-call lifecycle events", Coverage = UpstreamCoverage.Covered)]
    public async Task Emits_operation_and_model_call_lifecycle_events()
    {
        var genericStarts = 0;
        var userStarts = new List<EvaluateEvent>();
        var telemetryStarts = new List<EvaluateEvent>();
        var modelStarts = new List<EvaluateModelEvent>();
        var modelEnds = new List<EvaluateModelEvent>();
        var telemetryEnds = new List<EvaluateEvent>();
        var state = new Dictionary<string, object?> { ["message"] = "refund" };
        await Evaluate.EvaluateAsync(new EvaluateRequest
        {
            Model = new EvalModel { Result = new EvaluationModelResult(SampleAnswers(), usage: new OperationUsage(30, 4)) },
            State = state,
            Questions = SampleQuestions(),
            GenerateCallId = () => "test-call-id",
            RuntimeContext = new Dictionary<string, object?> { ["requestId"] = "request-1", ["secret"] = "hidden" },
            OnStart = item => { userStarts.Add(item); return Task.CompletedTask; },
            OnEnd = item => { userStarts.Add(item); return Task.CompletedTask; },
            Telemetry = new EvaluateTelemetry
            {
                RecordInputs = false,
                RecordOutputs = true,
                FunctionId = "evaluate-test",
                OnEvaluateStart = item => { telemetryStarts.Add(item); return Task.CompletedTask; },
                OnEvaluateEnd = item => { telemetryEnds.Add(item); return Task.CompletedTask; },
                OnModelStart = item => { modelStarts.Add(item); return Task.CompletedTask; },
                OnModelEnd = item => { modelEnds.Add(item); return Task.CompletedTask; },
            },
        });
        Assert.Equal(0, genericStarts);
        Assert.Equal("test-call-id", telemetryStarts[0].CallId);
        Assert.Equal("ai.evaluate", telemetryStarts[0].OperationId);
        Assert.Empty(telemetryStarts[0].RuntimeContext);
        Assert.False(telemetryStarts[0].RecordInputs);
        Assert.True(telemetryStarts[0].RecordOutputs);
        Assert.Equal("evaluate-test", telemetryStarts[0].FunctionId);
        Assert.Equal(2, telemetryStarts[0].MaxRetries);
        Assert.Equal("ai.evaluate.doEvaluate", modelStarts[0].OperationId);
        Assert.Equal((int?)30, modelEnds[0].Usage!.InputTokens);
        Assert.Null(modelEnds[0].Usage.TotalTokens);
        Assert.Equal((int?)34, telemetryEnds[0].Usage!.TotalTokens);
        Assert.Equal("hidden", userStarts[0].RuntimeContext["secret"]);
        Assert.Null(userStarts[0].FunctionId);
        Assert.Equal("hidden", userStarts[1].RuntimeContext["secret"]);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/evaluate/evaluate.test.ts::telemetry::includes only selected runtime context fields", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_only_selected_runtime_context_fields()
    {
        EvaluateEvent? seen = null;
        await Evaluate.EvaluateAsync(new EvaluateRequest
        {
            Model = new EvalModel(),
            State = "text",
            Questions = SampleQuestions(),
            RuntimeContext = new Dictionary<string, object?> { ["requestId"] = "request-1", ["secret"] = "hidden" },
            Telemetry = new EvaluateTelemetry
            {
                IncludeRuntimeContext = new Dictionary<string, bool> { ["requestId"] = true },
                OnEvaluateStart = item => { seen = item; return Task.CompletedTask; },
            },
        });
        Assert.Equal("request-1", seen!.RuntimeContext["requestId"]);
        Assert.False(seen.RuntimeContext.ContainsKey("secret"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/evaluate/evaluate.test.ts::telemetry::emits an error event when evaluation fails", Coverage = UpstreamCoverage.Covered)]
    public async Task Emits_an_error_event_when_evaluation_fails()
    {
        string? callId = null;
        Exception? seen = null;
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => Evaluate.EvaluateAsync(new EvaluateRequest
        {
            Model = new EvalModel { FailWith = new InvalidOperationException("evaluation failed") },
            State = "text",
            Questions = SampleQuestions(),
            MaxRetries = 0,
            GenerateCallId = () => "test-call-id",
            Telemetry = new EvaluateTelemetry { OnError = (id, exception) => { callId = id; seen = exception; return Task.CompletedTask; } },
        }));
        Assert.Equal("evaluation failed", error.Message);
        Assert.Equal("test-call-id", callId);
        Assert.Same(error, seen);
    }

    private static Dictionary<string, EvaluationQuestion> SampleQuestions()
    {
        return new Dictionary<string, EvaluationQuestion>
        {
            ["topic"] = new EvaluationQuestion("choice", "Team?", new Dictionary<string, object?> { ["billing"] = null, ["support"] = new Dictionary<string, object?> { ["includes"] = new List<object?> { "help" } } }),
            ["severity"] = new EvaluationQuestion("score", new List<object?> { "Severity?" }, new List<object?> { "Low", "Medium", "High" }),
            ["refund"] = new EvaluationQuestion("boolean", "Refund?", new Dictionary<string, object?> { ["true"] = "Money back", ["false"] = null }),
        };
    }

    private static Dictionary<string, EvaluationAnswer> SampleAnswers()
    {
        return new Dictionary<string, EvaluationAnswer>
        {
            ["refund"] = new EvaluationAnswer("boolean", probability: 0.92),
            ["severity"] = new EvaluationAnswer("score", score: 1.6, probabilities: new Dictionary<string, double> { ["0"] = 0, ["1"] = 0.4, ["2"] = 0.6 }),
            ["topic"] = new EvaluationAnswer("choice", choice: "billing", probabilities: new Dictionary<string, double> { ["billing"] = 0.9, ["support"] = 0.1 }),
        };
    }

    private static Dictionary<string, EvaluationAnswer> With(string key, EvaluationAnswer replacement)
    {
        var answers = SampleAnswers();
        answers[key] = replacement;
        return answers;
    }

    private sealed class EvalModel : IEvaluationCaller
    {
        public string Provider => "mock-provider";

        public string ModelId => "mock-model-id";

        public string SpecificationVersion { get; set; } = "v4";

        public IReadOnlyList<string> Supported { get; set; } = new[] { "choice", "score", "boolean" };

        public IReadOnlyList<string> SupportedQuestionTypes => Supported;

        public EvaluationModelResult Result { get; set; } = new EvaluationModelResult(SampleAnswers());

        public bool ReturnNullAnswers { get; set; }

        public int Failures { get; set; }

        public int FailureStatus { get; set; } = 429;

        public Exception? FailWith { get; set; }

        public (CancellationTokenSource Source, Exception Reason)? AbortDuringCall { get; set; }

        public List<EvaluationModelCall> Calls { get; } = new List<EvaluationModelCall>();

        public Task<EvaluationModelResult> DoEvaluateAsync(EvaluationModelCall call, CancellationToken cancellationToken)
        {
            Calls.Add(call);
            if (AbortDuringCall is { } abort)
            {
                abort.Source.Cancel();
                return Task.FromResult(Result);
            }

            if (FailWith != null)
            {
                throw FailWith;
            }

            if (Failures > 0)
            {
                Failures--;
                var message = FailureStatus == 529 ? "Overloaded" : "Rate limited";
                throw new RetryableCallException(message, FailureStatus, new Dictionary<string, string> { ["retry-after-ms"] = "0" });
            }

            if (ReturnNullAnswers)
            {
                return Task.FromResult(new EvaluationModelResult(new Dictionary<string, EvaluationAnswer>()));
            }

            return Task.FromResult(Result);
        }
    }
}
