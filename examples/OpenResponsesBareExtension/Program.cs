// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI;
using Vercel.AI.OpenResponses;
using Vercel.AI.Provider;

// generate-text/open-responses/bare-extension.ts
#region open-responses-bare-extension
var webSearchExtension = new OpenResponsesExtension
{
    Id = "acme.web_search",
    AllowBareTypes = true,
    BareToolType = "web_search",
    BareItemTypes = ["web_search_call"],
    EncodeTool = context => new JsonObject
    {
        ["name"] = context.Name,
        ["search_context_size"] = context.Args.GetProperty("searchContextSize").GetString(),
    },
    DecodeItem = context =>
    {
        var item = context.Item;
        if (
            item["call_id"] is not JsonValue callId
            || callId.GetValueKind() != JsonValueKind.String
            || item["name"] is not JsonValue name
            || name.GetValueKind() != JsonValueKind.String
            || item["query"] is not JsonValue query
            || query.GetValueKind() != JsonValueKind.String
        )
        {
            return null;
        }

        return
        [
            new GeneratedToolCall(
                callId.GetValue<string>(),
                name.GetValue<string>(),
                new JsonObject { ["query"] = query.GetValue<string>() }.ToJsonString()
            )
            {
                ProviderExecuted = true,
            },
        ];
    },
};

var requestBodies = new List<JsonObject>();

var handler = new MockHandler(request =>
{
    requestBodies.Add((JsonObject)JsonNode.Parse(request)!);
    var firstTurn = requestBodies.Count == 1;

    var output = new JsonArray();
    if (firstTurn)
    {
        output.Add(
            new JsonObject
            {
                ["type"] = "web_search_call",
                ["id"] = "search_1",
                ["status"] = "completed",
                ["call_id"] = "call_1",
                ["name"] = "webSearch",
                ["query"] = "Open Responses bare extension codecs",
                ["opaque_receipt"] = new JsonObject { ["trace_id"] = "trace_1" },
            }
        );
    }

    return new JsonObject
    {
        ["id"] = firstTurn ? "response_1" : "response_2",
        ["object"] = "response",
        ["created_at"] = 0,
        ["status"] = "completed",
        ["model"] = "acme-model",
        ["output"] = output,
        ["usage"] = new JsonObject
        {
            ["input_tokens"] = 1,
            ["output_tokens"] = 1,
            ["total_tokens"] = 2,
        },
    }.ToJsonString();
});

var openResponses = OpenResponsesProvider.Create(
    new OpenResponsesOptions
    {
        Name = "acme",
        BaseUrl = "https://example.com/v1",
        ApiKey = "mock-api-key",
        Extensions = [webSearchExtension],
    },
    handler
);

var webSearch = Tool.ProviderDefined(
    "acme.web_search",
    "webSearch",
    JsonDocument.Parse("""{"searchContextSize":"medium"}""").RootElement,
    JsonDocument
        .Parse(
            """{"type":"object","properties":{"query":{"type":"string"}},"required":["query"]}"""
        )
        .RootElement
);

var messages = new List<ModelMessage>
{
    new UserModelMessage("Find the Open Responses extension documentation."),
};

var first = await Ai.GenerateTextAsync(
    new GenerateTextOptions
    {
        Model = openResponses.LanguageModel("acme-model"),
        Messages = messages,
        Tools = [webSearch],
    }
);
messages.AddRange(first.Response.Messages);

await Ai.GenerateTextAsync(
    new GenerateTextOptions
    {
        Model = openResponses.LanguageModel("acme-model"),
        Messages = messages,
        Tools = [webSearch],
    }
);
#endregion

Console.WriteLine("Encoded bare tool: " + requestBodies[0]["tools"]);
Console.WriteLine(
    "Decoded tool call: "
        + string.Join(
            ", ",
            first.ToolCalls.Select(call => call.ToolName + " " + call.ArgumentsJson)
        )
);
Console.WriteLine("Replayed bare item: " + requestBodies[1]["input"]);

/// <summary>An HTTP handler that answers every request with a JSON body built by a callback.</summary>
internal sealed class MockHandler(Func<string, string> respond) : HttpMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var body = await request.Content!.ReadAsStringAsync(cancellationToken);
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(respond(body), Encoding.UTF8, "application/json"),
        };
    }
}
