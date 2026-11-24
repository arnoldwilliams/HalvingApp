namespace HalvingFun.Services;

/// <summary>
/// Speaks the question and the praise out loud, so children who cannot read yet
/// can still play. Grown-ups can switch it off from the home screen.
/// </summary>
public sealed class VoiceGuide
{
    private const string Key = "voice_enabled";

    private CancellationTokenSource? _speech;

    public bool IsEnabled
    {
        get => Preferences.Default.Get(Key, true);
        set => Preferences.Default.Set(Key, value);
    }

    public void Say(string text)
    {
        if (!IsEnabled || string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        Stop();
        _speech = new CancellationTokenSource();
        _ = SpeakAsync(text, _speech.Token);
    }

    public void Stop()
    {
        _speech?.Cancel();
        _speech?.Dispose();
        _speech = null;
    }

    private static async Task SpeakAsync(string text, CancellationToken token)
    {
        try
        {
            await TextToSpeech.Default.SpeakAsync(text, null, token);
        }
        catch (OperationCanceledException)
        {
            // A newer sentence interrupted this one.
        }
        catch (Exception ex)
        {
            // No speech engine installed, or the platform does not support it.
            System.Diagnostics.Debug.WriteLine($"Speech failed: {ex.Message}");
        }
    }
}
