// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Ui;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>
/// The scaffolding of <c>process-ui-message-stream.test.ts</c>: <c>createUIMessageStream</c>, the <c>state</c> and <c>writeCalls</c> variables,
/// and the <c>runUpdateMessageJob</c> function.
/// </summary>
internal sealed class ProcessUIMessageStreamHarness
{
    /// <summary>The <c>state</c> variable of the upstream test.</summary>
    public StreamingUIMessageState State { get; private set; } = null!;

    /// <summary>The <c>writeCalls</c> variable of the upstream test. Each entry is <c>{ "message": ... }</c>.</summary>
    public JsonArray WriteCalls { get; } = [];

    /// <summary>Maps to <c>createStreamingUIMessageState</c> in the test.</summary>
    public void CreateState(string messageId, string? lastMessage)
    {
        State = UIMessageStreamProcessing.CreateStreamingUIMessageState(
            lastMessage == null ? null : (JsonObject)JsonNode.Parse(lastMessage)!,
            messageId);
    }

    /// <summary>Maps to <c>runUpdateMessageJob</c> in the test.</summary>
    public async Task RunUpdateMessageJob(Func<UpdateMessageJobContext, Task> job)
    {
        await job(new UpdateMessageJobContext(State, _ => WriteCalls.Add(new JsonObject { ["message"] = State.Message.DeepClone() }))).ConfigureAwait(false);
    }

    /// <summary>Maps to <c>createUIMessageStream</c>.</summary>
    public static async IAsyncEnumerable<JsonObject> CreateUIMessageStream(string chunksJson)
    {
        foreach (var chunk in (JsonArray)JsonNode.Parse(chunksJson)!)
        {
            yield return (JsonObject)chunk!.DeepClone();
            await Task.Yield();
        }
    }

    /// <summary>The options of <c>processUIMessageStream</c> with the test's <c>runUpdateMessageJob</c> and an <c>onError</c> that throws.</summary>
    public ProcessUIMessageStreamOptions Options(Action<Exception>? onError = null)
        => new(RunUpdateMessageJob, onError ?? (error => throw error));

    /// <summary>Maps to <c>consumeStream({ stream: processUIMessageStream({ ... }) })</c>.</summary>
    public async Task ProcessAsync(string chunksJson, ProcessUIMessageStreamOptions? options = null)
    {
        await foreach (var unused in UIMessageStreamProcessing.ProcessUIMessageStream(CreateUIMessageStream(chunksJson), options ?? Options()).ConfigureAwait(false))
        {
        }
    }

    /// <summary>Maps to <c>toMatchInlineSnapshot</c> and <c>toEqual</c> on a JSON value. Object key order does not matter.</summary>
    public static void AssertJson(string expectedJson, JsonNode? actual)
    {
        var expected = JsonNode.Parse(expectedJson);
        Assert.True(
            JsonEquals(expected, actual),
            "Expected:\n" + expected?.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }) + "\nActual:\n" + actual?.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    }

    // The port stores a field that upstream leaves undefined as a JSON null.
    private static bool JsonEquals(JsonNode? expected, JsonNode? actual)
    {
        switch (expected)
        {
            case JsonObject expectedObject when actual is JsonObject actualObject:
                foreach (var pair in expectedObject)
                {
                    if (!actualObject.TryGetPropertyValue(pair.Key, out var actualValue) || !JsonEquals(pair.Value, actualValue))
                    {
                        return false;
                    }
                }

                return actualObject.All(pair => expectedObject.ContainsKey(pair.Key) || pair.Value is null);
            case JsonArray expectedArray when actual is JsonArray actualArray:
                return expectedArray.Count == actualArray.Count && expectedArray.Zip(actualArray, JsonEquals).All(equal => equal);
            default:
                return JsonNode.DeepEquals(expected, actual);
        }
    }

    /// <summary>Maps to <c>state.message</c>.</summary>
    public JsonObject Message => State.Message;

    /// <summary>Maps to <c>state.message.parts</c>.</summary>
    public JsonArray Parts => (JsonArray)State.Message["parts"]!;

    /// <summary>Maps to <c>state.activeTextParts</c>.</summary>
    public JsonObject ActiveTextParts => new(State.ActiveTextParts.Select(pair => new KeyValuePair<string, JsonNode?>(pair.Key, pair.Value.DeepClone())));

    /// <summary>Maps to <c>state.activeReasoningParts</c>.</summary>
    public JsonObject ActiveReasoningParts => new(State.ActiveReasoningParts.Select(pair => new KeyValuePair<string, JsonNode?>(pair.Key, pair.Value.DeepClone())));

    /// <summary>Maps to <c>writeCalls.map(call =&gt; call.message.parts[index])</c>.</summary>
    public JsonArray WriteCallParts(int index)
        => new(WriteCalls.Select(call => ((JsonArray)call!["message"]!["parts"]!)[index]?.DeepClone()).ToArray());

    /// <summary>
    /// Maps to <c>writeCalls.map(call =&gt; call.message.parts.find(isToolUIPart)?.approval).filter(approval =&gt; approval != null)</c>.
    /// </summary>
    public JsonArray WriteCallApprovals()
        => new(WriteCalls
            .Select(call => ((JsonArray)call!["message"]!["parts"]!).FirstOrDefault(part => UIMessageJson.IsToolPart((JsonObject)part!)) is JsonObject tool ? tool["approval"]?.DeepClone() : null)
            .Where(approval => approval != null)
            .ToArray());

    /// <summary>Maps to <c>parts.find(part =&gt; ...)</c>.</summary>
    public static JsonObject? Find(JsonArray parts, Func<JsonObject, bool> predicate)
        => parts.OfType<JsonObject>().FirstOrDefault(predicate);

    /// <summary>Maps to <c>state.message.parts.find(part =&gt; part.toolCallId === toolCallId)</c>.</summary>
    public JsonObject? FindToolPart(string toolCallId)
        => Find(Parts, part => (string?)part["toolCallId"] == toolCallId);

    /// <summary>Maps to the first <c>writeCalls</c> entry whose message has a part that matches <paramref name="predicate"/>.</summary>
    public JsonObject? FindWriteCall(Func<JsonObject, bool> predicate)
        => WriteCalls.OfType<JsonObject>().FirstOrDefault(call => ((JsonArray)call["message"]!["parts"]!).OfType<JsonObject>().Any(predicate));

    /// <summary>Copies the listed fields that are present. Maps to an object literal that picks fields of a part.</summary>
    public static JsonObject PickKeys(JsonObject source, params string[] keys)
    {
        var picked = new JsonObject();
        foreach (var key in keys)
        {
            if (source.TryGetPropertyValue(key, out var value))
            {
                picked[key] = value?.DeepClone();
            }
        }

        return picked;
    }
}
