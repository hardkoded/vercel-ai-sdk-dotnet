// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;

namespace Vercel.AI.Util;

/// <summary>
/// Resolves a hostname and validates every returned address. Maps to <c>createSafeLookup</c> in <c>safe-node-fetch.ts</c>.
/// </summary>
internal static class SafeLookup
{
    /// <summary>Resolves a hostname to its addresses.</summary>
    internal delegate Task<IPAddress[]> Resolver(string hostname, CancellationToken cancellationToken);

    /// <summary>
    /// Resolves <paramref name="hostname"/> and throws <see cref="DownloadError"/> when it has no address or when any address
    /// is private or reserved. The default fetch connects to the returned addresses, so a second lookup cannot rebind them.
    /// </summary>
    public static async Task<IPAddress[]> ResolveValidatedAsync(string hostname, Resolver resolver, CancellationToken cancellationToken)
    {
        var addresses = await resolver(hostname, cancellationToken).ConfigureAwait(false);
        if (addresses == null || addresses.Length == 0)
        {
            throw new DownloadError(hostname, "Hostname " + hostname + " did not resolve to an address");
        }

        foreach (var address in addresses)
        {
            ValidateDownloadAddress(address, hostname);
        }

        return addresses;
    }

    /// <summary>Throws <see cref="DownloadError"/> when <paramref name="address"/> is private or reserved.</summary>
    public static void ValidateDownloadAddress(IPAddress address, string hostname)
    {
        var text = address.ToString();
        var isUnsafe = address.AddressFamily == AddressFamily.InterNetwork
            ? !Download.IsIPv4(text) || Download.IsPrivateIPv4(text)
            : address.AddressFamily == AddressFamily.InterNetworkV6
                ? Download.IsPrivateIPv6(text)
                : true;

        if (isUnsafe)
        {
            throw new DownloadError(hostname, "Hostname " + hostname + " resolved to disallowed IP address " + text);
        }
    }

    /// <summary>The system resolver.</summary>
    public static Task<IPAddress[]> SystemResolveAsync(string hostname, CancellationToken cancellationToken)
    {
#if NET6_0_OR_GREATER
        return Dns.GetHostAddressesAsync(hostname, cancellationToken);
#else
        cancellationToken.ThrowIfCancellationRequested();
        return Dns.GetHostAddressesAsync(hostname);
#endif
    }

#if NET5_0_OR_GREATER
    /// <summary>
    /// A handler that resolves, validates, and then connects to the validated address itself, so the socket is pinned to the
    /// validated result. It does not follow redirects.
    /// </summary>
    public static HttpMessageHandler CreatePinnedHandler()
    {
        return new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            ConnectCallback = ConnectValidatedAsync,
        };
    }

    private static async ValueTask<Stream> ConnectValidatedAsync(SocketsHttpConnectionContext context, CancellationToken cancellationToken)
    {
        var addresses = await ResolveValidatedAsync(context.DnsEndPoint.Host, SystemResolveAsync, cancellationToken).ConfigureAwait(false);
        Exception? last = null;
        foreach (var address in addresses)
        {
            var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
            try
            {
                await socket.ConnectAsync(new IPEndPoint(address, context.DnsEndPoint.Port), cancellationToken).ConfigureAwait(false);
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

        throw new HttpRequestException(
            string.Format(CultureInfo.InvariantCulture, "Could not connect to {0}.", context.DnsEndPoint.Host),
            last);
    }
#endif
}
