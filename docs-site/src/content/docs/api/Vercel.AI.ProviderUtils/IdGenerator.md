---
title: "IdGenerator"
description: "Random id generator. The id is not cryptographically secure. A prefix is joined with separator. The separator must not appear in the alphabet."
editUrl: false
prev: false
next: false
sidebar:
  label: "IdGenerator"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_IdGenerator"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public static class IdGenerator
```


Random id generator. The id is not cryptographically secure.
A prefix is joined with `separator`. The separator must not appear in the alphabet.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/IdGenerator.cs#L11)

## Fields

<a id="Vercel_AI_ProviderUtils_IdGenerator_DefaultAlphabet"></a>

### DefaultAlphabet


```csharp
public const string DefaultAlphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"
```


Default alphabet, letters and digits.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/IdGenerator.cs#L14)

## Methods

<a id="Vercel_AI_ProviderUtils_IdGenerator_Create_"></a>
<a id="Vercel_AI_ProviderUtils_IdGenerator_Create_System_String_System_Int32_System_String_System_String_"></a>

### Create(string?, int, string?, string)


```csharp
public static Func<string> Create(string? prefix = null, int size = 16, string? alphabet = null, string separator = "-")
```


Creates a generator. &lt;code class="paramref"&gt;size&lt;/code&gt; is the length of the random part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `prefix` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `size` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `alphabet` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `separator` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [Func&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/IdGenerator.cs#L21)

<a id="Vercel_AI_ProviderUtils_IdGenerator_Generate_"></a>
<a id="Vercel_AI_ProviderUtils_IdGenerator_Generate"></a>

### Generate()


```csharp
public static string Generate()
```


Generates a 16-character id from the default alphabet.

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/IdGenerator.cs#L65)
