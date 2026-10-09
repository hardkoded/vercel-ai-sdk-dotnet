// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.OpenAIResponsesReasoningEffortUpdate;

/// <summary>Port of <c>openai-responses-reasoning-effort-update.test.ts</c> &gt; <c>non-reasoning updates for %s</c>.</summary>
public sealed class NonReasoningUpdatesTests
{
    private const string Prefix = "packages/openai/src/responses/openai-responses-reasoning-effort-update.test.ts::positioned reasoning effort updates (%s) > non-reasoning updates for %s::";

    private const string WireUser = "{\"role\":\"user\",\"content\":[{\"type\":\"input_text\",\"text\":\"Question\"}]}";

    private static readonly string[] Methods = { "generate", "stream" };

    private static readonly string[] Models = { "gpt-6-sol", "gpt-6-luna" };

    public static TheoryData<string, string, string, string[], string?, string?, bool> SamplingCases()
    {
        var data = new TheoryData<string, string, string, string[], string?, string?, bool>();
        foreach (var method in Methods)
        {
            foreach (var model in Models)
            {
                data.Add(method, model, "request update enables reasoning", new[] { "user" }, "none", "low", false);
                data.Add(method, model, "request update disables reasoning", new[] { "user" }, "low", "none", true);
                data.Add(method, model, "positioned update enables reasoning", new[] { "user", "low", "user" }, "none", null, false);
                data.Add(method, model, "last positioned update disables reasoning", new[] { "user", "low", "user", "none", "user" }, "none", null, true);
                data.Add(method, model, "positioned update overrides prepended request update", new[] { "user", "low", "user" }, "low", "none", false);
                data.Add(method, model, "positioned update disables default reasoning", new[] { "none", "user" }, null, null, true);
            }
        }

        return data;
    }

    public static TheoryData<string, string> MethodAndModel()
    {
        var data = new TheoryData<string, string>();
        foreach (var method in Methods)
        {
            foreach (var model in Models)
            {
                data.Add(method, model);
            }
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(SamplingCases))]
    [UpstreamTest(Prefix + "handles sampling and logprobs when $name", Coverage = UpstreamCoverage.Covered, Note = "The port has no reasoningSummary default, so reasoningSummary: null is not sent.")]
    public async Task Handles_sampling_and_logprobs_when_name(
        string method,
        string modelId,
        string name,
        string[] promptKinds,
        string? reasoningEffort,
        string? reasoningEffortUpdate,
        bool samplingSupported)
    {
        Assert.NotEmpty(name);
        var options = new JsonObject();
        if (reasoningEffort != null)
        {
            options["reasoningEffort"] = reasoningEffort;
        }

        if (reasoningEffortUpdate != null)
        {
            options["reasoningEffortUpdate"] = reasoningEffortUpdate;
        }

        options["logprobs"] = 2;
        options["include"] = new JsonArray("message.output_text.logprobs", "reasoning.encrypted_content");

        var (body, warnings) = await Request(method, Build(promptKinds), options, modelId, temperature: 0, topP: 0.9);

        Assert.Equal(reasoningEffort, body["reasoning"]?["effort"]?.GetValue<string>());
        Assert.Equal(samplingSupported ? 0.0 : (double?)null, body["temperature"]?.GetValue<double>());
        Assert.Equal(samplingSupported ? 0.9 : (double?)null, body["top_p"]?.GetValue<double>());
        Assert.Equal(samplingSupported ? 2 : (int?)null, body["top_logprobs"]?.GetValue<int>());
        OpenAIUpstream.Equal(
            body["include"],
            samplingSupported
                ? "[\"message.output_text.logprobs\",\"reasoning.encrypted_content\"]"
                : "[\"reasoning.encrypted_content\"]");
        if (samplingSupported)
        {
            Assert.Empty(warnings);
        }
        else
        {
            Assert.Equal(3, warnings.Count);
            var features = new[] { "temperature", "topP", "logprobs" };
            for (var i = 0; i < features.Length; i++)
            {
                Assert.Equal("unsupported", warnings[i].Type);
                Assert.Equal(features[i] + " is not supported for reasoning models", warnings[i].Message);
            }
        }
    }

