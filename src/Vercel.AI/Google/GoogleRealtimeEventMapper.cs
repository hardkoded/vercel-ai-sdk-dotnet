// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Vercel.AI.Provider;

namespace Vercel.AI.Google;

/// <summary>
/// Stateful event mapper for Google's Gemini Live API. Google's events have no response or item ids and a single
/// message can carry several pieces of data, so this class tracks turn state to generate consistent synthetic ids.
/// No socket is opened, so this compiles on netstandard2.0. Maps to <c>GoogleRealtimeEventMapper</c> and
/// <c>buildGoogleSessionConfig</c> in <c>google-realtime-event-mapper.ts</c>.
/// </summary>
public sealed class GoogleRealtimeEventMapper
{
    private static readonly Regex ThinkingLive = new(
        @"^gemini-[0-9]+\.[0-9]+-live\b.*thinking",
        RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(100)
    );

    private int _turnCounter;
    private int _inputTranscriptionCounter;
    private string _inputTranscriptionBuffer = string.Empty;
    private InputTranscriptionBoundary _inputTranscriptionBoundary =
        InputTranscriptionBoundary.None;
    private bool _preserveInputTranscriptionAcrossTurnComplete;
    private bool _hasAudio;
    private bool _hasText;
    private bool _hasTranscript;
    private bool _turnClosed;
    private int _inputAudioRate = 16000;

    private enum InputTranscriptionBoundary
    {
        None,
        Interrupted,
        TurnComplete,
    }

    private string ResponseId
    {
        get { return "google-resp-" + _turnCounter.ToString(CultureInfo.InvariantCulture); }
    }

    private string ItemId
    {
        get { return "google-item-" + _turnCounter.ToString(CultureInfo.InvariantCulture); }
    }

    /// <summary>
    /// Builds a Google-specific session configuration from a normalized config. Used to construct the
    /// <c>bidiGenerateContentSetup</c> payload for auth token creation. Maps to <c>buildGoogleSessionConfig</c>.
    /// </summary>
    public static JsonObject BuildGoogleSessionConfig(
        RealtimeModelV4SessionConfig? config,
        string modelId
    )
    {
        var setup = new JsonObject { ["model"] = GoogleModelPath.Get(modelId) };

        JsonObject? googleOptions = null;
        var restProviderOptions = new JsonObject();
        if (config?.ProviderOptions is { } providerOptions)
        {
            foreach (var pair in providerOptions)
            {
                if (pair.Key == "google")
                {
                    googleOptions = pair.Value as JsonObject;
                }
                else
                {
                    restProviderOptions[pair.Key] = pair.Value?.DeepClone();
                }
            }
        }

        var generationConfig = new JsonObject();
        if (config?.OutputModalities != null)
        {
            var modalities = new JsonArray();
            foreach (var modality in config.OutputModalities)
            {
                modalities.Add(modality.ToUpperInvariant());
            }

            generationConfig["responseModalities"] = modalities;
        }
        else
        {
            generationConfig["responseModalities"] = new JsonArray("AUDIO");
        }

        if (config?.Voice != null)
        {
            generationConfig["speechConfig"] = new JsonObject
            {
                ["voiceConfig"] = new JsonObject
                {
                    ["prebuiltVoiceConfig"] = new JsonObject { ["voiceName"] = config.Voice },
                },
            };
        }

        setup["generationConfig"] = generationConfig;

        if (config?.Instructions != null)
        {
            setup["systemInstruction"] = new JsonObject
            {
                ["parts"] = new JsonArray(new JsonObject { ["text"] = config.Instructions }),
            };
        }

        if (config?.Tools is { Count: > 0 })
        {
            var defaultToolBehavior = googleOptions?["defaultToolBehavior"];
            var declarations = new JsonArray();
            foreach (var tool in config.Tools)
            {
                var declaration = new JsonObject { ["name"] = tool.Name };
                if (tool.Description != null)
                {
                    declaration["description"] = tool.Description;
                }

                declaration["parametersJsonSchema"] = JsonNode.Parse(tool.Parameters.GetRawText());
                if (defaultToolBehavior != null)
                {
                    declaration["behavior"] = defaultToolBehavior.DeepClone();
                }

                declarations.Add(declaration);
            }

            setup["tools"] = new JsonArray(
                new JsonObject { ["functionDeclarations"] = declarations }
            );
        }

        if (config?.InputAudioTranscription != null)
        {
            setup["inputAudioTranscription"] = new JsonObject();
        }

        if (config?.OutputAudioTranscription != null)
        {
            setup["outputAudioTranscription"] = new JsonObject();
        }

        // Background-reasoning models require a `thinkingLevel` or `thinkingBudget`.
        // Default to the lowest-latency level when the effective thinking config (the
        // typed option, else a raw `providerOptions.generationConfig.thinkingConfig`)
        // sets neither. Merged last so it survives a raw `providerOptions.generationConfig`.
        var typedThinkingConfig = googleOptions?["thinkingConfig"] as JsonObject;
        var explicitThinkingConfig = typedThinkingConfig
            ?? (
                (restProviderOptions["generationConfig"] as JsonObject)?["thinkingConfig"]
                as JsonObject
            );
        JsonObject? thinkingConfig;
        if (
            IsThinkingLiveModel(modelId)
            && explicitThinkingConfig?["thinkingLevel"] == null
            && explicitThinkingConfig?["thinkingBudget"] == null
        )
        {
            thinkingConfig = (JsonObject?)explicitThinkingConfig?.DeepClone() ?? new JsonObject();
            thinkingConfig["thinkingLevel"] = "low";
        }
        else
        {
            thinkingConfig = typedThinkingConfig;
        }

        if (config?.ProviderOptions == null)
        {
            ApplyThinkingConfig(setup, generationConfig, thinkingConfig);
            return setup;
        }

        // Object.assign(setup, restProviderOptions)
        foreach (var pair in restProviderOptions.ToList())
        {
            restProviderOptions.Remove(pair.Key);
            setup[pair.Key] = pair.Value;
        }

        if (googleOptions?["translationConfig"] is { } translationConfig)
        {
            var target = setup["generationConfig"] as JsonObject ?? generationConfig;
            var merged = (JsonObject)target.DeepClone();
            merged["translationConfig"] = translationConfig.DeepClone();
            setup["generationConfig"] = merged;
        }

        ApplyThinkingConfig(setup, generationConfig, thinkingConfig);
        return setup;
    }

