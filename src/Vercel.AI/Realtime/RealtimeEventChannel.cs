// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI;

/// <summary>The options of a <see cref="RealtimeEventChannel"/>. Maps to the constructor options of <c>RealtimeEventChannel</c>.</summary>
public sealed class RealtimeEventChannelOptions
{
    /// <summary>Creates the options.</summary>
    public RealtimeEventChannelOptions(
        IClientRealtimeModel model,
        Action<JsonNode?> send,
        Func<RealtimeModelV4ServerEvent, Task> onEvent,
        Action<Exception> onError
    )
    {
        Model = model ?? throw new ArgumentNullException(nameof(model));
        Send = send ?? throw new ArgumentNullException(nameof(send));
        OnEvent = onEvent ?? throw new ArgumentNullException(nameof(onEvent));
        OnError = onError ?? throw new ArgumentNullException(nameof(onError));
    }

    /// <summary>The model whose codec the channel uses.</summary>
    public IClientRealtimeModel Model { get; }

    /// <summary>Writes a serialized message to the connection.</summary>
    public Action<JsonNode?> Send { get; }

    /// <summary>Receives each normalized server event.</summary>
    public Func<RealtimeModelV4ServerEvent, Task> OnEvent { get; }

    /// <summary>Receives recoverable errors.</summary>
    public Action<Exception> OnError { get; }

    /// <summary>Receives errors after which the input protocol cannot continue.</summary>
    public Action<Exception>? OnFatalError { get; init; }

    /// <summary>The queue bound. Defaults to 512.</summary>
    public int? MaxPending { get; init; }

    /// <summary>The bound on the bytes of pending incoming messages. Defaults to 8 MiB.</summary>
    public int? MaxPendingBytes { get; init; }
}

/// <summary>
/// One codec queue per connection; retired connections cannot deliver late work. Maps to <c>RealtimeEventChannel</c>.
/// </summary>
public sealed class RealtimeEventChannel
{
    private readonly object _gate = new();
    private readonly RealtimeEventChannelOptions _options;
    private readonly Func<JsonElement, IReadOnlyList<RealtimeModelV4ServerEvent>> _parse;
    private volatile bool _active = true;
    private volatile bool _finishing;
    private Task _incoming = Task.CompletedTask;
    private Task _outgoing = Task.CompletedTask;
    private int _incomingCount;
    private long _incomingBytes;
    private int _outgoingCount;

    /// <summary>Creates a channel over one connection.</summary>
    public RealtimeEventChannel(RealtimeEventChannelOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _parse = options.Model.CreateServerEventParser() ?? options.Model.ParseServerEvent;
    }

    /// <summary>Whether incoming messages are still waiting. Maps to <c>hasPendingIncoming</c>.</summary>
    public bool HasPendingIncoming
    {
        get
        {
            lock (_gate)
            {
                return _incomingCount > 0;
            }
        }
    }

    /// <summary>Retires the channel. Maps to <c>dispose</c>.</summary>
    public void Dispose()
    {
        _active = false;
    }

    /// <summary>Stops accepting messages, but delivers already received terminal events. Maps to <c>finish</c>.</summary>
    public async Task FinishAsync()
    {
        StopWriting();
        Task incoming;
        lock (_gate)
        {
            incoming = _incoming;
        }

        await incoming.ConfigureAwait(false);
        Dispose();
    }

    /// <summary>Stops accepting outgoing and incoming messages. Maps to <c>stopWriting</c>.</summary>
    public void StopWriting()
    {
        _finishing = true;
    }

    /// <summary>Serializes and sends a client event in order. Maps to <c>send</c>.</summary>
    public Task Send(RealtimeModelV4ClientEvent clientEvent, Func<bool>? shouldSend = null)
    {
        if (!_active || _finishing)
        {
            throw new InvalidOperationException("Realtime connection is closed");
        }

        Task operation;
        lock (_gate)
        {
            if (_outgoingCount >= (_options.MaxPending ?? 512))
            {
                throw new InvalidOperationException("Realtime outgoing queue is full");
            }

            _outgoingCount++;
            operation = RunSendAsync(_outgoing, clientEvent, shouldSend);
            _outgoing = TrackSendAsync(operation, clientEvent);
        }

        return operation;
    }

    /// <summary>Queues a text message from the connection. Maps to <c>receive</c>.</summary>
    public void Receive(string data)
    {
        Receive(data ?? throw new ArgumentNullException(nameof(data)), data.Length * 2L, null);
    }

