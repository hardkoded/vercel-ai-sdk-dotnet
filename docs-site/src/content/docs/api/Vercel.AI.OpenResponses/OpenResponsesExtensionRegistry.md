---
title: "OpenResponsesExtensionRegistry"
description: "Indexes extensions by id and by wire type."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenResponsesExtensionRegistry"
  hidden: true
---

<a id="Vercel_AI_OpenResponses_OpenResponsesExtensionRegistry"></a>

Namespace: [Vercel.AI.OpenResponses](/api/Vercel.AI.OpenResponses/)


```csharp
public sealed class OpenResponsesExtensionRegistry
```


Indexes extensions by id and by wire type.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L124)

## Constructors

<a id="Vercel_AI_OpenResponses_OpenResponsesExtensionRegistry__ctor_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesExtensionRegistry__ctor"></a>

### OpenResponsesExtensionRegistry()


```csharp
public OpenResponsesExtensionRegistry()
```


Creates an empty registry.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L127)

## Properties

<a id="Vercel_AI_OpenResponses_OpenResponsesExtensionRegistry_ByEventType_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesExtensionRegistry_ByEventType"></a>

### ByEventType


```csharp
public IDictionary<string, OpenResponsesExtension> ByEventType { get; }
```


Event type to the extension that decodes it.

**Value:** [IDictionary&lt;string, OpenResponsesExtension&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L137)

<a id="Vercel_AI_OpenResponses_OpenResponsesExtensionRegistry_ByExtensionId_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesExtensionRegistry_ByExtensionId"></a>

### ByExtensionId


```csharp
public IDictionary<string, OpenResponsesExtension> ByExtensionId { get; }
```


Extension id to the extension.

**Value:** [IDictionary&lt;string, OpenResponsesExtension&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L140)

<a id="Vercel_AI_OpenResponses_OpenResponsesExtensionRegistry_ByItemType_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesExtensionRegistry_ByItemType"></a>

### ByItemType


```csharp
public IDictionary<string, OpenResponsesExtension> ByItemType { get; }
```


Item type to the extension that decodes it.

**Value:** [IDictionary&lt;string, OpenResponsesExtension&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L143)

<a id="Vercel_AI_OpenResponses_OpenResponsesExtensionRegistry_ByProviderToolId_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesExtensionRegistry_ByProviderToolId"></a>

### ByProviderToolId


```csharp
public IDictionary<string, OpenResponsesExtension> ByProviderToolId { get; }
```


Provider-tool id to the extension that encodes it.

**Value:** [IDictionary&lt;string, OpenResponsesExtension&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L146)

<a id="Vercel_AI_OpenResponses_OpenResponsesExtensionRegistry_ByToolType_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesExtensionRegistry_ByToolType"></a>

### ByToolType


```csharp
public IDictionary<string, OpenResponsesExtension> ByToolType { get; }
```


Tool type to the extension that encodes it.

**Value:** [IDictionary&lt;string, OpenResponsesExtension&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L149)
