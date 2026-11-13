---
title: "AssemblyAIFailedResponseHandler"
description: "Creates .NET API exceptions from AssemblyAI failed JSON responses."
editUrl: false
prev: false
next: false
sidebar:
  label: "AssemblyAIFailedResponseHandler"
  hidden: true
---

<a id="Vercel_AI_AssemblyAI_AssemblyAIFailedResponseHandler"></a>

Namespace: [Vercel.AI.AssemblyAI](/api/Vercel.AI.AssemblyAI/)


```csharp
public static class AssemblyAIFailedResponseHandler
```


Creates .NET API exceptions from AssemblyAI failed JSON responses.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L123)

## Methods

<a id="Vercel_AI_AssemblyAI_AssemblyAIFailedResponseHandler_Handle_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAIFailedResponseHandler_Handle_System_String_System_Int32_System_String_System_Object_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_String_"></a>

### `Handle(string, int, string, object?, IReadOnlyDictionary<string, string>?, string?)`


```csharp
public static ApiException Handle(string responseBody, int statusCode, string url, object? requestBodyValues, IReadOnlyDictionary<string, string>? responseHeaders = null, string? statusText = null)
```


Parses an AssemblyAI error response and preserves the parsed body on [ErrorData](/api/Vercel.AI.Provider/ApiException/#Vercel_AI_Provider_ApiException_ErrorData).
A body that is not recognized falls back to &lt;code class="paramref"&gt;statusText&lt;/code&gt;, then to the body.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `responseBody` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `statusCode` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `requestBodyValues` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |
| `responseHeaders` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `statusText` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ApiException](/api/Vercel.AI.Provider/ApiException/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L129)
