---
title: "AnthropicFileDeleteResult"
description: "Result of deleting an Anthropic file."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicFileDeleteResult"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicFileDeleteResult"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicFileDeleteResult
```


Result of deleting an Anthropic file.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicFiles.cs#L458)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicFileDeleteResult__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicFileDeleteResult__ctor_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Boolean_"></a>

### `AnthropicFileDeleteResult(IReadOnlyDictionary<string, string>, bool)`


```csharp
public AnthropicFileDeleteResult(IReadOnlyDictionary<string, string> providerReference, bool deleted)
```


Creates a delete result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerReference` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `deleted` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicFiles.cs#L461)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicFileDeleteResult_Deleted_"></a>
<a id="Vercel_AI_Anthropic_AnthropicFileDeleteResult_Deleted"></a>

### Deleted


```csharp
public bool Deleted { get; }
```


Always true after a successful delete.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicFiles.cs#L477)

<a id="Vercel_AI_Anthropic_AnthropicFileDeleteResult_ProviderReference_"></a>
<a id="Vercel_AI_Anthropic_AnthropicFileDeleteResult_ProviderReference"></a>

### ProviderReference


```csharp
public IReadOnlyDictionary<string, string> ProviderReference { get; }
```


Reference containing the deleted `anthropic` file id.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicFiles.cs#L474)

<a id="Vercel_AI_Anthropic_AnthropicFileDeleteResult_Warnings_"></a>
<a id="Vercel_AI_Anthropic_AnthropicFileDeleteResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<ModelWarning> Warnings { get; }
```


Always empty.

**Value:** [IReadOnlyList&lt;ModelWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicFiles.cs#L471)
