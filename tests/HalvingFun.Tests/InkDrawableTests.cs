using HalvingFun.Drawing;
using HalvingFun.Models;
using Microsoft.Maui.Graphics.Skia;
using SkiaSharp;
using Xunit;

namespace HalvingFun.Tests;

/// <summary>
/// Renders the ink layer with the same Skia backend MAUI uses on device, so a
/// crash or an invisible highlight in the drawing code fails the build.
/// </summary>
public class InkDrawableTests
{
    private const int Size = 200;

    private static SKBitmap Render(IDrawable drawable)
    {
        using var context = new SkiaBitmapExportContext(Size, Size, 1f);
        drawable.Draw(context.Canvas, new RectF(0, 0, Size, Size));

        using var stream = new MemoryStream();
        context.WriteToStream(stream);
        stream.Position = 0;
        return SKBitmap.Decode(stream);
    }

    // Color stores 0..1 floats, SKColor stores bytes.
    private static bool Near(byte actual, float expected) => Math.Abs(actual - expected * 255f) < 3f;

    private static InkStroke Line(PenColour pen, params PointF[] points)
    {
        var stroke = new InkStroke(pen);
        foreach (var point in points)
        {
            stroke.Add(point);
        }

        return stroke;
    }

    [Fact]
    public void Empty_layer_draws_nothing()
    {
        using var bitmap = Render(new InkDrawable());

        Assert.Equal(0, bitmap.GetPixel(Size / 2, Size / 2).Alpha);
    }

    [Fact]
    public void Pen_line_is_drawn_in_the_chosen_colour()
    {
        var red = PenColour.All[0];
        var drawable = new InkDrawable
        {
            Strokes = [Line(red, new PointF(20, 100), new PointF(180, 100))],
        };

        using var bitmap = Render(drawable);

        var middle = bitmap.GetPixel(100, 100);
        Assert.True(Near(middle.Red, red.Colour.Red), $"red channel was {middle}");
        Assert.True(Near(middle.Green, red.Colour.Green), $"green channel was {middle}");
        Assert.True(Near(middle.Blue, red.Colour.Blue), $"blue channel was {middle}");
        Assert.Equal(0, bitmap.GetPixel(100, 30).Alpha);
    }

    [Fact]
    public void Correct_items_get_a_green_ring_and_wrong_ones_a_red_ring()
    {
        var slot = new RectF(60, 60, 80, 80);
        var drawable = new InkDrawable
        {
            Slots = [slot],
            Correct = new HashSet<int> { 0 },
        };

        using var correct = Render(drawable);

        // The ring hugs the slot edge, so sample just inside the top left corner.
        var ringPixel = correct.GetPixel(64, 64);
        Assert.True(ringPixel.Alpha > 0);
        Assert.True(ringPixel.Green > ringPixel.Red, $"expected a green ring, got {ringPixel}");

        drawable.Correct = new HashSet<int>();
        drawable.Incorrect = new HashSet<int> { 0 };

        using var incorrect = Render(drawable);
        var badPixel = incorrect.GetPixel(64, 64);
        Assert.True(badPixel.Alpha > 0);
        Assert.True(badPixel.Red > badPixel.Green, $"expected a red ring, got {badPixel}");
    }

    [Fact]
    public void Half_shade_only_covers_one_half_of_the_picture()
    {
        var item = new RectF(40, 40, 120, 120);
        var drawable = new InkDrawable
        {
            Shade = new HalfShade(item, Vertical: true, PenColour.All[1].Colour),
        };

        using var bitmap = Render(drawable);

        var left = bitmap.GetPixel(50, 100);
        var right = bitmap.GetPixel(150, 100);

        Assert.True(left.Alpha > 0, "the cut half should be tinted");
        Assert.Equal(0, right.Alpha);
    }

    [Fact]
    public void Shade_is_clipped_so_it_never_spills_past_the_picture()
    {
        var item = new RectF(40, 40, 120, 120);
        var drawable = new InkDrawable
        {
            Shade = new HalfShade(item, Vertical: true, PenColour.All[1].Colour),
        };

        using var bitmap = Render(drawable);

        Assert.Equal(0, bitmap.GetPixel(41, 41).Alpha);
        Assert.Equal(0, bitmap.GetPixel(20, 100).Alpha);
    }
}
