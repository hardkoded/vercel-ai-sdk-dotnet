// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.OpenAIResponsesPromptCacheDiagnostics;

/// <summary>Port of <c>openai-responses-prompt-cache-diagnostics.test.ts</c> &gt; <c>Responses cache diagnostics (%s)</c>.</summary>
public sealed class ResponsesCacheDiagnosticsTests
{
    private const string Prefix = "packages/openai/src/responses/openai-responses-prompt-cache-diagnostics.test.ts::Responses cache diagnostics (%s)::";

    public static TheoryData<string, string?> Cases()
    {
        var data = new TheoryData<string, string?>();
        foreach (var method in new[] { "generate", "stream" })
        {
            data.Add(method, "{\"type\":\"cache_miss\",\"reason\":\"text_format_changed\",\"cache_missed_tokens\":1200,\"comparison_reusable_tokens\":1500}");
            data.Add(method, "{\"type\":\"cache_hit\"}");
            data.Add(method, "{\"type\":\"comparison_response_not_found\"}");
            data.Add(method, "{\"type\":\"unavailable\"}");
            data.Add(method, "{\"type\":\"future_diagnostic\",\"reason\":\"future_reason\"}");
            data.Add(method, "null");
            data.Add(method, null);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest(Prefix + "forwards comparison requests and returns %j", Coverage = UpstreamCoverage.Covered, Commit = "5a645e92d2efc4563ae0dd7c6375327f61f4d271")]
    public async Task Forwards_comparison_requests_and_returns(string method, string? diagnostics)
    {
        var response = new JsonObject { ["id"] = "resp_test", ["created_at"] = 0, ["model"] = "gpt-6-astra", ["output"] = new JsonArray() };
        if (diagnostics != null)
        {
            response["prompt_cache_diagnostics"] = JsonNode.Parse(diagnostics);
        }

        var capture = new OpenAICapture();
        if (method == "generate")
        {
            capture.ResponseJson = response.ToJsonString();
        }
        else
        {
            capture.ServerSentEvents = "data: {\"type\":\"response.completed\",\"response\":" + response.ToJsonString() + "}\n\ndata: [DONE]\n\n";
        }

        var model = OpenAIUpstream.Provider(capture).ResponsesModel("gpt-6-astra");
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
            ProviderOptions = new Dictionary<string, JsonElement>
            {
                ["openai"] = OpenAIUpstream.Json("{\"promptCacheOptions\":{\"mode\":\"implicit\",\"ttl\":\"12h\",\"prewarm\":false,\"comparisonResponseId\":\"resp_baseline\"}}"),
            },
        };

        JsonElement? metadata;
        if (method == "generate")
        {
            metadata = (await model.DoGenerateAsync(options, CancellationToken.None)).ProviderMetadata;
        }
        else
        {
            var parts = new List<LanguageModelStreamPart>();
            await foreach (var part in (await model.DoStreamAsync(options, CancellationToken.None)))
            {
                parts.Add(part);
            }

            metadata = parts.OfType<FinishStreamPart>().First().ProviderMetadata;
        }

        OpenAIUpstream.Equal(
            JsonNode.Parse(capture.Body)!["prompt_cache_options"],
            "{\"mode\":\"implicit\",\"ttl\":\"12h\",\"prewarm\":false,\"comparison_response_id\":\"resp_baseline\"}");
        var openai = metadata is { ValueKind: JsonValueKind.Object } root && root.TryGetProperty("openai", out var value) ? value : default;
        if (diagnostics == null || diagnostics == "null")
        {
            Assert.False(openai.ValueKind == JsonValueKind.Object && openai.TryGetProperty("promptCacheDiagnostics", out _));
        }
        else
        {
            OpenAIUpstream.Equal(JsonNode.Parse(openai.GetProperty("promptCacheDiagnostics").GetRawText()), diagnostics);
        }
    }
}
