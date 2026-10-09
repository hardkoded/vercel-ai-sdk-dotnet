// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.OpenAI.OpenAIDecision.OpenAIDecisionSupport;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIDecision;

/// <summary>Port of <c>openai-decision.test.ts</c>.</summary>
public sealed class OpenAIDecisionTests
{
    private const string Prefix = "packages/openai/src/openai-decision.test.ts::";

    public static TheoryData<int> BadAnswerSets()
    {
        return new TheoryData<int> { 0, 1, 2, 3 };
    }

    public static TheoryData<int> MalformedBodies()
    {
        return new TheoryData<int> { 0, 1, 2, 3 };
    }

    public static TheoryData<string> AbsentOrNull()
    {
        return new TheoryData<string> { "undefined", "null" };
    }

    public static TheoryData<string?, string> BooleanCriteria()
    {
        return new TheoryData<string?, string>
        {
            { null, "Refund?" },
            { "{}", "Refund?" },
            { "{\"true\":null,\"false\":null}", "Refund?" },
            { "{\"true\":\"Money back\",\"false\":null}", "Refund?\n\nCriteria for true:\nMoney back" },
            { "{\"false\":[\"Status request\"]}", "Refund?\n\nCriteria for false:\n[\"Status request\"]" },
        };
    }

    public static TheoryData<string> SafetyIdentifiers()
    {
        return new TheoryData<string> { string.Empty, "user-123", new string('x', 128) };
    }

    public static TheoryData<string> InvalidSafetyIdentifiers()
    {
        return new TheoryData<string> { "123", "null", "\"" + new string('x', 129) + "\"" };
    }

    public static TheoryData<string> Names()
    {
        return new TheoryData<string> { "department", "severity", "refund" };
    }

