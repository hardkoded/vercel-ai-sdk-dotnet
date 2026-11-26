// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Util;

/// <summary>
/// An opt-in download transport that validates every resolved address and pins the connection to the validated address.
/// Maps to <c>createGuardedFetch</c> of the <c>guarded-fetch</c> package. On net10.0 the check runs inside the connect
/// callback, so a DNS answer cannot change between validation and connection. On netstandard2.0 the addresses are resolved
/// and validated before the request, and the runtime resolves again when it connects, so rebinding is not prevented there.
/// Pass it as the <c>fetch</c> of <see cref="Download.CreateDownload"/>. Dispose it to release pooled connections.
/// It does not use a proxy, because a proxy would resolve the target itself. Restrict egress at the platform level to use one.
/// Both targets throw <see cref="DownloadError"/> for an unsafe address.
/// </summary>
public sealed class GuardedFetch : IDisposable
{
    private readonly HttpClient _client;
    private readonly Func<string, CancellationToken, Task<IPAddress[]>> _resolve;
    private readonly string[] _trustedOrigins;

    /// <summary>Creates a guarded transport.</summary>
    /// <param name="trustedOrigins">
    /// Developer-configured origins, such as <c>http://localhost:11434</c>, whose connections skip address validation.
    /// Never build them from response data.
    /// </param>
    public GuardedFetch(IEnumerable<string>? trustedOrigins = null)
        : this(trustedOrigins, null) { }

    // The resolver is injectable so tests can answer with chosen addresses. It is the only DNS entry point.
    internal GuardedFetch(
        IEnumerable<string>? trustedOrigins,
        Func<string, CancellationToken, Task<IPAddress[]>>? resolve
    )
    {
        _resolve = resolve ?? ResolveAsync;
        _trustedOrigins = trustedOrigins == null ? [] : [.. trustedOrigins];
#if NET
        var handler = new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            UseProxy = false,
            ConnectCallback = ConnectAsync,
        };
#else
        var handler = new HttpClientHandler { AllowAutoRedirect = false, UseProxy = false };
#endif
        _client = new HttpClient(handler);
    }

    /// <summary>Requests <paramref name="url"/> with validated, pinned addresses.</summary>
    /// <exception cref="DownloadError">The host resolves to a local, private, or reserved address.</exception>
    public async Task<DownloadResponse> FetchAsync(string url, DownloadRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

#if !NET
        var uri = new Uri(url);
        if (!IsTrusted(uri.Host, uri.Port))
        {
            var addresses = await _resolve(uri.Host, request.AbortSignal).ConfigureAwait(false);
            AssertPublic(uri.Host, addresses);
        }
#endif
        try
        {
            return await Download.SendAsync(_client, url, request).ConfigureAwait(false);
        }
        catch (HttpRequestException error) when (error.InnerException is DownloadError inner)
        {
            // The runtime wraps an exception thrown by the connect callback.
            throw inner;
        }
    }

    /// <summary>Converts the transport to the <see cref="DownloadFetch"/> delegate.</summary>
    public static implicit operator DownloadFetch(GuardedFetch fetch)
    {
        if (fetch == null)
        {
            throw new ArgumentNullException(nameof(fetch));
        }

        return fetch.FetchAsync;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
    }

    /// <summary>
    /// Returns whether <paramref name="address"/> is a public address. Maps to <c>isSafeIpAddress</c> of <c>guarded-fetch</c>.
    /// </summary>
    public static bool IsSafeIpAddress(string address)
    {
        if (!IPAddress.TryParse(address, out var ip))
        {
            return false;
        }

        ip = ip.IsIPv6LinkLocal || !ip.IsIPv4MappedToIPv6 ? ip : ip.MapToIPv4();
        return ip.AddressFamily == AddressFamily.InterNetwork
            ? !Download.IsPrivateIPv4(ip.ToString())
            : !Download.IsPrivateIPv6(ip.ToString());
    }

    private static Task<IPAddress[]> ResolveAsync(string host, CancellationToken cancellationToken)
    {
#if NET
        return Dns.GetHostAddressesAsync(host, cancellationToken);
#else
        return Dns.GetHostAddressesAsync(host);
#endif
    }

    private static void AssertPublic(string host, IPAddress[] addresses)
    {
        if (addresses.Length == 0)
        {
            throw new DownloadError(host, "Hostname " + host + " did not resolve to an address");
        }

        foreach (var address in addresses)
        {
            if (!IsSafeIpAddress(address.ToString()))
            {
                throw new DownloadError(
                    host,
                    "Hostname "
                        + host
                        + " resolves to a private or reserved address, which is not allowed"
                );
            }
        }
    }

    private bool IsTrusted(string host, int port)
    {
        foreach (var origin in _trustedOrigins)
        {
            if (
                Uri.TryCreate(origin, UriKind.Absolute, out var trusted)
                && string.Equals(trusted.Host, host, StringComparison.OrdinalIgnoreCase)
                && trusted.Port == port
            )
            {
                return true;
            }
        }

        return false;
    }

#if NET
    private async ValueTask<Stream> ConnectAsync(
        SocketsHttpConnectionContext context,
        CancellationToken cancellationToken
    )
    {
        var endpoint = context.DnsEndPoint;
        var addresses = await _resolve(endpoint.Host, cancellationToken).ConfigureAwait(false);
        if (!IsTrusted(endpoint.Host, endpoint.Port))
        {
            AssertPublic(endpoint.Host, addresses);
        }

        // Connect to the addresses that were just validated, so a second lookup cannot return another answer.
        Exception? last = null;
        foreach (var address in addresses)
        {
            var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true,
            };
            try
            {
                await socket
                    .ConnectAsync(new IPEndPoint(address, endpoint.Port), cancellationToken)
                    .ConfigureAwait(false);
                return new NetworkStream(socket, ownsSocket: true);
            }
            catch (Exception error) when (error is SocketException)
            {
                socket.Dispose();
                last = error;
            }
            catch
            {
                socket.Dispose();
                throw;
            }
        }

        throw last ?? new SocketException((int)SocketError.HostNotFound);
    }
#endif
}
