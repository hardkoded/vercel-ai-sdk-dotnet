---
title: "GeneratedToolResult"
description: "A tool result: one the provider produced itself, such as a web or file search, or one the generation loop produced by running a client tool."
editUrl: false
prev: false
next: false
sidebar:
  label: "GeneratedToolResult"
  hidden: true
---

<a id="Vercel_AI_Provider_GeneratedToolResult"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class GeneratedToolResult : GeneratedContent
```


A tool result: one the provider produced itself, such as a web or file search, or one the generation loop produced by running a client tool.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GeneratedContent](/api/Vercel.AI.Provider/GeneratedContent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L485)

## Constructors

<a id="Vercel_AI_Provider_GeneratedToolResult__ctor_"></a>
<a id="Vercel_AI_Provider_GeneratedToolResult__ctor_System_String_System_String_System_String_System_Boolean_"></a>

### GeneratedToolResult(string, string, string, bool)


```csharp
public GeneratedToolResult(string toolCallId, string toolName, string resultJson, bool isError = false)
```


Creates a provider-executed tool result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `resultJson` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `isError` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L488)

## Properties

<a id="Vercel_AI_Provider_GeneratedToolResult_Dynamic_"></a>
<a id="Vercel_AI_Provider_GeneratedToolResult_Dynamic"></a>

### Dynamic


```csharp
public bool Dynamic { get; init; }
```


True when the tool is dynamic. Maps to the result's `dynamic`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L521)

<a id="Vercel_AI_Provider_GeneratedToolResult_Error_"></a>
<a id="Vercel_AI_Provider_GeneratedToolResult_Error"></a>

### Error


```csharp
public Exception? Error { get; init; }
```


The exception the tool threw, when the generation loop ran a client tool that failed.

**Value:** [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L515)

<a id="Vercel_AI_Provider_GeneratedToolResult_InputJson_"></a>
<a id="Vercel_AI_Provider_GeneratedToolResult_InputJson"></a>

### InputJson


```csharp
public string? InputJson { get; init; }
```


The JSON input of the tool call this result answers. Maps to the result's `input`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L530)

<a id="Vercel_AI_Provider_GeneratedToolResult_IsError_"></a>
<a id="Vercel_AI_Provider_GeneratedToolResult_IsError"></a>

### IsError


```csharp
public bool IsError { get; }
```


Whether the tool failed.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L512)

<a id="Vercel_AI_Provider_GeneratedToolResult_ProviderExecuted_"></a>
<a id="Vercel_AI_Provider_GeneratedToolResult_ProviderExecuted"></a>

### ProviderExecuted


```csharp
public bool ProviderExecuted { get; init; }
```


False when the generation loop ran a client tool. A provider's own result is provider-executed. Maps to the result's `providerExecuted`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L527)

<a id="Vercel_AI_Provider_GeneratedToolResult_ProviderMetadata_"></a>
<a id="Vercel_AI_Provider_GeneratedToolResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; init; }
```


Provider-specific metadata for this result, keyed by provider name.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L518)

<a id="Vercel_AI_Provider_GeneratedToolResult_ResultJson_"></a>
<a id="Vercel_AI_Provider_GeneratedToolResult_ResultJson"></a>

### ResultJson


```csharp
public string ResultJson { get; }
```


JSON result.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L509)

<a id="Vercel_AI_Provider_GeneratedToolResult_ToolCallId_"></a>
<a id="Vercel_AI_Provider_GeneratedToolResult_ToolCallId"></a>

### ToolCallId


```csharp
public string ToolCallId { get; }
```


Matching tool call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L503)

<a id="Vercel_AI_Provider_GeneratedToolResult_ToolMetadata_"></a>
<a id="Vercel_AI_Provider_GeneratedToolResult_ToolMetadata"></a>

### ToolMetadata


```csharp
public IReadOnlyDictionary<string, JsonElement>? ToolMetadata { get; init; }
```


Metadata of the tool, apart from provider metadata. Maps to the result's `toolMetadata`.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L524)

<a id="Vercel_AI_Provider_GeneratedToolResult_ToolName_"></a>
<a id="Vercel_AI_Provider_GeneratedToolResult_ToolName"></a>

### ToolName


```csharp
public string ToolName { get; }
```


Tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L506)
