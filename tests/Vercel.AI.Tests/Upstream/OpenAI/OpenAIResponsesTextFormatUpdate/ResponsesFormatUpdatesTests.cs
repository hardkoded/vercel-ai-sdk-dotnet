// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.OpenAIResponsesTextFormatUpdate.TextFormatUpdateSupport;

namespace Vercel.AI.Tests.OpenAIResponsesTextFormatUpdate;

/// <summary>Port of <c>openai-responses-text-format-update.test.ts</c> &gt; <c>Responses format updates (%s)</c>.</summary>
public sealed class ResponsesFormatUpdatesTests
{
    private const string Prefix = "packages/openai/src/responses/openai-responses-text-format-update.test.ts::Responses format updates (%s)::";

    public static TheoryData<string> Methods() => new() { "generate", "stream" };

    public static TheoryData<string, string> MethodsAndProviders()
    {
        var data = new TheoryData<string, string>();
        foreach (var method in new[] { "generate", "stream" })
        {
            data.Add(method, "openai");
            data.Add(method, "azure");
        }

        return data;
    }

    public static TheoryData<string, string> IncompatibleSettings()
    {
        var data = new TheoryData<string, string>();
        foreach (var method in new[] { "generate", "stream" })
        {
            data.Add(method, "{\"reasoningMode\":\"pro\"}");
            data.Add(method, "{\"contextManagement\":[{\"type\":\"compaction\",\"compactThreshold\":1000}]}");
            data.Add(method, "{\"truncation\":\"auto\"}");
        }

        return data;
    }

    public static TheoryData<string, string?> PreviousResponseIds()
    {
        var data = new TheoryData<string, string?>();
        foreach (var method in new[] { "generate", "stream" })
        {
            data.Add(method, null);
            data.Add(method, "resp_previous");
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(MethodsAndProviders))]
    [UpstreamTest(Prefix + "preserves combined and format-only updates in %s history", Coverage = UpstreamCoverage.Covered, Commit = "5a645e92d2efc4563ae0dd7c6375327f61f4d271")]
    public async Task Preserves_combined_and_format_only_updates_in_history(string method, string provider)
    {
        var prompt = new List<ModelMessage>
        {
            User(),
            Update("{\"textFormatUpdate\":" + Format + ",\"reasoningEffortUpdate\":\"high\"}"),
            User(),
        };
        var options = Options(
            prompt,
            "{\"textFormat\":{\"type\":\"text\"},\"reasoningEffort\":\"low\",\"store\":false}",
            provider,
            responseSchema: "{\"type\":\"object\"}");
        var providerName = provider + ".responses";
        options.JsonResponse = true;

        var firstCapture = new OpenAICapture();
        var (first, _) = await Request(method, options, firstCapture, provider: providerName);
        OpenAIUpstream.Equal(first["text"]!["format"], "{\"type\":\"text\"}");
        Assert.Equal("low", first["reasoning"]!["effort"]!.GetValue<string>());
        OpenAIUpstream.Equal(
            first["input"]![1],
            "{\"type\":\"configuration_update\",\"reasoning\":{\"effort\":\"high\"},\"text\":{\"format\":" + Format + "}}");

        prompt.Add(Update("{\"textFormatUpdate\":{\"type\":\"text\"}}"));
        prompt.Add(User());
        var secondCapture = new OpenAICapture();
        var (second, _) = await Request(method, options, secondCapture, provider: providerName);
        var firstInput = first["input"]!.AsArray();
        for (var i = 0; i < firstInput.Count; i++)
        {
            Assert.True(JsonNode.DeepEquals(firstInput[i], second["input"]![i]));
        }

        OpenAIUpstream.Equal(
            second["input"]![3],
            "{\"type\":\"configuration_update\",\"text\":{\"format\":{\"type\":\"text\"}}}");
        Assert.True(JsonNode.DeepEquals(first["text"], second["text"]));
    }

