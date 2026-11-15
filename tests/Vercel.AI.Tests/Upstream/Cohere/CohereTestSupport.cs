// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Cohere;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;

namespace Vercel.AI.Tests.Upstream.Cohere;

/// <summary>Helpers shared by the ported Cohere chat tests: the upstream <c>TEST_PROMPT</c>, a model, and mocked responses.</summary>
internal static class CohereTestSupport
{
    public static LanguageModelCallOptions TestPrompt()
    {
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new SystemModelMessage("you are a friendly bot!"), new UserModelMessage("Hello") },
        };
    }

    public static ILanguageModel Model(ParityHandler handler, string modelId = "command-r-plus", Func<string>? generateId = null)
    {
        var options = new CohereOptions { ApiKey = "test-api-key", GenerateId = generateId };
        return CohereProvider.Create(options, handler).LanguageModel(modelId);
    }

    public static ParityHandler JsonBody(string json)
    {
        return new ParityHandler(_ => ParityHandler.Json(json));
    }

    public static string Fixture(string name)
    {
        return File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", name));
    }

    public static ParityHandler ChunksFixture(string name)
    {
        return ChunkLines(File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "Fixtures", name + ".chunks.txt")).Where(line => line.Trim().Length > 0).ToArray());
    }

    // Each line becomes one SSE event named after its type, like the Cohere API sends them.
    public static ParityHandler ChunkLines(params string[] lines)
    {
        var sse = new StringBuilder();
        foreach (var line in lines)
        {
            sse.Append("event: ").Append(JsonNode.Parse(line)!["type"]!.GetValue<string>()).Append("\ndata: ").Append(line).Append("\n\n");
        }

        var bytes = Encoding.UTF8.GetBytes(sse.ToString());
        return new ParityHandler(_ => ParityHandler.Bytes(bytes, "text/event-stream"));
    }

    public static async Task<List<LanguageModelStreamPart>> Stream(ILanguageModel model, LanguageModelCallOptions options)
    {
        return await UpstreamChat.Read(await model.DoStreamAsync(options, CancellationToken.None));
    }

    public static Func<string> Ids(params string[] ids)
    {
        var next = 0;
        return () => ids[Math.Min(next++, ids.Length - 1)];
    }

    public static JsonElement Json(string json)
    {
        return JsonSerializer.Deserialize<JsonElement>(json);
    }

    public static void AssertSource(LanguageModelStreamPart part, string id, string title, string metadataJson)
    {
        var source = Assert.IsType<DocumentSourceStreamPart>(part);
        Assert.Equal(id, source.Id);
        Assert.Equal("text/plain", source.MediaType);
        Assert.Equal(title, source.Title);
        JsonAssert.Equal(source.ProviderMetadata!.Value, metadataJson);
    }

    public static void AssertSource(GeneratedContent part, string id, string title, string metadataJson)
    {
        var source = Assert.IsType<GeneratedDocumentSource>(part);
        Assert.Equal(id, source.Id);
        Assert.Equal("text/plain", source.MediaType);
        Assert.Equal(title, source.Title);
        JsonAssert.Equal(source.ProviderMetadata!.Value, metadataJson);
    }
}
