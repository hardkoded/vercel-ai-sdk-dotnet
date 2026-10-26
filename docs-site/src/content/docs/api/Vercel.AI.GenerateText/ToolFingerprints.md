---
title: "ToolFingerprints"
description: "Tool-definition fingerprints. Maps to fingerprintTools and detectToolDrift."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolFingerprints"
  hidden: true
---

<a id="Vercel_AI_GenerateText_ToolFingerprints"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public static class ToolFingerprints
```


Tool-definition fingerprints. Maps to `fingerprintTools` and `detectToolDrift`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolFingerprint.cs#L58)

## Methods

<a id="Vercel_AI_GenerateText_ToolFingerprints_DetectToolDrift_"></a>
<a id="Vercel_AI_GenerateText_ToolFingerprints_DetectToolDrift_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `DetectToolDrift(IReadOnlyDictionary<string, string>, IReadOnlyDictionary<string, string>)`


```csharp
public static ToolDrift DetectToolDrift(IReadOnlyDictionary<string, string> current, IReadOnlyDictionary<string, string> baseline)
```


Diffs two fingerprint maps. Names are compared as own keys.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `current` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `baseline` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [ToolDrift](/api/Vercel.AI.GenerateText/ToolDrift/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolFingerprint.cs#L78)

<a id="Vercel_AI_GenerateText_ToolFingerprints_FingerprintTools_"></a>
<a id="Vercel_AI_GenerateText_ToolFingerprints_FingerprintTools_System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_GenerateText_FingerprintTool__"></a>

### `FingerprintTools(IReadOnlyDictionary<string, FingerprintTool>)`


```csharp
public static IReadOnlyDictionary<string, string> FingerprintTools(IReadOnlyDictionary<string, FingerprintTool> tools)
```


Returns a stable digest for each tool. Function descriptions hash as a single placeholder.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `tools` | [IReadOnlyDictionary&lt;string, FingerprintTool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolFingerprint.cs#L61)