    /// <summary>
    /// Maps one Gemini Live server message into normalized events. Only a shared wire frame associates
    /// <c>usageMetadata</c> with a <c>response-done</c>; any other usage snapshot becomes a <c>custom</c> event.
    /// </summary>
    public IReadOnlyList<RealtimeModelV4ServerEvent> ParseServerEvent(JsonElement raw)
    {
        var clone = raw.Clone();
        var events = ParseEvent(clone, clone);
        var usage =
            clone.ValueKind == JsonValueKind.Object
            && clone.TryGetProperty("usageMetadata", out var usageMetadata)
                ? MapGoogleUsage(usageMetadata)
                : null;
        if (usage == null)
        {
            return events;
        }

        // Only a shared wire frame establishes an association with a completion.
        if (events.Any(e => e.Type == "response-done"))
        {
            foreach (var done in events.Where(e => e.Type == "response-done"))
            {
                done.Usage = usage;
            }

            return events;
        }

        var update = new RealtimeModelV4ServerEvent("custom")
        {
            RawType = "usageMetadata",
            Usage = usage,
            Raw = clone,
        };
        if (events.Count == 1 && events[0].Type == "custom" && events[0].RawType == "usageMetadata")
        {
            return [update];
        }

        events.Add(update);
        return events;
    }

    /// <summary>Serializes a normalized client event. Returns null for events Gemini Live ignores.</summary>
    public JsonObject? SerializeClientEvent(RealtimeModelV4ClientEvent clientEvent, string modelId)
    {
        switch (clientEvent.Type)
        {
            case "session-update":
                // Capture the configured capture rate so input audio blobs advertise
                // the real rate. Google accepts any rate as long as the blob's mimeType
                // matches; a mismatched label corrupts custom-rate audio.
                if (clientEvent.Config?.InputAudioFormatRate != null)
                {
                    _inputAudioRate = clientEvent.Config.InputAudioFormatRate.Value;
                }

                return new JsonObject
                {
                    ["setup"] = BuildGoogleSessionConfig(clientEvent.Config, modelId),
                };

            case "input-audio-append":
                return new JsonObject
                {
                    ["realtimeInput"] = new JsonObject
                    {
                        ["audio"] = new JsonObject
                        {
                            ["data"] = clientEvent.Audio,
                            ["mimeType"] =
                                "audio/pcm;rate="
                                + _inputAudioRate.ToString(CultureInfo.InvariantCulture),
                        },
                    },
                };

            case "input-audio-commit":
                return new JsonObject
                {
                    ["realtimeInput"] = new JsonObject { ["audioStreamEnd"] = true },
                };

            case "conversation-item-create":
                switch (clientEvent.Item?.Type)
                {
                    case "text-message":
                        return new JsonObject
                        {
                            ["realtimeInput"] = new JsonObject { ["text"] = clientEvent.Item.Text },
                        };
                    case "function-call-output":
                        return SerializeFunctionCallOutput(clientEvent.Item);
                    default:
                        return null;
                }

            default:
                // input-audio-clear, response-create, response-cancel, conversation-item-truncate
                return null;
        }
    }

