// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.Upstream.ProviderUtils;

/// <summary>Test double for the upstream <c>MockWebSocket</c> classes.</summary>
internal sealed class MockWebSocket : IWebSocketLike
{
    private readonly object _gate = new object();
    private readonly List<object> _sent = new List<object>();
    private readonly List<int?> _closes = new List<int?>();

    public MockWebSocket(Uri url, IReadOnlyList<string>? protocols, IReadOnlyDictionary<string, string>? headers)
    {
        Url = url;
        Protocols = protocols;
        Headers = headers;
    }

    public static List<MockWebSocket> Instances { get; } = new List<MockWebSocket>();

    public Uri Url { get; }

    public IReadOnlyList<string>? Protocols { get; }

    public IReadOnlyDictionary<string, string>? Headers { get; }

    public int ReadyState { get; set; }

    public long? BufferedAmount { get; set; } = 0;

    public Action<object?>? OnOpen { get; set; }

    public Action<object?>? OnMessage { get; set; }

    public Action<object?>? OnError { get; set; }

    public Action<object?>? OnClose { get; set; }

    public Action? CloseHook { get; set; }

    public IReadOnlyList<object> Sent
    {
        get
        {
            lock (_gate)
            {
                return _sent.ToList();
            }
        }
    }

    public IReadOnlyList<int?> Closes
    {
        get
        {
            lock (_gate)
            {
                return _closes.ToList();
            }
        }
    }

    public static WebSocketFactory Factory
    {
        get
        {
            return (url, protocols, headers) =>
            {
                var socket = new MockWebSocket(url, protocols, headers);
                lock (Instances)
                {
                    Instances.Add(socket);
                }

                return socket;
            };
        }
    }

    public static void Reset()
    {
        lock (Instances)
        {
            Instances.Clear();
        }
    }

    public void Send(string data)
    {
        lock (_gate)
        {
            _sent.Add(data);
        }
    }

    public void Send(byte[] data)
    {
        lock (_gate)
        {
            _sent.Add(data);
        }
    }

    public void Close(int? code = null, string? reason = null)
    {
        lock (_gate)
        {
            _closes.Add(code);
        }

        ReadyState = 3;
        CloseHook?.Invoke();
    }

    public void Open()
    {
        ReadyState = 1;
        OnOpen?.Invoke(null);
    }

    public void Message(JsonNode value)
    {
        OnMessage?.Invoke(value.ToJsonString());
    }

    public void ServerClose(int code = 1000, string? reason = null)
    {
        ReadyState = 3;
        OnClose?.Invoke(new WebSocketCloseEvent(code, reason));
    }
}

internal static class Wait
{
    public static async Task UntilAsync(Func<bool> condition, int timeoutMs = 10000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (!condition())
        {
            if (DateTime.UtcNow > deadline)
            {
                throw new TimeoutException("Condition was not met.");
            }

            await Task.Delay(5);
        }
    }

    public static Task FlushAsync()
    {
        return Task.Delay(50);
    }
}
