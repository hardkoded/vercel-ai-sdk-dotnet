---
title: "NamedException"
description: "An exception identified by a name such as AbortError or ResponseAborted."
editUrl: false
prev: false
next: false
sidebar:
  label: "NamedException"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_NamedException"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class NamedException : Exception, ISerializable
```


An exception identified by a name such as `AbortError` or `ResponseAborted`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Delay.cs#L24)

## Constructors

<a id="Vercel_AI_ProviderUtils_NamedException__ctor_"></a>
<a id="Vercel_AI_ProviderUtils_NamedException__ctor_System_String_System_String_"></a>

### NamedException(string, string)


```csharp
public NamedException(string name, string message)
```


Creates a named exception.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Delay.cs#L27)

## Properties

<a id="Vercel_AI_ProviderUtils_NamedException_Name_"></a>
<a id="Vercel_AI_ProviderUtils_NamedException_Name"></a>

### Name


```csharp
public string Name { get; }
```


Error name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Delay.cs#L34)
