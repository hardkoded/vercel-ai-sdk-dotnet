---
title: "SandboxWriteTextFileOptions"
description: "Options of WriteTextFileAsync(SandboxWriteTextFileOptions). Maps to SandboxWriteTextFileOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "SandboxWriteTextFileOptions"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_SandboxWriteTextFileOptions"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class SandboxWriteTextFileOptions : SandboxPathOptions
```


Options of [WriteTextFileAsync(SandboxWriteTextFileOptions)](/api/Vercel.AI.ProviderUtils/ISandboxSession/#Vercel_AI_ProviderUtils_ISandboxSession_WriteTextFileAsync_Vercel_AI_ProviderUtils_SandboxWriteTextFileOptions_). Maps to `SandboxWriteTextFileOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [SandboxPathOptions](/api/Vercel.AI.ProviderUtils/SandboxPathOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L88)

## Properties

<a id="Vercel_AI_ProviderUtils_SandboxWriteTextFileOptions_Content_"></a>
<a id="Vercel_AI_ProviderUtils_SandboxWriteTextFileOptions_Content"></a>

### Content


```csharp
public string Content { get; init; }
```


Content to write to the file. Maps to `content`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L91)

<a id="Vercel_AI_ProviderUtils_SandboxWriteTextFileOptions_Encoding_"></a>
<a id="Vercel_AI_ProviderUtils_SandboxWriteTextFileOptions_Encoding"></a>

### Encoding


```csharp
public string? Encoding { get; init; }
```


Text encoding used to encode the string to bytes. Defaults to `utf-8`. Maps to `encoding`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L94)
