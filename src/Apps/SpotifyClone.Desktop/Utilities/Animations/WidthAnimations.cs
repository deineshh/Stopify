using System.Windows;
using System.Windows.Media.Animation;

namespace SpotifyClone.Desktop.Utilities.Animations;

public static class WidthAnimations
{
    public static void BeginWidthAnimation(UIElement element, double fromWidth, double toWidth, double durationSeconds)
    {
        var widthAnimation = new DoubleAnimation
        {
            From = fromWidth,
            To = toWidth,
            Duration = TimeSpan.FromSeconds(durationSeconds),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };
        widthAnimation.Completed += (s, e) =>
        {
            if (element is FrameworkElement frameworkElement)
            {
                frameworkElement.Width = double.NaN;
            }
        };
        element.BeginAnimation(FrameworkElement.WidthProperty, widthAnimation);
    }

    public static void ResetWidthAnimation(UIElement element, double originalWidth, double durationSeconds)
    {
        var widthResetAnimation = new DoubleAnimation
        {
            To = originalWidth,
            Duration = TimeSpan.FromSeconds(durationSeconds),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };
        element.BeginAnimation(FrameworkElement.WidthProperty, widthResetAnimation);
    }
}
