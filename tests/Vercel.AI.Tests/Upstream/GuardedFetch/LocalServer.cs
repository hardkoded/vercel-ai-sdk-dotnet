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
        : this(request =>
        {
            var (status, location, body) = handler(request.Path);
            return new LocalResponse(status, location, body);
        }) { }

    public LocalServer(Func<LocalRequest, LocalResponse> handler)
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

    private async Task ServeAsync(TcpClient client, Func<LocalRequest, LocalResponse> handler)
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

            var parts = requestLine.Split(' ');
            var path = parts[1];
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string? line;
            while (!string.IsNullOrEmpty(line = await reader.ReadLineAsync()))
            {
                var index = line.IndexOf(':');
                headers[line[..index]] = line[(index + 1)..].Trim();
            }

            var body = string.Empty;
            if (headers.TryGetValue("content-length", out var length) && int.Parse(length) > 0)
            {
                var buffer = new char[int.Parse(length)];
                var read = 0;
                while (read < buffer.Length)
                {
                    var count = await reader.ReadAsync(buffer, read, buffer.Length - read);
                    if (count == 0)
                    {
                        break;
                    }

                    read += count;
                }

                body = new string(buffer, 0, read);
            }

            lock (Received)
            {
                Received.Add((path, headers));
            }

            var response = handler(new LocalRequest(parts[0], path, headers, body));
            var bytes = response.Bytes ?? Encoding.UTF8.GetBytes(response.Body);
            var head =
                $"HTTP/1.1 {response.Status} X\r\nconnection: close\r\ncontent-length: {bytes.Length}\r\n"
                + (response.Location == null ? string.Empty : $"location: {response.Location}\r\n")
                + (
                    response.ContentType == null
                        ? string.Empty
                        : $"content-type: {response.ContentType}\r\n"
                )
                + "\r\n";
            await stream.WriteAsync(Encoding.ASCII.GetBytes(head));
            await stream.WriteAsync(bytes);
        }
    }
}

internal sealed record LocalRequest(
    string Method,
    string Path,
    Dictionary<string, string> Headers,
    string Body
);

internal sealed record LocalResponse(
    int Status,
    string? Location = null,
    string Body = "",
    string? ContentType = null,
    byte[]? Bytes = null
);
