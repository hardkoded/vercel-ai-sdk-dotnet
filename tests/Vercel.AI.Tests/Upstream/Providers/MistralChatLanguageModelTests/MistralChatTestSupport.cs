// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Mistral;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.MistralChatLanguageModelTests;

/// <summary>Shared setup of <c>mistral-chat-language-model.test.ts</c>: the provider, the model and the fixtures.</summary>
internal static class MistralChatTestSupport
{
    public const string ChatCompletionsUrl = "https://api.mistral.ai/v1/chat/completions";

    public static string Fixture(string name) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", name));

    public static MistralChatLanguageModel Model(
        HttpMessageHandler handler,
        string modelId = "mistral-small-latest",
        Action<OpenAICompatibleOptions>? configure = null
    )
    {
        var options = new OpenAICompatibleOptions { ApiKey = "test-api-key" };
        configure?.Invoke(options);
        return (MistralChatLanguageModel)
            MistralProvider.Create(options, handler).LanguageModel(modelId);
    }

    public static UpstreamCapture JsonFixture(string name)
    {
        return new UpstreamCapture { ResponseBody = Fixture(name + ".json") };
    }

    /// <summary>Mirrors <c>prepareChunksFixtureResponse</c>: one <c>data:</c> event per fixture line and a final <c>[DONE]</c>.</summary>
    public static SseStreamHandler ChunksFixture(
        string name,
        IReadOnlyDictionary<string, string>? headers = null
    ) => new(SseStreamHandler.FixtureChunks(name, done: true), headers);

    public static SseStreamHandler Chunks(params string[] data) =>
        new(data.Select(item => "data: " + item + "\n\n"));

    public static LanguageModelCallOptions TestPrompt(string text = "Hello") =>
        UpstreamChat.Prompt(text);

    public static JsonNode Body(UpstreamCapture capture) =>
        JsonNode.Parse(capture.Requests[0].Body)!;

    /// <summary>Mirrors <c>mockId()</c>: ids <c>id-0</c>, <c>id-1</c>, and so on.</summary>
    public static Func<string> MockId()
    {
        var counter = -1;
        return () => "id-" + Interlocked.Increment(ref counter);
    }

    /// <summary>One line per stream part, so a test can compare a whole stream at once.</summary>
    public static List<string> Describe(IEnumerable<LanguageModelStreamPart> parts)
    {
        return parts
            .Select(part =>
                part switch
                {
                    StreamStartStreamPart start => "stream-start:" + start.Warnings.Count,
                    ResponseMetadataStreamPart meta => "response-metadata:"
                        + meta.Id
                        + ":"
                        + meta.ModelId
                        + ":"
                        + meta.Timestamp?.UtcDateTime.ToString(
                            "yyyy-MM-ddTHH:mm:ss.fffZ",
                            System.Globalization.CultureInfo.InvariantCulture
                        ),
                    TextStartStreamPart start => "text-start:" + start.Id,
                    TextDeltaStreamPart delta => "text-delta:" + delta.Id + ":" + delta.Delta,
                    TextEndStreamPart end => "text-end:" + end.Id,
                    ReasoningStartStreamPart start => "reasoning-start:" + start.Id,
                    ReasoningDeltaStreamPart delta => "reasoning-delta:"
                        + delta.Id
                        + ":"
                        + delta.Delta,
                    ReasoningEndStreamPart end => "reasoning-end:" + end.Id,
                    ToolInputStartStreamPart start => "tool-input-start:"
                        + start.Id
                        + ":"
                        + start.ToolName,
                    ToolInputDeltaStreamPart delta => "tool-input-delta:"
                        + delta.Id
                        + ":"
                        + delta.Delta,
                    ToolInputEndStreamPart end => "tool-input-end:" + end.Id,
                    ToolCallStreamPart call => "tool-call:"
                        + call.ToolCallId
                        + ":"
                        + call.ToolName
                        + ":"
                        + call.ArgumentsJson,
                    RawStreamPart raw => "raw:" + raw.RawJson,
                    ErrorStreamPart => "error",
                    FinishStreamPart finish => "finish:"
                        + finish.FinishReason
                        + ":"
                        + finish.RawFinishReason
                        + ":"
                        + finish.Usage.InputTokens
                        + "/"
                        + finish.Usage.NoCacheInputTokens
                        + "/"
                        + finish.Usage.CacheReadTokens
                        + ":"
                        + finish.Usage.OutputTokens
                        + "/"
                        + finish.Usage.TextTokens,
                    _ => part.Type,
                }
            )
            .ToList();
    }

    public static LanguageModelTool Tool(string name, string schema) =>
        new(name, null, UpstreamChat.Json(schema));
}
