namespace HalvingFun.Services;

public enum SoundEffect
{
    Correct,
    Wrong,
    Tap,
    Star,
    Fanfare,
}

public interface ISoundPlayer
{
    Task PlayAsync(SoundEffect effect);
}
