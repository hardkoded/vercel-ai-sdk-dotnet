---
title: "ToolCallStreamPart"
description: "A completed tool call."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolCallStreamPart"
  hidden: true
---

<a id="Vercel_AI_Provider_ToolCallStreamPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class ToolCallStreamPart : LanguageModelStreamPart
```


A completed tool call.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [LanguageModelStreamPart](/api/Vercel.AI.Provider/LanguageModelStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1704)

## Constructors

<a id="Vercel_AI_Provider_ToolCallStreamPart__ctor_"></a>
<a id="Vercel_AI_Provider_ToolCallStreamPart__ctor_System_String_System_String_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### ToolCallStreamPart(string, string, string, JsonElement?)


```csharp
public ToolCallStreamPart(string toolCallId, string toolName, string argumentsJson, JsonElement? providerMetadata = null)
```


Creates a tool-call part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `argumentsJson` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1707)

## Properties

<a id="Vercel_AI_Provider_ToolCallStreamPart_ArgumentsJson_"></a>
<a id="Vercel_AI_Provider_ToolCallStreamPart_ArgumentsJson"></a>

### ArgumentsJson


```csharp
public string ArgumentsJson { get; }
```


JSON arguments.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1723)

<a id="Vercel_AI_Provider_ToolCallStreamPart_Dynamic_"></a>
<a id="Vercel_AI_Provider_ToolCallStreamPart_Dynamic"></a>

### Dynamic


```csharp
public bool Dynamic { get; init; }
```


True when the tool is not known up front, such as a provider-executed MCP tool. Maps to the call's `dynamic`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1732)

<a id="Vercel_AI_Provider_ToolCallStreamPart_ProviderExecuted_"></a>
<a id="Vercel_AI_Provider_ToolCallStreamPart_ProviderExecuted"></a>

### ProviderExecuted


```csharp
public bool ProviderExecuted { get; init; }
```


True when the provider executed the tool, so the caller has nothing to run.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1729)

<a id="Vercel_AI_Provider_ToolCallStreamPart_ProviderMetadata_"></a>
<a id="Vercel_AI_Provider_ToolCallStreamPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata for this call.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1726)

<a id="Vercel_AI_Provider_ToolCallStreamPart_ToolCallId_"></a>
<a id="Vercel_AI_Provider_ToolCallStreamPart_ToolCallId"></a>

### ToolCallId


```csharp
public string ToolCallId { get; }
```


Tool call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1717)

<a id="Vercel_AI_Provider_ToolCallStreamPart_ToolName_"></a>
<a id="Vercel_AI_Provider_ToolCallStreamPart_ToolName"></a>

### ToolName


```csharp
public string ToolName { get; }
```


Tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1720)
