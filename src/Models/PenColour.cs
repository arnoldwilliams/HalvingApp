namespace HalvingFun.Models;

/// <summary>A felt-tip colour the child can draw with.</summary>
public sealed record PenColour(string Name, Color Colour)
{
    public static readonly IReadOnlyList<PenColour> All =
    [
        new("Red", Color.FromArgb("#E53935")),
        new("Blue", Color.FromArgb("#1E88E5")),
        new("Green", Color.FromArgb("#2E9E4F")),
        new("Purple", Color.FromArgb("#8E24AA")),
        new("Orange", Color.FromArgb("#F57C00")),
        new("Pink", Color.FromArgb("#EC407A")),
    ];
}
