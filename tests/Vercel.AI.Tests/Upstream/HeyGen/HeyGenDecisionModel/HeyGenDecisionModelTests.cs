// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.HeyGen;
using HeyGenProviderType = Vercel.AI.HeyGen.HeyGenProvider;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;
using static Vercel.AI.Tests.DecisionFixtures;
using static Vercel.AI.Tests.Upstream.HeyGen.HeyGenDecisionModel.HeyGenDecisionModelSupport;

namespace Vercel.AI.Tests.Upstream.HeyGen.HeyGenDecisionModel;

/// <summary>Port of the tests outside any <c>describe</c> in <c>heygen-decision-model.test.ts</c>.</summary>
public sealed class HeyGenDecisionModelTests
{
    private const string Prefix = "packages/heygen/src/heygen-decision-model.test.ts::";
    private const string Commit = "585caa76cd56696750626fa9f3f3f728bc6d8d03";

    private static readonly byte[] PngHeader = [0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a];

    public static TheoryData<int> RejectedInputs => new() { 0, 1, 2, 3, 4, 5, 6 };

    public static TheoryData<int> BadAnswerSets => new() { 0, 1, 2 };

    public static TheoryData<int> MalformedAnswers => new() { 0, 1, 2 };

    [Fact]
    [UpstreamTest(Prefix + "sends one native request with all question types and configured headers", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Sends_one_native_request_with_all_question_types_and_configured_headers()
    {
        var (_, model, capture) = Setup();
        await model.DoDecideAsync(Call(headers: new Dictionary<string, string> { ["shared"] = "call" }));
        Assert.Equal("v4", model.SpecificationVersion);
        Assert.Equal("heygen.decision", model.Provider);
        Assert.Equal("heygen-decision-1", model.ModelId);
        Assert.Equal(new[] { "choice", "score", "boolean" }, model.SupportedQuestionTypes);
        Assert.Equal(1, capture.Calls);
        Assert.Equal("https://example.com/v3/models/decisions", capture.Uri);
        Assert.Equal("test-key", capture.Headers["x-api-key"]);
        Assert.Contains("application/json", capture.Headers["Content-Type"]);
        Assert.Equal("custom", capture.Headers["x-provider"]);
        Assert.Equal("call", capture.Headers["shared"]);
        Assert.Contains("heygen", capture.Headers["User-Agent"]);
        JsonAssert.Equal(
            RequestBody(capture),
            """
            {
              "model": "heygen-decision-1",
              "state": "Video export fails for everyone.",
              "questions": {
                "blocked": { "type": "noul", "instructions": "Is the customer unable to work?" },
                "team": {
                  "type": "choice",
                  "instructions": "Which team should handle this request?",
                  "criteria": { "video": "Rendering", "account": "Access", "billing": "Payments" }
                },
                "impact": {
                  "type": "score",
                  "instructions": "How much does this affect the customer?",
                  "criteria": ["Cosmetic", "Slower", "Blocked"]
                }
              }
            }
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "maps answers and preserves confidence, legends, warnings, usage and response metadata", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Maps_answers_and_preserves_confidence_legends_warnings_usage_and_response_metadata()
    {
        var body = Fixture();
        body["data"]!["answers"]!["blocked"]!["warning"] = "Low evidence";
        var result = await Setup(body).Model.DoDecideAsync(Call());
        Assert.Equal(3, result.Answers.Count);
        var blocked = result.Answers["blocked"];
        Assert.Equal("boolean", blocked.Type);
        Assert.Equal(0.96, blocked.Probability);
        var team = result.Answers["team"];
        Assert.Equal("choice", team.Type);
        Assert.Equal("video", team.Choice);
        Assert.Equal(new Dictionary<string, double> { ["video"] = 0.8, ["account"] = 0.15, ["billing"] = 0.05 }, team.Probabilities);
        var impact = result.Answers["impact"];
        Assert.Equal("score", impact.Type);
        Assert.Equal(1.75, impact.Score);
        Assert.Equal(new Dictionary<string, double> { ["0"] = 0.05, ["1"] = 0.15, ["2"] = 0.8 }, impact.Probabilities);
        Assert.Null(result.Rounding);
        Assert.Equal(1200, result.Usage!.InputTokens);
        Assert.Equal(0, result.Usage.OutputTokens);
        Assert.Empty(result.Warnings);
        JsonAssert.Equal(
            result.ProviderMetadata!.Value,
            """{"heygen":{"requestId":"0123456789abcdef0123456789abcdef","confidence":{"team":0.7,"impact":0.625},"legend":{"impact":{"0":"Cosmetic","1":"Slower","2":"Blocked"}},"answerWarnings":{"blocked":"Low evidence"}}}""");
        Assert.Equal("decision-0123456789abcdef", result.Response!.Id);
        Assert.Equal("heygen-decision-1", result.Response.ModelId);
        Assert.Equal("trace-1", result.Response.Headers!["x-trace"]);
        JsonAssert.Equal(result.Response.Body!.Value, body.ToJsonString());
    }

    [Fact]
    [UpstreamTest(Prefix + "tolerates absent ancillary response fields", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Tolerates_absent_ancillary_response_fields()
    {
        var result = await Setup(new JsonObject { ["data"] = new JsonObject { ["answers"] = Fixture()["data"]!["answers"]!.DeepClone() } }).Model.DoDecideAsync(Call());
        Assert.Null(result.Usage);
        Assert.Null(result.Response!.Id);
        Assert.Equal("heygen-decision-1", result.Response.ModelId);
        Assert.False(result.ProviderMetadata!.Value.GetProperty("heygen").TryGetProperty("requestId", out _));
    }

    [Fact]
    [UpstreamTest(Prefix + "sends file parts as ordered image and video assets and keeps text and JSON in state", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Sends_file_parts_as_ordered_image_and_video_assets_and_keeps_text_and_JSON_in_state()
    {
        var (_, model, capture) = Setup(new JsonObject { ["data"] = new JsonObject { ["answers"] = new JsonObject { ["blocked"] = Fixture()["data"]!["answers"]!["blocked"]!.DeepClone() } } });
        await model.DoDecideAsync(Call(
            new Dictionary<string, DecisionQuestion> { ["blocked"] = Blocked() },
            [
                new DecisionModelTextPart("Compare the first image with the video."),
                Data("image", PngHeader),
                Url("video/mp4", "https://example.com/clip.mp4"),
                Reference("image/jpeg", "heygen", "asset-123"),
                new DecisionModelJsonPart(JsonDocument.Parse("""{"product":"vase"}""").RootElement.Clone()),
            ]));
        var body = RequestBody(capture);
        JsonAssert.Equal(body["state"], """["Compare the first image with the video.",{"product":"vase"}]""");
        JsonAssert.Equal(
            body["assets"],
            """
            {
              "images": {
                "image-1": { "type": "base64", "media_type": "image/png", "data": "iVBORw0KGgo=" },
                "image-2": { "type": "asset_id", "asset_id": "asset-123" }
              },
              "videos": { "video-1": { "type": "url", "url": "https://example.com/clip.mp4" } }
            }
            """);
    }

    private static (IReadOnlyList<DecisionModelStatePart> State, Type Error) RejectedInput(int index)
    {
        switch (index)
        {
            case 0:
                return (Enumerable.Range(0, 5).Select(_ => (DecisionModelStatePart)Base64("image/png", "iVBORw0KGgo=")).ToList(), typeof(InvalidArgumentException));
            case 1:
                return (Enumerable.Range(0, 2).Select(_ => (DecisionModelStatePart)Url("video/mp4", "https://example.com/a.mp4")).ToList(), typeof(InvalidArgumentException));
            case 2:
                return ([Url("image/png", "http://example.com/a.png")], typeof(InvalidArgumentException));
            case 3:
                return ([Data("image", [1, 2, 3])], typeof(InvalidArgumentException));
            case 4:
                return ([Base64("application/pdf", "JVBERi0=")], typeof(UnsupportedFunctionalityException));
            case 5:
                return ([InlineText("image/png", "not an image")], typeof(UnsupportedFunctionalityException));
            default:
                return ([Reference("image/png", "other", "file-1")], typeof(NoSuchProviderReferenceError));
        }
    }

    [Theory]
    [MemberData(nameof(RejectedInputs))]
    [UpstreamTest(Prefix + "rejects %s before HTTP", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Rejects_input_before_HTTP(int index)
    {
        var (state, error) = RejectedInput(index);
        var (_, model, capture) = Setup();
        var exception = await Record.ExceptionAsync(() => model.DoDecideAsync(Call(state: state)));
        Assert.IsType(error, exception);
        Assert.Equal(0, capture.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "warns about unsupported settings without adding fields to the request", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Warns_about_unsupported_settings_without_adding_fields_to_the_request()
    {
        var (_, model, capture) = Setup();
        var result = await model.DoDecideAsync(Call(providerOptions: "{\"heygen\":{\"temperature\":0}}", maxOutputTokens: 10, reasoning: "low"));
        Assert.Collection(
            result.Warnings,
            warning =>
            {
                Assert.Equal("unsupported", warning.Type);
                Assert.Equal("maxOutputTokens", warning.Feature);
            },
            warning =>
            {
                Assert.Equal("unsupported", warning.Type);
                Assert.Equal("reasoning", warning.Feature);
            },
            warning =>
            {
                Assert.Equal("unsupported", warning.Type);
                Assert.Equal("providerOptions.heygen.temperature", warning.Feature);
            });
        Assert.Equal(new[] { "model", "state", "questions" }, RequestBody(capture).Select(pair => pair.Key));
    }

    [Theory]
    [MemberData(nameof(BadAnswerSets))]
    [UpstreamTest(Prefix + "rejects missing and unexpected answers", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Rejects_missing_and_unexpected_answers(int index)
    {
        var answers = (JsonObject)Fixture()["data"]!["answers"]!.DeepClone();
        switch (index)
        {
            case 0:
                answers = new JsonObject();
                break;
            case 1:
                answers["extra"] = answers["blocked"]!.DeepClone();
                break;
            default:
                answers.Remove("blocked");
                break;
        }

        await Assert.ThrowsAsync<InvalidResponseDataException>(() => Setup(new JsonObject { ["data"] = new JsonObject { ["answers"] = answers } }).Model.DoDecideAsync(Call()));
    }

    [Theory]
    [MemberData(nameof(MalformedAnswers))]
    [UpstreamTest(Prefix + "rejects malformed native responses", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Rejects_malformed_native_responses(int index)
    {
        var answers = (JsonObject)Fixture()["data"]!["answers"]!.DeepClone();
        switch (index)
        {
            case 0:
                answers["blocked"] = new JsonObject { ["type"] = "noul", ["noul"] = 1.01 };
                break;
            case 1:
                answers["team"] = new JsonObject { ["type"] = "choice", ["choice"] = "video" };
                break;
            default:
                answers["blocked"] = new JsonObject { ["type"] = "refusal" };
                break;
        }

        await Assert.ThrowsAnyAsync<ApiException>(() => Setup(new JsonObject { ["data"] = new JsonObject { ["answers"] = answers } }).Model.DoDecideAsync(Call()));
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves HeyGen error codes and parameters", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Preserves_HeyGen_error_codes_and_parameters()
    {
        var body = new JsonObject
        {
            ["error"] = new JsonObject { ["code"] = "invalid_parameter", ["message"] = "Invalid question.", ["param"] = "questions.team.criteria" },
        };
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => Setup(body, HttpStatusCode.BadRequest).Model.DoDecideAsync(Call()));
        Assert.Equal("Invalid question. (invalid_parameter) [questions.team.criteria]", exception.Message);
        Assert.Equal(400, exception.StatusCode);
    }

    [Fact]
    [UpstreamTest(Prefix + "loads environment authentication lazily and uses the default endpoint", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Loads_environment_authentication_lazily_and_uses_the_default_endpoint()
    {
        var capture = new OpenAICapture { ResponseJson = Fixture().ToJsonString() };
        var previous = Environment.GetEnvironmentVariable("HEYGEN_API_KEY");
        try
        {
            var model = HeyGenProviderType.Create(null, capture).DecisionModel("heygen-decision-1-turbo");
            Environment.SetEnvironmentVariable("HEYGEN_API_KEY", "environment-key");
            await model.DoDecideAsync(Call());
        }
        finally
        {
            Environment.SetEnvironmentVariable("HEYGEN_API_KEY", previous);
        }

        Assert.Equal("https://api.heygen.com/v3/models/decisions", capture.Uri);
        Assert.Equal("environment-key", capture.Headers["x-api-key"]);
        Assert.Equal("heygen-decision-1-turbo", RequestBody(capture)["model"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Prefix + "restores the endpoint and authentication across workflow serialization", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Restores_the_endpoint_and_authentication_across_workflow_serialization()
    {
        var (_, model, _) = Setup();
        var serialized = model.Serialize();
        Assert.False(serialized.Config.ContainsKey("fetch"));
        Assert.False(serialized.Config.ContainsKey("httpClient"));
        var headers = Assert.IsAssignableFrom<IDictionary<string, object?>>(serialized.Config["headers"]);
        Assert.Equal("test-key", Assert.Single(headers, pair => pair.Key.Equals("x-api-key", StringComparison.OrdinalIgnoreCase)).Value);
        var capture = new OpenAICapture { ResponseJson = Fixture().ToJsonString() };
        await DecisionHeyGenModel.Deserialize(serialized, new HttpClient(capture)).DoDecideAsync(Call());
        Assert.Equal("https://example.com/v3/models/decisions", capture.Uri);
        Assert.Equal("test-key", capture.Headers["x-api-key"]);

        // A workflow runtime rehydrates the config from JSON, so values arrive as JsonElement.
        var rehydrated = new SerializedModelOptions(
            serialized.ModelId,
            JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(JsonSerializer.Serialize(serialized.Config))!.ToDictionary(pair => pair.Key, pair => (object?)pair.Value));
        var second = new OpenAICapture { ResponseJson = Fixture().ToJsonString() };
        await DecisionHeyGenModel.Deserialize(rehydrated, new HttpClient(second)).DoDecideAsync(Call());
        Assert.Equal("https://example.com/v3/models/decisions", second.Uri);
        Assert.Equal("test-key", second.Headers["x-api-key"]);
    }
}
