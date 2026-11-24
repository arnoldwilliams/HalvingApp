using AVFoundation;
using Foundation;

namespace HalvingFun.Services;

public sealed class IosSoundPlayer : SoundPlayerBase
{
    private readonly List<AVAudioPlayer> _playing = [];

    public IosSoundPlayer()
    {
        try
        {
            // Playback category keeps the cues audible when the ring/silent switch is on.
            var session = AVAudioSession.SharedInstance();
            session.SetCategory(AVAudioSessionCategory.Playback, AVAudioSessionCategoryOptions.MixWithOthers);
            session.SetActive(true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Audio session setup failed: {ex.Message}");
        }
    }

    protected override void PlayFile(string path)
    {
        var player = AVAudioPlayer.FromUrl(NSUrl.FromFilename(path), out var error);
        if (player is null || error is not null)
        {
            return;
        }

        player.PrepareToPlay();
        player.Play();
        _playing.Add(player);

        while (_playing.Count > 4)
        {
            var oldest = _playing[0];
            _playing.RemoveAt(0);
            oldest.Dispose();
        }
    }
}
