---
title: "AnthropicToolResultContent"
description: "A provider-executed tool result returned inside a Messages response."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicToolResultContent"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicToolResultContent"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicToolResultContent : GeneratedContent
```


A provider-executed tool result returned inside a Messages response.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GeneratedContent](/api/Vercel.AI.Provider/GeneratedContent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicContent.cs#L57)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicToolResultContent__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicToolResultContent__ctor_System_String_System_String_System_String_System_Boolean_System_Nullable_System_Text_Json_JsonElement__"></a>

### AnthropicToolResultContent(string, string, string, bool, JsonElement?)


```csharp
public AnthropicToolResultContent(string toolCallId, string toolName, string resultJson, bool isError, JsonElement? providerMetadata)
```


Creates a tool result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `resultJson` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `isError` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicContent.cs#L60)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicToolResultContent_IsError_"></a>
<a id="Vercel_AI_Anthropic_AnthropicToolResultContent_IsError"></a>

### IsError


```csharp
public bool IsError { get; }
```


Whether the tool failed.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicContent.cs#L80)

<a id="Vercel_AI_Anthropic_AnthropicToolResultContent_ProviderMetadata_"></a>
<a id="Vercel_AI_Anthropic_AnthropicToolResultContent_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata, including caller information.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicContent.cs#L83)

<a id="Vercel_AI_Anthropic_AnthropicToolResultContent_ResultJson_"></a>
<a id="Vercel_AI_Anthropic_AnthropicToolResultContent_ResultJson"></a>

### ResultJson


```csharp
public string ResultJson { get; }
```


JSON result.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicContent.cs#L77)

<a id="Vercel_AI_Anthropic_AnthropicToolResultContent_ToolCallId_"></a>
<a id="Vercel_AI_Anthropic_AnthropicToolResultContent_ToolCallId"></a>

### ToolCallId


```csharp
public string ToolCallId { get; }
```


Matching tool call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicContent.cs#L71)

<a id="Vercel_AI_Anthropic_AnthropicToolResultContent_ToolName_"></a>
<a id="Vercel_AI_Anthropic_AnthropicToolResultContent_ToolName"></a>

### ToolName


```csharp
public string ToolName { get; }
```


Tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicContent.cs#L74)