    [Theory]
    [MemberData(nameof(Methods))]
    [UpstreamTest(Prefix + "normalizes both the original format and historical updates without mutating them", Coverage = UpstreamCoverage.Covered, Commit = "5a645e92d2efc4563ae0dd7c6375327f61f4d271")]
    public async Task Normalizes_both_the_original_format_and_historical_updates_without_mutating_them(string method)
    {
        var unnormalized = "{\"type\":\"json_schema\",\"name\":\"count\",\"strict\":true,\"schema\":{\"type\":\"object\",\"properties\":{\"total\":{\"type\":\"integer\"}},\"required\":[\"total\"],\"additionalProperties\":false,\"propertyNames\":{\"type\":\"string\"}}}";
        var options = Options(
            new ModelMessage[] { Update("{\"textFormatUpdate\":" + unnormalized + "}"), User() },
            "{\"textFormat\":" + unnormalized + "}");

        var (body, warnings) = await Request(method, options, new OpenAICapture());

        OpenAIUpstream.Equal(body["text"]!["format"]!["schema"], Schema);
        OpenAIUpstream.Equal(body["input"]![0]!["text"]!["format"]!["schema"], Schema);
        Assert.Equal(2, warnings.Count(warning => warning.Type == "compatibility"));
        Assert.Contains("propertyNames", unnormalized, StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(Methods))]
    [UpstreamTest(Prefix + "does not apply wire-schema normalization to a schema used only for Output validation", Coverage = UpstreamCoverage.Covered, Commit = "5a645e92d2efc4563ae0dd7c6375327f61f4d271")]
    public async Task Does_not_apply_wire_schema_normalization_to_a_schema_used_only_for_Output_validation(string method)
    {
        var options = Options(
            new ModelMessage[] { Update("{\"textFormatUpdate\":" + Format + "}"), User() },
            "{\"textFormat\":{\"type\":\"text\"}}",
            responseSchema: "{\"type\":\"object\",\"propertyNames\":{\"type\":\"number\"}}");

        var (body, _) = await Request(method, options, new OpenAICapture());

        OpenAIUpstream.Equal(body["text"]!["format"], "{\"type\":\"text\"}");
    }

    [Theory]
    [MemberData(nameof(Methods))]
    [UpstreamTest(Prefix + "retains the effective reasoning effort across format-only updates", Coverage = UpstreamCoverage.Covered, Commit = "5a645e92d2efc4563ae0dd7c6375327f61f4d271")]
    public async Task Retains_the_effective_reasoning_effort_across_format_only_updates(string method)
    {
        var options = Options(
            new ModelMessage[]
            {
                Update("{\"reasoningEffortUpdate\":\"none\"}"),
                User(),
                Update("{\"textFormatUpdate\":" + Format + "}"),
                User(),
            },
            "{\"reasoningEffort\":\"low\",\"reasoningSummary\":null}",
            temperature: 0.2,
            topP: 0.8);

        var (body, warnings) = await Request(method, options, new OpenAICapture());

        Assert.Equal(0.2, body["temperature"]!.GetValue<double>());
        Assert.Equal(0.8, body["top_p"]!.GetValue<double>());
        Assert.Empty(warnings);
    }

    [Theory]
    [MemberData(nameof(Methods))]
    [UpstreamTest(Prefix + "supports continuation with previousResponseId", Coverage = UpstreamCoverage.Covered, Commit = "5a645e92d2efc4563ae0dd7c6375327f61f4d271")]
    public async Task Supports_continuation_with_previousResponseId(string method)
    {
        var options = Options(
            new ModelMessage[] { Update("{\"textFormatUpdate\":" + Format + "}"), User() },
            "{\"previousResponseId\":\"resp_previous\"}");

        var (body, _) = await Request(method, options, new OpenAICapture());

        Assert.Equal("resp_previous", body["previous_response_id"]!.GetValue<string>());
        Assert.Null(body["text"]);
        OpenAIUpstream.Equal(body["input"]![0], "{\"type\":\"configuration_update\",\"text\":{\"format\":" + Format + "}}");
    }

    [Theory]
    [MemberData(nameof(IncompatibleSettings))]
    [UpstreamTest(Prefix + "rejects incompatible settings %j", Coverage = UpstreamCoverage.Covered, Commit = "5a645e92d2efc4563ae0dd7c6375327f61f4d271")]
    public async Task Rejects_incompatible_settings(string method, string settings)
    {
        var capture = new OpenAICapture();
        var options = Options(new ModelMessage[] { Update("{\"textFormatUpdate\":" + Format + "}"), User() }, settings);

        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => Request(method, options, capture));

