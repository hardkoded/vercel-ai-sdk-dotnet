// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.ProviderUtils.ExtractionLanguageModelSuite.ExtractionLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.ExtractionLanguageModelSuite;

/// <summary>Port of <c>describe('maxFieldsPerCall')</c> in <c>extraction-language-model.test.ts</c>.</summary>
public sealed class MaxFieldsPerCallTests
{
    private const string Describe = Prefix + "maxFieldsPerCall::";

    public static TheoryData<int> InvalidMaxFieldsPerCall => new() { 0, -1 };

    [Fact]
    [UpstreamTest(Describe + "sends every field in one call by default", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_every_field_in_one_call_by_default()
    {
        var (model, language, _) = Setup();
        language.OnGenerate = (call, _) => Task.FromResult(Respond(call));

        var result = await model.DoExtractAsync(Call(ManyFields()));

        var call = Assert.Single(language.Calls);
        Assert.Equal(40, Properties(call).Count);
        AssertAnswer("number", 38d, result.Answers["price 38"]);
    }

    [Fact]
    [UpstreamTest(Describe + "splits fields into parallel nullable calls and merges the answers", Coverage = UpstreamCoverage.Covered)]
    public async Task Splits_fields_into_parallel_nullable_calls_and_merges_the_answers()
    {
        var (model, language, _) = Setup(maxFieldsPerCall: 16);
        language.OnGenerate = (call, _) => Task.FromResult(Respond(call));

        var result = await model.DoExtractAsync(Call(ManyFields()));

        Assert.Equal(3, language.Calls.Count);
        var sizes = language.Calls.Select(call =>
        {
            var properties = Properties(call);
            Assert.All(properties, property => Assert.Equal(2, property.Value!["anyOf"]!.AsArray().Count));
            return properties.Count;
        }).ToList();
        Assert.Equal(new[] { 16, 16, 8 }, sizes);
        Assert.Equal(ManyFields().Keys, result.Answers.Keys);
        for (var i = 0; i < 40; i++)
        {
            AssertAnswer("number", i % 2 == 0 ? (double)i : null, result.Answers["price " + i.ToString(CultureInfo.InvariantCulture)]);
        }

        Assert.Equal(60, result.Usage!.InputTokens);
        Assert.Equal(36, result.Usage.OutputTokens);
        Assert.Null(result.Usage.TotalTokens);
        Assert.Equal(3, result.Warnings.Count);
        JsonAssert.Equal(result.ProviderMetadata!.Value, "{\"test\":{\"call\":true}}");
        Assert.Equal("response", result.Response!.Id);
        Assert.Equal("resolved", result.Response.ModelId);
    }

    [Fact]
    [UpstreamTest(Describe + "keeps field ids that match object prototype properties when merging", Coverage = UpstreamCoverage.Partial, Note = "C# dictionaries have no prototype, so the Object.getPrototypeOf check has no counterpart. The merged ids and their order are checked.")]
    public async Task Keeps_field_ids_that_match_object_prototype_properties_when_merging()
    {
        var (model, language, _) = Setup(maxFieldsPerCall: 1);
        language.OnGenerate = (call, _) => Task.FromResult(Respond(call, "{\"f0\":\"x\"}"));

        var result = await model.DoExtractAsync(Call(new Dictionary<string, ExtractionField>
        {
            ["__proto__"] = new ExtractionField("text"),
            ["constructor"] = new ExtractionField("text"),
        }));

        Assert.Equal(new[] { "__proto__", "constructor" }, result.Answers.Keys);
    }

    [Fact]
    [UpstreamTest(Describe + "reports unknown usage when any call does not report it", Coverage = UpstreamCoverage.Covered)]
    public async Task Reports_unknown_usage_when_any_call_does_not_report_it()
    {
        var (model, language, _) = Setup(maxFieldsPerCall: 16);
        var first = true;
        language.OnGenerate = (call, _) =>
        {
            if (!first)
            {
                return Task.FromResult(Respond(call));
            }

            first = false;
            return Task.FromResult(Respond(call, usage: new LanguageModelUsage(null, 12, null, textTokens: 12)));
        };

        var result = await model.DoExtractAsync(Call(ManyFields()));

        Assert.Null(result.Usage!.InputTokens);
        Assert.Equal(36, result.Usage.OutputTokens);
    }

    [Fact]
    [UpstreamTest(Describe + "fails the whole call and aborts sibling calls when one call fails", Coverage = UpstreamCoverage.Covered)]
    public async Task Fails_the_whole_call_and_aborts_sibling_calls_when_one_call_fails()
    {
        var (model, language, _) = Setup(maxFieldsPerCall: 1);
        var error = new InvalidOperationException("first call failed");
        language.OnGenerate = (_, token) => language.Tokens.Count == 1
            ? Task.FromException<LanguageModelGenerateResult>(error)
            : UntilCancelled(token);

        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() => model.DoExtractAsync(Call(new Dictionary<string, ExtractionField>
        {
            ["a"] = new ExtractionField("text"),
            ["b"] = new ExtractionField("text"),
            ["c"] = new ExtractionField("text"),
        })));

        Assert.Same(error, thrown);
        Assert.Equal(3, language.Tokens.Count);
        Assert.All(language.Tokens, token => Assert.True(token.IsCancellationRequested));
    }

    [Fact]
    [UpstreamTest(Describe + "forwards a caller abort to every call and removes its listener", Coverage = UpstreamCoverage.Partial, Note = "The listener is the linked CancellationTokenSource registration, released by its disposal. A CancellationToken does not expose its registrations, so the removal is not checked.")]
    public async Task Forwards_a_caller_abort_to_every_call_and_removes_its_listener()
    {
        var (model, language, _) = Setup(maxFieldsPerCall: 1);
        using var controller = new CancellationTokenSource();
        language.OnGenerate = (_, token) => UntilCancelled(token);

        var extraction = model.DoExtractAsync(
            Call(
                new Dictionary<string, ExtractionField> { ["a"] = new ExtractionField("text"), ["b"] = new ExtractionField("text") },
                cancellationToken: controller.Token),
            controller.Token);
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (language.Tokens.Count < 2 && DateTime.UtcNow < deadline)
        {
            await Task.Delay(10);
        }

        Assert.Equal(2, language.Tokens.Count);
        controller.Cancel();

        var thrown = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => extraction);
        Assert.Equal(controller.Token, thrown.CancellationToken);
        Assert.All(language.Tokens, token => Assert.True(token.IsCancellationRequested));
    }

