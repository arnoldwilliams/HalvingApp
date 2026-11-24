namespace HalvingFun.Services;

/// <summary>
/// Copies the bundled WAV files out of the app package once, then hands the file
/// path to the platform player.
/// </summary>
public abstract class SoundPlayerBase : ISoundPlayer
{
    private static readonly Dictionary<SoundEffect, string> Files = new()
    {
        [SoundEffect.Correct] = "correct.wav",
        [SoundEffect.Wrong] = "wrong.wav",
        [SoundEffect.Tap] = "tap.wav",
        [SoundEffect.Star] = "star.wav",
        [SoundEffect.Fanfare] = "fanfare.wav",
    };

    private readonly Dictionary<SoundEffect, string> _readyPaths = [];

    public async Task PlayAsync(SoundEffect effect)
    {
        try
        {
            var path = await EnsureFileAsync(effect);
            if (path is not null)
            {
                PlayFile(path);
            }
        }
        catch (Exception ex)
        {
            // A missing or unsupported audio file must never break the game.
            System.Diagnostics.Debug.WriteLine($"Sound {effect} failed: {ex.Message}");
        }
    }

    protected abstract void PlayFile(string path);

    private async Task<string?> EnsureFileAsync(SoundEffect effect)
    {
        if (_readyPaths.TryGetValue(effect, out var cached))
        {
            return cached;
        }

        var fileName = Files[effect];
        var target = Path.Combine(FileSystem.CacheDirectory, fileName);

        if (!File.Exists(target))
        {
            await using var source = await FileSystem.OpenAppPackageFileAsync(fileName);
            await using var destination = File.Create(target);
            await source.CopyToAsync(destination);
        }

        _readyPaths[effect] = target;
        return target;
    }
}
