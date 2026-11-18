---
title: "SandboxWriteFileOptions"
description: "Options of WriteFileAsync(SandboxWriteFileOptions). Maps to SandboxWriteFileOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "SandboxWriteFileOptions"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_SandboxWriteFileOptions"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class SandboxWriteFileOptions : SandboxPathOptions
```


Options of [WriteFileAsync(SandboxWriteFileOptions)](/api/Vercel.AI.ProviderUtils/ISandboxSession/#Vercel_AI_ProviderUtils_ISandboxSession_WriteFileAsync_Vercel_AI_ProviderUtils_SandboxWriteFileOptions_). Maps to `SandboxWriteFileOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [SandboxPathOptions](/api/Vercel.AI.ProviderUtils/SandboxPathOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L74)

## Properties

<a id="Vercel_AI_ProviderUtils_SandboxWriteFileOptions_Content_"></a>
<a id="Vercel_AI_ProviderUtils_SandboxWriteFileOptions_Content"></a>

### Content


```csharp
public Stream Content { get; init; }
```


Content to write to the file. Maps to `content`.

**Value:** [Stream](https://learn.microsoft.com/dotnet/api/system.io.stream)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L77)
