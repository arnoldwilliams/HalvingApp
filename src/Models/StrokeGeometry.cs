namespace HalvingFun.Models;

/// <summary>Geometry helpers for turning a free-hand pen line into an answer.</summary>
public static class StrokeGeometry
{
    /// <summary>Even-odd ray casting test used to see whether a ring surrounds an item.</summary>
    public static bool Contains(IReadOnlyList<PointF> ring, PointF point)
    {
        var inside = false;
        for (int i = 0, j = ring.Count - 1; i < ring.Count; j = i++)
        {
            var a = ring[i];
            var b = ring[j];
            if (a.Y > point.Y != b.Y > point.Y &&
                point.X < (b.X - a.X) * (point.Y - a.Y) / (b.Y - a.Y) + a.X)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    /// <summary>Shoelace area, so a tiny scribble is never treated as a circle.</summary>
    public static float Area(IReadOnlyList<PointF> ring)
    {
        if (ring.Count < 3)
        {
            return 0f;
        }

        float sum = 0f;
        for (int i = 0, j = ring.Count - 1; i < ring.Count; j = i++)
        {
            sum += (ring[j].X + ring[i].X) * (ring[j].Y - ring[i].Y);
        }

        return MathF.Abs(sum) / 2f;
    }

    /// <summary>
    /// Decides whether a pen line is a fair "cut in half" for the given picture.
    /// The line may be roughly vertical or roughly horizontal, but it has to run
    /// through the middle of the food and reach both sides of it.
    /// </summary>
    public static bool IsFairCut(IReadOnlyList<PointF> line, RectF item, out bool vertical)
    {
        vertical = false;
        if (line.Count < 2)
        {
            return false;
        }

        var start = line[0];
        var end = line[^1];
        float dx = end.X - start.X;
        float dy = end.Y - start.Y;

        if (MathF.Abs(dy) < 1f && MathF.Abs(dx) < 1f)
        {
            return false;
        }

        vertical = MathF.Abs(dy) > MathF.Abs(dx);
        float across = vertical ? MathF.Abs(dx) : MathF.Abs(dy);
        float along = vertical ? MathF.Abs(dy) : MathF.Abs(dx);

        // More than about 30 degrees off straight up-and-down (or across).
        if (across > along * 0.55f)
        {
            return false;
        }

        var centre = item.Center;
        float size = vertical ? item.Height : item.Width;
        if (along < size * 0.5f)
        {
            return false;
        }

        float distance = DistanceFromLine(centre, start, end);
        if (distance > item.Width * 0.16f)
        {
            return false;
        }

        float near = vertical ? MathF.Min(start.Y, end.Y) : MathF.Min(start.X, end.X);
        float far = vertical ? MathF.Max(start.Y, end.Y) : MathF.Max(start.X, end.X);
        float mid = vertical ? centre.Y : centre.X;
        float reach = size * 0.22f;

        return near <= mid - reach && far >= mid + reach;
    }

    public static float DistanceFromLine(PointF point, PointF start, PointF end)
    {
        float dx = end.X - start.X;
        float dy = end.Y - start.Y;
        float length = MathF.Sqrt(dx * dx + dy * dy);
        if (length < 0.001f)
        {
            return MathF.Sqrt(MathF.Pow(point.X - start.X, 2) + MathF.Pow(point.Y - start.Y, 2));
        }

        return MathF.Abs(dy * point.X - dx * point.Y + end.X * start.Y - end.Y * start.X) / length;
    }
}
