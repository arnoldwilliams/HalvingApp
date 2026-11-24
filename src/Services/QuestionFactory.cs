namespace HalvingFun.Services;

using HalvingFun.Models;

/// <summary>Builds a short, gently graded set of questions for one play session.</summary>
public sealed class QuestionFactory
{
    private static readonly (FoodItem Item, int Total)[] Pool =
    [
        (FoodItem.Apple, 2), (FoodItem.Apple, 4), (FoodItem.Apple, 6), (FoodItem.Apple, 8),
        (FoodItem.Biscuit, 2), (FoodItem.Biscuit, 4), (FoodItem.Biscuit, 6), (FoodItem.Biscuit, 8), (FoodItem.Biscuit, 10),
        (FoodItem.Sweet, 4), (FoodItem.Sweet, 6), (FoodItem.Sweet, 8), (FoodItem.Sweet, 10),
        (FoodItem.Berry, 2), (FoodItem.Berry, 4), (FoodItem.Berry, 6), (FoodItem.Berry, 8), (FoodItem.Berry, 10),
        (FoodItem.Pizza, 2), (FoodItem.Pizza, 4), (FoodItem.Pizza, 6), (FoodItem.Pizza, 8), (FoodItem.Pizza, 10),
        (FoodItem.Toast, 2), (FoodItem.Toast, 4), (FoodItem.Toast, 6), (FoodItem.Toast, 8), (FoodItem.Toast, 10),
    ];

    public IReadOnlyList<HalvingQuestion> CreateSession(int length = 8)
    {
        return Pool
            .OrderBy(_ => Random.Shared.Next())
            .Take(length)
            .OrderBy(pair => pair.Total)
            .Select(pair => new HalvingQuestion(pair.Item, pair.Total))
            .ToList();
    }

    public IReadOnlyList<FoodItem> CreateSnackSession()
    {
        return FoodItem.Snacks.OrderBy(_ => Random.Shared.Next()).ToList();
    }
}
