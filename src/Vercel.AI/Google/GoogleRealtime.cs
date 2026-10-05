// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Vercel.AI.Google;

/// <summary>One normalized Gemini Live event.</summary>
public sealed class GoogleRealtimeEvent
{
    /// <summary>Creates an event.</summary>
    public GoogleRealtimeEvent(string type)
    {
        Type = type;
    }

    /// <summary>Event type.</summary>
    public string Type { get; }

    /// <summary>Response id.</summary>
    public string? ResponseId { get; set; }

    /// <summary>Item id.</summary>
    public string? ItemId { get; set; }

    /// <summary>Tool call id.</summary>
    public string? CallId { get; set; }

    /// <summary>Tool name.</summary>
    public string? Name { get; set; }

    /// <summary>Text, audio, or argument payload.</summary>
    public string? Delta { get; set; }

    /// <summary>Completed tool arguments.</summary>
    public string? Arguments { get; set; }

    /// <summary>Transcript text.</summary>
    public string? Transcript { get; set; }

    /// <summary>Custom raw type.</summary>
    public string? RawType { get; set; }

    /// <summary>Response status.</summary>
    public string? Status { get; set; }

    /// <summary>Provider message this event was parsed from.</summary>
    public JsonElement? Raw { get; set; }
}

/// <summary>Session fields accepted by <see cref="GoogleRealtime"/>.</summary>
public sealed class GoogleRealtimeSession
{
    /// <summary>Model id.</summary>
    public string ModelId { get; set; } = string.Empty;

    /// <summary>System instruction.</summary>
    public string? Instructions { get; set; }

    /// <summary>Voice name.</summary>
    public string? Voice { get; set; }

    /// <summary>Output modalities. Defaults to audio.</summary>
    public IReadOnlyList<string>? OutputModalities { get; set; }

    /// <summary>Function tools.</summary>
    public IReadOnlyList<GoogleRealtimeTool>? Tools { get; set; }

    /// <summary>Provider options. <c>google</c> holds translation and thinking config. Other keys are copied onto setup.</summary>
    public JsonElement? ProviderOptions { get; set; }

    /// <summary>When true, request input transcription.</summary>
    public bool InputAudioTranscription { get; set; }

    /// <summary>When true, request output transcription.</summary>
    public bool OutputAudioTranscription { get; set; }

    /// <summary>Capture rate advertised on input audio blobs.</summary>
    public int? InputAudioRate { get; set; }
}

/// <summary>A realtime function tool.</summary>
public sealed class GoogleRealtimeTool
{
    /// <summary>Creates a tool.</summary>
    public GoogleRealtimeTool(string name, string? description, JsonElement? parameters)
    {
        Name = name;
        Description = description;
        Parameters = parameters;
    }

    /// <summary>Tool name.</summary>
    public string Name { get; }

    /// <summary>Description.</summary>
    public string? Description { get; }

    /// <summary>JSON Schema parameters.</summary>
    public JsonElement? Parameters { get; }
}

