---
title: "JsonResponseTools"
description: "Helpers for providers that cannot combine tools with native structured output. They add an internal function tool whose arguments are the structured output,..."
editUrl: false
prev: false
next: false
sidebar:
  label: "JsonResponseTools"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_JsonResponseTools"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public static class JsonResponseTools
```


Helpers for providers that cannot combine tools with native structured output. They add an internal function tool whose
arguments are the structured output, then turn that tool call back into text.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonResponseTools.cs#L15)

## Methods

<a id="Vercel_AI_ProviderUtils_JsonResponseTools_ConvertJsonResponseToolStream_"></a>
<a id="Vercel_AI_ProviderUtils_JsonResponseTools_ConvertJsonResponseToolStream_System_Collections_Generic_IAsyncEnumerable_Vercel_AI_Provider_LanguageModelStreamPart__System_String_System_Threading_CancellationToken_"></a>

### `ConvertJsonResponseToolStream(IAsyncEnumerable<LanguageModelStreamPart>, string, CancellationToken)`


```csharp
public static IAsyncEnumerable<LanguageModelStreamPart> ConvertJsonResponseToolStream(IAsyncEnumerable<LanguageModelStreamPart> source, string toolName, CancellationToken cancellationToken = default)
```


Rewrites a language-model stream so the JSON response tool &lt;code class="paramref"&gt;toolName&lt;/code&gt; produces text.
Maps to `convertJsonResponseToolStream`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `source` | [IAsyncEnumerable&lt;LanguageModelStreamPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1) |  |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [IAsyncEnumerable&lt;LanguageModelStreamPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonResponseTools.cs#L23)
