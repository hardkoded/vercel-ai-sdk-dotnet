// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using Vercel.AI.Util;
using static Vercel.AI.Tests.DecisionFixtures;
using static Vercel.AI.Tests.Upstream.Operations.Decide.DecideSupport;
using DecideOperation = Vercel.AI.Operations.Decide;

namespace Vercel.AI.Tests.Upstream.Operations.Decide;

/// <summary>Keeps tests that replace <c>Download.Fetch</c> from racing each other.</summary>
[CollectionDefinition("DownloadFetch", DisableParallelization = true)]
public sealed class DownloadFetchCollection
{
}

/// <summary>Port of <c>decide.test.ts</c>.</summary>
[Collection("DownloadFetch")]
public sealed class DecideTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::decides answers to mixed questions in one call and preserves distributions and metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Decides_answers_to_mixed_questions_in_one_call_and_preserves_distributions_and_metadata()
    {
        var warning = OperationWarning.Other("Provider note");
        var timestamp = new DateTime(2026, 9, 16, 12, 0, 0, DateTimeKind.Utc);
        var metadata = OperationJson.Parse("{\"test\":{\"confidence\":0.8}}");
        var logged = new List<WarningLogContext>();
        WarningLog.Logger = new Action<WarningLogContext>(logged.Add);
        try
        {
            var model = new DecideModel();
            using var source = new CancellationTokenSource();
            var state = new Dictionary<string, object?> { ["message"] = "refund", ["history"] = new List<object?> { "hello" } };
            var questions = SampleQuestions();
            model.Result = new DecisionModelResult(
                SampleAnswers(),
                new[] { warning },
                new OperationUsage(30, 4),
                providerMetadata: metadata,
                response: new ProviderResponse(new Dictionary<string, string> { ["x-request-id"] = "request" }, OperationJson.Parse("{\"raw\":true}"), "response", timestamp, "actual-model"));
            var result = await DecideOperation.DecideAsync(new DecideRequest
            {
                Model = model,
                State = S(state),
                Questions = questions,
                CancellationToken = source.Token,
                Headers = new Dictionary<string, string> { ["custom"] = "value" },
                ProviderOptions = OperationJson.Parse("{\"test\":{\"option\":true}}"),
            });
            Assert.Single(model.Calls);
            var part = Assert.IsType<DecisionModelJsonPart>(Assert.Single(model.Calls[0].State));
            Assert.Equal(J(state), part.Value.GetRawText());
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
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::allows choice and score without distributions, with unknown usage left unknown", Coverage = UpstreamCoverage.Covered)]
    public async Task Allows_choice_and_score_without_distributions_with_unknown_usage_left_unknown()
    {
        var now = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var answers = new Dictionary<string, DecisionAnswer>
        {
            ["topic"] = new DecisionAnswer("choice", choice: "support"),
            ["severity"] = new DecisionAnswer("score", score: 1.4),
            ["refund"] = new DecisionAnswer("boolean", probability: 0.92),
        };
        var result = await DecideOperation.DecideAsync(new DecideRequest
        {
            Model = new DecideModel { Result = new DecisionModelResult(answers) },
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
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::rejects unsupported types before deciding any answer", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_unsupported_types_before_deciding_any_answer()
    {
        var model = new DecideModel { Supported = new[] { "choice", "score" } };
        var error = await Assert.ThrowsAnyAsync<DecisionUnsupportedQuestionTypeException>(() => DecideOperation.DecideAsync(new DecideRequest { Model = model, State = "refund", Questions = SampleQuestions() }));
        Assert.Equal("AI_DecisionUnsupportedQuestionTypeError", error.ErrorName);
        Assert.Equal("refund", error.QuestionId);
        Assert.Equal("boolean", error.QuestionType);
        Assert.Equal("mock-provider", error.Provider);
        Assert.Equal("mock-model-id", error.ModelId);
        Assert.True(DecisionUnsupportedQuestionTypeError.IsInstance(error));
        Assert.Empty(model.Calls);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::rejects an unsupported model version", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_an_unsupported_model_version()
    {
        var model = new DecideModel { SpecificationVersion = "v99" };
        var error = await Assert.ThrowsAsync<UnsupportedModelVersionException>(() => DecideOperation.DecideAsync(new DecideRequest { Model = model, State = "text", Questions = SampleQuestions() }));
        Assert.Contains("v2", error.Message);
        Assert.Empty(model.Calls);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::retries transient errors with the configured retry limit", Coverage = UpstreamCoverage.Covered)]
    public async Task Retries_transient_errors_with_the_configured_retry_limit()
    {
        var starts = 0;
        var ends = 0;
        var model = new DecideModel { Failures = 1 };
        await DecideOperation.DecideAsync(new DecideRequest
        {
            Model = model,
            State = "text",
            Questions = SampleQuestions(),
            MaxRetries = 1,
            Telemetry = new DecideTelemetry
            {
                OnDecisionModelCallStart = _ => { starts++; return Task.CompletedTask; },
                OnDecisionModelCallEnd = _ => { ends++; return Task.CompletedTask; },
            },
        });
        Assert.Equal(2, model.Calls.Count);
        Assert.Equal(1, starts);
        Assert.Equal(1, ends);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::honors maxRetries: 0", Coverage = UpstreamCoverage.Covered)]
    public async Task Honors_maxRetries_0()
    {
        var model = new DecideModel { Failures = 5, FailureStatus = 529 };
        var error = await Assert.ThrowsAsync<RetryableCallException>(() => DecideOperation.DecideAsync(new DecideRequest { Model = model, State = "text", Questions = SampleQuestions(), MaxRetries = 0 }));
        Assert.Equal("Overloaded", error.Message);
        Assert.Single(model.Calls);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::does not call the provider when already aborted", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_call_the_provider_when_already_aborted()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        var reason = new InvalidOperationException("Cancelled");
        var model = new DecideModel();
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => DecideOperation.DecideAsync(new DecideRequest
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
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::does not return a result after cancellation during a call", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_return_a_result_after_cancellation_during_a_call()
    {
        using var source = new CancellationTokenSource();
        var reason = new InvalidOperationException("Cancelled");
        var model = new DecideModel { AbortDuringCall = (source, reason) };
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => DecideOperation.DecideAsync(new DecideRequest
        {
            Model = model,
            State = "text",
            Questions = SampleQuestions(),
            CancellationToken = source.Token,
            AbortReason = reason,
        }));
        Assert.Same(reason, error);
    }

    [Theory]
    [InlineData("topic")]
    [InlineData("severity")]
    [InlineData("refund")]
    [InlineData("topic", "refund")]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::rejects refused questions with a refusal error without retrying (%s)", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_refused_questions_with_a_refusal_error_without_retrying(params string[] refused)
    {
        var answers = SampleAnswers();
        foreach (var id in refused)
        {
            answers[id] = new DecisionAnswer("refusal");
        }

        var model = new DecideModel { Result = new DecisionModelResult(answers, Array.Empty<OperationWarning>()) };
        var error = await Assert.ThrowsAsync<DecisionRefusalError>(() => DecideOperation.DecideAsync(new DecideRequest { Model = model, State = "text", Questions = SampleQuestions(), MaxRetries = 2 }));
        Assert.Equal(refused, error.QuestionIds);
        Assert.Equal("mock-provider", error.Provider);
        Assert.Equal("mock-model-id", error.ModelId);
        Assert.Single(model.Calls);
    }

    public static IEnumerable<object[]> PublicStates()
    {
        yield return new object[] { "Inspect this package.", new List<object?> { TextPart("Inspect this package.") } };
        yield return new object[] { new Dictionary<string, object?> { ["product"] = "glass vase" }, new List<object?> { JsonPart(new Dictionary<string, object?> { ["product"] = "glass vase" }) } };
        yield return new object[] { new List<object?>(), new List<object?>() };
        yield return new object[]
        {
            new List<object?> { JsonPart(new List<object?> { 1, null }), TextPart("Inspect.") },
            new List<object?> { JsonPart(new List<object?> { 1, null }), TextPart("Inspect.") },
        };
    }

    [Fact]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::preserves ordered state parts and normalizes file shorthands", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_ordered_state_parts_and_normalizes_file_shorthands()
    {
        var model = new DecideModel();
        var bytes = new byte[] { 0x89, 0x50, 0x4e, 0x47 };
        await DecideOperation.DecideAsync(new DecideRequest
        {
            Model = model,
            Questions = SampleQuestions(),
            State = S(new List<object?>
            {
                TextPart(string.Empty),
                JsonPart(new List<object?> { 1, null, new Dictionary<string, object?> { ["label"] = "package" } }),
                new Dictionary<string, object?> { ["type"] = "file", ["mediaType"] = "image", ["data"] = FilePartInput.FromArrayBuffer(bytes), ["filename"] = "package.png" },
                new Dictionary<string, object?> { ["type"] = "file", ["mediaType"] = "image", ["data"] = new Uri("data:image/png;base64,iVBORw==") },
                TextPart("Inspect both images."),
            }),
        });
        Assert.Equal(
            J(new List<object?>
            {
                TextPart(string.Empty),
                JsonPart(new List<object?> { 1, null, new Dictionary<string, object?> { ["label"] = "package" } }),
                FilePart("image/png", bytes, "package.png"),
                FilePart("image/png", "iVBORw==", null),
                TextPart("Inspect both images."),
            }),
            J(Canon(model.Calls[0].State)));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::treats an empty array as an empty list of state parts", Coverage = UpstreamCoverage.Covered)]
    public async Task Treats_an_empty_array_as_an_empty_list_of_state_parts()
    {
        var model = new DecideModel();
        await DecideOperation.DecideAsync(new DecideRequest { Model = model, Questions = SampleQuestions(), State = S(new List<object?>()) });
        Assert.Empty(model.Calls[0].State);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::rejects malformed state parts before I/O: %j", Coverage = UpstreamCoverage.Partial, Note = "Typed parts cannot be malformed. Only a missing state and a null part can be built. The other upstream cases (a string, an untyped object, an unknown part type, text that is not a string, a file without data, a json part without a value, and a json part with NaN) do not compile or throw when the part is created.")]
    public async Task Rejects_malformed_state_parts_before_IO()
    {
        var model = new DecideModel();
        await Assert.ThrowsAsync<InvalidArgumentException>(() => DecideOperation.DecideAsync(new DecideRequest { Model = model, Questions = SampleQuestions(), State = null }));
        await Assert.ThrowsAsync<InvalidArgumentException>(() => DecideOperation.DecideAsync(new DecideRequest { Model = model, Questions = SampleQuestions(), State = DecisionState.From(new DecisionStatePart[] { null! }) }));
        Assert.Empty(model.Calls);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::downloads image URLs and sends their bytes to the decision model", Coverage = UpstreamCoverage.Covered)]
    public async Task Downloads_image_URLs_and_sends_their_bytes_to_the_decision_model()
    {
        var model = new DecideModel();
        var bytes = new byte[] { 0x89, 0x50, 0x4e, 0x47 };
        var fetches = 0;
        var previous = Download.Fetch;
        try
        {
            Download.Fetch = (url, request) =>
            {
                fetches++;
                return Task.FromResult(DownloadResponse.Bytes(bytes, "image/png"));
            };
            await DecideOperation.DecideAsync(new DecideRequest
            {
                Model = model,
                Questions = SampleQuestions(),
                State = S(new List<object?> { new Dictionary<string, object?> { ["type"] = "file", ["mediaType"] = "image", ["data"] = new Uri("https://example.com/package.png") } }),
            });
        }
        finally
        {
            Download.Fetch = previous;
        }

        Assert.Equal(1, fetches);
        Assert.Equal(J(new List<object?> { FilePart("image/png", bytes, null) }), J(Canon(model.Calls[0].State)));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::stops before provider I/O when an image download fails", Coverage = UpstreamCoverage.Covered)]
    public async Task Stops_before_provider_IO_when_an_image_download_fails()
    {
        var model = new DecideModel();
        var previous = Download.Fetch;
        try
        {
            Download.Fetch = (url, request) => Task.FromResult(new DownloadResponse(404, "Not Found", null, null));
            var error = await Assert.ThrowsAsync<DownloadError>(() => DecideOperation.DecideAsync(new DecideRequest
            {
                Model = model,
                Questions = SampleQuestions(),
                State = S(new List<object?> { new Dictionary<string, object?> { ["type"] = "file", ["mediaType"] = "image/png", ["data"] = new Uri("https://example.com/missing.png") } }),
            }));
            Assert.Equal("AI_DownloadError", error.ErrorName);
        }
        finally
        {
            Download.Fetch = previous;
        }

        Assert.Empty(model.Calls);
    }

    [Theory]
    [MemberData(nameof(PublicStates))]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::normalizes public state into provider parts: $state", Coverage = UpstreamCoverage.Covered)]
    public async Task Normalizes_public_state_into_provider_parts(object state, List<object?> expected)
    {
        var publicState = S(state);
        var model = new DecideModel();
        var started = new List<DecideEvent>();
        var modelStarted = new List<DecisionModelEvent>();
        await DecideOperation.DecideAsync(new DecideRequest
        {
            Model = model,
            Questions = SampleQuestions(),
            State = publicState,
            OnStart = item => { started.Add(item); return Task.CompletedTask; },
            Telemetry = new DecideTelemetry { OnDecisionModelCallStart = item => { modelStarted.Add(item); return Task.CompletedTask; } },
        });
        Assert.Equal(J(expected), J(Canon(model.Calls[0].State)));
        Assert.Same(publicState, Assert.Single(started).State);
        Assert.Equal(J(expected), J(Canon(Assert.Single(modelStarted).State)));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::preserves structured questions for provider calls and all lifecycle events", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_structured_questions_for_provider_calls_and_all_lifecycle_events()
    {
        var publicQuestions = StructuredQuestions();
        var model = new DecideModel
        {
            Result = new DecisionModelResult(new Dictionary<string, DecisionAnswer>
            {
                ["topic"] = new DecisionAnswer("choice", choice: "billing"),
                ["severity"] = new DecisionAnswer("score", score: 1.25),
                ["refund"] = new DecisionAnswer("boolean", probability: 0.8),
                ["noCriteria"] = new DecisionAnswer("boolean", probability: 0.5),
                ["emptyCriteria"] = new DecisionAnswer("boolean", probability: 0.5),
                ["nullCriteria"] = new DecisionAnswer("boolean", probability: 0.5),
                ["falseCriteria"] = new DecisionAnswer("boolean", probability: 0.5),
            }),
        };
        var seen = new List<IReadOnlyDictionary<string, DecisionQuestion>>();
        await DecideOperation.DecideAsync(new DecideRequest
        {
            Model = model,
            State = "package",
            Questions = publicQuestions,
            OnStart = item => { seen.Add(item.Questions); return Task.CompletedTask; },
            OnEnd = item => { seen.Add(item.Questions); return Task.CompletedTask; },
            Telemetry = new DecideTelemetry
            {
                OnDecisionModelCallStart = item => { seen.Add(item.Questions); return Task.CompletedTask; },
                OnDecisionModelCallEnd = item => { seen.Add(item.Questions); return Task.CompletedTask; },
            },
        });
        Assert.Same(publicQuestions, model.Calls[0].Questions);
        Assert.Equal(4, seen.Count);
        Assert.All(seen, item => Assert.Same(publicQuestions, item));
        var original = StructuredQuestions();
        Assert.Equal(original.Keys, publicQuestions.Keys);
        foreach (var pair in original)
        {
            Assert.Equal(pair.Value.Type, publicQuestions[pair.Key].Type);
            Assert.Equal(pair.Value.Instructions?.ToString(), publicQuestions[pair.Key].Instructions?.ToString());
            Assert.Equal(pair.Value.Criteria?.ToNode().ToJsonString(), publicQuestions[pair.Key].Criteria?.ToNode().ToJsonString());
        }
    }

    private static Dictionary<string, DecisionQuestion> BoxQuestions()
    {
        return new Dictionary<string, DecisionQuestion> { ["target"] = Q("box", "The cat") };
    }

    private static DecisionBox ValidBox(double? top = null, double? bottom = null, double? left = null, double? right = null)
    {
        return new DecisionBox(top ?? 0.25, bottom ?? 0.82, left ?? 0.44, right ?? 0.84);
    }

    private static DecideModel BoxModel(DecisionBox? box)
    {
        return new DecideModel
        {
            Supported = new[] { "box" },
            Result = new DecisionModelResult(new Dictionary<string, DecisionAnswer> { ["target"] = new DecisionAnswer("box", box: box) }),
        };
    }

    [Fact]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::preserves normalized box coordinates", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_normalized_box_coordinates()
    {
        var result = await DecideOperation.DecideAsync(new DecideRequest
        {
            Model = BoxModel(ValidBox()),
            State = string.Empty,
            Questions = BoxQuestions(),
        });
        var answer = result.Answers["target"];
        Assert.Equal("box", answer.Type);
        Assert.Equal(0.25, answer.Box!.Top);
        Assert.Equal(0.82, answer.Box.Bottom);
        Assert.Equal(0.44, answer.Box.Left);
        Assert.Equal(0.84, answer.Box.Right);
    }

    public static IEnumerable<object?[]> MalformedBoxes()
    {
        yield return new object?[] { null };
        yield return new object?[] { ValidBox(top: -0.1) };
        yield return new object?[] { ValidBox(bottom: 1.1) };
        yield return new object?[] { ValidBox(left: double.NaN) };
        yield return new object?[] { ValidBox(right: double.PositiveInfinity) };
        yield return new object?[] { ValidBox(left: 0.9) };
        yield return new object?[] { ValidBox(bottom: 0.2) };
        yield return new object?[] { ValidBox(right: 0.44) };
        yield return new object?[] { ValidBox(bottom: 0.25) };
    }

    [Theory]
    [MemberData(nameof(MalformedBoxes))]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::rejects malformed box coordinates: %j", Coverage = UpstreamCoverage.Partial, Note = "DecisionBox is typed, so the string coordinate, empty array, empty object, and extra-key cases cannot be built. A missing box stands in for undefined, null, [], and {}.")]
    public async Task Rejects_malformed_box_coordinates(DecisionBox? box)
    {
        await Assert.ThrowsAsync<InvalidResponseDataException>(() => DecideOperation.DecideAsync(new DecideRequest
        {
            Model = BoxModel(box),
            State = string.Empty,
            Questions = BoxQuestions(),
        }));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::passes a single Score level to the provider and accepts its zero score", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_a_single_Score_level_to_the_provider_and_accepts_its_zero_score()
    {
        var answers = new Dictionary<string, DecisionAnswer>
        {
            ["severity"] = new DecisionAnswer("score", score: 0, probabilities: new Dictionary<string, double> { ["0"] = 1 }),
        };
        var model = new DecideModel { Result = new DecisionModelResult(answers) };
        var questions = new Dictionary<string, DecisionQuestion> { ["severity"] = Q("score", null, new List<object?> { "Product unusable" }) };
        var result = await DecideOperation.DecideAsync(new DecideRequest { Model = model, State = "The product is broken.", Questions = questions });
        Assert.Same(questions, model.Calls[0].Questions);
        Assert.Equal(0, result.Answers["severity"].Score);
        Assert.Equal(new Dictionary<string, double> { ["0"] = 1 }, result.Answers["severity"].Probabilities);
    }

    public static IEnumerable<object?[]> InvalidSingleLevelScoreAnswers()
    {
        yield return new object?[] { 1.0, new Dictionary<string, double> { ["0"] = 1 } };
        yield return new object?[] { 0.0, new Dictionary<string, double> { ["0"] = 0.5 } };
        yield return new object?[] { 0.0, new Dictionary<string, double> { ["0"] = 0.5, ["1"] = 0.5 } };
    }

    [Theory]
    [MemberData(nameof(InvalidSingleLevelScoreAnswers))]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::rejects invalid single-level Score answers: %j", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_invalid_single_level_Score_answers(double score, Dictionary<string, double> probabilities)
    {
        var answers = new Dictionary<string, DecisionAnswer> { ["severity"] = new DecisionAnswer("score", score: score, probabilities: probabilities) };
        await Assert.ThrowsAsync<InvalidResponseDataException>(() => DecideOperation.DecideAsync(new DecideRequest
        {
            Model = new DecideModel { Result = new DecisionModelResult(answers) },
            State = "The product is broken.",
            Questions = new Dictionary<string, DecisionQuestion> { ["severity"] = Q("score", null, new List<object?> { "Product unusable" }) },
        }));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(1.0)]
    [InlineData(512.0)]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::forwards maxOutputTokens: %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Forwards_maxOutputTokens(double? maxOutputTokens)
    {
        var model = new DecideModel();
        await DecideOperation.DecideAsync(new DecideRequest { Model = model, State = "test", Questions = SampleQuestions(), MaxOutputTokens = maxOutputTokens });
        Assert.Equal(maxOutputTokens, model.Calls[0].MaxOutputTokens);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    [InlineData(1.5)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::rejects invalid maxOutputTokens before calling the model: %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_invalid_maxOutputTokens_before_calling_the_model(double maxOutputTokens)
    {
        var model = new DecideModel();
        await Assert.ThrowsAsync<InvalidArgumentError>(() => DecideOperation.DecideAsync(new DecideRequest { Model = model, State = "test", Questions = SampleQuestions(), MaxOutputTokens = maxOutputTokens }));
        Assert.Empty(model.Calls);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("provider-default")]
    [InlineData("none")]
    [InlineData("minimal")]
    [InlineData("low")]
    [InlineData("medium")]
    [InlineData("high")]
    [InlineData("xhigh")]
    [InlineData("max")]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::forwards reasoning and reports it in lifecycle callbacks: %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Forwards_reasoning_and_reports_it_in_lifecycle_callbacks(string? reasoning)
    {
        var model = new DecideModel();
        DecideEvent? start = null;
        DecideEvent? end = null;
        await DecideOperation.DecideAsync(new DecideRequest
        {
            Model = model,
            State = "test",
            Questions = SampleQuestions(),
            Reasoning = reasoning,
            OnStart = e =>
            {
                start = e;
                return Task.CompletedTask;
            },
            OnEnd = e =>
            {
                end = e;
                return Task.CompletedTask;
            },
        });
        Assert.Equal(reasoning, model.Calls[0].Reasoning);
        Assert.Equal(reasoning, start!.Reasoning);
        Assert.Equal(reasoning, end!.Reasoning);
    }
}
