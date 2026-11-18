---
title: "SandboxDirectoryEntry"
description: "Entry in a directory listing. Maps to SandboxDirectoryEntry."
editUrl: false
prev: false
next: false
sidebar:
  label: "SandboxDirectoryEntry"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_SandboxDirectoryEntry"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class SandboxDirectoryEntry
```


Entry in a directory listing. Maps to `SandboxDirectoryEntry`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L137)

## Properties

<a id="Vercel_AI_ProviderUtils_SandboxDirectoryEntry_Name_"></a>
<a id="Vercel_AI_ProviderUtils_SandboxDirectoryEntry_Name"></a>

### Name


```csharp
public string Name { get; init; }
```


Name of the entry, without the directory path. Maps to `name`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L140)

<a id="Vercel_AI_ProviderUtils_SandboxDirectoryEntry_Type_"></a>
<a id="Vercel_AI_ProviderUtils_SandboxDirectoryEntry_Type"></a>

### Type


```csharp
public SandboxDirectoryEntryType Type { get; init; }
```


Type of the entry. Maps to `type`.

**Value:** [SandboxDirectoryEntryType](/api/Vercel.AI.ProviderUtils/SandboxDirectoryEntryType/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L143)
