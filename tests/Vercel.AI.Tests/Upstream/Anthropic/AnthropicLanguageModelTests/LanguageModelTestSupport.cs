// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>
/// The helpers of <c>anthropic-language-model.test.ts</c>: <c>prepareJsonFixtureResponse</c>, <c>prepareChunksFixtureResponse</c>
/// and the request body of the first server call.
/// </summary>
internal static class LanguageModelTestSupport
{
    /// <summary>Maps <c>prepareJsonFixtureResponse(filename)</c>.</summary>
    public static CaptureHandler JsonFixture(string filename)
    {
        return AnthropicParity.Ok(
            File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", filename + ".json"))
        );
    }

    /// <summary>Maps <c>prepareChunksFixtureResponse(filename)</c>.</summary>
    public static CaptureHandler ChunksFixture(string filename)
    {
        var handler = AnthropicParity.Ok(
            string.Concat(SseStreamHandler.FixtureChunks(filename, true))
        );
        handler.ContentType = "text/event-stream";
        return handler;
    }

    /// <summary>Maps <c>providerOptions: { anthropic: ... }</c>.</summary>
    public static IReadOnlyDictionary<string, JsonElement> Anthropic(string json)
    {
        return new Dictionary<string, JsonElement> { ["anthropic"] = AnthropicParity.Json(json) };
    }

    /// <summary>Maps <c>await server.calls[0].requestBodyJson</c>.</summary>
    public static JsonNode RequestBody(CaptureHandler handler)
    {
        return JsonNode.Parse(handler.Body)!;
    }

    /// <summary>Maps <c>convertReadableStreamToArray(result.stream)</c> for <c>doStream</c>.</summary>
    public static async Task<List<LanguageModelStreamPart>> Stream(
        AnthropicProvider provider,
        string modelId,
        LanguageModelCallOptions options
    )
    {
        var parts = new List<LanguageModelStreamPart>();
        await foreach (
            var part in (
                await provider
                    .LanguageModel(modelId)
                    .DoStreamAsync(options, CancellationToken.None)
                    .ConfigureAwait(false)
            ).ConfigureAwait(false)
        )
        {
            parts.Add(part);
        }

        return parts;
    }

    /// <summary>Maps <c>expect(warnings).toEqual([...])</c> for a feature warning.</summary>
    public static void AssertWarning(
        CallWarning warning,
        string type,
        string feature,
        string details
    )
    {
        Assert.Equal(type, warning.Type);
        Assert.Equal(feature, warning.Feature);
        Assert.Equal(details, warning.Message);
    }

    /// <summary>Maps <c>expect(warnings).toContainEqual({ type, feature, details })</c>.</summary>
    public static void AssertContainsWarning(
        IReadOnlyList<CallWarning> warnings,
        string type,
        string feature,
        string details
    )
    {
        Assert.Contains(warnings, warning =>
            warning.Type == type && warning.Feature == feature && warning.Message == details
        );
    }

    /// <summary>Maps <c>mockId({ prefix })</c>: <c>prefix-0</c>, <c>prefix-1</c>, and so on.</summary>
    public static Func<string> MockId(string prefix)
    {
        var counter = -1;
        return () => prefix + "-" + Interlocked.Increment(ref counter);
    }

    /// <summary>Maps <c>createAnthropic({ apiKey: 'test-api-key', generateId: mockId({ prefix: 'id' }) })</c>.</summary>
    public static AnthropicProvider ProviderWithIds(CaptureHandler handler)
    {
        return AnthropicParity.Client(
            handler,
            new AnthropicOptions { ApiKey = "test-api-key", GenerateId = MockId("id") }
        );
    }

    /// <summary>
    /// Maps the shape the upstream snapshots print for <c>result.content</c>. Properties that are <c>undefined</c> upstream are left out.
    /// </summary>
    public static JsonNode Snapshot(IReadOnlyList<GeneratedContent> content)
    {
        var array = new JsonArray();
        foreach (var part in content)
        {
            array.Add(Snapshot(part));
        }

        return array;
    }

    private static JsonNode Snapshot(GeneratedContent part)
    {
        var node = new JsonObject { ["type"] = part.Type };
        switch (part)
        {
            case GeneratedText text:
                node["text"] = text.Text;
                if (text.Citations != null)
                {
                    var citations = new JsonArray();
                    foreach (var citation in text.Citations)
                    {
                        var entry = new JsonObject();
                        if (citation.StartIndex != null)
                        {
                            entry["startIndex"] = citation.StartIndex;
                        }

                        if (citation.EndIndex != null)
                        {
                            entry["endIndex"] = citation.EndIndex;
                        }

                        if (citation.CitedText != null)
                        {
                            entry["citedText"] = citation.CitedText;
                        }

                        entry["source"] = Snapshot(citation.Source);
                        citations.Add(entry);
                    }

                    node["citations"] = citations;
                }

                Metadata(node, text.ProviderMetadata);
                break;
            case GeneratedDocumentSource document:
                node["sourceType"] = "document";
                node["id"] = document.Id;
                node["mediaType"] = document.MediaType;
                node["title"] = document.Title;
                if (document.Filename != null)
                {
                    node["filename"] = document.Filename;
                }

                Metadata(node, document.ProviderMetadata);
                break;
            case GeneratedSource source:
                node["sourceType"] = "url";
                node["id"] = source.Id;
                node["url"] = source.Url;
                if (source.Title != null)
                {
                    node["title"] = source.Title;
                }

                Metadata(node, source.ProviderMetadata);
                break;
            case GeneratedToolCall call:
                node["toolCallId"] = call.ToolCallId;
                node["toolName"] = call.ToolName;
                node["input"] = call.ArgumentsJson;
                if (call.ProviderExecuted)
                {
                    node["providerExecuted"] = true;
                }

                if (call.Dynamic)
                {
                    node["dynamic"] = true;
                }

                Metadata(node, call.ProviderMetadata);
                break;
            case AnthropicToolResultContent result:
                node["toolCallId"] = result.ToolCallId;
                node["toolName"] = result.ToolName;
                node["result"] = JsonNode.Parse(result.ResultJson);
                if (result.IsError)
                {
                    node["isError"] = true;
                }

                Metadata(node, result.ProviderMetadata);
                break;
            case AnthropicCustomContent custom:
                node["kind"] = custom.Kind;
                Metadata(node, custom.ProviderMetadata);
                break;
            default:
                throw new InvalidOperationException("No snapshot for " + part.GetType().Name + ".");
        }

        return node;
    }

    private static void Metadata(JsonObject node, JsonElement? metadata)
    {
        if (metadata is { } value && value.ValueKind != JsonValueKind.Undefined)
        {
            node["providerMetadata"] = JsonNode.Parse(value.GetRawText());
        }
    }

    /// <summary>Maps <c>expect(actual).toEqual(expected)</c> for JSON.</summary>
    public static void AssertJson(string expected, JsonNode? actual)
    {
        JsonAssert.Equal(actual, expected);
    }
}