/// <summary>
/// Maps Gemini Live WebSocket messages. No socket is opened, so this compiles on netstandard2.0.
/// </summary>
public sealed class GoogleRealtime
{
    private static readonly Regex ThinkingLive = new(@"^gemini-\d+\.\d+-live\b.*thinking", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));

    private int _turn;
    private bool _audio;
    private bool _text;
    private bool _transcript;
    private bool _closed;
    private int _rate = 16000;

    /// <summary>Maps one server message into one or more normalized events.</summary>
    public IReadOnlyList<GoogleRealtimeEvent> ParseServerEvent(JsonElement raw)
    {
        var events = MapServerEvent(raw);
        foreach (var item in events)
        {
            item.Raw = raw.Clone();
        }

        return events;
    }

    private IReadOnlyList<GoogleRealtimeEvent> MapServerEvent(JsonElement raw)
    {
        if (raw.TryGetProperty("setupComplete", out _))
        {
            return new[] { new GoogleRealtimeEvent("session-created") };
        }

        if (raw.TryGetProperty("toolCall", out var toolCall))
        {
            BeginIfClosed();
            var events = new List<GoogleRealtimeEvent>();
            if (toolCall.TryGetProperty("functionCalls", out var calls))
            {
                foreach (var call in calls.EnumerateArray())
                {
                    var args = call.TryGetProperty("args", out var value) ? value.GetRawText() : "{}";
                    var id = GoogleJson.String(call, "id");
                    events.Add(new GoogleRealtimeEvent("function-call-arguments-delta") { ResponseId = ResponseId, ItemId = ItemId, CallId = id, Delta = args });
                    events.Add(new GoogleRealtimeEvent("function-call-arguments-done") { ResponseId = ResponseId, ItemId = ItemId, CallId = id, Name = GoogleJson.String(call, "name"), Arguments = args });
                }
            }

            return events;
        }

        if (raw.TryGetProperty("toolCallCancellation", out _))
        {
            return new[] { new GoogleRealtimeEvent("custom") { RawType = "toolCallCancellation" } };
        }

        if (raw.TryGetProperty("goAway", out _))
        {
            return new[] { new GoogleRealtimeEvent("custom") { RawType = "goAway" } };
        }

        if (raw.TryGetProperty("sessionResumptionUpdate", out _))
        {
            return new[] { new GoogleRealtimeEvent("custom") { RawType = "sessionResumptionUpdate" } };
        }

        if (raw.TryGetProperty("serverContent", out var content))
        {
            return ParseContent(content);
        }

        if (raw.TryGetProperty("inputTranscription", out var input) && GoogleJson.String(input, "text") != null)
        {
            return new[] { new GoogleRealtimeEvent("input-transcription-completed") { ItemId = "google-input-" + _turn, Transcript = GoogleJson.String(input, "text") } };
        }

        string? rawType = null;
        foreach (var property in raw.EnumerateObject())
        {
            rawType = property.Name;
            break;
        }

        return new[] { new GoogleRealtimeEvent("custom") { RawType = rawType ?? "unknown" } };
    }

    /// <summary>Serializes a client event. Returns null for events Gemini Live ignores.</summary>
    public JsonObject? SerializeClient(string type, GoogleRealtimeSession? session, string? audio, string? text, string? callId, string? callName, string? callOutput)
    {
        switch (type)
        {
            case "session-update":
                if (session?.InputAudioRate != null)
                {
                    _rate = session.InputAudioRate.Value;
                }

                return new JsonObject { ["setup"] = BuildSession(session ?? new GoogleRealtimeSession()) };
            case "input-audio-append":
                return new JsonObject { ["realtimeInput"] = new JsonObject { ["audio"] = new JsonObject { ["data"] = audio, ["mimeType"] = "audio/pcm;rate=" + _rate } } };
            case "input-audio-commit":
                return new JsonObject { ["realtimeInput"] = new JsonObject { ["audioStreamEnd"] = true } };
            case "conversation-item-create" when text != null:
                return new JsonObject { ["realtimeInput"] = new JsonObject { ["text"] = text } };
            case "conversation-item-create" when callId != null:
                return new JsonObject
                {
                    ["toolResponse"] = new JsonObject
                    {
                        ["functionResponses"] = new JsonArray
                        {
                            new JsonObject { ["id"] = callId, ["name"] = callName, ["response"] = FunctionResponse(callOutput) },
                        },
                    },
                };
            default:
                return null;
        }
    }

    /// <summary>Builds the Live <c>setup</c> object.</summary>
    public static JsonObject BuildSession(GoogleRealtimeSession session)
    {
        var setup = new JsonObject { ["model"] = GoogleModelPath.Get(session.ModelId) };
        var generation = new JsonObject();
        if (session.OutputModalities is { Count: > 0 })
        {
            var modalities = new JsonArray();
            foreach (var modality in session.OutputModalities)
            {
                modalities.Add(modality.ToUpperInvariant());
            }

            generation["responseModalities"] = modalities;
        }
        else
        {
            generation["responseModalities"] = new JsonArray(JsonValue.Create("AUDIO"));
        }

        if (!string.IsNullOrEmpty(session.Voice))
        {
            generation["speechConfig"] = new JsonObject { ["voiceConfig"] = new JsonObject { ["prebuiltVoiceConfig"] = new JsonObject { ["voiceName"] = session.Voice } } };
        }

        setup["generationConfig"] = generation;
        if (!string.IsNullOrEmpty(session.Instructions))
        {
            setup["systemInstruction"] = new JsonObject { ["parts"] = new JsonArray { new JsonObject { ["text"] = session.Instructions } } };
        }

        JsonElement google = default;
        if (session.ProviderOptions is { ValueKind: JsonValueKind.Object } options)
        {
            foreach (var property in options.EnumerateObject())
            {
                if (property.Name == "google")
                {
                    google = property.Value;
                }
                else if (property.Name == "generationConfig" && property.Value.ValueKind == JsonValueKind.Object)
                {
                    // A raw generationConfig replaces the defaults, matching Object.assign.
                    generation = JsonNode.Parse(property.Value.GetRawText()) as JsonObject ?? new JsonObject();
                    setup["generationConfig"] = generation;
                }
                else if (property.Name != "generationConfig")
                {
                    setup[property.Name] = JsonNode.Parse(property.Value.GetRawText());
                }
            }
        }

        if (session.Tools is { Count: > 0 })
        {
            var declarations = new JsonArray();
            foreach (var tool in session.Tools)
            {
                var declaration = new JsonObject { ["name"] = tool.Name, ["parametersJsonSchema"] = GoogleJson.Clone(tool.Parameters) ?? new JsonObject() };
                if (tool.Description != null)
                {
                    declaration["description"] = tool.Description;
                }
                if (google.ValueKind == JsonValueKind.Object && GoogleJson.String(google, "defaultToolBehavior") is { } behavior)
                {
                    declaration["behavior"] = behavior;
                }

                declarations.Add(declaration);
            }

            setup["tools"] = new JsonArray { new JsonObject { ["functionDeclarations"] = declarations } };
        }

        if (session.InputAudioTranscription)
        {
            setup["inputAudioTranscription"] = new JsonObject();
        }

        if (session.OutputAudioTranscription)
        {
            setup["outputAudioTranscription"] = new JsonObject();
        }

        if (google.ValueKind == JsonValueKind.Object && google.TryGetProperty("translationConfig", out var translation))
        {
            generation = setup["generationConfig"] as JsonObject ?? generation;
            generation["translationConfig"] = JsonNode.Parse(translation.GetRawText());
            setup["generationConfig"] = generation;
        }

        JsonNode? thinking = null;
        if (google.ValueKind == JsonValueKind.Object && google.TryGetProperty("thinkingConfig", out var configured))
        {
            thinking = JsonNode.Parse(configured.GetRawText());
        }
        else if (IsThinkingLive(session.ModelId))
        {
            thinking = new JsonObject { ["thinkingLevel"] = "low" };
        }

        if (thinking != null)
        {
            generation = setup["generationConfig"] as JsonObject ?? generation;
            generation["thinkingConfig"] = thinking;
            setup["generationConfig"] = generation;
        }

        return setup;
    }

    /// <summary>Wraps a non-object tool result in <c>{ output }</c> so the Live API receives a Struct.</summary>
    public static JsonNode FunctionResponse(string? output)
    {
        if (string.IsNullOrWhiteSpace(output))
        {
            return JsonNode.Parse("{\"output\":" + JsonSerializer.Serialize(output) + "}")!;
        }

        try
        {
            var node = JsonNode.Parse(output);
            if (node is JsonObject)
            {
                return node;
            }

            return JsonNode.Parse("{\"output\":" + output + "}")!;
        }
        catch (JsonException)
        {
            return JsonNode.Parse("{\"output\":" + JsonSerializer.Serialize(output) + "}")!;
        }
    }

    private IReadOnlyList<GoogleRealtimeEvent> ParseContent(JsonElement content)
    {
        var events = new List<GoogleRealtimeEvent>();
        if (content.TryGetProperty("interrupted", out var interrupted) && interrupted.ValueKind == JsonValueKind.True)
        {
            events.Add(new GoogleRealtimeEvent("speech-started"));
        }

        if (content.TryGetProperty("modelTurn", out var turn) && turn.TryGetProperty("parts", out var parts))
        {
            BeginIfClosed();
            foreach (var part in parts.EnumerateArray())
            {
                if (part.TryGetProperty("inlineData", out var inline) && GoogleJson.String(inline, "data") is { } audio)
                {
                    _audio = true;
                    events.Add(new GoogleRealtimeEvent("audio-delta") { ResponseId = ResponseId, ItemId = ItemId, Delta = audio });
                }

                if (GoogleJson.String(part, "text") is { } text)
                {
                    _text = true;
                    events.Add(new GoogleRealtimeEvent("text-delta") { ResponseId = ResponseId, ItemId = ItemId, Delta = text });
                }
            }
        }

        if (content.TryGetProperty("outputTranscription", out var output) && GoogleJson.String(output, "text") is { } transcript)
        {
            _transcript = true;
            events.Add(new GoogleRealtimeEvent("audio-transcript-delta") { ResponseId = ResponseId, ItemId = ItemId, Delta = transcript });
        }

        if (content.TryGetProperty("inputTranscription", out var input) && GoogleJson.String(input, "text") is { } inputText)
        {
            events.Add(new GoogleRealtimeEvent("input-transcription-completed") { ItemId = "google-input-" + _turn, Transcript = inputText });
        }

        if (content.TryGetProperty("generationComplete", out var generated) && generated.ValueKind == JsonValueKind.True)
        {
            events.Add(new GoogleRealtimeEvent("custom") { RawType = "generationComplete" });
        }

        if (GoogleJson.String(content, "interactionStatus") != null)
        {
            events.Add(new GoogleRealtimeEvent("custom") { RawType = "interactionStatus" });
        }

        if (content.TryGetProperty("waitingForInput", out var waiting) && waiting.ValueKind == JsonValueKind.True)
        {
            events.Add(new GoogleRealtimeEvent("custom") { RawType = "waitingForInput" });
        }

        if (content.TryGetProperty("turnComplete", out var complete) && complete.ValueKind == JsonValueKind.True)
        {
            if (_audio)
            {
                events.Add(new GoogleRealtimeEvent("audio-done") { ResponseId = ResponseId, ItemId = ItemId });
            }

            if (_text)
            {
                events.Add(new GoogleRealtimeEvent("text-done") { ResponseId = ResponseId, ItemId = ItemId });
            }

            if (_transcript)
            {
                events.Add(new GoogleRealtimeEvent("audio-transcript-done") { ResponseId = ResponseId, ItemId = ItemId });
            }

            events.Add(new GoogleRealtimeEvent("response-done") { ResponseId = ResponseId, Status = "completed" });
            _closed = true;
        }

        if (events.Count == 0)
        {
            events.Add(new GoogleRealtimeEvent("custom") { RawType = "serverContent" });
        }

        return events;
    }

    private void BeginIfClosed()
    {
        if (!_closed)
        {
            return;
        }

        _turn++;
        _audio = false;
        _text = false;
        _transcript = false;
        _closed = false;
    }

    private string ResponseId
    {
        get { return "google-resp-" + _turn; }
    }

    private string ItemId
    {
        get { return "google-item-" + _turn; }
    }

    private static bool IsThinkingLive(string modelId)
    {
        var name = modelId ?? string.Empty;
        var slash = name.LastIndexOf('/');
        if (slash >= 0)
        {
            name = name.Substring(slash + 1);
        }

        return ThinkingLive.IsMatch(name);
    }
}
