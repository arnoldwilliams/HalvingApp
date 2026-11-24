using HalvingFun.Services;

namespace HalvingFun.Pages;

public partial class HomePage : ContentPage
{
    private readonly QuestionFactory _factory;
    private readonly StarBank _stars;
    private readonly ISoundPlayer _sound;
    private readonly VoiceGuide _voice;

    private bool _navigating;
    private bool _resetArmed;

    public HomePage(QuestionFactory factory, StarBank stars, ISoundPlayer sound, VoiceGuide voice)
    {
        InitializeComponent();

        _factory = factory;
        _stars = stars;
        _sound = sound;
        _voice = voice;

        VoiceSwitch.IsToggled = _voice.IsEnabled;
        VoiceSwitch.Toggled += (_, e) => _voice.IsEnabled = e.Value;

        SemanticProperties.SetDescription(CircleCard, "Circle Half game. Ring half of the treats.");
        SemanticProperties.SetDescription(CutCard, "Cut in Half game. Draw a line down the middle of a snack.");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _navigating = false;
        UpdateStars();
    }

    private void UpdateStars() => StarsLabel.Text = _stars.Stars.ToString();

    private async void OnCircleHalfTapped(object? sender, TappedEventArgs e)
    {
        if (_navigating)
        {
            return;
        }

        _navigating = true;
        await BounceAsync(CircleCard);
        await _sound.PlayAsync(SoundEffect.Tap);
        await Navigation.PushAsync(new CircleHalfPage(_factory, _stars, _sound, _voice));
    }

    private async void OnCutInHalfTapped(object? sender, TappedEventArgs e)
    {
        if (_navigating)
        {
            return;
        }

        _navigating = true;
        await BounceAsync(CutCard);
        await _sound.PlayAsync(SoundEffect.Tap);
        await Navigation.PushAsync(new CutInHalfPage(_factory, _stars, _sound, _voice));
    }

    private static async Task BounceAsync(VisualElement element)
    {
        await element.ScaleToAsync(0.96, 70);
        await element.ScaleToAsync(1, 110, Easing.SpringOut);
    }

    private async void OnResetTapped(object? sender, EventArgs e)
    {
        if (!_resetArmed)
        {
            _resetArmed = true;
            ResetButton.Text = "Tap again to reset";
            await Task.Delay(3000);
            if (_resetArmed)
            {
                _resetArmed = false;
                ResetButton.Text = "Reset stars";
            }

            return;
        }

        _resetArmed = false;
        ResetButton.Text = "Reset stars";
        _stars.Reset();
        UpdateStars();
    }
}