    /// <summary>Queues a binary message from the connection. Maps to <c>receive</c>.</summary>
    public void Receive(byte[] data)
    {
        Receive(null, (data ?? throw new ArgumentNullException(nameof(data))).Length, data);
    }

    /// <summary>Queues a blob message from the connection. Maps to <c>receive</c>.</summary>
    public void Receive(RealtimeBlob data)
    {
        Receive(null, (data ?? throw new ArgumentNullException(nameof(data))).Size, data);
    }

    private async Task RunSendAsync(
        Task previous,
        RealtimeModelV4ClientEvent clientEvent,
        Func<bool>? shouldSend
    )
    {
        await Task.Yield();
        await previous.ConfigureAwait(false);
        ThrowIfClosed();
        if (shouldSend?.Invoke() == false)
        {
            return;
        }

        ThrowIfClosed();
        var data = await _options.Model.SerializeClientEventAsync(clientEvent).ConfigureAwait(false);
        ThrowIfClosed();
        if (shouldSend?.Invoke() == false)
        {
            return;
        }

        ThrowIfClosed();
        if (data != null)
        {
            _options.Send(data);
        }
    }

    private async Task TrackSendAsync(Task operation, RealtimeModelV4ClientEvent clientEvent)
    {
        try
        {
            await operation.ConfigureAwait(false);
        }
        catch (Exception error)
        {
            if (!_finishing)
            {
                // Capture owners escalate automatic audio failures; manual sends stay recoverable.
                if (clientEvent.Type != "input-audio-append" && clientEvent.Type != "session-close")
                {
                    Report(error);
                }
            }
        }
        finally
        {
            lock (_gate)
            {
                _outgoingCount--;
            }
        }
    }

    private void ThrowIfClosed()
    {
        if (!_active || _finishing)
        {
            throw new InvalidOperationException("Realtime connection is closed");
        }
    }

    private void Receive(string? text, long bytes, object? binary)
    {
        if (!_active || _finishing)
        {
            return;
        }

        lock (_gate)
        {
            if (
                _incomingCount >= (_options.MaxPending ?? 512)
                || _incomingBytes + bytes > (_options.MaxPendingBytes ?? 8 * 1024 * 1024)
            )
            {
                _finishing = true;
                // Reported outside the lock below.
            }
            else
            {
                _incomingCount++;
                _incomingBytes += bytes;
                _incoming = ProcessIncomingAsync(_incoming, text, binary, bytes);
                return;
            }
        }

        Fatal(
            new InvalidOperationException(
                "Realtime incoming queue is full; input protocol continuity was lost"
            )
        );
    }

    private async Task ProcessIncomingAsync(
        Task previous,
        string? text,
        object? binary,
        long bytes
    )
    {
        await Task.Yield();
        await previous.ConfigureAwait(false);
        try
        {
            if (!_active)
            {
                return;
            }

            text ??=
                binary is RealtimeBlob blob
                    ? await blob.TextAsync().ConfigureAwait(false)
                    : Encoding.UTF8.GetString((byte[])binary!);
            JsonElement value;
            try
            {
                using var document = JsonDocument.Parse(text);
                value = document.RootElement.Clone();
            }
            catch (JsonException error)
            {
                if (!_active)
                {
                    return;
                }

                throw new AiSdkException("Invalid JSON in realtime server message", error);
            }

            if (!_active)
            {
                return;
            }

            var health = _options.Model.GetHealthCheckResponse(value);
            if (!_active)
            {
                return;
            }

            if (health != null && !_finishing)
            {
                _options.Send(health);
            }

            if (!_active)
            {
                return;
            }

            foreach (var serverEvent in _parse(value))
            {
                if (!_active)
                {
                    return;
                }

                try
                {
                    await _options.OnEvent(serverEvent).ConfigureAwait(false);
                }
                catch (Exception error)
                {
                    Report(error);
                }
            }
        }
        catch (Exception error)
        {
            _finishing = true;
            Fatal(error);
        }
        finally
        {
            lock (_gate)
            {
                _incomingCount--;
                _incomingBytes -= bytes;
            }
        }
    }

    private void Fatal(Exception error)
    {
        if (!_active)
        {
            return;
        }

        if (_options.OnFatalError != null)
        {
            _options.OnFatalError(error);
        }
        else
        {
            Report(error);
        }
    }

    private void Report(Exception error)
    {
        if (!_active)
        {
            return;
        }

        try
        {
            _options.OnError(error);
        }
        catch
        {
            // Application callbacks cannot break the codec queue.
        }
    }
}
