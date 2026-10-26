---
title: "CustomContentUIPart"
description: "A provider-specific part. Maps to CustomContentUIPart."
editUrl: false
prev: false
next: false
sidebar:
  label: "CustomContentUIPart"
  hidden: true
---

<a id="Vercel_AI_Ui_CustomContentUIPart"></a>

Namespace: [Vercel.AI.Ui](/api/Vercel.AI.Ui/)


```csharp
public sealed class CustomContentUIPart : UIMessagePart
```


A provider-specific part. Maps to `CustomContentUIPart`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [UIMessagePart](/api/Vercel.AI.Ui/UIMessagePart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L82)

## Constructors

<a id="Vercel_AI_Ui_CustomContentUIPart__ctor_"></a>
<a id="Vercel_AI_Ui_CustomContentUIPart__ctor_System_String_"></a>

### CustomContentUIPart(string)


```csharp
public CustomContentUIPart(string kind)
```


Creates a custom part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `kind` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L85)

## Properties

<a id="Vercel_AI_Ui_CustomContentUIPart_Kind_"></a>
<a id="Vercel_AI_Ui_CustomContentUIPart_Kind"></a>

### Kind


```csharp
public string Kind { get; }
```


The kind of custom content, in the format `{provider}.{provider-type}`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L92)

<a id="Vercel_AI_Ui_CustomContentUIPart_ProviderMetadata_"></a>
<a id="Vercel_AI_Ui_CustomContentUIPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderMetadata { get; init; }
```


The provider metadata.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L95)
