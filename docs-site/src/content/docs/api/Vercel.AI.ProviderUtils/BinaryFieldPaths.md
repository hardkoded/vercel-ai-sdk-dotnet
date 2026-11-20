---
title: "BinaryFieldPaths"
description: "The compiled paths, a trie that is walked while scanning. The edges of a node are the bytes of an object key (a key ends with its closing quote), the items o..."
editUrl: false
prev: false
next: false
sidebar:
  label: "BinaryFieldPaths"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_BinaryFieldPaths"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class BinaryFieldPaths
```


The compiled paths, a trie that is walked while scanning. The edges of a node are the bytes of an object key
(a key ends with its closing quote), the items of an array, and "a path ends here".

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonBinaryFieldScanner.cs#L18)
