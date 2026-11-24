namespace HalvingFun.Drawing;

/// <summary>
/// Where the pictures sit inside the play area. The same maths drives the
/// <see cref="Microsoft.Maui.Controls.Image"/> grid and the ink layer, so the
/// pen lines always line up with what the child can see.
/// </summary>
public static class ItemLayout
{
    public static (int Rows, int Columns) GridFor(int count) => count switch
    {
        <= 2 => (1, 2),
        <= 4 => (2, 2),
        <= 6 => (2, 3),
        <= 8 => (2, 4),
        _ => (2, 5),
    };

    /// <summary>Square slots, one per item, evenly spaced in the play area.</summary>
    public static RectF[] Slots(RectF area, int count, float fill = 0.84f)
    {
        if (count <= 0 || area.Width <= 0 || area.Height <= 0)
        {
            return [];
        }

        var (rows, columns) = GridFor(count);
        float cellWidth = area.Width / columns;
        float cellHeight = area.Height / rows;
        float size = MathF.Min(cellWidth, cellHeight) * fill;

        var slots = new RectF[count];
        for (int index = 0; index < count; index++)
        {
            int row = index / columns;
            int column = index % columns;
            int inRow = Math.Min(columns, count - row * columns);
            float rowOffset = (columns - inRow) * cellWidth / 2f;

            float centreX = area.X + rowOffset + column * cellWidth + cellWidth / 2f;
            float centreY = area.Y + row * cellHeight + cellHeight / 2f;

            slots[index] = new RectF(centreX - size / 2f, centreY - size / 2f, size, size);
        }

        return slots;
    }

    /// <summary>One large picture in the middle of the play area.</summary>
    public static RectF Centre(RectF area, float fill = 0.86f)
    {
        float size = MathF.Min(area.Width, area.Height) * fill;
        return new RectF(
            area.Center.X - size / 2f,
            area.Center.Y - size / 2f,
            size,
            size);
    }

    /// <summary>The box a single picture really fills, honouring its aspect ratio.</summary>
    public static RectF Fitted(RectF area, float aspect, float fill = 0.88f)
    {
        float maxWidth = area.Width * fill;
        float maxHeight = area.Height * fill;

        float width = maxWidth;
        float height = width / aspect;
        if (height > maxHeight)
        {
            height = maxHeight;
            width = height * aspect;
        }

        return new RectF(
            area.Center.X - width / 2f,
            area.Center.Y - height / 2f,
            width,
            height);
    }
}
