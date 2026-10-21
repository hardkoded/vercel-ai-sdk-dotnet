# Secure URL fetching

When the SDK downloads a URL, for example a file URL in a prompt that the model cannot read itself, it validates the URL first (`Download.ValidateDownloadUrl` and `DownloadUrls.ValidateDownloadUrl`).

The SDK:

- **Rejects private, loopback, and link-local targets.** This covers IPv4 (`10/8`, `172.16/12`, `192.168/16`, `127/8`, `169.254/16`, CGNAT, multicast, and so on) and the equivalent IPv6 ranges, plus `localhost`, `.local`, `.localhost`, and `.internal` hostnames. Hostnames are compared without case, a trailing dot, or a port. Non-`http(s)` schemes are rejected, except `data:` URLs, which the SDK accepts and decodes locally.
- **Re-validates every inspectable redirect hop.** A URL that passes but then redirects to an internal address is blocked. An opaque redirect, whose destination the SDK cannot read, is blocked too, unless a browser-style runtime opts in to following it natively. In that case the SDK cannot validate the destination.

The `.internal` rule is defense in depth. It does not replace network-level protection and does not solve DNS rebinding.

## Fetching with redirect validation

`Download.FetchWithValidatedRedirectsAsync` is the helper behind `Download.GetAsync`. It requests one hop at a time, with `redirect` set to `manual`, and validates each URL before it requests it. It returns the final, non-redirect response. The caller checks `Ok` and reads the body. It throws `DownloadError` when a hop is unsafe, when the redirect limit (`maxRedirects`, default 10) is exceeded, or when an opaque redirect cannot be validated.

```csharp
var response = await Download.FetchWithValidatedRedirectsAsync(
    "https://example.com/file",
    headers,
    cancellationToken,
    trustedOrigin: "https://example.com");
```

- **Headers.** Each hop receives its own copy of the headers. A redirect that crosses origin drops every header except `user-agent`, and the dropped headers do not return when a later hop comes back to the first origin.
- **`trustedOrigin`.** A hop on this origin skips validation. Use it for an origin that you configure, such as a provider base URL that points to `localhost` or an internal host. Never build it from response data. A hop on any other origin is always validated, even when `trustedOrigin` is set.
