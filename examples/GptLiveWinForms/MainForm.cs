// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net.WebSockets;
using System.Text.Json;
using NAudio.Wave;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace GptLiveWinForms;

/// <summary>One button toggles a full-duplex voice session with GPT-Live 1.1 through AI Gateway.</summary>
public sealed class MainForm : Form
{
    private const string ModelId = "openai/gpt-live-1.1";
    private const int SampleRate = 24000;

    private readonly Button _talk = new()
    {
        Text = "Start talking",
        Dock = DockStyle.Top,
        Height = 56,
    };
    private readonly TextBox _log = new()
    {
        Dock = DockStyle.Fill,
        Multiline = true,
        ReadOnly = true,
        ScrollBars = ScrollBars.Vertical,
    };
    private readonly WaveFormat _format = new(SampleRate, 16, 1);
    private OpenAIRealtimeModelLive? _model;
    private IRealtimeSession? _session;
    private WaveInEvent? _microphone;
    private WaveOutEvent? _speaker;
    private BufferedWaveProvider? _playback;
    private CancellationTokenSource? _cts;
    private Task? _reader;

    public MainForm()
    {
        Text = "GPT-Live 1.1 voice";
        Width = 520;
        Height = 420;
        Controls.Add(_log);
        Controls.Add(_talk);
        _talk.Click += async (_, _) => await ToggleAsync();
        FormClosing += async (_, _) => await StopAsync();
    }

    private async Task ToggleAsync()
    {
        if (_session is not null)
        {
            await StopAsync();
            return;
        }

        var apiKey = Environment.GetEnvironmentVariable("AI_GATEWAY_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            Append("Set the AI_GATEWAY_API_KEY environment variable and restart.");
            return;
        }

        _talk.Enabled = false;
        try
        {
            var provider = OpenAIProvider.Create(
                new OpenAIOptions { ApiKey = apiKey, BaseUrl = "https://ai-gateway.vercel.sh/v1" }
            );
            _model = (OpenAIRealtimeModelLive)
                provider.RealtimeModel(ModelId, new OpenAIRealtimeOptions { Api = "live" });
            _cts = new CancellationTokenSource();
            _session = await _model.ConnectAsync(_cts.Token);

            _playback = new BufferedWaveProvider(_format) { DiscardOnBufferOverflow = true };
            _speaker = new WaveOutEvent();
            _speaker.Init(_playback);
            _speaker.Play();

            var started = new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            _reader = Task.Run(() => ReadEventsAsync(_session, _model, started, _cts.Token));

            await SendAsync(
                new RealtimeModelV4ClientEvent("session-start")
                {
                    Config = new RealtimeModelV4SessionConfig
                    {
                        InputAudioFormatType = "audio/pcm",
                        InputAudioFormatRate = SampleRate,
                    },
                }
            );

            // The gateway rejects audio sent before the session has started.
            await started.Task.WaitAsync(TimeSpan.FromSeconds(15), _cts.Token);

            // 20 ms chunks.
            _microphone = new WaveInEvent { WaveFormat = _format, BufferMilliseconds = 20 };
            _microphone.DataAvailable += OnMicrophoneData;
            _microphone.StartRecording();
            _talk.Text = "Stop";
        }
        catch (Exception ex)
        {
            Append("Could not start: " + ex.Message);
            await StopAsync();
        }
        finally
        {
            _talk.Enabled = true;
        }
    }

    private async Task ReadEventsAsync(
        IRealtimeSession session,
        OpenAIRealtimeModelLive model,
        TaskCompletionSource started,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await foreach (var json in session.Events.WithCancellation(cancellationToken))
            {
                using var document = JsonDocument.Parse(json);
                foreach (var e in model.ParseServerEvent(document.RootElement))
                {
                    switch (e.Type)
                    {
                        case "session-started":
                            started.TrySetResult();
                            Append("Connected. Start talking.");
                            break;
                        case "audio-chunk":
                            var pcm = Convert.FromBase64String(e.Delta!);
                            _playback?.AddSamples(pcm, 0, pcm.Length);
                            break;
                        case "transcript-fragment":
                            Append((e.Speaker == "user" ? "You: " : "AI: ") + e.Delta);
                            break;
                        case "error":
                            started.TrySetException(new InvalidOperationException(e.Message));
                            Append("Error: " + e.Message);
                            break;
                        case "session-closed":
                            return;
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Stopped by the user.
        }
        catch (Exception ex)
        {
            started.TrySetException(ex);
            Append("Error: " + ex.Message);
        }
    }

    private async void OnMicrophoneData(object? sender, WaveInEventArgs e)
    {
        try
        {
            await SendAsync(
                new RealtimeModelV4ClientEvent("input-audio-append")
                {
                    Audio = Convert.ToBase64String(e.Buffer, 0, e.BytesRecorded),
                }
            );
        }
        catch (Exception ex)
            when (ex is OperationCanceledException or WebSocketException or ObjectDisposedException)
        {
            // The session is closing.
        }
    }

    private Task SendAsync(RealtimeModelV4ClientEvent clientEvent)
    {
        var session = _session;
        var model = _model;
        if (session is null || model is null)
        {
            return Task.CompletedTask;
        }

        return session.SendAsync(
            model.SerializeClientEvent(clientEvent).ToJsonString(),
            _cts?.Token ?? CancellationToken.None
        );
    }

    private async Task StopAsync()
    {
        _microphone?.StopRecording();
        _microphone?.Dispose();
        _microphone = null;

        try
        {
            // Ask the gateway to close, then give it a moment to send the final events.
            await SendAsync(new RealtimeModelV4ClientEvent("session-close"));
            if (_reader is not null)
            {
                await _reader.WaitAsync(TimeSpan.FromSeconds(5));
            }
        }
        catch (Exception ex)
            when (ex
                    is OperationCanceledException
                        or TimeoutException
                        or WebSocketException
                        or ObjectDisposedException
            )
        {
            // Closing is best effort.
        }

        _cts?.Cancel();
        if (_session is not null)
        {
            await _session.DisposeAsync();
        }

        _session = null;
        _model = null;
        _reader = null;
        _cts?.Dispose();
        _cts = null;
        _speaker?.Dispose();
        _speaker = null;
        _playback = null;
        _talk.Text = "Start talking";
    }

    private void Append(string line)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => Append(line));
            return;
        }

        _log.AppendText(line + Environment.NewLine);
    }
}
