using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Home;

public static class HomeRecentPlaysItemBehavior
{
    #region Dependency Properties

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
            "Enable",
            typeof(bool),
            typeof(HomeRecentPlaysItemBehavior),
            new PropertyMetadata(false, OnEnableChanged));

    public static readonly DependencyProperty PlayButtonVisibilityProperty =
        DependencyProperty.RegisterAttached(
            "PlayButtonVisibility",
            typeof(Visibility),
            typeof(HomeRecentPlaysItemBehavior),
            new FrameworkPropertyMetadata(Visibility.Visible, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    #endregion

    #region Getters/Setters

    public static bool GetEnable(DependencyObject obj) =>
        (bool)obj.GetValue(EnableProperty);
    public static void SetEnable(DependencyObject obj, bool value) =>
        obj.SetValue(EnableProperty, value);

    public static Visibility GetPlayButtonVisibility(DependencyObject obj) =>
        (Visibility)obj.GetValue(PlayButtonVisibilityProperty);
    public static void SetPlayButtonVisibility(DependencyObject obj, Visibility value) =>
        obj.SetValue(PlayButtonVisibilityProperty, value);

    #endregion

    #region Property Callbacks

    private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Button element)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            element.MouseEnter += OnMouseEnter;
            element.MouseLeave += OnMouseLeave;
            element.Unloaded += DetachEvents;
        }
        else
        {
            element.MouseEnter -= OnMouseEnter;
            element.MouseLeave -= OnMouseLeave;
            element.Unloaded -= DetachEvents;
        }
    }

    #endregion

    #region Event Handlers

    private static void OnMouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        SetPlayButtonVisibility(element, Visibility.Visible);
    }

    private static void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        SetPlayButtonVisibility(element, Visibility.Hidden);
    }

    private static void DetachEvents(object sender, RoutedEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        element.MouseEnter -= OnMouseEnter;
        element.MouseLeave -= OnMouseLeave;
        element.Unloaded -= DetachEvents;

        SetEnable(element, false);
    }

    #endregion
}
