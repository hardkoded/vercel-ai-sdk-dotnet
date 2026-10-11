// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using XaiProviderType = Vercel.AI.Xai.XaiProvider;
using XaiResponsesLanguageModelType = Vercel.AI.Xai.XaiResponsesLanguageModel;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel;

/// <summary>Fixtures and helpers shared by the <c>xai-responses-language-model.test.ts</c> ports.</summary>
internal static class XaiResponsesLanguageModelSupport
{
    internal const string TestFile = "packages/xai/src/responses/xai-responses-language-model.test.ts::XaiResponsesLanguageModel";

    /// <summary>Creates a model whose generated ids are <c>id-0</c>, <c>id-1</c>, and so on, like the upstream <c>mockId</c>.</summary>
    internal static XaiResponsesLanguageModelType Model(UpstreamCapture capture, string modelId = "grok-4-fast-non-reasoning")
    {
        var provider = XaiProviderType.Create(
            new OpenAICompatibleOptions { ApiKey = "test-key", BaseUrl = "https://api.x.ai/v1" },
            capture);
        var counter = 0;
        return new XaiResponsesLanguageModelType(provider, modelId, () => "id-" + counter++);
    }

    internal static LanguageModelCallOptions HelloPrompt()
    {
        return UpstreamChat.Prompt("hello");
    }

    /// <summary>A scripted JSON response with the given extra properties.</summary>
    internal static UpstreamCapture JsonResponse(string json)
    {
        return new UpstreamCapture { ResponseBody = json };
    }

    /// <summary>A completed response with no output and a small usage.</summary>
    internal static UpstreamCapture EmptyResponse(string model = "grok-4-fast-non-reasoning", string extra = "")
    {
        return JsonResponse("{\"id\":\"resp_123\",\"object\":\"response\",\"status\":\"completed\",\"model\":\"" + model + "\",\"output\":[],\"usage\":{\"input_tokens\":10,\"output_tokens\":5}" + extra + "}");
    }

    /// <summary>A server-sent-event response. Each argument is the JSON of one event.</summary>
    internal static UpstreamCapture StreamResponse(params string[] events)
    {
        return new UpstreamCapture { MediaType = "text/event-stream", ResponseBody = UpstreamChat.Sse(events) };
    }

    /// <summary>A server-sent-event response that replays a recorded <c>.chunks.txt</c> fixture.</summary>
    internal static UpstreamCapture ChunksFixture(string name)
    {
        var lines = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "Fixtures", name)).Where(line => line.Trim().Length > 0);
        return StreamResponse(lines.ToArray());
    }

    internal static JsonObject Body(UpstreamCapture capture)
    {
        return UpstreamChat.Body(capture);
    }

    internal static Task<List<LanguageModelStreamPart>> Collect(XaiResponsesLanguageModelType model, LanguageModelCallOptions options)
    {
        return UpstreamChat.Read(model.DoStreamAsync(options, CancellationToken.None));
    }

    internal static LanguageModelTool WeatherTool()
    {
        return new LanguageModelTool(
            "weather",
            "get weather",
            UpstreamChat.Json("{\"type\":\"object\",\"properties\":{\"location\":{\"type\":\"string\"}}}"));
    }

    /// <summary>The first stream part of type <typeparamref name="T"/>.</summary>
    internal static T First<T>(IEnumerable<LanguageModelStreamPart> parts)
        where T : LanguageModelStreamPart
    {
        return parts.OfType<T>().First();
    }

}
