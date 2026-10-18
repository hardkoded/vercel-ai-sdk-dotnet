// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Perplexity;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.DecisionFixtures;
using static Vercel.AI.Tests.Upstream.Perplexity.PerplexityDecisionModel.PerplexityDecisionModelSupport;

namespace Vercel.AI.Tests.Upstream.Perplexity.PerplexityDecisionModel;

/// <summary>Port of <c>perplexity-decision-model.test.ts</c>.</summary>
public sealed class PerplexityDecisionModelTests
{
    private const string Prefix = "packages/perplexity/src/perplexity-decision-model.test.ts::";

    public static TheoryData<string> ModelIds => new()
    {
        "pplx-decider-v1.2",
        "pplx-decider-flash-v1.2",
        "pplx-decider-latest",
        "pplx-decider-flash-latest",
        "perplexity/pplx-decider-latest",
        "perplexity/pplx-decider-flash-latest",
        "pplx-decider-v1.1-27b",
        "pplx-decider-v1-27b",
        "future-model",
    };

    public static TheoryData<int> BadAnswerSets => new() { 0, 1, 2 };

    public static TheoryData<int> MalformedAnswers => new() { 0, 1, 2, 3 };

    public static TheoryData<string> AbsentOrNull => new() { "undefined", "null" };

    public static TheoryData<string> ErrorCodes => new() { "null", "\"400\"", "401" };

    public static TheoryData<int> Statuses => new() { 404, 405, 413, 429, 500, 502, 503, 504 };

    public static TheoryData<string> InstructionFields => new() { "{}", "{\"instructions\":undefined}", "{\"instructions\":null}" };

