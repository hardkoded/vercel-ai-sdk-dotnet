# Secure URL fetching

When the SDK downloads a URL, for example a file URL in a prompt that the model cannot read itself, it validates the URL first (`Download.ValidateDownloadUrl` and `DownloadUrls.ValidateDownloadUrl`).

The SDK:

- **Rejects private, loopback, and link-local targets.** This covers IPv4 (`10/8`, `172.16/12`, `192.168/16`, `127/8`, `169.254/16`, CGNAT, multicast, and so on) and the equivalent IPv6 ranges, plus `localhost`, `.local`, `.localhost`, and `.internal` hostnames. Hostnames are compared without case, a trailing dot, or a port. Non-`http(s)` schemes are rejected, except `data:` URLs, which the SDK accepts and decodes locally.
- **Re-validates every inspectable redirect hop.** A URL that passes but then redirects to an internal address is blocked. An opaque redirect, whose destination the SDK cannot read, is blocked too, unless a browser-style runtime opts in to following it natively. In that case the SDK cannot validate the destination.
- **Rejects HTTPS downgrades on server-side downloads.** A redirect from HTTPS to HTTP is blocked with a `DownloadError` before the SDK requests the insecure target, even when that target matches `trustedOrigin`. Initial HTTP URLs, HTTP-to-HTTP redirects, and HTTP-to-HTTPS upgrades remain supported.

## What the SDK does not check

The default fetch does not validate resolved DNS addresses and does not pin connections. A hostname that passes URL validation but resolves to a private address, or a DNS rebinding attack, is not blocked by the SDK. This is a deliberate change: DNS protection is opt-in.

Applications that download untrusted URLs must pass a DNS-validating fetch (a `DownloadFetch` that resolves the host, rejects private and reserved addresses, and connects to the validated address) wherever the SDK accepts one, such as `Download.Fetch`, the `fetch` argument of `Download.GetAsync`, or `WebFetchTools.Fetch`. Otherwise enforce equivalent outbound network restrictions. A `trustedOrigin` skips URL validation for a developer-configured origin.

## GuardedFetch

`GuardedFetch` (`createGuardedFetch` of the `guarded-fetch` package upstream) is an opt-in transport that implements the DNS-validating fetch described above. It converts implicitly to a `DownloadFetch`, so it fits every place that takes one. Dispose it to release pooled connections.

```csharp
using var guardedFetch = new GuardedFetch();

var result = await Download.CreateDownload(fetch: guardedFetch)
    .InvokeAsync(new DownloadRequestItem(new Uri("https://example.com/file.png"), false));
```

On net10.0 the connect callback of `SocketsHttpHandler` resolves the host, rejects the connection when any resolved address is local, private, or reserved, and connects to the validated address, so a DNS answer cannot change between the check and the connection. On netstandard2.0 the addresses are resolved and validated before the request, but the runtime resolves again when it connects, so DNS rebinding is not prevented there. Restrict network egress on that target. `GuardedFetch` never uses a proxy, so a proxy variable such as `HTTPS_PROXY` is ignored. Restrict egress at the platform level if you need one.

`new GuardedFetch(trustedOrigins)` lists developer-configured origins, such as `http://localhost:11434`, whose connections skip the address check. A trusted origin only skips the address check. `Download.CreateDownload` and `Download.GetAsync` still reject `localhost` and private IP literals in the URL, so pass the same origin as `trustedOrigin` to `Download.FetchWithValidatedRedirectsAsync` or `Download.FetchWithValidatedEndpointAsync` to reach it. Never build the list from response data. `GuardedFetch.IsSafeIpAddress(address)` tells whether an address is public.

`GuardedFetch.Client` is the pinned `HttpClient` behind the transport. Pass it where a provider or MCP transport takes an `HttpClient`, and pass the transport itself as the download fetch, so API calls and response URL downloads use the same validated connections. `OpenAICompatibleOptions.DownloadFetch` (the provider `fetch` upstream) sets the fetch that Fal uses for response URL downloads.

```csharp
using var guardedFetch = new GuardedFetch(["https://fal.run"]);
var fal = new FalProvider(
    guardedFetch.Client,
    new OpenAICompatibleOptions { ApiKey = key, DownloadFetch = guardedFetch });
var mcp = new HttpMcpTransport(guardedFetch.Client, new Uri("https://mcp.example.com/mcp"));
```

