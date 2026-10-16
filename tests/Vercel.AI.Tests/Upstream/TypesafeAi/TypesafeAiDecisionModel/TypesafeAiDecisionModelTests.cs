// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;
using Vercel.AI.TypeSafeAi;
using Vercel.AI.Util;
using static Vercel.AI.Tests.DecisionFixtures;
using static Vercel.AI.Tests.Upstream.TypesafeAi.TypesafeAiDecisionModel.TypesafeAiDecisionModelSupport;

namespace Vercel.AI.Tests.Upstream.TypesafeAi.TypesafeAiDecisionModel;

/// <summary>Port of the tests outside any <c>describe</c> in <c>typesafe-ai-decision-model.test.ts</c>.</summary>
public sealed class TypesafeAiDecisionModelTests
{
    private const string Prefix = "packages/typesafe-ai/src/typesafe-ai-decision-model.test.ts::";

    [Fact]
    [UpstreamTest(Prefix + "sends all three question types and structured rubrics in one request", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_all_three_question_types_and_structured_rubrics_in_one_request()
    {
        var server = Server();
        var questions = Questions();
        await CreateProvider(server).DecisionModel("jev-latest").DoDecideAsync(Call(questions), CancellationToken.None);
        var call = Assert.Single(server.Calls);
        Assert.True(JsonNode.DeepEquals(NativeRequest, JsonNode.Parse(call.Body)), call.Body);
        Assert.Equal("boolean", questions["requestsRefund"].Type);
        Assert.Equal("Bearer test-api-key", call.Headers["authorization"]);
        Assert.Equal("application/json", call.Headers["content-type"]);
    }

    [Theory]
    [InlineData("jev-1.13.0")]
    [InlineData("jev-1.14.0")]
    [InlineData("jev-latest")]
    [InlineData("jev-preview")]
    [UpstreamTest(Prefix + "sends the model ID or alias unchanged", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_the_model_ID_or_alias_unchanged(string modelId)
    {
        var server = Server();
        await CreateProvider(server).DecisionModel(modelId).DoDecideAsync(Call(), CancellationToken.None);
        var body = JsonNode.Parse(Assert.Single(server.Calls).Body)!.AsObject();
        Assert.Equal(modelId, body["model"]!.GetValue<string>());
        var expected = NativeRequest;
        expected["model"] = modelId;
        Assert.True(JsonNode.DeepEquals(expected, body), body.ToJsonString());
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves native scores, probabilities, confidence metadata, and usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_native_scores_probabilities_confidence_metadata_and_usage()
    {
        var result = await CreateProvider(Server()).DecisionModel("jev-latest").DoDecideAsync(Call(), CancellationToken.None);
        Assert.Equal(3, result.Answers.Count);
        var department = result.Answers["department"];
        Assert.Equal("choice", department.Type);
        Assert.Equal("billing", department.Choice);
        AssertProbabilities(new Dictionary<string, double> { ["technical"] = 0, ["other"] = 0, ["billing"] = 1 }, department.Probabilities);
        var severity = result.Answers["severity"];
        Assert.Equal("score", severity.Type);
        Assert.Equal(0.97, severity.Score);
        AssertProbabilities(new Dictionary<string, double> { ["0"] = 0.13, ["1"] = 0.76, ["2"] = 0.11 }, severity.Probabilities);
        var requestsRefund = result.Answers["requestsRefund"];
        Assert.Equal("boolean", requestsRefund.Type);
        Assert.Equal(0.99, requestsRefund.Probability);
        Assert.Equal(2, result.Rounding!.ProbabilityDecimals);
        Assert.Equal(2, result.Rounding.ScoreDecimals);
        JsonAssert.Equal(result.ProviderMetadata!.Value, "{\"typesafe\":{\"confidence\":{\"department\":1,\"severity\":0.64}}}");
        Assert.Equal(471, result.Usage!.InputTokens);
        Assert.Equal(71, result.Usage.OutputTokens);
        Assert.Equal("jev-1.13.0", result.Response!.ModelId);
        JsonAssert.Equal(result.Response.Body!.Value, NativeResponseJson);
    }

    [Fact]
    [UpstreamTest(Prefix + "passes custom fetch, headers, base URL, and future model IDs", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_custom_fetch_headers_base_URL_and_future_model_IDs()
    {
        var server = Server();
        server.ResponseHeaders["x-request-id"] = "test-id";
        var custom = TypeSafeAiProvider.Create(
            new TypeSafeAiOptions
            {
                ApiKey = "custom-key",
                BaseUrl = "https://custom.example/v1/",
                Headers = new Dictionary<string, string> { ["custom"] = "provider", ["shared"] = "provider" },
            },
            server);
        var result = await custom.DecisionModel("jev-future").DoDecideAsync(
            Call(headers: new Dictionary<string, string> { ["shared"] = "call" }),
            CancellationToken.None);
        var call = Assert.Single(server.Calls);
        Assert.Equal("https://custom.example/v1/systemone", call.Url);
        Assert.Equal("Bearer custom-key", call.Headers["authorization"]);
        Assert.Equal("provider", call.Headers["custom"]);
        Assert.Equal("call", call.Headers["shared"]);
        Assert.Contains("ai-sdk/typesafe-ai/" + AiSdkVersion.Version, call.Headers["user-agent"]);
        Assert.Equal("jev-future", JsonNode.Parse(call.Body)!["model"]!.GetValue<string>());
        Assert.Equal("test-id", result.Response!.Headers!["x-request-id"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "loads TYPESAFE_AI_API_KEY lazily from the environment", Coverage = UpstreamCoverage.Covered)]
    public async Task Loads_TYPESAFE_AI_API_KEY_lazily_from_the_environment()
    {
        var server = Server();
        var environmentModel = TypeSafeAiProvider.Create(null, server).DecisionModel("jev-latest");
        var previous = Environment.GetEnvironmentVariable(TypeSafeAiProvider.ApiKeyEnvironmentVariable);
        Environment.SetEnvironmentVariable(TypeSafeAiProvider.ApiKeyEnvironmentVariable, "environment-key");
        try
        {
            await environmentModel.DoDecideAsync(Call(), CancellationToken.None);
        }
        finally
        {
            Environment.SetEnvironmentVariable(TypeSafeAiProvider.ApiKeyEnvironmentVariable, previous);
        }

        Assert.Equal("Bearer environment-key", server.Calls[0].Headers["authorization"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "reports unsupported provider options without passing them through", Coverage = UpstreamCoverage.Covered)]
    public async Task Reports_unsupported_provider_options_without_passing_them_through()
    {
        var server = Server();
        var result = await CreateProvider(server).DecisionModel("jev-latest").DoDecideAsync(
            Call(providerOptions: "{\"typesafe\":{\"temperature\":0}}"),
            CancellationToken.None);
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("providerOptions.typesafe.temperature", warning.Feature);
        Assert.True(JsonNode.DeepEquals(NativeRequest, JsonNode.Parse(server.Calls[0].Body)), server.Calls[0].Body);
    }

    public static IEnumerable<object[]> ProviderLimitQuestions()
    {
        var options = new Dictionary<string, object?>();
        for (var index = 0; index < 256; index++)
        {
            options[index.ToString()] = null;
        }

        yield return new object[] { Q("choice", "Pick", options) };
        yield return new object[] { Q("score", "Rate", Enumerable.Range(0, 11).Select(index => (object?)("Level " + index)).ToList()) };
    }

    [Theory]
    [MemberData(nameof(ProviderLimitQuestions))]
    [UpstreamTest(Prefix + "rejects provider limits before HTTP for $type", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_provider_limits_before_HTTP_for_type(DecisionQuestion question)
    {
        var server = Server();
        await Assert.ThrowsAsync<InvalidArgumentError>(() => CreateProvider(server).DecisionModel("jev-latest").DoDecideAsync(
            Call(new Dictionary<string, DecisionQuestion> { ["question"] = question }, state: JsonState("test")),
            CancellationToken.None));
        Assert.Empty(server.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "accepts null optional metadata without inventing usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_null_optional_metadata_without_inventing_usage()
    {
        var server = Server();
        server.ResponseBody = "{\"model\":null,\"usage\":null,\"answers\":{\"topic\":{\"type\":\"choice\",\"choice\":\"a\",\"probabilities\":{\"a\":1},\"confidence\":null}}}";
        var result = await CreateProvider(server).DecisionModel("jev-latest").DoDecideAsync(Call(), CancellationToken.None);
        Assert.Null(result.Usage!.InputTokens);
        Assert.Null(result.Usage.OutputTokens);
        Assert.Equal("jev-latest", result.Response!.ModelId);
        JsonAssert.Equal(result.ProviderMetadata!.Value, "{\"typesafe\":{\"confidence\":{}}}");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "forwards cancellation to fetch",
        Coverage = UpstreamCoverage.Partial,
        Note = "HttpClient hands the handler a linked token, so the test checks that cancelling the caller's token cancels the token the handler sees instead of comparing token identity.")]
    public async Task Forwards_cancellation_to_fetch()
    {
        using var controller = new CancellationTokenSource();
        var handler = new CancellingHandler(controller);
        var model = TypeSafeAiProvider.Create(new TypeSafeAiOptions { ApiKey = "test" }, handler).DecisionModel("jev-latest");
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => model.DoDecideAsync(Call(), controller.Token));
        Assert.Equal(1, handler.Calls);
        Assert.True(handler.TokenCanBeCanceled);
        Assert.True(handler.TokenCancelledAfterAbort);
    }

    [Fact]
    [UpstreamTest(Prefix + "serializes and restores a model for workflow boundaries", Coverage = UpstreamCoverage.Covered)]
    public async Task Serializes_and_restores_a_model_for_workflow_boundaries()
    {
        var original = new TypeSafeAiDecisionModel("jev-latest", new TypeSafeAiDecisionModelConfig
        {
            Provider = "typesafe.decision",
            BaseUrl = "https://api.typesafe.ai/v1",
            Headers = () => new Dictionary<string, string> { ["Authorization"] = "Bearer restored-key" },
            HttpClient = new HttpClient(Server()),
        });
        var serialized = original.Serialize();
        Assert.False(serialized.Config.ContainsKey("fetch"));
        Assert.False(serialized.Config.ContainsKey("httpClient"));
        var server = Server();
        var restored = TypeSafeAiDecisionModel.Deserialize(serialized, new HttpClient(server));
        await restored.DoDecideAsync(Call(), CancellationToken.None);
        Assert.Equal("Bearer restored-key", server.Calls[0].Headers["authorization"]);
        Assert.Equal(new[] { "choice", "score", "boolean", "box" }, restored.SupportedQuestionTypes);
    }

    [Fact]
    [UpstreamTest(Prefix + "restores authentication from the environment when no headers are serialized", Coverage = UpstreamCoverage.Covered)]
    public async Task Restores_authentication_from_the_environment_when_no_headers_are_serialized()
    {
        var server = Server();
        var previous = Environment.GetEnvironmentVariable(TypeSafeAiProvider.ApiKeyEnvironmentVariable);
        Environment.SetEnvironmentVariable(TypeSafeAiProvider.ApiKeyEnvironmentVariable, "workflow-key");
        try
        {
            var restored = TypeSafeAiDecisionModel.Deserialize(
                new SerializedModelOptions(
                    "jev-latest",
                    new Dictionary<string, object?> { ["provider"] = "typesafe.decision", ["baseURL"] = "https://api.typesafe.ai/v1" }),
                new HttpClient(server));
            await restored.DoDecideAsync(Call(), CancellationToken.None);
        }
        finally
        {
            Environment.SetEnvironmentVariable(TypeSafeAiProvider.ApiKeyEnvironmentVariable, previous);
        }

        Assert.Equal("Bearer workflow-key", server.Calls[0].Headers["authorization"]);
    }

    private static void AssertProbabilities(Dictionary<string, double> expected, IReadOnlyDictionary<string, double>? actual)
    {
        Assert.NotNull(actual);
        Assert.Equal(expected.Count, actual!.Count);
        foreach (var pair in expected)
        {
            Assert.Equal(pair.Value, actual[pair.Key]);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [UpstreamTest(Prefix + "sends box questions and enables the images header (image: %s)", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_box_questions_and_enables_the_images_header(bool withImage)
    {
        var server = Server();
        var response = JsonNode.Parse(NativeResponseJson)!.AsObject();
        response["answers"]!["target"] = JsonNode.Parse("{\"type\":\"box\",\"box\":{\"top\":0.25,\"bottom\":0.82,\"left\":0.44,\"right\":0.84},\"confidence\":0.92}");
        server.ResponseBody = response.ToJsonString();
        var questions = Questions();
        questions["target"] = Q("box", "The mug");
        var model = CreateProvider(server).DecisionModel("jev-latest");
        var state = withImage
            ? new List<object?> { FilePart("image/png", "AQID") }
            : null;
        var result = await model.DoDecideAsync(Call(questions, state), CancellationToken.None);
        Assert.Contains("box", model.SupportedQuestionTypes);
        var box = result.Answers["target"].Box!;
        Assert.Equal("box", result.Answers["target"].Type);
        Assert.Equal((0.25, 0.82, 0.44, 0.84), (box.Top, box.Bottom, box.Left, box.Right));
        Assert.Equal(0.92, result.ProviderMetadata!.Value.GetProperty("typesafe").GetProperty("confidence").GetProperty("target").GetDouble());
        var call = Assert.Single(server.Calls);
        Assert.Equal("true", call.Headers["typesafe-danger-zone-images"]);
        var body = withImage
            ? JsonNode.Parse(call.Form.Single(part => part.Name == "request").Text)!
            : JsonNode.Parse(call.Body)!;
        var target = body["questions"]!["target"]!.AsObject();
        Assert.Equal(2, target.Count);
        Assert.Equal("box", target["type"]!.GetValue<string>());
        Assert.Equal("The mug", target["instructions"]!.GetValue<string>());
    }

    private sealed class CancellingHandler : HttpMessageHandler
    {
        private readonly CancellationTokenSource _controller;

        public CancellingHandler(CancellationTokenSource controller)
        {
            _controller = controller;
        }

        public int Calls { get; private set; }

        public bool TokenCanBeCanceled { get; private set; }

        public bool TokenCancelledAfterAbort { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            TokenCanBeCanceled = cancellationToken.CanBeCanceled;
            _controller.Cancel();
            TokenCancelledAfterAbort = cancellationToken.IsCancellationRequested;
            throw new OperationCanceledException("cancelled", cancellationToken);
        }
    }
}
