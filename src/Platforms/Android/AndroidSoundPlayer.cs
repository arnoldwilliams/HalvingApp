using Android.Media;

namespace HalvingFun.Services;

public sealed class AndroidSoundPlayer : SoundPlayerBase
{
    private readonly List<MediaPlayer> _playing = [];

    protected override void PlayFile(string path)
    {
        var context = Android.App.Application.Context;
        var player = MediaPlayer.Create(context, Android.Net.Uri.FromFile(new Java.IO.File(path)));
        if (player is null)
        {
            return;
        }

        player.Start();
        _playing.Add(player);

        while (_playing.Count > 4)
        {
            var oldest = _playing[0];
            _playing.RemoveAt(0);
            oldest.Release();
            oldest.Dispose();
        }
    }
}
