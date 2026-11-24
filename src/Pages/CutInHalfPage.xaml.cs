using HalvingFun.Drawing;
using HalvingFun.Models;
using HalvingFun.Services;
using Microsoft.Maui.Controls.Shapes;

namespace HalvingFun.Pages;

public partial class CutInHalfPage : ContentPage
{
    private static readonly Color Praise = Color.FromArgb("#2E9E4F");
    private static readonly Color Nudge = Color.FromArgb("#F57C00");

    private readonly StarBank _stars;
    private readonly ISoundPlayer _sound;
    private readonly VoiceGuide _voice;
    private readonly List<FoodItem> _snacks;

    private readonly InkDrawable _ink = new();
    private readonly List<InkStroke> _strokes = [];
    private readonly List<(PenColour Pen, Border Ring)> _penDots = [];

    private int _index;
    private RectF _itemRect;
    private PenColour _pen = PenColour.All[0];
    private InkStroke? _active;
    private bool _locked;
    private bool _onScreen;
    private int _attempts;
    private string? _speech;

    public CutInHalfPage(QuestionFactory factory, StarBank stars, ISoundPlayer sound, VoiceGuide voice)
    {
        InitializeComponent();

        _stars = stars;
        _sound = sound;
        _voice = voice;
        _snacks = factory.CreateSnackSession().ToList();

        _ink.Strokes = _strokes;
        Ink.Drawable = _ink;

        BuildPenBar();
        PlayArea.SizeChanged += (_, _) => LayoutPicture();

        LoadSnack();
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

    private FoodItem Snack => _snacks[_index];

    private void LoadSnack()
    {
        _strokes.Clear();
        _ink.Shade = null;
        _ink.Correct = new HashSet<int>();
        _active = null;
        _locked = false;
        _attempts = 0;

        SnackImage.Source = Snack.Image;
        PromptLabel.Text = $"Cut the {Snack.Singular} in half";
        ProgressLabel.Text = $"Snack {_index + 1} of {_snacks.Count}";

        LayoutPicture();
        UpdateStars();
        Ink.Invalidate();

        _speech = $"Cut the {Snack.Singular} in half. Draw one line down the middle.";
        Speak();
    }

    private void LayoutPicture()
    {
        if (PlayArea.Width <= 0 || PlayArea.Height <= 0)
        {
            return;
        }

        var area = new RectF(0, 0, (float)PlayArea.Width, (float)PlayArea.Height);
        _itemRect = ItemLayout.Fitted(area, Snack.Aspect);

        SnackImage.WidthRequest = _itemRect.Width;
        SnackImage.HeightRequest = _itemRect.Height;
        _ink.Slots = [_itemRect];

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

        _ink.Shade = null;
        _ink.Correct = new HashSet<int>();
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
        _ink.Shade = null;
        _ink.Correct = new HashSet<int>();
        Ink.Invalidate();
    }

    private async void OnCheckClicked(object? sender, EventArgs e)
    {
        if (_locked || _itemRect.Width <= 0)
        {
            return;
        }

        _locked = true;

        var line = _strokes.LastOrDefault();
        if (line is null || line.Points.Count < 2)
        {
            await EncourageAsync("Draw one straight line down the middle.");
            return;
        }

        if (StrokeGeometry.IsFairCut(line.Points, _itemRect, out var vertical))
        {
            await CelebrateAsync(line, vertical);
        }
        else
        {
            await EncourageAsync("Draw one line right down the middle, from edge to edge.");
        }
    }

    private async Task CelebrateAsync(InkStroke line, bool vertical)
    {
        _ink.Shade = new HalfShade(_itemRect, vertical, line.Pen.Colour);
        _ink.Correct = new HashSet<int> { 0 };
        Ink.Invalidate();

        await _sound.PlayAsync(SoundEffect.Correct);
        await _sound.PlayAsync(SoundEffect.Star);
        _stars.Add(1);
        UpdateStars();
        _voice.Say($"Perfect! Two equal halves of the {Snack.Singular}.");

        var hasMore = _index + 1 < _snacks.Count;
        await Feedback.ShowAsync(
            "✂️",
            "Perfect halves!",
            $"Two equal halves of the {Snack.Singular}.",
            hasMore ? "Next snack" : "Finish",
            Praise);

        if (hasMore)
        {
            _index++;
            LoadSnack();
            return;
        }

        await FinishSessionAsync();
    }

    private async Task EncourageAsync(string message)
    {
        _attempts++;
        _ink.Shade = null;

        if (_attempts > 1)
        {
            message = "Look for the middle. One line, straight down.";
        }

        Ink.Invalidate();
        await _sound.PlayAsync(SoundEffect.Wrong);
        _voice.Say(message);
        await Feedback.ShowAsync("🤔", "Try again", message, "Try again", Nudge);

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

        _index = 0;
        ShuffleSnacks();
        LoadSnack();
    }

    private void Speak()
    {
        if (_onScreen && _speech is not null)
        {
            _voice.Say(_speech);
        }
    }

    private void ShuffleSnacks()
    {
        for (int i = _snacks.Count - 1; i > 0; i--)
        {
            int j = Random.Shared.Next(i + 1);
            (_snacks[i], _snacks[j]) = (_snacks[j], _snacks[i]);
        }
    }

    private void UpdateStars() => StarsLabel.Text = _stars.Stars.ToString();

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        _voice.Stop();
        await Navigation.PopAsync();
    }
}
