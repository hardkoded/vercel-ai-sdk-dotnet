---
title: "ToolResultStreamPart"
description: "A tool result the provider reported."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolResultStreamPart"
  hidden: true
---

<a id="Vercel_AI_Provider_ToolResultStreamPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class ToolResultStreamPart : LanguageModelStreamPart
```


A tool result the provider reported.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [LanguageModelStreamPart](/api/Vercel.AI.Provider/LanguageModelStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1852)

## Constructors

<a id="Vercel_AI_Provider_ToolResultStreamPart__ctor_"></a>
<a id="Vercel_AI_Provider_ToolResultStreamPart__ctor_System_String_System_String_System_Text_Json_JsonElement_"></a>

### ToolResultStreamPart(string, string, JsonElement)


```csharp
public ToolResultStreamPart(string toolCallId, string toolName, JsonElement result)
```


Creates a tool-result part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `result` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1855)

## Properties

<a id="Vercel_AI_Provider_ToolResultStreamPart_IsError_"></a>
<a id="Vercel_AI_Provider_ToolResultStreamPart_IsError"></a>

### IsError


```csharp
public bool IsError { get; init; }
```


Whether the tool failed. [Result](#Vercel_AI_Provider_ToolResultStreamPart_Result) then holds the error.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1873)

<a id="Vercel_AI_Provider_ToolResultStreamPart_ProviderMetadata_"></a>
<a id="Vercel_AI_Provider_ToolResultStreamPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; init; }
```


Provider metadata for this result, keyed by provider name.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1876)

<a id="Vercel_AI_Provider_ToolResultStreamPart_Result_"></a>
<a id="Vercel_AI_Provider_ToolResultStreamPart_Result"></a>

### Result


```csharp
public JsonElement Result { get; }
```


Result value.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1870)

<a id="Vercel_AI_Provider_ToolResultStreamPart_ToolCallId_"></a>
<a id="Vercel_AI_Provider_ToolResultStreamPart_ToolCallId"></a>

### ToolCallId


```csharp
public string ToolCallId { get; }
```


Matching tool call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1864)

<a id="Vercel_AI_Provider_ToolResultStreamPart_ToolName_"></a>
<a id="Vercel_AI_Provider_ToolResultStreamPart_ToolName"></a>

### ToolName


```csharp
public string ToolName { get; }
```


Tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1867)
