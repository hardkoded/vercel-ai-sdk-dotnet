// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

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
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private OpenAIProvider? _provider;
    private OpenAIRealtimeModelLive? _model;
    private IRealtimeSession? _session;
    private WaveInEvent? _microphone;
    private WaveOutEvent? _speaker;
    private BufferedWaveProvider? _playback;
    private CancellationTokenSource? _cts;
    private Task? _reader;
    private bool _stopping;
    private string? _lastSpeaker; // UI thread only

    public MainForm()
    {
        Text = "GPT-Live 1.1 voice";
        Width = 520;
        Height = 420;
        Controls.Add(_log);
        Controls.Add(_talk);
        _talk.Click += async (_, _) => await ToggleAsync();
        FormClosing += async (_, e) =>
        {
            if (_stopping)
            {
                e.Cancel = true;
                return;
            }

            if (_session is null)
            {
                return;
            }

            // WinForms does not wait for an async handler. Stop first, then close again.
            e.Cancel = true;
            await StopAsync();
            Close();
        };
    }

    private async Task ToggleAsync()
    {
        if (_stopping)
        {
            return;
        }

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
            _provider ??= OpenAIProvider.Create(
                new OpenAIOptions { ApiKey = apiKey, BaseUrl = "https://ai-gateway.vercel.sh/v1" }
            );
            var model = (OpenAIRealtimeModelLive)
                _provider.RealtimeModel(ModelId, new OpenAIRealtimeOptions { Api = "live" });
            var cts = new CancellationTokenSource();
            _model = model;
            _cts = cts;
            var session = await model.ConnectAsync(cts.Token);
            _session = session;

            // Realtime audio can arrive faster than it plays, so keep a long buffer.
            _playback = new BufferedWaveProvider(_format)
            {
                BufferDuration = TimeSpan.FromSeconds(60),
                DiscardOnBufferOverflow = true,
            };
            _speaker = new WaveOutEvent();
            _speaker.Init(_playback);
            _speaker.Play();

            var started = new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            _reader = Task.Run(() => ReadEventsAsync(session, model, started, cts.Token));

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
            await started.Task.WaitAsync(TimeSpan.FromSeconds(15), cts.Token);

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
                            AppendTranscript(e.Speaker, e.Delta);
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
            Append("Error: " + ex.Message);
        }
        finally
        {
            // Wakes a start that is still waiting, with the real reason instead of a timeout.
            started.TrySetException(new InvalidOperationException("The session closed."));
            if (!_stopping && !cancellationToken.IsCancellationRequested)
            {
                // The gateway ended the session. Reset the button and release the devices.
                Post(async () => await StopAsync());
            }
        }
    }

    private async void OnMicrophoneData(object? sender, WaveInEventArgs e)
    {
        try
        {
            // Drop a chunk when the previous send is still running, so audio never queues up.
            await SendAsync(
                new RealtimeModelV4ClientEvent("input-audio-append")
                {
                    Audio = Convert.ToBase64String(e.Buffer, 0, e.BytesRecorded),
                },
                dropIfBusy: true
            );
        }
        catch (Exception)
        {
            // async void must not throw. A dead session is reported by the reader.
        }
    }

    private async Task SendAsync(RealtimeModelV4ClientEvent clientEvent, bool dropIfBusy = false)
    {
        var session = _session;
        var model = _model;
        var token = _cts?.Token ?? CancellationToken.None;
        if (session is null || model is null)
        {
            return;
        }

        // A WebSocket allows one send at a time.
        if (dropIfBusy)
        {
            if (!_sendLock.Wait(0, CancellationToken.None))
            {
                return;
            }
        }
        else
        {
            await _sendLock.WaitAsync(token);
        }

        try
        {
            await session.SendAsync(model.SerializeClientEvent(clientEvent).ToJsonString(), token);
        }
        finally
        {
            _sendLock.Release();
        }
    }

    private async Task StopAsync()
    {
        if (_stopping)
        {
            return;
        }

        _stopping = true;
        _talk.Enabled = false;
        try
        {
            var microphone = _microphone;
            _microphone = null;
            Quiet(() => microphone?.StopRecording());
            Quiet(() => microphone?.Dispose());

            try
            {
                // Ask the gateway to close, then give it a moment to send the final events.
                await SendAsync(new RealtimeModelV4ClientEvent("session-close"));
                if (_reader is not null)
                {
                    await _reader.WaitAsync(TimeSpan.FromSeconds(5));
                }
            }
            catch (Exception)
            {
                // Closing is best effort. The session is disposed below either way.
            }

            Quiet(() => _cts?.Cancel());
            if (_session is not null)
            {
                try
                {
                    await _session.DisposeAsync();
                }
                catch (Exception)
                {
                    // Already closed.
                }
            }

            Quiet(() => _speaker?.Dispose());
            Quiet(() => _cts?.Dispose());
        }
        finally
        {
            _session = null;
            _model = null;
            _reader = null;
            _cts = null;
            _speaker = null;
            _playback = null;
            _stopping = false;
            if (!IsDisposed)
            {
                _talk.Text = "Start talking";
                _talk.Enabled = true;
            }
        }
    }

    // Teardown runs every step even when an earlier one throws, for example when a device is unplugged.
    private static void Quiet(Action action)
    {
        try
        {
            action();
        }
        catch (Exception)
        {
            // Best effort.
        }
    }

    private void Post(Action action)
    {
        if (IsDisposed || !IsHandleCreated)
        {
            return;
        }

        try
        {
            BeginInvoke(action);
        }
        catch (InvalidOperationException)
        {
            // The form closed while this was queued.
        }
    }

    private void Append(string line)
    {
        if (InvokeRequired)
        {
            Post(() => Append(line));
            return;
        }

        _lastSpeaker = null;
        _log.AppendText(line + Environment.NewLine);
    }

    // Transcript deltas are partial words. Keep them on one line until the speaker changes.
    private void AppendTranscript(string? speaker, string? delta) =>
        Post(() =>
        {
            if (speaker != _lastSpeaker)
            {
                _lastSpeaker = speaker;
                if (_log.TextLength > 0 && !_log.Text.EndsWith(Environment.NewLine))
                {
                    _log.AppendText(Environment.NewLine);
                }

                _log.AppendText(speaker == "user" ? "You: " : "AI: ");
            }

            _log.AppendText(delta);
        });
}
