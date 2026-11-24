using HalvingFun.Drawing;
using Xunit;

namespace HalvingFun.Tests;

public class ItemLayoutTests
{
    private static readonly RectF PlayArea = new(0, 0, 600, 400);

    [Theory]
    [InlineData(1, 1, 2)]
    [InlineData(2, 1, 2)]
    [InlineData(4, 2, 2)]
    [InlineData(6, 2, 3)]
    [InlineData(8, 2, 4)]
    [InlineData(10, 2, 5)]
    public void Grid_matches_the_number_of_items(int count, int rows, int columns)
    {
        Assert.Equal((rows, columns), ItemLayout.GridFor(count));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(8)]
    [InlineData(10)]
    public void Every_item_gets_a_square_slot_inside_the_play_area(int count)
    {
        var slots = ItemLayout.Slots(PlayArea, count);

        Assert.Equal(count, slots.Length);
        foreach (var slot in slots)
        {
            Assert.True(slot.Width > 0);
            Assert.Equal(slot.Width, slot.Height, 3);
            Assert.True(slot.X >= PlayArea.X);
            Assert.True(slot.Y >= PlayArea.Y);
            Assert.True(slot.Right <= PlayArea.Right);
            Assert.True(slot.Bottom <= PlayArea.Bottom);
        }
    }

    [Fact]
    public void Slots_are_spread_across_the_play_area()
    {
        var slots = ItemLayout.Slots(PlayArea, 6);

        Assert.True(slots[0].Center.X < slots[1].Center.X);
        Assert.True(slots[1].Center.X < slots[2].Center.X);
        Assert.Equal(slots[0].Center.Y, slots[2].Center.Y, 3);
        Assert.True(slots[0].Center.Y < slots[3].Center.Y);

        float leftGap = slots[0].X - PlayArea.X;
        float rightGap = PlayArea.Right - slots[2].Right;
        Assert.Equal(leftGap, rightGap, 1);
    }

    [Fact]
    public void Slots_never_overlap()
    {
        var slots = ItemLayout.Slots(PlayArea, 10);

        for (int a = 0; a < slots.Length; a++)
        {
            for (int b = a + 1; b < slots.Length; b++)
            {
                Assert.False(slots[a].IntersectsWith(slots[b]));
            }
        }
    }

    [Fact]
    public void No_items_means_no_slots()
    {
        Assert.Empty(ItemLayout.Slots(PlayArea, 0));
        Assert.Empty(ItemLayout.Slots(new RectF(0, 0, 0, 0), 4));
    }

    [Fact]
    public void Fitted_box_keeps_the_picture_shape()
    {
        var box = ItemLayout.Fitted(PlayArea, 2f);

        Assert.Equal(2f, box.Width / box.Height, 3);
        Assert.True(box.Width <= PlayArea.Width);
        Assert.True(box.Height <= PlayArea.Height);
    }
}
