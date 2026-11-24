namespace HalvingFun.Models;

/// <summary>A "circle half of the group" question.</summary>
public sealed record HalvingQuestion(FoodItem Item, int Total)
{
    public int Half => Total / 2;

    public string Prompt => $"Circle half of the {Item.Plural}";

    public string SpokenPrompt => $"Circle half of the {Item.Plural}.";

    public string AnswerSentence => $"Half of {Total} is {Half}";
}
