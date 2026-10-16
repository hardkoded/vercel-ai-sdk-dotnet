// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.DecisionFixtures;
using static Vercel.AI.Tests.Upstream.ProviderUtils.DecisionLanguageModelSuite.DecisionLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.DecisionLanguageModelSuite;

/// <summary>Port of <c>decision-language-model.test.ts</c>.</summary>
public sealed class DecisionLanguageModelTests
{
    private const string Prefix = "packages/provider-utils/src/decision-language-model.test.ts::";

    public static TheoryData<double> BooleanProbabilities => new() { 0, 0.02, 0.5, 0.98, 1 };

    public static TheoryData<string> InvalidBooleanProbabilities => new() { "-0.01", "1.01", "1e999", "-1e999", "null", "true", "false", "\"0.5\"", "{}", "[]" };

    public static TheoryData<string> MissingOrExtraBooleanAnswers => new()
    {
        "{\"q0\":\"c1\",\"q1\":1.25}",
        "{\"q0\":\"c1\",\"q1\":1.25,\"q2\":0.5,\"extra\":0.5}",
    };

    public static TheoryData<FinishReason> UnfinishedReasons => new() { FinishReason.Length, FinishReason.ContentFilter, FinishReason.ToolCalls, FinishReason.Error, FinishReason.Other };

    public static TheoryData<string> MalformedOutputs => new()
    {
        string.Empty,
        "not JSON",
        "```json\n{}\n```",
        "null",
        "[]",
        "{}",
        "{\"q0\":\"c1\"}",
        "{\"q0\":\"c1\",\"q1\":1,\"extra\":true}",
        "{\"q0\":\"C1\",\"q1\":1}",
        "{\"q0\":\"c01\",\"q1\":1}",
        "{\"q0\":\"c3\",\"q1\":1}",
        "{\"q0\":1,\"q1\":1}",
        "{\"q0\":\"c1\",\"q1\":\"1\"}",
        "{\"q0\":\"c1\",\"q1\":-0.01}",
        "{\"q0\":\"c1\",\"q1\":2.01}",
        "{\"q0\":\"c1\",\"q1\":1e999}",
        "{\"q0\":\"c1\",\"q1\":null}",
        "{\"q0\":\"c1\",\"q1\":1,\"__proto__\":{}}",
    };

    public static TheoryData<string> InvalidRubricShapes => new() { "no questions", "choice without options", "score with one level" };

