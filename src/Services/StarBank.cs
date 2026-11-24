namespace HalvingFun.Services;

/// <summary>Remembers the stars the child has collected, across app launches.</summary>
public sealed class StarBank
{
    private const string Key = "stars";

    public int Stars => Preferences.Default.Get(Key, 0);

    public int Add(int amount)
    {
        var total = Stars + amount;
        Preferences.Default.Set(Key, total);
        return total;
    }

    public void Reset() => Preferences.Default.Set(Key, 0);
}
