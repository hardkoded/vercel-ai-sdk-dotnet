---
title: "AssistantCustomPart"
description: "A provider-specific part, identified by Kind."
editUrl: false
prev: false
next: false
sidebar:
  label: "AssistantCustomPart"
  hidden: true
---

<a id="Vercel_AI_Provider_AssistantCustomPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class AssistantCustomPart : AssistantContentPart
```


A provider-specific part, identified by [Kind](#Vercel_AI_Provider_AssistantCustomPart_Kind).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [AssistantContentPart](/api/Vercel.AI.Provider/AssistantContentPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1142)

## Constructors

<a id="Vercel_AI_Provider_AssistantCustomPart__ctor_"></a>
<a id="Vercel_AI_Provider_AssistantCustomPart__ctor_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Text_Json_JsonElement__"></a>

### `AssistantCustomPart(string, IReadOnlyDictionary<string, JsonElement>?)`


```csharp
public AssistantCustomPart(string kind, IReadOnlyDictionary<string, JsonElement>? providerOptions = null)
```


Creates a custom part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `kind` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerOptions` | [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1145)

## Properties

<a id="Vercel_AI_Provider_AssistantCustomPart_Kind_"></a>
<a id="Vercel_AI_Provider_AssistantCustomPart_Kind"></a>

### Kind


```csharp
public string Kind { get; }
```


Provider-scoped kind.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1152)
