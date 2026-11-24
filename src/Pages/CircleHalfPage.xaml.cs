using HalvingFun.Drawing;
using HalvingFun.Models;
using HalvingFun.Services;
using Microsoft.Maui.Controls.Shapes;

namespace HalvingFun.Pages;

public partial class CircleHalfPage : ContentPage
{
    private static readonly Color Praise = Color.FromArgb("#2E9E4F");
    private static readonly Color Nudge = Color.FromArgb("#F57C00");

    private readonly QuestionFactory _factory;
    private readonly StarBank _stars;
    private readonly ISoundPlayer _sound;
    private readonly VoiceGuide _voice;

    private readonly InkDrawable _ink = new();
    private readonly List<InkStroke> _strokes = [];
    private readonly List<Image> _pictures = [];
    private readonly List<(PenColour Pen, Border Ring)> _penDots = [];

    private HalvingRound _round = null!;
    private IReadOnlyList<RectF> _slots = [];
    private PenColour _pen = PenColour.All[0];
    private InkStroke? _active;
    private bool _locked;
    private bool _onScreen;
    private int _attempts;
    private string? _speech;

    public CircleHalfPage(QuestionFactory factory, StarBank stars, ISoundPlayer sound, VoiceGuide voice)
    {
        InitializeComponent();

        _factory = factory;
        _stars = stars;
        _sound = sound;
        _voice = voice;

        _ink.Strokes = _strokes;
        Ink.Drawable = _ink;

        BuildPenBar();
        PlayArea.SizeChanged += (_, _) => LayoutPictures();

        StartSession();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _onScreen = true;
        Speak();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _onScreen = false;
        _voice.Stop();
    }

    private void StartSession()
    {
        _round = new HalvingRound(_factory.CreateSession());
        LoadQuestion();
    }

    private void LoadQuestion()
    {
        _strokes.Clear();
        _ink.Correct = new HashSet<int>();
        _ink.Incorrect = new HashSet<int>();
        _active = null;
        _locked = false;
        _attempts = 0;
        _slots = [];

        var question = _round.Current;
        PromptLabel.Text = question.Prompt;
        ProgressLabel.Text = $"Question {_round.Number} of {_round.Count}";

        BuildPictureGrid(question.Total);
        LayoutPictures();
        UpdateStars();

        Ink.Invalidate();
        _speech = question.SpokenPrompt;
        Speak();
    }

    private void BuildPictureGrid(int count)
    {
        ItemGrid.Children.Clear();
        ItemGrid.RowDefinitions.Clear();
        ItemGrid.ColumnDefinitions.Clear();
        _pictures.Clear();

        var (rows, columns) = ItemLayout.GridFor(count);
        for (int row = 0; row < rows; row++)
        {
            ItemGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        }

        for (int column = 0; column < columns; column++)
        {
            ItemGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        }

        for (int index = 0; index < count; index++)
        {
            var picture = new Image
            {
                Source = _round.Current.Item.Image,
                Aspect = Aspect.AspectFit,
                InputTransparent = true,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            };

            ItemGrid.Add(picture, index % columns, index / columns);
            _pictures.Add(picture);
        }
    }

    private void LayoutPictures()
    {
        if (_round is null || PlayArea.Width <= 0 || PlayArea.Height <= 0)
        {
            return;
        }

        var area = new RectF(0, 0, (float)PlayArea.Width, (float)PlayArea.Height);
        _slots = ItemLayout.Slots(area, _round.Current.Total);
        _ink.Slots = _slots;

        for (int index = 0; index < _pictures.Count && index < _slots.Count; index++)
        {
            _pictures[index].WidthRequest = _slots[index].Width;
            _pictures[index].HeightRequest = _slots[index].Height;
        }

        Ink.Invalidate();
    }

    private void BuildPenBar()
    {
        foreach (var pen in PenColour.All)
        {
            var ring = new Border
            {
                WidthRequest = 44,
                HeightRequest = 44,
                Padding = 0,
                BackgroundColor = Colors.White,
                Stroke = Colors.Transparent,
                StrokeThickness = 3,
                StrokeShape = new Ellipse(),
                Content = new Ellipse
                {
                    Fill = new SolidColorBrush(pen.Colour),
                    WidthRequest = 32,
                    HeightRequest = 32,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                },
            };

            var tap = new TapGestureRecognizer();
            tap.Tapped += async (_, _) => await SelectPenAsync(pen);
            ring.GestureRecognizers.Add(tap);
            SemanticProperties.SetDescription(ring, $"{pen.Name} pen");

            PenBar.Add(ring);
            _penDots.Add((pen, ring));
        }

        HighlightPen();
    }

