---
title: "OperationHeaders"
description: "Header and retry helpers shared by the operation types."
editUrl: false
prev: false
next: false
sidebar:
  label: "OperationHeaders"
  hidden: true
---

<a id="Vercel_AI_Operations_OperationHeaders"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public static class OperationHeaders
```


Header and retry helpers shared by the operation types.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L471)

## Methods

<a id="Vercel_AI_Operations_OperationHeaders_MediaTypeFromContentType_"></a>
<a id="Vercel_AI_Operations_OperationHeaders_MediaTypeFromContentType_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_String_"></a>

### `MediaTypeFromContentType(IReadOnlyDictionary<string, string>?, string)`


```csharp
public static string? MediaTypeFromContentType(IReadOnlyDictionary<string, string>? headers, string requiredPrefix)
```


Reads a header, ignoring case and parameters after `;`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `requiredPrefix` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L501)

<a id="Vercel_AI_Operations_OperationHeaders_WithUserAgent_"></a>
<a id="Vercel_AI_Operations_OperationHeaders_WithUserAgent_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_String_"></a>

### `WithUserAgent(IReadOnlyDictionary<string, string>?, string)`


```csharp
public static Dictionary<string, string> WithUserAgent(IReadOnlyDictionary<string, string>? headers, string suffix)
```


Lowercases header names and appends the AI SDK user-agent.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `suffix` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [Dictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L474)
