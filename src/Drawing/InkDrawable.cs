using HalvingFun.Models;

namespace HalvingFun.Drawing;

/// <summary>Which half of a picture to colour in once it has been cut fairly.</summary>
public sealed record HalfShade(RectF Item, bool Vertical, Color Colour);

/// <summary>Draws the child's pen lines plus the green/red feedback rings.</summary>
public sealed class InkDrawable : IDrawable
{
    private static readonly Color GoodColour = Color.FromArgb("#2E9E4F");
    private static readonly Color BadColour = Color.FromArgb("#E53935");

    public IReadOnlyList<InkStroke> Strokes { get; set; } = [];

    public IReadOnlyList<RectF> Slots { get; set; } = [];

    public IReadOnlySet<int> Correct { get; set; } = new HashSet<int>();

    public IReadOnlySet<int> Incorrect { get; set; } = new HashSet<int>();

    public HalfShade? Shade { get; set; }

    public float PenWidth { get; set; } = 13f;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        DrawShade(canvas);
        DrawRings(canvas);

        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeLineJoin = LineJoin.Round;
        canvas.StrokeSize = PenWidth;

        foreach (var stroke in Strokes)
        {
            if (stroke.Points.Count == 0)
            {
                continue;
            }

            canvas.StrokeColor = stroke.Pen.Colour;
            canvas.DrawPath(ToPath(stroke.Points));
        }
    }

    private void DrawShade(ICanvas canvas)
    {
        if (Shade is not { } shade)
        {
            return;
        }

        // Clip to a soft rounded box so the tint does not spill past the picture's corners.
        float radius = MathF.Min(shade.Item.Width, shade.Item.Height) * 0.22f;
        var shape = new PathF();
        shape.AppendRoundedRectangle(shade.Item, radius);

        var half = shade.Vertical
            ? new RectF(shade.Item.X, shade.Item.Y, shade.Item.Width / 2f, shade.Item.Height)
            : new RectF(shade.Item.X, shade.Item.Y, shade.Item.Width, shade.Item.Height / 2f);

        canvas.SaveState();
        canvas.ClipPath(shape);
        canvas.FillColor = shade.Colour.WithAlpha(0.32f);
        canvas.FillRectangle(half);
        canvas.RestoreState();
    }

    private void DrawRings(ICanvas canvas)
    {
        canvas.StrokeSize = 5f;
        canvas.StrokeDashPattern = [7f, 5f];

        for (int index = 0; index < Slots.Count; index++)
        {
            var isGood = Correct.Contains(index);
            var isBad = Incorrect.Contains(index);
            if (!isGood && !isBad)
            {
                continue;
            }

            var ring = Grow(Slots[index], 10f);
            var colour = isGood ? GoodColour : BadColour;

            canvas.FillColor = colour.WithAlpha(0.14f);
            canvas.FillRoundedRectangle(ring, 18f);
            canvas.StrokeColor = colour;
            canvas.DrawRoundedRectangle(ring, 18f);
        }

        canvas.StrokeDashPattern = null;
    }

    private static RectF Grow(RectF rect, float amount) =>
        new(rect.X - amount, rect.Y - amount, rect.Width + amount * 2f, rect.Height + amount * 2f);

    /// <summary>Smooth the sampled touch points so the line looks like felt tip.</summary>
    private static PathF ToPath(IReadOnlyList<PointF> points)
    {
        var path = new PathF();
        path.MoveTo(points[0]);

        if (points.Count == 1)
        {
            path.LineTo(points[0].X + 0.5f, points[0].Y + 0.5f);
            return path;
        }

        for (int index = 1; index < points.Count - 1; index++)
        {
            var current = points[index];
            var next = points[index + 1];
            path.QuadTo(
                current.X,
                current.Y,
                (current.X + next.X) / 2f,
                (current.Y + next.Y) / 2f);
        }

        path.LineTo(points[^1]);
        return path;
    }
}
