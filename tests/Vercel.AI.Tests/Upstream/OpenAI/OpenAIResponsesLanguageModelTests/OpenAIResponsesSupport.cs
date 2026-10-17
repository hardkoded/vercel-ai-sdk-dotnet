// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Fixtures and helpers shared by the <c>openai-responses-language-model.test.ts</c> ports.</summary>
internal static class OpenAIResponsesSupport
{
    internal static string Fixture(string name) => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", name));

    // Parses a Responses body with predictable source ids: id-0, id-1, and so on.
    internal static LanguageModelGenerateResult Generate(JsonNode body, IReadOnlyList<LanguageModelTool>? tools = null)
    {
        var raw = body.ToJsonString();
        using var document = JsonDocument.Parse(raw);
        var next = 0;
        return OpenAIResponsesLanguageModel.Parse(
            document.RootElement,
            Array.Empty<OpenAICallWarning>(),
            raw,
            new Dictionary<string, string>(),
            () => "id-" + next++,
            OpenAIParallelToolCall.FunctionToolNames(tools));
    }

    internal static JsonNode FixtureBody(string name) => JsonNode.Parse(Fixture(name))!;

    internal static string Sse(IEnumerable<JsonNode> events) =>
        string.Concat(events.Select(@event => "data: " + @event.ToJsonString() + "\n\n"));

    // The TEST_TOOLS of the upstream test file.
    internal static readonly IReadOnlyList<LanguageModelTool> TestTools = new[]
    {
        new LanguageModelTool(
            "weather",
            null,
            OpenAIUpstream.Json("""{"type":"object","properties":{"location":{"type":"string"}},"required":["location"],"additionalProperties":false}""")),
        new LanguageModelTool(
            "cityAttractions",
            null,
            OpenAIUpstream.Json("""{"type":"object","properties":{"city":{"type":"string"}},"required":["city"],"additionalProperties":false}""")),
    };

    internal static async Task<List<LanguageModelStreamPart>> Stream(string serverSentEvents, IReadOnlyList<LanguageModelTool>? tools = null)
    {
        var capture = new OpenAICapture { ServerSentEvents = serverSentEvents };
        return await OpenAIUpstream.Read(
            (await OpenAIUpstream.Provider(capture).ResponsesModel("gpt-5-nano").DoStreamAsync(WithTools(tools), CancellationToken.None))).ConfigureAwait(false);
    }

    private static LanguageModelCallOptions WithTools(IReadOnlyList<LanguageModelTool>? tools)
    {
        var options = OpenAIUpstream.Hello();
        options.Tools = tools;
        return options;
    }

    internal static JsonNode CitationsJson(IReadOnlyList<Citation>? citations) =>
        CitationJson.ToJson(citations ?? Array.Empty<Citation>());
}
