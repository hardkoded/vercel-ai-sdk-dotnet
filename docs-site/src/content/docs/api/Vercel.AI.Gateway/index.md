---
title: "Vercel.AI.Gateway"
description: "Vercel AI Gateway provider."
editUrl: false
sidebar:
  label: "Vercel.AI.Gateway"
---

[API reference](/api/)

Vercel AI Gateway provider.

## Classes

| Type | Summary |
| --- | --- |
| [GatewayAccessPolicyDeniedError](/api/Vercel.AI.Gateway/GatewayAccessPolicyDeniedError/) | An AI Gateway access policy that applies to the request does not allow the requested model or provider. |
| [GatewayAuthenticationError](/api/Vercel.AI.Gateway/GatewayAuthenticationError/) | Authentication failed. |
| [GatewayBatchApi](/api/Vercel.AI.Gateway/GatewayBatchApi/) | AI Gateway batch API. Downloads the JSONL results of a batch. |
| [GatewayConfig](/api/Vercel.AI.Gateway/GatewayConfig/) | Configuration shared by Gateway fetchers (GatewayConfig). |
| [GatewayCreditsResponse](/api/Vercel.AI.Gateway/GatewayCreditsResponse/) | The Gateway credit balance. Maps to GatewayCreditsResponse. |
| [GatewayDecisionModel](/api/Vercel.AI.Gateway/GatewayDecisionModel/) | Answers questions through the Gateway /decision-model route. |
| [GatewayEmbeddingModel](/api/Vercel.AI.Gateway/GatewayEmbeddingModel/) | Gateway embedding model. |
| [GatewayError](/api/Vercel.AI.Gateway/GatewayError/) | Base class for AI Gateway errors. The message includes [generationId] when one was returned. |
| [GatewayErrors](/api/Vercel.AI.Gateway/GatewayErrors/) | Parses Gateway auth headers and error bodies. |
| [GatewayExtractionModel](/api/Vercel.AI.Gateway/GatewayExtractionModel/) | Extracts typed field values through the Gateway /extraction-model route. Maps to GatewayExtractionModel. |
| [GatewayFailedDependencyError](/api/Vercel.AI.Gateway/GatewayFailedDependencyError/) | A dependency required by the Gateway request was unavailable. Status 424 is not retried. |
| [GatewayFetchMetadata](/api/Vercel.AI.Gateway/GatewayFetchMetadata/) | Fetches the available models (GET /config) and the credits (GET /v1/credits) from the Gateway. |
| [GatewayFetchMetadataResponse](/api/Vercel.AI.Gateway/GatewayFetchMetadataResponse/) | The models the Gateway offers. Maps to GatewayFetchMetadataResponse. |
| [GatewayForbiddenError](/api/Vercel.AI.Gateway/GatewayForbiddenError/) | The request was rejected by policy. |
| [GatewayGenerationInfo](/api/Vercel.AI.Gateway/GatewayGenerationInfo/) | Information about a Gateway generation. |
| [GatewayGenerationInfoFetcher](/api/Vercel.AI.Gateway/GatewayGenerationInfoFetcher/) | Fetches generation info (GET /v1/generation?id=) from the Gateway. |
| [GatewayGenerationInfoParams](/api/Vercel.AI.Gateway/GatewayGenerationInfoParams/) | Parameters for CancellationToken). |
| [GatewayImageModel](/api/Vercel.AI.Gateway/GatewayImageModel/) | Gateway image model. |
| [GatewayInternalServerError](/api/Vercel.AI.Gateway/GatewayInternalServerError/) | The Gateway failed internally. |
| [GatewayInvalidRequestError](/api/Vercel.AI.Gateway/GatewayInvalidRequestError/) | The Gateway rejected the request. |
| [GatewayLanguageModel](/api/Vercel.AI.Gateway/GatewayLanguageModel/) | Gateway language model. The request body is the V4 call options. |
| [GatewayLanguageModelEntry](/api/Vercel.AI.Gateway/GatewayLanguageModelEntry/) | A model the Gateway offers. Maps to GatewayLanguageModelEntry. |
| [GatewayLanguageModelSpecification](/api/Vercel.AI.Gateway/GatewayLanguageModelSpecification/) | The AI SDK specification of a Gateway model. Maps to GatewayLanguageModelSpecification. |
| [GatewayMintedClientSecret](/api/Vercel.AI.Gateway/GatewayMintedClientSecret/) | A client secret minted through the Gateway. ExpiresAt is epoch seconds. |
| [GatewayModelNotFoundError](/api/Vercel.AI.Gateway/GatewayModelNotFoundError/) | The requested model is not available. |
| [GatewayModelPricing](/api/Vercel.AI.Gateway/GatewayModelPricing/) | Pricing of a Gateway model, in USD per token. Maps to the pricing of GatewayLanguageModelEntry. |
| [GatewayNotFoundError](/api/Vercel.AI.Gateway/GatewayNotFoundError/) | A Gateway resource was not found. |
| [GatewayOptions](/api/Vercel.AI.Gateway/GatewayOptions/) | AI Gateway settings. The default base URL is https://ai-gateway.vercel.sh/v4/ai. |
| [GatewayProvider](/api/Vercel.AI.Gateway/GatewayProvider/) | Vercel AI Gateway provider. Language, embedding, image, and video calls use the Gateway V4 routes (/language-model, /embedding-model, /image-model, /video-model). |
| [GatewayRateLimitError](/api/Vercel.AI.Gateway/GatewayRateLimitError/) | A Gateway rate limit. |
| [GatewayRealtimeAuth](/api/Vercel.AI.Gateway/GatewayRealtimeAuth/) | Shared WebSocket subprotocol contract for AI Gateway realtime, streaming transcription, and streaming speech auth. Maps to gateway-realtime-auth.ts. The Gateway auth (bearer) token travels through the Sec-WebSocket-Protocol handshake because a browser WebSocket cannot set request headers. |
| [GatewayRealtimeClientSecret](/api/Vercel.AI.Gateway/GatewayRealtimeClientSecret/) | A client secret together with the WebSocket URL to open. Maps to RealtimeModelV4ClientSecretResult. |
| [GatewayRealtimeModel](/api/Vercel.AI.Gateway/GatewayRealtimeModel/) | Realtime model backed by the AI Gateway. The Gateway normalizes realtime like every other modality, so this model is a thin identity codec over the normalized protocol; only the connection and authentication are Gateway-specific. |
| [GatewayRealtimeModelConfig](/api/Vercel.AI.Gateway/GatewayRealtimeModelConfig/) | Configuration of a GatewayRealtimeModel. Maps to GatewayRealtimeModelConfig. |
| [GatewayRealtimeModelId](/api/Vercel.AI.Gateway/GatewayRealtimeModelId/) | Known AI Gateway realtime model ids. Maps to GatewayRealtimeModelId; any other string is accepted too. |
| [GatewayRealtimeWebSocketConfig](/api/Vercel.AI.Gateway/GatewayRealtimeWebSocketConfig/) | URL and subprotocols a client opens a WebSocket with. Maps to the result of getWebSocketConfig. |
| [GatewayResponseError](/api/Vercel.AI.Gateway/GatewayResponseError/) | The Gateway error body could not be read. |
| [GatewayServiceCollectionExtensions](/api/Vercel.AI.Gateway/GatewayServiceCollectionExtensions/) | Registers the Gateway provider. |
| [GatewaySpeechFactoryGetTokenResult](/api/Vercel.AI.Gateway/GatewaySpeechFactoryGetTokenResult/) | The result of minting a streaming speech client secret. Maps to GatewaySpeechFactoryGetTokenResult. |
| [GatewaySpeechModel](/api/Vercel.AI.Gateway/GatewaySpeechModel/) | Gateway streaming speech model. Maps to GatewaySpeechModel in gateway-speech-model.ts. Only the URL helper used by experimentalspeech.getToken is ported here; the model itself is tracked in #752. |
| [GatewaySpendReport](/api/Vercel.AI.Gateway/GatewaySpendReport/) | Fetches a spend report (GET /v1/report) from the Gateway. |
| [GatewaySpendReportParams](/api/Vercel.AI.Gateway/GatewaySpendReportParams/) | Parameters of CancellationToken). Maps to GatewaySpendReportParams. |
| [GatewaySpendReportResponse](/api/Vercel.AI.Gateway/GatewaySpendReportResponse/) | A spend report. Maps to GatewaySpendReportResponse. |
| [GatewaySpendReportRow](/api/Vercel.AI.Gateway/GatewaySpendReportRow/) | A row of a spend report. Maps to GatewaySpendReportRow. |
| [GatewayTakoSearch](/api/Vercel.AI.Gateway/GatewayTakoSearch/) | JSON schemas for the Gateway Tako search tool. |
| [GatewayTakoSearchConfig](/api/Vercel.AI.Gateway/GatewayTakoSearchConfig/) | Developer-only configuration of the Gateway Tako search tool. These options are not part of the tool input, so the model cannot see or set them. The Gateway applies the defaults and validates them. Maps to TakoSearchConfig. |
| [GatewayTimeoutError](/api/Vercel.AI.Gateway/GatewayTimeoutError/) | The client timed out before the Gateway responded. |
| [GatewayToolError](/api/Vercel.AI.Gateway/GatewayToolError/) | Error returned as the output of a Gateway provider-executed tool instead of a result. Maps to GatewayToolError. |
| [GatewayTranscriptionFactoryGetTokenResult](/api/Vercel.AI.Gateway/GatewayTranscriptionFactoryGetTokenResult/) | The result of minting a streaming transcription client secret. Maps to GatewayTranscriptionFactoryGetTokenResult. |
| [GatewayTranscriptionModel](/api/Vercel.AI.Gateway/GatewayTranscriptionModel/) | Gateway transcription model, routed through /transcription-model. Maps to GatewayTranscriptionModel. Streaming transcription runs over the /transcription-model WebSocket. |
| [GatewayVideoModel](/api/Vercel.AI.Gateway/GatewayVideoModel/) | Gateway video model. CancellationToken) reads the first data event of an SSE response from /video-model. CancellationToken) and CancellationToken) use /video-model/start and /video-model/status. Maps to GatewayVideoModel in gateway-video-model.ts. |
| [TakoCard](/api/Vercel.AI.Gateway/TakoCard/) | A card of a Tako search response. Maps to TakoCard. |
| [TakoCardSource](/api/Vercel.AI.Gateway/TakoCardSource/) | A source of a TakoCard. |
| [TakoCostUsage](/api/Vercel.AI.Gateway/TakoCostUsage/) | Cost of one part of a Tako search. Maps to TakoSearchResponse['usage']['compute'] and ['variablecost']. |
| [TakoDataFreshness](/api/Vercel.AI.Gateway/TakoDataFreshness/) | Freshness of the data of a TakoCard. |
| [TakoDataset](/api/Vercel.AI.Gateway/TakoDataset/) | Inline dataset of a result content. Maps to TakoResultContent['dataset']. |
| [TakoDatasetColumn](/api/Vercel.AI.Gateway/TakoDatasetColumn/) | A column of a TakoDataset. |
| [TakoDatasetSource](/api/Vercel.AI.Gateway/TakoDatasetSource/) | A source of a TakoDataset. |
| [TakoDataSourceConfig](/api/Vercel.AI.Gateway/TakoDataSourceConfig/) | Options of the curated data source. Maps to TakoDataSourceConfig. |
| [TakoDataUsage](/api/Vercel.AI.Gateway/TakoDataUsage/) | Data cost of a Tako search. Maps to TakoSearchResponse['usage']['data']. |
| [TakoExportPricing](/api/Vercel.AI.Gateway/TakoExportPricing/) | Pricing of exporting a card's rows. Maps to TakoResultContent['exportpricing']. |
| [TakoLocation](/api/Vercel.AI.Gateway/TakoLocation/) | End-user coordinates for localized results. Maps to TakoSearchConfig['location']. |
| [TakoManifestColumn](/api/Vercel.AI.Gateway/TakoManifestColumn/) | A column of the manifest of a result content. |
| [TakoMethodology](/api/Vercel.AI.Gateway/TakoMethodology/) | A methodology of a TakoCard. |
| [TakoMetricDefinition](/api/Vercel.AI.Gateway/TakoMetricDefinition/) | A metric definition of a TakoCard. |
| [TakoNode](/api/Vercel.AI.Gateway/TakoNode/) | A Data Graph node of a TakoCard. |
| [TakoOutputSettings](/api/Vercel.AI.Gateway/TakoOutputSettings/) | Settings that control card rendering. Maps to TakoSearchConfig['outputSettings']. |
| [TakoPlaceholder](/api/Vercel.AI.Gateway/TakoPlaceholder/) | Set when Tako withholds a card's data. Maps to TakoCard['placeholder']. |
| [TakoResultContent](/api/Vercel.AI.Gateway/TakoResultContent/) | Content of a card or web result. Maps to TakoResultContent. |
| [TakoSearchOutput](/api/Vercel.AI.Gateway/TakoSearchOutput/) | Output of the Tako search tool: a TakoSearchResponse or a GatewayToolError. Maps to TakoSearchOutput (TakoSearchError \| TakoSearchResponse, where TakoSearchError is GatewayToolError). |
| [TakoSearchResponse](/api/Vercel.AI.Gateway/TakoSearchResponse/) | Output of the Tako search tool. Deserialize the tool output into it. Maps to TakoSearchResponse. |
| [TakoSearchSourcesConfig](/api/Vercel.AI.Gateway/TakoSearchSourcesConfig/) | Sources to search. Maps to TakoSearchConfig['sources']. |
| [TakoUsage](/api/Vercel.AI.Gateway/TakoUsage/) | Usage of a Tako search. Maps to TakoSearchResponse['usage']. |
| [TakoWebResult](/api/Vercel.AI.Gateway/TakoWebResult/) | A web result of a Tako search response. Maps to TakoWebResult. |
| [TakoWebSourceConfig](/api/Vercel.AI.Gateway/TakoWebSourceConfig/) | Options of the web source. Maps to TakoWebSourceConfig. |
| [V4Json](/api/Vercel.AI.Gateway/V4Json/) | Serializes call options and parses results in the Language Model V4 JSON shape the Gateway expects. |
| [VercelRequestContext](/api/Vercel.AI.Gateway/VercelRequestContext/) | The ambient Vercel request context. Maps to the @vercel/request-context object that Vercel stores on globalThis[Symbol.for('@vercel/request-context')] and that getContext in @vercel/oidc reads. A .NET host supplies it with String%7d), for example from ASP.NET Core middleware. |

## Structs

| Type | Summary |
| --- | --- |
| [TakoDatasetCell](/api/Vercel.AI.Gateway/TakoDatasetCell/) | A dataset cell: a boolean, number, string or null. Maps to TakoDatasetCell. |

## Enums

| Type | Summary |
| --- | --- |
| [GatewayCredentialType](/api/Vercel.AI.Gateway/GatewayCredentialType/) | The kind of credentials a request used. |
| [GatewaySpendReportDatePart](/api/Vercel.AI.Gateway/GatewaySpendReportDatePart/) | The time granularity of a spend report grouped by day. |
| [GatewaySpendReportGroupBy](/api/Vercel.AI.Gateway/GatewaySpendReportGroupBy/) | The primary aggregation dimension of a spend report. |
