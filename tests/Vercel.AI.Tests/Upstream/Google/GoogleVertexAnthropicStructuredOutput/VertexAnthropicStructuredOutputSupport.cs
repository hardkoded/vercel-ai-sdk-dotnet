// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleVertexAnthropicStructuredOutput;

/// <summary>Shared helpers of <c>google-vertex-anthropic-structured-output.test.ts</c>.</summary>
internal static class VertexAnthropicStructuredOutputSupport
{
    public const string ReportSchema = "{\"type\":\"object\",\"properties\":{\"overview\":{\"type\":\"object\",\"properties\":{\"goals\":{\"type\":\"string\"},\"participants\":{\"type\":\"string\"}},\"required\":[\"goals\",\"participants\"],\"additionalProperties\":false},\"findings\":{\"type\":\"array\",\"minItems\":1,\"items\":{\"type\":\"object\",\"properties\":{\"title\":{\"type\":\"string\"},\"summary\":{\"type\":\"string\"},\"objectiveIndexes\":{\"type\":\"array\",\"minItems\":1,\"items\":{\"type\":\"number\",\"minimum\":0}},\"fragments\":{\"type\":\"array\",\"items\":{\"type\":\"object\",\"properties\":{\"marker\":{\"type\":\"string\"},\"themeName\":{\"type\":[\"string\",\"null\"]}},\"required\":[\"marker\",\"themeName\"],\"additionalProperties\":false}}},\"required\":[\"title\",\"summary\",\"objectiveIndexes\",\"fragments\"],\"additionalProperties\":false}}},\"required\":[\"overview\",\"findings\"],\"additionalProperties\":false}";

    public const string Report = "{\"overview\":{\"goals\":\"Test timers\",\"participants\":\"Home cooks\"},\"findings\":[{\"title\":\"Clear controls\",\"summary\":\"Timers are easy to start\",\"objectiveIndexes\":[0],\"fragments\":[{\"marker\":\"A\",\"themeName\":null}]}]}";

    public static JsonElement Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    public static LanguageModelCallOptions Options(string? schema, string? mode = null)
    {
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Invent a timer report.") },
        };
        if (schema != null)
        {
            options.JsonResponse = true;
            options.JsonSchema = Parse(schema);
        }

        if (mode != null)
        {
            options.ProviderOptions = GoogleUpstream.ProviderOptions("{\"anthropic\":{\"structuredOutputMode\":\"" + mode + "\"}}");
        }

