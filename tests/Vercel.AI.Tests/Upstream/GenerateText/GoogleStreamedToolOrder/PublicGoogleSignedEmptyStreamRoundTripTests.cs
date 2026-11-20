// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.GoogleStreamedToolOrder;

/// <summary>Port of <c>google-streamed-tool-order.test.ts</c> &gt; <c>public Google signed-empty stream round trip</c>.</summary>
public sealed class PublicGoogleSignedEmptyStreamRoundTripTests
{
    // Synthetic protocol cases; every request is handled by the scripted handler below.
    private const string Signature = "b3BhcXVlLXN5bnRoZXRpYy1zaWduYXR1cmUvKz0=";
    private const string CallSignature = "Y2FsbC1zaWduYXR1cmU=";
    private const string Args =
        "{\"query\":\"example\",\"options\":{\"limit\":0,\"enabled\":false}}";

    private const string Call =
        "{\"functionCall\":{\"id\":\"call-1\",\"name\":\"lookup\",\"args\":"
        + Args
        + "},\"thoughtSignature\":\""
        + CallSignature
        + "\"}";

    private const string NoArgsCall =
        "{\"functionCall\":{\"id\":\"call-2\",\"name\":\"read_theme\"}}";
    private const string A = "{\"text\":\"A\"}";
    private const string B = "{\"text\":\"B\"}";
    private const string R1 = "{\"text\":\"R1\",\"thought\":true}";
    private const string R2 = "{\"text\":\"R2\",\"thought\":true}";
    private const string SignedText = "{\"text\":\"\",\"thoughtSignature\":\"" + Signature + "\"}";
    private const string SignedReasoning =
        "{\"text\":\"\",\"thought\":true,\"thoughtSignature\":\"" + Signature + "\"}";

    private static string Text(string value, string? signature = null) =>
        Part("text", value, signature);

    private static string Reasoning(string value, string? signature = null) =>
        Part("reasoning", value, signature);

    private static string Part(string type, string value, string? signature) =>
        type + ":" + value + "|" + signature;

    private static readonly string Lookup = "tool-call:call-1:lookup:" + Args + "|" + CallSignature;
    private const string ReadTheme = "tool-call:call-2:read_theme:{}|";

    public static IEnumerable<object[]> Cases()
    {
        yield return
        [
            "signed empty text after completed call",
            new[] { new[] { A, Call, SignedText } },
            new[] { Text("A"), Lookup, Text("", Signature) },
            new[] { A, Call, SignedText },
        ];
        yield return
        [
            "separate events and no-args call",
            new[] { new[] { A }, new[] { NoArgsCall }, new[] { SignedText }, new[] { B } },
            new[] { Text("A"), ReadTheme, Text("", Signature), Text("B") },
            new[] { A, NoArgsCall, SignedText, B },
        ];
        yield return
        [
            "signed empty reasoning after completed call",
            new[] { new[] { A, NoArgsCall, SignedReasoning, B } },
            new[] { Text("A"), ReadTheme, Reasoning("", Signature), Text("B") },
            new[] { A, NoArgsCall, SignedReasoning, B },
        ];
        yield return
        [
            "empty text between reasoning parts",
            new[] { new[] { R1 }, new[] { SignedText }, new[] { R2 } },
            new[] { Reasoning("R1"), Text("", Signature), Reasoning("R2") },
            new[] { R1, SignedText, R2 },
        ];
        yield return
        [
            "empty reasoning between reasoning parts",
            new[] { new[] { R1, SignedReasoning, R2 } },
            new[] { Reasoning("R1"), Reasoning("", Signature), Reasoning("R2") },
            new[] { R1, SignedReasoning, R2 },
        ];
        yield return
        [
            "text-only trailing signature normalization",
            new[] { new[] { A }, new[] { SignedText } },
            new[] { Text("A", Signature) },
            new[] { "{\"text\":\"A\",\"thoughtSignature\":\"" + Signature + "\"}" },
        ];
        yield return
        [
            "unsigned empty text remains removed",
            new[] { new[] { A, NoArgsCall, "{\"text\":\"\"}", B } },
            new[] { Text("A"), ReadTheme, Text("B") },
            new[] { A, NoArgsCall, B },
        ];
    }

    private static string? SignatureOf(JsonElement? metadata)
    {
        if (metadata is not { ValueKind: JsonValueKind.Object } root)
        {
            return null;
        }

        foreach (var name in new[] { "google", "vertex", "googleVertex" })
        {
            if (
                root.TryGetProperty(name, out var inner)
                && inner.TryGetProperty("thoughtSignature", out var signature)
            )
            {
                return signature.GetString();
            }
        }

        return null;
    }

    private static string? SignatureOf(IReadOnlyDictionary<string, JsonElement>? options) =>
        options is null ? null : SignatureOf(JsonSerializer.SerializeToElement(options));

