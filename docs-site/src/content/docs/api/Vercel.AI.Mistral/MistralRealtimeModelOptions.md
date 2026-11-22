---
title: "MistralRealtimeModelOptions"
description: "Mistral voice agent options. Maps to mistralRealtimeModelOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "MistralRealtimeModelOptions"
  hidden: true
---

<a id="Vercel_AI_Mistral_MistralRealtimeModelOptions"></a>

Namespace: [Vercel.AI.Mistral](/api/Vercel.AI.Mistral/)


```csharp
public sealed class MistralRealtimeModelOptions
```


Mistral voice agent options. Maps to `mistralRealtimeModelOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeModelOptions.cs#L87)

## Properties

<a id="Vercel_AI_Mistral_MistralRealtimeModelOptions_AgentId_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeModelOptions_AgentId"></a>

### AgentId


```csharp
public string? AgentId { get; init; }
```


Saved agent whose configuration is overridden for this session. Maps to `agentId`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeModelOptions.cs#L102)

<a id="Vercel_AI_Mistral_MistralRealtimeModelOptions_AgentVersion_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeModelOptions_AgentVersion"></a>

### AgentVersion


```csharp
public int? AgentVersion { get; init; }
```


Version of the saved agent. Omit to use its latest version. Maps to `agentVersion`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeModelOptions.cs#L105)

<a id="Vercel_AI_Mistral_MistralRealtimeModelOptions_Stt_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeModelOptions_Stt"></a>

### Stt


```csharp
public MistralRealtimeSttOptions? Stt { get; init; }
```


Speech recognition settings. Maps to `stt`.

**Value:** [MistralRealtimeSttOptions](/api/Vercel.AI.Mistral/MistralRealtimeSttOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeModelOptions.cs#L90)

<a id="Vercel_AI_Mistral_MistralRealtimeModelOptions_Tts_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeModelOptions_Tts"></a>

### Tts


```csharp
public MistralRealtimeTtsOptions? Tts { get; init; }
```


Speech synthesis settings. Maps to `tts`.

**Value:** [MistralRealtimeTtsOptions](/api/Vercel.AI.Mistral/MistralRealtimeTtsOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeModelOptions.cs#L93)

<a id="Vercel_AI_Mistral_MistralRealtimeModelOptions_Turn_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeModelOptions_Turn"></a>

### Turn


```csharp
public MistralRealtimeTurnOptions? Turn { get; init; }
```


Interruptions and reminders. Maps to `turn`.

**Value:** [MistralRealtimeTurnOptions](/api/Vercel.AI.Mistral/MistralRealtimeTurnOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeModelOptions.cs#L96)

<a id="Vercel_AI_Mistral_MistralRealtimeModelOptions_WelcomeMessage_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeModelOptions_WelcomeMessage"></a>

### WelcomeMessage


```csharp
public string? WelcomeMessage { get; init; }
```


Text spoken when the session connects. Maps to `welcomeMessage`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeModelOptions.cs#L99)

## Methods

<a id="Vercel_AI_Mistral_MistralRealtimeModelOptions_BuildMistralRealtimeSessionConfig_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeModelOptions_BuildMistralRealtimeSessionConfig_Vercel_AI_Provider_RealtimeModelV4SessionConfig_System_String_System_Boolean_"></a>

### BuildMistralRealtimeSessionConfig(RealtimeModelV4SessionConfig, string?, bool)


```csharp
public static JsonObject BuildMistralRealtimeSessionConfig(RealtimeModelV4SessionConfig config, string? modelId = null, bool update = false)
```


Builds the REST configuration, or with &lt;code class="paramref"&gt;update&lt;/code&gt; the subset accepted by `session.update`.
Maps to `buildMistralRealtimeSessionConfig`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `config` | [RealtimeModelV4SessionConfig](/api/Vercel.AI.Provider/RealtimeModelV4SessionConfig/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `update` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

**Exceptions**

| Type | Condition |
| --- | --- |
| [UnsupportedFunctionalityException](/api/Vercel.AI.Operations/UnsupportedFunctionalityException/) | A setting the managed voice agent cannot honor. |
| [InvalidArgumentException](/api/Vercel.AI.Operations/InvalidArgumentException/) | A provider option is invalid. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeModelOptions.cs#L141)

<a id="Vercel_AI_Mistral_MistralRealtimeModelOptions_Parse_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeModelOptions_Parse_System_Text_Json_JsonElement_"></a>

### Parse(JsonElement)


```csharp
public static MistralRealtimeModelOptions? Parse(JsonElement providerOptions)
```


Parses `providerOptions.mistral` with a strict schema: unknown keys and out-of-range values are rejected.
Maps to `parseProviderOptions` with `mistralRealtimeModelOptions`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerOptions` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [MistralRealtimeModelOptions](./)

**Exceptions**

| Type | Condition |
| --- | --- |
| [InvalidArgumentException](/api/Vercel.AI.Operations/InvalidArgumentException/) | An option has the wrong shape or is out of range. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeModelOptions.cs#L112)
