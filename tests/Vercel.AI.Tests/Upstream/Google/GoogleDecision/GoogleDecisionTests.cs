// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.DecisionFixtures;
using static Vercel.AI.Tests.Upstream.Google.GoogleDecision.GoogleDecisionSupport;

namespace Vercel.AI.Tests.Upstream.Google.GoogleDecision;

/// <summary>Port of <c>google-decision.test.ts</c>.</summary>
public sealed class GoogleDecisionTests
{
    private const string Prefix = "packages/google/src/google-decision.test.ts::";

    public static TheoryData<string> InvalidResponses()
    {
        return new TheoryData<string> { "missing answers", "empty answers", "missing question", "extra question", "wrong question ID", "mismatched type", "unknown choice", "out-of-bounds score" };
    }

    public static TheoryData<string> InvalidQuestions()
    {
        return new TheoryData<string> { "choice-one", "choice-256", "score-one", "score-256", "score-null", "score-empty", "boolean-no-instructions" };
    }

    public static TheoryData<double> InvalidProbabilities()
    {
        return new TheoryData<double> { -0.1, 1.1 };
    }

    public static TheoryData<string> UnsupportedMedia()
    {
        return new TheoryData<string> { "text/plain", "application/octet-stream" };
    }

    private static JsonObject Answers(Action<JsonObject>? edit = null)
    {
        var body = Fixture();
        edit?.Invoke(body["answers"]!.AsObject());
        return body;
    }

    private static JsonObject InvalidResponse(string label)
    {
        switch (label)
        {
            case "missing answers":
                return new JsonObject();
            case "empty answers":
                return new JsonObject { ["answers"] = new JsonObject() };
            case "missing question":
                return new JsonObject { ["answers"] = new JsonObject { ["department"] = Fixture()["answers"]!["department"]!.DeepClone() } };
            case "extra question":
                return Answers(answers => answers["extra"] = new JsonObject { ["type"] = "noul", ["noul"] = 0.5 });
            case "wrong question ID":
                return Answers(answers =>
                {
                    var refund = answers["requestsRefund"]!.DeepClone();
                    answers.Remove("requestsRefund");
                    answers["wrong"] = refund;
                });
            case "mismatched type":
                return Answers(answers => answers["requestsRefund"] = answers["department"]!.DeepClone());
            case "unknown choice":
                return Answers(answers => answers["department"]!["choice"] = "unknown");
            default:
                return Answers(answers => answers["severity"]!["score"] = 3);
        }
    }

    private static DecisionQuestion InvalidQuestion(string label)
    {
        switch (label)
        {
            case "choice-one":
                return Q("choice", "Choose", new Dictionary<string, object?> { ["only"] = null });
            case "choice-256":
                return Q("choice", "Choose", Enumerable.Range(0, 256).ToDictionary(i => "c" + i, _ => (object?)null));
            case "score-one":
                return Q("score", "Rate", new List<object?> { "One" });
            case "score-256":
                return Q("score", "Rate", Enumerable.Repeat((object?)"Level", 256).ToList());
            case "score-null":
                return Q("score", "Rate", new List<object?> { "Low", null });
            case "score-empty":
                return Q("score", "Rate", new List<object?> { "Low", string.Empty });
            default:
                return Q("boolean", null, new Dictionary<string, object?> { ["true"] = "Yes" });
        }
    }

    private static JsonNode? Json(string text)
    {
        return JsonNode.Parse(text);
    }