    [Theory]
    [MemberData(nameof(InvalidMaxFieldsPerCall))]
    [UpstreamTest(Describe + "rejects maxFieldsPerCall %s", Coverage = UpstreamCoverage.Partial, Note = "maxFieldsPerCall is an int?, so the 1.5 and NaN cases cannot be expressed. 0 and -1 are checked.")]
    public void Rejects_maxFieldsPerCall(int maxFieldsPerCall)
    {
        Assert.Throws<InvalidArgumentException>(() => Setup(maxFieldsPerCall: maxFieldsPerCall));
    }

    private static Dictionary<string, ExtractionField> ManyFields()
    {
        return Enumerable.Range(0, 40).ToDictionary(i => "price " + i.ToString(CultureInfo.InvariantCulture), _ => new ExtractionField("number"));
    }

    private static JsonObject Properties(LanguageModelCallOptions call)
    {
        return JsonNode.Parse(call.JsonSchema!.Value.GetRawText())!["properties"]!.AsObject();
    }

    // Port of answerFromPrompt: answers each field with its index when even, else null.
    private static string AnswerFromPrompt(LanguageModelCallOptions call)
    {
        var message = Assert.IsType<UserModelMessage>(call.Prompt[1]);
        var part = Assert.IsType<TextContentPart>(message.Content[0]);
        var descriptions = JsonNode.Parse(part.Text)!["fields"]!.AsObject();
        var answers = new JsonObject();
        foreach (var pair in descriptions)
        {
            var index = int.Parse(pair.Value!["id"]!.GetValue<string>().Split(' ')[1], CultureInfo.InvariantCulture);
            answers[pair.Key] = index % 2 == 0 ? index : null;
        }

        return answers.ToJsonString();
    }

    // Port of respond.
    private static LanguageModelGenerateResult Respond(LanguageModelCallOptions call, string? text = null, LanguageModelUsage? usage = null)
    {
        using var metadata = JsonDocument.Parse("{\"test\":{\"call\":true}}");
        return new LanguageModelGenerateResult(
            Text(text ?? AnswerFromPrompt(call)),
            FinishReason.Stop,
            usage ?? new LanguageModelUsage(20, 12, 32, cacheReadTokens: 0, cacheWriteTokens: 0, reasoningTokens: 0, noCacheInputTokens: 20, textTokens: 12),
            rawFinishReason: "end",
            warnings: new[] { new CallWarning("other", "call warning") },
            responseId: "response",
            providerMetadata: metadata.RootElement.Clone(),
            responseModelId: "resolved");
    }
}
