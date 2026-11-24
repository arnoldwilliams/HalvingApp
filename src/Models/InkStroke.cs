namespace HalvingFun.Models;

/// <summary>One continuous pen line drawn by the child.</summary>
public sealed class InkStroke(PenColour pen)
{
    public PenColour Pen { get; } = pen;

    public List<PointF> Points { get; } = [];

    public void Add(PointF point)
    {
        // Skip samples that are almost on top of the previous one; it keeps the
        // stroke smooth and the hit testing cheap.
        if (Points.Count > 0)
        {
            var last = Points[^1];
            var dx = point.X - last.X;
            var dy = point.Y - last.Y;
            if (dx * dx + dy * dy < 4f)
            {
                return;
            }
        }

        Points.Add(point);
    }
}
