// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.OpenAITextFormatUpdate;

/// <summary>Port of <c>openai-text-format-update.test.ts</c> &gt; <c>OpenAI format updates through %sText</c>.</summary>
public sealed class OpenAIFormatUpdatesThroughTextTests
{
    private const string Prefix = "packages/ai/src/generate-text/openai-text-format-update.test.ts::OpenAI format updates through %sText::";

    private const string Schema = "{\"type\":\"object\",\"properties\":{\"total\":{\"type\":\"number\"}},\"required\":[\"total\"],\"additionalProperties\":false}";

    private const string Format = "{\"type\":\"json_schema\",\"name\":\"crate_count\",\"strict\":true,\"schema\":" + Schema + "}";

    public static TheoryData<string, bool> Cases()
    {
        var data = new TheoryData<string, bool>();
        foreach (var method in new[] { "generate", "stream" })
        {
            data.Add(method, false);
            data.Add(method, true);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest(Prefix + "preserves controls, validates Output, and replays history (store: %s)", Coverage = UpstreamCoverage.Covered, Commit = "5a645e92d2efc4563ae0dd7c6375327f61f4d271")]
    public async Task Preserves_controls_validates_Output_and_replays_history(string method, bool store)
    {
        var server = new SequencedResponsesHandler(method);
        var model = OpenAIProvider.Create(new OpenAIOptions { ApiKey = "test" }, server).ResponsesModel("gpt-6-astra");
        var storeJson = store ? "true" : "false";
        var providerOptions = new Dictionary<string, JsonElement>
        {
            ["openai"] = OpenAIUpstream.Json("{\"store\":" + storeJson + ",\"textFormat\":{\"type\":\"text\"}}"),
        };
        var messages = new List<ModelMessage> { new UserModelMessage("There are 17 red and 26 blue crates.") };

        StepResponse firstResponse;
        IReadOnlyList<ModelMessage> firstMessages;
        {
            var options = new StreamTextOptions { Model = model, AllowSystemInMessages = true, Messages = messages.ToArray(), ProviderOptions = providerOptions, MaxRetries = 0 };
            (firstResponse, firstMessages, _, _, _) = await Run(method, options);
        }

        messages.AddRange(firstMessages);
        messages.Add(Update("{\"textFormatUpdate\":" + Format + "}"));
        messages.Add(new UserModelMessage("Count all the crates."));

        var secondOptions = new Dictionary<string, JsonElement>
        {
            ["openai"] = OpenAIUpstream.Json(
                "{\"store\":" + storeJson + ",\"textFormat\":{\"type\":\"text\"},\"promptCacheOptions\":{\"comparisonResponseId\":\"" + firstResponse.Id + "\"}}"),
        };
        var (_, secondMessages, _, secondOutput, secondMetadata) = await Run(
            method,
            new StreamTextOptions
            {
                Model = model,
                AllowSystemInMessages = true,
                Messages = messages.ToArray(),
                Output = OutputSpec.Object(Schema),
                ProviderOptions = secondOptions,
                MaxRetries = 0,
            });
        OpenAIUpstream.Equal(JsonNode.Parse(secondOutput!.Value.GetRawText()), "{\"total\":43}");
        OpenAIUpstream.Equal(
            JsonNode.Parse(secondMetadata!.Value.GetProperty("openai").GetProperty("promptCacheDiagnostics").GetRawText()),
            "{\"type\":\"cache_hit\"}");

        messages.AddRange(secondMessages);
        messages.Add(Update("{\"textFormatUpdate\":{\"type\":\"text\"}}"));
        messages.Add(new UserModelMessage("Say it in plain text."));
        var (_, _, thirdText, _, _) = await Run(
            method,
            new StreamTextOptions { Model = model, AllowSystemInMessages = true, Messages = messages.ToArray(), ProviderOptions = providerOptions, MaxRetries = 0 });
        Assert.Equal("There are 43 crates.", thirdText);

        var bodies = server.Bodies.Select(body => JsonNode.Parse(body)!).ToList();
        Assert.Equal(3, bodies.Count);
        foreach (var body in bodies)
        {
            OpenAIUpstream.Equal(body["text"]!["format"], "{\"type\":\"text\"}");
        }

        OpenAIUpstream.Equal(
            bodies[1]["input"]![2],
            "{\"type\":\"configuration_update\",\"text\":{\"format\":" + Format + "}}");
        var secondInput = bodies[1]["input"]!.AsArray();
        var thirdInput = bodies[2]["input"]!.AsArray();
        for (var i = 0; i < secondInput.Count; i++)
        {
            Assert.True(JsonNode.DeepEquals(secondInput[i], thirdInput[i]));
        }

        OpenAIUpstream.Equal(
            thirdInput[5],
            "{\"type\":\"configuration_update\",\"text\":{\"format\":{\"type\":\"text\"}}}");
        OpenAIUpstream.Equal(bodies[1]["prompt_cache_options"], "{\"comparison_response_id\":\"resp_0\"}");
    }

    private static SystemModelMessage Update(string options) =>
        new(string.Empty, new Dictionary<string, JsonElement> { ["openai"] = OpenAIUpstream.Json(options) });

    private static async Task<(StepResponse Response, IReadOnlyList<ModelMessage> Messages, string Text, JsonElement? Output, JsonElement? ProviderMetadata)> Run(string method, StreamTextOptions options)
    {
        var client = new AiClient(Vercel.AI.Gateway.GatewayProvider.Create(new Vercel.AI.Gateway.GatewayOptions { ApiKey = "test" }));
        if (method == "generate")
        {
            var result = await client.GenerateTextAsync(options);
            return (result.Response, result.ResponseMessages, result.Text, result.Output, result.ProviderMetadata);
        }

        var stream = client.StreamTextAsync(options);
        await stream.ConsumeStreamAsync();
        return (await stream.Response, await stream.ResponseMessages, await stream.Text, await stream.Output, await stream.ProviderMetadata);
    }

    /// <summary>Answers the n-th call like upstream's <c>server.urls[url].response</c> and keeps every request body.</summary>
    private sealed class SequencedResponsesHandler(string method) : HttpMessageHandler
    {
        public List<string> Bodies { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var callNumber = Bodies.Count;
            Bodies.Add(await request.Content!.ReadAsStringAsync(cancellationToken));
            var item = new JsonObject
            {
                ["id"] = $"msg_{callNumber}",
                ["type"] = "message",
                ["role"] = "assistant",
                ["phase"] = "final_answer",
                ["content"] = new JsonArray(new JsonObject
                {
                    ["type"] = "output_text",
                    ["text"] = callNumber == 1 ? "{\"total\":43}" : "There are 43 crates.",
                    ["annotations"] = new JsonArray(),
                }),
            };
            JsonObject Response() => new()
            {
                ["id"] = $"resp_{callNumber}",
                ["created_at"] = 0,
                ["model"] = "gpt-6-astra",
                ["output"] = new JsonArray(item.DeepClone()),
                ["usage"] = new JsonObject { ["input_tokens"] = 20, ["output_tokens"] = 10 },
                ["prompt_cache_diagnostics"] = new JsonObject { ["type"] = "cache_hit" },
            };

            if (method == "generate")
            {
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(Response().ToJsonString(), Encoding.UTF8, "application/json") };
            }

            var text = item["content"]![0]!["text"]!.GetValue<string>();
            var events = new JsonObject[]
            {
                new() { ["type"] = "response.created", ["response"] = Response() },
                new() { ["type"] = "response.output_item.added", ["output_index"] = 0, ["item"] = item.DeepClone() },
                new() { ["type"] = "response.output_text.delta", ["item_id"] = item["id"]!.GetValue<string>(), ["output_index"] = 0, ["content_index"] = 0, ["delta"] = text },
                new() { ["type"] = "response.output_item.done", ["output_index"] = 0, ["item"] = item.DeepClone() },
                new() { ["type"] = "response.completed", ["response"] = Response() },
            };
            var body = string.Concat(events.Select(e => "data: " + e.ToJsonString() + "\n\n"));
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "text/event-stream") };
        }
    }
}
