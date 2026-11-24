namespace HalvingFun.Services;

using HalvingFun.Models;

/// <summary>Tracks progress through a list of questions.</summary>
public sealed class HalvingRound(IReadOnlyList<HalvingQuestion> questions)
{
    private int _index;

    public HalvingQuestion Current => questions[_index];

    public int Number => _index + 1;

    public int Count => questions.Count;

    /// <summary>Moves on to the next question, returning false when the round is over.</summary>
    public bool Advance()
    {
        if (_index + 1 >= questions.Count)
        {
            return false;
        }

        _index++;
        return true;
    }
}