The `.internal` rule is defense in depth. It does not replace network-level protection.

## Fetching with redirect validation

`Download.FetchWithValidatedRedirectsAsync` is the helper behind `Download.GetAsync`. It requests one hop at a time, with `redirect` set to `manual`, and validates each URL before it requests it. It returns the final, non-redirect response. The caller checks `Ok` and reads the body. It throws `DownloadError` when a hop is unsafe, when the redirect limit (`maxRedirects`, default 10) is exceeded, or when an opaque redirect cannot be validated.

```csharp
var response = await Download.FetchWithValidatedRedirectsAsync(
    "https://example.com/file",
    headers,
    cancellationToken,
    trustedOrigin: "https://example.com");
```

- **Headers.** Before the first request, the helper removes proxy, origin-spoofing, cloud-metadata, cookie, and hop-by-hop headers (`host`, `forwarded`, `via`, `x-forwarded-*`, `x-real-ip`, `metadata`, `metadata-flavor`, `cookie`, and similar). It keeps `authorization` and other credential headers, so the caller must make sure the first URL may receive them. Each hop receives its own copy of the headers. A redirect that crosses origin drops every header except `user-agent`, and the dropped headers do not return when a later hop comes back to the first origin.
- **`trustedOrigin`.** A hop on this origin skips validation. Use it for an origin that you configure, such as a provider base URL that points to `localhost` or an internal host. Never build it from response data. A hop on any other origin is always validated, even when `trustedOrigin` is set.
- **Cancelling redirect bodies.** The helper starts cancelling a redirect response's body and moves on. It does not wait for the cancel to finish.

## Fetching an untrusted URL

`Download.FetchUntrustedUrlAsync` (`fetchUntrustedUrl` upstream) is `FetchWithValidatedRedirectsAsync` with first-hop credential isolation. Without a `credentialedOrigin` (or a `trustedOrigin`, when `credentialedOrigin` is omitted) that matches the URL, only allowlisted request metadata (`accept`, `accept-language`, `range`, `user-agent`, conditional headers, tracing headers, and similar) goes out on the first hop. Add non-credential names with `untrustedFirstHopHeaders`. Both origins must come from developer configuration, never from response data. An explicit `credentialedOrigin` does not exempt the URL from validation.

```csharp
var response = await Download.FetchUntrustedUrlAsync(
    fileUrl,
    headers,
    credentialedOrigin: "https://api.example.com",
    fetch: guardedFetch);
```

## Fetching one endpoint

`Download.FetchWithValidatedEndpointAsync` validates one URL and requests it once, without following redirects. Use it for requests that carry credentials or a body. Pass a `DownloadRequest` with `Method`, `Headers`, and `Body`. By default a redirect is rejected (`redirect` is `error`). Pass `manual` only if you validate the `Location` target yourself before you request it. As with `FetchWithValidatedRedirectsAsync`, `trustedOrigin` skips validation for a developer-configured origin.

```csharp
var response = await Download.FetchWithValidatedEndpointAsync(
    "https://auth.example.com/token",
    new DownloadRequest(headers, cancellationToken, "manual") { Method = "POST", Body = formBytes });
```

Pass `requireDnsValidation: true` when the endpoint comes from untrusted data, as in MCP OAuth. Then an untrusted URL is rejected with a `DownloadError` unless you pass a custom `fetch` (such as `GuardedFetch`) that validates DNS at connection time and pins the connection. The default fetch and `Download.Fetch` do not count, even when you pass `Download.Fetch` explicitly. A URL on `trustedOrigin` skips the check, and so does a runtime where `Download.IsBrowser` is true.

## Generated file downloads

`GenerateTextOptions.ExperimentalDownload` (`experimental_download` upstream) also handles generated file URLs that the model returns, for example an image URL from an image-generation response. Generated file URLs are always downloaded, so a `null` result for one throws a `DownloadException`. Without the option, they are downloaded with `Download.GetAsync` and the default fetch. Pass `Download.CreateDownload(fetch: ...)` to use your own transport. `CreateDownload` returns a `CreatedDownload`. `InvokeAsync(IReadOnlyList<DownloadRequestItem>)` is the batch shape, which passes a URL the model supports through as `null`. `InvokeAsync(DownloadRequestItem)` downloads one URL. It converts implicitly to the `Download` option of `TranscribeRequest` and `GenerateVideoRequest`, and to the batch delegate of `ExperimentalDownload`, `DecideRequest.Download` and `ExtractRequest.Download`.

