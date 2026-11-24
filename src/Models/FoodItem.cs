namespace HalvingFun.Models;

/// <summary>
/// One of the food pictures taken from the worksheet. The image files are the
/// transparent PNGs produced by <c>tools/extract_artwork.py</c>.
/// </summary>
public sealed record FoodItem(string Image, string Singular, string Plural, float Aspect)
{
    public static readonly FoodItem Apple = new("apple.png", "apple", "apples", 454f / 530f);
    public static readonly FoodItem Biscuit = new("biscuit.png", "biscuit", "biscuits", 1f);
    public static readonly FoodItem Sweet = new("sweet.png", "sweet", "sweets", 640f / 246f);
    public static readonly FoodItem Berry = new("berry.png", "berry", "berries", 1f);
    public static readonly FoodItem Pizza = new("pizza.png", "pizza", "pizzas", 1f);
    public static readonly FoodItem Toast = new("toast.png", "toast", "toasts", 1f);
    public static readonly FoodItem BreadRoll = new("breadroll.png", "bread roll", "bread rolls", 604f / 313f);

    /// <summary>Groups used by the "circle half" game.</summary>
    public static readonly IReadOnlyList<FoodItem> Groups = [Apple, Biscuit, Sweet, Berry, Pizza, Toast];

    /// <summary>Snacks used by the "cut in half" game.</summary>
    public static readonly IReadOnlyList<FoodItem> Snacks = [Pizza, Toast, BreadRoll];
}
