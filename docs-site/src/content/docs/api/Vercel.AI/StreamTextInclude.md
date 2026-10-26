---
title: "StreamTextInclude"
description: "What streamText keeps of the request and the provider stream. Maps to StreamTextInclude."
editUrl: false
prev: false
next: false
sidebar:
  label: "StreamTextInclude"
  hidden: true
---

<a id="Vercel_AI_StreamTextInclude"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class StreamTextInclude
```


What `streamText` keeps of the request and the provider stream. Maps to `StreamTextInclude`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1093)

## Properties

<a id="Vercel_AI_StreamTextInclude_RawChunks_"></a>
<a id="Vercel_AI_StreamTextInclude_RawChunks"></a>

### RawChunks


```csharp
public bool RawChunks { get; init; }
```


Whether to forward the provider's raw chunks as [RawPart](/api/Vercel.AI/RawPart/) in the full stream. It asks the model to include them.
Maps to `rawChunks`. The default is &lt;a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool"&gt;false&lt;/a&gt;.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1111)

<a id="Vercel_AI_StreamTextInclude_RequestBody_"></a>
<a id="Vercel_AI_StreamTextInclude_RequestBody"></a>

### RequestBody


```csharp
public bool RequestBody { get; init; }
```


Whether to retain the request body in the `start-step` part. The request body can be large when sending images or files.
Maps to `requestBody`. The default is &lt;a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool"&gt;false&lt;/a&gt;.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1099)

<a id="Vercel_AI_StreamTextInclude_RequestMessages_"></a>
<a id="Vercel_AI_StreamTextInclude_RequestMessages"></a>

### RequestMessages


```csharp
public bool RequestMessages { get; init; }
```


Whether to retain the request messages in the `start-step` part. The request messages can be large when sending images or files.
Maps to `requestMessages`. The default is &lt;a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool"&gt;false&lt;/a&gt;.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1105)
