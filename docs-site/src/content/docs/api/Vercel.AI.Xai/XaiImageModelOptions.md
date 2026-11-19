---
title: "XaiImageModelOptions"
description: "Provider options for xAI image generation, read from providerOptions.xai. Maps to XaiImageModelOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "XaiImageModelOptions"
  hidden: true
---

<a id="Vercel_AI_Xai_XaiImageModelOptions"></a>

Namespace: [Vercel.AI.Xai](/api/Vercel.AI.Xai/)


```csharp
public sealed class XaiImageModelOptions
```


Provider options for xAI image generation, read from `providerOptions.xai`. Maps to `XaiImageModelOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiImageModelOptions.cs#L11)

## Properties

<a id="Vercel_AI_Xai_XaiImageModelOptions_AspectRatio_"></a>
<a id="Vercel_AI_Xai_XaiImageModelOptions_AspectRatio"></a>

### AspectRatio


```csharp
public string? AspectRatio { get; set; }
```


Aspect ratio, used when the call sets no aspect ratio. Maps to `aspect_ratio`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiImageModelOptions.cs#L18)

<a id="Vercel_AI_Xai_XaiImageModelOptions_OutputFormat_"></a>
<a id="Vercel_AI_Xai_XaiImageModelOptions_OutputFormat"></a>

### OutputFormat


```csharp
public string? OutputFormat { get; set; }
```


Output format such as `jpeg`. Maps to `output_format`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiImageModelOptions.cs#L21)

<a id="Vercel_AI_Xai_XaiImageModelOptions_Quality_"></a>
<a id="Vercel_AI_Xai_XaiImageModelOptions_Quality"></a>

### Quality


```csharp
public string? Quality { get; set; }
```


`low`, `medium`, `high`, or `auto`. Maps to `quality`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiImageModelOptions.cs#L30)

<a id="Vercel_AI_Xai_XaiImageModelOptions_Resolution_"></a>
<a id="Vercel_AI_Xai_XaiImageModelOptions_Resolution"></a>

### Resolution


```csharp
public string? Resolution { get; set; }
```


`1k`, `1.5k`, or `2k`. Maps to `resolution`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiImageModelOptions.cs#L27)

<a id="Vercel_AI_Xai_XaiImageModelOptions_SyncMode_"></a>
<a id="Vercel_AI_Xai_XaiImageModelOptions_SyncMode"></a>

### SyncMode


```csharp
public bool? SyncMode { get; set; }
```


Maps to `sync_mode`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiImageModelOptions.cs#L24)

<a id="Vercel_AI_Xai_XaiImageModelOptions_User_"></a>
<a id="Vercel_AI_Xai_XaiImageModelOptions_User"></a>

### User


```csharp
public string? User { get; set; }
```


A unique identifier representing the end user. Maps to `user`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiImageModelOptions.cs#L33)

## Methods

<a id="Vercel_AI_Xai_XaiImageModelOptions_Parse_"></a>
<a id="Vercel_AI_Xai_XaiImageModelOptions_Parse_System_Nullable_System_Text_Json_JsonElement__"></a>

### Parse(JsonElement?)


```csharp
public static XaiImageModelOptions? Parse(JsonElement? providerOptions)
```


Reads `providerOptions.xai`. Maps to `parseProviderOptions` with `xaiImageModelOptions`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerOptions` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [XaiImageModelOptions](./)

**Exceptions**

| Type | Condition |
| --- | --- |
| [InvalidArgumentException](/api/Vercel.AI.Operations/InvalidArgumentException/) | An option has the wrong shape. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiImageModelOptions.cs#L37)