    [Fact]
    [UpstreamTest(Prefix + "sends all primitives directly to Decisions with configured authentication and headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_all_primitives_directly_to_Decisions_with_configured_authentication_and_headers()
    {
        var (model, capture) = Setup();
        await model.DoDecideAsync(Call(headers: new Dictionary<string, string> { ["shared"] = "call" }));
        Assert.Equal("openai.decision", model.Provider);
        Assert.Equal("v4", model.SpecificationVersion);
        Assert.Equal(new[] { "choice", "score", "boolean" }, model.SupportedQuestionTypes);
        Assert.Equal(1, capture.Calls);
        Assert.Equal("https://example.com/v1/decisions", capture.Uri);
        Assert.Equal("Bearer test-key", capture.Headers["Authorization"]);
        Assert.Equal("org-test", capture.Headers["OpenAI-Organization"]);
        Assert.Equal("proj-test", capture.Headers["OpenAI-Project"]);
        Assert.Equal("configured", capture.Headers["x-provider"]);
        Assert.Equal("call", capture.Headers["shared"]);
        Assert.Contains("ai-sdk/openai/", capture.Headers["User-Agent"]);
        JsonAssert.Equal(RequestBody(capture), """
            {
              "model": "gpt-6-luna",
              "input": "A billing issue with a workaround.",
              "questions": [
                {
                  "type": "choice",
                  "name": "department",
                  "instructions": "Pick the team.",
                  "choices": [
                    { "value": "technical", "description": "Bugs" },
                    { "value": "billing", "description": "Charges" }
                  ]
                },
                {
                  "type": "score",
                  "name": "severity",
                  "instructions": "Rate severity.",
                  "levels": [
                    { "label": "0", "description": "Low" },
                    { "label": "1", "description": "Medium" },
                    { "label": "2", "description": "High" }
                  ]
                },
                {
                  "type": "predicate",
                  "name": "refund",
                  "instructions": "Is a refund requested?"
                }
              ]
            }
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves native values, maps reordered named answers, and exposes confidence and response metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_native_values_maps_reordered_named_answers_and_exposes_confidence_and_response_metadata()
    {
        var reversed = new JsonArray(Fixture()["answers"]!.AsArray().Reverse().Select(answer => answer!.DeepClone()).ToArray());
        var body = new JsonObject { ["answers"] = reversed };
        var (model, _) = Setup(body);
        var result = await model.DoDecideAsync(Call());
        Assert.Equal(3, result.Answers.Count);
        AssertAnswer(result.Answers["department"], "choice", choice: "billing", probabilities: new[] { ("technical", 0.08), ("billing", 0.92) });
        AssertAnswer(result.Answers["severity"], "score", score: 0.98, probabilities: new[] { ("0", 0.08), ("1", 0.86), ("2", 0.06) });
        AssertAnswer(result.Answers["refund"], "boolean", probability: 0.96);
        JsonAssert.Equal(result.ProviderMetadata!.Value, "{\"openai\":{\"confidence\":{\"department\":0.88,\"severity\":0.79}}}");
        Assert.Equal(2, result.Rounding!.ProbabilityDecimals);
        Assert.Equal(2, result.Rounding.ScoreDecimals);
        Assert.Null(result.Usage);
        JsonAssert.Equal(result.Response!.Body!.Value, body.ToJsonString());
        Assert.Equal("gpt-6-luna", result.Response.ModelId);
        Assert.Equal("req-test", result.Response.Headers!["x-request-id"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "serializes structured input and rubrics, omits null descriptions, and preserves boolean criteria", Coverage = UpstreamCoverage.Covered)]
    public async Task Serializes_structured_input_and_rubrics_omits_null_descriptions_and_preserves_boolean_criteria()
    {
        var (model, capture) = Setup();
        var questions = new Dictionary<string, EvaluationQuestion>
        {
            ["department"] = new EvaluationQuestion(
                "choice",
                new Dictionary<string, object?> { ["task"] = "route" },
                new Dictionary<string, object?>
                {
                    ["technical"] = null,
                    ["billing"] = new Dictionary<string, object?> { ["rubric"] = new List<object?> { "charges" } },
                }),
            ["severity"] = new EvaluationQuestion(
                "score",
                new List<object?> { "severity" },
                new List<object?> { null, new Dictionary<string, object?> { ["meaning"] = "high" } }),
            ["refund"] = new EvaluationQuestion(
                "boolean",
                "Refund?",
                new Dictionary<string, object?>
                {
                    ["true"] = new Dictionary<string, object?> { ["meaning"] = "Explicit request for money back" },
                    ["false"] = "The customer only asks about refund status.",
                }),
        };
        await model.DoDecideAsync(Call(questions, new Dictionary<string, object?> { ["ticket"] = new List<object?> { "charged twice" } }));
        JsonAssert.Equal(RequestBody(capture), """
            {
              "model": "gpt-6-luna",
              "input": "{\"ticket\":[\"charged twice\"]}",
              "questions": [
                {
                  "name": "department",
                  "type": "choice",
                  "instructions": "{\"task\":\"route\"}",
                  "choices": [
                    { "value": "technical" },
                    { "value": "billing", "description": "{\"rubric\":[\"charges\"]}" }
                  ]
                },
                {
                  "name": "severity",
                  "type": "score",
                  "instructions": "[\"severity\"]",
                  "levels": [
                    { "label": "0" },
                    { "label": "1", "description": "{\"meaning\":\"high\"}" }
                  ]
                },
                {
                  "name": "refund",
                  "type": "predicate",
                  "instructions": "Refund?\n\nCriteria for true:\n{\"meaning\":\"Explicit request for money back\"}\n\nCriteria for false:\nThe customer only asks about refund status."
                }
              ]
            }
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "warns about unsupported Responses options without sending them", Coverage = UpstreamCoverage.Covered)]
    public async Task Warns_about_unsupported_Responses_options_without_sending_them()
    {
        var (model, capture) = Setup();
        var result = await model.DoDecideAsync(Call(providerOptions: "{\"openai\":{\"reasoningEffort\":\"high\"}}"));
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("providerOptions.openai.reasoningEffort", warning.Feature);
        Assert.False(RequestBody(capture).ContainsKey("reasoning"));
    }

    [Theory]
    [MemberData(nameof(BadAnswerSets))]
    [UpstreamTest(Prefix + "rejects missing, duplicate, and unexpected answer names", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_missing_duplicate_and_unexpected_answer_names(int set)
    {
        JsonNode Answer(int index) => Fixture()["answers"]![index]!.DeepClone();
        var answers = set switch
        {
            0 => new JsonArray(),
            1 => new JsonArray(Answer(0), Answer(1), Answer(2), Answer(0)),
            2 => new JsonArray(Answer(0), Answer(0), Answer(2)),
            _ => new JsonArray(Answer(0), Answer(1), JsonNode.Parse("{\"type\":\"predicate\",\"name\":\"unknown\",\"probability\":0.5}")),
        };
        await Assert.ThrowsAsync<InvalidResponseDataException>(() => Setup(new JsonObject { ["answers"] = answers }).Model.DoDecideAsync(Call()));
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects duplicate distribution values before converting arrays to maps", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_duplicate_distribution_values_before_converting_arrays_to_maps()
    {
        var body = Fixture();
        var probabilities = body["answers"]![0]!["probabilities"]!.AsArray();
        probabilities.Add(probabilities[0]!.DeepClone());
        await Assert.ThrowsAsync<InvalidResponseDataException>(() => Setup(body).Model.DoDecideAsync(Call()));
    }

    [Theory]
    [MemberData(nameof(MalformedBodies))]
    [UpstreamTest(Prefix + "rejects malformed successful responses", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_malformed_successful_responses(int index)
    {
        var body = JsonNode.Parse(index switch
        {
            0 => "{\"answers\":[{\"type\":\"predicate\",\"name\":\"refund\",\"probability\":1.1}]}",
            1 => "{\"answers\":[{\"type\":\"choice\",\"name\":\"department\",\"choice\":\"billing\"}]}",
            2 => "{\"answers\":[{\"type\":\"score\",\"name\":\"severity\",\"score\":1,\"probabilities\":[{\"value\":0.5,\"probability\":1}]}]}",
            _ => "{\"answers\":null}",
        });
        await Assert.ThrowsAsync<ApiException>(() => Setup(body).Model.DoDecideAsync(Call()));
    }

    [Fact]
    [UpstreamTest(Prefix + "uses the standard OpenAI API error handler", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_the_standard_OpenAI_API_error_handler()
    {
        var body = JsonNode.Parse("{\"error\":{\"message\":\"Decisions access not enabled\",\"type\":\"invalid_request_error\"}}");
        var error = await Assert.ThrowsAsync<PermissionDeniedException>(() => Setup(body, HttpStatusCode.Forbidden).Model.DoDecideAsync(Call()));
        Assert.Equal("Decisions access not enabled", error.Message);
        Assert.Equal(403, error.StatusCode);
    }

    [Fact]
    [UpstreamTest(Prefix + "forwards cancellation to custom fetch", Coverage = UpstreamCoverage.Covered)]
    public async Task Forwards_cancellation_to_custom_fetch()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        var handler = new CancellationObserver();
        var provider = OpenAIProvider.Create(new OpenAIOptions { ApiKey = "test" }, handler);
        await Assert.ThrowsAsync<ApiUserAbortException>(() => provider.DecisionModel("gpt-6-luna").DoDecideAsync(Call(cancellationToken: source.Token), source.Token));
        Assert.Equal(1, handler.Calls);
        Assert.True(handler.Cancelled);
    }

    [Fact]
    [UpstreamTest(Prefix + "uses lazy environment authentication and configured provider name", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_lazy_environment_authentication_and_configured_provider_name()
    {
        var capture = new OpenAICapture { ResponseJson = Fixture().ToJsonString() };
        var baseUrl = Environment.GetEnvironmentVariable("OPENAI_BASE_URL");
        var previous = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        try
        {
            Environment.SetEnvironmentVariable("OPENAI_BASE_URL", null);
            var provider = OpenAIProvider.Create(new OpenAIOptions { ProviderName = "custom" }, capture);
            var model = provider.DecisionModel("future-decisions-model");
            Environment.SetEnvironmentVariable("OPENAI_API_KEY", "environment-key");
            await model.DoDecideAsync(Call());
            Assert.Equal("custom.decision", model.Provider);
            Assert.Equal("https://api.openai.com/v1/decisions", capture.Uri);
            Assert.Equal("Bearer environment-key", capture.Headers["Authorization"]);
            Assert.Equal("future-decisions-model", RequestBody(capture)["model"]!.GetValue<string>());
        }
        finally
        {
            Environment.SetEnvironmentVariable("OPENAI_API_KEY", previous);
            Environment.SetEnvironmentVariable("OPENAI_BASE_URL", baseUrl);
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "maps a captured live Decisions response including usage, confidence, and resolved model", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_a_captured_live_Decisions_response_including_usage_confidence_and_resolved_model()
    {
        var live = Fixture("decision-live.json");
        var body = (JsonObject)live.DeepClone();
        body["model"] = "gpt-6-luna-resolved";
        var questions = Questions();
        var renamed = new Dictionary<string, EvaluationQuestion>
        {
            ["department"] = questions["department"],
            ["severity"] = questions["severity"],
            ["requestsRefund"] = questions["refund"],
        };
        var result = await Setup(body).Model.DoDecideAsync(Call(renamed));
        Assert.Equal(387, result.Usage!.InputTokens);
        Assert.Equal(3, result.Usage.OutputTokens);
        Assert.Null(result.Usage.TotalTokens);
        JsonAssert.Equal(result.ProviderMetadata!.Value, "{\"openai\":{\"confidence\":{\"department\":1,\"severity\":1},\"usage\":" + live["usage"]!.ToJsonString() + "}}");
        Assert.Equal("gpt-6-luna-resolved", result.Response!.ModelId);
        JsonAssert.Equal(result.Response.Body!.Value, body.ToJsonString());
        Assert.Equal(3, result.Answers.Count);
        AssertAnswer(result.Answers["department"], "choice", choice: "billing", probabilities: new[] { ("billing", 1.0), ("technical", 0.0), ("other", 0.0) });
        AssertAnswer(result.Answers["severity"], "score", score: 1, probabilities: new[] { ("0", 0.0), ("1", 1.0), ("2", 0.0) });
        AssertAnswer(result.Answers["requestsRefund"], "boolean", probability: 0.99);
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves nonzero native cache and reasoning counts without changing aggregate usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_nonzero_native_cache_and_reasoning_counts_without_changing_aggregate_usage()
    {
        const string usage = "{\"input_tokens\":387,\"output_tokens\":3,\"total_tokens\":390,\"input_tokens_details\":{\"cached_tokens\":50,\"cache_write_tokens\":20},\"output_tokens_details\":{\"reasoning_tokens\":2}}";
        var body = Fixture();
        body["usage"] = JsonNode.Parse(usage);
        var result = await Setup(body).Model.DoDecideAsync(Call());
        Assert.Equal(387, result.Usage!.InputTokens);
        Assert.Equal(3, result.Usage.OutputTokens);
        Assert.Null(result.Usage.TotalTokens);
        JsonAssert.Equal(result.ProviderMetadata!.Value.GetProperty("openai").GetProperty("usage"), usage);
    }

    [Theory]
    [MemberData(nameof(AbsentOrNull))]
    [UpstreamTest(Prefix + "accepts absent or null usage and model without inventing counts", Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_absent_or_null_usage_and_model_without_inventing_counts(string value)
    {
        var body = Fixture();
        if (value == "null")
        {
            body["usage"] = null;
            body["model"] = null;
        }

        var result = await Setup(body).Model.DoDecideAsync(Call());
        Assert.Null(result.Usage);
        Assert.False(result.ProviderMetadata!.Value.GetProperty("openai").TryGetProperty("usage", out _));
        Assert.Equal("gpt-6-luna", result.Response!.ModelId);
    }

    [Fact]
    [UpstreamTest(Prefix + "accepts partial usage and preserves zero token counts", Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_partial_usage_and_preserves_zero_token_counts()
    {
        const string usage = "{\"input_tokens\":0,\"output_tokens\":null,\"input_tokens_details\":null,\"output_tokens_details\":null,\"total_tokens\":null}";
        var body = Fixture();
        body["usage"] = JsonNode.Parse(usage);
        var result = await Setup(body).Model.DoDecideAsync(Call());
        Assert.Equal(0, result.Usage!.InputTokens);
        Assert.Null(result.Usage.OutputTokens);
        JsonAssert.Equal(result.ProviderMetadata!.Value.GetProperty("openai").GetProperty("usage"), usage);
    }

    [Theory]
    [MemberData(nameof(BooleanCriteria))]
    [UpstreamTest(Prefix + "formats only supplied non-null boolean criteria", Coverage = UpstreamCoverage.Covered)]
    public async Task Formats_only_supplied_non_null_boolean_criteria(string? criteria, string expected)
    {
        var (model, capture) = Setup();
        object? parsed = null;
        if (criteria != null)
        {
            using var document = JsonDocument.Parse(criteria);
            parsed = ToValue(document.RootElement.Clone());
        }

        var questions = Questions();
        questions["refund"] = new EvaluationQuestion("boolean", "Refund?", parsed);
        await model.DoDecideAsync(Call(questions));
        Assert.Equal(expected, RequestBody(capture)["questions"]![2]!["instructions"]!.GetValue<string>());
    }

    [Theory]
    [MemberData(nameof(SafetyIdentifiers))]
    [UpstreamTest(Prefix + "sends safetyIdentifier and warns only about unsupported options", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_safetyIdentifier_and_warns_only_about_unsupported_options(string safetyIdentifier)
    {
        var (model, capture) = Setup();
        var options = new JsonObject { ["openai"] = new JsonObject { ["safetyIdentifier"] = safetyIdentifier, ["reasoningEffort"] = "high" } };
        var result = await model.DoDecideAsync(Call(providerOptions: options.ToJsonString()));
        var body = RequestBody(capture);
        Assert.Equal(safetyIdentifier, body["safety_identifier"]!.GetValue<string>());
        Assert.False(body.ContainsKey("reasoningEffort"));
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("providerOptions.openai.reasoningEffort", warning.Feature);
    }

    [Theory]
    [MemberData(nameof(InvalidSafetyIdentifiers))]
    [UpstreamTest(Prefix + "rejects invalid safety identifiers before HTTP", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_invalid_safety_identifiers_before_HTTP(string safetyIdentifierJson)
    {
        var (model, capture) = Setup();
        await Assert.ThrowsAsync<InvalidArgumentException>(() => model.DoDecideAsync(Call(providerOptions: "{\"openai\":{\"safetyIdentifier\":" + safetyIdentifierJson + "}}")));
        Assert.Equal(0, capture.Calls);
    }

    [Theory]
    [MemberData(nameof(Names))]
    [UpstreamTest(Prefix + "returns a refusal answer for a refused %s question and keeps the others", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_a_refusal_answer_for_a_refused_question_and_keeps_the_others(string name)
    {
        var body = Fixture();
        var answers = body["answers"]!.AsArray();
        for (var index = 0; index < answers.Count; index++)
        {
            if (answers[index]!["name"]!.GetValue<string>() == name)
            {
                answers[index] = JsonNode.Parse("{\"type\":\"refusal\",\"name\":\"" + name + "\"}");
            }
        }

        var (refusing, capture) = Setup(body);
        var refused = await refusing.DoDecideAsync(Call());
        var complete = await Setup().Model.DoDecideAsync(Call());

        Assert.Equal(complete.Answers.Keys.OrderBy(key => key), refused.Answers.Keys.OrderBy(key => key));
        foreach (var pair in complete.Answers)
        {
            if (pair.Key == name)
            {
                AssertAnswer(refused.Answers[name], "refusal");
            }
            else
            {
                var other = refused.Answers[pair.Key];
                Assert.Equal(pair.Value.Type, other.Type);
                Assert.Equal(pair.Value.Choice, other.Choice);
                Assert.Equal(pair.Value.Score, other.Score);
                Assert.Equal(pair.Value.Probability, other.Probability);
                Assert.Equal(pair.Value.Probabilities, other.Probabilities);
            }
        }

        Assert.False(refused.ProviderMetadata!.Value.GetProperty("openai").GetProperty("confidence").TryGetProperty(name, out _));
        Assert.Equal(1, capture.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "fails the decision when an unnamed question is refused", Coverage = UpstreamCoverage.Covered)]
    public async Task Fails_the_decision_when_an_unnamed_question_is_refused()
    {
        var body = Fixture();
        var answers = body["answers"]!.AsArray();
        for (var index = 0; index < answers.Count; index++)
        {
            if (answers[index]!["name"]!.GetValue<string>() == "refund")
            {
                answers[index] = JsonNode.Parse("{\"type\":\"refusal\",\"name\":null}");
            }
        }

        var error = await Assert.ThrowsAsync<InvalidResponseDataException>(() => Setup(body).Model.DoDecideAsync(Call()));
        Assert.Equal("OpenAI Decisions refused an unnamed question.", error.Message);
        JsonAssert.Equal((JsonElement)error.Data!, body.ToJsonString());
    }

    private sealed class CancellationObserver : HttpMessageHandler
    {
        public int Calls { get; private set; }

        public bool Cancelled { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            Cancelled = cancellationToken.IsCancellationRequested;
            throw new TaskCanceledException("Cancelled");
        }
    }
}
