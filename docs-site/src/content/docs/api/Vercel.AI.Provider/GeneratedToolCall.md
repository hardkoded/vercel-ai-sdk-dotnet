---
title: "GeneratedToolCall"
description: "A function tool call."
editUrl: false
prev: false
next: false
sidebar:
  label: "GeneratedToolCall"
  hidden: true
---

<a id="Vercel_AI_Provider_GeneratedToolCall"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public class GeneratedToolCall : GeneratedContent
```


A function tool call.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GeneratedContent](/api/Vercel.AI.Provider/GeneratedContent/)

**Derived:** [GoogleToolCall](/api/Vercel.AI.Google/GoogleToolCall/), [HuggingFaceToolCall](/api/Vercel.AI.HuggingFace/HuggingFaceToolCall/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L207)

## Constructors

<a id="Vercel_AI_Provider_GeneratedToolCall__ctor_"></a>
<a id="Vercel_AI_Provider_GeneratedToolCall__ctor_System_String_System_String_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### GeneratedToolCall(string, string, string, JsonElement?)


```csharp
public GeneratedToolCall(string toolCallId, string toolName, string argumentsJson, JsonElement? providerMetadata = null)
```


Creates a tool call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `argumentsJson` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L210)

## Properties

<a id="Vercel_AI_Provider_GeneratedToolCall_ArgumentsJson_"></a>
<a id="Vercel_AI_Provider_GeneratedToolCall_ArgumentsJson"></a>

### ArgumentsJson


```csharp
public string ArgumentsJson { get; }
```


JSON object arguments.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L226)

<a id="Vercel_AI_Provider_GeneratedToolCall_Dynamic_"></a>
<a id="Vercel_AI_Provider_GeneratedToolCall_Dynamic"></a>

### Dynamic


```csharp
public bool Dynamic { get; init; }
```


True when the tool is not known up front, such as a provider-executed MCP tool. Maps to the call's `dynamic`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L235)

<a id="Vercel_AI_Provider_GeneratedToolCall_Error_"></a>
<a id="Vercel_AI_Provider_GeneratedToolCall_Error"></a>

### Error


```csharp
public Exception? Error { get; init; }
```


Why the call is [Invalid](#Vercel_AI_Provider_GeneratedToolCall_Invalid). Maps to `error`.

**Value:** [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L244)

<a id="Vercel_AI_Provider_GeneratedToolCall_Invalid_"></a>
<a id="Vercel_AI_Provider_GeneratedToolCall_Invalid"></a>

### Invalid


```csharp
public bool Invalid { get; init; }
```


True when the call failed to parse: its tool is unknown, its input did not parse, or the input did not match the tool's
input schema. Such a call is [Dynamic](#Vercel_AI_Provider_GeneratedToolCall_Dynamic) and is never executed. Maps to `invalid`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L241)

<a id="Vercel_AI_Provider_GeneratedToolCall_ProviderExecuted_"></a>
<a id="Vercel_AI_Provider_GeneratedToolCall_ProviderExecuted"></a>

### ProviderExecuted


```csharp
public bool ProviderExecuted { get; init; }
```


True when the provider executed the tool, so the caller has nothing to run.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L232)

<a id="Vercel_AI_Provider_GeneratedToolCall_ProviderMetadata_"></a>
<a id="Vercel_AI_Provider_GeneratedToolCall_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider-specific metadata for this call, such as a thought signature.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L229)

<a id="Vercel_AI_Provider_GeneratedToolCall_Title_"></a>
<a id="Vercel_AI_Provider_GeneratedToolCall_Title"></a>

### Title


```csharp
public string? Title { get; init; }
```


Display title of the tool, when the tool has one. Maps to `title`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L247)

<a id="Vercel_AI_Provider_GeneratedToolCall_ToolCallId_"></a>
<a id="Vercel_AI_Provider_GeneratedToolCall_ToolCallId"></a>

### ToolCallId


```csharp
public string ToolCallId { get; }
```


Provider id for this call.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L220)

<a id="Vercel_AI_Provider_GeneratedToolCall_ToolMetadata_"></a>
<a id="Vercel_AI_Provider_GeneratedToolCall_ToolMetadata"></a>

### ToolMetadata


```csharp
public IReadOnlyDictionary<string, JsonElement>? ToolMetadata { get; init; }
```


Metadata of the tool, kept apart from [ProviderMetadata](#Vercel_AI_Provider_GeneratedToolCall_ProviderMetadata). Maps to `toolMetadata`.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L250)

<a id="Vercel_AI_Provider_GeneratedToolCall_ToolName_"></a>
<a id="Vercel_AI_Provider_GeneratedToolCall_ToolName"></a>

### ToolName


```csharp
public string ToolName { get; }
```


Tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L223)
