using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SpotifyClone.Desktop.Utilities.Animations;

public static class ScaleAnimations
{
    public static void BeginScaleAnimation(UIElement element, double scaleTo, double durationSeconds)
    {
        var scaleTransform = new ScaleTransform(1.0, 1.0);
        element.RenderTransform = scaleTransform;
        element.RenderTransformOrigin = new Point(0.5, 0.5);
        var scaleUpAnimation = new DoubleAnimation(1.0, scaleTo, TimeSpan.FromSeconds(durationSeconds))
        {
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };
        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleUpAnimation);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleUpAnimation);
    }

    public static void ResetScaleAnimation(UIElement element, double durationSeconds)
    {
        if (element.RenderTransform is not ScaleTransform scaleTransform)
        {
            return;
        }

        var scaleDownAnimation = new DoubleAnimation(scaleTransform.ScaleX, 1.0, TimeSpan.FromSeconds(durationSeconds))
        {
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };
        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleDownAnimation);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleDownAnimation);
    }
}