        return options;
    }

    public static ILanguageModel Model(string modelId, Handler handler, bool? supportsNativeStructuredOutput, bool authToken = false)
    {
        var options = new GoogleVertexAnthropicOptions { Project = "test-project", Location = "us-east5" };
        if (supportsNativeStructuredOutput != null)
        {
            options.SupportsNativeStructuredOutput = supportsNativeStructuredOutput.Value;
        }

        if (authToken)
        {
            options.GenerateAuthToken = _ => Task.FromResult("test-token");
        }

        return GoogleVertexAnthropicProvider.Create(options, handler).LanguageModel(modelId);
    }

    public static async Task<(List<GeneratedContent> Content, IReadOnlyList<CallWarning> Warnings, List<LanguageModelStreamPart> Parts)> Call(ILanguageModel model, string method, LanguageModelCallOptions options)
    {
        if (method == "generate")
        {
            var result = await model.DoGenerateAsync(options, CancellationToken.None).ConfigureAwait(false);
            return (result.Content.ToList(), result.Warnings, new List<LanguageModelStreamPart>());
        }

        var stream = await model.DoStreamAsync(options, CancellationToken.None).ConfigureAwait(false);
        var parts = new List<LanguageModelStreamPart>();
        await foreach (var part in stream.Stream.ConfigureAwait(false))
        {
            Assert.IsNotType<ErrorStreamPart>(part);
            parts.Add(part);
        }

        return (new List<GeneratedContent>(), Array.Empty<CallWarning>(), parts);
    }

    public static string JoinText(IEnumerable<LanguageModelStreamPart> parts)
    {
        return string.Concat(parts.OfType<TextDeltaStreamPart>().Select(part => part.Delta));
    }

    /// <summary>Scripted Messages response: a native text block or a <c>tool_use</c> block, as JSON or SSE.</summary>
    public sealed class Handler : HttpMessageHandler
    {
        private readonly string _method;
        private readonly bool _native;
        private readonly string _value;
        private readonly string _toolName;

        public Handler(string method, bool native, string value, string toolName = "json")
        {
            _method = method;
            _native = native;
            _value = value;
            _toolName = toolName;
        }

        public int Calls { get; private set; }

        public JsonNode Body { get; private set; } = new JsonObject();

        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;

        public string? ErrorBody { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            Body = JsonNode.Parse(await request.Content!.ReadAsStringAsync().ConfigureAwait(false))!;
            Headers.Clear();
            foreach (var header in request.Headers)
            {
                Headers[header.Key] = string.Join(",", header.Value);
            }

            if (request.Headers.Authorization != null)
            {
                Headers["authorization"] = request.Headers.Authorization.ToString();
            }

            if (ErrorBody != null)
            {
                return new HttpResponseMessage(Status) { Content = new StringContent(ErrorBody, Encoding.UTF8, "application/json") };
            }

            return _method == "generate"
                ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(Message(), Encoding.UTF8, "application/json") }
                : new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(Sse(), Encoding.UTF8, "text/event-stream") };
        }

        private static string Compact(JsonNode node) => node.ToJsonString();

        private string StopReason => _native ? "end_turn" : "tool_use";

        private JsonObject Block()
        {
            return _native
                ? new JsonObject { ["type"] = "text", ["text"] = _value }
                : new JsonObject { ["type"] = "tool_use", ["id"] = "tool-1", ["name"] = _toolName, ["input"] = JsonNode.Parse(_value) };
        }

        private string Message()
        {
            return Compact(new JsonObject
            {
                ["id"] = "msg-1",
                ["type"] = "message",
                ["role"] = "assistant",
                ["model"] = "claude-opus-5",
                ["content"] = new JsonArray(Block()),
                ["stop_reason"] = StopReason,
                ["stop_sequence"] = null,
                ["usage"] = new JsonObject { ["input_tokens"] = 1, ["output_tokens"] = 1 },
            });
        }

        private string Sse()
        {
            var text = _native ? _value : Compact(JsonNode.Parse(_value)!);
            var start = Block();
            if (_native)
            {
                start["text"] = string.Empty;
            }
            else
            {
                start["input"] = new JsonObject();
            }

            var events = new List<JsonObject>
            {
                new JsonObject
                {
                    ["type"] = "message_start",
                    ["message"] = new JsonObject
                    {
                        ["id"] = "msg-1",
                        ["type"] = "message",
                        ["role"] = "assistant",
                        ["model"] = "claude-opus-5",
                        ["content"] = new JsonArray(),
                        ["stop_reason"] = null,
                        ["stop_sequence"] = null,
                        ["usage"] = new JsonObject { ["input_tokens"] = 1, ["output_tokens"] = 1 },
                    },
                },
                new JsonObject { ["type"] = "content_block_start", ["index"] = 0, ["content_block"] = start },
            };
            foreach (var delta in new[] { string.Empty, text.Substring(0, 13), text.Substring(13) })
            {
                events.Add(new JsonObject
                {
                    ["type"] = "content_block_delta",
                    ["index"] = 0,
                    ["delta"] = _native
                        ? new JsonObject { ["type"] = "text_delta", ["text"] = delta }
                        : new JsonObject { ["type"] = "input_json_delta", ["partial_json"] = delta },
                });
            }

            events.Add(new JsonObject { ["type"] = "content_block_stop", ["index"] = 0 });
            events.Add(new JsonObject
            {
                ["type"] = "message_delta",
                ["delta"] = new JsonObject { ["stop_reason"] = StopReason, ["stop_sequence"] = null },
                ["usage"] = new JsonObject { ["output_tokens"] = 1 },
            });
            events.Add(new JsonObject { ["type"] = "message_stop" });
            return string.Concat(events.Select(item => "data: " + Compact(item) + "\n\n"));
        }
    }
}