    private static void ApplyThinkingConfig(
        JsonObject setup,
        JsonObject generationConfig,
        JsonObject? thinkingConfig
    )
    {
        if (thinkingConfig == null)
        {
            return;
        }

        var target = setup["generationConfig"] as JsonObject ?? generationConfig;
        var merged = (JsonObject)target.DeepClone();
        merged["thinkingConfig"] = thinkingConfig.DeepClone();
        setup["generationConfig"] = merged;
    }

    /// <summary>
    /// Live models that reason in the background (e.g. <c>gemini-3.8-live-extended-thinking</c>). Google requires
    /// exactly one of <c>thinkingLevel</c> / <c>thinkingBudget</c> in their setup and rejects <c>thinkingConfig</c>
    /// on every other Live model.
    /// </summary>
    private static bool IsThinkingLiveModel(string modelId)
    {
        var name = modelId ?? string.Empty;
        var slash = name.LastIndexOf('/');
        if (slash >= 0)
        {
            name = name.Substring(slash + 1);
        }

        return ThinkingLive.IsMatch(name.ToLowerInvariant());
    }

    /// <summary>
    /// Maps Gemini Live <c>usageMetadata</c> to normalized usage. The Live <c>UsageMetadata</c> uses
    /// <c>responseTokenCount</c> / <c>responseTokensDetails</c> for output, not the <c>candidates*</c> names of the
    /// regular <c>generateContent</c> response. Gross totals preserve provider counts even when modality details are
    /// absent. Never guess a text/audio split from those totals.
    /// </summary>
    private static RealtimeModelV4Usage? MapGoogleUsage(JsonElement usageMetadata)
    {
        if (usageMetadata.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var (inputText, inputAudio) = SumByModality(Property(usageMetadata, "promptTokensDetails"));
        var (outputText, outputAudio) = SumByModality(
            Property(usageMetadata, "responseTokensDetails")
        );
        var prompt = Count(usageMetadata, "promptTokenCount");
        var response = Count(usageMetadata, "responseTokenCount");

        var usage = new RealtimeModelV4Usage
        {
            InputTokens =
                prompt != null
                    ? prompt + (Count(usageMetadata, "toolUsePromptTokenCount") ?? 0)
                    : null,
            OutputTokens =
                response != null
                    ? response + (Count(usageMetadata, "thoughtsTokenCount") ?? 0)
                    : null,
            CachedInputTokens = Count(usageMetadata, "cachedContentTokenCount"),
            InputTextTokens = inputText,
            InputAudioTokens = inputAudio,
            OutputTextTokens = outputText,
            OutputAudioTokens = outputAudio,
        };

        // Drop empty fields; return null when nothing is observable.
        return
            usage.InputTokens == null
            && usage.OutputTokens == null
            && usage.CachedInputTokens == null
            && usage.InputTextTokens == null
            && usage.InputAudioTokens == null
            && usage.OutputTextTokens == null
            && usage.OutputAudioTokens == null
            ? null
            : usage;
    }

    /// <summary>Aggregate Gemini per-modality token details into text/audio buckets.</summary>
    private static (int? Text, int? Audio) SumByModality(JsonElement? details)
    {
        int? text = null;
        int? audio = null;
        if (details is not { ValueKind: JsonValueKind.Array } array)
        {
            return (text, audio);
        }

        foreach (var detail in array.EnumerateArray())
        {
            var count = Count(detail, "tokenCount");
            if (count == null)
            {
                continue;
            }

            var modality = Property(detail, "modality")
                is { ValueKind: JsonValueKind.String } modalityValue
                ? modalityValue.GetString()!.ToUpperInvariant()
                : null;
            if (modality == "AUDIO")
            {
                audio = (audio ?? 0) + count;
            }
            else if (modality == "TEXT")
            {
                text = (text ?? 0) + count;
            }
        }

        return (text, audio);
    }

    private static int? Count(JsonElement element, string name)
    {
        if (Property(element, name) is { ValueKind: JsonValueKind.Number } value)
        {
            return (int)value.GetDouble();
        }

        return null;
    }

    private static JsonElement? Property(JsonElement element, string name)
    {
        if (
            element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(name, out var value)
            && value.ValueKind != JsonValueKind.Null
        )
        {
            return value;
        }

        return null;
    }

    private static string? NonEmptyString(JsonElement element, string name)
    {
        if (
            Property(element, name) is { ValueKind: JsonValueKind.String } value
            && value.GetString() is { Length: > 0 } text
        )
        {
            return text;
        }

        return null;
    }

    private static string? OptionalString(JsonElement element, string name)
    {
        return Property(element, name) is { ValueKind: JsonValueKind.String } value
            ? value.GetString()
            : null;
    }

    private static bool IsTrue(JsonElement element, string name)
    {
        return Property(element, name) is { ValueKind: JsonValueKind.True };
    }

    /// <summary>
    /// The value Gemini accepts for <c>functionResponse.response</c>: a <c>google.protobuf.Struct</c>, which is an
    /// object and nothing else. An object is passed through unchanged and everything else is wrapped in
    /// <c>{ output }</c>.
    /// </summary>
    private static JsonObject ToFunctionResponseStruct(JsonNode? value)
    {
        return value is JsonObject obj ? obj : new JsonObject { ["output"] = value };
    }

    private static JsonObject SerializeFunctionCallOutput(RealtimeModelV4ConversationItem item)
    {
        JsonObject response;
        try
        {
            response = ToFunctionResponseStruct(JsonNode.Parse(item.Output ?? string.Empty));
        }
        catch (JsonException)
        {
            // Preserve non-JSON output in the required object wrapper.
            response = new JsonObject { ["output"] = item.Output };
        }

        return new JsonObject
        {
            ["toolResponse"] = new JsonObject
            {
                ["functionResponses"] = new JsonArray(
                    new JsonObject
                    {
                        ["id"] = item.CallId,
                        ["name"] = item.Name,
                        ["response"] = response,
                    }
                ),
            },
        };
    }

    /// <summary>
    /// Rolls over to the next turn lazily, once model content for the next turn arrives. <c>turnComplete</c> merely
    /// marks the current turn closed. This keeps an output transcript that arrives shortly after
    /// <c>turnComplete</c> attached to the turn it belongs to.
    /// </summary>
    private void BeginTurnIfClosed()
    {
        if (!_turnClosed)
        {
            return;
        }

        _turnCounter++;
        _hasAudio = false;
        _hasText = false;
        _hasTranscript = false;
        _turnClosed = false;

        // Once the next response starts, its completion must delimit its own
        // input even if the interrupted response never sent a turnComplete.
        _preserveInputTranscriptionAcrossTurnComplete = false;
    }

    /// <summary>
    /// Google streams input transcription as a sequence of non-accumulating fragments for a single user utterance.
    /// Concatenates consecutive fragments into one running transcript under a stable synthetic id. Google's
    /// <c>finished</c> signal is the authoritative utterance boundary; for older payloads without that signal,
    /// response boundaries remain a fallback. Input transcriptions use a sequence independent of the turn counter.
    /// </summary>
    private (string ItemId, string Transcript)? AccumulateInputTranscription(
        JsonElement inputTranscription
    )
    {
        var text = NonEmptyString(inputTranscription, "text");
        var finishedValue = Property(inputTranscription, "finished");
        var finished = finishedValue is { ValueKind: JsonValueKind.True };

        // Google can send `finished` separately from text. Process it even when
        // text is absent or empty, without emitting an empty user message.
        if (text == null)
        {
            if (finished)
            {
                FinishInputTranscription();
            }

            return null;
        }

        var hasFinishedSignal = finishedValue != null;
        if (
            _inputTranscriptionBuffer.Length > 0
            && (
                _inputTranscriptionBoundary == InputTranscriptionBoundary.Interrupted
                || (
                    _inputTranscriptionBoundary == InputTranscriptionBoundary.TurnComplete
                    && !hasFinishedSignal
                )
            )
        )
        {
            FinishInputTranscription();
        }

        _inputTranscriptionBoundary = InputTranscriptionBoundary.None;
        _inputTranscriptionBuffer += text;
        var result = (
            "google-input-" + _inputTranscriptionCounter.ToString(CultureInfo.InvariantCulture),
            _inputTranscriptionBuffer
        );
        if (finished)
        {
            FinishInputTranscription();
        }

        return result;
    }

    private void FinishInputTranscription()
    {
        if (_inputTranscriptionBuffer.Length == 0)
        {
            return;
        }

        _inputTranscriptionCounter++;
        _inputTranscriptionBuffer = string.Empty;
    }

    private List<RealtimeModelV4ServerEvent> ParseEvent(JsonElement data, JsonElement raw)
    {
        if (Property(data, "setupComplete") != null)
        {
            return [new RealtimeModelV4ServerEvent("session-created") { Raw = raw }];
        }

        if (Property(data, "toolCall") is { } toolCall)
        {
            BeginTurnIfClosed();
            var events = new List<RealtimeModelV4ServerEvent>();
            if (
                Property(toolCall, "functionCalls") is
                { ValueKind: JsonValueKind.Array } functionCalls
            )
            {
                foreach (var functionCall in functionCalls.EnumerateArray())
                {
                    var args = Property(functionCall, "args") is { } value
                        ? value.GetRawText()
                        : "{}";
                    var id = OptionalString(functionCall, "id");
                    events.Add(
                        new RealtimeModelV4ServerEvent("function-call-arguments-delta")
                        {
                            ResponseId = ResponseId,
                            ItemId = ItemId,
                            CallId = id,
                            Delta = args,
                            Raw = raw,
                        }
                    );
                    events.Add(
                        new RealtimeModelV4ServerEvent("function-call-arguments-done")
                        {
                            ResponseId = ResponseId,
                            ItemId = ItemId,
                            CallId = id,
                            Name = OptionalString(functionCall, "name"),
                            Arguments = args,
                            Raw = raw,
                        }
                    );
                }
            }

            return events;
        }

        if (Property(data, "toolCallCancellation") != null)
        {
            return
            [
                new RealtimeModelV4ServerEvent("custom")
                {
                    RawType = "toolCallCancellation",
                    Raw = raw,
                },
            ];
        }

        if (Property(data, "goAway") != null)
        {
            return [new RealtimeModelV4ServerEvent("custom") { RawType = "goAway", Raw = raw }];
        }

        if (Property(data, "sessionResumptionUpdate") != null)
        {
            return
            [
                new RealtimeModelV4ServerEvent("custom")
                {
                    RawType = "sessionResumptionUpdate",
                    Raw = raw,
                },
            ];
        }

        if (Property(data, "serverContent") is { } serverContent)
        {
            return ParseServerContent(serverContent, raw);
        }

        if (
            Property(data, "inputTranscription") is { } inputTranscription
            && AccumulateInputTranscription(inputTranscription) is { } transcription
        )
        {
            return
            [
                new RealtimeModelV4ServerEvent("input-transcription-completed")
                {
                    ItemId = transcription.ItemId,
                    Transcript = transcription.Transcript,
                    Raw = raw,
                },
            ];
        }

        string? rawType = null;
        if (data.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in data.EnumerateObject())
            {
                rawType = property.Name;
                break;
            }
        }

        return
        [
            new RealtimeModelV4ServerEvent("custom")
            {
                RawType = rawType ?? "undefined",
                Raw = raw,
            },
        ];
    }

