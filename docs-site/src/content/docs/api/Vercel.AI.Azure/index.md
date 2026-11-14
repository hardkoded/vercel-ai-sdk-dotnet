---
title: "Vercel.AI.Azure"
description: "Azure OpenAI provider."
editUrl: false
sidebar:
  label: "Vercel.AI.Azure"
---

[API reference](/api/)

Azure OpenAI provider.

## Classes

| Type | Summary |
| --- | --- |
| [AzureImageModel](/api/Vercel.AI.Azure/AzureImageModel/) | Azure image model. Routes each call to the MAI image API or the OpenAI image API. Resolves the API per call: providerOptions also reach this model through a gateway. Maps to AzureImageModel in azure-openai-provider.ts. |
| [AzureImageModelMaiOptions](/api/Vercel.AI.Azure/AzureImageModelMaiOptions/) | MAI image options, read from providerOptions.azure. Maps to AzureImageModelMaiOptions and azureMaiImageModelOptionsShape in azure-mai-image-model-options.ts. |
| [AzureImageModelOptions](/api/Vercel.AI.Azure/AzureImageModelOptions/) | Azure image options, read from providerOptions.azure. Maps to AzureImageModelOptions and azureImageModelOptions in azure-image-model-options.ts. |
| [AzureModelCatalog](/api/Vercel.AI.Azure/AzureModelCatalog/) | Azure-owned models. Maps to azureModelCatalog in azure-model-catalog.ts. |
| [AzureModelRequestConfig](/api/Vercel.AI.Azure/AzureModelRequestConfig/) | Request routing of an Azure-owned model. Maps to AzureRequestConfig in azure-model-catalog.ts. |
| [AzureOpenAILanguageModelSettings](/api/Vercel.AI.Azure/AzureOpenAILanguageModelSettings/) | Settings for an Azure OpenAI language model. |
| [AzureOpenAIOptions](/api/Vercel.AI.Azure/AzureOpenAIOptions/) | Azure OpenAI settings. |
| [AzureOpenAIProvider](/api/Vercel.AI.Azure/AzureOpenAIProvider/) | Azure OpenAI provider. Calls use the /openai/v1 routes and the api-key header, or the deployments routes when UseDeploymentBasedUrls is set. |
| [AzureOpenAIServiceCollectionExtensions](/api/Vercel.AI.Azure/AzureOpenAIServiceCollectionExtensions/) | Registers Azure OpenAI. |
| [AzureSpeechModel](/api/Vercel.AI.Azure/AzureSpeechModel/) | Azure speech model. Resolves the API for every call: providerOptions.azure.api wins, and otherwise MAI-Voice models use the Azure Speech API and every other model uses the OpenAI speech API. Maps to AzureSpeechModel in azure-openai-provider.ts. |
| [AzureSpeechModelOptions](/api/Vercel.AI.Azure/AzureSpeechModelOptions/) | Azure speech model options, read from the azure provider options. They combine the OpenAI speech options and the Azure Speech options. Maps to AzureSpeechModelOptions in azure-speech-model-options.ts. |
| [AzureSpeechSpeechModel](/api/Vercel.AI.Azure/AzureSpeechSpeechModel/) | Azure Speech text to speech with SSML. The model is appended to the voice name, for example en-US-Harper:MAI-Voice-2. Maps to AzureSpeechSpeechModel in azure-speech-speech-model.ts. |
| [AzureSpeechSpeechModelOptions](/api/Vercel.AI.Azure/AzureSpeechSpeechModelOptions/) | Azure Speech text to speech options, read from the azure provider options. Maps to AzureSpeechModelSpeechOptions in azure-speech-speech-model-options.ts. |
| [AzureTranscriptionModel](/api/Vercel.AI.Azure/AzureTranscriptionModel/) | Azure transcription model. It resolves the API for each call, because providerOptions also reach this model through the gateway: openai for Azure OpenAI deployments, speech for the MAI-Transcribe Speech API and mai for MAI streaming transcription. Maps to AzureTranscriptionModel in azure-openai-provider.ts. |
