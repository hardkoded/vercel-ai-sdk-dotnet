---
title: "PartialJson"
description: "Parses complete JSON, or JSON repaired by FixJson."
editUrl: false
prev: false
next: false
sidebar:
  label: "PartialJson"
  hidden: true
---

<a id="Vercel_AI_Util_PartialJson"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public static class PartialJson
```


Parses complete JSON, or JSON repaired by [FixJson](/api/Vercel.AI.Util/FixJson/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/PartialJson.cs#L29)

## Fields

<a id="Vercel_AI_Util_PartialJson_FailedParse"></a>

### FailedParse


```csharp
public const string FailedParse = "failed-parse"
```


Neither the original nor the repaired text parsed.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/PartialJson.cs#L41)

<a id="Vercel_AI_Util_PartialJson_RepairedParse"></a>

### RepairedParse


```csharp
public const string RepairedParse = "repaired-parse"
```


The original text failed and the repaired text parsed.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/PartialJson.cs#L38)

<a id="Vercel_AI_Util_PartialJson_SuccessfulParse"></a>

### SuccessfulParse


```csharp
public const string SuccessfulParse = "successful-parse"
```


The original text parsed.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/PartialJson.cs#L35)

<a id="Vercel_AI_Util_PartialJson_UndefinedInput"></a>

### UndefinedInput


```csharp
public const string UndefinedInput = "undefined-input"
```


Input was null, the JavaScript `undefined` case.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/PartialJson.cs#L32)

## Methods

<a id="Vercel_AI_Util_PartialJson_Parse_"></a>
<a id="Vercel_AI_Util_PartialJson_Parse_System_String_"></a>

### Parse(string?)


```csharp
public static PartialJsonResult Parse(string? jsonText)
```


Parses &lt;code class="paramref"&gt;jsonText&lt;/code&gt;. A null argument is undefined input and is not parsed.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `jsonText` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [PartialJsonResult](/api/Vercel.AI.Util/PartialJsonResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/PartialJson.cs#L54)

<a id="Vercel_AI_Util_PartialJson_ParseAsync_"></a>
<a id="Vercel_AI_Util_PartialJson_ParseAsync_System_String_"></a>

### ParseAsync(string?)


```csharp
public static Task<PartialJsonResult> ParseAsync(string? jsonText)
```


Parses &lt;code class="paramref"&gt;jsonText&lt;/code&gt;. A null argument is undefined input and is not parsed.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `jsonText` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [Task&lt;PartialJsonResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/PartialJson.cs#L46)