    [Theory]
    [MemberData(nameof(MethodAndModel))]
    [UpstreamTest(Prefix + "preserves positioned none updates and the initial reasoning effort", Coverage = UpstreamCoverage.Covered, Note = "The port has no reasoningSummary default, so reasoningSummary: null is not sent.")]
    public async Task Preserves_positioned_none_updates_and_the_initial_reasoning_effort(string method, string modelId)
    {
        var prompt = Build(new[] { "user", "none", "user", "low", "user" });
        var original = prompt.ToArray();

        var (body, warnings) = await Request(method, prompt, new JsonObject { ["reasoningEffort"] = "low" }, modelId);

        OpenAIUpstream.Equal(
            body["input"],
            "[" + WireUser + "," + WireUpdate("none") + "," + WireUser + "," + WireUpdate("low") + "," + WireUser + "]");
        OpenAIUpstream.Equal(body["reasoning"], "{\"effort\":\"low\"}");
        Assert.Empty(warnings);
        Assert.Equal(original, prompt);
    }

    [Theory]
    [MemberData(nameof(MethodAndModel))]
    [UpstreamTest(Prefix + "prepends a request-level none update", Coverage = UpstreamCoverage.Covered)]
    public async Task Prepends_a_request_level_none_update(string method, string modelId)
    {
        var (body, warnings) = await Request(
            method,
            new ModelMessage[] { User() },
            new JsonObject { ["reasoningEffort"] = "low", ["reasoningEffortUpdate"] = "none" },
            modelId);

        OpenAIUpstream.Equal(body["input"], "[" + WireUpdate("none") + "," + WireUser + "]");
        Assert.Equal("low", body["reasoning"]!["effort"]!.GetValue<string>());
        Assert.Empty(warnings);
    }

    [Theory]
    [MemberData(nameof(MethodAndModel))]
    [UpstreamTest(Prefix + "deduplicates matching request-level and first positioned none updates", Coverage = UpstreamCoverage.Covered)]
    public async Task Deduplicates_matching_request_level_and_first_positioned_none_updates(string method, string modelId)
    {
        var (body, warnings) = await Request(
            method,
            Build(new[] { "none", "user" }),
            new JsonObject { ["reasoningEffortUpdate"] = "none" },
            modelId);

        OpenAIUpstream.Equal(body["input"], "[" + WireUpdate("none") + "," + WireUser + "]");
        Assert.Empty(warnings);
    }

    private static string WireUpdate(string effort) =>
        "{\"type\":\"configuration_update\",\"reasoning\":{\"effort\":\"" + effort + "\"}}";

    private static UserModelMessage User() => new("Question");

    private static SystemModelMessage Update(string effort) =>
        new(string.Empty, new Dictionary<string, JsonElement>
        {
            ["openai"] = OpenAIUpstream.Json("{\"reasoningEffortUpdate\":\"" + effort + "\"}"),
        });

    private static List<ModelMessage> Build(IEnumerable<string> kinds)
    {
        var prompt = new List<ModelMessage>();
        foreach (var kind in kinds)
        {
            prompt.Add(kind == "user" ? User() : Update(kind));
        }

        return prompt;
    }

    private static async Task<(JsonNode Body, IReadOnlyList<CallWarning> Warnings)> Request(
        string method,
        IReadOnlyList<ModelMessage> prompt,
        JsonObject options,
        string modelId,
        double? temperature = null,
        double? topP = null)
    {
        const string response = "{\"id\":\"resp_test\",\"created_at\":0,\"model\":\"m\",\"output\":[]}";
        var capture = new OpenAICapture();
        if (method == "generate")
        {
            capture.ResponseJson = response;
        }
        else
        {
            capture.ServerSentEvents = "data: {\"type\":\"response.completed\",\"response\":" + response + "}\n\ndata: [DONE]\n\n";
        }

        var callOptions = new LanguageModelCallOptions
        {
            Prompt = prompt,
            Temperature = temperature,
            TopP = topP,
            ProviderOptions = new Dictionary<string, JsonElement> { ["openai"] = OpenAIUpstream.Json(options.ToJsonString()) },
        };
        var model = OpenAIUpstream.Provider(capture).ResponsesModel(modelId);
        IReadOnlyList<CallWarning> warnings;
        if (method == "generate")
        {
            warnings = (await model.DoGenerateAsync(callOptions, CancellationToken.None)).Warnings;
        }
        else
        {
            var parts = new List<LanguageModelStreamPart>();
            await foreach (var part in model.DoStreamAsync(callOptions, CancellationToken.None))
            {
                parts.Add(part);
            }

            Assert.Empty(parts.OfType<ErrorStreamPart>());
            warnings = parts.OfType<StreamStartStreamPart>().First().Warnings;
        }

        var body = JsonNode.Parse(capture.Body)!;
        Assert.Equal(method == "stream" ? true : (bool?)null, body["stream"]?.GetValue<bool>());
        return (body, warnings);
    }
}
