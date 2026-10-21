# Secure URL fetching

When the SDK downloads a URL, for example a file URL in a prompt that the model cannot read itself, it validates the URL first (`Download.ValidateDownloadUrl` and `DownloadUrls.ValidateDownloadUrl`).

The SDK:

- **Rejects private, loopback, and link-local targets.** This covers IPv4 (`10/8`, `172.16/12`, `192.168/16`, `127/8`, `169.254/16`, CGNAT, multicast, and so on) and the equivalent IPv6 ranges, plus `localhost`, `.local`, `.localhost`, and `.internal` hostnames. Hostnames are compared without case, a trailing dot, or a port. Non-`http(s)` schemes are rejected too.
- **Re-validates every redirect hop.** A URL that passes but then redirects to an internal address is blocked.

The `.internal` rule is defense in depth. It does not replace network-level protection and does not solve DNS rebinding.
