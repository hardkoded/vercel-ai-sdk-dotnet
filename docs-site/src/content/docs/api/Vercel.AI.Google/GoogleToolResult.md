---
title: "GoogleToolResult"
description: "A provider-executed tool result."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleToolResult"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleToolResult"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleToolResult : GeneratedContent
```


A provider-executed tool result.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GeneratedContent](/api/Vercel.AI.Provider/GeneratedContent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleResponse.cs#L476)

## Constructors

<a id="Vercel_AI_Google_GoogleToolResult__ctor_"></a>
<a id="Vercel_AI_Google_GoogleToolResult__ctor_System_String_System_String_System_String_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### GoogleToolResult(string, string, string, string, JsonElement?)


```csharp
public GoogleToolResult(string toolCallId, string toolName, string outcome, string output, JsonElement? providerMetadata = null)
```


Creates a tool result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `outcome` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `output` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleResponse.cs#L479)

## Properties

<a id="Vercel_AI_Google_GoogleToolResult_Outcome_"></a>
<a id="Vercel_AI_Google_GoogleToolResult_Outcome"></a>

### Outcome


```csharp
public string Outcome { get; }
```


Code-execution outcome, when this result came from code execution.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleResponse.cs#L496)

<a id="Vercel_AI_Google_GoogleToolResult_Output_"></a>
<a id="Vercel_AI_Google_GoogleToolResult_Output"></a>

### Output


```csharp
public string Output { get; }
```


Result payload.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleResponse.cs#L499)

<a id="Vercel_AI_Google_GoogleToolResult_ProviderMetadata_"></a>
<a id="Vercel_AI_Google_GoogleToolResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleResponse.cs#L502)

<a id="Vercel_AI_Google_GoogleToolResult_ToolCallId_"></a>
<a id="Vercel_AI_Google_GoogleToolResult_ToolCallId"></a>

### ToolCallId


```csharp
public string ToolCallId { get; }
```


Matching tool-call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleResponse.cs#L490)

<a id="Vercel_AI_Google_GoogleToolResult_ToolName_"></a>
<a id="Vercel_AI_Google_GoogleToolResult_ToolName"></a>

### ToolName


```csharp
public string ToolName { get; }
```


Tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleResponse.cs#L493)
