---
title: "OtherWarning"
description: "A provider warning that is not one of the structured kinds."
editUrl: false
prev: false
next: false
sidebar:
  label: "OtherWarning"
  hidden: true
---

<a id="Vercel_AI_Util_OtherWarning"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class OtherWarning : ModelWarning
```


A provider warning that is not one of the structured kinds.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ModelWarning](/api/Vercel.AI.Util/ModelWarning/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LogWarnings.cs#L86)

## Constructors

<a id="Vercel_AI_Util_OtherWarning__ctor_"></a>
<a id="Vercel_AI_Util_OtherWarning__ctor_System_String_"></a>

### OtherWarning(string)


```csharp
public OtherWarning(string message)
```


Creates the warning.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LogWarnings.cs#L89)

## Properties

<a id="Vercel_AI_Util_OtherWarning_Message_"></a>
<a id="Vercel_AI_Util_OtherWarning_Message"></a>

### Message


```csharp
public string Message { get; }
```


Warning text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LogWarnings.cs#L101)

<a id="Vercel_AI_Util_OtherWarning_Type_"></a>
<a id="Vercel_AI_Util_OtherWarning_Type"></a>

### Type


```csharp
public override string Type { get; }
```


Warning kind: `unsupported`, `compatibility`, `deprecated`, or `other`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LogWarnings.cs#L95)