When `OpenAIProvider.Create` or `OpenAICompatibleProvider.Create` gets a handler, the provider's `HttpClient` also serves untrusted downloads (`downloadFetch ?? config.fetch` upstream) unless `DownloadHttpClient` is set. The URL and redirect checks still run.

```csharp
var result = await client.GenerateTextAsync(new GenerateTextOptions
{
    Model = model,
    Prompt = "Draw an otter.",
    ExperimentalDownload = Download.CreateDownload(fetch: guardedFetch),
});
```

`ExperimentalDownload` also handles URLs in the prompt. Before each model call, the SDK checks every file part that has a URL against `ILanguageModel.SupportedUrls` (`supportedUrls` upstream) and passes the batch to the download function with `IsUrlSupportedByModel` set. The function returns `null` for a URL the model reads itself and the bytes for every other URL. The batch is empty when the prompt has no URLs. Without the option, prompt files whose URL the model does not support are downloaded with the default function. The model of the step decides, so a `PrepareStep` model switch changes which URLs are downloaded. File parts in tool results (`ToolModelMessage.OutputContent`, and the tool results of assistant messages) are part of the same batch and are inlined with the downloaded bytes. Streamed file parts (`FilePart`) and streamed reasoning files (`ReasoningFilePart`, from a `ReasoningFileStreamPart`) are downloaded the same way as generated files, and `StreamTextResult.Files` holds the files of the last step.

For generated files, the download function is called once per file with a one-item batch. The request carries the call's `AbortSignal`, and `CreateDownload` forwards it, so cancellation and timeouts reach the download. The SDK does not validate the URL before it calls your function: `Download.CreateDownload` validates it, so a custom function must do the same.

## Google Vertex tool-result files and MaaS images

`GoogleVertexProvider` downloads HTTP(S) file URLs in tool results and sends them to Vertex as inline data. The size of each file is limited by `VertexOptions.ToolResultDownloads.MaxBytes` (`toolResultDownloads.maxBytes` upstream), 7 MiB by default. Direct `gs://` references are not downloaded for Gemini 3 and later models: Vertex fetches them (`image/png`, `image/jpeg`, `image/webp`, `application/pdf`, `text/plain`).

The downloads use `VertexOptions.DownloadFetch`, or `Download.Fetch`, or the default fetch, in that order. They never use the provider's `HttpClient`, so the Vertex API key or bearer token is not sent to the file host, and redirects are followed one validated hop at a time. For untrusted URLs, pass a DNS-validating `DownloadFetch`. Upstream forwards its one `fetch` option here. A .NET provider authenticates through its `HttpClient` handler, so the raw transport is a separate option.

`GoogleVertexMaasProvider.ImageModel` downloads an image URL on another origin without the provider credentials. Set `GoogleVertexPartnerOptions.DownloadHttpClient` to a client that does not follow redirects to use your own transport.

## Decision and extraction state downloads

`DecideRequest.Download` and `ExtractRequest.Download` (`experimental_download` upstream) are the only way URL files in `state` are downloaded. Without the hook, a URL file throws `DownloadError` before any download or model call. Inline bytes, base64 content, and data URLs need no hook. For untrusted URLs, pass `Download.CreateDownload(fetch: ...)` with a `DownloadFetch` that validates DNS at connection time and pins the connection to the validated addresses. A model's own transport does not cover these downloads. `Download.CreateDownload()` without a `fetch` explicitly opts into the default fetch.

```csharp
var result = await Extract.ExtractAsync(new ExtractRequest
{
    Model = openai.ExtractionModel("gpt-6-luna"),
    State = new DecisionStatePart[] { new DecisionFilePart("application/pdf", FilePartInput.FromUrl(new Uri("https://example.com/invoice.pdf"))) },
    Fields = fields,
    Download = Download.CreateDownload(fetch: guardedFetch),
});
```
