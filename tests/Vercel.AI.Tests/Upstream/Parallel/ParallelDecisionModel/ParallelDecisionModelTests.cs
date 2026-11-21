// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Parallel;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;
using static Vercel.AI.Tests.DecisionFixtures;
using static Vercel.AI.Tests.Upstream.Parallel.ParallelDecisionModel.ParallelDecisionModelSupport;

namespace Vercel.AI.Tests.Upstream.Parallel.ParallelDecisionModel;

/// <summary>Port of the tests outside any <c>describe</c> in <c>parallel-decision-model.test.ts</c>.</summary>
[Collection(ParallelEnvironmentCollection.Name)]
public sealed class ParallelDecisionModelTests
{
    private const string Prefix = "packages/parallel/src/parallel-decision-model.test.ts::";
    private const string UpstreamCommit = "6a648d45fffeadc101797685fb76e8b0c19e639a";

    [Fact]
    [UpstreamTest(
        Prefix + "sends the native contract with configured auth, options and request headers",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Sends_the_native_contract_with_configured_auth_options_and_request_headers()
    {
        var server = new ParallelServer();
        await CreateModel(server)
            .DoDecideAsync(
                Call(
                    headers: new Dictionary<string, string> { ["x-call"] = "custom" },
                    providerOptions: """{"parallel":{"includeExcerpts":false}}"""
                ),
                CancellationToken.None
            );
        var call = Assert.Single(server.Calls);
        Assert.Equal(Url, call.Url);
        Assert.Equal("test-key", call.Headers["x-api-key"]);
        Assert.Equal("custom", call.Headers["x-provider"]);
        Assert.Equal("custom", call.Headers["x-call"]);
        Assert.StartsWith("application/json", call.Headers["content-type"]);
        Assert.False(call.Headers.ContainsKey("authorization"));
        Assert.Contains("ai-sdk/parallel/" + AiSdkVersion.Version, call.Headers["user-agent"]);
        var expected = JsonNode.Parse(
            """
            {"mode":"fast","include_excerpts":false,"state":{"company":"Parallel","website":"https://parallel.ai"},"questions":{"hasSearch":{"type":"noul","instructions":"Does it offer web search?"},"availability":{"type":"choice","instructions":"Is the API available?","criteria":{"available":"Publicly available","unknown":"Insufficient evidence"}},"fit":{"type":"score","instructions":"How well does it fit?","criteria":["No fit","Partial fit","Full fit"]}}}
            """
        );
        Assert.True(JsonNode.DeepEquals(expected, call.Json), call.Body);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preserves full precision, citations, confidence, legends and usage",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Preserves_full_precision_citations_confidence_legends_and_usage()
    {
        var result = await CreateModel(new ParallelServer())
            .DoDecideAsync(Call(), CancellationToken.None);
        var fixture = FixtureNode();
        Assert.Equal(3, result.Answers.Count);
        var hasSearch = result.Answers["hasSearch"];
        Assert.Equal("boolean", hasSearch.Type);
        Assert.Equal(0.9992643798509062, hasSearch.Probability);
        var availability = result.Answers["availability"];
        Assert.Equal("choice", availability.Type);
        Assert.Equal("available", availability.Choice);
        Assert.Equal(
            new Dictionary<string, double>
            {
                ["available"] = 0.987654321,
                ["unknown"] = 0.012345679,
            },
            availability.Probabilities
        );
        var fit = result.Answers["fit"];
        Assert.Equal("score", fit.Type);
        Assert.Equal(1.7123456789, fit.Score);
        Assert.Equal(
            new Dictionary<string, double>
            {
                ["0"] = 0.1,
                ["1"] = 0.0876543211,
                ["2"] = 0.8123456789,
            },
            fit.Probabilities
        );

        var metadata = new JsonObject
        {
            ["parallel"] = new JsonObject
            {
                ["citations"] = fixture["citations"]!.DeepClone(),
                ["confidence"] = new JsonObject
                {
                    ["availability"] = 0.87654321,
                    ["fit"] = 0.7654321,
                },
                ["legend"] = new JsonObject
                {
                    ["fit"] = fixture["answers"]!["fit"]!["legend"]!.DeepClone(),
                },
                ["decisionId"] = fixture["decision_id"]!.DeepClone(),
                ["mode"] = "fast",
            },
        };
        JsonAssert.Equal(result.ProviderMetadata!.Value, metadata.ToJsonString());
        Assert.Null(result.Rounding);
        Assert.Equal(4212, result.Usage!.InputTokens);
        Assert.Equal(0, result.Usage.OutputTokens);
        Assert.Equal(fixture["decision_id"]!.GetValue<string>(), result.Response!.Id);
        Assert.Equal("fast", result.Response.ModelId);
        JsonAssert.Equal(result.Response.Body!.Value, Fixture);

        // The usage schema judges the numeric value, not its JSON spelling.
        var spelled = FixtureNode();
        spelled["usage"] = JsonNode.Parse("""{"input_tokens":4212.0,"output_tokens":0e0}""");
        var equivalent = await CreateModel(new ParallelServer(spelled.ToJsonString()))
            .DoDecideAsync(Call(), CancellationToken.None);
        Assert.Equal(4212, equivalent.Usage!.InputTokens);
        Assert.Equal(0, equivalent.Usage.OutputTokens);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preserves refusal answers alongside successful answers",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Preserves_refusal_answers_alongside_successful_answers()
    {
        var response = FixtureNode();
        response["answers"]!["hasSearch"] = new JsonObject { ["type"] = "refusal" };
        var result = await CreateModel(new ParallelServer(response.ToJsonString()))
            .DoDecideAsync(Call(), CancellationToken.None);
        Assert.Equal("refusal", result.Answers["hasSearch"].Type);
        Assert.Equal("score", result.Answers["fit"].Type);
        Assert.Equal(1.7123456789, result.Answers["fit"].Score);
    }

    [Theory]
    [InlineData("[]")]
    [InlineData("""[{"url":"https://example.com","excerpts":[]}]""")]
    [UpstreamTest(
        Prefix + "preserves empty evidence fields: %j",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Preserves_empty_evidence_fields(string citations)
    {
        var response = FixtureNode();
        response["citations"] = JsonNode.Parse(citations);
        var result = await CreateModel(new ParallelServer(response.ToJsonString()))
            .DoDecideAsync(Call(), CancellationToken.None);
        JsonAssert.Equal(
            JsonSerializer.SerializeToElement(
                JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())!["parallel"]![
                    "citations"
                ]
            ),
            citations
        );
    }

    [Theory]
    [InlineData("[]", "[]")]
    [InlineData("""[{"text":"A company"}]""", "\"A company\"")]
    [InlineData("""[{"json":null}]""", "null")]
    [InlineData("""[{"json":[1,"two"]}]""", """[1,"two"]""")]
    [InlineData(
        """[{"text":"A company"},{"json":{"website":"https://example.com"}}]""",
        """["A company",{"website":"https://example.com"}]"""
    )]
    [UpstreamTest(
        Prefix + "preserves JSON and text state: %j",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Preserves_JSON_and_text_state(string parts, string expectedState)
    {
        var state = new List<DecisionModelStatePart>();
        foreach (var part in JsonNode.Parse(parts)!.AsArray())
        {
            state.Add(
                part!["text"] is { } text
                    ? new DecisionModelTextPart(text.GetValue<string>())
                    : JsonPart(part["json"]?.ToJsonString() ?? "null")
            );
        }

        var server = new ParallelServer();
        await CreateModel(server).DoDecideAsync(Call(state: state), CancellationToken.None);
        var sent = Assert.Single(server.Calls).Json!.AsObject();
        Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(expectedState), sent["state"]),
            sent["state"]?.ToJsonString()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "rejects unsupported inputs and invalid options before provider I/O",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Rejects_unsupported_inputs_and_invalid_options_before_provider_IO()
    {
        var server = new ParallelServer();
        var model = CreateModel(server);
        await Assert.ThrowsAsync<DecisionUnsupportedQuestionTypeError>(() =>
            model.DoDecideAsync(
                Call(new Dictionary<string, DecisionQuestion> { ["box"] = Q("box", "Find a box") }),
                CancellationToken.None
            )
        );
        await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() =>
            model.DoDecideAsync(
                Call(state: [FilePart("image/png", "abcd")]),
                CancellationToken.None
            )
        );
        await Assert.ThrowsAsync<InvalidArgumentError>(() =>
            model.DoDecideAsync(
                Call(providerOptions: """{"parallel":{"includeExcerpts":"false"}}"""),
                CancellationToken.None
            )
        );
        Assert.Empty(server.Calls);
    }

    public static IEnumerable<object[]> MalformedResponses()
    {
        var invalidProbability = FixtureNode();
        invalidProbability["answers"]!["hasSearch"] = JsonNode.Parse(
            """{"type":"noul","noul":2}"""
        );
        var missingAnswers = FixtureNode();
        missingAnswers["answers"] = new JsonObject
        {
            ["hasSearch"] = missingAnswers["answers"]!["hasSearch"]!.DeepClone(),
        };
        var extraAnswer = FixtureNode();
        extraAnswer["answers"]!["unexpected"] = extraAnswer["answers"]!["hasSearch"]!.DeepClone();
        var invalidCitations = FixtureNode();
        invalidCitations["citations"] = JsonNode.Parse(
            """[{"url":"https://example.com","excerpts":"not-an-array"}]"""
        );
        yield return new object[] { invalidProbability.ToJsonString() };
        yield return new object[] { missingAnswers.ToJsonString() };
        yield return new object[] { extraAnswer.ToJsonString() };
        yield return new object[] { invalidCitations.ToJsonString() };
    }

    [Theory]
    [MemberData(nameof(MalformedResponses))]
    [UpstreamTest(
        Prefix + "rejects malformed provider responses",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Rejects_malformed_provider_responses(string response)
    {
        var error = await Assert.ThrowsAnyAsync<Exception>(() =>
            CreateModel(new ParallelServer(response)).DoDecideAsync(Call(), CancellationToken.None)
        );
        Assert.True(error is ApiCallError or InvalidResponseDataException, error.ToString());
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.UnprocessableEntity)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [UpstreamTest(
        Prefix + "preserves provider errors and retry headers (%i)",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Preserves_provider_errors_and_retry_headers(HttpStatusCode status)
    {
        var server = new ParallelServer(
            """{"type":"error","error":{"message":"Request rejected","ref_id":"error-123"}}""",
            status
        );
        var error = await Assert.ThrowsAsync<ApiCallError>(() =>
            CreateModel(server).DoDecideAsync(Call(), CancellationToken.None)
        );
        Assert.Equal("Request rejected", error.Message);
        Assert.Equal((int)status, error.StatusCode);
        Assert.Equal("2", error.GetHeader("retry-after"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "passes abort signals through unchanged",
        Coverage = UpstreamCoverage.Partial,
        Commit = UpstreamCommit,
        Note = "HttpClient hands the handler a linked token, never the caller's own instance, so identity is asserted as propagation: canceling the caller's token cancels the token the handler received."
    )]
    public async Task Passes_abort_signals_through_unchanged()
    {
        var server = new ParallelServer();
        var held = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        server.Hold = held.Task;
        using var abort = new CancellationTokenSource();
        var pending = CreateModel(server)
            .DoDecideAsync(Call(cancellationToken: abort.Token), abort.Token);
        while (server.Signals.Count < 1)
        {
            await Task.Delay(5);
        }

        var signal = server.Signals[0];
        Assert.True(signal.CanBeCanceled);
        Assert.False(signal.IsCancellationRequested);
        abort.Cancel();
        Assert.True(signal.IsCancellationRequested);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "warns about unsupported generation and provider options",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Warns_about_unsupported_generation_and_provider_options()
    {
        var result = await CreateModel(new ParallelServer())
            .DoDecideAsync(
                Call(
                    maxOutputTokens: 10,
                    reasoning: "low",
                    providerOptions: """{"parallel":{"unused":true}}"""
                ),
                CancellationToken.None
            );
        Assert.Collection(
            result.Warnings,
            warning =>
                Assert.Equal(("unsupported", "maxOutputTokens"), (warning.Type, warning.Feature)),
            warning => Assert.Equal(("unsupported", "reasoning"), (warning.Type, warning.Feature)),
            warning =>
                Assert.Equal(
                    ("unsupported", "providerOptions.parallel.unused"),
                    (warning.Type, warning.Feature)
                )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "uses environment auth, the default endpoint, and exported provider",
        Coverage = UpstreamCoverage.Partial,
        Commit = UpstreamCommit,
        Note = "The exported provider (parallel) is ParallelProvider.Default, which cannot take a test handler, so the environment key and the default endpoint are asserted on a provider created without settings."
    )]
    public async Task Uses_environment_auth_the_default_endpoint_and_exported_provider()
    {
        var previous = Environment.GetEnvironmentVariable(
            ParallelProvider.ApiKeyEnvironmentVariable
        );
        Environment.SetEnvironmentVariable(
            ParallelProvider.ApiKeyEnvironmentVariable,
            "environment-key"
        );
        try
        {
            var server = new ParallelServer();
            await ParallelProvider
                .Create(handler: server)
                .DecisionModel("fast")
                .DoDecideAsync(Call(), CancellationToken.None);
            var call = Assert.Single(server.Calls);
            Assert.Equal("https://api.parallel.ai/v1/decisions", call.Url);
            Assert.Equal("environment-key", call.Headers["x-api-key"]);
            Assert.Same(ParallelProvider.Default, ParallelProvider.Default);
        }
        finally
        {
            Environment.SetEnvironmentVariable(
                ParallelProvider.ApiKeyEnvironmentVariable,
                previous
            );
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "rejects unsupported model families",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public void Rejects_unsupported_model_families()
    {
        var provider = CreateProvider(new ParallelServer());
        Assert.Contains(
            "No such languageModel",
            Assert.Throws<NoSuchModelError>(() => provider.LanguageModel("fast")).Message
        );
        Assert.Contains(
            "No such embeddingModel",
            Assert.Throws<NoSuchModelError>(() => provider.EmbeddingModel("fast")).Message
        );
        Assert.Contains(
            "No such imageModel",
            Assert.Throws<NoSuchModelError>(() => provider.ImageModel("fast")).Message
        );
        Assert.Contains(
            "No such decisionModel",
            Assert.Throws<NoSuchModelError>(() => provider.DecisionModel("")).Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "serializes model configuration and restores the request contract",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Serializes_model_configuration_and_restores_the_request_contract()
    {
        var model = CreateModel(new ParallelServer());
        var serialized = model.Serialize();
        Assert.Equal("fast", serialized.ModelId);
        Assert.Equal("https://example.com/v1", serialized.Config["baseURL"]);
        var server = new ParallelServer();
        var restored = DecisionParallelModel.Deserialize(
            new SerializedModelOptions(serialized.ModelId, serialized.Config),
            new HttpClient(server, disposeHandler: false)
        );
        await restored.DoDecideAsync(Call(), CancellationToken.None);
        Assert.Equal("test-key", Assert.Single(server.Calls).Headers["x-api-key"]);

        // A config persisted as JSON comes back with JsonElement values.
        var persisted = JsonSerializer.Deserialize<Dictionary<string, object?>>(
            JsonSerializer.Serialize(serialized.Config)
        )!;
        var fromJson = new ParallelServer();
        await DecisionParallelModel
            .Deserialize(
                new SerializedModelOptions(serialized.ModelId, persisted),
                new HttpClient(fromJson, disposeHandler: false)
            )
            .DoDecideAsync(Call(), CancellationToken.None);
        var jsonCall = Assert.Single(fromJson.Calls);
        Assert.Equal(Url, jsonCall.Url);
        Assert.Equal("test-key", jsonCall.Headers["x-api-key"]);
        Assert.Equal("custom", jsonCall.Headers["x-provider"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "forwards future mode strings without a package update",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Forwards_future_mode_strings_without_a_package_update()
    {
        var response = FixtureNode();
        response["mode"] = "future-mode";
        var server = new ParallelServer(response.ToJsonString());
        var model = ParallelProvider
            .Create(new ParallelProviderSettings { ApiKey = "test-key" }, server)
            .DecisionModel("future-mode");
        var result = await model.DoDecideAsync(Call(), CancellationToken.None);
        Assert.Equal("future-mode", Assert.Single(server.Calls).Json!["mode"]!.GetValue<string>());
        Assert.Equal(
            "future-mode",
            JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())!["parallel"]![
                "mode"
            ]!.GetValue<string>()
        );
        Assert.Equal("future-mode", result.Response!.ModelId);
    }
}