    private static string Summarize(object part) =>
        part switch
        {
            GeneratedText text => Text(text.Text, SignatureOf(text.ProviderMetadata)),
            GeneratedReasoning reasoning => Reasoning(
                reasoning.Text,
                SignatureOf(reasoning.ProviderMetadata)
            ),
            GeneratedToolCall call => "tool-call:"
                + call.ToolCallId
                + ":"
                + call.ToolName
                + ":"
                + call.ArgumentsJson
                + "|"
                + SignatureOf(call.ProviderMetadata),
            AssistantTextPart text => Text(text.Text, SignatureOf(text.ProviderOptions)),
            AssistantReasoningPart reasoning => Reasoning(
                reasoning.Text,
                SignatureOf(reasoning.ProviderOptions)
            ),
            AssistantToolCallPart call => Summarize(call.ToolCall),
            _ => part.GetType().Name,
        };

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest(
        "packages/ai/src/generate-text/google-streamed-tool-order.test.ts::public Google signed-empty stream round trip::$name",
        Coverage = UpstreamCoverage.Covered,
        Commit = "fd7e8a75d719314f79528f0c9fda734bb83e68f5"
    )]
    public async Task Round_trips_signed_empty_parts(
        string name,
        string[][] groups,
        string[] expected,
        string[] wire
    )
    {
        Assert.NotEmpty(name);
        var handler = new SequenceHandler(
            GoogleStreamOrderSupport.Sse(
                groups.Select(group => GoogleStreamOrderSupport.Parts(group)).ToList()
            ),
            "{\"candidates\":[{\"content\":{\"role\":\"model\",\"parts\":[{\"text\":\"Done\"}]},\"finishReason\":\"STOP\",\"index\":0}]}"
        );
        var model = GoogleProvider
            .Create(new GoogleOptions { ApiKey = "dummy-key" }, handler)
            .LanguageModel("gemini-2.5-flash");
        var tools = new[]
        {
            Tool.Function(
                "lookup",
                null,
                "{\"type\":\"object\",\"properties\":{\"query\":{\"type\":\"string\"},\"options\":{\"type\":\"object\",\"properties\":{\"limit\":{\"type\":\"number\"},\"enabled\":{\"type\":\"boolean\"}},\"required\":[\"limit\",\"enabled\"]}},\"required\":[\"query\",\"options\"]}",
                null
            ),
            Tool.Function("read_theme", null, "{\"type\":\"object\",\"properties\":{}}", null),
        };
        var client = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));

        var result = client.StreamTextAsync(
            new StreamTextOptions
            {
                Model = model,
                Prompt = "Synthetic preservation check",
                Tools = tools,
            }
        );
        var events = new List<TextStreamPart>();
        await foreach (var part in result.Stream())
        {
            events.Add(part);
            Assert.IsNotType<ErrorPart>(part);
        }

        var step = (await result.Steps).Last();
        Assert.Equal(
            expected,
            step.Content.Select(Summarize).Where(item => !item.StartsWith("Generated"))
        );
        var assistant = Assert.IsType<AssistantModelMessage>(
            step.Response.Messages.First(message => message is AssistantModelMessage)
        );
        Assert.Equal(expected, assistant.Parts.Select(Summarize));

        var signedEvents = events
            .Select(part =>
                part switch
                {
                    TextStartPart e => ("text-start", SignatureOf(e.ProviderMetadata)),
                    TextDeltaPart e => ("text-delta", SignatureOf(e.ProviderMetadata)),
                    TextEndPart e => ("text-end", SignatureOf(e.ProviderMetadata)),
                    ReasoningStartPart e => ("reasoning-start", SignatureOf(e.ProviderMetadata)),
                    ReasoningDeltaPart e => ("reasoning-delta", SignatureOf(e.ProviderMetadata)),
                    _ => ("", null),
                }
            )
            .Where(item => item.Item2 == Signature)
            .ToList();
        var signedExpected = expected.FirstOrDefault(item => item.EndsWith("|" + Signature));
        if (signedExpected != null)
        {
            Assert.NotEmpty(signedEvents);
            var expectedType = signedExpected[..signedExpected.IndexOf(':')];
            Assert.All(signedEvents, item =>
                Assert.StartsWith(expectedType + "-", item.Item1, StringComparison.Ordinal)
            );
        }

        var messages = new List<ModelMessage>
        {
            new UserModelMessage("Synthetic preservation check"),
        };
        messages.AddRange(step.Response.Messages);
        if (step.ToolCalls.Count > 0)
        {
            var results = step
                .ToolCalls.Select(call => new ToolModelMessage(
                    call.ToolCallId,
                    call.ToolName,
                    "{\"done\":true}",
                    false,
                    "json"
                ))
                .ToList();
            messages.Add(results.Count == 1 ? results[0] : new ToolModelMessage(results));
        }

        messages.Add(new UserModelMessage("Continue"));
        await client.GenerateTextAsync(
            new GenerateTextOptions
            {
                Model = model,
                Tools = tools,
                Messages = messages,
            }
        );

        Assert.Equal(2, handler.Bodies.Count);
        var expectedWire = new JsonArray(
            wire.Select(part =>
                {
                    var node = JsonNode.Parse(part)!.AsObject();
                    if (node["functionCall"] is JsonObject call && !call.ContainsKey("args"))
                    {
                        call["args"] = new JsonObject();
                    }

                    return (JsonNode?)node;
                })
                .ToArray()
        );
        var contents = JsonNode.Parse(handler.Bodies[1])!["contents"]!.AsArray();
        var model0 = contents.First(content => content!["role"]!.GetValue<string>() == "model")!;
        Assert.True(
            JsonNode.DeepEquals(expectedWire, model0["parts"]),
            model0["parts"]!.ToJsonString()
        );
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse("{\"role\":\"user\",\"parts\":[{\"text\":\"Continue\"}]}"),
                contents[^1]
            ),
            contents[^1]!.ToJsonString()
        );
    }

    private sealed class SequenceHandler(params string[] responses) : HttpMessageHandler
    {
        public List<string> Bodies { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            Bodies.Add(
                request.Content == null
                    ? string.Empty
                    : await request.Content.ReadAsStringAsync(cancellationToken)
            );
            var index = Bodies.Count - 1;
            var isStream = index == 0;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    responses[Math.Min(index, responses.Length - 1)],
                    Encoding.UTF8,
                    isStream ? "text/event-stream" : "application/json"
                ),
            };
        }
    }
}
