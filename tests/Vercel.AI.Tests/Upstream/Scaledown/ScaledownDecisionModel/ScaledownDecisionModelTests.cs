// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.ScaleDown;
using Vercel.AI.Util;
using static Vercel.AI.Tests.DecisionFixtures;
using static Vercel.AI.Tests.Upstream.Scaledown.ScaledownDecisionModel.ScaledownDecisionModelSupport;

namespace Vercel.AI.Tests.Upstream.Scaledown.ScaledownDecisionModel;

/// <summary>Port of the tests outside any <c>describe</c> in <c>scaledown-decision-model.test.ts</c>.</summary>
public sealed class ScaledownDecisionModelTests
{
    private const string Prefix = "packages/scaledown/src/scaledown-decision-model.test.ts::";

    [Fact]
    [UpstreamTest(Prefix + "maps all three question types, native results, usage, and metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_all_three_question_types_native_results_usage_and_metadata()
    {
        var server = new ScaleDownServer();
        var result = await CreateProvider(server).DecisionModel("classify-1").DoDecideAsync(Call(), CancellationToken.None);
        var call = Assert.Single(server.Calls);
        Assert.Equal(Url, call.Url);
        var expected = JsonNode.Parse("""
            {"model":"classify-1","state":{"text":"The export crashes in Safari."},"questions":{"category":{"type":"choice","criteria":{"bug":"Broken functionality","feedback":"feedback"}},"workaround":{"type":"noul","instructions":"Is there a workaround?"},"severity":{"type":"score","criteria":["Cosmetic","Degraded","Blocking"]}}}
            """);
        Assert.True(JsonNode.DeepEquals(expected, call.Json), call.Body);
        Assert.Equal("test-key", call.Headers["x-api-key"]);

        Assert.Equal(3, result.Answers.Count);
        var category = result.Answers["category"];
        Assert.Equal("choice", category.Type);
        Assert.Equal("bug", category.Choice);
        Assert.Equal(new Dictionary<string, double> { ["bug"] = 0.93, ["feedback"] = 0.07 }, category.Probabilities);
        var workaround = result.Answers["workaround"];
        Assert.Equal("boolean", workaround.Type);
        Assert.Equal(0.79, workaround.Probability);
        var severity = result.Answers["severity"];
        Assert.Equal("score", severity.Type);
        Assert.Equal(1.43, severity.Score);
        Assert.Equal(new Dictionary<string, double> { ["0"] = 0, ["1"] = 0.57, ["2"] = 0.43 }, severity.Probabilities);
        Assert.Equal(103, result.Usage!.InputTokens);
        Assert.Equal(3, result.Usage.OutputTokens);
        JsonAssert.Equal(result.ProviderMetadata!.Value, """{"scaledown":{"confidence":{"category":0.93,"severity":0.57},"legend":{"severity":{"0":"Cosmetic","1":"Degraded","2":"Blocking"}},"cost":0.0000044}}""");
        Assert.Equal("classify-1", result.Response!.ModelId);
        JsonAssert.Equal(result.Response.Body!.Value, ResponseJson);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves JSON state, structured rubrics, and boolean criteria as text", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_JSON_state_structured_rubrics_and_boolean_criteria_as_text()
    {
        var server = new ScaleDownServer();
        var questions = new Dictionary<string, DecisionQuestion>
        {
            ["category"] = Q("choice", new Dictionary<string, object?> { ["task"] = "Route" }, new Dictionary<string, object?> { ["bug"] = new List<object?> { "Broken" } }),
            ["workaround"] = Q("boolean", null, new Dictionary<string, object?> { ["true"] = "Another browser works", ["false"] = "No alternative works" }),
        };
        await CreateProvider(server).DecisionModel("classify-1").DoDecideAsync(
            Call(questions, [JsonPart("""{"ticket":"Export is broken"}""")]),
            CancellationToken.None);
        var body = Assert.Single(server.Calls).Json!;
        Assert.Equal("""{"ticket":"Export is broken"}""", body["state"]!["text"]!.GetValue<string>());
        Assert.Equal("""{"task":"Route"}""", body["questions"]!["category"]!["instructions"]!.GetValue<string>());
        Assert.Equal("""["Broken"]""", body["questions"]!["category"]!["criteria"]!["bug"]!.GetValue<string>());
        Assert.Equal("noul", body["questions"]!["workaround"]!["type"]!.GetValue<string>());
        Assert.Equal("Answer yes when: Another browser works\nAnswer no when: No alternative works", body["questions"]!["workaround"]!["instructions"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Prefix + "encodes document input alongside text", Coverage = UpstreamCoverage.Covered)]
    public async Task Encodes_document_input_alongside_text()
    {
        var server = new ScaleDownServer();
        await CreateProvider(server).DecisionModel("classify-1").DoDecideAsync(
            Call(state: [new DecisionModelTextPart("Classify this invoice."), FilePart("application/pdf", bytes: [1, 2, 3])]),
            CancellationToken.None);
        var state = Assert.Single(server.Calls).Json!["state"]!;
        Assert.Equal("Classify this invoice.", state["text"]!.GetValue<string>());
        Assert.Equal("AQID", state["document"]!.GetValue<string>());
        Assert.Equal("application/pdf", state["document_mime_type"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "passes custom settings, call headers, abort signal, and model ID",
        Coverage = UpstreamCoverage.Partial,
        Note = "HttpClient links the cancellation token, so the test cannot assert that the same token reaches the handler. It asserts that a canceled token stops the call.")]
    public async Task Passes_custom_settings_call_headers_abort_signal_and_model_ID()
    {
        var server = new ScaleDownServer();
        var provider = ScaleDownProvider.Create(
            new ScaleDownProviderSettings
            {
                ApiKey = "custom-key",
                BaseUrl = "https://custom.example/",
                Headers = new Dictionary<string, string> { ["shared"] = "provider" },
            },
            server);
        await provider.DecisionModel("classify").DoDecideAsync(Call(headers: new Dictionary<string, string> { ["shared"] = "call" }), CancellationToken.None);
        var call = Assert.Single(server.Calls);
        Assert.Equal("https://custom.example/v1/scaledown", call.Url);
        Assert.Equal("custom-key", call.Headers["x-api-key"]);
        Assert.Contains("ai-sdk/scaledown/" + AiSdkVersion.Version, call.Headers["user-agent"]);
        Assert.Equal("call", call.Headers["shared"]);
        Assert.Equal("classify", call.Json!["model"]!.GetValue<string>());

        using var canceled = new CancellationTokenSource();
        canceled.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => provider.DecisionModel("classify").DoDecideAsync(Call(cancellationToken: canceled.Token), canceled.Token));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "restores workflow configuration and falls back to environment authentication",
        Coverage = UpstreamCoverage.Partial,
        Note = "The default provider (scaledown and its scaleDown alias) is ScaleDownProvider.Default, which cannot take a test handler, so the environment fallback is asserted on a provider created without an API key.")]
    public async Task Restores_workflow_configuration_and_falls_back_to_environment_authentication()
    {
        var server = new ScaleDownServer();
        var model = CreateProvider(server).DecisionModel("classify-1");
        var previous = Environment.GetEnvironmentVariable(ScaleDownProvider.ApiKeyEnvironmentVariable);
        Environment.SetEnvironmentVariable(ScaleDownProvider.ApiKeyEnvironmentVariable, "env-key");
        try
        {
            var serialized = model.Serialize();
            var restored = DecisionScaleDownModel.Deserialize(
                new Vercel.AI.ProviderUtils.SerializedModelOptions(serialized.ModelId, new Dictionary<string, object?>
                {
                    ["provider"] = "scaledown.decision",
                    ["baseURL"] = "https://api.scaledown.xyz",
                }),
                new HttpClient(server, disposeHandler: false));
            await restored.DoDecideAsync(Call(), CancellationToken.None);
            Assert.Equal("classify-1", serialized.ModelId);
            var headers = Assert.IsAssignableFrom<IDictionary<string, object?>>(serialized.Config["headers"]);
            Assert.Equal("test-key", headers["x-api-key"]);
            Assert.Equal("env-key", server.Calls[0].Headers["x-api-key"]);

            await CreateProvider(server, apiKey: null).DecisionModel("classify-1").DoDecideAsync(Call(), CancellationToken.None);
            Assert.Equal("env-key", server.Calls[1].Headers["x-api-key"]);
            Assert.Equal(["choice", "boolean", "score"], restored.SupportedQuestionTypes);
        }
        finally
        {
            Environment.SetEnvironmentVariable(ScaleDownProvider.ApiKeyEnvironmentVariable, previous);
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "warns about unsupported generation settings and provider options", Coverage = UpstreamCoverage.Covered)]
    public async Task Warns_about_unsupported_generation_settings_and_provider_options()
    {
        var server = new ScaleDownServer();
        var result = await CreateProvider(server).DecisionModel("classify-1").DoDecideAsync(
            Call(maxOutputTokens: 10, reasoning: "high", providerOptions: """{"scaledown":{"reasoning":true}}"""),
            CancellationToken.None);
        Assert.Collection(
            result.Warnings,
            warning => Assert.Equal(("unsupported", "maxOutputTokens"), (warning.Type, warning.Feature)),
            warning => Assert.Equal(("unsupported", "reasoning"), (warning.Type, warning.Feature)),
            warning => Assert.Equal(("unsupported", "providerOptions.scaledown.reasoning"), (warning.Type, warning.Feature)));
    }

    public static IEnumerable<object[]> UnsupportedScoreCriteria()
    {
        yield return new object[] { new List<object?> { "Only level" } };
        yield return new object[] { Enumerable.Repeat((object?)"Level", 11).ToList() };
        yield return new object[] { new List<object?> { "Low", null } };
    }

    [Theory]
    [MemberData(nameof(UnsupportedScoreCriteria))]
    [UpstreamTest(Prefix + "rejects unsupported score criteria %j before sending a request", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_unsupported_score_criteria_before_sending_a_request(List<object?> criteria)
    {
        var server = new ScaleDownServer();
        await Assert.ThrowsAsync<InvalidArgumentError>(() => CreateProvider(server).DecisionModel("classify-1").DoDecideAsync(
            Call(new Dictionary<string, DecisionQuestion> { ["severity"] = Q("score", null, criteria) }),
            CancellationToken.None));
        Assert.Empty(server.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects unsupported questions and files before sending a request", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_unsupported_questions_and_files_before_sending_a_request()
    {
        var server = new ScaleDownServer();
        var provider = CreateProvider(server);
        await Assert.ThrowsAsync<DecisionUnsupportedQuestionTypeError>(() => provider.DecisionModel("classify-1").DoDecideAsync(
            Call(new Dictionary<string, DecisionQuestion> { ["location"] = Q("box", null) }),
            CancellationToken.None));
        await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => provider.DecisionModel("classify-1").DoDecideAsync(
            Call(state: [FilePart("audio/wav", base64: "AQID")]),
            CancellationToken.None));
        Assert.Empty(server.Calls);
        Assert.Throws<NoSuchModelError>(() => provider.LanguageModel("unknown"));
        Assert.Throws<NoSuchModelError>(() => provider.EmbeddingModel("unknown"));
        Assert.Throws<NoSuchModelError>(() => provider.ImageModel("unknown"));
    }

    [Theory]
    [InlineData("""{"detail":"Insufficient credits"}""", "Insufficient credits")]
    [InlineData("""{"detail":[{"msg":"Invalid model"}]}""", "Invalid model")]
    [UpstreamTest(Prefix + "surfaces API errors: %j", Coverage = UpstreamCoverage.Covered)]
    public async Task Surfaces_API_errors(string body, string message)
    {
        var server = new ScaleDownServer { Status = HttpStatusCode.UnprocessableEntity, ResponseBody = body };
        var error = await Assert.ThrowsAsync<ApiCallError>(() => CreateProvider(server).DecisionModel("classify-1").DoDecideAsync(Call(), CancellationToken.None));
        Assert.Equal(422, error.StatusCode);
        Assert.Equal(message, error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects a malformed successful response", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_a_malformed_successful_response()
    {
        var server = new ScaleDownServer { ResponseBody = """{"answers":{"category":{"type":"choice"}}}""" };
        await Assert.ThrowsAsync<ApiCallError>(() => CreateProvider(server).DecisionModel("classify-1").DoDecideAsync(Call(), CancellationToken.None));
    }
}
