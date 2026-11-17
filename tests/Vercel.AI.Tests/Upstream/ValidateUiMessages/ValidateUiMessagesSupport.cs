// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Ui;

namespace Vercel.AI.Tests.ValidateUiMessages;

/// <summary>Helpers of <c>validate-ui-messages.test.ts</c>. Messages are written as upstream's JSON and read into typed messages.</summary>
internal static class ValidateUiMessagesSupport
{
    internal const string Prefix = "packages/ai/src/ui/validate-ui-messages.test.ts::";

    internal static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement.Clone();

    internal static IReadOnlyList<UIMessage> Messages(string json) =>
        ((JsonArray)JsonNode.Parse(json)!)
            .Select(node => UIMessageJson.FromJson((JsonObject)node!))
            .ToList();

    internal static JsonNode Render(IReadOnlyList<UIMessage> messages) =>
        new JsonArray(
            messages.Select(message => (JsonNode)UIMessageJson.ToJson(message)).ToArray()
        );

    internal static void AssertMessages(string expectedJson, IReadOnlyList<UIMessage> actual)
    {
        var expected = JsonNode.Parse(expectedJson);
        Assert.True(
            JsonNode.DeepEquals(expected, Render(actual)),
            "Expected: " + expected + "\nActual: " + Render(actual)
        );
    }

    internal static Task<IReadOnlyList<UIMessage>> Validate(
        string messagesJson,
        IReadOnlyDictionary<string, Tool>? tools = null,
        JsonNode? metadataSchema = null,
        IReadOnlyDictionary<string, JsonNode>? dataSchemas = null,
        IReadOnlyDictionary<string, Func<JsonElement, Task<JsonElement>>>? refine = null
    ) =>
        Ai.ValidateUIMessagesAsync(
            new ValidateUIMessagesOptions
            {
                Messages = Messages(messagesJson),
                Tools = tools,
                MetadataSchema = metadataSchema,
                DataSchemas = dataSchemas,
                ExperimentalRefineToolInput = refine,
            }
        );

    internal static Task<SafeValidateUIMessagesResult> SafeValidate(
        string messagesJson,
        IReadOnlyDictionary<string, Tool>? tools = null,
        JsonNode? metadataSchema = null,
        IReadOnlyDictionary<string, JsonNode>? dataSchemas = null
    ) =>
        Ai.SafeValidateUIMessagesAsync(
            new ValidateUIMessagesOptions
            {
                Messages = Messages(messagesJson),
                Tools = tools,
                MetadataSchema = metadataSchema,
                DataSchemas = dataSchemas,
            }
        );

    /// <summary>The <c>testTool</c> of the tool parts tests: <c>{ foo: string }</c> in, <c>{ result: string }</c> out.</summary>
    internal static Dictionary<string, Tool> TestTools() =>
        new()
        {
            ["foo"] = new Tool(
                "foo",
                null,
                Json(
                    "{\"type\":\"object\",\"properties\":{\"foo\":{\"type\":\"string\"}},\"required\":[\"foo\"]}"
                ),
                null
            )
            {
                OutputSchema = Json(
                    "{\"type\":\"object\",\"properties\":{\"result\":{\"type\":\"string\"}},\"required\":[\"result\"]}"
                ),
            },
        };

    internal static string UserText(string text = "Hello, world!") =>
        "[{\"id\":\"1\",\"role\":\"user\",\"parts\":[{\"type\":\"text\",\"text\":\""
        + text
        + "\"}]}]";

    /// <summary>A one-message assistant chat with the given parts.</summary>
    internal static string Assistant(string partsJson) =>
        "[{\"id\":\"1\",\"role\":\"assistant\",\"parts\":" + partsJson + "}]";
}