    [Fact]
    [UpstreamTest(Prefix + "sends one native request with all question types and configured headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_one_native_request_with_all_question_types_and_configured_headers()
    {
        var (model, capture) = Setup();
        await model.DoDecideAsync(Call(headers: new Dictionary<string, string> { ["shared"] = "call" }));
        Assert.Equal("v4", model.SpecificationVersion);
        Assert.Equal("perplexity.decision", model.Provider);
        Assert.Equal(new[] { "choice", "score", "boolean" }, model.SupportedQuestionTypes);
        Assert.Equal(1, capture.Calls);
        Assert.Equal("https://example.com/v1/decisions", capture.Uri);
        Assert.Equal("Bearer test-key", capture.Headers["Authorization"]);
        Assert.Equal("application/json; charset=utf-8", capture.Headers["Content-Type"]);
        Assert.Equal("custom", capture.Headers["x-provider"]);
        Assert.Equal("call", capture.Headers["shared"]);
        Assert.Equal("vercel-ai-sdk", capture.Headers["x-pplx-integration"]);
        Assert.Contains("ai-sdk/perplexity/", capture.Headers["User-Agent"]);
        JsonAssert.Equal(RequestBody(capture), "{\"model\":\"pplx-decider-latest\",\"state\":{\"review\":\"Great sound, but the battery broke.\"},\"questions\":" + QuestionsJson + "}");
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves native precision, confidence, legends, usage and response metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_native_precision_confidence_legends_usage_and_response_metadata()
    {
        var fixture = Fixture();
        var result = await Setup().Model.DoDecideAsync(Call());
        Assert.Equal(3, result.Answers.Count);
        Assert.Equal("boolean", result.Answers["defect"].Type);
        Assert.Equal(fixture["answers"]!["defect"]!["noul"]!.GetValue<double>(), result.Answers["defect"].Probability);
        Assert.Equal("choice", result.Answers["sentiment"].Type);
        Assert.Equal("mixed", result.Answers["sentiment"].Choice);
        Assert.Equal(Distribution(fixture["answers"]!["sentiment"]!["probabilities"]!), result.Answers["sentiment"].Probabilities);
        Assert.Equal("score", result.Answers["severity"].Type);
        Assert.Equal(fixture["answers"]!["severity"]!["score"]!.GetValue<double>(), result.Answers["severity"].Score);
        Assert.Equal(Distribution(fixture["answers"]!["severity"]!["probabilities"]!), result.Answers["severity"].Probabilities);
        Assert.Null(result.Rounding);
        Assert.Equal(367, result.Usage!.InputTokens);
        Assert.Equal(3, result.Usage.OutputTokens);
        JsonAssert.Equal(result.ProviderMetadata!.Value, "{\"perplexity\":{\"confidence\":{\"sentiment\":" + fixture["answers"]!["sentiment"]!["confidence"]!.ToJsonString() + ",\"severity\":" + fixture["answers"]!["severity"]!["confidence"]!.ToJsonString() + "},\"legend\":{\"severity\":" + fixture["answers"]!["severity"]!["legend"]!.ToJsonString() + "}}}");
        Assert.Equal("request-123", result.Response!.Id);
        Assert.Equal("pplx-decider-latest", result.Response.ModelId);
        JsonAssert.Equal(result.Response.Body!.Value, fixture.ToJsonString());
        Assert.Equal("request-123", result.Response.Headers!["x-request-id"]);
        Assert.Equal("9", result.Response.Headers["x-ratelimit-remaining"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves structured instructions, rubrics and optional boolean criteria", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_structured_instructions_rubrics_and_optional_boolean_criteria()
    {
        var (model, capture) = Setup();
        var questions = new Dictionary<string, DecisionQuestion>
        {
            ["defect"] = Q("boolean", new Dictionary<string, object?> { ["task"] = "Check for defects" }, new Dictionary<string, object?> { ["true"] = new List<object?> { "Broken" }, ["false"] = null }),
            ["sentiment"] = Q(
                "choice",
                new List<object?> { "Classify" },
                new Dictionary<string, object?>
                {
                    ["positive"] = new Dictionary<string, object?> { ["description"] = "Satisfied" },
                    ["mixed"] = new List<object?> { "Praise", "Complaints" },
                    ["negative"] = null,
                }),
            ["severity"] = Q(
                "score",
                new Dictionary<string, object?> { ["task"] = "Rate" },
                new List<object?> { new Dictionary<string, object?> { ["impact"] = "None" }, new List<object?> { "Some" }, "Blocking" }),
        };
        await model.DoDecideAsync(Call(questions));
        JsonAssert.Equal(RequestBody(capture)["questions"], """
            {
              "defect": { "type": "noul", "instructions": { "task": "Check for defects" }, "criteria": { "true": ["Broken"], "false": null } },
              "sentiment": {
                "type": "choice",
                "instructions": ["Classify"],
                "criteria": { "positive": { "description": "Satisfied" }, "mixed": ["Praise", "Complaints"], "negative": null }
              },
              "severity": {
                "type": "score",
                "instructions": { "task": "Rate" },
                "criteria": [{ "impact": "None" }, ["Some"], "Blocking"]
              }
            }
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves structured score legends in provider metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_structured_score_legends_in_provider_metadata()
    {
        const string legend = "{\"0\":{\"impact\":\"none\"},\"1\":[\"partial\"],\"2\":\"blocking\"}";
        var body = Fixture();
        body["answers"]!["severity"]!["legend"] = JsonNode.Parse(legend);
        var result = await Setup(body).Model.DoDecideAsync(Call());
        JsonAssert.Equal(result.ProviderMetadata!.Value.GetProperty("perplexity").GetProperty("legend"), "{\"severity\":" + legend + "}");
    }

    [Fact]
    [UpstreamTest(Prefix + "warns about unsupported options without adding fields to the request", Coverage = UpstreamCoverage.Covered)]
    public async Task Warns_about_unsupported_options_without_adding_fields_to_the_request()
    {
        var (model, capture) = Setup();
        var result = await model.DoDecideAsync(Call(providerOptions: "{\"perplexity\":{\"temperature\":0,\"searchDomainFilter\":[\"example.com\"]}}"));
        Assert.Collection(
            result.Warnings,
            warning =>
            {
                Assert.Equal("unsupported", warning.Type);
                Assert.Equal("providerOptions.perplexity.temperature", warning.Feature);
            },
            warning =>
            {
                Assert.Equal("unsupported", warning.Type);
                Assert.Equal("providerOptions.perplexity.searchDomainFilter", warning.Feature);
            });
        Assert.Equal(new[] { "model", "state", "questions" }, RequestBody(capture).Select(pair => pair.Key));
    }

    [Theory]
    [MemberData(nameof(ModelIds))]
    [UpstreamTest(Prefix + "passes model ID %s through without resolving aliases", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_model_ID_through_without_resolving_aliases(string modelId)
    {
        var body = Fixture();
        body["model"] = modelId;
        var (provider, capture) = SetupProvider(body);
        var result = await provider.DecisionModel(modelId).DoDecideAsync(Call());
        Assert.Equal(modelId, RequestBody(capture)["model"]!.GetValue<string>());
        Assert.Equal(modelId, result.Response!.ModelId);
    }

    [Fact]
    [UpstreamTest(Prefix + "loads environment authentication lazily and uses the default endpoint", Coverage = UpstreamCoverage.Covered)]
    public async Task Loads_environment_authentication_lazily_and_uses_the_default_endpoint()
    {
        var capture = new OpenAICapture { ResponseJson = Fixture().ToJsonString() };
        var previous = Environment.GetEnvironmentVariable(PerplexityProvider.ApiKeyVariable);
        try
        {
            var model = PerplexityProvider.Create(null, capture).DecisionModel("pplx-decider-latest");
            Environment.SetEnvironmentVariable(PerplexityProvider.ApiKeyVariable, "environment-key");
            await model.DoDecideAsync(Call());
        }
        finally
        {
            Environment.SetEnvironmentVariable(PerplexityProvider.ApiKeyVariable, previous);
        }

        Assert.Equal("https://api.perplexity.ai/v1/decisions", capture.Uri);
        Assert.Equal("Bearer environment-key", capture.Headers["Authorization"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "restores the endpoint and authentication across workflow serialization", Coverage = UpstreamCoverage.Covered)]
    public async Task Restores_the_endpoint_and_authentication_across_workflow_serialization()
    {
        var (model, capture) = Setup();
        var serialized = model.Serialize();
        Assert.False(serialized.Config.ContainsKey("fetch"));
        Assert.False(serialized.Config.ContainsKey("httpClient"));
        var headers = Assert.IsAssignableFrom<IDictionary<string, object?>>(serialized.Config["headers"]);
        Assert.Equal("Bearer test-key", Assert.Single(headers, pair => pair.Key.Equals("authorization", StringComparison.OrdinalIgnoreCase)).Value);
        var restored = Vercel.AI.Perplexity.PerplexityDecisionModel.Deserialize(serialized, new HttpClient(capture));
        await restored.DoDecideAsync(Call());
        Assert.Equal("https://example.com/v1/decisions", capture.Uri);
        Assert.Equal("Bearer test-key", capture.Headers["Authorization"]);

        // A workflow runtime rehydrates the config from JSON, so values arrive as JsonElement.
        var rehydrated = new Vercel.AI.ProviderUtils.SerializedModelOptions(
            serialized.ModelId,
            JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(JsonSerializer.Serialize(serialized.Config))!.ToDictionary(pair => pair.Key, pair => (object?)pair.Value));
        var (_, secondCapture) = Setup();
        await Vercel.AI.Perplexity.PerplexityDecisionModel.Deserialize(rehydrated, new HttpClient(secondCapture)).DoDecideAsync(Call());
        Assert.Equal("https://example.com/v1/decisions", secondCapture.Uri);
        Assert.Equal("Bearer test-key", secondCapture.Headers["Authorization"]);
    }

    private static JsonObject Answers(int index)
    {
        var answers = (JsonObject)Fixture()["answers"]!.DeepClone();
        switch (index)
        {
            case 0:
                return new JsonObject();
            case 1:
                answers["extra"] = answers["defect"]!.DeepClone();
                return answers;
            default:
                answers.Remove("defect");
                return answers;
        }
    }

    [Theory]
    [MemberData(nameof(BadAnswerSets))]
    [UpstreamTest(Prefix + "rejects missing and unexpected answers", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_missing_and_unexpected_answers(int index)
    {
        await Assert.ThrowsAsync<InvalidResponseDataException>(() => Setup(new JsonObject { ["answers"] = Answers(index) }).Model.DoDecideAsync(Call()));
    }

    [Theory]
    [MemberData(nameof(MalformedAnswers))]
    [UpstreamTest(Prefix + "rejects malformed native responses", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_malformed_native_responses(int index)
    {
        var answers = (JsonObject)Fixture()["answers"]!.DeepClone();
        switch (index)
        {
            case 0:
                answers["defect"] = new JsonObject { ["type"] = "noul", ["noul"] = 1.01 };
                break;
            case 1:
                answers["sentiment"] = new JsonObject { ["type"] = "choice", ["choice"] = "mixed" };
                break;
            case 2:
                answers["severity"]!["probabilities"] = new JsonObject { ["0"] = -1 };
                break;
            default:
                answers["defect"] = new JsonObject { ["type"] = "refusal" };
                break;
        }

        await Assert.ThrowsAnyAsync<ApiException>(() => Setup(new JsonObject { ["answers"] = answers }).Model.DoDecideAsync(Call()));
    }

    [Theory]
    [MemberData(nameof(AbsentOrNull))]
    [UpstreamTest(Prefix + "tolerates absent or null ancillary response fields", Coverage = UpstreamCoverage.Covered)]
    public async Task Tolerates_absent_or_null_ancillary_response_fields(string kind)
    {
        var body = Fixture();
        foreach (var node in new JsonObject[] { body, (JsonObject)body["answers"]!["sentiment"]!, (JsonObject)body["answers"]!["severity"]! })
        {
            foreach (var name in node == body ? new[] { "model", "usage" } : node == body["answers"]!["sentiment"] ? new[] { "confidence" } : new[] { "confidence", "legend" })
            {
                if (kind == "null")
                {
                    node[name] = null;
                }
                else
                {
                    node.Remove(name);
                }
            }
        }

        var result = await Setup(body).Model.DoDecideAsync(Call());
        Assert.Null(result.Usage);
        Assert.Equal("pplx-decider-latest", result.Response!.ModelId);
        JsonAssert.Equal(result.ProviderMetadata!.Value, "{\"perplexity\":{\"confidence\":{},\"legend\":{}}}");
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves zero usage and confidence", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_zero_usage_and_confidence()
    {
        var body = Fixture();
        body["usage"] = new JsonObject { ["input_tokens"] = 0, ["output_tokens"] = null };
        body["answers"]!["sentiment"]!["confidence"] = 0;
        var result = await Setup(body).Model.DoDecideAsync(Call());
        Assert.Equal(0, result.Usage!.InputTokens);
        Assert.Null(result.Usage.OutputTokens);
        Assert.Equal(0, result.ProviderMetadata!.Value.GetProperty("perplexity").GetProperty("confidence").GetProperty("sentiment").GetDouble());
    }

    [Theory]
    [MemberData(nameof(ErrorCodes))]
    [UpstreamTest(Prefix + "preserves error messages with code %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_error_messages_with_code(string code)
    {
        var body = new JsonObject
        {
            ["error"] = new JsonObject
            {
                ["code"] = JsonNode.Parse(code),
                ["message"] = "Invalid model",
                ["type"] = "invalid_request_error",
                ["param"] = null,
            },
        };
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => Setup(body, HttpStatusCode.BadRequest).Model.DoDecideAsync(Call()));
        Assert.Equal("Invalid model", exception.Message);
        Assert.Equal(400, exception.StatusCode);
        Assert.False(exception.IsRetryable);
    }

    [Theory]
    [MemberData(nameof(Statuses))]
    [UpstreamTest(Prefix + "preserves HTTP status %s and retry headers even with non-JSON errors", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_HTTP_status_and_retry_headers_even_with_non_JSON_errors(int status)
    {
        var exception = await Assert.ThrowsAnyAsync<ApiException>(() => Setup(status: (HttpStatusCode)status, rawBody: status == 504 ? "<html>Gateway Timeout</html>" : string.Empty).Model.DoDecideAsync(Call()));
        Assert.Equal(status, exception.StatusCode);
        Assert.Equal(status == 429 || status >= 500, exception.IsRetryable);
        Assert.Equal("2", exception.ResponseHeaders!["retry-after"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "forwards cancellation without retrying inside the provider", Coverage = UpstreamCoverage.Covered)]
    public async Task Forwards_cancellation_without_retrying_inside_the_provider()
    {
        using var source = new CancellationTokenSource();
        var handler = new CancellingHandler(source);
        var model = PerplexityProvider.Create(new OpenAICompatibleOptions { ApiKey = "test" }, handler).DecisionModel("pplx-decider-latest");
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => model.DoDecideAsync(Call(), source.Token));
        Assert.Equal(1, handler.Calls);
    }

    [Theory]
    [MemberData(nameof(InstructionFields))]
    [UpstreamTest(Prefix + "preserves criteria-only questions on the wire with %j", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_criteria_only_questions_on_the_wire(string instructionFields)
    {
        // An absent and an undefined instructions field are omitted from the request. A null one is sent as null.
        var sendsNull = instructionFields.EndsWith("null}", StringComparison.Ordinal);
        var (model, capture) = Setup();
        DecisionQuestion Question(string type, object? criteria)
        {
            var question = Q(type, null, criteria);
            return sendsNull ? new DecisionQuestion(question.Type, null, question.Criteria) { HasInstructions = true } : question;
        }

        var questions = new Dictionary<string, DecisionQuestion>
        {
            ["defect"] = Question("boolean", new Dictionary<string, object?> { ["true"] = new List<object?> { "Broken" }, ["false"] = null }),
            ["sentiment"] = Question("choice", new Dictionary<string, object?> { ["positive"] = "Satisfied", ["mixed"] = "Praise and complaints", ["negative"] = "Dissatisfied" }),
            ["severity"] = Question("score", new List<object?> { "Cosmetic", "Inconvenient", "Product unusable" }),
        };
        await model.DoDecideAsync(Call(questions));
        var instructions = sendsNull ? "\"instructions\": null, " : string.Empty;
        JsonAssert.Equal(RequestBody(capture)["questions"], """
            {
              "defect": { "type": "noul", __I__"criteria": { "true": ["Broken"], "false": null } },
              "sentiment": { "type": "choice", __I__"criteria": { "positive": "Satisfied", "mixed": "Praise and complaints", "negative": "Dissatisfied" } },
              "severity": { "type": "score", __I__"criteria": ["Cosmetic", "Inconvenient", "Product unusable"] }
            }
            """.Replace("__I__", instructions));
    }

    [Fact]
    [UpstreamTest(Prefix + "sends a single Score level and preserves its zero score and unit probability", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_a_single_Score_level_and_preserves_its_zero_score_and_unit_probability()
    {
        var (model, capture) = Setup(JsonNode.Parse("""
            {
              "model": "pplx-decider-latest",
              "answers": {
                "severity": {
                  "type": "score",
                  "score": 0,
                  "probabilities": { "0": 1 },
                  "confidence": 1,
                  "legend": { "0": "Product unusable" }
                }
              }
            }
            """));
        var result = await model.DoDecideAsync(Call(new Dictionary<string, DecisionQuestion> { ["severity"] = Q("score", null, new List<object?> { "Product unusable" }) }));
        Assert.Equal(1, capture.Calls);
        JsonAssert.Equal(RequestBody(capture)["questions"], "{\"severity\":{\"type\":\"score\",\"criteria\":[\"Product unusable\"]}}");
        var answer = Assert.Single(result.Answers);
        Assert.Equal("severity", answer.Key);
        Assert.Equal("score", answer.Value.Type);
        Assert.Equal(0, answer.Value.Score);
        Assert.Equal(new Dictionary<string, double> { ["0"] = 1 }, answer.Value.Probabilities);
        JsonAssert.Equal(result.ProviderMetadata!.Value, "{\"perplexity\":{\"confidence\":{\"severity\":1},\"legend\":{\"severity\":{\"0\":\"Product unusable\"}}}}");
    }

    private static Dictionary<string, double> Distribution(JsonNode node)
    {
        return node.AsObject().ToDictionary(pair => pair.Key, pair => pair.Value!.GetValue<double>());
    }

    private sealed class CancellingHandler : HttpMessageHandler
    {
        private readonly CancellationTokenSource _source;

        public CancellingHandler(CancellationTokenSource source)
        {
            _source = source;
        }

        public int Calls { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            Assert.True(cancellationToken.CanBeCanceled);
            _source.Cancel();
            throw new OperationCanceledException("Cancelled", _source.Token);
        }
    }
}