    private async Task SelectPenAsync(PenColour pen)
    {
        if (_pen.Name == pen.Name)
        {
            return;
        }

        _pen = pen;
        HighlightPen();
        await _sound.PlayAsync(SoundEffect.Tap);
    }

    private void HighlightPen()
    {
        foreach (var (pen, ring) in _penDots)
        {
            var chosen = pen.Name == _pen.Name;
            ring.Stroke = chosen ? pen.Colour : Colors.Transparent;
            ring.Scale = chosen ? 1.08 : 1;
        }
    }

    private void OnInkStart(object? sender, TouchEventArgs e)
    {
        if (_locked || e.Touches.Length == 0)
        {
            return;
        }

        _ink.Correct = new HashSet<int>();
        _ink.Incorrect = new HashSet<int>();

        _active = new InkStroke(_pen);
        _active.Add(e.Touches[0]);
        _strokes.Add(_active);
        Ink.Invalidate();
    }

    private void OnInkDrag(object? sender, TouchEventArgs e)
    {
        if (_active is null || e.Touches.Length == 0)
        {
            return;
        }

        _active.Add(e.Touches[0]);
        Ink.Invalidate();
    }

    private void OnInkEnd(object? sender, TouchEventArgs e) => _active = null;

    private void OnClearClicked(object? sender, EventArgs e)
    {
        if (_locked)
        {
            return;
        }

        _strokes.Clear();
        _active = null;
        _ink.Correct = new HashSet<int>();
        _ink.Incorrect = new HashSet<int>();
        Ink.Invalidate();
    }

    private async void OnCheckClicked(object? sender, EventArgs e)
    {
        if (_locked || _slots.Count == 0)
        {
            return;
        }

        _locked = true;
        var question = _round.Current;
        var circled = CircledItems();

        if (circled.Count == question.Half)
        {
            await CelebrateAsync(question, circled);
        }
        else
        {
            await EncourageAsync(question, circled);
        }
    }

    private HashSet<int> CircledItems()
    {
        var circled = new HashSet<int>();
        if (_slots.Count == 0)
        {
            return circled;
        }

        // A quick scribble is not a ring, so ignore anything much smaller than an item.
        float smallestRing = _slots[0].Width * _slots[0].Height * 0.25f;

        foreach (var stroke in _strokes)
        {
            if (stroke.Points.Count < 4 || StrokeGeometry.Area(stroke.Points) < smallestRing)
            {
                continue;
            }

            for (int index = 0; index < _slots.Count; index++)
            {
                if (StrokeGeometry.Contains(stroke.Points, _slots[index].Center))
                {
                    circled.Add(index);
                }
            }
        }

        return circled;
    }

    private async Task CelebrateAsync(HalvingQuestion question, HashSet<int> circled)
    {
        _ink.Correct = circled;
        _ink.Incorrect = new HashSet<int>();
        Ink.Invalidate();

        await _sound.PlayAsync(SoundEffect.Correct);
        await _sound.PlayAsync(SoundEffect.Star);
        _stars.Add(1);
        UpdateStars();
        _voice.Say($"Well done! {question.AnswerSentence}.");

        var hasMore = _round.Number < _round.Count;
        await Feedback.ShowAsync(
            "🎉",
            "Well done!",
            question.AnswerSentence,
            hasMore ? "Next" : "Finish",
            Praise);

        if (hasMore && _round.Advance())
        {
            LoadQuestion();
            return;
        }

        await FinishSessionAsync();
    }

    private async Task EncourageAsync(HalvingQuestion question, HashSet<int> circled)
    {
        _attempts++;
        _ink.Correct = new HashSet<int>();
        _ink.Incorrect = circled;
        Ink.Invalidate();

        await _sound.PlayAsync(SoundEffect.Wrong);

        string detail = circled.Count switch
        {
            0 => "Draw a ring around the ones you want to share.",
            _ when _attempts == 1 => $"You circled {circled.Count}. Count them again!",
            _ => $"You circled {circled.Count}. {question.AnswerSentence}.",
        };

        _voice.Say(detail);
        await Feedback.ShowAsync("🤔", "Try again", detail, "Try again", Nudge);

        _locked = false;
    }

    private async Task FinishSessionAsync()
    {
        await _sound.PlayAsync(SoundEffect.Fanfare);
        _voice.Say($"You did it! You have {_stars.Stars} stars.");

        await Feedback.ShowAsync(
            "🏆",
            "You did it!",
            $"You have {_stars.Stars} stars",
            "Play again",
            Praise);

        StartSession();
    }

    private void Speak()
    {
        if (_onScreen && _speech is not null)
        {
            _voice.Say(_speech);
        }
    }

    private void UpdateStars() => StarsLabel.Text = _stars.Stars.ToString();

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        _voice.Stop();
        await Navigation.PopAsync();
    }
}
