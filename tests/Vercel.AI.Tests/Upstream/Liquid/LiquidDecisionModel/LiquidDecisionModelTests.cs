// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Liquid;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;
using static Vercel.AI.Tests.DecisionFixtures;
using static Vercel.AI.Tests.Upstream.Liquid.LiquidDecisionModel.LiquidDecisionModelSupport;

namespace Vercel.AI.Tests.Upstream.Liquid.LiquidDecisionModel;

/// <summary>Port of the tests outside any <c>describe</c> in <c>liquid-decision-model.test.ts</c>.</summary>
public sealed class LiquidDecisionModelTests
{
    private const string Prefix = "packages/liquid/src/liquid-decision-model.test.ts::";
    private const string Commit = "cf5fe9a22344de9285e77aef4144bbb54e15b788";

    public static TheoryData<int> StatesSent => new() { 0, 1 };

    public static TheoryData<int> RejectedInputs => new() { 0, 1, 2, 3, 4, 5, 6 };

    public static TheoryData<int> BadAnswerSets => new() { 0, 1, 2 };

    public static TheoryData<int> MalformedAnswers => new() { 0, 1, 2 };

    [Fact]
    [UpstreamTest(
        Prefix + "sends one native request with configured headers",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Sends_one_native_request_with_configured_headers()
    {
        var (_, model, capture) = Setup();
        await model.DoDecideAsync(
            Call(headers: new Dictionary<string, string> { ["shared"] = "call" })
        );
        Assert.Equal("v4", model.SpecificationVersion);
        Assert.Equal("liquid.decision", model.Provider);
        Assert.Equal("d1.2", model.ModelId);
        Assert.Equal(new[] { "choice", "score", "boolean", "box" }, model.SupportedQuestionTypes);
        Assert.Equal(1, capture.Calls);
        Assert.Equal("https://example.com/decisions/v1/systemone", capture.Uri);
        Assert.Equal("Bearer test-key", capture.Headers["Authorization"]);
        Assert.Contains("application/json", capture.Headers["Content-Type"]);
        Assert.Equal("custom", capture.Headers["x-provider"]);
        Assert.Equal("call", capture.Headers["shared"]);
        Assert.Contains("liquid", capture.Headers["User-Agent"]);
        JsonAssert.Equal(
            RequestBody(capture),
            """
            {
              "model": "d1.2",
              "state": "Help! My payouts have been failing for 3 days.",
              "questions": {
                "department": {
                  "type": "choice",
                  "instructions": "Which team should handle this?",
                  "criteria": {
                    "billing": "Payments, invoicing, refunds",
                    "technical": "Bugs, outages, integrations",
                    "sales": "Pricing, upgrades, new accounts"
                  }
                },
                "is_urgent": { "type": "noul", "instructions": "Does this convey urgency?" },
                "frustration": {
                  "type": "score",
                  "instructions": "How frustrated is the customer?",
                  "criteria": ["Calm", "Frustrated", "Very angry"]
                }
              }
            }
            """
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "maps answers and preserves confidence, legends, usage, cost and response metadata",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Maps_answers_and_preserves_confidence_legends_usage_cost_and_response_metadata()
    {
        var body = TextFixture();
        var fixture = Fixture();
        var result = await Setup(body).Model.DoDecideAsync(Call());
        Assert.Equal(3, result.Answers.Count);
        var department = result.Answers["department"];
        Assert.Equal("choice", department.Type);
        Assert.Equal("billing", department.Choice);
        Assert.Equal(
            Distribution(fixture["answers"]!["department"]!["probabilities"]!),
            department.Probabilities
        );
        var urgent = result.Answers["is_urgent"];
        Assert.Equal("boolean", urgent.Type);
        Assert.Equal(
            fixture["answers"]!["is_urgent"]!["noul"]!.GetValue<double>(),
            urgent.Probability
        );
        var frustration = result.Answers["frustration"];
        Assert.Equal("score", frustration.Type);
        Assert.Equal(
            fixture["answers"]!["frustration"]!["score"]!.GetValue<double>(),
            frustration.Score
        );
        Assert.Equal(
            Distribution(fixture["answers"]!["frustration"]!["probabilities"]!),
            frustration.Probabilities
        );
        Assert.Null(result.Rounding);
        Assert.Equal(612, result.Usage!.InputTokens);
        Assert.Equal(0, result.Usage.OutputTokens);
        Assert.Empty(result.Warnings);
        JsonAssert.Equal(
            result.ProviderMetadata!.Value,
            new JsonObject
            {
                ["liquid"] = new JsonObject
                {
                    ["cost"] = fixture["usage"]!["cost"]!.DeepClone(),
                    ["confidence"] = new JsonObject
                    {
                        ["department"] = fixture["answers"]!["department"]![
                            "confidence"
                        ]!.DeepClone(),
                        ["frustration"] = fixture["answers"]!["frustration"]![
                            "confidence"
                        ]!.DeepClone(),
                    },
                    ["legend"] = new JsonObject
                    {
                        ["frustration"] = fixture["answers"]!["frustration"]![
                            "legend"
                        ]!.DeepClone(),
                    },
                    ["found"] = new JsonObject(),
                },
            }.ToJsonString()
        );
        Assert.Equal("d1.2", result.Response!.ModelId);
        Assert.Equal("trace-1", result.Response.Headers!["x-trace"]);
        JsonAssert.Equal(result.Response.Body!.Value, body.ToJsonString());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "sends a Box question with its image and maps the box",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Sends_a_Box_question_with_its_image_and_maps_the_box()
    {
        var fixture = Fixture();
        var (_, model, capture) = Setup(
            new JsonObject
            {
                ["model"] = "d1.2",
                ["answers"] = new JsonObject
                {
                    ["subject"] = fixture["answers"]!["subject"]!.DeepClone(),
                },
            }
        );
        var result = await model.DoDecideAsync(Call(BoxQuestions(), BoxState));
        JsonAssert.Equal(
            RequestBody(capture),
            """
            {
              "model": "d1.2",
              "state": "A support screenshot.",
              "images": ["data:image/png;base64,iVBORw0KGgo="],
              "questions": { "subject": { "type": "box", "instructions": "The animal in the picture." } }
            }
            """
        );
        var subject = result.Answers["subject"];
        Assert.Equal("box", subject.Type);
        var expected = fixture["answers"]!["subject"]!["box"]!;
        Assert.Equal(expected["top"]!.GetValue<double>(), subject.Box!.Top);
        Assert.Equal(expected["bottom"]!.GetValue<double>(), subject.Box.Bottom);
        Assert.Equal(expected["left"]!.GetValue<double>(), subject.Box.Left);
        Assert.Equal(expected["right"]!.GetValue<double>(), subject.Box.Right);
        var liquid = result.ProviderMetadata!.Value.GetProperty("liquid");
        Assert.Equal(
            fixture["answers"]!["subject"]!["confidence"]!.GetValue<double>(),
            liquid.GetProperty("confidence").GetProperty("subject").GetDouble()
        );
        Assert.Equal(1, liquid.GetProperty("found").GetProperty("subject").GetDouble());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "returns a refusal when the Box object is not found",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Returns_a_refusal_when_the_Box_object_is_not_found()
    {
        var notFound = BoxNotFoundFixture();
        var result = await Setup(notFound)
            .Model.DoDecideAsync(
                Call(
                    new Dictionary<string, DecisionQuestion>
                    {
                        ["car"] = Q("box", "The red sports car."),
                    },
                    BoxState
                )
            );
        var car = Assert.Single(result.Answers);
        Assert.Equal("car", car.Key);
        Assert.Equal("refusal", car.Value.Type);
        Assert.Null(car.Value.Box);
        Assert.Equal(
            notFound["answers"]!["car"]!["found"]!.GetValue<double>(),
            result
                .ProviderMetadata!.Value.GetProperty("liquid")
                .GetProperty("found")
                .GetProperty("car")
                .GetDouble()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "sends images and audio as ordered top-level data URLs and joins the remaining state",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Sends_images_and_audio_as_ordered_top_level_data_URLs_and_joins_the_remaining_state()
    {
        var (_, model, capture) = Setup(
            new JsonObject
            {
                ["answers"] = new JsonObject
                {
                    ["is_urgent"] = Fixture()["answers"]!["is_urgent"]!.DeepClone(),
                },
            }
        );
        await model.DoDecideAsync(
            Call(
                new Dictionary<string, DecisionQuestion> { ["is_urgent"] = IsUrgent() },
                [
                    new DecisionModelTextPart("The voice note belongs to this ticket."),
                    Data("audio", Wav),
                    Data("image", Png),
                    Base64("image/webp", "UklGRg=="),
                    new DecisionModelJsonPart(
                        JsonDocument.Parse("""{"plan":"enterprise"}""").RootElement.Clone()
                    ),
                ]
            )
        );
        var body = RequestBody(capture);
        Assert.Equal(
            "The voice note belongs to this ticket.\n{\"plan\":\"enterprise\"}",
            body["state"]!.GetValue<string>()
        );
        JsonAssert.Equal(
            body["images"],
            """["data:image/png;base64,iVBORw0KGgo=","data:image/webp;base64,UklGRg=="]"""
        );
        JsonAssert.Equal(body["audio"], """["data:audio/wav;base64,UklGRgAAAABXQVZF"]""");
    }

    [Theory]
    [MemberData(nameof(StatesSent))]
    [UpstreamTest(Prefix + "sends %s", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Sends_state(int index)
    {
        // 0: a single JSON object as structured state. 1: media-only state as null.
        IReadOnlyList<DecisionModelStatePart> state =
            index == 0
                ?
                [
                    new DecisionModelJsonPart(
                        JsonDocument.Parse("""{"message":"Refund me"}""").RootElement.Clone()
                    ),
                ]
                : [Base64("audio/mpeg", "SUQz")];
        var (_, model, capture) = Setup(
            new JsonObject
            {
                ["answers"] = new JsonObject
                {
                    ["is_urgent"] = Fixture()["answers"]!["is_urgent"]!.DeepClone(),
                },
            }
        );
        await model.DoDecideAsync(
            Call(new Dictionary<string, DecisionQuestion> { ["is_urgent"] = IsUrgent() }, state)
        );
        var sent = RequestBody(capture)["state"];
        if (index == 0)
        {
            JsonAssert.Equal(sent, """{"message":"Refund me"}""");
        }
        else
        {
            Assert.Null(sent);
        }
    }

    [Theory]
    [MemberData(nameof(RejectedInputs))]
    [UpstreamTest(
        Prefix + "rejects %s before HTTP",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Rejects_input_before_HTTP(int index)
    {
        var (state, error) = RejectedInput(index);
        var (_, model, capture) = Setup();
        var exception = await Record.ExceptionAsync(() => model.DoDecideAsync(Call(state: state)));
        Assert.IsType(error, exception);
        Assert.Equal(0, capture.Calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "warns about unsupported settings without adding fields to the request",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Warns_about_unsupported_settings_without_adding_fields_to_the_request()
    {
        var (_, model, capture) = Setup();
        var result = await model.DoDecideAsync(
            Call(
                providerOptions: "{\"liquid\":{\"temperature\":0}}",
                maxOutputTokens: 10,
                reasoning: "low"
            )
        );
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
                Assert.Equal("providerOptions.liquid.temperature", warning.Feature);
            }
        );
        Assert.Equal(
            new[] { "model", "state", "questions" },
            RequestBody(capture).Select(pair => pair.Key)
        );
    }

    [Theory]
    [MemberData(nameof(BadAnswerSets))]
    [UpstreamTest(
        Prefix + "rejects missing and unexpected answers",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Rejects_missing_and_unexpected_answers(int index)
    {
        var answers = TextAnswers();
        switch (index)
        {
            case 0:
                answers = new JsonObject();
                break;
            case 1:
                answers["extra"] = answers["is_urgent"]!.DeepClone();
                break;
            default:
                answers.Remove("is_urgent");
                break;
        }

        await Assert.ThrowsAsync<InvalidResponseDataException>(() =>
            Setup(new JsonObject { ["answers"] = answers }).Model.DoDecideAsync(Call())
        );
    }

    [Theory]
    [MemberData(nameof(MalformedAnswers))]
    [UpstreamTest(
        Prefix + "rejects malformed native responses",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Rejects_malformed_native_responses(int index)
    {
        var answers = TextAnswers();
        switch (index)
        {
            case 0:
                answers["is_urgent"] = new JsonObject { ["type"] = "noul", ["noul"] = 1.01 };
                break;
            case 1:
                answers["department"] = new JsonObject
                {
                    ["type"] = "choice",
                    ["choice"] = "technical",
                };
                break;
            default:
                answers["is_urgent"] = new JsonObject { ["type"] = "text", ["text"] = "yes" };
                break;
        }

        await Assert.ThrowsAnyAsync<ApiException>(() =>
            Setup(new JsonObject { ["answers"] = answers }).Model.DoDecideAsync(Call())
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preserves Liquid error messages and codes",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Preserves_Liquid_error_messages_and_codes()
    {
        var body = new JsonObject
        {
            ["error"] = new JsonObject
            {
                ["message"] = "Invalid API key provided.",
                ["type"] = "authentication_error",
                ["param"] = null,
                ["code"] = "invalid_api_key",
            },
        };
        var exception = await Assert.ThrowsAnyAsync<ApiException>(() =>
            Setup(body, HttpStatusCode.Unauthorized).Model.DoDecideAsync(Call())
        );
        Assert.Equal("Invalid API key provided. (invalid_api_key)", exception.Message);
        Assert.Equal(401, exception.StatusCode);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "restores the endpoint and authentication across workflow serialization",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Restores_the_endpoint_and_authentication_across_workflow_serialization()
    {
        var (_, model, _) = Setup();
        var serialized = model.Serialize();
        Assert.False(serialized.Config.ContainsKey("fetch"));
        Assert.False(serialized.Config.ContainsKey("httpClient"));
        var headers = Assert.IsAssignableFrom<IDictionary<string, object?>>(
            serialized.Config["headers"]
        );
        Assert.Equal(
            "Bearer test-key",
            Assert
                .Single(headers, pair =>
                    pair.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase)
                )
                .Value
        );
        var capture = new OpenAICapture { ResponseJson = TextFixture().ToJsonString() };
        await DecisionLiquidModel
            .Deserialize(serialized, new HttpClient(capture))
            .DoDecideAsync(Call());
        Assert.Equal("https://example.com/decisions/v1/systemone", capture.Uri);
        Assert.Equal("Bearer test-key", capture.Headers["Authorization"]);

        // A workflow runtime rehydrates the config from JSON, so values arrive as JsonElement.
        var rehydrated = new SerializedModelOptions(
            serialized.ModelId,
            JsonSerializer
                .Deserialize<Dictionary<string, JsonElement>>(
                    JsonSerializer.Serialize(serialized.Config)
                )!
                .ToDictionary(pair => pair.Key, pair => (object?)pair.Value)
        );
        var second = new OpenAICapture { ResponseJson = TextFixture().ToJsonString() };
        await DecisionLiquidModel
            .Deserialize(rehydrated, new HttpClient(second))
            .DoDecideAsync(Call());
        Assert.Equal("https://example.com/decisions/v1/systemone", second.Uri);
        Assert.Equal("Bearer test-key", second.Headers["Authorization"]);
    }

    private static Dictionary<string, double> Distribution(JsonNode probabilities)
    {
        return probabilities
            .AsObject()
            .ToDictionary(pair => pair.Key, pair => pair.Value!.GetValue<double>());
    }

    private static (IReadOnlyList<DecisionModelStatePart> State, Type Error) RejectedInput(
        int index
    )
    {
        switch (index)
        {
            case 0:
                return (
                    Enumerable
                        .Range(0, 9)
                        .Select(_ => (DecisionModelStatePart)Data("image/png", Png))
                        .ToList(),
                    typeof(InvalidArgumentException)
                );
            case 1:
                return (
                    Enumerable
                        .Range(0, 9)
                        .Select(_ => (DecisionModelStatePart)Data("audio/wav", Wav))
                        .ToList(),
                    typeof(InvalidArgumentException)
                );
            case 2:
                return (
                    [Base64("image/gif", "R0lGOD==")],
                    typeof(UnsupportedFunctionalityException)
                );
            case 3:
                return ([Data("audio", [1, 2, 3])], typeof(UnsupportedFunctionalityException));
            case 4:
                return ([Base64("video/mp4", "AAAA")], typeof(UnsupportedFunctionalityException));
            case 5:
                return (
                    [Url("image/png", "https://example.com/a.png")],
                    typeof(UnsupportedFunctionalityException)
                );
            default:
                return (
                    [Reference("image/png", "liquid", "file-1")],
                    typeof(UnsupportedFunctionalityException)
                );
        }
    }
}
