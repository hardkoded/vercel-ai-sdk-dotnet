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
    internal static LanguageModelGenerateResult Generate(JsonNode body)
    {
        var raw = body.ToJsonString();
        using var document = JsonDocument.Parse(raw);
        var next = 0;
        return OpenAIResponsesLanguageModel.Parse(
            document.RootElement,
            Array.Empty<OpenAICallWarning>(),
            raw,
            new Dictionary<string, string>(),
            () => "id-" + next++);
    }

    internal static JsonNode FixtureBody(string name) => JsonNode.Parse(Fixture(name))!;

    internal static string Sse(IEnumerable<JsonNode> events) =>
        string.Concat(events.Select(@event => "data: " + @event.ToJsonString() + "\n\n"));

    internal static async Task<List<LanguageModelStreamPart>> Stream(string serverSentEvents)
    {
        var capture = new OpenAICapture { ServerSentEvents = serverSentEvents };
        return await OpenAIUpstream.Read(
            OpenAIUpstream.Provider(capture).ResponsesModel("gpt-5-nano").DoStreamAsync(OpenAIUpstream.Hello(), CancellationToken.None)).ConfigureAwait(false);
    }

    internal static JsonNode CitationsJson(IReadOnlyList<Citation>? citations) =>
        CitationJson.ToJson(citations ?? Array.Empty<Citation>());
}
