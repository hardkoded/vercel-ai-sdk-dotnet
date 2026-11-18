---
title: "SandboxReadTextFileOptions"
description: "Options of ReadTextFileAsync(SandboxReadTextFileOptions). Maps to SandboxReadTextFileOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "SandboxReadTextFileOptions"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_SandboxReadTextFileOptions"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class SandboxReadTextFileOptions : SandboxPathOptions
```


Options of [ReadTextFileAsync(SandboxReadTextFileOptions)](/api/Vercel.AI.ProviderUtils/ISandboxSession/#Vercel_AI_ProviderUtils_ISandboxSession_ReadTextFileAsync_Vercel_AI_ProviderUtils_SandboxReadTextFileOptions_). Maps to `SandboxReadTextFileOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [SandboxPathOptions](/api/Vercel.AI.ProviderUtils/SandboxPathOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L52)

## Properties

<a id="Vercel_AI_ProviderUtils_SandboxReadTextFileOptions_Encoding_"></a>
<a id="Vercel_AI_ProviderUtils_SandboxReadTextFileOptions_Encoding"></a>

### Encoding


```csharp
public string? Encoding { get; init; }
```


Text encoding used to decode the file bytes. Defaults to `utf-8`. Maps to `encoding`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L55)

<a id="Vercel_AI_ProviderUtils_SandboxReadTextFileOptions_EndLine_"></a>
<a id="Vercel_AI_ProviderUtils_SandboxReadTextFileOptions_EndLine"></a>

### EndLine


```csharp
public int? EndLine { get; init; }
```


1-based inclusive end line. When past the line count of the file, the read returns through the end of the file. Maps to `endLine`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L61)

<a id="Vercel_AI_ProviderUtils_SandboxReadTextFileOptions_StartLine_"></a>
<a id="Vercel_AI_ProviderUtils_SandboxReadTextFileOptions_StartLine"></a>

### StartLine


```csharp
public int? StartLine { get; init; }
```


1-based inclusive start line. Defaults to 1. Maps to `startLine`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L58)