        Assert.Equal("Message-level textFormatUpdate", error.Functionality);
        Assert.Equal("Configuration updates require standard reasoning mode without automatic compaction or automatic truncation", error.Message);
        Assert.Equal(0, capture.Calls);
    }

    [Theory]
    [MemberData(nameof(Methods))]
    [UpstreamTest(Prefix + "rejects unsupported models instead of silently losing historical updates", Coverage = UpstreamCoverage.Covered, Commit = "5a645e92d2efc4563ae0dd7c6375327f61f4d271")]
    public async Task Rejects_unsupported_models_instead_of_silently_losing_historical_updates(string method)
    {
        var capture = new OpenAICapture();
        var options = Options(new ModelMessage[] { Update("{\"textFormatUpdate\":" + Format + "}"), User() });

        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => Request(method, options, capture, "gpt-5.4"));

        Assert.Equal("Message-level textFormatUpdate", error.Functionality);
        Assert.Equal("Configuration updates are only supported by GPT-6 and later models", error.Message);
        Assert.Equal(0, capture.Calls);
    }

    [Theory]
    [MemberData(nameof(Methods))]
    [UpstreamTest(Prefix + "requires an empty control message", Coverage = UpstreamCoverage.Covered, Commit = "5a645e92d2efc4563ae0dd7c6375327f61f4d271")]
    public async Task Requires_an_empty_control_message(string method)
    {
        var options = Options(new ModelMessage[] { Update("{\"textFormatUpdate\":" + Format + "}", "Instructions"), User() });

        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => Request(method, options, new OpenAICapture()));

        Assert.Contains("Message-level textFormatUpdate requires empty system message content.", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(Methods))]
    [UpstreamTest(Prefix + "rejects adjacent reasoning and format updates", Coverage = UpstreamCoverage.Covered, Commit = "5a645e92d2efc4563ae0dd7c6375327f61f4d271")]
    public async Task Rejects_adjacent_reasoning_and_format_updates(string method)
    {
        var options = Options(new ModelMessage[]
        {
            Update("{\"reasoningEffortUpdate\":\"low\"}"),
            Update("{\"textFormatUpdate\":" + Format + "}"),
            User(),
        });

        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => Request(method, options, new OpenAICapture()));

        Assert.Equal("Adjacent configuration updates", error.Functionality);
    }

    [Theory]
    [MemberData(nameof(PreviousResponseIds))]
    [UpstreamTest(Prefix + "rejects request-level effort with a leading format update (previousResponseId: %s)", Coverage = UpstreamCoverage.Covered, Commit = "5a645e92d2efc4563ae0dd7c6375327f61f4d271")]
    public async Task Rejects_request_level_effort_with_a_leading_format_update(string method, string? previousResponseId)
    {
        var capture = new OpenAICapture();
        var openai = new JsonObject { ["reasoningEffortUpdate"] = "high" };
        if (previousResponseId != null)
        {
            openai["previousResponseId"] = previousResponseId;
        }

        var options = Options(new ModelMessage[] { Update("{\"textFormatUpdate\":" + Format + "}"), User() }, openai.ToJsonString());

        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => Request(method, options, capture));

        Assert.Equal("Adjacent configuration updates", error.Functionality);
        Assert.Equal(
            "Configuration updates cannot be consecutive. Set reasoningEffortUpdate and textFormatUpdate in the same empty system message, and omit request-level reasoningEffortUpdate when using message-level updates.",
            error.Message);
        Assert.Equal(0, capture.Calls);
    }

    [Theory]
    [MemberData(nameof(Methods))]
    [UpstreamTest(Prefix + "combines effort and format in the leading message without prepending a duplicate request update", Coverage = UpstreamCoverage.Covered, Commit = "5a645e92d2efc4563ae0dd7c6375327f61f4d271")]
    public async Task Combines_effort_and_format_in_the_leading_message_without_prepending_a_duplicate_request_update(string method)
    {
        var options = Options(
            new ModelMessage[] { Update("{\"textFormatUpdate\":" + Format + ",\"reasoningEffortUpdate\":\"high\"}"), User() },
            "{\"reasoningEffortUpdate\":\"high\"}");

        var (body, _) = await Request(method, options, new OpenAICapture());

        Assert.Equal(2, body["input"]!.AsArray().Count);
    }

    [Theory]
    [MemberData(nameof(Methods))]
    [UpstreamTest(Prefix + "allows explicit compaction after configuration updates", Coverage = UpstreamCoverage.Covered, Commit = "5a645e92d2efc4563ae0dd7c6375327f61f4d271")]
    public async Task Allows_explicit_compaction_after_configuration_updates(string method)
    {
        var options = Options(
            new ModelMessage[] { Update("{\"textFormatUpdate\":" + Format + "}"), User() },
            "{\"compactionTrigger\":true}");

        var (body, _) = await Request(method, options, new OpenAICapture());

        var input = body["input"]!.AsArray();
        OpenAIUpstream.Equal(input[^1], "{\"type\":\"compaction_trigger\"}");
    }

    [Theory]
    [MemberData(nameof(Methods))]
    [UpstreamTest(Prefix + "rejects json_object as an update format", Coverage = UpstreamCoverage.Covered, Commit = "5a645e92d2efc4563ae0dd7c6375327f61f4d271")]
    public async Task Rejects_json_object_as_an_update_format(string method)
    {
        var options = Options(new ModelMessage[] { Update("{\"textFormatUpdate\":{\"type\":\"json_object\"}}"), User() });

        await Assert.ThrowsAsync<InvalidArgumentException>(() => Request(method, options, new OpenAICapture()));
    }
}
