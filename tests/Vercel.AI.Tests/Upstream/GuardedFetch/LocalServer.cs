// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Vercel.AI.Tests.Upstream.GuardedFetch;

/// <summary>Minimal HTTP/1.1 server on 127.0.0.1 that records each request's path and headers. Replaces <c>createServer</c>.</summary>
internal sealed class LocalServer : IDisposable
{
    private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
    private readonly CancellationTokenSource _stop = new();
    private readonly Task _loop;

    public LocalServer(Func<string, (int Status, string? Location, string Body)> handler)
    {
        _listener.Start();
        Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
        _loop = Task.Run(async () =>
        {
            while (!_stop.IsCancellationRequested)
            {
                TcpClient client;
                try
                {
                    client = await _listener.AcceptTcpClientAsync(_stop.Token);
                }
                catch
                {
                    return;
                }

                _ = Task.Run(() => ServeAsync(client, handler));
            }
        });
    }

    public int Port { get; }

    public List<(string Path, Dictionary<string, string> Headers)> Received { get; } = [];

    public void Dispose()
    {
        _stop.Cancel();
        _listener.Stop();
        try
        {
            _loop.Wait(1000);
        }
        catch (AggregateException) { }

        _stop.Dispose();
    }

    private async Task ServeAsync(
        TcpClient client,
        Func<string, (int Status, string? Location, string Body)> handler
    )
    {
        using (client)
        {
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.ASCII, false, 1024, true);
            var requestLine = await reader.ReadLineAsync();
            if (requestLine == null)
            {
                return;
            }

            var path = requestLine.Split(' ')[1];
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string? line;
            while (!string.IsNullOrEmpty(line = await reader.ReadLineAsync()))
            {
                var index = line.IndexOf(':');
                headers[line[..index]] = line[(index + 1)..].Trim();
            }

            lock (Received)
            {
                Received.Add((path, headers));
            }

            var (status, location, body) = handler(path);
            var bytes = Encoding.UTF8.GetBytes(body);
            var head =
                $"HTTP/1.1 {status} X\r\nconnection: close\r\ncontent-length: {bytes.Length}\r\n"
                + (location == null ? string.Empty : $"location: {location}\r\n")
                + "\r\n";
            await stream.WriteAsync(Encoding.ASCII.GetBytes(head));
            await stream.WriteAsync(bytes);
        }
    }
}
