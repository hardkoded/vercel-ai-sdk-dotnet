---
title: "GoogleToolResultPart"
description: "One tool result."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleToolResultPart"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleToolResultPart"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleToolResultPart
```


One tool result.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L249)

## Constructors

<a id="Vercel_AI_Google_GoogleToolResultPart__ctor_"></a>
<a id="Vercel_AI_Google_GoogleToolResultPart__ctor_System_String_"></a>

### GoogleToolResultPart(string)


```csharp
public GoogleToolResultPart(string toolName)
```


Creates a tool result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L252)

## Properties

<a id="Vercel_AI_Google_GoogleToolResultPart_Content_"></a>
<a id="Vercel_AI_Google_GoogleToolResultPart_Content"></a>

### Content


```csharp
public IReadOnlyList<GoogleToolContentPart>? Content { get; set; }
```


Multimodal content parts when [OutputKind](#Vercel_AI_Google_GoogleToolResultPart_OutputKind) is `content`.

**Value:** [IReadOnlyList&lt;GoogleToolContentPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L273)

<a id="Vercel_AI_Google_GoogleToolResultPart_DenialReason_"></a>
<a id="Vercel_AI_Google_GoogleToolResultPart_DenialReason"></a>

### DenialReason


```csharp
public string? DenialReason { get; set; }
```


Reason a tool call was denied.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L270)

<a id="Vercel_AI_Google_GoogleToolResultPart_Output_"></a>
<a id="Vercel_AI_Google_GoogleToolResultPart_Output"></a>

### Output


```csharp
public string? Output { get; set; }
```


JSON or text payload.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L267)

<a id="Vercel_AI_Google_GoogleToolResultPart_OutputKind_"></a>
<a id="Vercel_AI_Google_GoogleToolResultPart_OutputKind"></a>

### OutputKind


```csharp
public string OutputKind { get; set; }
```


`json`, `text`, `content`, `error-text`, `error-json`, or `execution-denied`. The last three are sent on `response.error`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L264)

<a id="Vercel_AI_Google_GoogleToolResultPart_ProviderOptions_"></a>
<a id="Vercel_AI_Google_GoogleToolResultPart_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }
```


Provider options keyed by namespace.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L276)

<a id="Vercel_AI_Google_GoogleToolResultPart_ToolCallId_"></a>
<a id="Vercel_AI_Google_GoogleToolResultPart_ToolCallId"></a>

### ToolCallId


```csharp
public string? ToolCallId { get; set; }
```


Matching tool-call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L261)

<a id="Vercel_AI_Google_GoogleToolResultPart_ToolName_"></a>
<a id="Vercel_AI_Google_GoogleToolResultPart_ToolName"></a>

### ToolName


```csharp
public string ToolName { get; }
```


Tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L258)
