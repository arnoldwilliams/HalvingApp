using HalvingFun.Models;
using HalvingFun.Services;
using Xunit;

namespace HalvingFun.Tests;

public class QuestionFactoryTests
{
    [Fact]
    public void Session_has_the_requested_number_of_questions()
    {
        var questions = new QuestionFactory().CreateSession();

        Assert.Equal(8, questions.Count);
        Assert.Equal(4, new QuestionFactory().CreateSession(4).Count);
    }

    [Fact]
    public void Questions_are_graded_from_easiest_to_hardest()
    {
        var questions = new QuestionFactory().CreateSession();

        for (int index = 1; index < questions.Count; index++)
        {
            Assert.True(questions[index].Total >= questions[index - 1].Total);
        }
    }

    [Fact]
    public void Every_question_has_an_whole_number_half()
    {
        foreach (var question in new QuestionFactory().CreateSession(30))
        {
            Assert.True(question.Total >= 2);
            Assert.Equal(0, question.Total % 2);
            Assert.Equal(question.Total / 2, question.Half);
        }
    }

    [Fact]
    public void Snack_session_uses_every_snack_once()
    {
        var snacks = new QuestionFactory().CreateSnackSession();

        Assert.Equal(FoodItem.Snacks.Count, snacks.Count);
        Assert.Equal(FoodItem.Snacks.Count, snacks.Distinct().Count());
    }

    [Fact]
    public void Round_walks_through_every_question_once()
    {
        var round = new HalvingRound(new QuestionFactory().CreateSession(3));

        Assert.Equal(1, round.Number);
        Assert.Equal(3, round.Count);

        Assert.True(round.Advance());
        Assert.Equal(2, round.Number);

        Assert.True(round.Advance());
        Assert.Equal(3, round.Number);

        Assert.False(round.Advance());
        Assert.Equal(3, round.Number);
    }

    [Fact]
    public void Prompt_names_the_food_and_answers_read_aloud()
    {
        var question = new HalvingQuestion(FoodItem.Apple, 6);

        Assert.Equal("Circle half of the apples", question.Prompt);
        Assert.Equal("Half of 6 is 3", question.AnswerSentence);
        Assert.Equal(3, question.Half);
    }
}
