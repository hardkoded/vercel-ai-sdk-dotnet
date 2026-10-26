---
title: "SystemModelMessage"
description: "System instructions."
editUrl: false
prev: false
next: false
sidebar:
  label: "SystemModelMessage"
  hidden: true
---

<a id="Vercel_AI_Provider_SystemModelMessage"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class SystemModelMessage : ModelMessage
```


System instructions.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ModelMessage](/api/Vercel.AI.Provider/ModelMessage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L617)

## Constructors

<a id="Vercel_AI_Provider_SystemModelMessage__ctor_"></a>
<a id="Vercel_AI_Provider_SystemModelMessage__ctor_System_String_"></a>

### SystemModelMessage(string)


```csharp
public SystemModelMessage(string content)
```


Creates a system message.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `content` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L620)

<a id="Vercel_AI_Provider_SystemModelMessage__ctor_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Text_Json_JsonElement__"></a>

### `SystemModelMessage(string, IReadOnlyDictionary<string, JsonElement>?)`


```csharp
public SystemModelMessage(string content, IReadOnlyDictionary<string, JsonElement>? providerOptions)
```


Creates a system message with provider options, such as an OpenAI `reasoningEffortUpdate`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `content` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerOptions` | [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L626)

## Properties

<a id="Vercel_AI_Provider_SystemModelMessage_Content_"></a>
<a id="Vercel_AI_Provider_SystemModelMessage_Content"></a>

### Content


```csharp
public string Content { get; }
```


Instruction text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L634)

<a id="Vercel_AI_Provider_SystemModelMessage_ProviderOptions_"></a>
<a id="Vercel_AI_Provider_SystemModelMessage_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; }
```


Provider-specific options for this message.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L637)
