---
title: "DataUIPart"
description: "A data part. Its type is data-{name}. Maps to DataUIPart."
editUrl: false
prev: false
next: false
sidebar:
  label: "DataUIPart"
  hidden: true
---

<a id="Vercel_AI_Ui_DataUIPart"></a>

Namespace: [Vercel.AI.Ui](/api/Vercel.AI.Ui/)


```csharp
public sealed class DataUIPart : UIMessagePart
```


A data part. Its type is `data-{name}`. Maps to `DataUIPart`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [UIMessagePart](/api/Vercel.AI.Ui/UIMessagePart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L232)

## Constructors

<a id="Vercel_AI_Ui_DataUIPart__ctor_"></a>
<a id="Vercel_AI_Ui_DataUIPart__ctor_System_String_System_Text_Json_JsonElement_"></a>

### DataUIPart(string, JsonElement)


```csharp
public DataUIPart(string name, JsonElement data)
```


Creates a data part named &lt;code class="paramref"&gt;name&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `data` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L235)

## Properties

<a id="Vercel_AI_Ui_DataUIPart_Data_"></a>
<a id="Vercel_AI_Ui_DataUIPart_Data"></a>

### Data


```csharp
public JsonElement Data { get; }
```


The data.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L249)

<a id="Vercel_AI_Ui_DataUIPart_Id_"></a>
<a id="Vercel_AI_Ui_DataUIPart_Id"></a>

### Id


```csharp
public string? Id { get; init; }
```


The part id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L246)

<a id="Vercel_AI_Ui_DataUIPart_Name_"></a>
<a id="Vercel_AI_Ui_DataUIPart_Name"></a>

### Name


```csharp
public string Name { get; }
```


The data name, without the `data-` prefix.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L243)
