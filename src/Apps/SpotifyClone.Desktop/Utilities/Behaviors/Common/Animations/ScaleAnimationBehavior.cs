using SpotifyClone.Desktop.Utilities.Animations;
using System.Windows;
using System.Windows.Input;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Common.Animations;

public static class ScaleAnimationBehavior
{
    #region Dependency Properties

    private static readonly DependencyProperty EnableOnHoverProperty = DependencyProperty.RegisterAttached(
        "EnableOnHover",
        typeof(bool),
        typeof(ScaleAnimationBehavior),
        new PropertyMetadata(false, OnEnableOnHoverChanged));

    private static readonly DependencyProperty ScaleFactorProperty = DependencyProperty.RegisterAttached(
        "ScaleFactor",
        typeof(double),
        typeof(ScaleAnimationBehavior),
        new PropertyMetadata(1.0));

    private static readonly DependencyProperty DurationProperty = DependencyProperty.RegisterAttached(
        "Duration",
        typeof(double),
        typeof(ScaleAnimationBehavior),
        new PropertyMetadata(0.0));

    #endregion

    #region Getters/Setters

    public static bool GetEnableOnHover(DependencyObject obj) =>
        (bool)obj.GetValue(EnableOnHoverProperty);
    public static void SetEnableOnHover(DependencyObject obj, bool value) =>
        obj.SetValue(EnableOnHoverProperty, value);

    public static double GetScaleFactor(DependencyObject obj) =>
        (double)obj.GetValue(ScaleFactorProperty);
    public static void SetScaleFactor(DependencyObject obj, double value) =>
        obj.SetValue(ScaleFactorProperty, value);

    public static double GetDuration(DependencyObject obj) =>
        (double)obj.GetValue(DurationProperty);
    public static void SetDuration(DependencyObject obj, double value) =>
        obj.SetValue(DurationProperty, value);

    #endregion

    #region Property Callbacks

    private static void OnEnableOnHoverChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            element.MouseEnter += ScaleIn;
            element.MouseLeave += ScaleOut;
            element.Unloaded += DetachEvents;
        }
        else
        {
            element.MouseEnter -= ScaleIn;
            element.MouseLeave -= ScaleOut;
            element.Unloaded -= DetachEvents;
        }
    }

    #endregion

    #region Event Handlers

    private static void ScaleIn(object sender, MouseEventArgs e)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        ScaleAnimations.BeginScaleAnimation(element, GetScaleFactor(element), GetDuration(element));
    }

    private static void ScaleOut(object sender, MouseEventArgs e)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        ScaleAnimations.ResetScaleAnimation(element, GetDuration(element));
    }

    private static void DetachEvents(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        element.MouseEnter -= ScaleIn;
        element.MouseLeave -= ScaleOut;
        element.Unloaded -= DetachEvents;

        SetEnableOnHover(element, false);
    }

    #endregion
}