    [Fact]
    [UpstreamTest(Prefix + "calls Decisions directly with all three question types and preserves native probabilities", Commit = "f70336158e026a1aa493f41eb22fc8df7c6aadb1", Coverage = UpstreamCoverage.Partial, Note = "Asserting that the fetch request carries the same AbortSignal instance has no .NET equivalent; the call runs with a CancellationToken.")]
    public async Task Calls_Decisions_directly_with_all_three_question_types_and_preserves_native_probabilities()
    {
        var (model, capture) = Setup();
        var fixture = Fixture();
        using var source = new CancellationTokenSource();
        var result = await model.DoDecideAsync(Call(
            headers: new Dictionary<string, string> { ["x-call"] = "forwarded" },
            cancellationToken: source.Token), source.Token);
        Assert.Equal("google.decision", model.Provider);
        Assert.Equal("gemini-decision-1", model.ModelId);
        Assert.Equal("v4", model.SpecificationVersion);
        Assert.Equal(new[] { "choice", "score", "boolean" }, model.SupportedQuestionTypes);
        Assert.Equal("choice", result.Answers["department"].Type);
        Assert.Equal("billing", result.Answers["department"].Choice);
        Assert.Equal(0.8624268048164867, result.Answers["department"].Probabilities!["billing"]);
        Assert.Equal(0.13757319518351327, result.Answers["department"].Probabilities!["technical"]);
        Assert.Equal("score", result.Answers["severity"].Type);
        Assert.Equal(0.8093588003103453, result.Answers["severity"].Score);
        Assert.Equal(0.3028687295463396, result.Answers["severity"].Probabilities!["0"]);
        Assert.Equal(0.5849037405969755, result.Answers["severity"].Probabilities!["1"]);
        Assert.Equal(0.11222752985668494, result.Answers["severity"].Probabilities!["2"]);
        Assert.Equal("boolean", result.Answers["requestsRefund"].Type);
        Assert.Equal(0.36248188111145113, result.Answers["requestsRefund"].Probability);
        Assert.Equal(193, result.Usage!.InputTokens);
        Assert.Equal(0, result.Usage.OutputTokens);
        Assert.Empty(result.Warnings);
        Assert.Equal("gemini-decision-1", result.Response!.ModelId);
        Assert.Equal("req-test", result.Response.Headers!["x-request-id"]);
        Assert.True(JsonNode.DeepEquals(fixture, JsonNode.Parse(result.Response.Body!.Value.GetRawText())));
        var expectedMetadata = new JsonObject
        {
            ["google"] = new JsonObject
            {
                ["confidence"] = new JsonObject { ["department"] = 0.8624268048164867, ["severity"] = 0.5849037405969755, ["requestsRefund"] = 0.6375181188885488 },
                ["legend"] = new JsonObject { ["severity"] = new JsonObject { ["0"] = "Low", ["1"] = "Medium", ["2"] = "High" } },
                ["usage"] = fixture["usage"]!.DeepClone(),
            },
        };
        Assert.True(JsonNode.DeepEquals(expectedMetadata, JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())));
        Assert.Equal(1, capture.Calls);
        Assert.Equal("https://example.com/v1beta/decisions", capture.Uri);
        Assert.Equal("test-key", capture.Headers["x-goog-api-key"]);
        Assert.Equal("configured", capture.Headers["x-provider"]);
        Assert.Equal("forwarded", capture.Headers["x-call"]);
        Assert.Contains("ai-sdk", capture.Headers["user-agent"]);
        var expectedRequest = new JsonObject
        {
            ["model"] = "gemini-decision-1",
            ["input"] = new JsonArray(new JsonObject { ["type"] = "text", ["text"] = "{\"message\":\"A billing issue with a workaround.\"}" }),
            ["questions"] = new JsonObject
            {
                ["department"] = new JsonObject { ["type"] = "choice", ["instructions"] = "Pick the team.", ["criteria"] = new JsonObject { ["technical"] = "Bugs", ["billing"] = "Charges" } },
                ["severity"] = new JsonObject { ["type"] = "score", ["instructions"] = "[\"Rate severity.\"]", ["criteria"] = new JsonArray("Low", "Medium", "High") },
                ["requestsRefund"] = new JsonObject { ["type"] = "noul", ["instructions"] = "Is a refund requested?" },
            },
        };
        Assert.True(JsonNode.DeepEquals(expectedRequest, RequestBody(capture)));
    }

    [Fact]
    [UpstreamTest(Prefix + "serializes structured instructions and criteria without changing their data", Commit = "f70336158e026a1aa493f41eb22fc8df7c6aadb1", Coverage = UpstreamCoverage.Covered)]
    public async Task Serializes_structured_instructions_and_criteria_without_changing_their_data()
    {
        var (model, capture) = Setup();
        await model.DoDecideAsync(Call(new Dictionary<string, DecisionQuestion>
        {
            ["department"] = Q(
                "choice",
                new Dictionary<string, object?> { ["task"] = "Pick a team" },
                new Dictionary<string, object?>
                {
                    ["billing"] = new Dictionary<string, object?> { ["handles"] = new[] { "payments" } },
                    ["technical"] = null,
                }),
            ["severity"] = Q(
                "score",
                "Rate severity",
                new List<object?> { new Dictionary<string, object?> { ["level"] = "low" }, new[] { "medium" }, "high" }),
            ["requestsRefund"] = Q(
                "boolean",
                "Refund?",
                new Dictionary<string, object?>
                {
                    ["true"] = new Dictionary<string, object?> { ["requests"] = "money back" },
                    ["false"] = new[] { "No refund needed" },
                }),
        }));
        var expected = Json("""
            {
              "department": { "type": "choice", "instructions": "{\"task\":\"Pick a team\"}", "criteria": { "billing": "{\"handles\":[\"payments\"]}", "technical": null } },
              "severity": { "type": "score", "instructions": "Rate severity", "criteria": ["{\"level\":\"low\"}", "[\"medium\"]", "high"] },
              "requestsRefund": { "type": "noul", "instructions": "Refund?", "criteria": { "true_description": "{\"requests\":\"money back\"}", "false_description": "[\"No refund needed\"]" } }
            }
            """);
        Assert.True(JsonNode.DeepEquals(expected, RequestBody(capture)["questions"]));
    }

    [Fact]
    [UpstreamTest(Prefix + "omits null boolean descriptions", Commit = "f70336158e026a1aa493f41eb22fc8df7c6aadb1", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_null_boolean_descriptions()
    {
        var (model, capture) = Setup();
        var questions = Questions();
        questions["requestsRefund"] = Q("boolean", "Refund?", new Dictionary<string, object?> { ["true"] = "Money back", ["false"] = null });
        await model.DoDecideAsync(Call(questions));
        var criteria = RequestBody(capture)["questions"]!["requestsRefund"]!["criteria"]!;
        Assert.True(JsonNode.DeepEquals(Json("""{ "true_description": "Money back" }"""), criteria));
    }

    [Fact]
    [UpstreamTest(Prefix + "warns about generation settings instead of sending them to Decisions", Commit = "f70336158e026a1aa493f41eb22fc8df7c6aadb1", Coverage = UpstreamCoverage.Covered)]
    public async Task Warns_about_generation_settings_instead_of_sending_them_to_Decisions()
    {
        var (model, capture) = Setup();
        var result = await model.DoDecideAsync(Call(
            providerOptions: "{\"google\":{\"thinkingConfig\":{\"thinkingLevel\":\"high\"},\"cachedContent\":\"cache-id\"}}",
            maxOutputTokens: 100,
            reasoning: "high"));
        Assert.Equal(
            new[] { "maxOutputTokens", "reasoning", "providerOptions.google.thinkingConfig", "providerOptions.google.cachedContent" },
            result.Warnings.Select(warning => warning.Feature).ToArray());
        Assert.All(result.Warnings, warning => Assert.Equal("unsupported", warning.Type));
        Assert.Equal(new[] { "model", "input", "questions" }, RequestBody(capture).Select(pair => pair.Key).ToArray());
    }

    [Fact]
    [UpstreamTest(Prefix + "sends text, JSON, images, audio, video, and PDF state in order", Commit = "f70336158e026a1aa493f41eb22fc8df7c6aadb1", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_text_JSON_images_audio_video_and_PDF_state_in_order()
    {
        var (model, capture) = Setup();
        await model.DoDecideAsync(Call(state: new List<object?>
        {
            new Dictionary<string, object?> { ["type"] = "text", ["text"] = "Check these files." },
            new Dictionary<string, object?> { ["type"] = "json", ["value"] = new Dictionary<string, object?> { ["type"] = "image", ["user"] = "data" } },
            File("image/png", new Dictionary<string, object?> { ["type"] = "data", ["data"] = new byte[] { 1, 2, 3 } }),
            File("audio/mp3", new Dictionary<string, object?> { ["type"] = "url", ["url"] = "https://example.com/clip.mp3" }),
            File("video/mp4", new Dictionary<string, object?> { ["type"] = "data", ["data"] = "AQID" }),
            File("application/pdf", new Dictionary<string, object?> { ["type"] = "data", ["data"] = "BAUG" }),
        }));
        var expected = Json("""
            [
              { "type": "text", "text": "Check these files." },
              { "type": "text", "text": "{\"type\":\"image\",\"user\":\"data\"}" },
              { "type": "image", "mime_type": "image/png", "data": "AQID" },
              { "type": "audio", "mime_type": "audio/mp3", "uri": "https://example.com/clip.mp3" },
              { "type": "video", "mime_type": "video/mp4", "data": "AQID" },
              { "type": "document", "mime_type": "application/pdf", "data": "BAUG" }
            ]
            """);
        Assert.True(JsonNode.DeepEquals(expected, RequestBody(capture)["input"]));
    }

    [Fact]
    [UpstreamTest(Prefix + "returns refusals for all questions when input is safety blocked", Commit = "f70336158e026a1aa493f41eb22fc8df7c6aadb1", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_refusals_for_all_questions_when_input_is_safety_blocked()
    {
        var feedback = new JsonObject { ["block_reason"] = "SAFETY", ["block_reason_message"] = "Blocked input." };
        var (model, _) = Setup(new JsonObject { ["prompt_feedback"] = feedback.DeepClone(), ["usage"] = Fixture()["usage"]!.DeepClone() });
        var result = await model.DoDecideAsync(Call());
        Assert.Equal(new[] { "refusal", "refusal", "refusal" }, new[] { "department", "severity", "requestsRefund" }.Select(id => result.Answers[id].Type).ToArray());
        Assert.True(JsonNode.DeepEquals(feedback, JsonNode.Parse(result.ProviderMetadata!.Value.GetProperty("google").GetProperty("promptFeedback").GetRawText())));
        Assert.Equal(193, result.Usage!.InputTokens);
        Assert.Equal(0, result.Usage.OutputTokens);
    }

    [Theory]
    [MemberData(nameof(InvalidResponses))]
    [UpstreamTest(Prefix + "rejects $label", Commit = "f70336158e026a1aa493f41eb22fc8df7c6aadb1", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_invalid_response(string label)
    {
        var (model, _) = Setup(InvalidResponse(label));
        await Assert.ThrowsAsync<InvalidResponseDataException>(() => model.DoDecideAsync(Call()));
    }

    [Theory]
    [MemberData(nameof(InvalidProbabilities))]
    [UpstreamTest(Prefix + "rejects an invalid boolean probability (%s)", Commit = "f70336158e026a1aa493f41eb22fc8df7c6aadb1", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_an_invalid_boolean_probability(double noul)
    {
        var (model, _) = Setup(Answers(answers => answers["requestsRefund"] = new JsonObject { ["type"] = "noul", ["noul"] = noul }));
        await Assert.ThrowsAsync<ApiException>(() => model.DoDecideAsync(Call()));
    }

    [Fact]
    [UpstreamTest(Prefix + "accepts absent optional response metadata", Commit = "f70336158e026a1aa493f41eb22fc8df7c6aadb1", Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_absent_optional_response_metadata()
    {
        var body = new JsonObject
        {
            ["answers"] = new JsonObject
            {
                ["department"] = Fixture()["answers"]!["department"]!.DeepClone(),
                ["severity"] = Fixture()["answers"]!["severity"]!.DeepClone(),
                ["requestsRefund"] = new JsonObject { ["type"] = "noul", ["noul"] = 0.02, ["confidence"] = null },
            },
            ["usage"] = null,
            ["prompt_feedback"] = null,
        };
        body["answers"]!["department"]!["confidence"] = null;
        body["answers"]!["severity"]!["confidence"] = null;
        body["answers"]!["severity"]!["legend"] = null;
        var (model, _) = Setup(body);
        var result = await model.DoDecideAsync(Call());
        Assert.Null(result.Usage);
        Assert.True(JsonNode.DeepEquals(Json("""{ "google": { "confidence": {}, "legend": {} } }"""), JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())));
    }

    [Theory]
    [MemberData(nameof(InvalidQuestions))]
    [UpstreamTest(Prefix + "rejects unsupported rubric constraints before sending a request (%j)", Commit = "f70336158e026a1aa493f41eb22fc8df7c6aadb1", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_unsupported_rubric_constraints_before_sending_a_request(string label)
    {
        var (model, capture) = Setup();
        await Assert.ThrowsAsync<InvalidArgumentException>(() => model.DoDecideAsync(Call(new Dictionary<string, DecisionQuestion> { ["question"] = InvalidQuestion(label) })));
        Assert.Equal(0, capture.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects empty questions before sending a request", Commit = "f70336158e026a1aa493f41eb22fc8df7c6aadb1", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_empty_questions_before_sending_a_request()
    {
        var (model, capture) = Setup();
        await Assert.ThrowsAsync<InvalidArgumentException>(() => model.DoDecideAsync(Call(new Dictionary<string, DecisionQuestion>())));
        Assert.Equal(0, capture.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects Box before sending a request", Commit = "f70336158e026a1aa493f41eb22fc8df7c6aadb1", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_Box_before_sending_a_request()
    {
        var (model, capture) = Setup();
        var error = await Assert.ThrowsAnyAsync<Exception>(() => model.DoDecideAsync(Call(new Dictionary<string, DecisionQuestion> { ["box"] = Q("box", "Find a box") })));
        Assert.True(DecisionUnsupportedQuestionTypeError.IsInstance(error));
        Assert.Equal(0, capture.Calls);
    }

    [Theory]
    [MemberData(nameof(UnsupportedMedia))]
    [UpstreamTest(Prefix + "rejects unsupported media (%s)", Commit = "f70336158e026a1aa493f41eb22fc8df7c6aadb1", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_unsupported_media(string mediaType)
    {
        var (model, capture) = Setup();
        await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => model.DoDecideAsync(Call(state: new List<object?>
        {
            File(mediaType, new Dictionary<string, object?> { ["type"] = "data", ["data"] = "AQID" }),
        })));
        Assert.Equal(0, capture.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects file references before sending a request", Commit = "f70336158e026a1aa493f41eb22fc8df7c6aadb1", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_file_references_before_sending_a_request()
    {
        var (model, capture) = Setup();
        await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => model.DoDecideAsync(Call(state: new List<object?>
        {
            File("image/png", new Dictionary<string, object?> { ["type"] = "reference", ["reference"] = new Dictionary<string, string> { ["google"] = "files/image" } }),
        })));
        Assert.Equal(0, capture.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects an aborted decision without a request", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_an_aborted_decision_without_a_request()
    {
        var (model, capture) = Setup();
        using var source = new CancellationTokenSource();
        source.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => model.DoDecideAsync(Call(cancellationToken: source.Token), source.Token));
        Assert.Equal(0, capture.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "uses Google API error handling without falling back to generateContent", Commit = "f70336158e026a1aa493f41eb22fc8df7c6aadb1", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_Google_API_error_handling_without_falling_back_to_generateContent()
    {
        var body = new JsonObject { ["error"] = new JsonObject { ["code"] = 403, ["message"] = "EAP access required", ["status"] = "PERMISSION_DENIED" } };
        var (model, capture) = Setup(body, 403);
        var error = await Assert.ThrowsAnyAsync<AiSdkException>(() => model.DoDecideAsync(Call()));
        Assert.Contains("EAP access required", error.Message);
        Assert.Equal(1, capture.Calls);
        Assert.EndsWith("/decisions", capture.Uri);
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves the deprecated factory and direct model method", Commit = "f70336158e026a1aa493f41eb22fc8df7c6aadb1", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_the_deprecated_factory_and_direct_model_method()
    {
        var capture = new GoogleDecisionCapture { ResponseJson = Fixture().ToJsonString() };
        var provider = GoogleProvider.Create(new GoogleOptions { ApiKey = "test-key", BaseUrl = "https://example.com/v1beta/" }, capture);
#pragma warning disable CS0618
        var model = provider.EvaluationModel("gemini-decision-1");
        var result = await model.DoEvaluateAsync(EvaluationModelCall.From(Call()), CancellationToken.None);
#pragma warning restore CS0618
        Assert.Equal("choice", result.Answers["department"].Type);
        Assert.Equal("billing", result.Answers["department"].Choice);
        Assert.Equal("https://example.com/v1beta/decisions", capture.Uri);
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves custom provider names and default API version", Commit = "f70336158e026a1aa493f41eb22fc8df7c6aadb1", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_custom_provider_names_and_default_API_version()
    {
        var capture = new GoogleDecisionCapture { ResponseJson = Fixture().ToJsonString() };
        var provider = new GoogleProviderWithName(new HttpClient(capture), new GoogleOptions { ApiKey = "test-key" }, "custom.generative-ai");
        var model = provider.DecisionModel("custom-model");
        await model.DoDecideAsync(Call());
        Assert.Equal("custom.decision", model.Provider);
        Assert.Equal("https://generativelanguage.googleapis.com/v1beta/decisions", capture.Uri);
        Assert.Equal("custom-model", (string)RequestBody(capture)["model"]!);
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves workflow serialization", Commit = "f70336158e026a1aa493f41eb22fc8df7c6aadb1", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_workflow_serialization()
    {
        var provider = GoogleProvider.Create(new GoogleOptions { ApiKey = "test-key", BaseUrl = "https://example.com/v1beta/", Headers = { ["x-provider"] = "configured" } });
        var model = provider.DecisionModel("gemini-decision-1");
        var serialized = model.Serialize();
        var capture = new GoogleDecisionCapture { ResponseJson = Fixture().ToJsonString() };
        var restored = GoogleDecisionModel.Deserialize(serialized, new HttpClient(capture));
        Assert.Equal(model.ModelId, restored.ModelId);
        Assert.Equal(model.Provider, restored.Provider);
        Assert.False(serialized.Config.ContainsKey("fetch"));
        Assert.False(serialized.Config.ContainsKey("httpClient"));
        var headers = Assert.IsAssignableFrom<IDictionary<string, object?>>(serialized.Config["headers"]);
        Assert.Equal("test-key", headers["x-goog-api-key"]);
        Assert.Equal("configured", headers["x-provider"]);
        await restored.DoDecideAsync(Call());
        Assert.Equal("test-key", capture.Headers["x-goog-api-key"]);
        Assert.Equal("configured", capture.Headers["x-provider"]);
    }

    private static Dictionary<string, object?> File(string mediaType, Dictionary<string, object?> data)
    {
        return new Dictionary<string, object?> { ["type"] = "file", ["mediaType"] = mediaType, ["data"] = data };
    }

    private sealed class GoogleProviderWithName : GoogleProvider
    {
        public GoogleProviderWithName(HttpClient client, GoogleOptions options, string modelProvider)
            : base(client, options, modelProvider)
        {
        }
    }
}
