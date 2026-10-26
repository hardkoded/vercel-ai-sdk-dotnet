---
title: "ToolInputStartStreamPart"
description: "Marks the start of streamed tool input."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolInputStartStreamPart"
  hidden: true
---

<a id="Vercel_AI_Provider_ToolInputStartStreamPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class ToolInputStartStreamPart : LanguageModelStreamPart
```


Marks the start of streamed tool input.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [LanguageModelStreamPart](/api/Vercel.AI.Provider/LanguageModelStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1754)

## Constructors

<a id="Vercel_AI_Provider_ToolInputStartStreamPart__ctor_"></a>
<a id="Vercel_AI_Provider_ToolInputStartStreamPart__ctor_System_String_System_String_System_Nullable_System_Text_Json_JsonElement__System_Nullable_System_Boolean__System_Nullable_System_Boolean__System_String_System_Nullable_Vercel_AI_Provider_ToolInputEncoding__"></a>

### ToolInputStartStreamPart(string, string, JsonElement?, bool?, bool?, string?, ToolInputEncoding?)


```csharp
public ToolInputStartStreamPart(string id, string toolName, JsonElement? providerMetadata = null, bool? providerExecuted = null, bool? dynamic = null, string? title = null, ToolInputEncoding? inputEncoding = null)
```


Creates a tool-input-start part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `providerExecuted` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `dynamic` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `title` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `inputEncoding` | `ToolInputEncoding?` |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1757)

## Properties

<a id="Vercel_AI_Provider_ToolInputStartStreamPart_Dynamic_"></a>
<a id="Vercel_AI_Provider_ToolInputStartStreamPart_Dynamic"></a>

### Dynamic


```csharp
public bool? Dynamic { get; }
```


True for a tool the caller did not declare.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1789)

<a id="Vercel_AI_Provider_ToolInputStartStreamPart_Id_"></a>
<a id="Vercel_AI_Provider_ToolInputStartStreamPart_Id"></a>

### Id


```csharp
public string Id { get; }
```


Tool call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1777)

<a id="Vercel_AI_Provider_ToolInputStartStreamPart_InputEncoding_"></a>
<a id="Vercel_AI_Provider_ToolInputStartStreamPart_InputEncoding"></a>

### InputEncoding


```csharp
public ToolInputEncoding? InputEncoding { get; }
```


Encoding of the input deltas. Defaults to JSON when &lt;a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null"&gt;null&lt;/a&gt;. Text deltas concatenate
to a raw string. The final tool call input must still be JSON-encoded, whatever the delta encoding.

**Value:** `ToolInputEncoding?`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1798)

<a id="Vercel_AI_Provider_ToolInputStartStreamPart_ProviderExecuted_"></a>
<a id="Vercel_AI_Provider_ToolInputStartStreamPart_ProviderExecuted"></a>

### ProviderExecuted


```csharp
public bool? ProviderExecuted { get; }
```


True when the provider executes the tool.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1786)

<a id="Vercel_AI_Provider_ToolInputStartStreamPart_ProviderMetadata_"></a>
<a id="Vercel_AI_Provider_ToolInputStartStreamPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata for this tool input.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1783)

<a id="Vercel_AI_Provider_ToolInputStartStreamPart_Title_"></a>
<a id="Vercel_AI_Provider_ToolInputStartStreamPart_Title"></a>

### Title


```csharp
public string? Title { get; }
```


Optional tool title.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1792)

<a id="Vercel_AI_Provider_ToolInputStartStreamPart_ToolName_"></a>
<a id="Vercel_AI_Provider_ToolInputStartStreamPart_ToolName"></a>

### ToolName


```csharp
public string ToolName { get; }
```


Tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1780)
