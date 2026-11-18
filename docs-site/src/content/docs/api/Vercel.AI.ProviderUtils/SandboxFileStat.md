---
title: "SandboxFileStat"
description: "Information about a file or directory in the sandbox. Symbolic links are followed. Maps to SandboxFileStat."
editUrl: false
prev: false
next: false
sidebar:
  label: "SandboxFileStat"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_SandboxFileStat"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class SandboxFileStat
```


Information about a file or directory in the sandbox. Symbolic links are followed. Maps to `SandboxFileStat`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L111)

## Properties

<a id="Vercel_AI_ProviderUtils_SandboxFileStat_Size_"></a>
<a id="Vercel_AI_ProviderUtils_SandboxFileStat_Size"></a>

### Size


```csharp
public long Size { get; init; }
```


Size in bytes. The size of a directory is implementation-defined. Maps to `size`.

**Value:** [long](https://learn.microsoft.com/dotnet/api/system.int64)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L117)

<a id="Vercel_AI_ProviderUtils_SandboxFileStat_Type_"></a>
<a id="Vercel_AI_ProviderUtils_SandboxFileStat_Type"></a>

### Type


```csharp
public SandboxFileType Type { get; init; }
```


Type of the path. Maps to `type`.

**Value:** [SandboxFileType](/api/Vercel.AI.ProviderUtils/SandboxFileType/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L114)
