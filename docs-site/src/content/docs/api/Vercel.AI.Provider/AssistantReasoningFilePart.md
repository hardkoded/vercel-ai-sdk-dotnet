---
title: "AssistantReasoningFilePart"
description: "A file the model produced as part of its reasoning. Maps to the assistant reasoning-file part."
editUrl: false
prev: false
next: false
sidebar:
  label: "AssistantReasoningFilePart"
  hidden: true
---

<a id="Vercel_AI_Provider_AssistantReasoningFilePart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class AssistantReasoningFilePart : AssistantFilePart
```


A file the model produced as part of its reasoning. Maps to the assistant `reasoning-file` part.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [AssistantContentPart](/api/Vercel.AI.Provider/AssistantContentPart/) → [AssistantFilePart](/api/Vercel.AI.Provider/AssistantFilePart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L845)

## Constructors

<a id="Vercel_AI_Provider_AssistantReasoningFilePart__ctor_"></a>
<a id="Vercel_AI_Provider_AssistantReasoningFilePart__ctor_System_String_System_String_System_Byte___System_Collections_Generic_IReadOnlyDictionary_System_String_System_Text_Json_JsonElement__"></a>

### `AssistantReasoningFilePart(string, string?, byte[]?, IReadOnlyDictionary<string, JsonElement>?)`


```csharp
public AssistantReasoningFilePart(string mediaType, string? url, byte[]? data, IReadOnlyDictionary<string, JsonElement>? providerOptions = null)
```


Creates a reasoning-file part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) | IANA media type. |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) | Remote URL, when the file is not inlined. |
| `data` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) | Inline bytes. |
| `providerOptions` | [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) | Provider-specific options for this part. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L852)