    [Fact]
    [UpstreamTest(Prefix + "maps exact caller labels and fractional scores without inventing distributions", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_exact_caller_labels_and_fractional_scores_without_inventing_distributions()
    {
        var (model, language, _) = Setup();

        var result = await model.DoDecideAsync(Call());

        AssertChoice("Needs review", result.Answers["category"]);
        AssertScore(1.25, result.Answers["severity"]);
        Assert.Single(language.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "accepts exact choice labels when an internal option code is not returned", Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_exact_choice_labels_when_an_internal_option_code_is_not_returned()
    {
        var (model, _, _) = Setup(Text("{\"q1\":1.25,\"q0\":\"Needs review\"}"));

        var result = await model.DoDecideAsync(Call());

        AssertChoice("Needs review", result.Answers["category"]);
        AssertScore(1.25, result.Answers["severity"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "prefers internal option codes over identical choice labels", Coverage = UpstreamCoverage.Covered)]
    public async Task Prefers_internal_option_codes_over_identical_choice_labels()
    {
        var (model, _, _) = Setup(Text("{\"q0\":\"c1\"}"));
        var questions = new Dictionary<string, DecisionQuestion>
        {
            ["category"] = Category(new Dictionary<string, object?> { ["c1"] = "first label", ["other"] = "second label" }),
        };

        var result = await model.DoDecideAsync(Call(questions));

        AssertChoice("other", result.Answers["category"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "uses a portable flat schema with required fields and internal option codes", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_a_portable_flat_schema_with_required_fields_and_internal_option_codes()
    {
        var (model, language, _) = Setup();

        await model.DoDecideAsync(Call());

        var call = language.Calls[0];
        Assert.Equal("decision", call.JsonSchemaName);
        var schema = JsonNode.Parse(call.JsonSchema!.Value.GetRawText())!;
        var description = schema["properties"]!["q1"]!["description"]!.GetValue<string>();
        Assert.Contains("0 to 2", description, StringComparison.Ordinal);
        AssertJson(
            "{\"type\":\"object\",\"properties\":{\"q0\":{\"type\":\"string\",\"enum\":[\"c0\",\"c1\",\"c2\"]},\"q1\":{\"type\":\"number\",\"description\":" + JsonSerializer.Serialize(description) + "}},\"required\":[\"q0\",\"q1\"],\"additionalProperties\":false}",
            schema);

        var message = Assert.IsType<UserModelMessage>(call.Prompt[1]);
        AssertJson(
            "{\"questions\":{"
            + "\"q0\":{\"id\":\"category\",\"type\":\"choice\",\"instructions\":{\"task\":[\"Pick the exact label\"]},\"criteria\":{"
            + "\"c0\":{\"label\":\"Needs Review\",\"description\":{\"meaning\":\"manual\"}},"
            + "\"c1\":{\"label\":\"Needs review\",\"description\":[\"automatic\"]},"
            + "\"c2\":{\"label\":\"other\",\"description\":null}}},"
            + "\"q1\":{\"id\":\"severity\",\"type\":\"score\",\"instructions\":[\"Rate the impact\"],\"criteria\":[\"low\",{\"meaning\":\"medium\"},null]}}}",
            JsonNode.Parse(Assert.IsType<TextContentPart>(message.Content[0]).Text));
        Assert.Equal("Shared state:", Assert.IsType<TextContentPart>(message.Content[1]).Text);
        Assert.Equal(JsonSerializer.Serialize(StateValue()), Assert.IsType<TextContentPart>(message.Content[2]).Text);
        Assert.Null(call.Temperature);
        Assert.Null(call.Tools);
        Assert.Equal("none", call.Reasoning);
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves usage, response, warnings, and provider metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_usage_response_warnings_and_provider_metadata()
    {
        var (model, _, result) = Setup();

        var output = await model.DoDecideAsync(Call());

        Assert.Equal(20, output.Usage!.InputTokens);
        Assert.Equal(12, output.Usage.OutputTokens);
        Assert.Null(output.Usage.TotalTokens);
        Assert.Null(output.Usage.Tokens);
        var warning = Assert.Single(output.Warnings);
        Assert.Equal(result.Warnings[0].Type, warning.Type);
        Assert.Equal(result.Warnings[0].Message, warning.Message);
        Assert.Equal("response", output.Response!.Id);
        Assert.Equal("resolved", output.Response.ModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(0).UtcDateTime, output.Response.Timestamp);
        Assert.Equal(result.ResponseHeaders, output.Response.Headers);
        Assert.Equal("{\"raw\":true}", output.Response.Body!.Value.GetRawText());
        Assert.Equal(result.ProviderMetadata!.Value.GetRawText(), output.ProviderMetadata!.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(Prefix + "forwards cancellation, headers, and provider options unchanged", Coverage = UpstreamCoverage.Covered)]
    public async Task Forwards_cancellation_headers_and_provider_options_unchanged()
    {
        var (model, language, _) = Setup();
        using var controller = new CancellationTokenSource();
        var headers = new Dictionary<string, string> { ["test"] = "header" };

        await model.DoDecideAsync(Call(providerOptions: "{\"test\":{\"reasoning\":\"low\"}}", headers: headers, cancellationToken: controller.Token), controller.Token);

        var call = language.Calls[0];
        Assert.Equal("none", call.Reasoning);
        Assert.Equal(controller.Token, language.Tokens[0]);
        Assert.Equal("header", Assert.Single(call.Headers!).Value);
        Assert.Equal("test", call.Headers!.Keys.Single());
        var option = Assert.Single(call.ProviderOptions!);
        Assert.Equal("test", option.Key);
        Assert.Equal("{\"reasoning\":\"low\"}", option.Value.GetRawText());
    }

    [Theory]
    [MemberData(nameof(BooleanProbabilities))]
    [UpstreamTest(Prefix + "preserves Boolean P(true) %s without thresholding in a mixed decision", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_Boolean_P_true_without_thresholding_in_a_mixed_decision(double probability)
    {
        var (model, language, _) = Setup(Text("{\"q2\":" + probability.ToString("R", CultureInfo.InvariantCulture) + ",\"q1\":1.25,\"q0\":\"c1\"}"));
        const string instructions = "{\"task\":[\"Is a refund requested?\"]}";
        const string whenTrue = "{\"meaning\":\"A refund is requested\"}";
        const string whenFalse = "[\"No refund requested\"]";
        var questions = Questions();
        questions["flag"] = Q("boolean", instructions, new Dictionary<string, object?> { ["true"] = whenTrue, ["false"] = whenFalse });

        var result = await model.DoDecideAsync(Call(questions));

        Assert.Equal(new[] { "choice", "score", "boolean" }, model.SupportedQuestionTypes);
        Assert.Equal(new[] { "category", "severity", "flag" }, result.Answers.Keys);
        AssertChoice("Needs review", result.Answers["category"]);
        AssertScore(1.25, result.Answers["severity"]);
        var flag = result.Answers["flag"];
        Assert.Equal("boolean", flag.Type);
        Assert.Equal(probability, flag.Probability);
        Assert.Null(flag.Choice);
        Assert.Null(flag.Score);
        Assert.Null(flag.Probabilities);
        var call = Assert.Single(language.Calls);
        var schema = JsonNode.Parse(call.JsonSchema!.Value.GetRawText())!;
        var q2 = schema["properties"]!["q2"]!.AsObject();
        Assert.Equal("number", q2["type"]!.GetValue<string>());
        Assert.Contains("probability", q2["description"]!.GetValue<string>(), StringComparison.Ordinal);
        Assert.False(q2.ContainsKey("minimum"));
        Assert.False(q2.ContainsKey("maximum"));
        AssertJson("[\"q0\",\"q1\",\"q2\"]", schema["required"]);
        var message = Assert.IsType<UserModelMessage>(call.Prompt[1]);
        var expected = new JsonObject
        {
            ["id"] = "flag",
            ["type"] = "boolean",
            ["instructions"] = instructions,
            ["criteria"] = new JsonObject { ["true"] = whenTrue, ["false"] = whenFalse },
        };
        AssertJson(expected.ToJsonString(), JsonNode.Parse(Assert.IsType<TextContentPart>(message.Content[0]).Text)!["questions"]!["q2"]);
        Assert.Contains("not confidence in whichever outcome", Assert.IsType<SystemModelMessage>(call.Prompt[0]).Content, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Prefix + "supports Boolean questions without criteria", Coverage = UpstreamCoverage.Covered)]
    public async Task Supports_Boolean_questions_without_criteria()
    {
        var (model, _, _) = Setup(Text("{\"q0\":0.25}"));

        var result = await model.DoDecideAsync(Call(
            new Dictionary<string, DecisionQuestion> { ["flag"] = Q("boolean", "Yes?") },
            TextState("test")));

        var flag = Assert.Single(result.Answers);
        Assert.Equal("flag", flag.Key);
        Assert.Equal("boolean", flag.Value.Type);
        Assert.Equal(0.25, flag.Value.Probability);
    }

    [Theory]
    [MemberData(nameof(InvalidBooleanProbabilities))]
    [UpstreamTest(Prefix + "rejects invalid Boolean probability %s without returning partial answers", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_invalid_Boolean_probability_without_returning_partial_answers(string value)
    {
        var (model, _, _) = Setup(Text("{\"q0\":\"c1\",\"q1\":1.25,\"q2\":" + value + "}"));

        await Assert.ThrowsAsync<InvalidResponseDataException>(() => model.DoDecideAsync(Call(WithFlag())));
    }

    [Theory]
    [MemberData(nameof(MissingOrExtraBooleanAnswers))]
    [UpstreamTest(Prefix + "rejects missing or extra answers for mixed Boolean decisions: %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_missing_or_extra_answers_for_mixed_Boolean_decisions(string text)
    {
        var (model, _, _) = Setup(Text(text));

        await Assert.ThrowsAsync<InvalidResponseDataException>(() => model.DoDecideAsync(Call(WithFlag())));
    }

    [Theory]
    [MemberData(nameof(UnfinishedReasons))]
    [UpstreamTest(Prefix + "rejects %s even with valid JSON", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_even_with_valid_JSON(FinishReason finishReason)
    {
        var (model, _, _) = Setup(finishReason: finishReason);

        await Assert.ThrowsAsync<InvalidResponseDataException>(() => model.DoDecideAsync(Call()));
    }

    [Theory]
    [MemberData(nameof(MalformedOutputs))]
    [UpstreamTest(Prefix + "rejects malformed decision output %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_malformed_decision_output(string text)
    {
        var (model, _, _) = Setup(Text(text));

        await Assert.ThrowsAsync<InvalidResponseDataException>(() => model.DoDecideAsync(Call()));
    }

    [Fact]
    [UpstreamTest(Prefix + "joins text parts and ignores reasoning", Coverage = UpstreamCoverage.Covered)]
    public async Task Joins_text_parts_and_ignores_reasoning()
    {
        var (model, _, _) = Setup(new GeneratedContent[]
        {
            new GeneratedReasoning("internal"),
            new GeneratedText("{\"q0\":\"c1\","),
            new GeneratedText("\"q1\":1}"),
        });

        var result = await model.DoDecideAsync(Call());

        AssertScore(1, result.Answers["severity"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves unusual IDs without using them as schema property names", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_unusual_IDs_without_using_them_as_schema_property_names()
    {
        var (model, _, _) = Setup();
        var weird = new Dictionary<string, DecisionQuestion> { ["__proto__"] = Category(), ["a.b [✓]"] = Severity() };

        var result = await model.DoDecideAsync(Call(weird));

        Assert.Equal(new[] { "__proto__", "a.b [✓]" }, result.Answers.Keys);
        Assert.True(result.Answers.ContainsKey("__proto__"));
    }

    [Fact]
    [UpstreamTest(Prefix + "does not retry or wrap language-model errors", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_retry_or_wrap_language_model_errors()
    {
        var (model, language, _) = Setup();
        var error = new ApiCallError("retry later", "https://example.com", new Dictionary<string, object?>(), 429);
        language.OnGenerate = (_, _) => throw error;

        var thrown = await Assert.ThrowsAsync<ApiCallError>(() => model.DoDecideAsync(Call()));

        Assert.Same(error, thrown);
        Assert.Single(language.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects pre-aborted requests without invoking the model", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_pre_aborted_requests_without_invoking_the_model()
    {
        var (model, language, _) = Setup();
        using var controller = new CancellationTokenSource();
        controller.Cancel();

        var thrown = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => model.DoDecideAsync(Call(cancellationToken: controller.Token), controller.Token));

        Assert.Equal(controller.Token, thrown.CancellationToken);
        Assert.Empty(language.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "checks cancellation after the model returns", Coverage = UpstreamCoverage.Covered)]
    public async Task Checks_cancellation_after_the_model_returns()
    {
        var (model, language, result) = Setup();
        using var controller = new CancellationTokenSource();
        language.OnGenerate = (_, _) =>
        {
            controller.Cancel();
            return Task.FromResult(result);
        };

        var thrown = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => model.DoDecideAsync(Call(cancellationToken: controller.Token), controller.Token));

        Assert.Equal(controller.Token, thrown.CancellationToken);
    }

    [Theory]
    [MemberData(nameof(InvalidRubricShapes))]
    [UpstreamTest(Prefix + "rejects invalid rubric shapes before model I/O", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_invalid_rubric_shapes_before_model_IO(string shape)
    {
        var (model, language, _) = Setup();
        var questions = shape switch
        {
            "no questions" => new Dictionary<string, DecisionQuestion>(),
            "choice without options" => new Dictionary<string, DecisionQuestion> { ["category"] = Category(new Dictionary<string, object?>()) },
            _ => new Dictionary<string, DecisionQuestion> { ["severity"] = Severity(new List<object?> { "only" }) },
        };

        await Assert.ThrowsAsync<InvalidArgumentException>(() => model.DoDecideAsync(Call(questions, TextState("text"))));

        Assert.Empty(language.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "sends images as files and preserves state part order in the prompt", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_images_as_files_and_preserves_state_part_order_in_the_prompt()
    {
        var (model, language, _) = Setup();
        var image = new Dictionary<string, object?>
        {
            ["type"] = "file",
            ["mediaType"] = "image/png",
            ["data"] = new Dictionary<string, object?> { ["type"] = "data", ["data"] = new byte[] { 1, 2 } },
            ["providerOptions"] = new Dictionary<string, object?> { ["test"] = new Dictionary<string, object?> { ["detail"] = "high" } },
        };

        await model.DoDecideAsync(Call(state: new List<object?>
        {
            new Dictionary<string, object?> { ["type"] = "text", ["text"] = "Inspect." },
            image,
            new Dictionary<string, object?> { ["type"] = "json", ["value"] = new List<object?> { 1, null } },
        }));

        var user = Assert.IsType<UserModelMessage>(language.Calls[0].Prompt[1]);
        Assert.Equal(5, user.Content.Count);
        Assert.Contains("\"questions\"", Assert.IsType<TextContentPart>(user.Content[0]).Text, StringComparison.Ordinal);
        Assert.Equal("Shared state:", Assert.IsType<TextContentPart>(user.Content[1]).Text);
        Assert.Equal("Inspect.", Assert.IsType<TextContentPart>(user.Content[2]).Text);
        var file = Assert.IsType<FileContentPart>(user.Content[3]);
        Assert.Equal("image/png", file.MediaType);
        Assert.Equal(new byte[] { 1, 2 }, file.Data);
        Assert.Null(file.Url);
        Assert.Null(file.FileName);
        var option = Assert.Single(file.ProviderOptions!);
        Assert.Equal("test", option.Key);
        Assert.Equal("{\"detail\":\"high\"}", option.Value.GetRawText());
        Assert.Equal("[1,null]", Assert.IsType<TextContentPart>(user.Content[4]).Text);
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects unsupported media before generating a decision", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_unsupported_media_before_generating_a_decision()
    {
        var (model, language, _) = Setup();

        await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => model.DoDecideAsync(Call(state: new List<object?>
        {
            new Dictionary<string, object?>
            {
                ["type"] = "file",
                ["mediaType"] = "audio/wav",
                ["data"] = new Dictionary<string, object?> { ["type"] = "data", ["data"] = "AAAA" },
            },
        })));

        Assert.Empty(language.Calls);
    }

    private static GeneratedContent[] Text(string text)
    {
        return new GeneratedContent[] { new GeneratedText(text) };
    }

    private static List<object?> TextState(string text)
    {
        return new List<object?> { new Dictionary<string, object?> { ["type"] = "text", ["text"] = text } };
    }

    private static Dictionary<string, DecisionQuestion> WithFlag()
    {
        var questions = Questions();
        questions["flag"] = Q("boolean", "Yes?");
        return questions;
    }

    private static void AssertChoice(string choice, DecisionAnswer answer)
    {
        Assert.Equal("choice", answer.Type);
        Assert.Equal(choice, answer.Choice);
    }

    private static void AssertScore(double score, DecisionAnswer answer)
    {
        Assert.Equal("score", answer.Type);
        Assert.Equal(score, answer.Score);
    }

    private static void AssertJson(string expected, JsonNode? actual)
    {
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expected), actual), actual?.ToJsonString());
    }
}
