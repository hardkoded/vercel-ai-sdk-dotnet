---
title: "AssistantContentPart"
description: "One part of an assistant message, kept in the order the model produced it."
editUrl: false
prev: false
next: false
sidebar:
  label: "AssistantContentPart"
  hidden: true
---

<a id="Vercel_AI_Provider_AssistantContentPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public abstract class AssistantContentPart
```


One part of an assistant message, kept in the order the model produced it.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [AssistantCustomPart](/api/Vercel.AI.Provider/AssistantCustomPart/), [AssistantFilePart](/api/Vercel.AI.Provider/AssistantFilePart/), [AssistantReasoningPart](/api/Vercel.AI.Provider/AssistantReasoningPart/), [AssistantTextPart](/api/Vercel.AI.Provider/AssistantTextPart/), [AssistantToolApprovalRequestPart](/api/Vercel.AI.Provider/AssistantToolApprovalRequestPart/), [AssistantToolCallPart](/api/Vercel.AI.Provider/AssistantToolCallPart/), [AssistantToolResultPart](/api/Vercel.AI.Provider/AssistantToolResultPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L753)

## Constructors

<a id="Vercel_AI_Provider_AssistantContentPart__ctor_"></a>
<a id="Vercel_AI_Provider_AssistantContentPart__ctor_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Text_Json_JsonElement__"></a>

### `AssistantContentPart(string, IReadOnlyDictionary<string, JsonElement>?)`


```csharp
protected AssistantContentPart(string type, IReadOnlyDictionary<string, JsonElement>? providerOptions)
```


Creates a part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerOptions` | [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L756)

## Properties

<a id="Vercel_AI_Provider_AssistantContentPart_ProviderOptions_"></a>
<a id="Vercel_AI_Provider_AssistantContentPart_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; }
```


Provider-specific options for this part, such as an `itemId`.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L766)

<a id="Vercel_AI_Provider_AssistantContentPart_Type_"></a>
<a id="Vercel_AI_Provider_AssistantContentPart_Type"></a>

### Type


```csharp
public string Type { get; }
```


Part type: `text`, `file`, `reasoning`, `reasoning-file`, `tool-call`, `tool-result`, or `custom`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L763)
