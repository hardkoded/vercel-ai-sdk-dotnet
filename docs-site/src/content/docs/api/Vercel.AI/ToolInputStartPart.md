---
title: "ToolInputStartPart"
description: "The model started streaming tool input."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolInputStartPart"
  hidden: true
---

<a id="Vercel_AI_ToolInputStartPart"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class ToolInputStartPart : TextStreamPart
```


The model started streaming tool input.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [TextStreamPart](/api/Vercel.AI/TextStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1207)

## Constructors

<a id="Vercel_AI_ToolInputStartPart__ctor_"></a>
<a id="Vercel_AI_ToolInputStartPart__ctor_System_String_System_String_System_Nullable_System_Text_Json_JsonElement__System_Nullable_System_Boolean__System_Boolean_System_Nullable_Vercel_AI_Provider_ToolInputEncoding__System_String_"></a>

### ToolInputStartPart(string, string, JsonElement?, bool?, bool, ToolInputEncoding?, string?)


```csharp
public ToolInputStartPart(string id, string toolName, JsonElement? providerMetadata, bool? providerExecuted, bool dynamic, ToolInputEncoding? inputEncoding = null, string? title = null)
```


Creates a tool-input-start part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `providerExecuted` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `dynamic` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `inputEncoding` | `ToolInputEncoding?` |  |
| `title` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1210)

## Properties

<a id="Vercel_AI_ToolInputStartPart_Dynamic_"></a>
<a id="Vercel_AI_ToolInputStartPart_Dynamic"></a>

### Dynamic


```csharp
public bool Dynamic { get; }
```


True for a tool the caller did not declare, or a dynamic tool.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1238)

<a id="Vercel_AI_ToolInputStartPart_Id_"></a>
<a id="Vercel_AI_ToolInputStartPart_Id"></a>

### Id


```csharp
public string Id { get; }
```


Tool call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1226)

<a id="Vercel_AI_ToolInputStartPart_InputEncoding_"></a>
<a id="Vercel_AI_ToolInputStartPart_InputEncoding"></a>

### InputEncoding


```csharp
public ToolInputEncoding? InputEncoding { get; }
```


Encoding of the input deltas. Defaults to JSON when &lt;a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null"&gt;null&lt;/a&gt;.

**Value:** `ToolInputEncoding?`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1244)

<a id="Vercel_AI_ToolInputStartPart_ProviderExecuted_"></a>
<a id="Vercel_AI_ToolInputStartPart_ProviderExecuted"></a>

### ProviderExecuted


```csharp
public bool? ProviderExecuted { get; }
```


True when the provider executes the tool.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1235)

<a id="Vercel_AI_ToolInputStartPart_ProviderMetadata_"></a>
<a id="Vercel_AI_ToolInputStartPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata for this tool input.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1232)

<a id="Vercel_AI_ToolInputStartPart_Title_"></a>
<a id="Vercel_AI_ToolInputStartPart_Title"></a>

### Title


```csharp
public string? Title { get; }
```


Display title of the tool. Maps to `title`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1223)

<a id="Vercel_AI_ToolInputStartPart_ToolMetadata_"></a>
<a id="Vercel_AI_ToolInputStartPart_ToolMetadata"></a>

### ToolMetadata


```csharp
public IReadOnlyDictionary<string, JsonElement>? ToolMetadata { get; init; }
```


Metadata of the tool, apart from provider metadata. Maps to the part's `toolMetadata`.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1241)

<a id="Vercel_AI_ToolInputStartPart_ToolName_"></a>
<a id="Vercel_AI_ToolInputStartPart_ToolName"></a>

### ToolName


```csharp
public string ToolName { get; }
```


Tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1229)
