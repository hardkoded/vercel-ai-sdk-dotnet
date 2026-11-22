// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AI_GATEWAY_API_KEY")))
{
    Console.WriteLine("Set AI_GATEWAY_API_KEY to run this example.");
    return;
}

// 20 ms of silent mono PCM16 at 24 kHz; replace with microphone audio.
var silence = Convert.ToBase64String(new byte[960]);

var gateway = GatewayProvider.Create();
var model = (GatewayRealtimeModelLive)gateway.ExperimentalRealtime("openai/gpt-live-1.1", null);
var connection = await model.GetServerWebSocketConfigAsync(
    RealtimeModelV4SessionIntent.Conversation,
    CancellationToken.None
);

using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
using var socket = new ClientWebSocket();
foreach (var header in connection.Headers)
{
    socket.Options.SetRequestHeader(header.Key, header.Value);
}

await socket.ConnectAsync(new Uri(connection.Url), timeout.Token);

async Task SendAsync(RealtimeModelV4ClientEvent clientEvent)
{
    var frame = model
        .SerializeClientEvent(clientEvent, RealtimeModelV4SessionIntent.Conversation)
        .ToJsonString();
    await socket.SendAsync(
        Encoding.UTF8.GetBytes(frame),
        WebSocketMessageType.Text,
        endOfMessage: true,
        timeout.Token
    );
}

await SendAsync(
    new RealtimeModelV4ClientEvent("session-start")
    {
        Config = new RealtimeModelV4SessionConfig
        {
            Instructions = "Keep replies to one short sentence.",
            InputAudioFormatType = "audio/pcm",
            InputAudioFormatRate = 24000,
            OutputAudioFormatType = "audio/pcm",
            OutputAudioFormatRate = 24000,
        },
    }
);

using var audio = new CancellationTokenSource();
Task? audioLoop = null;
var outputBytes = 0;
var buffer = new byte[64 * 1024];
try
{
    while (true)
    {
        using var message = new MemoryStream();
        WebSocketReceiveResult received;
        do
        {
            received = await socket.ReceiveAsync(buffer, timeout.Token);
            if (received.MessageType == WebSocketMessageType.Close)
            {
                throw new InvalidOperationException("Socket closed before final usage.");
            }

            message.Write(buffer, 0, received.Count);
        } while (!received.EndOfMessage);

        // Gateway sends normalized events, so parsing is stateless.
        using var document = JsonDocument.Parse(message.ToArray());
        var serverEvent = model.ParseServerEvent(document.RootElement);
        switch (serverEvent.Type)
        {
            case "session-started":
                Console.WriteLine("Session started: " + serverEvent.SessionId);
                await SendAsync(
                    new RealtimeModelV4ClientEvent("context-append")
                    {
                        Content = "Greet the caller now, then listen.",
                        DelegationId = null,
                        ProviderOptions = new System.Text.Json.Nodes.JsonObject
                        {
                            ["openai"] = new System.Text.Json.Nodes.JsonObject
                            {
                                ["channel"] = "instructions",
                            },
                        },
                    }
                );
                audioLoop = Task.Run(
                    async () =>
                    {
                        while (!audio.IsCancellationRequested)
                        {
                            await SendAsync(
                                new RealtimeModelV4ClientEvent("input-audio-append")
                                {
                                    Audio = silence,
                                }
                            );
                            await Task.Delay(20, audio.Token);
                        }
                    },
                    audio.Token
                );
                break;
            case "audio-chunk":
                outputBytes += Convert.FromBase64String(serverEvent.Delta!).Length;
                break;
            case "transcript-fragment" when serverEvent.Speaker == "assistant":
                Console.Write(serverEvent.Delta);
                break;
            case "session-closed":
                Console.WriteLine();
                Console.WriteLine("Output audio bytes: " + outputBytes);
                Console.WriteLine("Final usage: " + serverEvent.Usage?.Seconds + " s");
                return;
            case "error":
                throw new InvalidOperationException(serverEvent.Message);
        }

        // Close once the spoken reply has finished.
        if (serverEvent.Type == "transcript-fragment" && outputBytes > 0 && audioLoop is not null)
        {
            await SendAsync(new RealtimeModelV4ClientEvent("session-close"));
        }
    }
}
finally
{
    // Always end the billable session.
    await audio.CancelAsync();
    try
    {
        if (audioLoop is not null)
        {
            await audioLoop;
        }
    }
    catch (OperationCanceledException) { }

    if (socket.State == WebSocketState.Open)
    {
        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
    }
}
