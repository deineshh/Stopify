using SpotifyClone.Desktop.Utilities.Animations;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Titlebar;

public static class NavigationButtonBehavior
{
    #region Dependency Properties

    private static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
            "Enable",
            typeof(bool?),
            typeof(NavigationButtonBehavior),
            new PropertyMetadata(null, OnEnableChanged));

    #endregion

    #region Getters/Setters

    public static bool? GetEnable(UIElement element) =>
        (bool?)element.GetValue(EnableProperty);
    public static void SetEnable(UIElement element, bool? value) =>
        element.SetValue(EnableProperty, value);

    #endregion

    #region Property Callbacks

    private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Label element)
        {
            return;
        }

        element.MouseEnter -= SetCursor;
        element.MouseLeave -= ResetCursor;
        element.Unloaded -= DetachEvents;

        if ((bool?)e.NewValue == true)
        {
            ColorAnimations.AnimateForeground(element, Colors.DarkGray, .02);
        }
        else if ((bool?)e.NewValue == false)
        {
            ColorAnimations.AnimateForeground(element, Color.FromRgb(50, 50, 50), .02);
        }

        element.MouseEnter += SetCursor;
        element.MouseLeave += ResetCursor;
        element.Unloaded += DetachEvents;
    }

    #endregion

    #region Event Handlers

    private static void SetCursor(object sender, MouseEventArgs args)
    {
        if (sender is not Label element)
        {
            return;
        }

        Mouse.OverrideCursor = GetEnable(element) == true ? Cursors.Hand : Cursors.No;
    }

    private static void ResetCursor(object sender, MouseEventArgs args) =>
        Mouse.OverrideCursor = Cursors.Arrow;

    private static void DetachEvents(object sender, RoutedEventArgs e)
    {
        if (sender is not Label element)
        {
            return;
        }

        element.MouseEnter -= SetCursor;
        element.MouseLeave -= ResetCursor;
        element.Unloaded -= DetachEvents;

        SetEnable(element, false);
    }

    #endregion
}
