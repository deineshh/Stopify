using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SpotifyClone.Desktop.Utilities.Animations;

public static class ColorAnimations
{
    private static void AnimateBrushColor(DependencyObject target, DependencyProperty brushProperty, Color? fromColor, Color toColor, double durationSeconds)
    {
        var currentBrush = target.GetValue(brushProperty) as SolidColorBrush;

        if (currentBrush == null && fromColor == null)
        {
            return;
        }

        Color startColor = fromColor ?? currentBrush!.Color;

        var colorAnimation = new ColorAnimation
        {
            From = startColor,
            To = toColor,
            Duration = TimeSpan.FromSeconds(durationSeconds)
        };

        var animatedBrush = new SolidColorBrush(startColor);
        animatedBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);

        target.SetValue(brushProperty, animatedBrush);
    }

    public static void AnimateForeground(DependencyObject target, Color toColor, double durationSeconds, Color? fromColor = null) =>
        AnimateBrushColor(target, Control.ForegroundProperty, fromColor, toColor, durationSeconds);

    public static void AnimateBackground(DependencyObject target, Color toColor, double durationSeconds, Color? fromColor = null) =>
        AnimateBrushColor(target, Panel.BackgroundProperty, fromColor, toColor, durationSeconds);

    public static void AnimateBorderBrush(DependencyObject target, Color toColor, double durationSeconds, Color? fromColor = null) =>
        AnimateBrushColor(target, Control.BorderBrushProperty, fromColor, toColor, durationSeconds);
}
