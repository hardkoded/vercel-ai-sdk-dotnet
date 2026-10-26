---
title: "MockGenerateResult"
description: "Generate result stored in a scripted list."
editUrl: false
prev: false
next: false
sidebar:
  label: "MockGenerateResult"
  hidden: true
---

<a id="Vercel_AI_Testing_MockGenerateResult"></a>

Namespace: [Vercel.AI.Testing](/api/Vercel.AI.Testing/)


```csharp
public sealed class MockGenerateResult
```


Generate result stored in a scripted list.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L32)

## Constructors

<a id="Vercel_AI_Testing_MockGenerateResult__ctor_"></a>
<a id="Vercel_AI_Testing_MockGenerateResult__ctor_System_String_"></a>

### MockGenerateResult(string)


```csharp
public MockGenerateResult(string text)
```


Creates a result whose only content is &lt;code class="paramref"&gt;text&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L35)

## Properties

<a id="Vercel_AI_Testing_MockGenerateResult_Content_"></a>
<a id="Vercel_AI_Testing_MockGenerateResult_Content"></a>

### Content


```csharp
public IReadOnlyList<MockTextContent> Content { get; }
```


Generated content.

**Value:** [IReadOnlyList&lt;MockTextContent&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L41)