    private List<RealtimeModelV4ServerEvent> ParseServerContent(
        JsonElement serverContent,
        JsonElement raw
    )
    {
        var events = new List<RealtimeModelV4ServerEvent>();

        if (IsTrue(serverContent, "interrupted"))
        {
            // A barge-in ends the current user utterance; the next input
            // transcription fragment belongs to a fresh utterance. The interrupted
            // response's trailing turnComplete must not split that new utterance.
            _inputTranscriptionBoundary = InputTranscriptionBoundary.Interrupted;
            _preserveInputTranscriptionAcrossTurnComplete = true;
            _turnClosed = true;
            events.Add(new RealtimeModelV4ServerEvent("speech-started") { Raw = raw });
        }

        if (
            Property(serverContent, "modelTurn") is { } modelTurn
            && Property(modelTurn, "parts") is { } parts
        )
        {
            // New model response content marks the start of the next turn.
            BeginTurnIfClosed();
            if (parts.ValueKind == JsonValueKind.Array)
            {
                foreach (var part in parts.EnumerateArray())
                {
                    if (
                        Property(part, "inlineData") is { } inlineData
                        && NonEmptyString(inlineData, "data") is { } data
                    )
                    {
                        _hasAudio = true;
                        events.Add(
                            new RealtimeModelV4ServerEvent("audio-delta")
                            {
                                ResponseId = ResponseId,
                                ItemId = ItemId,
                                Delta = data,
                                Raw = raw,
                            }
                        );
                    }

                    if (NonEmptyString(part, "text") is { } text)
                    {
                        _hasText = true;
                        events.Add(
                            new RealtimeModelV4ServerEvent("text-delta")
                            {
                                ResponseId = ResponseId,
                                ItemId = ItemId,
                                Delta = text,
                                Raw = raw,
                            }
                        );
                    }
                }
            }
        }

        if (
            Property(serverContent, "outputTranscription") is { } outputTranscription
            && NonEmptyString(outputTranscription, "text") is { } outputText
        )
        {
            _hasTranscript = true;
            events.Add(
                new RealtimeModelV4ServerEvent("audio-transcript-delta")
                {
                    ResponseId = ResponseId,
                    ItemId = ItemId,
                    Delta = outputText,
                    Raw = raw,
                }
            );
        }

        if (
            Property(serverContent, "inputTranscription") is { } inputTranscription
            && AccumulateInputTranscription(inputTranscription) is { } transcription
        )
        {
            events.Add(
                new RealtimeModelV4ServerEvent("input-transcription-completed")
                {
                    ItemId = transcription.ItemId,
                    Transcript = transcription.Transcript,
                    Raw = raw,
                }
            );
        }

        // `generationComplete` means generation has stopped, but playback and the
        // turn can remain open. Keep it distinct from `response-done`, which is
        // emitted only when Google sends `turnComplete`.
        if (IsTrue(serverContent, "generationComplete"))
        {
            events.Add(
                new RealtimeModelV4ServerEvent("custom")
                {
                    RawType = "generationComplete",
                    Raw = raw,
                }
            );
        }

        // `interactionStatus` (IN_PROGRESS | IDLE | WAITING_FOR_INPUT) is the definitive
        // session-activity signal for background-reasoning models: `turnComplete`
        // no longer implies the model is idle.
        if (Property(serverContent, "interactionStatus") != null)
        {
            events.Add(
                new RealtimeModelV4ServerEvent("custom")
                {
                    RawType = "interactionStatus",
                    Raw = raw,
                }
            );
        }

        // `waitingForInput` is the always-on Proactive Audio turn-taking signal:
        // the model has yielded the floor and is not generating because it
        // expects the user to continue.
        if (IsTrue(serverContent, "waitingForInput"))
        {
            events.Add(
                new RealtimeModelV4ServerEvent("custom") { RawType = "waitingForInput", Raw = raw }
            );
        }

        if (IsTrue(serverContent, "turnComplete"))
        {
            if (_hasAudio)
            {
                events.Add(
                    new RealtimeModelV4ServerEvent("audio-done")
                    {
                        ResponseId = ResponseId,
                        ItemId = ItemId,
                        Raw = raw,
                    }
                );
            }

            if (_hasText)
            {
                events.Add(
                    new RealtimeModelV4ServerEvent("text-done")
                    {
                        ResponseId = ResponseId,
                        ItemId = ItemId,
                        Raw = raw,
                    }
                );
            }

            if (_hasTranscript)
            {
                events.Add(
                    new RealtimeModelV4ServerEvent("audio-transcript-done")
                    {
                        ResponseId = ResponseId,
                        ItemId = ItemId,
                        Raw = raw,
                    }
                );
            }

            events.Add(
                new RealtimeModelV4ServerEvent("response-done")
                {
                    ResponseId = ResponseId,
                    Status = "completed",
                    Raw = raw,
                }
            );

            // Mark the turn closed but defer advancing the counter until the next
            // response actually begins (see `BeginTurnIfClosed`).
            _turnClosed = true;
            if (_preserveInputTranscriptionAcrossTurnComplete)
            {
                _preserveInputTranscriptionAcrossTurnComplete = false;
            }
            else if (_inputTranscriptionBoundary != InputTranscriptionBoundary.Interrupted)
            {
                // For payloads without `finished`, the completed response remains the
                // fallback signal that the next transcription is a new utterance.
                _inputTranscriptionBoundary = InputTranscriptionBoundary.TurnComplete;
            }
        }

        if (events.Count == 0)
        {
            events.Add(
                new RealtimeModelV4ServerEvent("custom") { RawType = "serverContent", Raw = raw }
            );
        }

        return events;
    }
}
