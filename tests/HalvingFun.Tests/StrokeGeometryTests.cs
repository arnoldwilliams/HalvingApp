using HalvingFun.Models;
using Xunit;

namespace HalvingFun.Tests;

public class StrokeGeometryTests
{
    private static readonly RectF Square = new(0, 0, 100, 100);

    private static List<PointF> Circle(PointF centre, float radius, int steps = 32)
    {
        var points = new List<PointF>();
        for (int i = 0; i < steps; i++)
        {
            float angle = MathF.Tau * i / steps;
            points.Add(new PointF(
                centre.X + radius * MathF.Cos(angle),
                centre.Y + radius * MathF.Sin(angle)));
        }

        return points;
    }

    [Fact]
    public void Ring_contains_the_item_inside_it()
    {
        var ring = Circle(new PointF(100, 100), 40);

        Assert.True(StrokeGeometry.Contains(ring, new PointF(100, 100)));
        Assert.True(StrokeGeometry.Contains(ring, new PointF(120, 90)));
    }

    [Fact]
    public void Ring_does_not_contain_items_outside_it()
    {
        var ring = Circle(new PointF(100, 100), 40);

        Assert.False(StrokeGeometry.Contains(ring, new PointF(200, 100)));
        Assert.False(StrokeGeometry.Contains(ring, new PointF(100, 20)));
    }

    [Fact]
    public void Area_of_a_square_ring_matches_its_size()
    {
        List<PointF> square = [new(0, 0), new(10, 0), new(10, 10), new(0, 10)];

        Assert.Equal(100f, StrokeGeometry.Area(square), 3);
    }

    [Fact]
    public void Area_of_a_short_scribble_is_zero()
    {
        List<PointF> scribble = [new(0, 0), new(5, 5)];

        Assert.Equal(0f, StrokeGeometry.Area(scribble));
    }

    [Fact]
    public void Vertical_line_through_the_middle_is_a_fair_cut()
    {
        List<PointF> line = [new(50, 8), new(50, 92)];

        Assert.True(StrokeGeometry.IsFairCut(line, Square, out var vertical));
        Assert.True(vertical);
    }

    [Fact]
    public void Horizontal_line_through_the_middle_is_a_fair_cut()
    {
        List<PointF> line = [new(8, 50), new(92, 50)];

        Assert.True(StrokeGeometry.IsFairCut(line, Square, out var vertical));
        Assert.False(vertical);
    }

    [Fact]
    public void Diagonal_line_is_not_a_fair_cut()
    {
        List<PointF> line = [new(8, 8), new(92, 92)];

        Assert.False(StrokeGeometry.IsFairCut(line, Square, out _));
    }

    [Fact]
    public void Line_off_to_one_side_is_not_a_fair_cut()
    {
        List<PointF> line = [new(88, 8), new(88, 92)];

        Assert.False(StrokeGeometry.IsFairCut(line, Square, out _));
    }

    [Fact]
    public void Short_line_that_does_not_reach_the_edges_is_not_a_fair_cut()
    {
        List<PointF> line = [new(50, 40), new(50, 60)];

        Assert.False(StrokeGeometry.IsFairCut(line, Square, out _));
    }

    [Fact]
    public void Single_point_is_not_a_fair_cut()
    {
        List<PointF> line = [new(50, 50)];

        Assert.False(StrokeGeometry.IsFairCut(line, Square, out _));
    }

    [Fact]
    public void Distance_from_line_is_measured_perpendicular()
    {
        float distance = StrokeGeometry.DistanceFromLine(new PointF(10, 30), new PointF(0, 0), new PointF(100, 0));

        Assert.Equal(30f, distance, 3);
    }
}
