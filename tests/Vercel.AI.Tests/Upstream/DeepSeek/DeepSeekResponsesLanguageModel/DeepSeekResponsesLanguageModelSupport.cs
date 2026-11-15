// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.DeepSeek;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using DeepSeekResponsesLanguageModelType = Vercel.AI.DeepSeek.DeepSeekResponsesLanguageModel;
using XaiSupport = Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel.XaiResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekResponsesLanguageModel;

/// <summary>Fixtures and helpers shared by the <c>deepseek-responses-language-model.test.ts</c> ports.</summary>
internal static class DeepSeekResponsesLanguageModelSupport
{
    internal const string TestFile = "packages/deepseek/src/responses/deepseek-responses-language-model.test.ts::DeepSeekResponsesLanguageModel";

    /// <summary>Upstream commit that added the tests. The parity catalog is older.</summary>
    internal const string Commit = "f9d43bdaa78a0564105942c7a6709469a120c3ee";

    /// <summary>The <c>webSearchCall</c> item the upstream tests replay.</summary>
    internal const string WebSearchCall = """{"type":"web_search_call","id":"ws_1","status":"completed","action":{"type":"search","queries":["python official website","ws_call_id=ws_1"]}}""";

    /// <summary>Creates <c>provider.responses(modelId)</c> for a provider that talks to <paramref name="capture"/>.</summary>
    internal static DeepSeekResponsesLanguageModelType Model(UpstreamCapture capture, string modelId = "deepseek-flash")
    {
        var provider = DeepSeekProvider.Create(
            new OpenAICompatibleOptions { ApiKey = "test-api-key" },
            capture);
        return provider.ResponsesModel(modelId);
    }

    /// <summary>Serializes like <c>JSON.stringify</c>, which does not escape quotes in strings as <c>\u0022</c>.</summary>
    private static readonly JsonSerializerOptions JsOptions = new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    /// <summary>The <c>TEST_PROMPT</c> of the upstream file.</summary>
    internal static LanguageModelCallOptions TestPrompt()
    {
        return UpstreamChat.Prompt("Hello");
    }

    /// <summary>The <c>REASONING_PROMPT</c> of the upstream file.</summary>
    internal static LanguageModelCallOptions ReasoningPrompt()
    {
        return UpstreamChat.Prompt("Why is the sky blue? Answer in one sentence.");
    }

    /// <summary>The <c>webSearchTool</c> of the upstream file.</summary>
    internal static Tool WebSearchTool()
    {
        return DeepSeekTools.WebSearch();
    }

    internal static LanguageModelTool WebSearchLanguageModelTool()
    {
        return LanguageModelTool.ProviderDefined("deepseek.web_search", "web_search", UpstreamChat.Json("{}"));
    }

    /// <summary>The upstream <c>prepareJsonResponse</c>: a completed response with <paramref name="overrides"/> merged in.</summary>
    internal static UpstreamCapture JsonResponse(string overrides = "{}")
    {
        var body = JsonNode.Parse("""
            {"id":"resp_1","object":"response","created_at":1793084842,"model":"deepseek-flash","status":"completed","output":[],
             "usage":{"input_tokens":1,"output_tokens":1,"total_tokens":2}}
            """)!.AsObject();
        foreach (var pair in JsonNode.Parse(overrides)!.AsObject().ToList())
        {
            body.Remove(pair.Key);
            body[pair.Key] = pair.Value?.DeepClone();
        }

        return new UpstreamCapture { ResponseBody = body.ToJsonString(JsOptions) };
    }

    /// <summary>The upstream <c>prepareJsonFixtureResponse</c>. Like <c>JSON.parse</c> and the test server, it sends the minified JSON.</summary>
    internal static UpstreamCapture JsonFixture(string name)
    {
        var json = JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", name + ".json")))!;
        return new UpstreamCapture { ResponseBody = json.ToJsonString(JsOptions) };
    }

    /// <summary>A snapshot stored next to the fixtures. The upstream inline snapshot, with unset fields left out.</summary>
    internal static JsonNode Snapshot(string fileName)
    {
        return JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName)))!;
    }

    /// <summary>
    /// A generate result and its request in the shape of the upstream <c>doGenerate</c> result:
    /// content, finish reason, provider metadata, request body, response, usage, and warnings.
    /// </summary>
    internal static JsonObject GenerateSnapshot(LanguageModelGenerateResult result, UpstreamCapture capture)
    {
        var finishReason = new JsonObject { ["unified"] = XaiSupport.Unified(result.FinishReason) };
        if (result.RawFinishReason != null)
        {
            finishReason["raw"] = result.RawFinishReason;
        }

        // The test server reports the content type without the charset that the .NET handler adds.
        var headers = new JsonObject();
        foreach (var header in result.ResponseHeaders.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            headers[header.Key.ToLowerInvariant()] = header.Value.Replace("; charset=utf-8", string.Empty);
        }

        var response = new JsonObject
        {
            ["body"] = JsonNode.Parse(result.RawResponse!),
            ["headers"] = headers,
            ["id"] = result.ResponseId,
            ["modelId"] = result.ResponseModelId,
            ["timestamp"] = result.ResponseTimestamp!.Value.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", System.Globalization.CultureInfo.InvariantCulture),
        };
        var warnings = new JsonArray();
        foreach (var warning in result.Warnings)
        {
            warnings.Add(new JsonObject { ["type"] = warning.Type, ["feature"] = warning.Feature });
        }

        return new JsonObject
        {
            ["content"] = XaiSupport.ToJson(result.Content),
            ["finishReason"] = finishReason,
            ["providerMetadata"] = JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText()),
            ["request"] = new JsonObject { ["body"] = RequestBody(capture) },
            ["response"] = response,
            ["usage"] = XaiSupport.Usage(result.Usage),
            ["warnings"] = warnings,
        };
    }

    /// <summary>The upstream <c>prepareChunksFixtureResponse</c>: replays a recorded <c>.chunks.txt</c> fixture.</summary>
    internal static UpstreamCapture ChunksFixture(string name)
    {
        var lines = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "Fixtures", name + ".chunks.txt")).Where(line => line.Trim().Length > 0);
        return StreamResponse(lines.ToArray());
    }

    /// <summary>A server-sent-event response. Each argument is the JSON of one event.</summary>
    internal static UpstreamCapture StreamResponse(params string[] events)
    {
        return new UpstreamCapture { MediaType = "text/event-stream", ResponseBody = UpstreamChat.Sse(events) };
    }

    /// <summary>The JSON of a request body, like <c>server.calls[0].requestBodyJson</c>.</summary>
    internal static JsonObject RequestBody(UpstreamCapture capture)
    {
        return UpstreamChat.Body(capture);
    }
}
