---
title: "GenerateTextInclude"
description: "What generateText keeps of each step. Large payloads, such as base64-encoded images, can cause memory issues. Maps to GenerateTextInclude."
editUrl: false
prev: false
next: false
sidebar:
  label: "GenerateTextInclude"
  hidden: true
---

<a id="Vercel_AI_GenerateTextInclude"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public class GenerateTextInclude
```


What `generateText` keeps of each step. Large payloads, such as base64-encoded images, can cause memory issues. Maps to `GenerateTextInclude`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2606)

## Properties

<a id="Vercel_AI_GenerateTextInclude_RequestBody_"></a>
<a id="Vercel_AI_GenerateTextInclude_RequestBody"></a>

### RequestBody


```csharp
public bool RequestBody { get; init; }
```


Whether to retain the request body in step results. The request body can be large when sending images or files.
Maps to `requestBody`. The default is &lt;a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool"&gt;false&lt;/a&gt;.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2612)

<a id="Vercel_AI_GenerateTextInclude_RequestMessages_"></a>
<a id="Vercel_AI_GenerateTextInclude_RequestMessages"></a>

### RequestMessages


```csharp
public bool RequestMessages { get; init; }
```


Whether to retain the request messages in step results. The request messages can be large when sending images or files.
Maps to `requestMessages`. The default is &lt;a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool"&gt;false&lt;/a&gt;.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2618)

<a id="Vercel_AI_GenerateTextInclude_ResponseBody_"></a>
<a id="Vercel_AI_GenerateTextInclude_ResponseBody"></a>

### ResponseBody


```csharp
public bool ResponseBody { get; init; }
```


Whether to retain the response body in step results. The response body can be large.
Maps to `responseBody`. The default is &lt;a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool"&gt;false&lt;/a&gt;.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2624)
