---
title: "OpenResponsesExtensions"
description: "Builds an OpenResponsesExtensionRegistry."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenResponsesExtensions"
  hidden: true
---

<a id="Vercel_AI_OpenResponses_OpenResponsesExtensions"></a>

Namespace: [Vercel.AI.OpenResponses](/api/Vercel.AI.OpenResponses/)


```csharp
public static class OpenResponsesExtensions
```


Builds an [OpenResponsesExtensionRegistry](/api/Vercel.AI.OpenResponses/OpenResponsesExtensionRegistry/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L153)

## Methods

<a id="Vercel_AI_OpenResponses_OpenResponsesExtensions_CreateRegistry_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesExtensions_CreateRegistry_System_Collections_Generic_IReadOnlyList_Vercel_AI_OpenResponses_OpenResponsesExtension__"></a>

### `CreateRegistry(IReadOnlyList<OpenResponsesExtension>?)`


```csharp
public static OpenResponsesExtensionRegistry CreateRegistry(IReadOnlyList<OpenResponsesExtension>? extensions)
```


Indexes &lt;code class="paramref"&gt;extensions&lt;/code&gt; and rejects incomplete or colliding registrations.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `extensions` | [IReadOnlyList&lt;OpenResponsesExtension&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

**Returns:** [OpenResponsesExtensionRegistry](/api/Vercel.AI.OpenResponses/OpenResponsesExtensionRegistry/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L156)
