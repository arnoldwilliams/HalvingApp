namespace HalvingFun.Views;

/// <summary>
/// A modal "well done / try again" card. <see cref="ShowAsync"/> returns when the
/// child taps the single action button, so pages can await the answer.
/// </summary>
public partial class FeedbackOverlay : ContentView
{
    private TaskCompletionSource? _dismissed;

    public FeedbackOverlay()
    {
        InitializeComponent();
    }

    public async Task ShowAsync(string emoji, string title, string detail, string actionText, Color accent)
    {
        EmojiLabel.Text = emoji;
        TitleLabel.Text = title;
        TitleLabel.TextColor = accent;
        DetailLabel.Text = detail;
        ActionButton.Text = actionText;
        ActionButton.BackgroundColor = accent;

        _dismissed = new TaskCompletionSource();
        IsVisible = true;
        Opacity = 0;
        Card.Opacity = 0;
        Card.Scale = 0.85;

        await Task.WhenAll(
            this.FadeToAsync(1, 140),
            Card.FadeToAsync(1, 180),
            Card.ScaleToAsync(1, 320, Easing.SpringOut));

        await _dismissed.Task;

        await Task.WhenAll(
            this.FadeToAsync(0, 130),
            Card.ScaleToAsync(0.9, 130));

        IsVisible = false;
        _dismissed = null;
    }

    private void OnActionClicked(object? sender, EventArgs e) => _dismissed?.TrySetResult();
}
